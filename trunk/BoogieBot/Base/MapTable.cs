using System;
using System.Collections.Generic;
using System.Text;

namespace BoogieBot.Common
{
    /// <summary>Object to access Map.dbc</summary>
    public class MapTable : DBCFile
    {
        public MapTable() : base(@"DBFilesClient\Map.dbc")
        {
        }

        /// <summary>Returns Map Name for a given MapID (eg, 0=eastern kingdoms, 1=kalimdor, etc)</summary>
        public String getMapName(uint mapid)
        {
            for (uint i = 0; i < wdbc_header.nRecords; i++)
            {
                uint id = getFieldAsUint32(i, 0);

                if (id == mapid)
                    return getStringForField(i, 1);
            }

            throw new Exception("mapid wasn't found");
        }
    }
}
