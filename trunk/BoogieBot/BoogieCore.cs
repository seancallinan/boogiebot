using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using Microsoft.Win32;

using Foole.Utils;

namespace BoogieBot.Common
{
    // This class keeps track of various important objects. This class cannot be instantiated.
    public abstract class BoogieCore
    {
        // Objects accessible to anyone
        public static IniReader configFile;            // For reading the bots .ini file
        public static World world;                     // For accessing and modifying World Objects, including the player
        public static AreaTable areaTable;             // For accessing areatable.dbc
        public static MapTable mapTable;               // For accessing map.dbc
        public static CallBackLog Log;                 // For sending logtext to the UI
        public static CallBackEvent Event;             // For sending events to the UI
        public static String wowPath;                  // Obvious
        public static WoWVersion wowVersion;           // WoW.exe / WoW.app version information
        public static WoWType wowType;                 // WoWtype. (win32, OSX - ppc or x86, etc)

        // Internal stuff
        private static Boolean inited = false;
        private static RealmListClient realmListClient;      // My Pvt handle to the RealmListClient
        private static WorldServerClient worldServerClient;  // My Pvt handle to the WorldServerClient
        private static Player player;                   // For accessing anything Player related.

        private static String BoogieBotConfigFileName = "boogiebot.ini";

        // Initialize the Core :D
        public static void InitCore(CallBackLog l, CallBackEvent e)
        {
            // Can't run this more than once
            if (inited) return;

            // We need to be able to Log stuff
            if (l == null) return;
            if (e == null) return;

            Log = l;
            Event = e;

            Log(LogType.System, "BoogieBot.dll Initializing...");

            // Initialize everything
            try
            {
                if (!File.Exists(Environment.CurrentDirectory + "/" + BoogieCore.BoogieBotConfigFileName))
                    throw new FileNotFoundException("Configuration file not found.", BoogieCore.BoogieBotConfigFileName);

                configFile = new IniReader(Environment.CurrentDirectory + "/boogiebot.ini");

                // NOTE: Set any OS specific variables so things can be done differently later, ie. Windows or Linux, etc.
                OperatingSystem os = Environment.OSVersion;
                switch(os.Platform)
                {
                    case PlatformID.Win32Windows:
                        Log(LogType.System, "> Operating System: Windows");
                        break;
                    case PlatformID.Win32NT:
                        Log(LogType.System, "> Operating System: Windows NT");
                        break;
                    case PlatformID.Unix:
                        Log(LogType.System, "> Operating System: Unix");
                        break;
                }
                Log(LogType.System, "> OS Version: {0}.{1}  (Build: {2})  ({3})", os.Version.Major, os.Version.Minor, os.Version.Build, os.ServicePack);

                // Find WoW's Folder
                wowPath = BoogieCore.getWowPath();
                Log(LogType.System, "> WowPath: {0}", wowPath);

                // Find WoW's Version
                wowVersion = BoogieCore.getWoWVersion();
                Log(LogType.System, "> WoW Version: World of Warcraft {0}.{1}.{2}.{3} ({4}) Found!  Emulating this version.", wowVersion.major, wowVersion.minor, wowVersion.update, wowVersion.build, BoogieCore.WowTypeString);


                world = new World();
                player = new Player();

                //areaTable = new AreaTable();
                //mapTable = new MapTable();
            }
            catch (Exception ex)
            {
                // Bot Start up Failed. Log why, and rethrow the exception.
                Log(LogType.System, ex.Message);
                Log(LogType.System, ex.StackTrace);

                throw new Exception("BoogieBot.dll Init Failure.", ex);
            }

            inited = true;
            Log(LogType.System, "BoogieBot.dll Initialized.");
        }

        //// Initialize the Core (WoWChat version) This excludes almost everything... since wowchat doesn't need much :P
        //public static void InitCoreWoWChat(CallBackLog l, CallBackEvent e)
        //{
        //    // Can't run this more than once
        //    if (inited) return;

        //    // We need to be able to Log stuff
        //    if (l == null) return;
        //    if (e == null) return;

        //    Log = l;
        //    Event = e;

        //    try
        //    {
        //        if (!File.Exists(Environment.CurrentDirectory + "/" + BoogieCore.WowChatConfigFileName))
        //            throw new FileNotFoundException("Configuration file not found.", BoogieCore.WowChatConfigFileName);

        //        configFile = new IniReader(Environment.CurrentDirectory + "/" + BoogieCore.WowChatConfigFileName);

