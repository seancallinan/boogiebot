using System;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Text;

using WowTriangles;
using BoogieBot.Common;

/*
 * Tris class (and realed) are responsible for all pathfinfing 
 */

namespace Pather.Graph
{
    ///////////////////////////////////////////////////////
    // START PathGraph
    public class Location
    {
        private float x;
        private float y;
        private float z;

        public Location(Coordinate l)
        {
            this.x = l.X; this.y = l.Y; this.z = l.Z;
        }


        public Location(float x, float y, float z)
        {
            this.x = x; this.y = y; this.z = z;
        }

        public float X { get { return x; } }
        public float Y { get { return y; } }
        public float Z { get { return z; } }
        public float GetDistanceTo(Location l)
        {
            float dx = x - l.X;
            float dy = y - l.Y;
            float dz = z - l.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public override String ToString()
        {
            //String s = String.Format(
            String s = "[" + x + "," + y + "," + z + "]";
            return s;
        }


        public Location InFrontOf(float heading, float d)
        {
            float nx = x + (float)Math.Cos(heading) * d;
            float ny = y + (float)Math.Sin(heading) * d;
            float nz = z;
            return new Location(nx, ny, nz);
        }

        public ulong GUID()
        {
            ulong ix = (ulong)(x + 100000.0);
            ulong iy = (ulong)(y + 100000.0);
            ulong iz = (ulong)(z + 100000.0);
            ulong g = ix | (iy << 24) | (iz << 48);
            return g;
        }
    }

    public class GraphChunk
    {
        public const int CHUNK_SIZE = 512;


        float base_x, base_y;
        public int ix, iy;
        public bool modified = false;
        public long LRU = 0;

        Spot[,] spots;

        public GraphChunk(float base_x, float base_y, int ix, int iy)
        {
            this.base_x = base_x;
            this.base_y = base_y;
            this.ix = ix;
            this.iy = iy;
            spots = new Spot[CHUNK_SIZE, CHUNK_SIZE];
            modified = false;
        }

        public void Clear()
        {
            foreach (Spot s in spots)
                if (s != null) s.traceBack = null;

            spots = null;
        }

        private void LocalCoords(float x, float y, out int ix, out int iy)
        {
            ix = (int)(x - base_x);
            iy = (int)(y - base_y);
        }

        public Spot GetSpot2D(float x, float y)
        {
            int ix, iy;
            LocalCoords(x, y, out ix, out iy);
            Spot s = spots[ix, iy];
            return s;
        }

        public Spot GetSpot(float x, float y, float z)
        {
            Spot s = GetSpot2D(x, y);

            while (s != null && !s.IsCloseZ(z))
            {
                s = s.next;
            }

            return s;
        }

        // return old spot at conflicting poision
        // or the same as passed the function if all was ok
        public Spot AddSpot(Spot s)
        {
            Spot old = GetSpot(s.X, s.Y, s.Z);
            if (old != null) return old;
            int x, y;

            s.chunk = this;

            LocalCoords(s.X, s.Y, out x, out y);

            s.next = spots[x, y];
            spots[x, y] = s;
            modified = true;
            return s;
        }




        public List<Spot> GetAllSpots()
        {
            List<Spot> l = new List<Spot>();
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    Spot s = spots[x, y];
                    while (s != null)
                    {
                        l.Add(s);
                        s = s.next;
                    }
                }
            }
            return l;
        }

        private string FileName()
        {
            return String.Format("c_{0,3:000}_{1,3:000}.bin", ix, iy);
        }

        private const uint FILE_MAGIC = 0x12341234;
        private const uint FILE_ENDMAGIC = 0x43214321;
        private const uint SPOT_MAGIC = 0x53504f54;


        // Per spot: 
        // uint32 magic
        // uint32 reserved;
        // uint32 flags;
        // float x;
        // float y;
        // float z;
        // uint32 no_paths
        //   for each path
        //     float x;
        //     float y;
        //     float z;


        public bool Load(string baseDir)
        {
            string fileName = FileName();
            string filenamebin = baseDir + fileName;

            System.IO.Stream stream = null;
            System.IO.BinaryReader file = null;
            int n_spots = 0;
            int n_steps = 0;
            try
            {
                stream = System.IO.File.OpenRead(filenamebin);
                if (stream != null)
                {
                    file = new System.IO.BinaryReader(stream);
                    if (file != null)
                    {
                        uint magic = file.ReadUInt32();
                        if (magic == FILE_MAGIC)
                        {

                            uint type;
                            while ((type = file.ReadUInt32()) != FILE_ENDMAGIC)
                            {
                                n_spots++;
                                uint reserved = file.ReadUInt32();
                                uint flags = file.ReadUInt32();
                                float x = file.ReadSingle();
                                float y = file.ReadSingle();
                                float z = file.ReadSingle();
                                uint n_paths = file.ReadUInt32();
                                Spot s = new Spot(x, y, z);
                                s.flags = flags;

                                for (uint i = 0; i < n_paths; i++)
                                {
                                    n_steps++;
                                    float sx = file.ReadSingle();
                                    float sy = file.ReadSingle();
                                    float sz = file.ReadSingle();
                                    s.AddPathTo(sx, sy, sz);
                                }
                                AddSpot(s);
                            }
                        }
                    }
                }
            }
            catch { }
            if (file != null)
            {
                file.Close();
            }
            if (stream != null)
            {
                stream.Close();
            }


            Log("Loaded " + fileName + " " + n_spots + " spots " + n_steps + " steps");

            modified = false;
            return false;
        }




