
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using System.Runtime.InteropServices;
using System.Resources;

using Pather;

using Pather.Graph;
using BoogieBot.Common;
using WowTriangles;

namespace Pather
{

    /////////////////////////////////////////////////////////////////
    // START PPather

    /*
        Creds go to:
          shammer    - Taxi, BGQueue and some zone tricks
          danbopes   - Macroless zone detection and various UI interaction code
          persist    - Tireless testing and many good ideas for features
          GliderFlix  - Class skill Training
          orca       - ForceSell in the Vendor task
          

     */

    public class GSpellTimer
    {
        private uint dur = 0;
        private uint lastCheck = 0;
        private uint now;
        private bool ready = false;

        [DllImport("winmm.dll", EntryPoint = "timeGetTime")]
        private static extern uint MM_GetTime();


        public GSpellTimer(Int32 duration, bool bleh)
        {
            lastCheck = MM_GetTime();
            dur = (uint)duration;
        }

        public GSpellTimer(Int32 duration)
        {
            lastCheck = MM_GetTime();
            dur = (uint)duration;
        }

        public bool IsReady
        {
            get {
                if (lastCheck == 0)
                    lastCheck = MM_GetTime();
                now = MM_GetTime();
                if (now - lastCheck >= dur)
                    return true;

                return false;
            }
            
        }
        public bool IsReadySlow
        {
            get {
                if (lastCheck == 0)
                    lastCheck = MM_GetTime();
                now = MM_GetTime();

                uint diff = now - lastCheck;
                if (diff >= dur)
                {
                    Thread.Sleep(51);
                    return true;
                }

                Thread.Sleep(51);
                return false;
            }
        }

        public void Reset()
        {
            lastCheck = 0;
        }

        public void Wait()
        {
        }
        public void ForceReady()
        {
        }

        public uint TicksLeft
        {
            get {
                if (lastCheck == 0)
                    lastCheck = MM_GetTime();
                now = MM_GetTime();

                uint diff = now - lastCheck;
                return diff;
            }
        }
    }

