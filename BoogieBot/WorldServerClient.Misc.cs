using System;
using System.Collections.Generic;
using System.Text;

using Foole.Crypt;
using Foole.Utils;
using Foole.WoW;

namespace BoogieBot.Common
{
    // Misc. Packet Handling
    partial class WorldServerClient
    {
        private void Handle_InitWorldStates(WoWReader wr)
        {
            BoogieCore.Log(LogType.NeworkComms, "WS: Recieved Init World States..");
            SMSG_Debug(wr);
        }

        private void Handle_LoginSetTimeSpeed(WoWReader wr)
        {
            BoogieCore.Log(LogType.NeworkComms, "WS: Recieved Login SetTimeSpeed (??)..");
            SMSG_Debug(wr);
        }

        private void Handle_SpellLogExecute(WoWReader wr)
        {
            BoogieCore.Log(LogType.NeworkComms, "WS: Recieved Spell Log Execute..");
            SMSG_Debug(wr);
        }

        private void Handle_NewWorld(WoWReader wr)
        {
            Object obj = BoogieCore.world.getPlayerObject();
            WorldZone(obj.GUID.GetOldGuid());
            
            UInt32 mapid = wr.ReadUInt32();
            BoogieCore.world.zoned(mapid);          // Tell World we zoned, and give new mapid
            obj.coord = new Coordinate(wr.ReadSingle(), wr.ReadSingle(), wr.ReadSingle(), wr.ReadSingle());
            WoWWriter ww = new WoWWriter(OpCode.MSG_MOVE_WORLDPORT_ACK);
            //ww.Write(BoogieCore.world.getPlayerObject().GUID.GetOldGuid());
            Send(ww.ToArray());
            SendMoveHeartBeat(obj.coord);
            BoogieCore.Log(LogType.System, "Got worldport for mapid: {0} xyz: {1} {2} {3}", mapid, obj.coord.X, obj.coord.Y, obj.coord.Z);
            BoogieCore.world.updatePlayerLocationUI();
        }


        private void Handle_VendorInventoryList(WoWReader wr)
        {
        }

        private void Query_AH(UInt32 entry)
        {
            WoWWriter wr = new WoWWriter(OpCode.CMSG_AUCTION_LIST_ITEMS);
            wr.Write(entry);
            wr.Write((byte)0); // "message"
            wr.Write((byte)0); // minlevel
            wr.Write((byte)0); // maxlevel
            wr.Write(0xFFFFFFFF); // unk
            wr.Write(0xFFFFFFFF); // item class
            wr.Write(0xFFFFFFFF); // item subclass
            wr.Write(0xFFFFFFFF); // rarity
            wr.Write((byte)0); // usable
            Send(wr.ToArray());
            AHEntry = entry + 50;
        }

        private void Query_Item_Single(UInt32 id)
        {
            WoWWriter ww = new WoWWriter(OpCode.CMSG_ITEM_QUERY_SINGLE);
            ww.Write(id);
            ww.Write((UInt64)0);
            Send(ww.ToArray());
        }
    }
}