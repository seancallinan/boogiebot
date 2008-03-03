using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace BoogieBot.Common
{
    /// <summary>Manages all opened MPQ Archives and allows easy access to files.</summary>
    public class MPQManager
    {
        /// <summary>Creates MPQManager object.</summary>
        /// <param name="wowpath">Full path to World of Warcraft game folder.</param>
        public MPQManager(String wowpath)
        {
        }

        /// <summary>Searches for specified file in all opened MPQ Archives, and returns an MPQFile object to access it.</summary>
        /// <param name="filename">Filename to search for in MPQs, and open</param>
        /// <param name="usepatch">Include patch.mpq in search?</param>
        /*public MPQFile Open(String filename, Boolean usepatch)
        {
            foreach (MPQArchive mpqArchive in archiveList)
            {
                if (!usepatch & mpqArchive.Filename.Equals("patch.MPQ"))
                {
                    BoogieCore.Log(LogType.SystemDebug, "MPQManager: Skipping patch.MPQ");
                    continue;
                }

                try
                {
                    return mpqArchive.OpenFile(filename);
                }
                catch (FileNotFoundException)
                {
                    // This is normal here, ignore it.
                    //BoogieCore.Log(LogType.SystemDebug, "MPQManager: {0} was not found in {1}.", filename, mpqArchive.Filename);
                }
                catch (Exception ex)
                {
                    BoogieCore.Log(LogType.System, "MPQManager: Caught exception in {0}: {1}.  (Ignoring and continuing)", mpqArchive.Filename, ex.Message);
                }
            }

            BoogieCore.Log(LogType.System, "MPQManager: {0} was not found in ANY archives. Typo?", filename);
            // If we got to here, the file wasn't found in any open MPQ archives. Throw exception :(
            throw new FileNotFoundException("The file was not found.", filename);
        }*/

        /*public List<MPQArchive> ArchiveList
        {
            get { return ArchiveList; }
        }*/

        //private List<MPQArchive> archiveList = new List<MPQArchive>();

        // Probably shouldn't be hardcoded in, but stored in an .INI file??
        //private String[] mpqFileList = { "patch.MPQ", /*"expansion.MPQ",*/ "dbc.MPQ", "terrain.MPQ", "wmo.MPQ" };
    }
}

// List of all MPQs
// "backup.MPQ", "base.MPQ", "dbc.MPQ", "expansion.MPQ", "expansionLoc.MPQ", "fonts.MPQ", "interface.MPQ", "misc.MPQ", "model.MPQ", "patch.MPQ", "sound.MPQ", "speech.MPQ", "terrain.MPQ", "texture.MPQ", "wmo.MPQ" };