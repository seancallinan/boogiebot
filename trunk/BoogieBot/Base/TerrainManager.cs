using System;
using System.Collections.Generic;
using System.Text;

namespace BoogieBot.Common
{
    /// <summary>Manages Terrain Data. Provides numerous useful methods to query terrain data, and does so by looking up (and if nessessary, loading in) the appropriate maptile.</summary>
    public class TerrainManager
    {
        private List<MapTile> mapTiles;

        private static float TILESIZE = 533.33333f;
        private static float ZEROPOINT = 32.0f * TILESIZE;

        public TerrainManager()
        {
            mapTiles = new List<MapTile>();
        }

        public String getLocationAsString(Coordinate c, uint mapid, uint zoneid)
        {
            String mapName = BoogieCore.mapTable.getMapName(mapid);
            String areaName = BoogieCore.areaTable.getAreaName(zoneid);

            return String.Format("{0}: {1}: ({2}, {3}, {4})", mapName, areaName, c.X, c.Y, c.Z);
        }

        public Coordinate getZ(Coordinate c)
        {
            // Make a new coordinate object so we don't modify the original
            Coordinate h = new Coordinate(c.X, c.Y, c.Z, c.O);
            h.Z = getZ(c.X, c.Y);
            return h;
        }

        public float getZ(float x, float y)
        {
            doMaintenance(false);

            int TileX = (int)(((0f - y) + TerrainManager.ZEROPOINT) / TerrainManager.TILESIZE);
            int TileZ = (int)(((0f - x) + TerrainManager.ZEROPOINT) / TerrainManager.TILESIZE);

            // Find the maptile on the list of loaded tiles.
            MapTile tile = findTile(TileX, TileZ);

            // Ask the maptile to get z for x,y
            return tile.getZ(x, y);
        }

        public float getWaterHeight(float x, float y)
        {
            doMaintenance(false);

            int TileX = (int)(((0f - y) + TerrainManager.ZEROPOINT) / TerrainManager.TILESIZE);
            int TileZ = (int)(((0f - x) + TerrainManager.ZEROPOINT) / TerrainManager.TILESIZE);

            // Find the maptile on the list of loaded tiles.
            MapTile tile = findTile(TileX, TileZ);

            // Ask the maptile to get z for x,y
            return tile.getWaterHeight(x, y);
        }

        // Notify the terrain manager that we have just zoned.
        public void zoned()
        {
            // If we just zoned to a different map, do maintenance and flush the current tile list
            doMaintenance(true);
        }

        // Finds Maptile x,z on the list
        private MapTile findTile(int x, int z)
        {
            foreach (MapTile mapTile in mapTiles)
            {
                if (mapTile.X == x && mapTile.Z == z)
                    return mapTile;
            }

            // Wasn't a tile we have currently Loaded? Load it in!!
            return loadTile(x, z);
        }

        // Loads a maptile in
        private MapTile loadTile(int x, int z)
        {
            String mapname = BoogieCore.mapTable.getMapName(BoogieCore.world.getMapID());

            MapTile tile = new MapTile(mapname, x, z);
            mapTiles.Add(tile);
            return tile;
        }

        // Do maintenance
        private void doMaintenance(Boolean flush)
        {
            // Delete all maptiles off the list
            if (flush)
            {
                mapTiles = new List<MapTile>();
            }

            // If the list is getting long
            if (mapTiles.Count > 100)
            {
                // Prune it.
                mapTiles = new List<MapTile>();
            }
        }

        // DEBUG METHODS /////////////////////////////////////////////////////////////////
        public int DEBUG_TileCount()
        {
            return mapTiles.Count;
        }
    }
}