        //        wowPath = BoogieCore.getWowPath();
        //        Log(LogType.System, "WowPath = {0}", wowPath);

        //        wowVersion = getWoWVersion();
        //        Log(LogType.System, "World of Warcraft {0}.{1}.{2}.{3} {4} Found! Emulating this version.", wowVersion.major, wowVersion.minor, wowVersion.update, wowVersion.build, BoogieCore.WowTypeString);

        //        world = new World();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Bot Start up Failed. Log why, and rethrow the exception.
        //        Log(LogType.System, ex.Message);
        //        Log(LogType.System, ex.StackTrace);

        //        throw ex;
        //    }

        //    inited = true;
        //    wowchat = true;
        //    Log(LogType.System, "WoWChat Initialized.");
        //}

        public static Boolean ConnectToRealmListServer()
        {
            if (!inited)
                throw new Exception("Run BoogieCore.Init() first.");

            if (realmListClient != null)
                throw new Exception("Already connected?");

            IPAddress RLAddr;

            string Address = configFile.ReadString("Connection", "Host", "us.logon.worldofwarcraft.com");
            int Port = configFile.ReadInteger("Connection", "Port", 3724);

            Regex DnsMatch = new Regex("[a-zA-Z]");

            if (DnsMatch.IsMatch(Address))
                RLAddr = Dns.GetHostEntry(Address).AddressList[0];
            else
                RLAddr = System.Net.IPAddress.Parse(Address);

            IPEndPoint RLDest = new IPEndPoint(RLAddr, Port);

            Log(LogType.System, "Attempting connection to Realm List Server at {0}.", Address);

            try
            {
                realmListClient = new RealmListClient();

                if (!realmListClient.Connect(RLDest))
                {
                    realmListClient = null;
                    return false;
                }

                if (!realmListClient.Logon())
                {
                    realmListClient = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                BoogieCore.Log(LogType.System, "Failed to maintain connection with realm list server. Details below:\n{0}", ex.Message);
                realmListClient = null;
                return false;
            }

            return true;
        }

        public static void ConnectToWorldServer(IPEndPoint ep)
        {
            if (!inited)
                throw new Exception("Run BoogieCore.Init() first.");

            if (worldServerClient != null)
                throw new Exception("Already connected?");

            if (realmListClient.mUsername.Length > 2 && realmListClient.K.Length >= 16)
                worldServerClient = new WorldServerClient(ep, realmListClient.mUsername, realmListClient.K);
            else
                Log(LogType.Error, "Unable to login to Game server! Unknown error occured. Check above!");
        }

        public static void Disconnect()
        {
            if (!inited)
                throw new Exception("Run BoogieCore.Init() first.");

            realmListClient = null;

            if (worldServerClient != null)
            {
                // Stop the WS thread and wait till it ends.
                worldServerClient.StopThread(false); // causes deadlocks atm.
                worldServerClient = null;
            }
        }


        // Properties
        public static RealmListClient RealmListClient { get { return realmListClient; } }
        public static WorldServerClient WorldServerClient { get { return worldServerClient; } }
        public static World World { get { return world; } }
        public static Player Player { get { return player; } }

        public static String WowTypeString
        {
            get
            {
                switch (BoogieCore.wowType)
                {
                    case WoWType.OSXppc:
                        return "OSX/ppc";
                    case WoWType.OSXx86:
                        return "OSX/x86";
                    case WoWType.Win32:
                        return "WIN32/x86";
                    default:
                        return "Unknown";
                }
            }
        }

        // Determine the Wow path. Will try to read it from the INI, or from the Registry. INI overrides Registry.
        // (ie. you may have multiple wow installations and want to use a specific one)
        private static String getWowPath()
        {
            String wowPath = configFile.ReadString("WoW", "Wowpath");

            //if(BoogieCore.wowType != WoWType.Win32) return;  // If its not windows wow, it won't be in the registry ;p
            // Okay maybe i'll keep that commented out; I just place my WoW.app into my windows wow folder LOL.

            if (wowPath.Equals(""))
            {
                RegistryKey rootKey = RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, "");
                RegistryKey blizKey = rootKey.OpenSubKey(@"SOFTWARE\Blizzard Entertainment\World of Warcraft");

                if(blizKey != null)
                    wowPath = (String)blizKey.GetValue("InstallPath");

                if (wowPath == null || wowPath == "")
                    throw new Exception("Couldn't determine wowpath! Please set it in your boogiebot.ini file.");
            }

            return wowPath;
        }

