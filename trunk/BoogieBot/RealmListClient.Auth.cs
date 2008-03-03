using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Foole.Crypt;
using Foole.WoW;

namespace BoogieBot.Common
{
    public partial class RealmListClient
    {
        private Srp6 srp;       // http://srp.stanford.edu/design.html  <- SRP6 information

        private BigInteger A;   // My public key?
        private BigInteger B;   // Server's public key
        private BigInteger a;   // my random number, used to initalize A from g and N.
        private byte[] I;       // Hash of "username:password"
        private BigInteger M;   // Combination of... everything!
        private byte[] M2;      // M2 is the combination of the server's everything to proof with ours (which we don't actually do, cause we trust blizzard, right?)


        private byte[] N;       // Modulus for A and B
        private byte[] g;       // base for A and B

        private BigInteger Salt;    // Server provided salt
        private byte[] crcsalt;     // Server provided crcsalt for file crc's.

        public byte[] K;        // Our combined key used for encryption of all traffic

        public void DoLogonChallenge()
        {
            wout.Write((byte)RLOp.AUTH_LOGON_CHALLENGE);
            wout.Write((byte)3);                             // Used to be 2, now its 3.

            wout.Write((UInt16)(30 + mUsername.Length));                                                        // Packet size + name length

            wout.Write((byte)'W'); wout.Write((byte)'o'); wout.Write((byte)'W'); wout.Write((byte)'\0');        // WoW

            wout.Write(BoogieCore.wowVersion.major);
            wout.Write(BoogieCore.wowVersion.minor);
            wout.Write(BoogieCore.wowVersion.update);
            wout.Write(BoogieCore.wowVersion.build);

            switch (BoogieCore.wowType)
            {
                case WoWType.OSXppc:
                    wout.Write((byte)'C'); wout.Write((byte)'P'); wout.Write((byte)'P'); wout.Write((byte)'\0');     // CPP
                    wout.Write((byte)'X'); wout.Write((byte)'S'); wout.Write((byte)'O'); wout.Write((byte)'\0');     // XSO
                    break;
                case WoWType.OSXx86: // NOT TESTED -- I don't have an intel mac.
                    wout.Write((byte)'6'); wout.Write((byte)'8'); wout.Write((byte)'X'); wout.Write((byte)'\0');     // 68X
                    wout.Write((byte)'X'); wout.Write((byte)'S'); wout.Write((byte)'O'); wout.Write((byte)'\0');     // XSO
                    break;
                case WoWType.Win32:
                    wout.Write((byte)'6'); wout.Write((byte)'8'); wout.Write((byte)'x'); wout.Write((byte)'\0');     // 68x
                    wout.Write((byte)'n'); wout.Write((byte)'i'); wout.Write((byte)'W'); wout.Write((byte)'\0');     // niW
                    break;
                default:
                    throw new Exception();  // Shouldn't get here anyway.
            }

            wout.Write((byte)'S'); wout.Write((byte)'U'); wout.Write((byte)'n'); wout.Write((byte)'e');  // SUne

            wout.Write(1);

            wout.Write((byte)127); wout.Write((byte)0); wout.Write((byte)0); wout.Write((byte)1);       // Interestingly, mac sends IPs in reverse order.

            wout.Write((byte)mUsername.Length);
            wout.Write(Encoding.Default.GetBytes(mUsername)); // Name - NOT null terminated
            wout.Flush();
        }

