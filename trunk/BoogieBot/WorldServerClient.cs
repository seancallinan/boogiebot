#define LOG

using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Xml;
using System.Timers;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Resources;

using Foole.Crypt;
using Foole.Utils;
using Foole.WoW;


namespace BoogieBot.Common
{
    public partial class WorldServerClient
    {
        #region Member variables

        [DllImport("winmm.dll", EntryPoint = "timeGetTime")]
        public static extern uint MM_GetTime();
        private UInt32 LastUpdateTime = 0;

        public Thread WorldThread = null;

        private bool isRunning;

        private string mUsername;

        protected WoWCrypt mCrypt = new WoWCrypt();
        private int mSendCryptSize;
        private int mRecvCryptSize;
        private byte[] Key;

        private UInt32 ServerSeed;
        private UInt32 ClientSeed;
        private Random random = new Random();
        private Socket mSocket;
        System.Timers.Timer PingTimer = new System.Timers.Timer();
        System.Timers.Timer MoveUpdateTimer = new System.Timers.Timer();

        // Ping stuffs
        private UInt32 Ping_Seq;
        private UInt32 Ping_Req_Time;
        private UInt32 Ping_Res_Time;
        public UInt32 Latency;

        private ArrayList Objects = new ArrayList();
        private Hashtable EntryList = new Hashtable();
        private Hashtable EntryQueue = new Hashtable();

#if (LOG)
        private TextWriter tw = File.CreateText("packets.txt");
#endif

        public Character[] characterList;


        // Ah temp vars
        bool SentHello = false;
        UInt32 AHEntry = 0;

        Pather.PPather pather = null;

        // TILE stuff

        static int TilesCount = 64;
        static float TileSize = 533.33333f;
        float _maxY = (TilesCount*TileSize/2);
        float _maxX = (TilesCount*TileSize/2);
        float _minY = (-TilesCount*TileSize/2);
        float _minX = (-TilesCount*TileSize/2);

        static int CellsPerTile = 8;
        float _cellSize = (TileSize / CellsPerTile);

        #endregion
        public class UpdateThread 
        {
            UInt32 TimeNow = MM_GetTime();
            UInt32 LastUpdateTime = MM_GetTime();


            public void Start()
            {
                while (true)
                {
                    Thread.Sleep(330);
                    if (BoogieCore.WorldServerClient == null)
                        continue;

                    TimeNow = MM_GetTime();
                    BoogieCore.WorldServerClient.UpdatePosition(TimeNow - LastUpdateTime);
                    LastUpdateTime = MM_GetTime();
                    if (BoogieCore.world.getPlayerObject() != null)
                        if (BoogieCore.world.getPlayerObject().GetCoordinates() != null)
                            BoogieCore.WorldServerClient.SendMoveHeartBeat();
                }

            }
        }
        public WorldServerClient(IPEndPoint ep, string username, byte[] key)
        {
            isRunning = false;

            mSendCryptSize = 6; // 2 length + 4 opcode
            mRecvCryptSize = 2; // 2 opcode

            mUsername = username;
            Key = key;

            try
            {
                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                mSocket.Connect(ep);
            }
            catch(SocketException ex)
            {
                BoogieCore.Log(LogType.Error, "Failed to connect to realm: {0}", ex.Message);
                return;
            }

            isRunning = true;


            WorldThread = new Thread(new ThreadStart(this.Start));
            WorldThread.Name = "WS Client Thread";
            WorldThread.IsBackground = true;
            LastUpdateTime = MM_GetTime();
            WorldThread.Start();

            BoogieCore.Log(LogType.System, "{0}: WS Thread Started", Time.GetTime());
        }

        private void Start()
        {
            UpdateThread Updater = new UpdateThread();

            Thread UpdateThread = new Thread(new ThreadStart(Updater.Start));
            UpdateThread.IsBackground = true;
            UpdateThread.Start();

            // Setup ping timer/initial values
            PingTimer.Elapsed += new ElapsedEventHandler(Ping);
            PingTimer.Interval = 10000;
            PingTimer.Enabled = false;

            Ping_Seq = 1;
            Latency = 1;

            // Fire up pather

            pather = new Pather.PPather();

            pather.OnStartGlide();


            // Loopdeloop

            Loop();

            // Thread is dead if it gets to this point
            UpdateThread.Abort();


            pather.OnStopGlide();
            pather = null;

        }

        private void Loop()
        {
            byte[] data;
            int dataSize;

            do
            {
                try
                {
                    UInt32 TimeNow = MM_GetTime();
                    Update(TimeNow - LastUpdateTime);
                    LastUpdateTime = MM_GetTime();

                    dataSize = parseSize(OnReceive(2));
                    data = OnReceive(dataSize);
                    decryptData(data);
                    processData(data);
                }
                catch (Exception ex)    // Server dc'd us most likely ;P
                {
                    BoogieCore.Log(LogType.Error, "Caught Exception while reading off the network: {0}", ex.Message);
                    BoogieCore.Log(LogType.Error, "{0}", ex.StackTrace);
                }
            }
            while (isRunning);

            isRunning = false;

            if (mSocket.Connected)
                mSocket.Disconnect(false);

            BoogieCore.Log(LogType.System, "WS: Thread Stopping: {0}", Time.GetTime());
        }

