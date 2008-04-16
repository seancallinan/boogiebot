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
        public enum MovementFlags : ulong
{
	// Byte 1 (Resets on Movement Key Press)
    MOVEFLAG_MOVE_STOP                  = 0x00,			//verified
	MOVEFLAG_MOVE_FORWARD				= 0x01,			//verified
	MOVEFLAG_MOVE_BACKWARD				= 0x02,			//verified
	MOVEFLAG_STRAFE_LEFT				= 0x04,			//verified
	MOVEFLAG_STRAFE_RIGHT				= 0x08,			//verified
	MOVEFLAG_TURN_LEFT					= 0x10,			//verified
	MOVEFLAG_TURN_RIGHT					= 0x20,			//verified
	MOVEFLAG_PITCH_DOWN					= 0x40,			//Unconfirmed
	MOVEFLAG_PITCH_UP					= 0x80,			//Unconfirmed

	// Byte 2 (Resets on Situation Change)
	MOVEFLAG_WALK						= 0x100,		//verified
	MOVEFLAG_TAXI						= 0x200,		
	MOVEFLAG_NO_COLLISION				= 0x400,
	MOVEFLAG_FLYING	    				= 0x800,		//verified
	MOVEFLAG_REDIRECTED					= 0x1000,		//Unconfirmed
	MOVEFLAG_FALLING					= 0x2000,       //verified
	MOVEFLAG_FALLING_FAR				= 0x4000,		//verified
	MOVEFLAG_FREE_FALLING				= 0x8000,		//half verified

	// Byte 3 (Set by server. TB = Third Byte. Completely unconfirmed.)
	MOVEFLAG_TB_PENDING_STOP			= 0x10000,		// (MOVEFLAG_PENDING_STOP)
	MOVEFLAG_TB_PENDING_UNSTRAFE		= 0x20000,		// (MOVEFLAG_PENDING_UNSTRAFE)
	MOVEFLAG_TB_PENDING_FALL			= 0x40000,		// (MOVEFLAG_PENDING_FALL)
	MOVEFLAG_TB_PENDING_FORWARD			= 0x80000,		// (MOVEFLAG_PENDING_FORWARD)
	MOVEFLAG_TB_PENDING_BACKWARD		= 0x100000,		// (MOVEFLAG_PENDING_BACKWARD)
	MOVEFLAG_SWIMMING          		    = 0x200000,		//  verified
	MOVEFLAG_FLYING_PITCH_UP	        = 0x400000,		// (half confirmed)(MOVEFLAG_PENDING_STR_RGHT)
	MOVEFLAG_TB_MOVED					= 0x800000,		// (half confirmed) gets called when landing (MOVEFLAG_MOVED)

	// Byte 4 (Script Based Flags. Never reset, only turned on or off.)
	MOVEFLAG_AIR_SUSPENSION	   	 		= 0x1000000,	// confirmed allow body air suspension(good name? lol).
	MOVEFLAG_AIR_SWIMMING				= 0x2000000,	// confirmed while flying.
	MOVEFLAG_SPLINE_MOVER				= 0x4000000,	// Unconfirmed
	MOVEFLAG_IMMOBILIZED				= 0x8000000,
	MOVEFLAG_WATER_WALK					= 0x10000000,
	MOVEFLAG_FEATHER_FALL				= 0x20000000,	// Does not negate fall damage.
	MOVEFLAG_LEVITATE					= 0x40000000,
	MOVEFLAG_LOCAL						= 0x80000000,	// This flag defaults to on. (Assumption)

	// Masks
	MOVEFLAG_MOVING_MASK				= 0x03,
	MOVEFLAG_STRAFING_MASK				= 0x0C,
	MOVEFLAG_TURNING_MASK				= 0x30,
	MOVEFLAG_FALLING_MASK				= 0x6000,
	MOVEFLAG_MOTION_MASK				= 0xE00F,		// Forwards, Backwards, Strafing, Falling
	MOVEFLAG_PENDING_MASK				= 0x7F0000,
	MOVEFLAG_PENDING_STRAFE_MASK		= 0x600000,
	MOVEFLAG_PENDING_MOVE_MASK			= 0x180000,
	MOVEFLAG_FULL_FALLING_MASK			= 0xE000,
};
        public enum MoveState { StartRun, Run, StartWalk, Walk, StartStrafeL, StartStrafeR, StrafeR, StrafeL, StopRun, StopWalk, StopStrafe };

        public UInt16 MoveMask = 0;
        private ulong MoveFlags = 0;

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
            BoogieCore.Log(LogType.SystemDebug, "Got teleport to: {0} {1} {2} {3}", x, y, z, orient);

            BoogieCore.world.getPlayerObject().SetCoordinates(new Coordinate(x, y, z, orient));

            WoWWriter ww = new WoWWriter(OpCode.MSG_MOVE_TELEPORT_ACK);
            ww.Write(BoogieCore.world.getPlayerObject().GUID.GetOldGuid());
            Send(ww.ToArray());
            SendMoveHeartBeat(BoogieCore.world.getPlayerObject().GetCoordinates());
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
        private void SetMoveFlag(MovementFlags flag)
        {
            MoveFlags |= (ulong)flag;
        }
        private void UnSetMoveFlag(MovementFlags flag)
        {
            MoveFlags &= ~(ulong)flag;
        }
        public void StartMoveForward()
        {
            SetMoveFlag(MovementFlags.MOVEFLAG_TURN_LEFT);
            BuildMovePacket(OpCode.MSG_MOVE_START_TURN_LEFT, BoogieCore.World.getPlayerObject().GetCoordinates());
        }

        public void StopMoveForward()
        {
            //UnSetMoveFlag(MovementFlags.MOVEFLAG_MOVE_STOP);
            MoveFlags = 0;
            BuildMovePacket(OpCode.MSG_MOVE_STOP_TURN, BoogieCore.World.getPlayerObject().GetCoordinates());
        }

        public void MoveJump()
        {
            BuildMovePacket(OpCode.MSG_MOVE_JUMP, BoogieCore.world.getPlayerObject().GetCoordinates());
        }
        private  void BuildMovePacket(OpCode op, Coordinate c)
        {
            BuildMovePacket(op, c.X, c.Y, c.Z, c.O);
        }

        private void BuildMovePacket(OpCode op, float x, float y, float z, float o)
        {
            WoWWriter ww = new WoWWriter(op);
            ww.Write((UInt32)MoveFlags);
            ww.Write((byte)255);
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
            BoogieCore.Log(LogType.Error, "Got movement opcode for {0} with the length {1}", guid, wr.Remaining);

            MovementInfo mi = new MovementInfo(wr);
            BoogieCore.Log(LogType.Error, "Updating coordinates for object {0}, x={1} y={2} z={3}", guid, mi.x, mi.y, mi.z);

            if (BoogieCore.world.getObject(guid) != null)
            {
                //BoogieCore.Log(LogType.Error, "Updating coordinates for object {0}, x={1} y={2} z={3}", BoogieCore.world.getObject(guid).Name, mi.x, mi.y, mi.z);
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
