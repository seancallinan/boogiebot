using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Timers;
using System.IO;

using Foole.Crypt;
using Foole.Utils;
using Foole.WoW;

namespace BoogieBot.Common
{
    public struct CharEquipment
    {
        public UInt32 EntryID;
        public byte Type;
    }

    partial class WorldServerClient
    {

        byte[] account_data = {
0x78, 0x01, 0xBD, 0x57, 0xDB, 0x96, 0xAB, 0x28, 0x10, 0x7D, 0xE7, 0x2B, 0xFC, 0x04, 0xB5, 0x93,
0xB6, 0xF3, 0x48, 0x14, 0x23, 0xD3, 0x44, 0xB2, 0x04, 0x3B, 0x27, 0xE7, 0x85, 0xFF, 0xFF, 0x8B,
0x29, 0xAE, 0x82, 0x4A, 0x27, 0x0F, 0x33, 0x59, 0xDD, 0xAE, 0x68, 0x5D, 0x36, 0x50, 0x54, 0x15,
0x9B, 0x1F, 0x32, 0x09, 0xCA, 0xC7, 0xA2, 0x46, 0x08, 0x77, 0x1D, 0xE9, 0x7E, 0x96, 0x6F, 0x7E,
0x93, 0xA0, 0x51, 0x97, 0x99, 0xB2, 0x4E, 0x4D, 0xA4, 0x9D, 0x66, 0x2A, 0xAF, 0x64, 0x94, 0xAA,
0x1D, 0xF0, 0x38, 0x12, 0x56, 0xE0, 0x59, 0x72, 0x84, 0xDC, 0x97, 0x40, 0x64, 0xEC, 0x10, 0xFA,
0xCB, 0x47, 0xE2, 0x25, 0x45, 0x5D, 0x9E, 0x9A, 0xEA, 0x78, 0x04, 0x1B, 0xCE, 0xF8, 0x24, 0x90,
0xC0, 0x8F, 0xA2, 0x3E, 0x1E, 0xFD, 0x83, 0x6E, 0x78, 0x92, 0x8F, 0xA2, 0x6A, 0x4A, 0xF3, 0x80,
0x06, 0x4D, 0x98, 0x76, 0x46, 0x5D, 0xD5, 0x4D, 0x51, 0x22, 0x33, 0x76, 0xF1, 0x79, 0x30, 0xA2,
0xCF, 0x03, 0xE2, 0x7D, 0x4F, 0x5B, 0x32, 0x69, 0x49, 0x75, 0xAA, 0xE1, 0x07, 0x3D, 0x08, 0x63,
0x4E, 0xAB, 0x3F, 0xEF, 0x03, 0x15, 0x37, 0x30, 0xD0, 0xA3, 0x54, 0xF5, 0x97, 0xFE, 0xF5, 0x32,
0x45, 0xC7, 0x9E, 0x4F, 0xD7, 0x44, 0x45, 0xAE, 0x5C, 0x92, 0x20, 0x01, 0x7F, 0x49, 0xFE, 0x48,
0xB5, 0x91, 0x8A, 0x87, 0x90, 0xC4, 0x7A, 0x6A, 0xE0, 0x12, 0x5D, 0xF9, 0x08, 0x92, 0x49, 0xC5,
0x0B, 0xAA, 0x8E, 0xA7, 0x20, 0x5F, 0xCD, 0xCA, 0x9B, 0x6F, 0x80, 0x7D, 0x24, 0xCD, 0x74, 0x61,
0x41, 0xB0, 0x28, 0x1F, 0x4F, 0xF5, 0x0F, 0xA7, 0xA3, 0x59, 0xA5, 0x5E, 0x07, 0x3C, 0x41, 0xC1,
0x08, 0xFE, 0x21, 0xFB, 0x1A, 0x2A, 0xE4, 0xAE, 0x62, 0xE4, 0x12, 0xE2, 0x66, 0x55, 0xAB, 0x61,
0xAC, 0x4A, 0xCD, 0x02, 0xA2, 0xA6, 0x63, 0xEA, 0x1E, 0x84, 0xFB, 0xEF, 0x10, 0x18, 0x1D, 0xC5, 
0x6E, 0xF4, 0x1B, 0x63, 0xA3, 0x4A, 0x2F, 0x23, 0x9F, 0x88, 0x95, 0x95, 0x10, 0x12, 0xF1, 0x4D, 
0x61, 0x27, 0xBE, 0x8E, 0xFA, 0x5F, 0xDB, 0x33, 0xCE, 0x65, 0x61, 0x37, 0xB6, 0x84, 0xFD, 0xBF, 
0x9E, 0xB1, 0x54, 0x57, 0x2A, 0x5A, 0xB3, 0x0D, 0x66, 0x6F, 0xFC, 0xFE, 0xF8, 0xE8, 0xF8, 0xBD, 
0xAB, 0x9A, 0x13, 0xE4, 0x83, 0x79, 0xBC, 0xA3, 0x20, 0xAC, 0x57, 0x03, 0x95, 0xC2, 0x4C, 0x49,
0x47, 0x4B, 0x0F, 0xE1, 0x50, 0x8D, 0x12, 0xA0, 0x05, 0xD9, 0x55, 0xDF, 0x88, 0xCC, 0xBA, 0x6A, 
0xDD, 0x2F, 0x9E, 0x3A, 0x3D, 0xF3, 0xBE, 0x46, 0x9B, 0xF7, 0xEE, 0x27, 0x0A, 0x15, 0xC1, 0x1E, 
0x37, 0x86, 0x1F, 0x90, 0x2A, 0xB9, 0xD9, 0xAF, 0xCC, 0xF2, 0x78, 0x03, 0x17, 0x92, 0x32, 0xF2, 
0x04, 0x2E, 0xB5, 0xCA, 0xA3, 0xB5, 0x13, 0xC1, 0x72, 0x9E, 0x88, 0xFA, 0x11, 0x2A, 0x8D, 0xEE, 
0xA1, 0x29, 0x0E, 0x8D, 0x8F, 0xED, 0xC6, 0x2C, 0x42, 0xCC, 0x1A, 0x9A, 0xB2, 0xCE, 0x2E, 0x38, 
0x86, 0xB4, 0x96, 0x11, 0xE6, 0x6A, 0x6B, 0x63, 0xDB, 0xF0, 0x9E, 0x8B, 0x64, 0x30, 0x80, 0x25, 
0x85, 0xF7, 0x3C, 0xB6, 0x0F, 0xBC, 0xEA, 0x20, 0x12, 0xC3, 0x5E, 0x62, 0xB9, 0x58, 0xE6, 0x0D,
0xFE, 0xDC, 0xD4, 0x05, 0xEB, 0x2A, 0xAD, 0x2A, 0xF3, 0xE8, 0xA4, 0x84, 0xF6, 0xC3, 0x98, 0x0D, 
0x69, 0x87, 0xAF, 0xF8, 0x62, 0xDB, 0x8B, 0x5E, 0x17, 0xD4, 0xC8, 0xA2, 0x3B, 0xCF, 0x7D, 0x1F, 
0xC6, 0xF4, 0x1A, 0x9D, 0x8E, 0x2B, 0xA7, 0x05, 0x52, 0x2B, 0x13, 0xAF, 0x48, 0x65, 0x52, 0x31, 
0xEF, 0x69, 0xD4, 0x19, 0x5F, 0x1F, 0x06, 0x97, 0x57, 0x59, 0x90, 0x95, 0x5D, 0x06, 0x2D, 0x4D, 
0xBF, 0x2C, 0x58, 0x6A, 0x96, 0xC1, 0x0A, 0x7B, 0xE8, 0x53, 0xD4, 0xC3, 0x95, 0x75, 0xD1, 0x7C, 
0x16, 0x75, 0xD5, 0xB8, 0x80, 0x6E, 0x0C, 0x5F, 0x00, 0xB4, 0x99, 0xE7, 0x11, 0x6D, 0x43, 0x31, 
0x4D, 0xC5, 0xEE, 0x51, 0x0C, 0x69, 0x4D, 0x5F, 0xC0, 0x0C, 0x4E, 0x2F, 0xC1, 0x06, 0xEB, 0x0C, 
0xB2, 0x9C, 0x70, 0x47, 0x4C, 0x57, 0x4D, 0xDB, 0x9A, 0x9D, 0xA0, 0x1D, 0x42, 0x0C, 0x94, 0xB0, 
0x4E, 0x28, 0x38, 0xA6, 0x75, 0x11, 0x87, 0x84, 0x5A, 0x52, 0x63, 0x63, 0xC7, 0xE5, 0x00, 0x47, 
0xFC, 0x8E, 0x25, 0x9E, 0x27, 0xAC, 0x2E, 0x70, 0x7C, 0xE7, 0xA0, 0x16, 0x03, 0x13, 0x91, 0x5F, 
0x21, 0xCC, 0x28, 0x3B, 0x16, 0x14, 0xCE, 0x51, 0x45, 0x46, 0x7D, 0xEE, 0x19, 0x2E, 0xB1, 0x37, 
0x91, 0x33, 0xB4, 0xA6, 0x6F, 0xA5, 0x47, 0xDB, 0x01, 0x80, 0xC3, 0x9D, 0xF2, 0x8E, 0xB6, 0xBB, 
0x15, 0xB6, 0x2C, 0x3B, 0x35, 0xD3, 0x11, 0xDE, 0x1B, 0x29, 0x58, 0x3D, 0xC9, 0x86, 0x95, 0xDD, 
0x53, 0xB8, 0x55, 0xAD, 0x64, 0xD3, 0x21, 0xE0, 0xAE, 0x1C, 0x9E, 0x0E, 0x90, 0xD6, 0xCF, 0x73, 
0xFC, 0xD4, 0xFE, 0x29, 0x7C, 0x48, 0xCD, 0xE7, 0xC8, 0xC1, 0x34, 0x07, 0xDA, 0x63, 0x38, 0xB4, 
0x3A, 0xC5, 0x78, 0x8B, 0x99, 0xED, 0x31, 0xC9, 0xAE, 0xBA, 0x33, 0x7C, 0xE0, 0xC0, 0x26, 0x6C, 
0x33, 0xAD, 0x6B, 0xA0, 0x7B, 0x50, 0xE2, 0x55, 0x89, 0xCE, 0x17, 0x65, 0xA9, 0x97, 0x1A, 0xC9, 
0x0C, 0xF5, 0x60, 0x99, 0x5E, 0x55, 0x03, 0xAD, 0x88, 0x95, 0x98, 0x31, 0x8A, 0x47, 0xE0, 0x37, 
0x9A, 0x6E, 0x80, 0xF3, 0xC7, 0x29, 0xF2, 0x1C, 0xF8, 0xD4, 0xD9, 0x1E, 0xAC, 0x59, 0x8A, 0x1B, 
0xAE, 0xC7, 0xAD, 0xE1, 0xB6, 0x3A, 0x0F, 0xA1, 0x43, 0x3B, 0x86, 0xA5, 0x27, 0xA6, 0x29, 0x1C, 
0x59, 0x58, 0x6A, 0x69, 0x18, 0xA9, 0x02, 0xCA, 0xD5, 0x39, 0x56, 0x59, 0x57, 0xC0, 0x4C, 0xBE, 
0x3E, 0xAC, 0xFC, 0x8E, 0xA7, 0x91, 0x8E, 0x17, 0xBB, 0x22, 0xA7, 0x00, 0x92, 0x49, 0x80, 0x1C, 
0x29, 0xE9, 0x99, 0xA9, 0xE7, 0x39, 0x9A, 0xDB, 0xAA, 0x33, 0x17, 0x22, 0x22, 0x98, 0x1E, 0xAD, 
0xA7, 0x0C, 0xF8, 0x64, 0x44, 0xA7, 0x80, 0x2E, 0x49, 0x46, 0x2E, 0x13, 0x9F, 0x03, 0xEF, 0xD2, 
0x84, 0x38, 0x16, 0xEF, 0xCD, 0xCA, 0x11, 0xCA, 0xCA, 0xCC, 0xC8, 0x53, 0x39, 0x27, 0xAC, 0xF7, 
0x84, 0x1F, 0x7B, 0x42, 0xCB, 0xB7, 0x57, 0xEE, 0x96, 0xB6, 0xAF, 0x84, 0xD0, 0x86, 0xA1, 0x77, 
0xAE, 0x84, 0xCD, 0x9E, 0xD0, 0xB0, 0xC6, 0xB5, 0xE5, 0x69, 0xCF, 0xB2, 0x2A, 0x13, 0xA9, 0xB9, 
0x56, 0xDC, 0xE9, 0xD8, 0xF1, 0x7B, 0x51, 0x21, 0x41, 0xFF, 0xC2, 0x3E, 0xC3, 0x3E, 0xC2, 0x85, 
0x02, 0xF6, 0xDB, 0xFC, 0x01, 0xD7, 0x6C, 0xBF, 0x21, 0x76, 0x15, 0xEA, 0xFC, 0x8B, 0x18, 0xF8, 
0x1D, 0xCE, 0x65, 0x84, 0xAE, 0x44, 0x08, 0x38, 0x84, 0xE1, 0xEE, 0x61, 0x38, 0xBC, 0xBE, 0x82, 
0x98, 0x3B, 0x83, 0xBF, 0x15, 0xD8, 0x3B, 0x88, 0xBD, 0x68, 0x20, 0x9F, 0xCC, 0x9E, 0x6A, 0x5B,
0x52, 0x6B, 0xA8, 0xAC, 0xBD, 0xDD, 0xB8, 0x48, 0xFE, 0x7A, 0xD7, 0x89, 0xE7, 0x5B, 0xBF, 0x36, 
0x5F, 0x30, 0x33, 0xF3, 0x2D, 0xA3, 0xF9, 0xAE, 0x29, 0xB3, 0x4F, 0xDD, 0x40, 0xD6, 0x12, 0x81, 
0xA5, 0x39, 0x5E, 0xE4, 0x29, 0x6F, 0xFC, 0x9D, 0x5A, 0xA4, 0x0D, 0x41, 0x33, 0x2A, 0x6F, 0x9B, 
0x6A, 0x52, 0x2F, 0x1F, 0x9F, 0x84, 0x34, 0x7A, 0xC7, 0x8D, 0x32, 0xF5, 0xF5, 0x2D, 0xCE, 0xB2, 
0x29, 0xEF, 0xE4, 0x46, 0x4B, 0x85, 0x8E, 0x56, 0xC5, 0x74, 0xC9, 0x36, 0xA2, 0x58, 0xA2, 0xFB, 
0x8D, 0xFB, 0x5E, 0x68, 0x53, 0x24, 0x88, 0xF4, 0xE9, 0x92, 0x12, 0xA8, 0x54, 0x15, 0xF9, 0x6C, 
0x56, 0x93, 0xB8, 0x6D, 0xB4, 0x91, 0x67, 0x74, 0x6E, 0xBB, 0xE9, 0x58, 0xD7, 0xD5, 0x59, 0xED, 
0x74, 0xCB, 0xA1, 0xAA, 0x37, 0xD6, 0x09, 0x37, 0xA7, 0xA4, 0x93, 0x2F, 0x27, 0xA3, 0x13, 0x84, 
0x03, 0xC4, 0x64, 0x45, 0x32, 0xC5, 0x54, 0xA5, 0xE7, 0x07, 0x35, 0x60, 0xF8, 0x67, 0xD0, 0xA4, 
0x8B, 0xDF, 0xF7, 0x4E, 0x6D, 0x76, 0x61, 0x42, 0x30, 0xF6, 0x11, 0x82, 0xDA, 0x3A, 0x6F, 0x1A, 
0xBE, 0x4F, 0x85, 0xB4, 0x27, 0xDB, 0x3E, 0xFC, 0xB4, 0xEA, 0xA0, 0x62, 0xE2, 0x7A, 0xFB, 0x78, 
0xAD, 0xDE, 0x80, 0x6F, 0x6F, 0xEA, 0xCD, 0xC0, 0xFC, 0x52, 0xDF, 0xAB, 0x91, 0x0E, 0x6F, 0x1B, 
0x09, 0xAE, 0x14, 0xAF, 0xF4, 0xBC, 0xFF, 0x60, 0x4D, 0x9F, 0x6F, 0x1B, 0x09, 0x98, 0xFB, 0x9B, 
0xD6, 0xF4, 0xF5, 0xB6, 0x91, 0x4E, 0x6F, 0x1B, 0x09, 0xC8, 0xCF, 0xFF, 0x1E, 0xBE, 0x7F, 0x01, 
0x34, 0x99, 0x02, 0x24};


