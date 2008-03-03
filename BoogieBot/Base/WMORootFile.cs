using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BoogieBot.Common
{
    /// <summary>WMO Root File. Parses and stores a World Model Object.</summary>
    public class WMORootFile : WMOFile
    {
        public MOHD mohd;                  // WMO Root Header
        public List<String> groupNames;    // List of Group Names (MOGN chunk)
        public MOGI[] groupInfo;           // List of Group Info
        public MOPV[] portalVertices;      // List of Portal Vertices
        public MOPT[] portalInfo;          // List of Portal Info
        public MOPR[] portalGroupRel;      // List of Portal <-> Group Relationships
        public MODD[] doodadLocations;     // List of Doodad Locations in this WMO

        public WMORootFile(String filename) : base(filename)
        {
        }

        /*protected override void parseFile(MPQFile mpqfile)
        {
            MemoryStream ms = mpqfile.GetStream();
            BinaryReader bin = new BinaryReader(ms);

            BoogieCore.Log(LogType.SystemDebug, "WMORootFile: Parsing {0}... ", mpqfile.Filename);

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

                if (tempHeader.Is("MOHD"))  // WMO Root Header
                {
                    mohd = new MOHD();
                    mohd.nTextures = bin.ReadUInt32();
                    mohd.nGroups = bin.ReadUInt32();
                    mohd.nPortals = bin.ReadUInt32();
                    mohd.nLights = bin.ReadUInt32();
                    mohd.nModels = bin.ReadUInt32();
                    mohd.nDoodads = bin.ReadUInt32();
                    mohd.nDoodadSets = bin.ReadUInt32();
                    mohd.ambientColour = bin.ReadUInt32();
                    mohd.wmoID = bin.ReadUInt32();
                    mohd.c1 = new Coordinate(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                    mohd.c2 = new Coordinate(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                    mohd.unknown = bin.ReadUInt32();

                    continue;
                }

                if (tempHeader.Is("MOTX"))  // List of textures (BLP Files) used in this map object.
                {
                    // Not needed.
                    continue;
                }

                if (tempHeader.Is("MOMT"))  // Materials used in this map object
                {
                    // Not needed.
                    continue;
                }

                if (tempHeader.Is("MOGN"))  // List of group names for the groups in this map object.
                {
                    byte[] groupNamesChunk = bin.ReadBytes((int)tempHeader.Size);

                    groupNames = new List<String>();

                    StringBuilder str = new StringBuilder();

                    // Convert szString's to a List<String>.
                    for (int i = 0; i < groupNamesChunk.Length; i++)
                    {
                        if (groupNamesChunk[i] == '\0')
                        {
                            if (str.Length > 1)
                                groupNames.Add(str.ToString());
                            str = new StringBuilder();
                        }
                        else
                            str.Append((char)groupNamesChunk[i]);
                    }

                    continue;
                }

                if (tempHeader.Is("MOGI"))  // Group information for WMO groups.
                {
                    uint num = tempHeader.Size / 32;

                    groupInfo = new MOGI[num];

                    for (int i = 0; i < num; i++)
                    {
                        groupInfo[i].flags = bin.ReadUInt32();
                        groupInfo[i].c1 = new Coordinate(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle()); ;
                        groupInfo[i].c2 = new Coordinate(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle()); ;
                        groupInfo[i].nameOffset = bin.ReadInt32();
                    }

                    continue;
                }

                if (tempHeader.Is("MOSB"))  // Skybox. If not empty, it contains a model (M2) filename to use as a skybox.
                {
                    // Not needed.
                    continue;
                }

                if (tempHeader.Is("MOPV"))  // Portal vertices. Portals are (always?) rectangles that specify where doors or entrances are in a WMO.
                {
                    uint num = tempHeader.Size / 48;

                    portalVertices = new MOPV[num];

                    for (int i = 0; i < num; i++)
                    {
                        portalVertices[i].c1 = new Coordinate(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                        portalVertices[i].c2 = new Coordinate(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                        portalVertices[i].c3 = new Coordinate(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                        portalVertices[i].c4 = new Coordinate(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                    }

                    continue;
                }

                if (tempHeader.Is("MOPT"))  // Portal information.
                {
                    uint num = tempHeader.Size / 20;

                    portalInfo = new MOPT[num];

                    for (int i = 0; i < num; i++)
                    {
                        portalInfo[i].startVertex = bin.ReadUInt16();
                        portalInfo[i].count = bin.ReadUInt16();
                        portalInfo[i].w = bin.ReadSingle();
                        portalInfo[i].x = bin.ReadSingle();
                        portalInfo[i].y = bin.ReadSingle();
                        portalInfo[i].z = bin.ReadSingle();
                    }

                    continue;
                }

                if (tempHeader.Is("MOPR"))  // Portal - Group relationship
                {
                    uint num = tempHeader.Size / 8;

                    portalGroupRel = new MOPR[num];

                    for (int i = 0; i < num; i++)
                    {
                        portalGroupRel[i].portalIndex = bin.ReadUInt16();
                        portalGroupRel[i].groupIndex = bin.ReadUInt16();
                        portalGroupRel[i].side = bin.ReadInt16();
                        portalGroupRel[i].filler = bin.ReadUInt16();
                    }

                    continue;
                }

                if (tempHeader.Is("MOVV"))  // Visible block vertices.
                {
                    // Do we need this? Don't think so.
                    continue;
                }

                if (tempHeader.Is("MOVB"))  // Visible block list.
                {
                    // Do we need this? Don't think so.
                    continue;
                }

                if (tempHeader.Is("MOLT"))  // Lighting information.
                {
                    // Not needed.
                    continue;
                }

                if (tempHeader.Is("MODS"))  // This chunk defines doodad sets.
                {
                    // Not needed.
                    continue;
                }

                if (tempHeader.Is("MODN"))  // List of filenames for M2 models that appear in this map tile.
                {
                    // Not needed.
                    continue;
                }

                if (tempHeader.Is("MODD"))  // Information for doodad instances.
                {
                    uint num = tempHeader.Size / 40;

                    doodadLocations = new MODD[num];

                    for (int i = 0; i < num; i++)
                    {
                        doodadLocations[i].nameOffset = bin.ReadUInt32();
                        doodadLocations[i].coord = new Coordinate(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                        doodadLocations[i].q.w = bin.ReadSingle();
                        doodadLocations[i].q.x = bin.ReadSingle();
                        doodadLocations[i].q.y = bin.ReadSingle();
                        doodadLocations[i].q.z = bin.ReadSingle();
                        doodadLocations[i].scale = bin.ReadSingle();
                        doodadLocations[i].b = bin.ReadByte();
                        doodadLocations[i].g = bin.ReadByte();
                        doodadLocations[i].r = bin.ReadByte();
                        doodadLocations[i].a = bin.ReadByte();
                    }

                    continue;
                }

                if (tempHeader.Is("MFOG"))  // Fog information.
                {
                    // Not Needed.
                    continue;
                }

                if (tempHeader.Is("MCVP"))  // Convex Volume Planes.
                {
                    // Do we need this? Don't think so.
                    continue;
                }

                // If we're still down here, we got a problem
                throw new Exception(String.Format("WMORootFile: Woah. Got a header of {0}. Don't know how to deal with this, bailing out.", tempHeader.ToString()));
            }

            // Further processing after finishing all chunks goes here.
        }*/

        // MOHD Chunk (WMO Header)
        public struct MOHD
        {
            public UInt32 nTextures;
            public UInt32 nGroups;
            public UInt32 nPortals;
            public UInt32 nLights;
            public UInt32 nModels;
            public UInt32 nDoodads;
            public UInt32 nDoodadSets;
            public UInt32 ambientColour;
            public UInt32 wmoID;
            public Coordinate c1;
            public Coordinate c2;
            public UInt32 unknown;
        }

        // MOGI Chunk (Group Info)
        public struct MOGI
        {
            public UInt32 flags;
            public Coordinate c1;
            public Coordinate c2;
            public Int32 nameOffset;
        }

        // MOPV (Portal Vertices)
        public struct MOPV
        {
            public Coordinate c1;
            public Coordinate c2;
            public Coordinate c3;
            public Coordinate c4;
        }

        // MOPT Chunk (Portal Info)
        public struct MOPT
        {
            public UInt16 startVertex;
            public UInt16 count;
            public float w;
            public float x;
            public float y;
            public float z;
        }

        // MOPR Chunk (Group - Portal Relationship)
        public struct MOPR
        {
            public UInt16 portalIndex;
            public UInt16 groupIndex;
            public Int16 side;
            public UInt16 filler;
        }

        // MODD Chunk (Doodad Placement)
        public struct MODD
        {
            public UInt32 nameOffset;
            public Coordinate coord;
            public Quaternion q;
            public float scale;
            public byte b;
            public byte g;
            public byte r;
            public byte a;
        }

        // A Quaternion
        public struct Quaternion
        {
            public float w;
            public float x;
            public float y;
            public float z;
        }
    }
}