        private bool HandleLogonChallenge()
        {
            byte op = win.ReadByte();
            byte unk = win.ReadByte();

            BoogieCore.Log(LogType.System, "Login Challenge: Response Type = {0}", unk);

            byte error = win.ReadByte();
            if (error > 0)
            {
                BoogieCore.Log(LogType.System, "Login Challenge: Error = {0}", error);
                return false;
            }
            B = new BigInteger(win.ReadBytes(32));               // Varies
            byte glen = win.ReadByte();                          // Length = 1
            g = win.ReadBytes(glen);                             // g = 7
            byte Nlen = win.ReadByte();                          // Length = 32
            N = win.ReadBytes(Nlen);                             // N = B79B3E2A87823CAB8F5EBFBF8EB10108535006298B5BADBD5B53E1895E644B89
            Salt = new BigInteger(win.ReadBytes(32));            // Salt = 3516482AC96291B3C84B4FC204E65B623EFC2563C8B4E42AA454D93FCD1B56BA
            crcsalt = win.ReadBytes(16);                         // Varies

            srp = new Srp6(new BigInteger(N), new BigInteger(g));

            // A hack, yes. We just keep trying till we get an S thats not negative so we get rid of auth=4 error logging on.
            BigInteger S;
            do
            {
                a = BigInteger.Random(19 * 8);
                A = srp.GetA(a);

                I = Srp6.GetLogonHash(mUsername, mPassword);

                BigInteger x = Srp6.Getx(Salt, I);
                BigInteger u = Srp6.Getu(A, B);
                S = srp.ClientGetS(a, B, x, u);
            }
            while (S < 0);

            K = Srp6.ShaInterleave(S);
            M = srp.GetM(mUsername, Salt, A, B, new BigInteger(K));

            unk = win.ReadByte();

            // BoogieCore.wardenClient.Initialize(K); // shame you don't have this class.

            return true;
        }

        public void DoLogonProof()
        {
            Sha1Hash sha;
            byte[] files_crc;
            string[] crcfilenames;

            switch (BoogieCore.wowType)
            {
                case WoWType.OSXppc:
                case WoWType.OSXx86:
                    crcfilenames = new string[] {
                            "World of Warcraft.app/Contents/MacOS/World of Warcraft",
                            "World of Warcraft.app/Contents/Info.plist",
                            "World of Warcraft.app/Contents/Resources/Main.nib/objects.xib",
                            "World of Warcraft.app/Contents/Resources/wow.icns",
                            "World of Warcraft.app/Contents/PkgInfo" };
                    break;

                case WoWType.Win32:
                    crcfilenames = new string[] { "WoW.exe", "fmod.dll", "ijl15.dll", "dbghelp.dll", "unicows.dll" };
                    break;

                default:
                    throw new Exception();
            }

            // Generate CRC/hashes of the Game Files
            files_crc = GenerateCrc(crcsalt, crcfilenames);

            // get crc_hash from files_crc
            sha = new Sha1Hash();
            sha.Update(A);
            sha.Update(files_crc);
            byte[] crc_hash = sha.Final();

            wout.Write((byte)RLOp.AUTH_LOGON_PROOF);
            wout.Write(A); // 32 bytes
            wout.Write(M); // 20 bytes
            wout.Write(crc_hash); // 20 bytes
            wout.Write((byte)0); // number of keys
            wout.Write((byte)0); // unk (1.11.x)
            wout.Flush();
        }

        private byte[] GenerateCrc(byte[] crcsalt, string[] crcfilenames)
        {
            Sha1Hash sha;

            byte[] buffer1 = new byte[0x40];
            byte[] buffer2 = new byte[0x40];

            for (int i = 0; i < 0x40; ++i)
            {
                buffer1[i] = 0x36;
                buffer2[i] = 0x5c;
            }

            for (int i = 0; i < crcsalt.Length; ++i)
            {
                buffer1[i] ^= crcsalt[i];
                buffer2[i] ^= crcsalt[i];
            }

            sha = new Sha1Hash();
            sha.Update(buffer1);


            foreach (string filename in crcfilenames)
            {
                try
                {
                    FileStream fs = new FileStream(BoogieCore.wowPath + filename, FileMode.Open, FileAccess.Read);
                    byte[] Buffer = new byte[fs.Length];
                    fs.Read(Buffer, 0, (int)fs.Length);
                    sha.Update(Buffer);
                }
                catch (Exception e)
                {
                    BoogieCore.Log(LogType.Error, e.Message);
                }
            }
            byte[] hash1 = sha.Final();

            sha = new Sha1Hash();
            sha.Update(buffer2);
            sha.Update(hash1);
            return sha.Final();
        }

        private bool HandleLogonProof()
        {
            byte op = win.ReadByte();
            byte error = win.ReadByte();

            if (error > 0)
            {
                BoogieCore.Log(LogType.System, "Login Proof: Error = {0}", error);
                return false;
            }

            M2 = win.ReadBytes(20);
            int unknown = win.ReadInt32();
            UInt16 unk2 = win.ReadUInt16();

            return true;
        }
    }
}