        public bool Save(string baseDir)
        {
            if (!modified) return true; // doh

            string fileName = FileName();
            string filename = baseDir + fileName;

            System.IO.Stream fileout = null;
            System.IO.BinaryWriter file = null;

            try
            {
                System.IO.Directory.CreateDirectory(baseDir);
            }
            catch { };

            int n_spots = 0;
            int n_steps = 0;
            try
            {

                fileout = System.IO.File.Create(filename + ".new");

                if (fileout != null)
                {
                    file = new System.IO.BinaryWriter(fileout);

                    if (file != null)
                    {
                        file.Write(FILE_MAGIC);

                        List<Spot> spots = GetAllSpots();
                        foreach (Spot s in spots)
                        {
                            file.Write(SPOT_MAGIC);
                            file.Write((uint)0); // reserved
                            file.Write((uint)s.flags);
                            file.Write((float)s.X);
                            file.Write((float)s.Y);
                            file.Write((float)s.Z);
                            uint n_paths = (uint)s.n_paths;
                            file.Write((uint)n_paths);
                            for (uint i = 0; i < n_paths; i++)
                            {
                                uint off = i * 3;
                                file.Write((float)s.paths[off]);
                                file.Write((float)s.paths[off + 1]);
                                file.Write((float)s.paths[off + 2]);
                                n_steps++;
                            }
                            n_spots++;

                        }

                        file.Write(FILE_ENDMAGIC);
                    }

                    if (file != null)
                    {
                        file.Close(); file = null;
                    }

                    if (fileout != null)
                    {
                        fileout.Close(); fileout = null;
                    }

                    String old = filename + ".bak";

                    if (System.IO.File.Exists(old))
                        System.IO.File.Delete(old);
                    if (System.IO.File.Exists(filename))
                        System.IO.File.Move(filename, old);
                    System.IO.File.Move(filename + ".new", filename);
                    if (System.IO.File.Exists(old))
                        System.IO.File.Delete(old);

                    modified = false;
                }
                else
                {
                    Log("Save failed");
                }
            }
            catch (Exception e)
            {
                Log("Save failed " + e);
            }

            if (file != null)
            {
                file.Close(); file = null;
            }

            if (fileout != null)
            {
                fileout.Close(); fileout = null;
            }

            Log("Saved " + fileName + " " + n_spots + " spots " + n_steps + " steps");

            return false;
        }

