using System;
using System.Collections.Generic;
using System.Text;

namespace BoogieBot.Common
{
    /// <summary>WMO. Represents a World Map Object.</summary>
    public class WMO
    {
        private WMORootFile root;           // WMO Root
        private WMOGroupFile[] groups;      // WMO Groups

        public WMO(String filename)
        {
            root = new WMORootFile(filename);

            int num = root.groupInfo.Length;

            groups = new WMOGroupFile[num];

            // Load in Group Files
            for (int i = 0; i < num; i++)
            {
                StringBuilder sb = new StringBuilder(filename);
                sb.Remove(sb.Length - 4, 4); // Trim ".wmo"
                sb.Append("_");
                sb.Append(String.Format("{0:D3}", i));
                sb.Append(".wmo");

                groups[i] = new WMOGroupFile(sb.ToString());
            }
        }
    }
}