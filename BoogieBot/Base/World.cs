using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using Foole.Utils;
using Foole.WoW;

namespace BoogieBot.Common
{
    /// <summary>Keeps track of the world. Keeps track of all objects, including the player as well as providing methods to move (or warp) the player and provides collision detection and pathing.</summary>
    public class World
    {
        private uint mMapID;
        private WoWGuid playerGuid;
        private List<Object> mObjects;

        public World()
        {
            mObjects = new List<Object>();
        }

        /// <summary>Set player object. This is done at login once a character is chosen.</summary>
        /// <param name="mapid">The map id</param>
        /// <param name="guid">Game UID of the Player Object</param>
        public void setPlayer(uint mapid, WoWGuid guid)
        {
            playerGuid = guid;
            mMapID = mapid;
        }

        /// <summary>Returns the current World MapID. This changes every time we zone.</summary>
        public uint getMapID()
        {
            return mMapID;
        }

        /// <summary>Updates the World (and wmo/terrain managers) that we have Zoned.</summary>
        /// <param name="mapid">The new map id</param>
        public void zoned(uint mapid)
        {
            mMapID = mapid;

        }

        /// <summary>Adds a new Object to the World.</summary>
        /// <param name="o">Objects to add.</param>
        public void newObject(Object o, bool nameonly)
        {
            mObjects.Add(o);

            if (nameonly)
                return;

            // If this is the player object being added, set up the Player Class.
            if(!BoogieCore.Player.Inited)
                if (o.GUID.GetOldGuid() == BoogieCore.Player.Character.GUID)
                {
                    BoogieCore.Player.setPlayer(o);
                }
        }

        /// <summary>Removes an Object from the World.</summary>
        /// <param name="guid">Guid of the objects to delete.</param>
        public void delObject(WoWGuid guid)
        {
            int i = getObjectIndex(guid);
            if (i >= 0)
                mObjects.RemoveAt(i);
        }

        /// <summary>Updates an Object in the World.</summary>
        /// <param name="o">Objects to update.</param>
        public void updateObject(Object o) // NOTE: Made this private so its not used. Object is now a class not a struct, so it can be updated anywhere without needing this method.
        {
            int i = getObjectIndex(o.GUID);
            mObjects[i] = o;

            // If this is the player object being updated, set up the Player Class.
            if (!BoogieCore.Player.Inited)
                if (o.GUID.GetOldGuid() == BoogieCore.Player.Character.GUID)
                    BoogieCore.Player.setPlayer(o);
        }

        /// <summary>Returns the Name of an Object in the World.</summary>
        /// <param name="guid">Guid of the objects to query.</param>
        public String getObjectName(WoWGuid guid)
        {
            int i = getObjectIndex(guid);

            if (i >= 0)
                return mObjects[i].Name;
            else
                return null;
        }

        /// <summary>Returns an Object from the World.</summary>
        /// <param name="guid">Guid of the objects to obtain.</param>
        public Object getObject(WoWGuid guid)
        {
            int i = getObjectIndex(guid);

            if (i >= 0)
                return mObjects[i];
            else
                return null;
        }

        /// <summary>Returns the Player Object.</summary>
        public Object getPlayerObject()
        {
            int i = getObjectIndex(playerGuid);

            if (i >= 0)
                return mObjects[i];
            else
                return null;
        }

        /// <summary>Returns the full list of Objects in the World.</summary>
        public List<Object> getObjectList()
        {
            return mObjects;
        }

        public Object[] getObjectListArray()
        {
            return mObjects.ToArray();
        }

        /// <summary>Returns whether a certain Object already exists in the World.</summary>
        /// <param name="o">Objects guid to check for.</param>
        public Boolean objectExists(WoWGuid guid)
        {
            if (getObjectIndex(guid) >= 0)
                return true;
            else
                return false;
        }

        // Find Object on list by GUID
        private int getObjectIndex(WoWGuid guid)
        {
            for (int i = 0; i < mObjects.Count; i++)
            {
                Object obj = mObjects[i];
                if (obj.GUID.GetOldGuid() == guid.GetOldGuid())
                    return i;
            }
            return -1;
        }

        // Find Object on list by Name
        private int getObjectIndex(String name)
        {
            if (name == "" || name == null)
                return -1;

            for (int i = 0; i < mObjects.Count; i++)
            {
                Object obj = mObjects[i];

                if (obj.Name == "" || obj.Name == null)
                    continue;
                if (obj.Name.ToLower() == name.ToLower())
                    return i;
            }
            return -1;
        }