        private void Handle_AuthRequest(WoWReader wr)
        {
            BoogieCore.Log(LogType.System, "WS: Recieved Authentication Challenge: Sending out response");
            ServerSeed = wr.ReadUInt32();
            ClientSeed = (UInt32)random.Next();

            Sha1Hash sha = new Sha1Hash();
            sha.Update(mUsername);
            sha.Update(0); // t
            sha.Update(ClientSeed);
            sha.Update(ServerSeed);
            sha.Update(Key);
            byte[] Digest = sha.Final();

            WoWWriter ww = new WoWWriter(OpCode.CMSG_AUTH_SESSION);
            ww.Write(BoogieCore.configFile.ReadInteger("WoW", "Build"));
            ww.Write((UInt32)0);
            ww.Write(mUsername);
            ww.Write(ClientSeed);
            ww.Write(Digest);

            StreamReader SR;
            WoWWriter buffer = new WoWWriter();
            SR = File.OpenText("Addons.txt");
            string Line = SR.ReadLine();

            while (Line != null)
            {
                string[] Fields = new string[3];
                string Name = null;
                UInt64 Checksum = 0;
                byte unk = 0x0;

                Fields = Line.Split(':');
                Name = Fields[0];
                Checksum = UInt64.Parse(Fields[1]);
                //unk = (Fields[2].ToCharArray())[0];

                if (Name != null && Checksum > 0)
                {
                    buffer.Write(Name);
                    buffer.Write(Checksum);
                    buffer.Write(unk);

                    //BoogieCore.Log("Adding addon {0} with the checksum {1}", Name, Checksum);
                }

                Line = SR.ReadLine();
            }

            SR.Close();

            byte[] buffer2 = Foole.Utils.Compression.Compress(buffer.ToArray());
            UInt32 Size = (UInt32)buffer.ToArray().Length;

            ww.Write(Size);
            ww.Write(buffer2);

            Send(ww);

            mCrypt.Init(Key);
        }

