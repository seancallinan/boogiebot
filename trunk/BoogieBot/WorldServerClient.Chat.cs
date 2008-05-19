using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Timers;
using System.IO;
using System.Threading;

using Foole.Crypt;
using Foole.Utils;
using Foole.WoW;

namespace BoogieBot.Common
{
    public enum ChatMsg
    {
        CHAT_MSG_ADDON = 0,
        CHAT_MSG_SAY = 1,
        CHAT_MSG_PARTY = 2,
        CHAT_MSG_RAID = 3,
        CHAT_MSG_GUILD = 4,
        CHAT_MSG_OFFICER = 5,
        CHAT_MSG_YELL = 6,
        CHAT_MSG_WHISPER = 7,
        CHAT_MSG_WHISPER_MOB = 8,
        CHAT_MSG_WHISPER_INFORM = 9,
        CHAT_MSG_EMOTE = 10,
        CHAT_MSG_TEXT_EMOTE = 11,
        CHAT_MSG_MONSTER_SAY = 12,
        CHAT_MSG_MONSTER_PARTY = 13,
        CHAT_MSG_MONSTER_YELL = 14,
        CHAT_MSG_MONSTER_WHISPER = 15,
        CHAT_MSG_MONSTER_EMOTE = 16,
        CHAT_MSG_CHANNEL = 17,
        CHAT_MSG_CHANNEL_JOIN = 18,
        CHAT_MSG_CHANNEL_LEAVE = 19,
        CHAT_MSG_CHANNEL_LIST = 20,
        CHAT_MSG_CHANNEL_NOTICE = 21,
        CHAT_MSG_CHANNEL_NOTICE_USER = 22,
        CHAT_MSG_AFK = 23,
        CHAT_MSG_DND = 24,
        CHAT_MSG_IGNORED = 25,
        CHAT_MSG_SKILL = 26,
        CHAT_MSG_LOOT = 27,
        CHAT_MSG_SYSTEM = 28,
        //29
        //30
        //31
        //32
        //33
        //34
        //35
        //36
        //37
        //38
        CHAT_MSG_BG_EVENT_NEUTRAL = 35,
        CHAT_MSG_BG_EVENT_ALLIANCE = 36,
        CHAT_MSG_BG_EVENT_HORDE = 37,
        CHAT_MSG_COMBAT_FACTION_CHANGE = 38,
        CHAT_MSG_RAID_LEADER = 39,
        CHAT_MSG_RAID_WARNING = 40,
        CHAT_MSG_RAID_WARNING_WIDESCREEN = 41,
        //42
        CHAT_MSG_FILTERED = 43,
        CHAT_MSG_BATTLEGROUND = 44,
        CHAT_MSG_BATTLEGROUND_LEADER = 45,
        CHAT_MSG_RESTRICTED = 46,
    };

    public delegate void ChatCB(ChatQueue que, string Username);

    public struct ChatQueue
    {
        public WoWGuid GUID;
        public byte Type;
        public UInt32 Language;
        public string Channel;
        public UInt32 Length;
        public string Message;
        public byte AFK;

    };

    // Also PLEASE make sure NUM_LANGUAGES is correctly updated as it is used.
    public enum Languages
    {
        LANG_UNIVERSAL = 0x00,
        LANG_ORCISH = 0x01,
        LANG_DARNASSIAN = 0x02,
        LANG_TAURAHE = 0x03,
        LANG_DWARVISH = 0x06,
        LANG_COMMON = 0x07,
        LANG_DEMONIC = 0x08,
        LANG_TITAN = 0x09,
        LANG_THELASSIAN = 0x0A,
        LANG_DRACONIC = 0x0B,
        LANG_KALIMAG = 0x0C,
        LANG_GNOMISH = 0x0D,
        LANG_TROLL = 0x0E,
        LANG_GUTTERSPEAK = 0x21,
        LANG_DRAENEI = 0x23,
        NUM_LANGUAGES = 0x24
    };

    partial class WorldServerClient
    {
        private ArrayList ChatQueued = new ArrayList();
        public ArrayList ChannelList = new ArrayList();