        /// <summary>Moves the player to the supplied coordinate.</summary>
        /// <param name="c">Coordinate to move the player to.</param>
        public void movePlayerTo(Coordinate c)
        {
        }

        /// <summary>Moves the player to the supplied coordinate.</summary>
        /// <param name="c">Coordinate to move the player to.</param>
        /// <param name="o">Orientation in which to move the player. (eg, for strafing)</param>
        public void movePlayerTo(Coordinate c, float o)
        {
        }

        /// <summary>Warps the player to the supplied coordinate.</summary>
        /// <param name="c">Coordinate to warp the player to.</param>
        public void warpPlayerTo(Coordinate c)
        {
            BoogieCore.Log(LogType.System, "World: Attemping to warp player to: {0}", c);
            BoogieCore.WorldServerClient.SendMoveHeartBeat(c);
        }

        public void StartRunForward()
        {
            BoogieCore.Log(LogType.System, "Starting run forward!");
            BoogieCore.WorldServerClient.StartMoveForward();
        }
        public void StopRun()
        {
            BoogieCore.WorldServerClient.StopMoveForward();
        }

        public void StartJump()
        {
            BoogieCore.WorldServerClient.MoveJump();
        }
        /// <summary>Tells the server to retrieve the mail list. Must be near a mailbox to work.</summary>
        public void getMail()
        {
            // Find the mailbox on the object list. (FIXME: Need to work out which is closest, and check if we are in range?)
            /*for (int i = 0; i < mObjects.Count; i++)
            {
                if(mObjects[i].Fields != null)  // Must have Fields
                    if(mObjects[i].Fields.Length >= (uint)UpdateFields.GAMEOBJECT_END)  // Fields array must go far enough
                        if(mObjects[i].Fields[(int)UpdateFields.GAMEOBJECT_TYPE_ID] == (uint)GAMEOBJECT_TYPES.GAMEOBJECT_TYPE_MAILBOX) // If its a mailbox
                        {
                            BoogieCore.Log(LogType.SystemDebug, "Attempting to read mail using mailbox {0}", mObjects[i].GUID);
                            BoogieCore.WorldServerClient.Query_GetMailList(mObjects[i].GUID);
                            return;
                        }
            }*/
            BoogieCore.Log(LogType.SystemDebug, "No nearby or known mailbox found!");
        }

        //  Updates the UI
        public void updatePlayerLocationUI()
        {
            Object mObj = getPlayerObject();

            // Update the UI on our location
            Event e1 = new Event(EventType.EVENT_SELF_MOVED, Time.GetTime(), mObj.coord);
            BoogieCore.Event(e1);


            //Event e2 = new Event(EventType.EVENT_LOCATION_UPDATE, Time.GetTime(), location);
            //BoogieCore.Event(e2);
        }


        // DEBUG CLASSES /////////////////////////////////////////////////////////////////
        public int DEBUG_ObjectCount()
        {
            return mObjects.Count;
        }