        private void Log(String s)
        {
            Console.WriteLine(s);
            //GContext.Main.Log(s);
        }
    }

    public class Spot
    {
        public const float Z_RESOLUTION = 2.0f; // Z spots max this close

        public const uint FLAG_VISITED = 0x0001;
        public const uint FLAG_BLOCKED = 0x0002;
        public const uint FLAG_MPQ_MAPPED = 0x0004;
        public const uint FLAG_WATER = 0x0008;

        public float X, Y, Z;
        public uint flags;


        public int n_paths = 0;
        public float[] paths; // 3 floats per outgoing path


        public GraphChunk chunk = null;
        public Spot next;  // list on same x,y, used by chunk

        public int searchID = 0;
        public Spot traceBack; // Used by search
        public float score; // Used by search
        public bool closed, scoreSet;


        public Spot(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Spot(Location l)
        {
            this.X = l.X;
            this.Y = l.Y;
            this.Z = l.Z;
        }

        public bool IsBlocked()
        {
            return GetFlag(FLAG_BLOCKED);
        }

        public Location location { get { return new Location(X, Y, Z); } }
        public float GetDistanceTo(Location l)
        {
            float dx = l.X - X;
            float dy = l.Y - Y;
            float dz = l.Z - Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public float GetDistanceTo(Spot s)
        {
            float dx = s.X - X;
            float dy = s.Y - Y;
            float dz = s.Z - Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public bool IsCloseZ(float z)
        {
            float dz = z - this.Z;
            if (dz > Z_RESOLUTION) return false;
            if (dz < -Z_RESOLUTION) return false;
            return true;
        }

        public void SetFlag(uint flag, bool val)
        {
            uint old = flags;
            if (val)
                flags |= flag;
            else
                flags &= ~flag;
            if (chunk != null && old != flags) chunk.modified = true;
        }

        public bool GetFlag(uint flag)
        {
            return (flags & flag) != 0;
        }

        public void SetLocation(Location l)
        {
            X = l.X;
            Y = l.Y;
            Z = l.Z;
            if (chunk != null) chunk.modified = true;
        }

        public Location GetLocation()
        {
            return new Location(X, Y, Z);
        }

        public bool GetPath(int i, out float x, out float y, out float z)
        {
            x = y = z = 0;
            if (i > n_paths) return false;
            int off = i * 3;
            x = paths[off];
            y = paths[off + 1];
            z = paths[off + 2];
            return true;
        }


        public Spot GetToSpot(PathGraph pg, int i)
        {
            float x, y, z;
            GetPath(i, out x, out y, out z);
            return pg.GetSpot(x, y, z);

        }

        public List<Spot> GetPathsToSpots(PathGraph pg)
        {
            List<Spot> list = new List<Spot>(n_paths);
            for (int i = 0; i < n_paths; i++)
            {
                list.Add(GetToSpot(pg, i));
            }
            return list;
        }

        public List<Location> GetPaths()
        {
            List<Location> l = new List<Location>();
            if (paths == null) return l;
            for (int i = 0; i < n_paths; i++)
            {
                int off = i * 3;
                Location loc = new Location(paths[off], paths[off + 1], paths[off + 2]);
                l.Add(loc);
            }
            return l;
        }

        public bool HasPathTo(PathGraph pg, Spot s)
        {
            for (int i = 0; i < n_paths; i++)
            {
                Spot to = GetToSpot(pg, i);
                if (to == s) return true;
            }
            return false;
        }

        public bool HasPathTo(Location l)
        {
            return HasPathTo(l.X, l.Y, l.Z);
        }



        public bool HasPathTo(float x, float y, float z)
        {
            if (paths == null) return false;
            for (int i = 0; i < n_paths; i++)
            {
                int off = i * 3;
                if (x == paths[off] &&
                   y == paths[off + 1] &&
                   z == paths[off + 2])
                    return true;
            }
            return false;
        }

        public void AddPathTo(Spot s)
        {
            AddPathTo(s.X, s.Y, s.Z);
        }

        public void AddPathTo(Location l)
        {
            AddPathTo(l.X, l.Y, l.Z);
        }

        public void AddPathTo(float x, float y, float z)
        {
            if (HasPathTo(x, y, z)) return;
            int old_size;
            if (paths == null) old_size = 0; else old_size = paths.Length / 3;
            if (n_paths + 1 > old_size)
            {
                int new_size = old_size * 2;
                if (new_size < 4) new_size = 4;
                Array.Resize<float>(ref paths, new_size * 3);
            }

            int off = n_paths * 3;
            paths[off] = x;
            paths[off + 1] = y;
            paths[off + 2] = z;
            n_paths++;
            if (chunk != null) chunk.modified = true;
        }

        public void RemovePathTo(Location l)
        {
            RemovePathTo(l.X, l.Y, l.Z);
        }

        public void RemovePathTo(float x, float y, float z)
        {
            // look for it
            int found_index = -1;
            for (int i = 0; i < n_paths && found_index == -1; i++)
            {
                int off = i * 3;
                if (paths[off] == x &&
                   paths[off + 1] == y &&
                   paths[off + 2] == z)
                {
                    found_index = i;
                }
            }
            if (found_index != -1)
            {
                //GContext.Main.Log("Remove path (" + found_index + ") to " + x + " " + y + " " + n_paths);
                for (int i = found_index; i < n_paths - 1; i++)
                {
                    int off = i * 3;
                    paths[off] = paths[off + 3];
                    paths[off + 1] = paths[off + 4];
                    paths[off + 2] = paths[off + 5];
                }
                n_paths--;
                if (chunk != null) chunk.modified = true;
            }
            else
            {
                //GContext.Main.Log("Found not path to remove (" + found_index + ") to " + x + " " + y + " ");

            }
        }

        // search stuff

        public bool SetSearchID(int id)
        {
            if (searchID != id)
            {
                closed = false;
                scoreSet = false;
                searchID = id;
                return true;
            }
            return false;
        }

        public bool SearchIsClosed(int id)
        {
            if (id == searchID) return closed;
            return false;
        }

        public void SearchClose(int id)
        {
            SetSearchID(id);
            closed = true;
        }

        public bool SearchScoreIsSet(int id)
        {
            if (id == searchID) return scoreSet;
            return false;
        }

        public float SearchScoreGet(int id)
        {
            return score;
        }

        public void SearchScoreSet(int id, float score)
        {
            SetSearchID(id);
            this.score = score;
            scoreSet = true;
        }
    }

    class SpotData<T>
    {
        Dictionary<Spot, T> data = new Dictionary<Spot, T>();

        public T Get(Spot s)
        {
            T t = default(T);
            data.TryGetValue(s, out t);
            return t;
        }

        public void Set(Spot s, T t)
        {
            if (data.ContainsKey(s))
                data.Remove(s);
            data.Add(s, t);
        }

        public bool IsSet(Spot s)
        {
            return data.ContainsKey(s);
        }

    }

    public class Path
    {
        List<Location> locations = new List<Location>();

        public Path()
        {
        }

        public Path(List<Spot> steps)
        {
            foreach (Spot s in steps)
            {
                AddLast(s.location);
            }
        }

        public int Count()
        {
            return locations.Count;
        }

        public Location GetFirst()
        {
            return Get(0);

        }
        public Location GetSecond()
        {
            if (locations.Count > 1)
                return Get(1);
            return null;
        }

        public Location GetLast()
        {
            return locations[locations.Count - 1];
        }

        public Location RemoveFirst()
        {
            Location l = Get(0);
            locations.RemoveAt(0);
            return l;
        }

        public Location Get(int index)
        {
            return locations[index];
        }

        public void AddFirst(Location l)
        {
            locations.Insert(0, l);
        }

        public void AddFirst(Path l)
        {
            locations.InsertRange(0, l.locations);
        }

        public void AddLast(Location l)
        {
            locations.Add(l);
        }


        public void AddLast(Path l)
        {
            locations.AddRange(l.locations);
        }
    }

    public interface ILocationHeuristics
    {
        float Score(float x, float y, float z);
    }

    public class PathGraph
    {
        public const float toonHeight = 2.0f;
        public const float toonSize = 0.5f;


        public const float MinStepLength = 2f;
        public const float WantedStepLength = 3f;
        public const float MaxStepLength = 5f;

        public const float CHUNK_BASE = 100000.0f; // Always keep positive
        string BaseDir = "pathing2";
        string Continent;
        SparseMatrix2D<GraphChunk> chunks;


        public ChunkedTriangleCollection triangleWorld;
        public TriangleCollection paint;

        List<GraphChunk> ActiveChunks = new List<GraphChunk>();
        long LRU = 0;


        public PathGraph(string continent,
                         ChunkedTriangleCollection triangles,
                         TriangleCollection paint)
        {
            this.Continent = continent;
            this.triangleWorld = triangles;
            this.paint = paint;
            Clear();
        }

        public void Close()
        {
            triangleWorld.Close();
        }

        public void Clear()
        {
            chunks = new SparseMatrix2D<GraphChunk>(8);
        }

        private void GetChunkCoord(float x, float y, out int ix, out int iy)
        {
            ix = (int)((CHUNK_BASE + x) / GraphChunk.CHUNK_SIZE);
            iy = (int)((CHUNK_BASE + y) / GraphChunk.CHUNK_SIZE);
        }

        private void GetChunkBase(int ix, int iy, out float bx, out float by)
        {
            bx = (float)ix * GraphChunk.CHUNK_SIZE - CHUNK_BASE;
            by = (float)iy * GraphChunk.CHUNK_SIZE - CHUNK_BASE;
        }

        private GraphChunk GetChunkAt(float x, float y)
        {
            int ix, iy;
            GetChunkCoord(x, y, out ix, out iy);
            GraphChunk c = chunks.Get(ix, iy);
            if (c != null) c.LRU = LRU++;
            return c;
        }

        private void CheckForChunkEvict()
        {
            lock (this)
            {
                if (ActiveChunks.Count < 25) return;

                GraphChunk evict = null;
                foreach (GraphChunk gc in ActiveChunks)
                {
                    if (evict == null || gc.LRU < evict.LRU)
                    {
                        evict = gc;
                    }
                }

                // It is full!
                evict.Save(BaseDir + "\\" + Continent + "\\");
                ActiveChunks.Remove(evict);
                chunks.Clear(evict.ix, evict.iy);
                evict.Clear();
            }
        }



        public void Save()
        {
            lock (this)
            {
                ICollection<GraphChunk> l = chunks.GetAllElements();
                foreach (GraphChunk gc in l)
                {
                    if (gc.modified)
                    {
                        gc.Save(BaseDir + "\\" + Continent + "\\");
                    }
                }
            }
        }

        // Create and load from file if exisiting
        private void LoadChunk(float x, float y)
        {
            GraphChunk gc = GetChunkAt(x, y);
            if (gc == null)
            {
                int ix, iy;
                GetChunkCoord(x, y, out ix, out iy);

                float base_x, base_y;
                GetChunkBase(ix, iy, out base_x, out base_y);

                gc = new GraphChunk(base_x, base_y, ix, iy);
                gc.LRU = LRU++;


                CheckForChunkEvict();

                gc.Load(BaseDir + "\\" + Continent + "\\");
                chunks.Set(ix, iy, gc);
                ActiveChunks.Add(gc);

            }
        }

        public Spot AddSpot(Spot s)
        {
            LoadChunk(s.X, s.Y);
            GraphChunk gc = GetChunkAt(s.X, s.Y);
            return gc.AddSpot(s);
        }

        // Connect according to MPQ data
        public Spot AddAndConnectSpot(Spot s)
        {
            s = AddSpot(s);
            List<Spot> close = FindAllSpots(s.location, MaxStepLength);
            if (!s.GetFlag(Spot.FLAG_MPQ_MAPPED))
            {

                foreach (Spot cs in close)
                {
                    if (cs.HasPathTo(this, s) && s.HasPathTo(this, cs) ||
                        cs.IsBlocked())
                    {
                    }
                    else if (!triangleWorld.IsStepBlocked(s.X, s.Y, s.Z, cs.X, cs.Y, cs.Z,
                                                     toonHeight, toonSize, null))
                    {
                        float mid_x = (s.X + cs.X) / 2;
                        float mid_y = (s.Y + cs.Y) / 2;
                        float mid_z = (s.Z + cs.Z) / 2;
                        float stand_z;
                        int flags;
                        if (triangleWorld.FindStandableAt(mid_x, mid_y,
                                                          mid_z - WantedStepLength * .75f, mid_z + WantedStepLength * .75f,
                                                          out stand_z, out flags, toonHeight, toonSize))
                        {
                            s.AddPathTo(cs);
                            cs.AddPathTo(s);
                        }
                    }
                }
            }
            return s;
        }

        public Spot GetSpot(float x, float y, float z)
        {
            LoadChunk(x, y);
            GraphChunk gc = GetChunkAt(x, y);
            return gc.GetSpot(x, y, z);
        }

        public Spot GetSpot2D(float x, float y)
        {
            LoadChunk(x, y);
            GraphChunk gc = GetChunkAt(x, y);
            return gc.GetSpot2D(x, y);
        }

        public Spot GetSpot(Location l)
        {
            if (l == null) return null;
            return GetSpot(l.X, l.Y, l.Z);
        }



        // this can be slow...

        public Spot FindClosestSpot(Location l_d)
        {
            return FindClosestSpot(l_d, 30.0f, null);
        }

        public Spot FindClosestSpot(Location l_d, Set<Spot> Not)
        {
            return FindClosestSpot(l_d, 30.0f, Not);
        }


        public Spot FindClosestSpot(Location l, float max_d)
        {
            return FindClosestSpot(l, max_d, null);
        }

        // this can be slow...
        public Spot FindClosestSpot(Location l, float max_d, Set<Spot> Not)
        {
            Spot closest = null;
            float closest_d = 1E30f;
            int d = 0;
            while ((float)d <= max_d + 0.1f)
            {
                for (int i = -d; i <= d; i++)
                {
                    float x_up = l.X + (float)d;
                    float x_dn = l.X - (float)d;
                    float y_up = l.Y + (float)d;
                    float y_dn = l.Y - (float)d;

                    Spot s0 = GetSpot2D(x_up, l.Y + i);
                    Spot s2 = GetSpot2D(x_dn, l.Y + i);

                    Spot s1 = GetSpot2D(l.X + i, y_dn);
                    Spot s3 = GetSpot2D(l.X + i, y_up);
                    Spot[] sv = { s0, s1, s2, s3 };
                    foreach (Spot s in sv)
                    {
                        Spot ss = s;
                        while (ss != null)
                        {
                            float di = ss.GetDistanceTo(l);
                            if (di < max_d && !ss.IsBlocked() &&
                                (di < closest_d))
                            {
                                closest = ss;
                                closest_d = di;
                            }
                            ss = ss.next;
                        }
                    }
                }

                if (closest_d < d) // can't get better
                {
                    //Log("Closest2 spot to " + l + " is " + closest);
                    return closest;
                }
                d++;
            }
            //Log("Closest1 spot to " + l + " is " + closest);
            return closest;
        }

        public List<Spot> FindAllSpots(Location l, float max_d)
        {
            List<Spot> sl = new List<Spot>();

            int d = 0;
            while ((float)d <= max_d + 0.1f)
            {
                for (int i = -d; i <= d; i++)
                {
                    float x_up = l.X + (float)d;
                    float x_dn = l.X - (float)d;
                    float y_up = l.Y + (float)d;
                    float y_dn = l.Y - (float)d;

                    Spot s0 = GetSpot2D(x_up, l.Y + i);
                    Spot s2 = GetSpot2D(x_dn, l.Y + i);

                    Spot s1 = GetSpot2D(l.X + i, y_dn);
                    Spot s3 = GetSpot2D(l.X + i, y_up);
                    Spot[] sv = { s0, s1, s2, s3 };
                    foreach (Spot s in sv)
                    {
                        Spot ss = s;
                        while (ss != null)
                        {
                            float di = ss.GetDistanceTo(l);
                            if (di < max_d)
                            {
                                sl.Add(ss);
                            }
                            ss = ss.next;
                        }
                    }
                }
                d++;
            }
            return sl;
        }

        public List<Spot> FindAllSpots(float min_x, float min_y, float max_x, float max_y)
        {
            // hmm, do it per chunk
            List<Spot> l = new List<Spot>();
            for (float mx = min_x; mx <= max_x + GraphChunk.CHUNK_SIZE - 1; mx += GraphChunk.CHUNK_SIZE)
            {
                for (float my = min_y; my <= max_y + GraphChunk.CHUNK_SIZE - 1; my += GraphChunk.CHUNK_SIZE)
                {
                    LoadChunk(mx, my);
                    GraphChunk gc = GetChunkAt(mx, my);
                    List<Spot> sl = gc.GetAllSpots();
                    foreach (Spot s in sl)
                    {
                        if (s.X >= min_x && s.X <= max_x &&
                           s.Y >= min_y && s.Y <= max_y)
                        {
                            l.Add(s);
                        }
                    }
                }
            }
            return l;
        }



        public Spot TryAddSpot(Spot wasAt, Location isAt)
        {
            Spot isAtSpot = FindClosestSpot(isAt, WantedStepLength);
            if (isAtSpot == null)
            {
                isAtSpot = GetSpot(isAt);
                if (isAtSpot == null)
                {
                    Spot s = new Spot(isAt);
                    s = AddSpot(s);
                    isAtSpot = s;
                }
                if (isAtSpot.GetFlag(Spot.FLAG_BLOCKED))
                {
                    isAtSpot.SetFlag(Spot.FLAG_BLOCKED, false);
                    Log("Cleared blocked flag");
                }
                if (wasAt != null)
                {
                    wasAt.AddPathTo(isAtSpot);
                    isAtSpot.AddPathTo(wasAt);
                }

                List<Spot> sl = FindAllSpots(isAtSpot.location, MaxStepLength);
                Log("Learned a new spot at " + isAtSpot.location + " connected to " + sl.Count + " other spots");
                foreach (Spot other in sl)
                {
                    if (other != isAtSpot)
                    {
                        other.AddPathTo(isAtSpot);
                        isAtSpot.AddPathTo(other);
                        Log("  connect to " + other.location);
                    }
                }

                wasAt = isAtSpot;
            }
            else
            {
                if (wasAt != null && wasAt != isAtSpot)
                {
                    // moved to an old spot, make sure they are connected
                    wasAt.AddPathTo(isAtSpot);
                    isAtSpot.AddPathTo(wasAt);
                }
                wasAt = isAtSpot;
            }

            return wasAt;
        }

        private bool LineCrosses(Location line0, Location line1, Location point)
        {
            float LineMag = line0.GetDistanceTo(line1); // Magnitude( LineEnd, LineStart );

            float U =
                (((point.X - line0.X) * (line1.X - line0.X)) +
                  ((point.Y - line0.Y) * (line1.Y - line0.Y)) +
                  ((point.Z - line0.Z) * (line1.Z - line0.Z))) /
                (LineMag * LineMag);

            if (U < 0.0f || U > 1.0f) return false;

            float InterX = line0.X + U * (line1.X - line0.X);
            float InterY = line0.Y + U * (line1.Y - line0.Y);
            float InterZ = line0.Z + U * (line1.Z - line0.Z);

            float Distance = point.GetDistanceTo(new Location(InterX, InterY, InterZ));
            if (Distance < 0.5f) return true;
            return false;
        }

        public void MarkBlockedAt(Location loc)
        {
            Spot s = new Spot(loc);
            s = AddSpot(s);
            s.SetFlag(Spot.FLAG_BLOCKED, true);
            // Find all paths leading though this one

            List<Spot> sl = FindAllSpots(loc, 5.0f);
            foreach (Spot sp in sl)
            {
                List<Location> paths = sp.GetPaths();
                foreach (Location to in paths)
                {
                    if (LineCrosses(sp.location, to, loc))
                    {
                        sp.RemovePathTo(to);
                    }
                }
            }

        }

        public void BlacklistStep(Location from, Location to)
        {
            Spot froms = GetSpot(from);
            if (froms != null)
                froms.RemovePathTo(to);
        }

        public void MarkStuckAt(Location loc, float heading)
        {
            // TODO another day...
            Location inf = loc.InFrontOf(heading, 1.0f);
            MarkBlockedAt(inf);

            // TODO
        }


        //////////////////////////////////////////////////////
        // Searching
        //////////////////////////////////////////////////////



        public Path InterpolatePath(Location from, Location to)
        {
            Path p = new Path();

            //Log("Interpolate from " + from + " to " + to);
            Location prev = from;

            int maxSteps = 100; // max 100 steps
            for (int i = 0; i <= maxSteps; i++)
            {
                float distance = prev.GetDistanceTo(to);
                int ss = (int)(distance / WantedStepLength);
                if (ss == 0) return p;

                float dx = (to.X - prev.X) / (float)ss;
                float dy = (to.Y - prev.Y) / (float)ss;
                float dz = (to.Z - prev.Z) / (float)ss;

                Location rover = new Location(
                    prev.X + dx,
                    prev.Y + dy,
                    prev.Z + dz);

                Spot closest = FindClosestSpot(rover, MaxStepLength);
                if (closest != null && closest.GetDistanceTo(rover) <= WantedStepLength)
                {
                    // something is there
                    // TODO do something
                }
                prev = rover;
                p.AddLast(rover);
            }
            return p;
        }

        public void Paint()
        {
            if (paint == null) return;
            ICollection<GraphChunk> l = chunks.GetAllElements();
            foreach (GraphChunk gc in l)
            {
                List<Spot> spots = gc.GetAllSpots();
                foreach (Spot s in spots)
                {
                    PaintSpot(s);

                    for (int i = 0; i < s.n_paths; i++)
                    {
                        float x, y, z;
                        s.GetPath(i, out x, out y, out z);
                        paint.PaintPath(s.X, s.Y, s.Z, x, y, z);
                    }
                }
            }
        }

        void PaintSpot(Spot s)
        {
            if (paint != null)
                paint.AddMarker(s.X, s.Y, s.Z);
        }

        void PaintBigSpot(Spot s)
        {
            if (paint != null)
                paint.AddBigMarker(s.X, s.Y, s.Z);
        }


        float TurnCost(Spot from, Spot to)
        {
            Spot prev = from.traceBack;
            if (prev == null) return 0.0f;
            return TurnCost(prev.X, prev.Y, prev.Z, from.X, from.Y, from.Z, to.X, to.Y, to.Z);

        }

        float TurnCost(float x0, float y0, float z0, float x1, float y1, float z1, float x2, float y2, float z2)
        {
            float v1x = x1 - x0;
            float v1y = y1 - y0;
            float v1z = z1 - z0;
            float v1l = (float)Math.Sqrt(v1x * v1x + v1y * v1y + v1z * v1z);
            v1x /= v1l;
            v1y /= v1l;
            v1z /= v1l;

            float v2x = x2 - x1;
            float v2y = y2 - y1;
            float v2z = z2 - z1;
            float v2l = (float)Math.Sqrt(v2x * v2x + v2y * v2y + v2z * v2z);
            v2x /= v2l;
            v2y /= v2l;
            v2z /= v2l;

            float ddx = v1x - v2x;
            float ddy = v1y - v2y;
            float ddz = v1z - v2z;
            return (float)Math.Sqrt(ddx * ddx + ddy * ddy + ddz * ddz);
        }

        // return null if failed or the last spot in the path found

        int searchID = 0;
        private Spot search(Spot src, Spot dst,
                            Location realDst,
                            float minHowClose, bool AllowInvented,
                            ILocationHeuristics locationHeuristics)
        {
            searchID++;
            int count = 0;
            int prevCount = 0;
            int currentSearchID = searchID;
            float heuristicsFactor = 1.3f;
            System.DateTime pre = System.DateTime.Now;
            System.DateTime lastSpam = pre;

            // lowest first queue
            PriorityQueue<Spot, float> q = new PriorityQueue<Spot, float>(); // (new SpotSearchComparer(dst, score)); ;
            q.Enqueue(src, -src.GetDistanceTo(dst) * heuristicsFactor);
            Spot BestSpot = null;

            //Set<Spot> closed      = new Set<Spot>();
            //SpotData<float> score = new SpotData<float>();

            src.SearchScoreSet(currentSearchID, 0.0f);
            src.traceBack = null;

            // A* -ish algorithm

            while (q.Count != 0) // && count < 100000)
            {
                float prio;
                Spot spot = q.Dequeue(out prio); // .Value; 
                //q.Remove(spot);


                if (spot.SearchIsClosed(currentSearchID)) continue;
                spot.SearchClose(currentSearchID);

                if (count % 100 == 0)
                {
                    System.TimeSpan span = System.DateTime.Now.Subtract(lastSpam);
                    if (span.Seconds != 0 && BestSpot != null)
                    {
                        Thread.Sleep(50); // give glider a chance to stop us
                        int t = span.Seconds * 1000 + span.Milliseconds;
                        if (t == 0)
                            Log("searching.... " + (count + 1) + " d: " + BestSpot.location.GetDistanceTo(realDst));
                        else
                            Log("searching.... " + (count + 1) + " d: " + BestSpot.location.GetDistanceTo(realDst) + " " + (count - prevCount) * 1000 / t + " steps/s");
                        lastSpam = System.DateTime.Now;
                        prevCount = count;
                    }
                }
                count++;


                if (spot.Equals(dst) || spot.location.GetDistanceTo(realDst) < minHowClose)
                {
                    System.TimeSpan ts = System.DateTime.Now.Subtract(pre);
                    int t = ts.Seconds * 1000 + ts.Milliseconds;
                    /*if(t == 0)
                        Log("  search found the way there. " + count); 
                    else
                        Log("  search found the way there. " + count + " " + (count * 1000) / t + " steps/s");
                      */
                    return spot; // got there
                }

                if (BestSpot == null ||
                   spot.location.GetDistanceTo(realDst) < BestSpot.location.GetDistanceTo(realDst))
                {
                    BestSpot = spot;
                }
                {
                    System.TimeSpan ts = System.DateTime.Now.Subtract(pre);
                    if (ts.Seconds > 15)
                    {
                        Log("too long search, aborting");
                        break;
                    }
                }

                float src_score = spot.SearchScoreGet(currentSearchID);

                //GContext.Main.Log("inspect: " + c + " score " + s);


                int new_found = 0;
                List<Spot> ll = spot.GetPathsToSpots(this);
                foreach (Spot to in ll)
                {
                    //Spot to = GetSpot(l);

                    if (to != null && !to.IsBlocked() && !to.SearchIsClosed(currentSearchID))
                    {
                        float old_score = 1E30f;

                        float new_score = src_score + spot.GetDistanceTo(to) + TurnCost(spot, to);
                        if (locationHeuristics != null)
                            new_score += locationHeuristics.Score(spot.X, spot.Y, spot.Z);
                        if (to.GetFlag(Spot.FLAG_WATER))
                            new_score += 30;

                        if (to.SearchScoreIsSet(currentSearchID))
                        {
                            old_score = to.SearchScoreGet(currentSearchID);
                        }

                        if (new_score < old_score)
                        {
                            // shorter path to here found
                            to.traceBack = spot;
                            //if (q.Contains(to)) 
                            //   q.Remove(to); // very sloppy to not dequeue it
                            to.SearchScoreSet(currentSearchID, new_score);
                            q.Enqueue(to, -(new_score + to.GetDistanceTo(dst) * heuristicsFactor));
                            new_found++;
                        }
                    }
                }

                //hmm search the triangles :p
                if (!spot.GetFlag(Spot.FLAG_MPQ_MAPPED))
                {

                    float PI = (float)Math.PI;

                    spot.SetFlag(Spot.FLAG_MPQ_MAPPED, true);
                    for (float a = 0; a < PI * 2; a += PI / 8)
                    {
                        float nx = spot.X + (float)Math.Sin(a) * WantedStepLength;// *0.8f;
                        float ny = spot.Y + (float)Math.Cos(a) * WantedStepLength;// *0.8f;
                        Spot s = GetSpot(nx, ny, spot.Z);
                        if (s == null)
                            s = FindClosestSpot(new Location(nx, ny, spot.Z), MinStepLength); // TODO: this is slow
                        if (s != null)
                        {

                        }
                        else
                        {
                            float new_z;
                            int flags;
                            // gogo find a new one
                            //GContext.Main.Log("gogo brave new world");
                            if (!triangleWorld.FindStandableAt(nx, ny,
                                                               spot.Z - WantedStepLength * .75f, spot.Z + WantedStepLength * .75f,
                                                               out new_z, out flags, toonHeight, toonSize))
                            {
                                Spot blocked = new Spot(nx, ny, spot.Z);
                                blocked.SetFlag(Spot.FLAG_BLOCKED, true);
                                AddSpot(blocked);
                            }
                            else
                            {
                                s = FindClosestSpot(new Location(nx, ny, new_z), MinStepLength);
                                if (s == null)
                                {
                                    if (!triangleWorld.IsStepBlocked(spot.X, spot.Y, spot.Z, nx, ny, new_z,
                                                                     toonHeight, toonSize, null))
                                    {

                                        Spot n = new Spot(nx, ny, new_z);
                                        Spot to = AddAndConnectSpot(n);
                                        if ((flags & ChunkedTriangleCollection.TriangleFlagDeepWater) != 0)
                                        {
                                            to.SetFlag(Spot.FLAG_WATER, true);
                                        }
                                        if (to != n || to.SearchIsClosed(currentSearchID))
                                        {
                                            // GContext.Main.Log("/sigh");
                                        }
                                        else
                                        {
                                            // There should be a path from source to this one now
                                            if (spot.HasPathTo(to.location))
                                            {
                                                float old_score = 1E30f;

                                                float new_score = src_score + spot.GetDistanceTo(to) + TurnCost(spot, to);
                                                if (locationHeuristics != null)
                                                    new_score += locationHeuristics.Score(spot.X, spot.Y, spot.Z);


                                                if (to.GetFlag(Spot.FLAG_WATER))
                                                    new_score += 30;

                                                if (to.SearchScoreIsSet(currentSearchID))
                                                {
                                                    old_score = to.SearchScoreGet(currentSearchID);
                                                }

                                                if (new_score < old_score)
                                                {
                                                    // shorter path to here found
                                                    to.traceBack = spot;
                                                    //if (q.Contains(to)) 
                                                    //    q.Remove(to);
                                                    to.SearchScoreSet(currentSearchID, new_score);
                                                    q.Enqueue(to, -(new_score + to.GetDistanceTo(dst) * heuristicsFactor));
                                                    new_found++;
                                                }
                                            }
                                            else
                                            {
                                                // woot! I added a new one and it is not connected!?!?
                                                //GContext.Main.Log("/cry");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

            }
            {
                System.TimeSpan ts = System.DateTime.Now.Subtract(pre);
                int t = ts.Seconds * 1000 + ts.Milliseconds; if (t == 0) t = 1;
                Log("  search failed. " + (count * 1000) / t + " steps/s");
                PaintBigSpot(BestSpot);
            }
            return BestSpot; // :(
        }


        private List<Spot> FollowTraceBack(Spot from, Spot to)
        {
            List<Spot> path = new List<Spot>();
            int count = 0;

            Spot r = to;
            path.Insert(0, to); // add last
            while (r != null)
            {
                Spot s = r.traceBack;

                if (s != null)
                {
                    path.Insert(0, s); // add first
                    r = s;
                    if (r == from) r = null;  // fount source
                }
                else
                    r = null;
                count++;
            }
            path.Insert(0, from); // add first
            return path;

        }

        public bool IsUnderwaterOrInAir(Location l)
        {
            int flags;
            float z;
            if (triangleWorld.FindStandableAt(l.X, l.Y, l.Z - 50.0f, l.Z + 5.0f, out z, out  flags, toonHeight, toonSize))
            {
                if ((flags & ChunkedTriangleCollection.TriangleFlagDeepWater) != 0)
                    return true;
                else
                    return false;
            }
            //return true; 
            return false;
        }

        public Path CreatePath(Spot from, Spot to, Location realDst,
                               float minHowClose, bool AllowInvented,
                               ILocationHeuristics locationHeuristics)
        {

            Spot newTo = search(from, to, realDst, minHowClose, AllowInvented,
                                locationHeuristics);
            if (newTo != null)
            {
                if (newTo.GetDistanceTo(to) <= minHowClose)
                {
                    List<Spot> path = FollowTraceBack(from, newTo);
                    return new Path(path);
                }
            }
            return null;
        }

        public Path CreatePath(Location fromLoc, Location toLoc,
                               float howClose)
        {
            return CreatePath(fromLoc, toLoc, howClose, null);
        }

        public Path CreatePath(Location fromLoc, Location toLoc,
                               float howClose,
                               ILocationHeuristics locationHeuristics)
        {
            GSpellTimer t = new GSpellTimer(0);
            Spot from = FindClosestSpot(fromLoc, MinStepLength);
            Spot to = FindClosestSpot(toLoc, MinStepLength);

            if (from == null)
            {
                from = AddAndConnectSpot(new Spot(fromLoc));
            }
            if (to == null)
            {
                to = AddAndConnectSpot(new Spot(toLoc));
            }

            Path rawPath = CreatePath(from, to, to.location, howClose, true, locationHeuristics);


            if (rawPath != null && paint != null)
            {
                Location prev = null;
                for (int i = 0; i < rawPath.Count(); i++)
                {
                    Location l = rawPath.Get(i);
                    paint.AddBigMarker(l.X, l.Y, l.Z);
                    if (prev != null)
                    {
                        paint.PaintPath(l.X, l.Y, l.Z + 3, prev.X, prev.Y, prev.Z + 3);
                    }
                    prev = l;
                }
            }
            //GContext.Main.Log("CreatePath took " + -t.TicksLeft);
            if (rawPath == null)
            {
                return null;
            }
            else
            {
                Location last = rawPath.GetLast();
                if (last.GetDistanceTo(toLoc) > 1.0)
                    rawPath.AddLast(toLoc);
            }
            return rawPath;
        }

        private void Log(String s)
        {
            //Console.WriteLine(s); 
            BoogieCore.Log(LogType.System, s);
        }
    }

    // END PathGrapgh
    ///////////////////////////////////////////////////////










}