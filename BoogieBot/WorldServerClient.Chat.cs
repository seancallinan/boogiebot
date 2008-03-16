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
        CHAT_MSG_SAY = 0x00,
        CHAT_MSG_PARTY = 0x01,
        CHAT_MSG_RAID = 0x02,
        CHAT_MSG_GUILD = 0x03,
        CHAT_MSG_OFFICER = 0x04,
        CHAT_MSG_YELL = 0x05,
        CHAT_MSG_WHISPER = 0x06,
        CHAT_MSG_WHISPER_INFORM = 0x07,
        CHAT_MSG_EMOTE = 0x08,
        CHAT_MSG_TEXT_EMOTE = 0x09,
        CHAT_MSG_SYSTEM = 0x0A,
        CHAT_MSG_MONSTER_SAY = 0x0B,
        CHAT_MSG_MONSTER_YELL = 0x0C,
        CHAT_MSG_MONSTER_EMOTE = 0x0D,
        CHAT_MSG_CHANNEL = 0x0E,
        CHAT_MSG_CHANNEL_JOIN = 0x0F,
        CHAT_MSG_CHANNEL_LEAVE = 0x10,
        CHAT_MSG_CHANNEL_LIST = 0x11,
        CHAT_MSG_CHANNEL_NOTICE = 0x12,
        CHAT_MSG_CHANNEL_NOTICE_USER = 0x13,
        CHAT_MSG_AFK = 0x14,
        CHAT_MSG_DND = 0x15,
        CHAT_MSG_COMBAT_LOG = 0x16,
        CHAT_MSG_IGNORED = 0x17,
        CHAT_MSG_SKILL = 0x18,
        CHAT_MSG_LOOT = 0x19,
        CHAT_MSG_RAIDLEADER = 0x57,
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

    // Where is Undead Gutterspeak ? 4 or 5? Also PLEASE make sure NUM_LANGUAGES is correctly updated as it is used.
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
        NUM_LANGUAGES = 0x0E
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
            UInt32 pvp_rank = 0;


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

            Event e = new Event(EventType.EVENT_CHAT, Time.GetTime(), que, username);
            BoogieCore.Event(e);
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

            Event e = new Event(EventType.EVENT_CHANNEL_JOINED, Time.GetTime(), channel);
            BoogieCore.Event(e);
        }

        public void PartChannel(string channel)
        {
            WoWWriter wr = new WoWWriter(OpCode.CMSG_LEAVE_CHANNEL);
            wr.Write((UInt32)0);
            wr.Write(channel);
            Send(wr.ToArray());

            ChannelList.Remove(channel);

            Event e = new Event(EventType.EVENT_CHANNEL_LEFT, Time.GetTime(), channel);
            BoogieCore.Event(e);
        }
    }
}