        public String DEBUG_ObjectNames()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < mObjects.Count; i++)
            {
                if (mObjects[i].Name == null || mObjects[i].Name == "")
                    sb.Append(String.Format("{0,-16}", "(no name"));
                else
                    sb.Append(String.Format("{0,-16}", mObjects[i].Name));

                sb.Append(String.Format("{0,-16} {1,-4} {2,-70}\n", mObjects[i].GUID, mObjects[i].Type, mObjects[i].coord));
            }
            return sb.ToString();
        }
    }

    public class Object
    {
        public string Name;
        public string SubName;
        public WoWGuid GUID;
        public byte Type;
        public UInt32 SubType;

        public UInt32 Rank;
        public UInt32 Family;

        public Coordinate coord;

        public UInt32 Race;
        public UInt32 Gender;
        public UInt32 Level;
        public UInt32[] Fields;

        public float walkSpeed, runSpeed, backWalkSpeed, swimSpeed, backSwimSpeed, turnRate;

        public void SetCoordinates(Coordinate l)
        {
            coord = l;
        }

        public Coordinate GetCoordinates()
        {
            return coord;
        }

        public float GetPositionX()
        {
            return coord.X;
        }

        public void SetPositionX(float x)
        {
            coord.X = x;
        }

        public float GetPositionY()
        {
            return coord.Y;
        }

        public void SetPositionY(float y)
        {
            coord.Y = y;
        }

        public float GetPositionZ()
        {
            return coord.Z;
        }

        public void SetPositionZ(float z)
        {
            coord.Z = z;
        }

        public float GetOrientation()
        {
            return coord.O;
        }

        public void SetOrientation(float o)
        {
            coord.O = o;
        }

        public float CalculateAngle(float x1, float y1)
        {
            return CalculateAngle(this.coord.X, this.coord.Y, x1, y1);
        }

        // Credit to ascent - I'm lazy :P
        public float CalculateAngle( float x1, float y1, float x2, float y2 )
        {
	        float dx = x2 - x1;
	        float dy = y2 - y1;
	        double angle = 0.0f;

	        // Calculate angle
	        if (dx == 0.0)
	        {
		        if (dy == 0.0)
			        angle = 0.0;
		        else if (dy > 0.0)
			        angle = Math.PI * 0.5;
		        else
			        angle = Math.PI * 3.0 * 0.5;
	        }
	        else if (dy == 0.0)
	        {
		        if (dx > 0.0)
			        angle = 0.0;
		        else
			        angle = Math.PI;
	        }
	        else
	        {
		        if (dx < 0.0)
			        angle = Math.Atan(dy / dx) + Math.PI;
		        else if (dy < 0.0)
			        angle = Math.Atan(dy / dx) + (2 * Math.PI);
		        else
			        angle = Math.Atan(dy / dx);
	        }

	        return (float)angle;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(String.Format("Name: {0}\n", Name));
            sb.Append(String.Format("SubName: {0}\n", SubName));
            sb.Append(String.Format("GUID: {0}\n", GUID));
            sb.Append(String.Format("Type: {0}\n", Type));
            sb.Append(String.Format("SubType: {0}\n", SubType));
            sb.Append(String.Format("Rank: {0}\n", Rank));
            sb.Append(String.Format("Family: {0}\n", Family));
            sb.Append(String.Format("Coord: {0}\n", coord));
            sb.Append(String.Format("Race: {0}\n", Race));
            sb.Append(String.Format("Gender: {0}\n", Gender));
            sb.Append(String.Format("Level: {0}\n", Level));
            sb.Append(String.Format("# Fields: {0}\n", Fields.Length));

            return sb.ToString();
        }
    }

    // from wowd
    public enum GAMEOBJECT_TYPES
    {
        GAMEOBJECT_TYPE_DOOR = 0,
        GAMEOBJECT_TYPE_BUTTON = 1,
        GAMEOBJECT_TYPE_QUESTGIVER = 2,
        GAMEOBJECT_TYPE_CHEST = 3,
        GAMEOBJECT_TYPE_BINDER = 4,
        GAMEOBJECT_TYPE_GENERIC = 5,
        GAMEOBJECT_TYPE_TRAP = 6,
        GAMEOBJECT_TYPE_CHAIR = 7,
        GAMEOBJECT_TYPE_SPELL_FOCUS = 8,
        GAMEOBJECT_TYPE_TEXT = 9,
        GAMEOBJECT_TYPE_GOOBER = 10,
        GAMEOBJECT_TYPE_TRANSPORT = 11,
        GAMEOBJECT_TYPE_AREADAMAGE = 12,
        GAMEOBJECT_TYPE_CAMERA = 13,
        GAMEOBJECT_TYPE_MAP_OBJECT = 14,
        GAMEOBJECT_TYPE_MO_TRANSPORT = 15,
        GAMEOBJECT_TYPE_DUEL_ARBITER = 16,
        GAMEOBJECT_TYPE_FISHINGNODE = 17,
        GAMEOBJECT_TYPE_RITUAL = 18,
        GAMEOBJECT_TYPE_MAILBOX = 19,
        GAMEOBJECT_TYPE_AUCTIONHOUSE = 20,
        GAMEOBJECT_TYPE_GUARDPOST = 21,
        GAMEOBJECT_TYPE_SPELLCASTER = 22,
        GAMEOBJECT_TYPE_MEETINGSTONE = 23,
        GAMEOBJECT_TYPE_FLAGSTAND = 24,
        GAMEOBJECT_TYPE_FISHINGHOLE = 25,
        GAMEOBJECT_TYPE_FLAGDROP = 26,
    }
}