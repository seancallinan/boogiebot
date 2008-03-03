using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Foole.WoW;        // for WoWGuid and UpdateFields

namespace BoogieBot.Common
{
    // Represents the 'Player' being played by the bot.
    public class Player
    {
        private Character character;

        private uint level;
        private uint exp;
        private uint nextlevelexp;

        private uint hp;
        private uint mana;
        private uint energy;
        private uint rage;

        private Boolean inited = false;

        private int globalCoolDown;

        private ActionBars actionBars;                  // Players Action Bars
        private FriendsListItem[] friendsList;          // Friends List
        private IgnoreListItem[] ignoreList;            // Ignore List
        private Talents talents;                        // Talents (Class Build)
        private Inventory inventory;                    // Items in players Inventory. (all bags, combined, treated as one large inventory)
        private Bank bank;                              // Items in players Bank. (bank, and bags, all combined)
        private Spells spells;                          // Spells the player currently knows. Spell cooldowns kept in here.
        private Reputation reputation;                  // Players reputation with various factions
        private Equipped equipped;                      // Items currently equipped. Durability also stored here.
        private Buffs buffs;                            // Players Buffs
        private Debuffs debuffs;                        // Players Debuffs
        private QuestLog questLog;                      // Players Quest Log

        public Player()
        {
        }

        // Initialize Player, with Character Object
        public void setPlayer(Character c)
        {
            character = c;
        }

        // Initialize Player, with Player Object Update Fields :D
        public void setPlayer(Object po)
        {
            BoogieCore.Log(LogType.System, "Player Class Initialized!");

            level           = po.Fields[(int)UpdateFields.UNIT_FIELD_LEVEL];
            //exp             = po.Fields[(int)UpdateFields.PLAYER_XP];
            //nextlevelexp    = po.Fields[(int)UpdateFields.PLAYER_NEXT_LEVEL_XP];
            hp              = po.Fields[(int)UpdateFields.UNIT_FIELD_HEALTH];       // probably wrong?

            // Create contained Objects
            questLog = new QuestLog(po);
            inventory = new Inventory(po);
            bank = new Bank(po);
            equipped = new Equipped(po);
            talents = new Talents(po);
            buffs = new Buffs(po);
            debuffs = new Debuffs(po);

            inited = true;
        }

        // Update Player, with Player Object Update Fields :D
        public void updatePlayer(Object po)
        {
            if (!inited)
                return;

            BoogieCore.Log(LogType.System, "Player Class Updated!");

            level = po.Fields[(int)UpdateFields.UNIT_FIELD_LEVEL];
            //exp = po.Fields[(int)UpdateFields.PLAYER_XP];
            //nextlevelexp = po.Fields[(int)UpdateFields.PLAYER_NEXT_LEVEL_XP];
            hp = po.Fields[(int)UpdateFields.UNIT_FIELD_HEALTH];       // probably wrong?
        }

        // Initialize Friends List, from the list recieved from the WorldServer
        public void setFriendList(FriendsListItem[] fl)
        {
            friendsList = fl;
        }

        // Initialize Ignore List, from the list recieved from the WorldServer
        public void setIgnoreList(IgnoreListItem[] il)
        {
            ignoreList = il;
        }

        // Update status of a Friend on our Friends List
        public void friendStatusUpdate(FriendsListItem f)
        {
            // Find friend in friends list, and update online status.
            for (int i = 0; i < friendsList.Length; i++)
            {
                if (friendsList[i].guid.GetOldGuid() == f.guid.GetOldGuid())
                {
                    friendsList[i].online = f.online;
                    return;
                }
            }
        }

        // Create new Spells Object, initialized by the list recieved from the WorldServer
        public void setSpells(SpellItem[] sl)
        {
            spells = new Spells(sl);
        }

        // Create new Reputation Object, initialized by the list recieved from the WorldServer
        public void setReputation(ReputationItem[] rl)
        {
            reputation = new Reputation(rl);
        }

        // Create a new Action Buttons Object, initialized by the list recieved from the WorldServer
        public void setActionBars(ActionButton[] abl)
        {
            actionBars = new ActionBars(abl);
        }

        // Do Levelup Stuff
        public void levelUp()
        {
            level++;
        }

        // Properties
        public Boolean      Inited      { get { return inited;      } }
        public Character    Character   { get { return character;   } }
        public Spells       Spells      { get { return spells;      } }
        public Reputation   Reputation  { get { return reputation;  } }
        public ActionBars   ActionBars  { get { return actionBars;  } }
        public Inventory    Inventory   { get { return inventory;   } }
        public Bank         Bank        { get { return bank;        } }
        public Equipped     Equipped    { get { return equipped;    } }
        public Talents      Talents     { get { return talents;     } }
        public Buffs        Buffs       { get { return buffs;       } }
        public Debuffs      Debuffs     { get { return debuffs;     } }
        public QuestLog     QuestLog    { get { return questLog;    } }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Player Class:\n");

            sb.Append(String.Format("Level:   {0}\n", level));
            sb.Append(String.Format("Exp:     {0}\n", exp));
            sb.Append(String.Format("HP:      {0}\n", hp));
            sb.Append(String.Format("Mana:    {0}\n", mana));
            sb.Append(String.Format("Rage:    {0}\n", rage));
            sb.Append(String.Format("Energy:  {0}\n", energy));

            // Append containted objects
            if(spells     != null) sb.Append(spells);
            if(reputation != null) sb.Append(reputation);
            if(actionBars != null) sb.Append(actionBars);
            if(inventory  != null) sb.Append(inventory);
            if(bank       != null) sb.Append(bank);
            if(equipped   != null) sb.Append(equipped);
            if(talents    != null) sb.Append(talents);
            if(buffs      != null) sb.Append(buffs);
            if(debuffs    != null) sb.Append(debuffs);
            if(questLog   != null) sb.Append(questLog);

            sb.Append("EOF.\n");

            return sb.ToString();
        }
    }

    // This is retrieved from the worldserver. May as well hold onto it in Player.
    public struct Character
    {
        public UInt64 GUID;
        public string Name;
        public byte Race;
        public byte Class;
        public byte Gender;
        public byte Skin;
        public byte Face;
        public byte HairStyle;
        public byte HairColor;
        public byte FacialHair;
        public byte Level;
        public UInt32 ZoneID;
        public UInt32 MapID;
        public float X;
        public float Y;
        public float Z;
        public UInt32 Guild;
        public UInt32 Unk;
        public byte RestState;
        public UInt32 PetInfoID;
        public UInt32 PetLevel;
        public UInt32 PetFamilyID;
        public CharEquipment[] Equipment;
    }

    public struct FriendsListItem
    {
        public WoWGuid guid;
        public bool online;
    }

    public struct IgnoreListItem
    {
        public WoWGuid guid;
    }
}