    public /*abstract */ class PPather
    {

        /*
          WTB: 

        */

        public const double PI = Math.PI;
        public static Random random = new Random();
        public static Mover mover;
        public static UnitRadar radar;
        public static CultureInfo numberFormat = System.Globalization.CultureInfo.InvariantCulture;
        private static Player Me = BoogieCore.Player;


        // settings

        // state


        public string CurrentContinent = null;
        public PathGraph world = null;
        Spot WasAt = null;

        public enum RunState_e { Stopped, Paused, Running };
        public RunState_e RunState = RunState_e.Stopped;
        public RunState_e WantedState = RunState_e.Stopped;

        Thread glideThread = null;
        private bool ShouldRun = true;

        //AdvancedMap.Map newMap;


		// subclass constructors must call this constructor via base()
		public PPather()
			: base() {
			//RegisterTasks();
		}







        /*
          Macro: 

13:43 Quest accepted: Galgar's Cactus Apple Surprise
13:44 Quest accepted: Sting of the Scorpid
13:44 Quest accepted: Vile Familiars
13:48 Galgar's Cactus Apple Surprise completed.

13:49 Lazy Peons failed: Inventory is full.

13:50 Quest accepted: Lazy Peons

14:00 Vile Familiars failed: Inventory is full.

14:00 Quest accepted: Burning Blade Medallion

/script ChatFrame1:AddMessage("Zone#" .. GetSubZoneText() .. ":" .. GetZoneText() .."#");
/script ChatFrame1:AddMessage ("Quest#" .. GetTitleText().."#");
*/

        void Pather_CombatLog(string rawText)
        {
            if (rawText == null) return;
            if (rawText.StartsWith("You have slain"))
            {
                int start = rawText.IndexOf("slain") + 6;
                int end = rawText.IndexOf("!");
                String mob = rawText.Substring(start, end - start);
                //Context.Log("Killed monster '" + mob + "'");
            }

        }

        private string FigureOutZone()
        {
            // danbopes supplies these 2 lines to remove the macro
            //GInterfaceObject ZoneTextString = GContext.Main.Interface.GetByName("ZoneTextFrame").GetChildObject("ZoneTextString");
            //GInterfaceObject SubZoneTextString = GContext.Main.Interface.GetByName("SubZoneTextFrame").GetChildObject("SubZoneTextString");
            return "Dalaran:Alterac Mountains"; // FIXME SubZoneTextString.LabelText + ":" + ZoneTextString.LabelText;
        }


        public void OnStartGlide()
        {

            glideThread = new Thread(new ThreadStart(this.Startup));
            glideThread.IsBackground = true;
            glideThread.Start();

            

        }

        private void SaveAllState()
        {
            if (world != null)
            {
                world.Save();
            }

        }

        public void OnStopGlide()
        {
            BoogieCore.Log(LogType.System, "Inside OnStopGlider()");


            WantedState = RunState_e.Stopped;
            RunState = RunState_e.Stopped;

            SaveAllState();


            if (world != null)
                world.Close();



            BoogieCore.Log(LogType.System, "TotalMemory " + System.GC.GetTotalMemory(false) / (1024 * 1024) + " MB");
            world = null; // release RAM

            BoogieCore.Log(LogType.System, "TotalMemory after GC " + System.GC.GetTotalMemory(true) / (1024 * 1024) + " MB");
            
        }


        private void Startup()
        {
            RunState = RunState_e.Paused;
            WantedState = RunState_e.Running;

            BoogieCore.Log(LogType.System, "TotalMemory before " + System.GC.GetTotalMemory(true) / (1024 * 1024) + " MB");

            WasAt = null;

            string zone = FigureOutZone();

            MPQTriangleSupplier mpq = new MPQTriangleSupplier();
            CurrentContinent = mpq.SetZone(zone);

            BoogieCore.Log(LogType.System, "Zone is : " + zone);
            BoogieCore.Log(LogType.System, "Continent is : " + CurrentContinent);


            string myFaction = "Unknown";


            ChunkedTriangleCollection triangleWorld = new ChunkedTriangleCollection(512);
            triangleWorld.SetMaxCached(9);
            triangleWorld.AddSupplier(mpq);

            world = new PathGraph(CurrentContinent, triangleWorld, null);
            mover = new Mover();
            radar = new UnitRadar();
            BoogieCore.Log(LogType.System, "Pather loaded!");

            while (ShouldRun)
            {
                Thread.Sleep(100);
            }
        }

        public void Shutdown()
        {
        }




        public void Patrol()
        {
            OnStartGlide();



            /*if (!Me.IsInCombat && !Me.IsDead)
                Rest();
            while (true)
            {*/
                MyPather();
                Thread.Sleep(1000);
            //}
        }

        private void UpdateXP()
        {

        }

        private GSpellTimer ChunkLoadT = new GSpellTimer(5000, true);

        public void ResetMyPos()
        {
            WasAt = null;
        }

        public void UpdateMyPos()
        {
            BoogieCore.Log(LogType.System, "[Update] 1");
            //radar.Update();
            if (world != null)
            {
                Coordinate loc = BoogieCore.world.getPlayerObject().GetCoordinates();
                Location isAt = new Location(loc.X, loc.Y, loc.Z);
                BoogieCore.Log(LogType.System, "[Update] 2");
                //if(WasAt != null)  Context.Log("was " + WasAt.location);
                //Context.Log("isAt " + isAt); 
                if (WasAt != null)
                {
                    BoogieCore.Log(LogType.System, "[Update] 3");
                    if (WasAt.GetLocation().GetDistanceTo(isAt) > 20)
                        WasAt = null;
                }
                BoogieCore.Log(LogType.System, "[Update] 4");
                WasAt = world.TryAddSpot(WasAt, isAt);
                BoogieCore.Log(LogType.System, "[Update] 5");
            }
        }







        public Coordinate PredictedLocation(BoogieBot.Common.Object mob)
        {
            Coordinate currentLocation = mob.GetCoordinates();
            double x = currentLocation.X;
            double y = currentLocation.Y;
            double z = currentLocation.Z;
            double heading = mob.GetOrientation();
            double dist = 4;

            x += Math.Cos(heading) * dist;
            y += Math.Sin(heading) * dist;

            Coordinate predictedLocation = new Coordinate((float)x, (float)y, (float)z);

            Coordinate closestLocatition = currentLocation;
            if (predictedLocation.DistanceTo(BoogieCore.world.getPlayerObject().GetCoordinates()) < closestLocatition.DistanceTo(BoogieCore.world.getPlayerObject().GetCoordinates()))
                closestLocatition = predictedLocation;
            return closestLocatition;
        }


        public static bool IsStupidItem(BoogieBot.Common.Object unit)
        {
            //if (unit.CreatureType == GCreatureType.Totem) return true;
            // Filter out all stupid sting found in outland
            string name = unit.Name.ToLower();
            if (name.Contains("target") || name.Contains("trigger") ||
                name.Contains("flak cannon") || name.Contains("trip wire") ||
                name.Contains("infernal rain") || name.Contains("anilia") ||
                name.Contains("teleporter credit") || name.Contains("door fel cannon") ||
                name.Contains("ethereum glaive") || name.Contains("orb flight"))
                return true;
            return false;
        }

        public bool IsItSafeAt(BoogieBot.Common.Object ignore, BoogieBot.Common.Object u)
        {
            return IsItSafeAt(ignore, u.GetCoordinates());
        }

        public bool IsItSafeAt(BoogieBot.Common.Object ignore, Location l)
        {
            return IsItSafeAt(ignore, new Coordinate(l.X, l.Y, l.Z));
        }

        public bool IsItSafeAt(BoogieBot.Common.Object ignore, Coordinate l)
        {
            return true;

        }

        public BoogieBot.Common.Object GetClosestPvPPlayer()
        {
   
            return null; // fixme
        }

        public BoogieBot.Common.Object GetClosestPvPPlayerAttackingMe()
        {

            return null; //fixme
        }

        public BoogieBot.Common.Object GetClosestFriendlyPlayer()
        {

            return null; //fixme
        }
        public BoogieBot.Common.Object FindAttacker()
        {
            return null;
        }

        public bool Face(BoogieBot.Common.Object monster)
        {
            return Face(monster, PI / 8);
        }


        public bool Face(BoogieBot.Common.Object monster, double tolerance)
        {
            BoogieBot.Common.Object player = BoogieCore.world.getPlayerObject();
            player.SetOrientation(player.CalculateAngle(monster.GetPositionX(), monster.GetPositionY()));
            return true;

        }
        

        public static bool IsHordePlayerFaction(BoogieBot.Common.Object u)
        {
            int f = 0;// u.FactionID;
            if (f == 2 ||
                f == 5 ||
                f == 6 ||
                f == 116 ||
                f == 1610)
                return true;
            return false;
        }

        public static bool IsAlliancePlayerFaction(BoogieBot.Common.Object u)
        {
            int f = 0;//u.FactionID;
            if (f == 1 ||
                f == 3 ||
                f == 4 ||
                f == 115 ||
                f == 1629)
                return true;

            return false;
        }

        public static bool IsPlayerFaction(BoogieBot.Common.Object u)
        {
            return IsHordePlayerFaction(u) || IsAlliancePlayerFaction(u);
        }


        public bool MoveToGetThemInFront(BoogieBot.Common.Object Target, BoogieBot.Common.Object Add)
        {
            double bearing = Add.GetOrientation();
            if (!IsInFrontOfMe(Add))
            {
                BoogieCore.Log(LogType.System, "Got the add " + Add.Name + " behind me");
                /*
                  hmm, just back up? or turn a bit too?
                */

                mover.Backwards(true);
                if (bearing < 0)
                {
                    BoogieCore.Log(LogType.System, "  back up left");
                    mover.RotateLeft(true);
                }
                else
                {
                    BoogieCore.Log(LogType.System, "  back up right");
                    mover.RotateRight(true);
                }
                Thread.Sleep(300);
                mover.RotateLeft(false);
                mover.RotateRight(false);
                Thread.Sleep(300);
                mover.Backwards(false);
                return true;
            }
            return false;
        }

        public void TweakMelee(BoogieBot.Common.Object Monster)
        {
            double Distance = Monster.GetCoordinates().DistanceTo(BoogieCore.world.getPlayerObject().GetCoordinates());
            double sensitivity = 2.5; // default melee distance is 4.8 - 2.5 = 2.3, no monster will chase us at 2.3
            double min = 4.5 - sensitivity;
            if (min < 1.0) min = 1.0;

            if (Distance > 4.5)
            {
                // Too far
                //Spam("Tweak forwards. "+ Distance + " > " + Context.MeleeDistance);
                mover.Forwards(true);
                Thread.Sleep(100);
                mover.Forwards(false);
            }
            else if (Distance < min)
            {
                // Too close
                //Spam("Tweak backwards. "+ Distance + " < " + min);
                mover.Backwards(true);
                Thread.Sleep(200);
                mover.Backwards(false);
            }
        }

        /*
         */
        // return value from -PI to PI
        // 
        double BearingToMe(BoogieBot.Common.Object unit)
        {
            Coordinate MyLocation = BoogieCore.world.getPlayerObject().GetCoordinates();
            float bearing = 0;// fixme (float)unit.GetHeadingDelta(MyLocation);
            return bearing;
        }

        public bool IsInFrontOfMe(BoogieBot.Common.Object unit)
        {
            double bearing = unit.GetOrientation();
            return bearing < PI / 2.0 && bearing > -PI / 2.0;
        }



        public bool Approach(BoogieBot.Common.Object monster, bool AbortIfUnsafe)
        {
            return Approach(monster, AbortIfUnsafe, 10000);
        }

        public bool Approach(BoogieBot.Common.Object monster, bool AbortIfUnsafe, int timeout)
        {
            
            BoogieCore.Log(LogType.System, "[Approach] 1");
            Coordinate loc = monster.GetCoordinates();
            float DistTo = loc.DistanceTo(BoogieCore.world.getPlayerObject().GetCoordinates());
            BoogieCore.Log(LogType.System, "[Approach] Distance to object: {0}", DistTo);
            if (DistTo < 4.5f &&
                Math.Abs(loc.O) < PI / 8)
            {
                mover.Stop();
                return true;
            }

            GSpellTimer approachTimeout = new GSpellTimer(timeout, false);
            StuckDetecter sd = new StuckDetecter(this, 1, 2);
            GSpellTimer t = new GSpellTimer(0);
            bool doJump = random.Next(4) == 0;
            EasyMover em = null;
            GSpellTimer NewTargetUpdate = new GSpellTimer(1000);


            BoogieCore.Log(LogType.System, "[Approach] 2");
            do
            {
                BoogieCore.Log(LogType.System, "[Approach] 3");
                UpdateMyPos();
                BoogieCore.Log(LogType.System, "[Approach] 4");
                // Check for stuck
                if (sd.checkStuck())
                {
                    BoogieCore.Log(LogType.System, "[Approach] 5");
                    BoogieCore.Log(LogType.System, "Major stuck on approach. Giving up");
                    mover.Stop();
                    return false;
                }
                double distance = monster.GetCoordinates().DistanceTo(BoogieCore.world.getPlayerObject().GetCoordinates());
                BoogieCore.Log(LogType.System, "[Approach] 6 - Dist = {0}", distance);
                bool moved;
               
                if (distance < 8)
                {
                    loc = monster.GetCoordinates();
                    BoogieCore.Log(LogType.System, "[Approach] 7");
                    moved = mover.moveTowardsFacing(loc, 4.5f, loc);
                    BoogieCore.Log(LogType.System, "[Approach] 8 {0}", moved);
                }
                else
                {
                    BoogieCore.Log(LogType.System, "[Approach] 9");
                    if (em == null)
                    {
                        loc = monster.GetCoordinates();
                        em = new EasyMover(this, new Location(loc), false, AbortIfUnsafe);
                    }
                    BoogieCore.Log(LogType.System, "[Approach] 10");
                    EasyMover.MoveResult mr = em.move();
                    BoogieCore.Log(LogType.System, "[Approach] 11 {0}", mr);
                    moved = true;
                    if (mr != EasyMover.MoveResult.Moving)
                    {
                        moved = false;
                    }
                    BoogieCore.Log(LogType.System, "[Approach] 12");
                }
                BoogieCore.Log(LogType.System, "[Approach] 13");

                if (!moved)
                {
                    mover.Stop();
                    return true;
                }

            } while (!approachTimeout.IsReadySlow);
            mover.Stop();
            BoogieCore.Log(LogType.System, "Approach timed out");
            return false;
        }

        public bool WalkTo(Coordinate loc, bool AbortIfUnsafe, int timeout, bool AllowDead)
        {
            if (loc.DistanceTo(BoogieCore.world.getPlayerObject().GetCoordinates()) < 4.5f &&
                Math.Abs(loc.O) < PI / 8)
            {
                mover.Stop();
                return true;
            }

            GSpellTimer approachTimeout = new GSpellTimer(timeout, false);
            StuckDetecter sd = new StuckDetecter(this, 1, 2);
            GSpellTimer t = new GSpellTimer(0);
            bool doJump = random.Next(4) == 0;
            EasyMover em = null;
            do
            {
                UpdateMyPos();

                // Check for stuck
                if (sd.checkStuck())
                {
                    BoogieCore.Log(LogType.System, "Major stuck on approach. Giving up");
                    mover.Stop();
                    return false;
                }


                double distance = loc.DistanceTo(BoogieCore.world.getPlayerObject().GetCoordinates());
                bool moved;
                if (distance < 8)
                {
                    moved = mover.moveTowardsFacing(loc, 4.5f, loc);
                }
                else
                {
                    if (em == null)
                        em = new EasyMover(this, new Location(loc), false, AbortIfUnsafe);
                    EasyMover.MoveResult mr = em.move();

                    moved = true;
                    if (mr != EasyMover.MoveResult.Moving)
                    {
                        moved = false;
                    }
                }

                if (!moved)
                {
                    mover.Stop();
                    BoogieCore.Log(LogType.System, "did not move");
                    return true;
                }

            } while (!approachTimeout.IsReadySlow );
            mover.Stop();
            BoogieCore.Log(LogType.System, "Approach timed out");
            return false;
        }



        public Coordinate InFrontOf(BoogieBot.Common.Object unit, double d)
        {
            double x = unit.GetCoordinates().X;
            double y = unit.GetCoordinates().Y;
            double z = unit.GetCoordinates().Z;
            double heading = unit.GetOrientation();
            x += Math.Cos(heading) * d;
            y += Math.Sin(heading) * d;

            return new Coordinate((float)x, (float)y, (float)z);
        }

        public Coordinate InFrontOf(Coordinate loc, double heading, double d)
        {
            double x = loc.X;
            double y = loc.Y;
            double z = loc.Z;

            x += Math.Cos(heading) * d;
            y += Math.Sin(heading) * d;

            return new Coordinate((float)x, (float)y, (float)z);
        }


        private void DoPatrolerLearnTravelMode()
        {

        }


        // called when we have died, return when we are alive again
        private void GhostRun()
        {
            BoogieCore.Log(LogType.System, "I died. Let's resurrect");
            Coordinate CorpseLocation = null;
 
            

                Coordinate gloc = new Coordinate(0, 0, 0);
                if (CorpseLocation != null) gloc = CorpseLocation;

                Location target = null;
                Coordinate gtarget;
                BoogieCore.Log(LogType.System, "Corpse is at " + gloc);

                if (gloc.Z == 0)
                {
                    BoogieCore.Log(LogType.System, "hmm, corpse Z == 0");
                    target = new Location(gloc);
                    for (int q = 0; q < 50; q += 5)
                    {
                        float stand_z = 0;
                        int flags = 0;
                        float x = gloc.X + random.Next(20) - 10;
                        float y = gloc.Y + random.Next(20) - 10;
                        bool ok = world.triangleWorld.FindStandableAt(x, y,
                                                                      -5000,
                                                                      5000,
                                                                      out stand_z, out flags, 0, 0);
                        if (ok)
                        {
                            target = new Location(x, y, stand_z);
                            break;
                        }
                    }
                }
                else
                {
                    target = new Location(gloc);

                }
                gtarget = new Coordinate(target.X, target.Y, target.Z);

                BoogieCore.Log(LogType.System, "Corpse is at " + target);
                EasyMover em = new EasyMover(this, target, false, false);

                // 2. Run to corpse
                while (Me.IsDead && Me.DistanceTo(gloc) > 20) // fixme
                {
                    EasyMover.MoveResult mr = em.move();
                    if (mr != EasyMover.MoveResult.Moving) return; // buhu
                    UpdateMyPos();
                    Thread.Sleep(50);

                }
                mover.Stop();

                // 3. Find a safe place to res
                // is within 20 yds of corpse now, dialog must be up
                float SafeDistance = 25.0f;
                while (true)
                {
                    // some brute force :p
                    BoogieBot.Common.Object[] monsters = BoogieCore.world.getObjectListArray();
                    float best_score = 1E30f;
                    float best_distance = 1E30f;
                    Location best_loc = null;
                    for (float x = -35; x <= 35; x += 5)
                    {
                        for (float y = -35; y <= 35; y += 5)
                        {
                            float rx = target.X + x;
                            float ry = target.Y + y;
                            Coordinate xxx = new Coordinate(rx, ry, 0);
                            if (xxx.DistanceTo(gtarget) < 35)
                            {
                                float stand_z = 0;
                                int flags = 0;
                                bool ok = world.triangleWorld.FindStandableAt(rx, ry,
                                                                              target.Z - 20,
                                                                              target.Z + 20,
                                                                              out stand_z, out flags, 0, 0);
                                if (ok)
                                {
                                    float score = 0.0f;
                                    Coordinate l = new Coordinate(rx, ry, stand_z);
                                    foreach (BoogieBot.Common.Object monster in monsters)
                                    {
                                        if (monster != null && !monster.IsDead)
                                        {
                                            float d = l.DistanceTo(monster.GetCoordinates());
                                            if (d < 35)
                                            {
                                                // one point per yard 
                                                score += 35 - d;
                                            }
                                        }
                                    }
                                    float this_d = Me.DistanceTo(l);
                                    if (score <= best_score && this_d < best_distance)
                                    {
                                        best_score = score;
                                        best_distance = this_d;
                                        best_loc = new Location(l);
                                    }
                                }
                            }
                        }
                    }
                    if (best_loc != null)
                    {
                        Coordinate best_gloc = new Coordinate(best_loc.X, best_loc.Y, best_loc.Z);
                        // walk over there
                        WalkTo(best_gloc, false, 10000, true);

                        // Check if I am safe
                        bool safe = true;
                        BoogieBot.Common.Object unsafe_monster = null;
                        foreach (BoogieBot.Common.Object monster in monsters)
                        {
                            if (!monster.IsDead && !PPather.IsStupidItem(monster))
                            {
                                float d = Me.DistanceTo(monster.GetCoordinates());
                                if (d < SafeDistance)
                                    if (Math.Abs(monster.GetPositionZ() - Me.Location.Z) < 15)
                                    {
                                        safe = false;
                                        unsafe_monster = monster;
                                    }

                            }
                        }
                        if (safe)
                        {
                            break; // yeah
                        }

                    }

                    // hmm, look again
                    Thread.Sleep(2000);
                    SafeDistance -= 0.5f;

            }
        }



        private void MyPather()
        {





            GSpellTimer taskTimer = new GSpellTimer(300);
            GSpellTimer updateStatusTimer = new GSpellTimer(2000);
            GSpellTimer nothingToDoTimer = new GSpellTimer(3 * 1000);
            bool exit = false;
            GSpellTimer Tick = new GSpellTimer(100);
            do
            {
                if (RunState != WantedState)
                {
                    RunState = WantedState;
                }

                if (updateStatusTimer.IsReady)
                {
                    UpdateXP();
                    updateStatusTimer.Reset();
                }
                if (RunState == RunState_e.Stopped)
                {
                    //Context.Log("Stop wanted. Stopping glide");
                    //Context.KillAction("PPather wants to stop", false);
                    Thread.Sleep(500); // pause
                }
                else if (RunState == RunState_e.Paused)
                {
                    UpdateMyPos();
                    Thread.Sleep(100); // pause
                }
                else if (RunState == RunState_e.Running)
                {
                    UpdateMyPos();
                    if (Me.IsDead)
                    {
                        Thread.Sleep(1000);
                        GhostRun();
                        Thread.Sleep(1500);
                        if (Me.IsDead)
                        {
                            //!!!
                        }
                        else
                        {
                            //Rest();
                        }
                    }

                    Tick.Wait();
                    Thread.Sleep(10); // min sleep time
                    Tick.Reset();
                }
            } while (!exit);
        }
    }


    

    // END PPather
    ///////////////////////////////////////////////////////
}


