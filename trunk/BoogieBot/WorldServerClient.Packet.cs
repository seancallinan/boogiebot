using System;
using System.Collections.Generic;
using System.Text;

using Foole.Crypt;
using Foole.Utils;
using Foole.WoW;

namespace BoogieBot.Common
{
    // Protocol Switch
    partial class WorldServerClient
    {
        protected void processData(byte[] Data)
        {
            
            WoWReader wr = new WoWReader(Data);
            OpCode Op = (OpCode)wr.ReadUInt16();

            BoogieCore.Log(LogType.NeworkComms, "Debugging packet for opcode: {0}", Op);
            SMSG_Debug(new WoWReader(Data));

            try
            {
                switch (Op)
                {
                    case OpCode.SMSG_AUTH_CHALLENGE:
                        Handle_AuthRequest(wr);
                        break;
                    case OpCode.SMSG_AUTH_RESPONSE:
                        Handle_AuthResponse(wr);
                        break;
                    case OpCode.SMSG_CHAR_ENUM:
                        Handle_CharEnum(wr);
                        break;
                    case OpCode.SMSG_WARDEN_DATA:
                        // Warden was figured out. but I won't give it out. GL
                        break;
                    case OpCode.SMSG_ACCOUNT_DATA_MD5:
                        WoWWriter ww = new WoWWriter(OpCode.CMSG_UPDATE_ACCOUNT_DATA);
                        ww.Write(0x00000002);
                        ww.Write((UInt32)0);
                        Send(ww.ToArray());
                        break;
                    case OpCode.SMSG_PONG:
                        {
                            UInt32 Server_Seq = wr.ReadUInt32();
                            if (Server_Seq == Ping_Seq)
                            {
                                Ping_Res_Time = MM_GetTime();
                                Latency = Ping_Res_Time - Ping_Req_Time;
                                Ping_Seq += 1;
                                //BoogieCore.Log(LogType.NeworkComms, "Got pong'd with a latency of: {0} sequence: {1}", Latency, Server_Seq);
                            }
                            else
                                BoogieCore.Log(LogType.Error, "Server pong'd bad sequence! Ours: {0} Theirs: {1}", Ping_Seq, Server_Seq);
                            break;
                        }
                    case OpCode.SMSG_ITEM_QUERY_SINGLE_RESPONSE:
                        BoogieCore.Log(LogType.NeworkComms, "Got Item Response");
                        break;
                    case OpCode.SMSG_SET_PROFICIENCY:
                        break;
                    case OpCode.MSG_MOVE_HEARTBEAT:
                    case OpCode.MSG_MOVE_START_TURN_RIGHT:
                    case OpCode.MSG_MOVE_STOP:
                    case OpCode.MSG_MOVE_START_TURN_LEFT:
                    case OpCode.MSG_MOVE_START_FORWARD:
                    case OpCode.MSG_MOVE_START_BACKWARD:
                    case OpCode.MSG_MOVE_STOP_TURN:
                    case OpCode.MSG_MOVE_START_STRAFE_LEFT:
                    case OpCode.MSG_MOVE_START_STRAFE_RIGHT:
                    case OpCode.MSG_MOVE_STOP_STRAFE:
                    case OpCode.MSG_MOVE_FALL_LAND:
                    case OpCode.MSG_MOVE_JUMP:
                    case OpCode.MSG_MOVE_SET_FACING:
                        MovementHandler(wr);
                        break;
                    case OpCode.SMSG_EMOTE:
                        break;
                    case OpCode.SMSG_WEATHER:
                        break;
                    case OpCode.MSG_MOVE_TELEPORT_ACK:
                        TeleportHandler(wr);
                        break;
                    case OpCode.SMSG_NEW_WORLD:
                        Handle_NewWorld(wr);
                        break;
                    case OpCode.SMSG_FORCE_MOVE_UNROOT:
                        SendMoveHeartBeat(BoogieCore.world.getPlayerObject().GetCoordinates());
                        break;
                    case OpCode.SMSG_UPDATE_OBJECT:
                        Handle_ObjUpdate(wr, false);
                        break;
                    case OpCode.SMSG_DESTROY_OBJECT:
                        Handle_ObjDestroy(wr);
                        break;
                    case OpCode.SMSG_COMPRESSED_UPDATE_OBJECT:
                        Handle_ObjUpdate(wr, true);
                        break;
                    case OpCode.SMSG_SPELL_START:
                        break;
                    case OpCode.SMSG_SPELL_GO:
                        break;
                    case OpCode.SMSG_CAST_RESULT:
                        break;
                    case OpCode.SMSG_MESSAGECHAT:
                        Handle_MessageChat(wr);
                        break;
                    case OpCode.SMSG_CHANNEL_NOTIFY:
                        break;
                    case OpCode.SMSG_NAME_QUERY_RESPONSE:
                        Handle_NameQuery(wr);
                        break;
                    case OpCode.SMSG_CREATURE_QUERY_RESPONSE:
                        Handle_CreatureQuery(wr);
                        break;
                    case OpCode.SMSG_GAMEOBJECT_QUERY_RESPONSE:
                        SMSG_Debug(wr);
                        break;
                    case OpCode.MSG_AUCTION_HELLO:
                        BoogieCore.Log(LogType.NeworkComms, "Got ah Hello!");
                        Query_AH(AHEntry);
                        break;
                    case OpCode.SMSG_AUCTION_LIST_RESULT:
                        BoogieCore.Log(LogType.NeworkComms, "Got ah response!");
                        Query_AH(AHEntry);
                        break;
                    case OpCode.SMSG_FRIEND_LIST:
                        Handle_FriendsList(wr);
                        break;
                    case OpCode.SMSG_FRIEND_STATUS:
                        Handle_FriendStatus(wr);
                        break;
                    //case OpCode.SMSG_IGNORE_LIST:
                    //    Handle_IgnoreList(wr);
                    //    break;
                    case OpCode.SMSG_INIT_WORLD_STATES:
                        Handle_InitWorldStates(wr);
                        break;
                    case OpCode.SMSG_INITIAL_SPELLS:
                        Handle_InitialSpells(wr);
                        break;
                    case OpCode.SMSG_LEARNED_SPELL:
                        Handle_LearntSpell(wr);
                        break;
                    case OpCode.SMSG_SUPERCEDED_SPELL:
                        Handle_SupercededSpell(wr);
                        break;
                    case OpCode.SMSG_INITIALIZE_FACTIONS:
                        Handle_InitializeFactions(wr);
                        break;
                    case OpCode.SMSG_LOGIN_SETTIMESPEED:
                        Handle_LoginSetTimeSpeed(wr);
                        break;
                    case OpCode.SMSG_SPELLLOGEXECUTE:
                        Handle_SpellLogExecute(wr);
                        break;
                    case OpCode.SMSG_MAIL_LIST_RESULT:
                        Handle_MailList(wr);
                        break;
                    case OpCode.SMSG_SEND_MAIL_RESULT:
                        // We don't send mail (yet).
                        break;
                    case OpCode.SMSG_RECEIVED_MAIL:
                        // You've got MAIL!
                        break;
                    case OpCode.SMSG_LIST_INVENTORY:
                        Handle_VendorInventoryList(wr);
                        break;
                    case OpCode.SMSG_ACTION_BUTTONS:
                        Handle_ActionButtons(wr);
                        break;
                    case OpCode.SMSG_LEVELUP_INFO:
                        Handle_LevelUp(wr);
                        break;
                    case OpCode.SMSG_LOG_XPGAIN:
                        Handle_XpGain(wr);
                        break;
                    default:
                        BoogieCore.Log(LogType.NeworkComms, "Got unknown opcode: {0} length: {1}", Op, wr.Remaining);
                        break;
                }
            }
            catch (Exception ex)
            {
                BoogieCore.Log(LogType.Error, "Caught Exception while processing packet with opcode of {0}:  Exception is: {1}", Op, ex.Message);
                //BoogieCore.Log(LogType.Error, "{0}", ex.StackTrace);
            }
        }
    }
}