        private void Update(UInt32 diff)
        {

        }

        public void StopThread(bool wait)
        {
            isRunning = false;

            if (mSocket.Connected)
                mSocket.Disconnect(false);

            if (wait)   // Wait till this thread shuts down.
                WorldThread.Join();
        }

        private void Ping(object source, ElapsedEventArgs e)
        {
            if (!mSocket.Connected)
            {
                PingTimer.Enabled = false;
                PingTimer.Stop();
                return;
            }

            Ping_Req_Time = MM_GetTime();

            BoogieCore.Log(LogType.NeworkComms, "Ping!");
            WoWWriter ww = new WoWWriter(OpCode.CMSG_PING);
            ww.Write(Ping_Seq);
            ww.Write(Latency);
            Send(ww.ToArray());
        }

#region  Socket Functions

        /// <summary>
        /// Reads bytes from the WoW Server.
        /// </summary>
        /// <param name="mSize">Number of bytes to read.</param>
        /// <returns>The data recieved. It will be exactly of mSize.</returns>
        protected byte[] OnReceive(int mSize)
        {
            byte[] data = new byte[mSize];
            int readSoFar = 0;

            // Keep looping till we recieve exactly how much we need.
            do
            {
                mSocket.Poll(10, SelectMode.SelectRead);

                if (mSocket.Available > 0)
                {
                    int read = mSocket.Receive(data, readSoFar, mSize - readSoFar, SocketFlags.None);
                    readSoFar += read;
                    Thread.Sleep(10);
                }
            }
            while (readSoFar < mSize && isRunning);

            return data;
        }

        private int parseSize(byte[] SizeBytes)
        {
            mCrypt.Decrypt(SizeBytes, 2);
            return ((SizeBytes[0] * 256) + SizeBytes[1]);
        }

        private void decryptData(byte[] Data)
        {
            mCrypt.Decrypt(Data, mRecvCryptSize);
#if (LOG)
            WoWReader wr = new WoWReader(Data);
            OpCode Op = (OpCode)wr.ReadUInt16();
            tw.WriteLine("Server->Client Opcode: {1} Size: {0}", Data.Length, Op);
            Debug.DumpBuffer(Data, Data.Length, tw);
            tw.WriteLine();
            tw.Flush();
#endif
        }

        public void Send(WoWWriter wr)
        {
            Send(wr.ToArray());
        }

        public void Send(byte[] Data)
        {

#if (LOG)
            WoWReader wr = new WoWReader(Data);
            OpCode Op = (OpCode)wr.ReadUInt16();

            tw.WriteLine("Client->Server OpCode: {1} Size: {0}", Data.Length, Op);
            Debug.DumpBuffer(Data, Data.Length, tw);
            tw.WriteLine();
            tw.Flush();
#endif
            int Length = Data.Length;
            byte[] Packet = new byte[2 + Length];
            Packet[0] = (byte)(Length >> 8);
            Packet[1] = (byte)(Length & 0xff);
            Data.CopyTo(Packet, 2);
            mCrypt.Encrypt(Packet, mSendCryptSize);

            try
            {
                mSocket.Send(Packet);
            }
            catch(SocketException e)
            {
                BoogieCore.Log(LogType.Error, "Unable to send packet! Error: {0}", e.Message);
                isRunning = false;
                
            }
        }
        

        //private WoWReader Receive(OpCode ExpectedOp)
        //{
        //    WoWReader wr = Receive();
        //    if (wr == null) return null;
        //    UInt16 length = wr.ReadUInt16();
        //    OpCode op = (OpCode)wr.ReadUInt16();
        //    if (op != ExpectedOp)
        //    {
        //        throw new Exception(String.Format("Got wsop {0}, expecting {1}", op, ExpectedOp));
        //    }
        //    return wr;
        //}

        //private WoWReader Receive()
        //{
        //    byte[] buffer = new byte[10024];

        //    mSocket.Poll(-1, SelectMode.SelectRead);
        //    if (mSocket.Available == 0) return null; // TODO: Something smarter

        //    int Read = mSocket.Receive(buffer, mSocket.Available, SocketFlags.None);
        //    WoWReader wr = new WoWReader(Trim(buffer, Read));
        //    return wr;
        //}

        //private byte[] Trim(byte[] Data, int Length)
        //{
        //    byte[] Trimmed = new byte[Length];
        //    Array.Copy(Data, 0, Trimmed, 0, Length);
        //    return Trimmed;
        //}

        public void Close()
        {
            mSocket.Close();
        }

#endregion

        private void SMSG_Debug(WoWReader wr)
        {
            Debug.DumpBuffer(wr.ToArray());
        }

        public bool IsRunning
        {
            get { return isRunning; }
        }
        private string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }


    }
}
