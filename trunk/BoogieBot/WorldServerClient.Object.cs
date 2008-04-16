#define DEBUG
using System;
using System.Collections.Generic;
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
    // WoWObject Packet Handling
    partial class WorldServerClient
    {
        private void Handle_ObjUpdate(WoWReader wr, bool Compressed)
        {
            if (Compressed)
            {
                Int32 size = wr.ReadInt32();
                byte[] decomped = Foole.Utils.Compression.Decompress(size, wr.ReadRemaining());
                wr = new WoWReader(decomped);
            }

            WoWGuid guid;
            UInt32 blockCount;
            byte unk1;
            byte blockType;
            byte objTypeId;

            blockCount = wr.ReadUInt32();
            unk1 = wr.ReadByte();

            BoogieCore.Log(LogType.Error, "Got obj update with {0} blocks", blockCount);
            for (UInt32 i = 0; i < blockCount; i++)
            {
                blockType = wr.ReadByte();
                #if (DEBUG)
                BoogieCore.Log(LogType.NeworkComms, "Block #{0}/{1} Type: {2}", i+1, blockCount, blockType);
                #endif

                switch (blockType)
                {
                    case 0: // Fields update
                        {
                            byte mask = wr.ReadByte();

                            if (mask == 0x00)
                                break;

                            guid = new WoWGuid(mask, wr.ReadBytes(WoWGuid.BitCount8(mask)));

                            UpdateMask UpdateMask = new UpdateMask();
                            byte bc = wr.ReadByte(); // Block Count

                            UpdateMask.SetCount((ushort)(bc * 32));
                            UpdateMask.SetMask(wr.ReadBytes(bc * 4), bc);
#if (DEBUG)
                            BoogieCore.Log(LogType.Error, "Field Update! FieldCount: {0}", UpdateMask.GetCount());
#endif
                            UInt32[] Fields = new UInt32[UpdateMask.GetCount()];

                            Object obj = BoogieCore.World.getObject(guid);

                            if (obj == null)
                            {
                                BoogieCore.Log(LogType.Error, "Object with the guid {0} not recognized in field update.", guid.GetOldGuid());
                            }

                            for (ushort x = 0; x < UpdateMask.GetCount(); x++)
                            {
                                if (UpdateMask.GetBit(x))
                                    if (obj == null) // FixMe
                                        wr.ReadUInt32();
                                    else
                                        obj.Fields[x] = wr.ReadUInt32();
                            }

                            // Update Player Class if these are Player Fields being changed.
                            if (obj != null)
                                if (obj.GUID.GetOldGuid() == BoogieCore.Player.Character.GUID)
                                    BoogieCore.Player.updatePlayer(obj);

                            break;
                        }
                    case 1: // Movement Update
                        {
                            byte mask = wr.ReadByte();

                            if (mask == 0x00)
                                break;

                            guid = new WoWGuid(mask, wr.ReadBytes(WoWGuid.BitCount8(mask)));
                            #if (DEBUG)
                            BoogieCore.Log(LogType.NeworkComms, "Got Movement update for GUID {0}", BitConverter.ToUInt64(guid.GetNewGuid(), 0));
#endif
                            UInt32 flags2 = 0, unk3;
                            float posX = 0;
                            float posY = 0;
                            float posZ = 0;
                            float facing = 0;
                            float walkSpeed, runSpeed, backWalkSpeed, swimSpeed, backSwimSpeed, turnRate = 0;

                            byte flags = wr.ReadByte();

                            if ((flags & 0x20) >= 1)
                            {
                                #if (DEBUG)
                                BoogieCore.Log(LogType.NeworkComms, "(flags & 20)");
#endif
                                flags2 = wr.ReadUInt32();
                                wr.ReadByte(); // 2.3.3
                                unk3 = wr.ReadUInt32();
                            }

                            if ((flags & 0x40) >= 1)
                            {
                                #if (DEBUG)
                                BoogieCore.Log(LogType.NeworkComms, "(flags & 40)");
#endif
                                posX = wr.ReadSingle();
                                posY = wr.ReadSingle();
                                posZ = wr.ReadSingle();
                                facing = wr.ReadSingle();
#if (DEBUG)
                                BoogieCore.Log(LogType.NeworkComms, "Position - X: {0} Y: {1} Z: {2} Orient: {3} ", posX, posY, posZ, facing);
#endif
                                if ((flags2 & 0x02000000) >= 1)	// player being transported
                                {
                                    #if (DEBUG)
                                    BoogieCore.Log(LogType.NeworkComms, "(flags2 & 0x02000000)");
#endif
                                    wr.ReadUInt32();	//guidlow
                                    wr.ReadUInt32();	//guidhigh
                                    wr.ReadSingle();	//x
                                    wr.ReadSingle();	//y
                                    wr.ReadSingle();	//z
                                    wr.ReadSingle();	//o
                                    wr.ReadSingle();    // unk
                                }
                            }


                            if ((flags & 0x20) >= 1)
                            {
                                #if (DEBUG)
                                BoogieCore.Log(LogType.NeworkComms, "(flags & 20)");
#endif
                                wr.ReadSingle(); //unk
                                if ((flags2 & 0x2000) >= 1)
                                {
#if (DEBUG)
                                    BoogieCore.Log(LogType.NeworkComms, "(flags & 2000)");
#endif
                                    wr.ReadSingle(); // pos unk1
                                    wr.ReadSingle(); // pos unk1
                                    wr.ReadSingle(); // pos unk1
                                    wr.ReadSingle(); // pos unk1
                                    //BoogieCore.Log(LogType.NeworkComms, "Position 2 - X: {0} Y: {1} Z: {2} Orient: {3} ", punk1, punk2, punk3, punk1);
                                }
                            }

                            if ((flags & 0x20) >= 1)
                            {
                                #if (DEBUG)
                                BoogieCore.Log(LogType.NeworkComms, "(flags & 20)");
#endif
                                walkSpeed = wr.ReadSingle();
                                runSpeed = wr.ReadSingle();
                                backWalkSpeed = wr.ReadSingle();
                                swimSpeed = wr.ReadSingle();
                                backSwimSpeed = wr.ReadSingle();
                                wr.ReadSingle(); //unk1
                                wr.ReadSingle(); //unk2
                                turnRate = wr.ReadSingle();
                                //BoogieCore.Log(LogType.NeworkComms, "Speed - (flags & 0x20)");
                            }

                            if ((flags & 0x20) >= 1)
                            {
                                //BoogieCore.Log(LogType.NeworkComms, "(flags & 0x20)");
                                if ((flags2 & 0x00400000) >= 1)
                                {
                                    //BoogieCore.Log(LogType.NeworkComms, "(flags2 & 0x00400000)");
                                    UInt32 splineFlags;

                                    splineFlags = wr.ReadUInt32();

                                    if ((splineFlags & 0x00010000) >= 1)
                                    {
                                        //BoogieCore.Log(LogType.NeworkComms, "(splineFlags & 0x00010000)");
                                        posX = wr.ReadSingle();
                                        posY = wr.ReadSingle();
                                        posZ = wr.ReadSingle();
                                        //BoogieCore.Log(LogType.NeworkComms, "Position 3 - X: {0} Y: {1} Z: {2} Orient: {3} ", posX, posY, posZ, facing);
                                    }

                                    if ((splineFlags & 0x00020000) >= 1)
                                    {
                                        //BoogieCore.Log(LogType.NeworkComms, "(splineFlags & 0x00020000)");
                                        wr.ReadUInt64();
                                    }

                                    if ((splineFlags & 0x00040000) >= 1)
                                    {
                                        //BoogieCore.Log(LogType.NeworkComms, "(splineFlags & 0x00040000)");
                                        float f;
                                        f = wr.ReadSingle();
                                    }

                                    UInt32 time1, time2, splineCount, unk4;

                                    //1.8
                                    time1 = wr.ReadUInt32();
                                    time2 = wr.ReadUInt32();
                                    unk4 = wr.ReadUInt32();
                                    splineCount = wr.ReadUInt32();
                                    //BoogieCore.Log(LogType.NeworkComms, "splineCount = {0}", splineCount);

                                    for (UInt32 j = 0; j < splineCount + 1; j++)
                                    {

                                        posX = wr.ReadSingle();
                                        posY = wr.ReadSingle();
                                        posZ = wr.ReadSingle();
                                        //BoogieCore.Log(LogType.NeworkComms, "Position 4 - X: {0} Y: {1} Z: {2} Orient: {3} ", posX, posY, posZ, facing);
                                    }
                                }
                            }

                            if ((flags & 0x8) >= 1)
                            {
                                #if (DEBUG)
                                BoogieCore.Log(LogType.NeworkComms, "(flags & 8)");
#endif
                                wr.ReadUInt32();
                                if ((flags & 0x10) >= 1)
                                {
#if (DEBUG)
                                    BoogieCore.Log(LogType.NeworkComms, "(flags & 10)");
#endif
                                    wr.ReadUInt32();
                                }
                            }
                            else if ((flags & 0x10) >= 1)
                            {
                                #if (DEBUG)
                                BoogieCore.Log(LogType.NeworkComms, "(flags & 10)");
#endif
                                wr.ReadUInt32();
                            }

                            if ((flags & 0x2) >= 1)
                            {
                                #if (DEBUG)
                                BoogieCore.Log(LogType.NeworkComms, "(flags & 0x2)");
#endif
                                wr.ReadUInt32();
                            }

                            break;
                        }

                    case 2: // ObjCreate
                    case 3: // ObjCreate
                        {
                            byte mask = wr.ReadByte();

                            guid = new WoWGuid(mask, wr.ReadBytes(WoWGuid.BitCount8(mask)));
                            objTypeId = wr.ReadByte();
                            #if (DEBUG)
                            BoogieCore.Log(LogType.NeworkComms, "Got Object Create Mask: 0x{0:x2} GUID: {1} ObjTypeID: {2} ", mask, BitConverter.ToUInt64(guid.GetNewGuid(), 0), objTypeId);
#endif
                            UInt32 flags2 = 0, unk3;
                            float posX = 0;
                            float posY = 0;
                            float posZ = 0;
                            float facing = 0;
                            float walkSpeed, runSpeed, backWalkSpeed, swimSpeed, backSwimSpeed, turnRate = 0;

                            byte flags = wr.ReadByte();

                            if ((flags & 0x20) >= 1)
                            {
                                //BoogieCore.Log(LogType.NeworkComms, "(flags & 20)");
                                flags2 = wr.ReadUInt32();
                                wr.ReadByte(); // 2.3.3
                                unk3 = wr.ReadUInt32();
                            }

                            if ((flags & 0x40) >= 1)
                            {
                                //BoogieCore.Log(LogType.NeworkComms, "(flags & 40)");
                                posX = wr.ReadSingle();
                                posY = wr.ReadSingle();
                                posZ = wr.ReadSingle();
                                facing = wr.ReadSingle();
                                //BoogieCore.Log(LogType.NeworkComms, "Position - X: {0} Y: {1} Z: {2} Orient: {3} ", posX, posY, posZ, facing);

                                if (((flags & 0x20) >= 1 && (flags2 & 0x0200) >= 1))	// player being transported
                                {

                                    //BoogieCore.Log(LogType.NeworkComms, "(flags & 0x20 && flags2 & 0x0200)");
                                    wr.ReadUInt32();	//guidlow
                                    wr.ReadUInt32();	//guidhigh
                                    wr.ReadSingle();	//x
                                    wr.ReadSingle();	//y
                                    wr.ReadSingle();	//z
                                    wr.ReadSingle();	//o
                                    wr.ReadSingle();    // unk
                                }
                            }


                            if ((flags & 0x20) >= 1)
                            {
                                //BoogieCore.Log(LogType.NeworkComms, "(flags & 20)");
                                wr.ReadSingle(); //unk
                                if ((flags2 & 0x2000) >= 1)
                                {
                                    //BoogieCore.Log(LogType.NeworkComms, "(flags & 2000)");
                                    wr.ReadSingle(); // pos unk1
                                    wr.ReadSingle(); // pos unk1
                                    wr.ReadSingle(); // pos unk1
                                    wr.ReadSingle(); // pos unk1
                                    //BoogieCore.Log(LogType.NeworkComms, "Position 2 - X: {0} Y: {1} Z: {2} Orient: {3} ", punk1, punk2, punk3, punk1);
                                }
                            }

                            if ((flags & 0x20) >= 1)
                            {
                                //BoogieCore.Log(LogType.NeworkComms, "(flags & 20)");
                                walkSpeed = wr.ReadSingle();
                                runSpeed = wr.ReadSingle();
                                backWalkSpeed = wr.ReadSingle();
                                swimSpeed = wr.ReadSingle();
                                backSwimSpeed = wr.ReadSingle();
                                wr.ReadSingle(); //unk1
                                wr.ReadSingle(); //unk2
                                turnRate = wr.ReadSingle();
                                //BoogieCore.Log(LogType.NeworkComms, "Speed - (flags & 0x20)");
                            }

                            if ((flags & 0x20) >= 1)
                            {
                                //BoogieCore.Log(LogType.NeworkComms, "(flags & 0x20)");
                                if ((flags2 & 0x08000000) >= 1)
                                {
                                    //BoogieCore.Log(LogType.NeworkComms, "(flags2 & 0x00400000)");
                                    UInt32 splineFlags;

                                    splineFlags = wr.ReadUInt32();

                                    if ((splineFlags & 0x00010000) >= 1)
                                    {
                                        BoogieCore.Log(LogType.NeworkComms, "(splineFlags & 0x00010000)");
                                        posX = wr.ReadSingle();
                                        posY = wr.ReadSingle();
                                        posZ = wr.ReadSingle();
                                        BoogieCore.Log(LogType.NeworkComms, "Position 3 - X: {0} Y: {1} Z: {2} Orient: {3} ", posX, posY, posZ, facing);
                                    }

                                    if ((splineFlags & 0x00020000) >= 1)
                                    {
                                        BoogieCore.Log(LogType.NeworkComms, "(splineFlags & 0x00020000)");
                                        wr.ReadUInt64();
                                    }

                                    if ((splineFlags & 0x00040000) >= 1)
                                    {
                                        BoogieCore.Log(LogType.NeworkComms, "(splineFlags & 0x00040000)");
                                        float f;
                                        f = wr.ReadSingle();
                                    }

                                    UInt32 time1, time2, splineCount, unk4;

                                    //1.8
                                    time1 = wr.ReadUInt32();
                                    time2 = wr.ReadUInt32();
                                    unk4 = wr.ReadUInt32();
                                    splineCount = wr.ReadUInt32();
                                    BoogieCore.Log(LogType.NeworkComms, "splineCount = {0}", splineCount);

                                    for (UInt32 j = 0; j < splineCount + 1; j++)
                                    {

                                        posX = wr.ReadSingle();
                                        posY = wr.ReadSingle();
                                        posZ = wr.ReadSingle();
                                        //BoogieCore.Log(LogType.NeworkComms, "Position 4 - X: {0} Y: {1} Z: {2} Orient: {3} ", posX, posY, posZ, facing);
                                    }
                                    
                                 
                                }
                            }

                            if ((flags & 0x8) >= 1)
                            {
                                //BoogieCore.Log(LogType.NeworkComms, "(flags & 8)");
                                wr.ReadUInt32();
                                if ((flags & 0x10) >= 1)
                                {
                                    //BoogieCore.Log(LogType.NeworkComms, "(flags & 10)");
                                    wr.ReadUInt32();
                                }
                            }
                            else if ((flags & 0x10) >= 1)
                            {
                                //BoogieCore.Log(LogType.NeworkComms, "(flags & 10)");
                                wr.ReadUInt32();
                            }

                            if ((flags & 0x2) >= 1)
                            {
                                //BoogieCore.Log(LogType.NeworkComms, "(flags & 0x2)");
                                wr.ReadUInt32();
                            }

                            UpdateMask UpdateMask = new UpdateMask();

                            byte bc = wr.ReadByte(); // Block Count
                            //BoogieCore.Log(LogType.Error, "Block Count = {0}, Mask = {1}, flags = {2}, flags2 = {3}", bc * 32, mask, flags, flags2);


                            UpdateMask.SetCount((ushort)(bc * 32));
                            UpdateMask.SetMask(wr.ReadBytes(bc * 4), bc);

                            if (UpdateMask.GetCount() > 2500)
                            {
                                int count = UpdateMask.GetCount();
                                BoogieCore.Log(LogType.Error, "Bad mask count = {0} ! aborting parse", count);
                                return;
                            }

                            //BoogieCore.Log(LogType.NeworkComms, "(ObjCreate) FieldCount: {0}", UpdateMask.GetCount());
                            UInt32[] Fields = new UInt32[UpdateMask.GetCount()];

                            for (ushort x = 0; x < UpdateMask.GetCount(); x++)
                            {
                                if (UpdateMask.GetBit(x))
                                    Fields[x] = wr.ReadUInt32();
                            }

                            if (!BoogieCore.world.objectExists(guid))   // Add new Object
                            {
                                UInt32 entryid = Fields[(int)UpdateFields.OBJECT_FIELD_ENTRY];
                                Object NewObj = new Object();
                                NewObj.GUID = guid;
                                NewObj.coord = new Coordinate(posX, posY, posZ, facing);
                                NewObj.Type = flags;
                                NewObj.Fields = Fields;

                                //GetTile(posX, posY);  // This does nothing?

                                BoogieCore.world.newObject(NewObj, false);

                                if (objTypeId == 4)
                                {
                                    QueryName(guid);
                                    BoogieCore.Log(LogType.NeworkComms, "Adding new Player {0}", BitConverter.ToUInt64(guid.GetNewGuid(), 0));
                                }
                                if (objTypeId == 3 || objTypeId == 5)
                                {
                                    BoogieCore.Log(LogType.System, "Querying for name of object with an entry of {0} and type of {1}", entryid, objTypeId);
                                    if (EntryList.ContainsKey(entryid) == false && EntryQueue.ContainsKey(entryid) == false)
                                    {
                                        EntryQueue.Add(entryid, true);
                                        if (objTypeId == 3)
                                        {
                                            WoWWriter wr2 = CreatureQuery(guid, entryid);
                                            Send(wr2.ToArray());
                                            BoogieCore.Log(LogType.NeworkComms, "Adding new Unit {0}", BitConverter.ToUInt64(guid.GetNewGuid(), 0));
                                        }
                                        if (objTypeId == 5)
                                        {
                                            WoWWriter wr2 = GameObjectQuery(guid, entryid);
                                            Send(wr2.ToArray());
                                            BoogieCore.Log(LogType.NeworkComms, "Adding new GameObject {0}", BitConverter.ToUInt64(guid.GetNewGuid(), 0));
                                        }
                                    }
                                    
                                }
                            }
                            else    // Update Existing Object
                            {
                                Object updateObj = BoogieCore.world.getObject(guid);

                                updateObj.coord = new Coordinate(posX, posY, posZ, facing);
                                updateObj.Type = flags;
                                updateObj.Fields = Fields;
                                BoogieCore.world.updateObject(updateObj);
                            }
                            break;
                        }
                    case 4: // Out Of Range update
                        {
                            UInt32 count = wr.ReadUInt32();

                            for (UInt32 j = 0; j < count; j++)
                            {
                                byte mask = wr.ReadByte();

                                guid = new WoWGuid(mask, wr.ReadBytes(WoWGuid.BitCount8(mask)));

                                BoogieCore.world.delObject(guid);
                            }

                            break;
                        }
                }
            }
        }

        //private void GetTile(float x, float y)
        //{
        //    float X, Y;
        //    X = (x + (32.0f * 533.33333f));
        //    Y = (y + (32.0f * 533.33333f));
        //    int gx = (int)((x + (32.0f * 533.33333f)) / 533.33333f);
        //    int gy = (int)(Y / 533.33333f);
        //}

        private void Handle_ObjDestroy(WoWReader wr)
        {
            WoWGuid guid = new WoWGuid(wr.ReadUInt64());

            BoogieCore.world.delObject(guid);
        }


        private void Handle_GameObjectQuery(WoWReader wr)
        {
            Entry entry = new Entry();
            entry.entry = wr.ReadUInt32();
            if (entry.entry < 1 || wr.Remaining < 4)
            {
                BoogieCore.Log(LogType.System, "Got {1} in GameObject query response for entryid or remaining in packet too small {0}", wr.Remaining, entry.entry);
                return;
            }

            entry.Type = wr.ReadUInt32();
            entry.DisplayID = wr.ReadUInt32();
            entry.name = wr.ReadString();

            BoogieCore.Log(LogType.NeworkComms, "Got GameObject Query Response - Entry: {0} - Name: {1} - Type {2}", entry.entry, entry.name, entry.Type);
            if (EntryList.ContainsKey(entry.entry) == false)
                EntryList.Add(entry.entry, entry);

            if (EntryQueue.ContainsKey(entry.entry))
                EntryQueue.Remove(entry.entry);

            foreach (Object obj in BoogieCore.world.getObjectList())
            {
                if (obj.Fields != null)
                {
                    if (obj.Fields[(int)UpdateFields.OBJECT_FIELD_ENTRY] == entry.entry)
                    {
                        obj.Type = (byte)entry.Type;
                        obj.Name = entry.name;
                    }
                }
            }
        }

        private void Handle_CreatureQuery(WoWReader wr)
        {
            Entry entry = new Entry();
            entry.entry = wr.ReadUInt32();
            entry.name = wr.ReadString();
            entry.blarg = wr.ReadBytes(3);
            entry.subname = wr.ReadString();
            entry.flags = wr.ReadUInt32();
            entry.subtype = wr.ReadUInt32();
            entry.family = wr.ReadUInt32();
            entry.rank = wr.ReadUInt32();

            BoogieCore.Log(LogType.NeworkComms, "Got CreatureQuery Response - Entry: {0} - Name: {1} - SubName {2}", entry.entry, entry.name, entry.subname);
            if (EntryList.ContainsKey(entry.entry) == false)
                EntryList.Add(entry.entry, entry);

            if (EntryQueue.ContainsKey(entry.entry))
                EntryQueue.Remove(entry.entry);

            foreach (Object obj in BoogieCore.world.getObjectList())
            {
                if (obj.Fields != null)
                {
                    if (obj.Fields[(int)UpdateFields.OBJECT_FIELD_ENTRY] == entry.entry)
                    {
                        if (entry.name.Contains("Auctioneer") && SentHello == false)
                        {
                            WoWWriter ww = new WoWWriter(OpCode.MSG_AUCTION_HELLO);
                            ww.Write(obj.GUID.GetOldGuid());
                            Send(ww.ToArray());
                            BoogieCore.Log(LogType.SystemDebug, "Sent AH Hello!");
                            SentHello = true;
                        }

                        obj.Name = entry.name;
                        obj.SubName = entry.subname;
                        obj.SubType = entry.subtype;
                        obj.Family = entry.family;
                        obj.Rank = entry.rank;
                    }
                }
            }
        }

        public void QueryName(WoWGuid guid)
        {
            WoWWriter wr = new WoWWriter(OpCode.CMSG_NAME_QUERY);
            wr.Write(guid.GetOldGuid());
            Send(wr.ToArray());
        }

        public void QueryName(UInt64 guid)
        {
            WoWWriter wr = new WoWWriter(OpCode.CMSG_NAME_QUERY);
            wr.Write(guid);
            Send(wr.ToArray());
        }

        public WoWWriter CreatureQuery(WoWGuid guid, UInt32 entry)
        {
            WoWWriter wr = new WoWWriter(OpCode.CMSG_CREATURE_QUERY);
            wr.Write(entry);
            wr.Write(guid.GetOldGuid());
            return wr;
        }

        public WoWWriter GameObjectQuery(WoWGuid guid, UInt32 entry)
        {
            WoWWriter wr = new WoWWriter(OpCode.CMSG_GAMEOBJECT_QUERY);
            wr.Write(entry);
            wr.Write(guid.GetOldGuid());
            return wr;
        }


        private float GetDistance2dSq(Object obj)
        {
            Object mObj = BoogieCore.world.getPlayerObject();

            float dx = obj.coord.X - mObj.coord.X;
            float dy = obj.coord.Y - mObj.coord.Y;

            return (dx*dx) + (dy*dy);
        }

        private void Handle_NameQuery(WoWReader wr)
        {
            WoWGuid guid = new WoWGuid(wr.ReadUInt64());
            string name = wr.ReadString();
            UInt16 unk = wr.ReadByte();
            UInt32 Race = wr.ReadUInt32();
            UInt32 Gender = wr.ReadUInt32();
            UInt32 Level = wr.ReadUInt32();

            BoogieCore.Log(LogType.NeworkComms, "Got NameQuery Response - GUID: {4} Name: {0} - Race: {1} - Gender: {2} - Level: {3}", name, Race, Gender, Level, BitConverter.ToUInt64(guid.GetNewGuid(), 0));

            Object obj = BoogieCore.world.getObject(guid);

            if (obj != null)    // Update existing object
            {
                obj.Name = name;
                obj.Race = Race;
                obj.Gender = Gender;
                obj.Level = Level;
                BoogieCore.world.updateObject(obj);
            }
            else                // Create new Object        -- FIXME: Add to new 'names only' list?
            {
                obj = new Object();
                obj.GUID = guid;
                obj.Name = name;
                obj.Race = Race;
                obj.Gender = Gender;
                obj.Level = Level;
                BoogieCore.world.newObject(obj, true);
            }

            /* Process chat message if we looked them up now */
            for (int i = 0; i < ChatQueued.Count; i++)
            {
                ChatQueue message = (ChatQueue)ChatQueued[i];
                if (message.GUID.GetOldGuid() == guid.GetOldGuid())
                {
                    BoogieCore.Event(new Event(EventType.EVENT_CHAT, Time.GetTime(), message, name));
                    ChatQueued.Remove(message);
                }
            }

            // WoWChat uses this to retrive names on friends and ignore list.
            BoogieCore.Event(new Event(EventType.EVENT_NAMEQUERY_RESPONSE, Time.GetTime(), guid, name));
        }
    }

    public struct Entry
    {
        public UInt32 Type;
        public UInt32 DisplayID;
        public UInt32 entry;
        public string name;
        public byte[] blarg;
        public string subname;
        public UInt32 flags;
        public UInt32 subtype;
        public UInt32 family;
        public UInt32 rank;
    }
}
