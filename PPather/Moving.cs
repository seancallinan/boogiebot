
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Text;

using Pather;
using Pather.Graph;
using BoogieBot.Common;

/*
 *  Classes to move the toon around 
 * 
 * Mover is a raw mover controlling what keys are pushed to move around
 * 
 * EasyMover easier access to MoveAlonger
 * 
 * MoveAlonger has the main logic to run along a computed path
 *
 */

namespace Pather
{ 
    public class Mover
    {

        private bool runForwards = false;
        private bool runBackwards = false;
        private bool strafeLeft = false;
        private bool strafeRight = false;
        private bool rotateLeft = false;
        private bool rotateRight = false;

        private const float PI = (float)Math.PI;

        public Mover(/*GContext Context*/)
        {
            //this.Context = Context;
        }

        private GSpellTimer KeyT = new GSpellTimer(50);
        private bool old_runForwards = false;
        private bool old_runBackwards = false;
        private bool old_strafeLeft = false;
        private bool old_strafeRight = false;
        private bool old_rotateLeft = false;
        private bool old_rotateRight = false;


        void PushKeys()
        {

            if (old_runForwards != runForwards)
            {
                KeyT.Wait();
                KeyT.Reset();
                if (runForwards)
                    PressKey("Common.Forward");
                else
                    ReleaseKey("Common.Forward");
                //Context.Log("Forwards: " + runForwards);
            }
            /*

                            if(runForwards)
                            {
                                GContext.Main.StartRun(); // 
                            }
                            else
                            {
                                GContext.Main.ReleaseRun(); // 
                            }
            */
            if (old_runBackwards != runBackwards)
            {
                KeyT.Wait();
                KeyT.Reset();
                if (runBackwards)
                    PressKey("Common.Back");
                else
                    ReleaseKey("Common.Back");
                //Context.Log("Backwards: " + runBackwards);
            }
            if (old_strafeLeft != strafeLeft)
            {
                KeyT.Wait();
                KeyT.Reset();
                if (strafeLeft)
                    PressKey("Common.StrafeLeft");
                else
                    ReleaseKey("Common.StrafeLeft");
                //Context.Log("StrageLeft: " + strafeLeft);
            }
            if (old_strafeRight != strafeRight)
            {
                KeyT.Wait();
                KeyT.Reset();
                if (strafeRight)
                    PressKey("Common.StrafeRight");
                else
                    ReleaseKey("Common.StrafeRight");
                //Context.Log("StrageRight: " + strafeRight);
            }

            /*
                            if(rotateRight || rotateLeft)
                            {
                                double head = GContext.Main.Me.Heading;
                                if(rotateRight) head -= Math.PI/2;
                                else if(rotateLeft)  head += Math.PI/2;
                                GContext.Main.StartSpinTowards(head);
                            }
                            else
                            {
                                GContext.Main.ReleaseSpin();
                            }
            */

            if (old_rotateLeft != rotateLeft)
            {
                KeyT.Wait();
                KeyT.Reset();
                if (rotateLeft)
                    PressKey("Common.RotateLeft");
                else
                    ReleaseKey("Common.RotateLeft");
                //Context.Log("RotateLeft: " + rotateLeft);
            }
            if (old_rotateRight != rotateRight)
            {
                KeyT.Wait();
                KeyT.Reset();
                if (rotateRight)
                    PressKey("Common.RotateRight");
                else
                    ReleaseKey("Common.RotateRight");
                //Context.Log("RotateRight: " + rotateRight);
            }

            old_runForwards = runForwards;
            old_runBackwards = runBackwards;
            old_strafeLeft = strafeLeft;
            old_strafeRight = strafeRight;
            old_rotateLeft = rotateLeft;
            old_rotateRight = rotateRight;
        }

        void PressKey(string name)
        {
            //Context.Log("Press : " + name);
            //Context.PressKey(name); // fixme
        }

        void ReleaseKey(string name) // fixme
        {
            //Context.Log("Release: " + name);
            //Context.ReleaseKey(name);
        }

        void SendKey(string name) //fixme
        {
            //Context.Log("Send: " + name);
            //Context.SendKey(name);
        }


        public void Jump()
        {
            SendKey("Common.Jump");
        }