        private void Handle_MessageChat(WoWReader wr)
        {

            string channel = null;
            UInt64 guid = 0;
            WoWGuid fguid = null, fguid2 = null;


            byte Type = wr.ReadByte();
            UInt32 Language = wr.ReadUInt32();

            fguid = new WoWGuid(wr.ReadUInt64());
            wr.ReadUInt32(); // rank?

            if ((ChatMsg)Type == ChatMsg.CHAT_MSG_CHANNEL)
            {
                channel = wr.ReadString();
                //pvp_rank = wr.ReadUInt32();
            }            

            //if (Type == 0 || Type == 1 || Type == 5 || Type == 0x53)
            //{
                fguid2 = new WoWGuid(wr.ReadUInt64());

            //}

            UInt32 Length = wr.ReadUInt32();
            string Message = wr.ReadString();
            byte afk = wr.ReadByte();


            string username = null;


            ChatQueue que = new ChatQueue() ;
            que.GUID = fguid;
            que.Type = Type;
            que.Language = Language;
            if ((ChatMsg)Type == ChatMsg.CHAT_MSG_CHANNEL)
                que.Channel = channel;
            que.Length = Length;
            que.Message = Message;
            que.AFK = afk;

            if (fguid.GetOldGuid() == 0)
            {
                username = "System";
            }
            else
            {
                username = BoogieCore.world.getObjectName(fguid);
            }

            if (username == null)
            {
                ChatQueued.Add(que);
                QueryName(guid);
                return;
            }
            ParseCommands(que, username);

            BoogieCore.Event(new Event(EventType.EVENT_CHAT, Time.GetTime(), que, username));
        }

        private void ParseCommands(ChatQueue queue, string username)
        {
            if ((ChatMsg)queue.Type == ChatMsg.CHAT_MSG_SAY)
            {
                if (queue.Message == "face")
                {
                    Object obj = BoogieCore.world.getObject(queue.GUID);
                    if (obj != null)
                    {
                            Object player = BoogieCore.world.getPlayerObject();
                            player.SetOrientation(player.CalculateAngle(obj.GetPositionX(), obj.GetPositionY()) );
                            SendMoveHeartBeat();
                            string message = String.Format("Facing {0}", obj.Name);
                            SendChatMsg(ChatMsg.CHAT_MSG_SAY, Languages.LANG_UNIVERSAL, message);
                            return;
                    }
                    SendChatMsg(ChatMsg.CHAT_MSG_SAY, Languages.LANG_UNIVERSAL, String.Format("Unable to find {0} in obj list", username));
                }
                if (queue.Message == "run")
                {
                    StartMoveForward();
                    SendChatMsg(ChatMsg.CHAT_MSG_SAY, Languages.LANG_UNIVERSAL, "Running...");

                }
                if (queue.Message == "stop")
                {
                    StopMoveForward();
                    SendChatMsg(ChatMsg.CHAT_MSG_SAY, Languages.LANG_UNIVERSAL, "stopping...");
                }

            }
        }

        
        public void SendChatMsg(ChatMsg Type, Languages Language, string Message)
        {
            if (Type != ChatMsg.CHAT_MSG_WHISPER || Type != ChatMsg.CHAT_MSG_CHANNEL)
                SendChatMsg(Type, Language, Message, "");
            else
                BoogieCore.Log(LogType.Error, "Got whisper message to send without destination");
        }

        public void SendChatMsg(ChatMsg Type, Languages Language, string Message, string To)
        {
            WoWWriter wr = new WoWWriter(OpCode.CMSG_MESSAGECHAT);
            wr.Write((UInt32)Type);
            wr.Write((UInt32)Language);
            if ((Type == ChatMsg.CHAT_MSG_WHISPER || Type == ChatMsg.CHAT_MSG_CHANNEL) && To != "")
                wr.Write(To);
            wr.Write(Message);

            Send(wr.ToArray());
        }

        public void SendEmoteMsg(ChatMsg Type, Languages Language, string Message, string To)
        {
            WoWWriter wr = new WoWWriter(OpCode.CMSG_TEXT_EMOTE);
            wr.Write((UInt32)Type);
            wr.Write((UInt32)Language);
            wr.Write(Message);

            Send(wr.ToArray());
        }

        public void JoinChannel(string channel, string password)
        {
            WoWWriter wr = new WoWWriter(OpCode.CMSG_JOIN_CHANNEL);
            wr.Write((UInt32)0); // dbc id??
            wr.Write((UInt16)0); // crap as ascent put it

            wr.Write(channel);
            wr.Write((byte)0);
            Send(wr.ToArray());

            ChannelList.Add(channel);

            BoogieCore.Event( new Event(EventType.EVENT_CHANNEL_JOINED, Time.GetTime(), channel) );
        }

        public void PartChannel(string channel)
        {
            WoWWriter wr = new WoWWriter(OpCode.CMSG_LEAVE_CHANNEL);
            wr.Write((UInt32)0);
            wr.Write(channel);
            Send(wr.ToArray());

            ChannelList.Remove(channel);

            BoogieCore.Event(new Event(EventType.EVENT_CHANNEL_LEFT, Time.GetTime(), channel));
        }
    }
}