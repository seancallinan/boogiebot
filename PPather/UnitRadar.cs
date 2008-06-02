using System;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Text;

using Pather;
using Pather.Graph;
using BoogieBot.Common;
using WowTriangles;

/*
 * UnitRadar keeps track of all units we can see
 * 
 * It keeps track of their movement speed and implements
 * the ILocationHeuristict for the PathGraph to avoid 
 * running into hostile monsters
 * 
 */

namespace Pather
{
    public class UnitRadar : ILocationHeuristics
    {
        class UnitData
        {
            public long guid;
            public BoogieBot.Common.Object unit;
            public Coordinate oldLocation;
            public double movementSpeed;

            public UnitData(BoogieBot.Common.Object u)
            {
                unit = u;
                guid = (long)u.GUID.GetOldGuid();
                movementSpeed = 0.0;
                oldLocation = u.coord;
            }
            public void Update(int dt)  // dt is milliseconds
            {
                if (dt == 0) return;
                double ds = (double)dt / 1000.0;
                double d = oldLocation.DistanceTo(unit.Location);
                movementSpeed = d / ds;
                oldLocation = unit.Location;
            }
        }

        Dictionary<long, UnitData> dic = new Dictionary<long, UnitData>();
        GSpellTimer updateTimer = new GSpellTimer(0);

        public UnitRadar()
        {
        }


        public float Score(float x, float y, float z)
        {
            Coordinate l = new Coordinate(x, y, z);
            Coordinate me = BoogieCore.World.getPlayerObject().GetCoordinates();
            if (l.DistanceTo(me) > 100.0) return 0;
            float s = 0;
            foreach (UnitData ud in dic.Values)
            {
                BoogieBot.Common.Object unit = ud.unit;
                if (unit.Reaction >= 2)
                {

                    float d = unit.coord.DistanceTo(l);
                    if (d < 30)
                    {
                        float n = 30 - d;
                        uint ld = unit.Level - BoogieCore.world.getPlayerObject().Level;
                        //if(ld < 0)
                        //    n /= -ld+2;
                        s += n;
                    }
                }
            }
            //if(s>0)
            //    GContext.Main.Log("  " + l + " score " + s);
            return s;
        }

        public void Update()
        {
            int dt = (int)-updateTimer.TicksLeft;
            if (dt >= 1000)
            {
                BoogieBot.Common.Object[] units = BoogieCore.world.getObjectListArray();
                Update(units);
            }
        }

        public void Update(BoogieBot.Common.Object[] units)
        {
            int dt = (int)-updateTimer.TicksLeft;
            foreach (BoogieBot.Common.Object u in units)
            {
                if (!u.IsDead && !PPather.IsStupidItem(u))
                {
                    UnitData ud;
                    if (dic.TryGetValue((long)u.GUID.GetOldGuid(), out ud))
                    {
                        ud.Update((int)dt);
                    }
                    else
                    {
                        // new one
                        ud = new UnitData(u);
                        dic.Add((long)u.GUID.GetOldGuid(), ud);
                    }
                }
            }

            List<long> rem = new List<long>();
            foreach (UnitData ud in dic.Values)
            {
                if (!ud.unit.IsValid) rem.Add(ud.guid);
            }
            foreach (long guid in rem)
            {
                dic.Remove(guid);
            }
            updateTimer.Reset();
        }

        public double GetSpeed(BoogieBot.Common.Object u)
        {
            UnitData ud;
            if (dic.TryGetValue((long)u.GUID.GetOldGuid(), out ud))
            {
                return ud.movementSpeed;
            }
            return 0.0;
        }
    }

}