        public void SwimUp(bool go)
        {
            if (go)
                PressKey("Common.Jump");
            else
                ReleaseKey("Common.Jump");
        }

        public void ResyncKeys()
        {
            KeyT.ForceReady();
            PushKeys();
        }

        public void MoveRandom()
        {
            int d = PPather.random.Next(4);
            if (d == 0) Forwards(true);
            if (d == 1) StrafeRight(true);
            if (d == 2) Backwards(true);
            if (d == 3) StrafeLeft(true);
        }

        public void StrafeLeft(bool go)
        {
            strafeLeft = go;
            if (go) strafeRight = false;
            PushKeys();
        }

        public void StrafeRight(bool go)
        {
            strafeRight = go;
            if (go) strafeLeft = false;
            PushKeys();
        }

        public void RotateLeft(bool go)
        {
            rotateLeft = go;
            if (go) rotateRight = false;
            PushKeys();
        }


        public void RotateRight(bool go)
        {
            rotateRight = go;
            if (go) rotateLeft = false;
            PushKeys();
        }


        public void Forwards(bool go)
        {
            if (go)
                BoogieCore.WorldServerClient.StartMoveForward();
            else
                BoogieCore.WorldServerClient.StopMoveForward();
        }

        public void Backwards(bool go)
        {
            runBackwards = go;
            if (go) runForwards = false;
            PushKeys();
        }

        public void StopRotate()
        {
            BoogieCore.WorldServerClient.StopMoveForward();
        }

        public void StopMove()
        {
            BoogieCore.WorldServerClient.StopMoveForward();
        }


        public void Stop()
        {
            StopMove();
            StopRotate();
        }

        public bool IsMoving()
        {
            return runForwards || runBackwards || strafeLeft || strafeRight;
        }


        public bool IsRotating()
        {
            return rotateLeft || rotateRight;
        }

        public bool IsRotatingLeft()
        {
            return rotateLeft;
        }
        public bool IsRotatingRight()
        {
            return rotateLeft;
        }


        /*
          1 - location is front
          2 - location is right
          3 - location is back
          4 - location is left
        */
        int GetLocationDirection(Coordinate loc)
        {
            int dir = 0;
            double b = loc.O;
            if (b > -PI / 4 && b <= PI / 4)  // Front
            {
                dir = 1;
            }
            if (b > -3 * PI / 4 && b <= -PI / 4) // Left
            {
                dir = 4;
            }
            if (b <= -3 * PI / 4 || b > 3 * PI / 4) //  Back   
            {
                dir = 3;
            }
            if (b > PI / 4 && b <= 3 * PI / 4) //  Right  
            {
                dir = 2;
            }
            //if (dir == 0)
                //Context.Log("Odd, no known direction");

            return dir;
        }