        private void Handle_AuthResponse(WoWReader wr)
        {
            byte error = wr.ReadByte();
            if (error != 0x0C)
            {
                BoogieCore.Log(LogType.Error, "WS: Authentication Failed: Error = {0}", error);
                return;
            }
            BoogieCore.Log(LogType.System, "WS: Authentication Successful!");

            BoogieCore.Log(LogType.System, "WS: Requesting Character List...");

            WoWWriter ww = new WoWWriter(OpCode.CMSG_CHAR_ENUM);
            Send(ww.ToArray());
            PingTimer.Enabled = true;

        }

        private void Handle_CharEnum(WoWReader wr)
        {
            BoogieCore.Log(LogType.NeworkComms, "WS: Recieved Character List..");
            byte count = wr.ReadByte();

            characterList = new Character[count];

            for (int i = 0; i < count; i++)
            {
                characterList[i].GUID = wr.ReadUInt64();
                characterList[i].Name = wr.ReadString();
                characterList[i].Race = wr.ReadByte();
                characterList[i].Class = wr.ReadByte();
                characterList[i].Gender = wr.ReadByte();
                characterList[i].Skin = wr.ReadByte();
                characterList[i].Face = wr.ReadByte();
                characterList[i].HairStyle = wr.ReadByte();
                characterList[i].HairColor = wr.ReadByte();
                characterList[i].FacialHair = wr.ReadByte();
                characterList[i].Level = wr.ReadByte();
                characterList[i].ZoneID = wr.ReadUInt32();
                characterList[i].MapID = wr.ReadUInt32();
                characterList[i].X = wr.ReadFloat();
                characterList[i].Y = wr.ReadFloat();
                characterList[i].Z = wr.ReadFloat();
                characterList[i].Guild = wr.ReadUInt32();
                characterList[i].Unk = wr.ReadUInt32();
                characterList[i].RestState = wr.ReadByte();
                characterList[i].PetInfoID = wr.ReadUInt32();
                characterList[i].PetLevel = wr.ReadUInt32();
                characterList[i].PetFamilyID = wr.ReadUInt32();

                CharEquipment[] equip = new CharEquipment[20];

                for (int x = 0; x < 20; x++)
                {
                    equip[x].EntryID = wr.ReadUInt32();
                    equip[x].Type = wr.ReadByte();
                    wr.ReadUInt32(); // enchant (2.4 patch)
                }

                characterList[i].Equipment = equip;
            }

            BoogieCore.Log(LogType.NeworkComms, "{0} characters in total.", characterList.Length);

            String defaultChar = BoogieCore.configFile.ReadString("Connection", "DefaultChar");
            if (defaultChar != "")
                foreach (Character c in characterList)
                    if (c.Name.ToLower() == defaultChar.ToLower())
                    {
                        BoogieCore.Log(LogType.System, "Defaulting to Character {0}", defaultChar);
                        BoogieCore.WorldServerClient.LoginChar(c.GUID);
                        return;
                    }

            if (count < 1)
            {
                string name = RandomString(6, false);
                BoogieCore.Log(LogType.System, "Auto-Generating Human Character with the name {0}", name);

                WoWWriter ww = new WoWWriter(OpCode.CMSG_CHAR_CREATE);
                ww.Write(name);
                ww.Write((byte)1); // race - human
                ww.Write((byte)1); // class - warrior
                ww.Write((byte)0); // gender - male
                ww.Write((byte)1); // skin
                ww.Write((byte)1); // face
                ww.Write((byte)1); // hair style
                ww.Write((byte)1); // hair color
                ww.Write((byte)1); // facial hair
                ww.Write((byte)1); // outfit id
                Send(ww.ToArray());
                ww = new WoWWriter(OpCode.CMSG_CHAR_ENUM);
                Send(ww.ToArray());
                return;
            }

            if (count == 1)
            {
                BoogieCore.Log(LogType.System, "Defaulting to Character {0}", characterList[0].Name);
                BoogieCore.WorldServerClient.LoginChar(characterList[0].GUID);
                return;
            }

            BoogieCore.Event(new Event(EventType.EVENT_CHAR_LIST, Time.GetTime(), characterList));
        }