        // Determine what version of WoW we're running.
        private static WoWVersion getWoWVersion()
        {
            WoWType wowType = (WoWType)configFile.ReadInteger("WoW", "WowType");

            switch (wowType)
            {
                case WoWType.OSXppc:
                    BoogieCore.wowType = wowType;
                    return getWoWVersion_OSX();
                case WoWType.OSXx86:
                    BoogieCore.wowType = wowType;
                    return getWoWVersion_OSX();
                case WoWType.Win32:
                    BoogieCore.wowType = wowType;
                    return getWoWVersion_Win32();
                default:
                    throw new Exception("Please fix wowtype in your config file.");
            }
        }

        // Find Windows WoW Version using FileVersionInfo
        private static WoWVersion getWoWVersion_Win32()
        {
            try
            {
                FileVersionInfo wowExeInfo = FileVersionInfo.GetVersionInfo(wowPath + @"\WoW.exe");
                return new WoWVersion((byte)wowExeInfo.FileMajorPart, (byte)wowExeInfo.FileMinorPart, (byte)wowExeInfo.FileBuildPart, (ushort)wowExeInfo.FilePrivatePart);
            }
            catch (Exception)
            {
                throw new Exception("Couldn't open wow.exe. Check that it exists, and wowpath is set correctly.");
            }
        }

        // Find OSX WoW Version, by reading in World of Warcraft.app's Info.plist. Fuck I hate XML though.
        private static WoWVersion getWoWVersion_OSX()
        {
            XmlTextReader reader;

            try
            {
                reader = new XmlTextReader(BoogieCore.wowPath + @"\World of Warcraft.app\Contents\Info.plist");
            }
            catch
            {
                throw new Exception("Couldn't open WoW.app. Check that it exists, and wowpath is set correctly.");
            }

            String previousText = "";

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        break;

                    case XmlNodeType.Text:
                        if (previousText.Equals("BlizzardFileVersion"))
                            return new WoWVersion(reader.Value);
                        previousText = reader.Value;
                        break;
                }
            }

            throw new Exception("Couldn't figure out World of Warcraft.app's version.");
        }
    }
    // End of BoogieCore class!!


    // Delegates - Used to make calls to the UI from this .dll
    public delegate void CallBackEvent(Event e);
    public delegate void CallBackLog(LogType logType, string format, params object[] parameters);

    // Different Types of WoW (platform/OS combiantions)
    public enum WoWType
    {
        Win32 = 0,
        OSXppc = 1,
        OSXx86 = 2
    }

    // Log types. Feel free to add more!
    public enum LogType
    {
        Error,              // Error Message. (This should be saved for fatal messages?)
        System,             // System Message
        SystemDebug,        // System Debug Message
        NeworkComms,        // Network Communications
        FileDebug           // File reading debug info
    }

    // Event types. You'll definitely be adding more.
    public enum EventType
    {
        EVENT_REALMLIST,
        EVENT_CHAR_LIST,
        EVENT_CHAR_CREATE_RESULT,
        EVENT_CHAT,
        EVENT_CHANNEL_JOINED,
        EVENT_CHANNEL_LEFT,
        EVENT_SELF_MOVED,
        EVENT_LOCATION_UPDATE,
        EVENT_FRIENDS_LIST,
        EVENT_IGNORE_LIST,
        EVENT_FRIEND_STATUS,
        EVENT_NAMEQUERY_RESPONSE
    }

    public class Event
    {
        public EventType eventType;
        public string eventTime;
        public object[] eventArgs;

        public Event(EventType type, string time, params object[] parms)
        {
            eventType = type;
            eventTime = time;
            eventArgs = parms;
        }
    }

    public struct WoWVersion
    {
        public WoWVersion(byte a, byte b, byte c, ushort d)
        {
            major = a; minor = b; update = c; build = d;
        }

        public WoWVersion(String versionString)
        {
            String[] versionParts = versionString.Split(new char[] {'.'});
            Byte.TryParse(versionParts[0], out major);
            Byte.TryParse(versionParts[1], out minor);
            Byte.TryParse(versionParts[2], out update);
            UInt16.TryParse(versionParts[3], out build);
        }

        // DON'T CHANGE THE TYPE ON THESE FIELDS
        // (otherwise u need to fix RealmListClient.Auth.cs)
        public byte major;
        public byte minor;
        public byte update;
        public ushort build;
    }
}