        public static double GetDistance3D(Coordinate l0, Coordinate l1)
        {
            double dx = l0.X - l1.X;
            double dy = l0.Y - l1.Y;
            double dz = l0.Z - l1.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public bool moveTowardsFacing(Coordinate to, double distance, Coordinate facing)
        {


            //BoogieBot.Common.Object player = BoogieCore.world.getPlayerObject();
            //player.SetOrientation(player.CalculateAngle(to.X, to.Y));
            //return true;
            bool moving = false;
            double d = to.DistanceTo(BoogieCore.world.getPlayerObject().GetCoordinates());
            BoogieCore.Log(LogType.System, "[Move] D = {0}", d);
            if (d > distance)
            {
                int dir = GetLocationDirection(to);
                if (dir != 0) moving |= true;
                if (dir == 1 || dir == 3 || dir == 0) { StrafeLeft(false); StrafeRight(false); };
                if (dir == 2 || dir == 4 || dir == 0) { Forwards(false); Backwards(false); };
                if (dir == 1) Forwards(true);
                if (dir == 2) StrafeRight(true);
                if (dir == 3) Backwards(true);
                if (dir == 4) StrafeLeft(true);
                BoogieCore.Log(LogType.System, "[MOVE] Get Direction: {0}", dir);
                //Context.Log("Move dir: " + dir);
            }
            else
            {
                BoogieCore.Log(LogType.System, "[Move] Is close {0}", d);
                StrafeLeft(false);
                StrafeRight(false);
                Forwards(false);
                Backwards(false);
            }

            /*BoogieCore.Log(LogType.System, "[Move] 1");
            double bearing = BoogieCore.World.getPlayerObject().CalculateAngle(facing.X, facing.Y);
            if (bearing < -PI / 8)
            {
                BoogieCore.Log(LogType.System, "[Move] 2");
                moving |= true;
            }
            else if (bearing > PI / 8)
            {
                moving |= true;
            }*/

            return moving;
        }

        public double GetMoveHeading(out double speed)
        {
            double head = BoogieCore.Player.Location.O;
            double r = 0; ;
            speed = 0.0;
            if (runForwards)
            {
                speed = 7.0; r = head;
                if (strafeRight) r += PI / 2;
                if (strafeLeft) r -= PI / 2;
                if (runBackwards) speed = 0.0;
            }
            else if (runBackwards)
            {
                speed = 4.5; r = head + PI;
                if (strafeRight) r -= PI / 2;
                if (strafeLeft) r += PI / 2;
                if (runBackwards) speed = 0.0;
            }
            else if (strafeLeft)
            {
                speed = 7.0; r = head + PI * 3.0 / 4.0;
                if (strafeRight) speed = 0;
            }
            else if (strafeRight)
            {
                speed = 7.0; r = head + PI / 4;
            }

            if (head >= 2 * PI) head -= 2 * PI;
            return head;
        }
    }

    public class StuckDetecter
    {
        Coordinate oldLocation = null;
        float predictedDX;
        float predictedDY;

        GSpellTimer StuckTimeout = new GSpellTimer(333); // Check every 333ms
        Player Me;
        Mover mover;
        int stuckSensitivity;
        int abortSensitivity;
        int stuckMove = 0;
        GSpellTimer lastStuckCheck = new GSpellTimer(0);
        bool firstStuckCheck = true;
        //Coordinate StuckLocation = null;

        public StuckDetecter(PPather pather, int stuckSensitivity, int abortSensitivity)
        {
            this.Me = BoogieCore.Player;
            this.stuckSensitivity = stuckSensitivity;
            this.abortSensitivity = abortSensitivity;
            this.mover = PPather.mover;
            firstStuckCheck = true;
        }

        public bool checkStuck()
        {
            if (firstStuckCheck)
            {
                oldLocation = Me.Location;
                predictedDX = 0;
                predictedDY = 0;
                firstStuckCheck = false;
                lastStuckCheck.Reset();
            }
            else
            {
                // update predicted location
                double h; double speed;
                h = mover.GetMoveHeading(out speed);

                float dt = (float)-lastStuckCheck.TicksLeft / 1000f;
                float dx = (float)Math.Cos(h) * (float)speed * dt;
                float dy = (float)Math.Sin(h) * (float)speed * dt;
                //GContext.Main.Log("speed: " + speed + " dt: " + dt + " dx: " + dx + " dy : " + dy);
                predictedDX += dx;
                predictedDY += dy;

                lastStuckCheck.Reset();
                if (StuckTimeout.IsReady)
                {
                    // Check stuck
                    Coordinate loc = Me.Location;
                    float realDX = loc.X - oldLocation.X;
                    float realDY = loc.Y - oldLocation.Y;
                    //GContext.Main.Log(" dx: " + predictedDX + " dy : " + predictedDY + " Real dx: " + realDX + " dy : " + realDY );

                    float predictDist = (float)Math.Sqrt(predictedDX * predictedDX + predictedDY * predictedDY);
                    float realDist = (float)Math.Sqrt(realDX * realDX + realDY * realDY);

                    //GContext.Main.Log(" pd " + predictDist + " rd " + realDist);


                    if (predictDist > realDist * 3)
                    {
                        // moving a lot slower than predicted
                        // check direction
                        Coordinate excpected = new Coordinate(loc.X + predictedDX, loc.Y + predictedDY, loc.Z);

                        //Context.Log("I am stuck " + stuckMove); //. Jumping to get free");
                        if (stuckMove == 0)
                        {
                            mover.Forwards(false);
                            mover.Forwards(true);
                            mover.StrafeLeft(true);
                            //mover.Jump();
                            //mover.StrafeRight(false);
                        }
                        else if (stuckMove == 1)
                        {
                            mover.Forwards(false);
                            mover.Forwards(true);
                            mover.StrafeLeft(true);
                            //Context.Log("  strafe left"); 
                            //mover.Jump();
                            //mover.StrafeLeft(false);
                        }
                        else if (stuckMove == 2)
                        {
                            mover.Forwards(false);
                            mover.Forwards(true);
                            mover.StrafeRight(true);
                            //Context.Log("  strafe left"); 
                            //mover.StrafeLeft(true);
                        }
                        else if (stuckMove == 2)
                        {
                            mover.Forwards(false);
                            mover.Forwards(true);
                            mover.StrafeRight(true);
                            //Context.Log("  strafe left"); 
                            //mover.StrafeLeft(true);

                        }
                        stuckMove++;
                        if (stuckMove >= abortSensitivity)
                        {
                            return true;
                        }

                    }
                    else
                    {
                        stuckMove = 0;

                    }
                    predictedDX = 0;
                    predictedDY = 0;
                    oldLocation = loc;
                    StuckTimeout.Reset();
                }


            }
            return false;
        }

    }

