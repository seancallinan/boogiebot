using System;
using System.Collections.Generic;
using System.Text;

namespace BoogieBot.Common
{
    /// <summary>Object to access WMOAreaTable.dbc</summary>
    public class WMOAreaTable : DBCFile
    {
        public WMOAreaTable() : base(@"DBFilesClient\WMOAreaTable.dbc")
        {
        }
    }
}
