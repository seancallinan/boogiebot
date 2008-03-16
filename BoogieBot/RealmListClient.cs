using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

using Foole.Crypt;
using Foole.WoW;

namespace BoogieBot.Common
{
    public partial class RealmListClient
    {
        public string mUsername;
        private string mPassword;

        private Socket mSocket;
        private NetworkStream ns;
        private WoWReader win;
        private WoWWriter wout;

        public RealmListClient()
        {
            // Get our IP address (TODO: Broken, re-write)
            //IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName ());
            //int i = 0;
            //foreach (string digit in (((ipEntry.AddressList).ToString()).Split(".".ToCharArray())))
            //    mIP[i++] = (Encoding.Default.GetBytes(digit))[0];

            mUsername = BoogieCore.configFile.ReadString("Connection", "User").ToUpper();
            mPassword = BoogieCore.configFile.ReadString("Connection", "Pass").ToUpper();

            if (mUsername.Length < 3 || mPassword.Length < 3)
            {
                BoogieCore.Log(LogType.Error, "Invalid user/pass given ({0} - {1}). Please correct in boogiebot.ini", mUsername, mPassword);
                mSocket.Disconnect(false);
                return;
            }
        }

        public bool Connect(IPEndPoint ep)
        {
            try
            {
                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

                mSocket.Connect(ep);

                ns = new NetworkStream(mSocket, true);
                win = new WoWReader(ns);
                wout = new WoWWriter(ns);

                BoogieCore.Log(LogType.System, "Connected successfully.");
                return true;
            }
            catch (Exception ex)
            {
                BoogieCore.Log(LogType.System, "Failed to open connection to realm list server. Details below:\n{0}", ex.Message);
                return false;
            }
        }

        public bool Logon()
        {
            if (mSocket.Connected == false)
                return false;

            BoogieCore.Log(LogType.System, "Login Challenge: Sending...");
            DoLogonChallenge();
            BoogieCore.Log(LogType.System, "Login Challenge: Waiting on response...");

            if (HandleLogonChallenge() == false)
            {
                BoogieCore.Log(LogType.System, "Login Challenge: Failed!");
                mSocket.Close();
                return false;
            }

            BoogieCore.Log(LogType.System, "Login Challenge: Success!");
            BoogieCore.Log(LogType.System, "Login Proof: Sending...");
            DoLogonProof();
            BoogieCore.Log(LogType.System, "Login Proof: Waiting on response...");

            if (HandleLogonProof() == false)
            {
                BoogieCore.Log(LogType.System, "Login Proof: Authentication Failure.");
                mSocket.Close();
                return false;
            }

            BoogieCore.Log(LogType.System, "Login Proof: Authentication Successful");
            BoogieCore.Log(LogType.System, "Sending RealmList Request...");
            SendRealmlistRequest();
            BoogieCore.Log(LogType.System, "Retrieving RealmList...");
            RetrieveRealmList();
            mSocket.Close();
            return true;
        }

        private void SendRealmlistRequest()
        {
            wout.Write((byte)RLOp.REALM_LIST);
            wout.Write(0);
            wout.Flush();
        }

        private void RetrieveRealmList()
        {
            Realm[] Realms;

            byte op = win.ReadByte();
            UInt16 Length = win.ReadUInt16();
            UInt32 Request = win.ReadUInt32();
            UInt16 NumOfRealms = win.ReadUInt16();

            Realms = new Realm[NumOfRealms];

            for (int i = 0; i < NumOfRealms; i++)
            {
                if((i+1) % 10 == 0)
                    BoogieCore.Log(LogType.SystemDebug, "Retrieved realm {0} of {1}.", i+1, NumOfRealms);

                Realms[i].Type = win.ReadByte();
                Realms[i].Color = win.ReadByte();
                win.ReadByte(); // unk
                Realms[i].Name = win.ReadString();
                Realms[i].Address = win.ReadString();
                Realms[i].Population = win.ReadFloat();
                Realms[i].NumChars = win.ReadByte();
                Realms[i].Language = win.ReadByte();
                Realms[i].Unk = win.ReadByte();
            }

            byte Unk1 = win.ReadByte();
            byte Unk2 = win.ReadByte();

            BoogieCore.Log(LogType.SystemDebug, "Done.");
            
            String defaultRealm = BoogieCore.configFile.ReadString("Connection", "DefaultRealm");

            if (defaultRealm != "")
            {
                foreach (Realm r in Realms)
                    if (r.Name.ToLower() == defaultRealm.ToLower())
                    {
                        BoogieCore.Log(LogType.System, "Defaulting to realm {0}", defaultRealm);

                        string[] address = r.Address.Split(':');
                        IPAddress WSAddr = Dns.GetHostEntry(address[0]).AddressList[0];
                        int WSPort = Int32.Parse(address[1]);
                        BoogieCore.ConnectToWorldServer(new IPEndPoint(WSAddr, WSPort));

                        return;
                    }
            }

            BoogieCore.Event(new Event(EventType.EVENT_REALMLIST, Time.GetTime(), Realms));
        }
    }

    public struct Realm
    {
        public UInt32 Type;
        public byte Color;
        public byte NameLen;
        public string Name;
        public byte AddrLen;
        public string Address;
        public float Population;
        public byte NumChars;
        public byte Language;
        public byte Unk; // const: 1
    }
}