    public class EasyMover
    {
        Location target;
        Player Me;
        PathGraph world = null;

        Mover mover;
        PPather pather;
        bool GiveUpIfStuck = false;
        bool GiveUpIfUnsafe = false;

        GSpellTimer PathTimeout = new GSpellTimer(5000); // no path can be older than this
        MoveAlonger MoveAlong;

        public enum MoveResult { Reached, Stuck, CantFindPath, Unsafe, Moving, GotThere };

        public EasyMover(PPather pather, Location target, bool GiveUpIfStuck, bool GiveUpIfUnsafe)
        {
            this.target = target;
            this.Me = BoogieCore.Player;
            this.world = pather.world;
            mover = PPather.mover;
            this.GiveUpIfStuck = GiveUpIfStuck;
            this.GiveUpIfUnsafe = GiveUpIfUnsafe;
            this.pather = pather;
        }
        public void SetPathTimeout(int ms)
        {
            PathTimeout = new GSpellTimer(ms); // no path can be older than this                
        }

        public void SetNewTarget(Location target)
        {
            MoveAlong = null;
            this.target = target;
        }

        public MoveResult move()
        {
            if (GiveUpIfUnsafe) // fixme
            {
                //if (!pather.IsItSafeAt(Me.Target, Me.Location))
                //    return MoveResult.Unsafe;
            }
            if (PathTimeout.IsReady)
            {
                MoveAlong = null;
            }
            BoogieCore.Log(LogType.System, "[Mover] MoveAlong = {0}", MoveAlong);
            if (MoveAlong == null)
            {
                Location from = new Location(Me.Location);
                mover.Stop();
                Path path = world.CreatePath(from, target, (float)4.5, PPather.radar); //fixme
                PathTimeout.Reset();
                BoogieCore.Log(LogType.System, "[Mover (New)] PathCount = {0}", path.Count());
                if (path == null || path.Count() == 0)
                {
                    //Context.Log("EasyMover: Can not create path . giving up");
                    mover.MoveRandom();
                    Thread.Sleep(200);
                    mover.Stop();
                    return MoveResult.CantFindPath;
                }
                else
                {
                    //Context.Log("Save path to " + pathfile); 
                    //path.Save(pathfile);
                    MoveAlong = new MoveAlonger(pather, path);
                }
            }

            if (MoveAlong != null)
            {
                Location from = new Location(Me.Location);
                BoogieCore.Log(LogType.System, "[Mover ] PathCount = {0}", MoveAlong.path.Count());

                if (MoveAlong.path.Count() == 0 || from.GetDistanceTo(target) < (float)5.0)
                {
                    BoogieCore.Log(LogType.System, "[Mover] Count = 0 or Dist < 5.0");
                    MoveAlong = null;
                    mover.Stop();
                    return MoveResult.GotThere;
                }
                else if (!MoveAlong.MoveAlong())
                {
                    BoogieCore.Log(LogType.System, "[Mover] Stuck!!");
                    MoveAlong = null; // got stuck!
                    if (GiveUpIfStuck) return MoveResult.Stuck;
                }
            }
            return MoveResult.Moving;
        }
    }

