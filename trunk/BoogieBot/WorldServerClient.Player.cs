using System;
using System.Collections.Generic;
using System.Text;

using Foole.Crypt;
using Foole.Utils;
using Foole.WoW;

namespace BoogieBot.Common
{
    // Player Packet Handling
    partial class WorldServerClient
    {
        private void Handle_FriendsList(WoWReader wr)
        {
            byte count = wr.ReadByte();

            FriendsListItem[] friendsList = new FriendsListItem[count];

            for (int i = 0; i < count; i++)
            {
                friendsList[i] = new FriendsListItem();
                friendsList[i].guid = new WoWGuid(wr.ReadUInt64());
                friendsList[i].online = wr.ReadBoolean();
                QueryName(friendsList[i].guid);
            }

            BoogieCore.Player.setFriendList(friendsList);
        }

        private void Handle_IgnoreList(WoWReader wr)
        {
            byte count = wr.ReadByte();

            IgnoreListItem[] ignoreList = new IgnoreListItem[count];

            for (int i = 0; i < count; i++)
            {
                ignoreList[i] = new IgnoreListItem();
                ignoreList[i].guid = new WoWGuid(wr.ReadUInt64());
                QueryName(ignoreList[i].guid);
            }

            BoogieCore.Player.setIgnoreList(ignoreList);
        }

        private void Handle_FriendStatus(WoWReader wr)
        {
            FriendsListItem friendStatus = new FriendsListItem();
            friendStatus.guid = new WoWGuid(wr.ReadUInt64());
            friendStatus.online = wr.ReadBoolean();

            BoogieCore.Player.friendStatusUpdate(friendStatus);
        }

        private void Handle_InitialSpells(WoWReader wr)
        {
            byte unknown = wr.ReadByte();           // Dunno. wowd sends a 0.
            UInt16 count = wr.ReadUInt16();

            SpellItem[] spellList = new SpellItem[count];

            for (int i = 0; i < count; i++)
            {
                spellList[i].spellID = wr.ReadUInt16();
                spellList[i].unknown = wr.ReadUInt16(); // 0xeeee
            }

            wr.ReadUInt16();              // Another 0 according to wowd.

            BoogieCore.Player.setSpells(spellList);
        }

        private void Handle_InitializeFactions(WoWReader wr)
        {
            UInt32 count = wr.ReadUInt32();     // always 64

            ReputationItem[] reputationList = new ReputationItem[count];

            for (int i = 0; i < count; i++)
            {
                reputationList[i].flag = wr.ReadByte();
                reputationList[i].standing = wr.ReadUInt32();
            }

            BoogieCore.Player.setReputation(reputationList);
        }

        private void Handle_ActionButtons(WoWReader wr)
        {
            ActionButton[] actionButtonList = new ActionButton[ActionBars.MaxButtons];

            for (int i = 0; i < ActionBars.MaxButtons; i++)
            {
                actionButtonList[i].action = wr.ReadUInt16();
                actionButtonList[i].type = wr.ReadByte();
                actionButtonList[i].misc = wr.ReadByte();
            }

            BoogieCore.Player.setActionBars(actionButtonList);
        }

        private void Handle_MailList(WoWReader wr)
        {
            BoogieCore.Log(LogType.NeworkComms, "WS: Recieved Mail List.. {0} bytes.", wr.Remaining);
            SMSG_Debug(wr);
        }

        private void Handle_LearntSpell(WoWReader wr)
        {
            // BoogieCore.Player.Spells.learntSpell()
        }

        private void Handle_SupercededSpell(WoWReader wr)
        {
            // BoogieCore.Player.Spells.supercededSpell()
        }

        private void Handle_LevelUp(WoWReader wr)
        {
            //BoogieCore.Player.levelUp();
        }

        private void Handle_XpGain(WoWReader wr)
        {
            //BoogieCore.Player.xpGain();
            //BoogieCore.AI.xpGain(); ??
        }

        public void Query_GetMailList(WoWGuid mailbox_guid)
        {
            WoWWriter wr = new WoWWriter(OpCode.CMSG_GET_MAIL_LIST);
            wr.Write(mailbox_guid.GetOldGuid());
            Send(wr.ToArray());
        }
    }
}
