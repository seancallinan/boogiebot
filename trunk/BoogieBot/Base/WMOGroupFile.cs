using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BoogieBot.Common
{
    /// <summary>WMO Group File. Parses and stores a World Model Object.</summary>
    public class WMOGroupFile : WMOFile
    {
        public MOGP mogp;                  // WMO Group Header
        public UInt16[] indices;           // Triangle Indices
        public Coordinate[] vertices;      // Triangle Vertex Coordinates
        public Vect3D[] normals;           // Triangle Normals
        public MLIQ mliq;                  // Liquid

        public WMOGroupFile(String filename) : base(filename)
        {
        }

        /*protected override void parseFile(MPQFile mpqfile)
        {
            MemoryStream ms = mpqfile.GetStream();
            BinaryReader bin = new BinaryReader(ms);

            BoogieCore.Log(LogType.SystemDebug, "WMOGroupFile: Parsing {0}... ", mpqfile.Filename);

            BlizChunkHeader tempHeader;
            long pos = 0;

            // Read bytes from the stream until we run out
            while (pos < ms.Length)
            {
                // Advance to the next Chunk
                ms.Position = pos;

                // Read in Chunk Header Name
                tempHeader = new BlizChunkHeader(bin.ReadChars(4), bin.ReadUInt32());
                tempHeader.Flip();

                // Set pos to the location of the next Chunk
                pos = ms.Position + tempHeader.Size;

                if (tempHeader.Is("MVER"))   // WMO File Version
                {
                    mver = new MVER();
                    mver.version = bin.ReadUInt32();

                    continue;
                }

                if (tempHeader.Is("MOGP"))  //  WMO Group Header
                {
                    pos = ms.Position + 68; // Size fix (header actually stores entire file size?? yay consistancy!)

                    mogp = new MOGP();
                    mogp.nameStart = bin.ReadUInt32();
                    mogp.nameStart2 = bin.ReadUInt32();
                    mogp.flags = bin.ReadUInt32();
                    mogp.c1 = new Coordinate(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                    mogp.c2 = new Coordinate(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                    mogp.portalStart = bin.ReadUInt16();
                    mogp.portalCount = bin.ReadUInt16();
                    mogp.batch1 = bin.ReadUInt16();
                    mogp.batch2 = bin.ReadUInt16();
                    mogp.batch3 = bin.ReadUInt16();
                    mogp.batch4 = bin.ReadUInt16();
                    mogp.fog1 = bin.ReadByte();
                    mogp.fog2 = bin.ReadByte();
                    mogp.fog3 = bin.ReadByte();
                    mogp.fog4 = bin.ReadByte();
                    mogp.Unknown1 = bin.ReadUInt32();
                    mogp.wmoGroupID = bin.ReadUInt32();
                    mogp.Unknown2 = bin.ReadUInt32();
                    mogp.Unknown3 = bin.ReadUInt32();

                    continue;
                }

                if (tempHeader.Is("MOPY"))  // Material info for triangles.
                {
                    // Not needed.
                    continue;
                }

                if (tempHeader.Is("MOVI"))  // Vertex indices for triangles.
                {
                    uint num = tempHeader.Size / 2;

                    indices = new UInt16[num];

                    for (int i = 0; i < num; i++)
                        indices[i] = bin.ReadUInt16();

                    continue;
                }

                if (tempHeader.Is("MOVT"))  // Vertices chunk.
                {
                    uint num = tempHeader.Size / 12;

                    vertices = new Coordinate[num];

                    for (int i = 0; i < num; i++)
                        vertices[i] = new Coordinate(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());

                    continue;
                }

                if (tempHeader.Is("MONR"))  // Normals.
                {
                    uint num = tempHeader.Size / 12;

                    normals = new Vect3D[num];

                    for (int i = 0; i < num; i++)
                        normals[i] = new Vect3D(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());

                    continue;
                }

                if (tempHeader.Is("MOTV"))  // Texture coordinates.
                {
                    // Not needed.
                    continue;
                }

                if (tempHeader.Is("MOBA"))  // Render batches.
                {
                    // Not needed.
                    continue;
                }

                if (tempHeader.Is("MOLR"))  // Light references.
                {
                    // Not needed.
                    continue;
                }

                if (tempHeader.Is("MODR"))  // Doodad references.
                {
                    // Need this.
                    continue;
                }

                if (tempHeader.Is("MOBN"))  // Array of t_BSP_NODE.
                {
                    // Do we need this? Don't think so; wowmapview and wowmodelview don't use it.
                    continue;
                }

                if (tempHeader.Is("MOBR"))  // Triangle indices (in MOVI which define triangles) to describe polygon planes defined by MOBN BSP nodes.
                {
                    // Do we need this? Don't think so; wowmapview and wowmodelview don't use it.
                    continue;
                }

                if (tempHeader.Is("MOCV"))  // Vertex colors, 4 bytes per vertex (BGRA), for WMO groups using indoor lighting.
                {
                    // Not needed.
                    continue;
                }

                if (tempHeader.Is("MLIQ"))  // Specifies liquids inside WMOs.
                {
                    mliq = new MLIQ();
                    mliq.x = bin.ReadUInt32();
                    mliq.y = bin.ReadUInt32();
                    mliq.a = bin.ReadUInt32();
                    mliq.b = bin.ReadUInt32();
                    mliq.coord = new Coordinate(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                    mliq.type = bin.ReadUInt16();

                    continue;
                }

                // If we're still down here, we got a problem
                throw new Exception(String.Format("WMOGroupFile: Woah. Got a header of {0}. Don't know how to deal with this, bailing out.", tempHeader.ToString()));
            }

            // Further processing after finishing all chunks goes here.
        }*/

        // MOGP Chunk (WMO Header)
        public struct MOGP
        {
            public UInt32 nameStart;
            public UInt32 nameStart2;
            public UInt32 flags;
            public Coordinate c1;
            public Coordinate c2;
            public UInt16 portalStart;
            public UInt16 portalCount;
            public UInt16 batch1;
            public UInt16 batch2;
            public UInt16 batch3;
            public UInt16 batch4;
            public byte fog1;
            public byte fog2;
            public byte fog3;
            public byte fog4;
            public UInt32 Unknown1;
            public UInt32 wmoGroupID;
            public UInt32 Unknown2;
            public UInt32 Unknown3;
        }

        // MLIQ Chunk (Liquid)
        public struct MLIQ
        {
            public UInt32 x;
            public UInt32 y;
            public UInt32 a;
            public UInt32 b;
            public Coordinate coord;
            public UInt16 type;
        }
    }
}