    public class MoveAlonger
    {
        public Path path;
        StuckDetecter sd;
        Player Me;
        Location prev;
        Location current;
        Location next;
        Mover mover;
        PathGraph world = null;
        int blockCount = 0;

        public MoveAlonger(PPather pather, Path path)
        {
            this.Me = BoogieCore.Player;
            this.path = path;
            this.world = pather.world;
            mover = PPather.mover;
            sd = new StuckDetecter(pather, 1, 2);
            prev = null;
            current = path.GetFirst();
            next = path.GetSecond();
        }

        public bool MoveAlong()
        {
            double max = 3.0;
            Coordinate loc = Me.Location;
            Location isAt = new Location(loc.X, loc.Y, loc.Z);
            /*
            while (isAt.GetDistanceTo(current) < max && next != null)
            {
                //Context.Log(current + " - " + next);
                path.RemoveFirst();
                if (path.Count() == 0)
                {
                    //Context.Log("ya");
                    //return true; // good in some way
                }
                else
                {
                    prev = current;
                    current = path.GetFirst();
                    next = path.GetSecond();
                }
            }
*/
            bool consume = false;
            do
            {

                bool blocked = false;

                consume = false;
                if (next != null)
                    world.triangleWorld.IsStepBlocked(loc.X, loc.Y, loc.Z, next.X, next.Y, next.Z,
                                                      PathGraph.toonHeight, PathGraph.toonSize, null);
                double d = isAt.GetDistanceTo(current);
                if ((d < max && !blocked) ||
                   d < 1.5)
                    consume = true;

                if (consume)
                {
                    //GContext.Main.Log("Consume spot " + current + " d " + d + " block " + blocked);
                    path.RemoveFirst();
                    if (path.Count() == 0)
                    {
                        break;
                    }
                    else
                    {
                        prev = current;
                        current = path.GetFirst();
                        next = path.GetSecond();
                    }
                }

            } while (consume);

            {
                //Context.Log("Move towards " + current);
                Coordinate gto = new Coordinate((float)current.X, (float)current.Y, (float)current.Z);
                Coordinate face;
                if (next != null)
                    face = new Coordinate(next.X, next.Y, next.Z);
                else
                    face = gto;

                if (!mover.moveTowardsFacing(gto, 0.5, face))
                {
                    //Context.Log("Can't move " + current);
                    world.BlacklistStep(prev, current);
                    //world.MarkStuckAt(loc, Me.Heading);                        
                    mover.MoveRandom();
                    Thread.Sleep(500);
                    mover.Stop();
                    return false;
                    // hmm, mover does not want to move, must be up or down
                }

                {
                    double h; double speed;
                    h = mover.GetMoveHeading(out speed);
                    float stand_z = 0.0f;
                    int flags = 0;
                    float x = isAt.X + (float)Math.Cos(h) * 1.0f;
                    float y = isAt.Y + (float)Math.Sin(h) * 1.0f;
                    float z = isAt.Z;
                    bool aheadOk = world.triangleWorld.FindStandableAt(x, y,
                                                                       z - 4,
                                                                       z + 6,
                                                                       out stand_z, out flags, 0, 0);
                    if (!aheadOk)
                    {
                        blockCount++;
                        world.MarkStuckAt(isAt, Me.Location.O);

                        if (prev != null)
                        {
                            Coordinate gprev = new Coordinate((float)prev.X, (float)prev.Y, (float)prev.Z);

                            if (!mover.moveTowardsFacing(gprev, 0.5, face))
                            {
                                mover.Stop();
                                return false;
                            }
                        }
                        if (blockCount > 1)
                        {
                            world.BlacklistStep(prev, current);
                            return false;
                        }

                        return true;
                    }
                    else
                        blockCount = 0;
                }


                if (sd.checkStuck())
                {
                    //Context.Log("Stuck at " + isAt);
                    world.MarkStuckAt(isAt, (float)Me.Heading);
                    world.BlacklistStep(prev, current);
                    mover.Stop();
                    return false;
                }
            }
            return true;
        }
    }

}

