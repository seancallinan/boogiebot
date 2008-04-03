using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Timers;
using System.IO;

using Foole.Crypt;
using Foole.Utils;
using Foole.WoW;


namespace BoogieBot.Common
{
    // Movement Packet Handling
    partial class WorldServerClient
    {

        public enum MoveState { StartRun, Run, StartWalk, Walk, StartStrafeL, StartStrafeR, StrafeR, StrafeL, StopRun, StopWalk, StopStrafe };

        public UInt16 MoveMask = 0;

        private void TeleportHandler(WoWReader wr)
        {
            float x, y, z, orient;
            byte mask = wr.ReadByte();

            WoWGuid guid = new WoWGuid(mask, wr.ReadBytes(WoWGuid.BitCount8(mask)));

            wr.ReadUInt32(); // flags
            
            wr.ReadUInt32(); // time?
            wr.ReadByte(); // unk 2.3.0

            wr.ReadSingle(); // unk2
            x = wr.ReadSingle();
            y = wr.ReadSingle();
            z = wr.ReadSingle();
            orient = wr.ReadSingle();
            wr.ReadUInt16(); // unk3
            wr.ReadByte(); // unk4

            BoogieCore.world.getPlayerObject().SetCoordinates(new Coordinate(x, y, z, orient));

            WoWWriter ww = new WoWWriter(OpCode.MSG_MOVE_TELEPORT_ACK);
            Send(ww.ToArray());
        }

        public void SendMoveHeartBeat()
        {
            SendMoveHeartBeat(BoogieCore.World.getPlayerObject().GetCoordinates());
        }


        public void SendMoveHeartBeat(Coordinate c)
        {
            SendMoveHeartBeat(c.X, c.Y, c.Z, c.O);
        }

        public void SendMoveHeartBeat(float x, float y, float z, float o)
        {
            BuildMovePacket(OpCode.MSG_MOVE_HEARTBEAT, x, y, z, o);
        }

        public void StartMoveForward()
        {
            BuildMovePacket(OpCode.MSG_MOVE_START_FORWARD, BoogieCore.world.getPlayerObject().GetCoordinates());
        }

        public void StopMoveForward()
        {
            BuildMovePacket(OpCode.MSG_MOVE_STOP, BoogieCore.world.getPlayerObject().GetCoordinates());
        }

        public void MoveJump()
        {
            BuildMovePacket(OpCode.MSG_MOVE_JUMP, BoogieCore.world.getPlayerObject().GetCoordinates());
        }
        private  void BuildMovePacket(OpCode op, Coordinate c)
        {
            WoWWriter ww = new WoWWriter(op);
            ww.Write((UInt32)0);
            ww.Write((byte)0);
            ww.Write((UInt32)MM_GetTime());

            ww.Write(c.X);
            ww.Write(c.Y);
            ww.Write(c.Z);
            ww.Write(c.O);

            ww.Write((UInt32)0);

            Send(ww.ToArray());
        }

        private void BuildMovePacket(OpCode op, float x, float y, float z, float o)
        {
            WoWWriter ww = new WoWWriter(op);
            ww.Write((UInt32)0);
            ww.Write((byte)0);
            ww.Write((UInt32)MM_GetTime());

            ww.Write(x);
            ww.Write(y);
            ww.Write(z);
            ww.Write(o);

            ww.Write((UInt32)0);

            Send(ww.ToArray());
        }

        private void MovementHandler(WoWReader wr)
        {
            WoWGuid guid;
            byte mask = wr.ReadByte();

            if (mask == 0x00)
                return;

            guid = new WoWGuid(mask, wr.ReadBytes(WoWGuid.BitCount8(mask)));

            MovementInfo mi = new MovementInfo(wr);

            if (BoogieCore.world.getObject(guid) != null)
            {
                BoogieCore.Log(LogType.Error, "Updating coordinates for object {0}, x={1} y={2} z={3}", BoogieCore.world.getObject(guid).Name, mi.x, mi.y, mi.z);
                BoogieCore.world.getObject(guid).SetCoordinates(mi.GetCoordinates());
            }
        }
    }

    public class MovementInfo
    {
        public UInt32 time;

        public UInt32 unk8, unk9, unk10, unk11, unk12;
        public UInt32 unklast;
        public float unk6;
        public float x, y, z, orientation;
        public UInt32 flags;
        public UInt32 FallTime;
        public UInt64 transGuid;
        public float transX, transY, transZ, transO;

        public Coordinate GetCoordinates()
        {
            return new Coordinate(x, y, z, orientation);
        }

        public MovementInfo(WoWReader wr)
        {
            transGuid = 0;
            flags = wr.ReadUInt32();
            wr.ReadByte();
            time = wr.ReadUInt32();

            x = wr.ReadFloat();
            y = wr.ReadFloat();
            z = wr.ReadFloat();
            orientation = wr.ReadFloat();

            if ((flags & 0x2000000) >= 1) // Transport
            {
                transGuid = wr.ReadUInt64();

                transX = wr.ReadFloat();
                transY = wr.ReadFloat();
                transZ = wr.ReadFloat();
                transO = wr.ReadFloat();
            }

            if ((flags & 0x200000) >= 1) // Swimming
            {
                unk6 = wr.ReadFloat();
            }

            if ((flags & 0x2000) >= 1) // Falling
            {
                FallTime = wr.ReadUInt32();
                unk8 = wr.ReadUInt32();
                unk9 = wr.ReadUInt32();
                unk10 = wr.ReadUInt32();
            }

            if ((flags & 0x4000000) >= 1)
            {
                unk12 = wr.ReadUInt32();
            }

            if (wr.Remaining >= 4) unklast = wr.ReadUInt32();

        }
    }
}