        public void WorldZone(UInt64 guid)
        {
            WoWWriter ww = new WoWWriter(OpCode.CMSG_PLAYER_LOGIN);
            ww.Write(guid);
            Send(ww.ToArray());

            ww = new WoWWriter(OpCode.CMSG_UPDATE_ACCOUNT_DATA);
            ww.Write((UInt32)7);
            ww.Write((UInt32)5144);
            ww.Write(account_data);
            Send(ww.ToArray());

            ww = new WoWWriter(OpCode.CMSG_NAME_QUERY);
            ww.Write(guid);
            Send(ww.ToArray());

            ww = new WoWWriter(OpCode.CMSG_SET_ACTIVE_MOVER);
            ww.Write(guid);
            Send(ww.ToArray());
        }
        public void LoginChar(UInt64 guid)
        {
            foreach (Character c in characterList)
            {
                if (c.GUID == guid)
                {
                    BoogieCore.World.setPlayer(c.MapID, new WoWGuid(guid));
                    BoogieCore.Player.setPlayer(c);
                }
            }
            characterList = null; // No longer needed!

            WoWWriter ww = new WoWWriter(OpCode.CMSG_PLAYER_LOGIN);
            ww.Write(guid);
            Send(ww.ToArray());

            ww = new WoWWriter(OpCode.CMSG_UPDATE_ACCOUNT_DATA);
            ww.Write((UInt32)7);
            ww.Write((UInt32)5144);
            ww.Write(account_data);
            Send(ww.ToArray());

            ww = new WoWWriter(OpCode.CMSG_NAME_QUERY);
            ww.Write(guid);
            Send(ww.ToArray());

            ww = new WoWWriter(OpCode.CMSG_SET_ACTIVE_MOVER);
            ww.Write(guid);
            Send(ww.ToArray());
            //MoveUpdateTimer.Enabled = true;

        }
    }
}