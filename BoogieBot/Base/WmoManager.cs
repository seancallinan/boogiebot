using System;
using System.Collections.Generic;
using System.Text;

namespace BoogieBot.Common
{
    /// <summary>Manages WMO Data. Provides numerous useful methods to query wmo data, and does so by looking up (and if nessessary, loading in) the appropriate wmo.</summary>
    public class WMOManager
    {
        private List<WMO> wmos;

        public WMOManager()
        {
            wmos = new List<WMO>();
        }

        // Notify the wmo manager that we have just zoned.
        public void zoned()
        {
            // If we just zoned to a different map, do maintenance and flush the current wmo list
            doMaintenance(true);
        }

        // Do maintenance
        private void doMaintenance(Boolean flush)
        {
            // Delete all wmos off the list
            if (flush)
            {
                wmos = new List<WMO>();
            }

            // If the list is getting long
            if (wmos.Count > 100)
            {
                // Prune it.
                wmos = new List<WMO>();
            }
        }

        // DEBUG METHODS /////////////////////////////////////////////////////////////////
        public int DEBUG_wmoCount()
        {
            return wmos.Count;
        }
    }
}
