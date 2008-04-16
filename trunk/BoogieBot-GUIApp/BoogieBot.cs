using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;

using BoogieBot.Common;


namespace BoogieBot.GUIApp
{
    public partial class BoogieBot : Form
    {
        // Form objects
        private About _win_about = new About();
        public Chat _win_chat = new Chat();
        private RealmList _win_rl; // = new RealmList();
        private CharList _win_CharList = new CharList();
        private Move _win_move = new Move();

        delegate void stupiddelegate(object sender, System.EventArgs args);
        delegate void CharList2(Character[] chars);


        public BoogieBot()
        {
            InitializeComponent();
            this.Closing += new CancelEventHandler(OnClosing);
        }

        public void ClearLog()
        {
            this.output.Text = "";
        }

        public void Log(LogType lt, string str, params object[] parameters)
        {
            if (lt == LogType.Error && !errorOutputToolStripMenuItem.Checked)
                return;

            if (lt == LogType.System && !systemOutputToolStripMenuItem.Checked)
                return;

            if (lt == LogType.SystemDebug && !systemDebugMessagesToolStripMenuItem.Checked)
                return;

            if (lt == LogType.NeworkComms && !packetCommunicationOutputToolStripMenuItem.Checked)
                return;

            if (lt == LogType.FileDebug && !fileReadingDebugOutputToolStripMenuItem.Checked)
                return;

            output.AppendText(String.Format(str, parameters) + "\r\n");
            //output.SelectionStart = output.Text.Length;   // this annoys me when i scroll up and try to read something
            //output.ScrollToCaret();
            output.Update();
        }

        public void Disconnect()
        {
            BoogieCore.Disconnect();

            Log(LogType.System, "--------------------------------------------------------------");
            Log(LogType.System, "Bot stopped.");

            this.startToolStripMenuItem.Enabled = true;
            this.stopToolStripMenuItem.Enabled = false;
            this.connectedStatusLabel.Text = "Disconnected.";
            this.locationStatusLabel.Text = "()";
        }

        public void Connect()
        {
            Log(LogType.System, "--------------------------------------------------------------");

            bool connected = BoogieCore.ConnectToRealmListServer();

            if (connected)
            {
                this.stopToolStripMenuItem.Enabled = true;
                this.startToolStripMenuItem.Enabled = false;
                this.connectedStatusLabel.Text = "Connected.";
            }
            else
            {
                this.startToolStripMenuItem.Enabled = true;
                this.stopToolStripMenuItem.Enabled = false;
                this.connectedStatusLabel.Text = "Disconnected.";
                this.locationStatusLabel.Text = "()";
            }
        }

        public void ShowRealmList(Realm[] Realms)
        {
            //string DefaultRealm = BoogieCore.configFile.ReadString("Connection", "DefaultRealm");

            _win_rl = new RealmList();
            _win_rl.Show();

            foreach (Realm realm in Realms)
            {
                string[] temp = new string[6];
                temp[0] = realm.Name;
                temp[1] = realm.Address;
                temp[2] = realm.Type.ToString();
                temp[3] = realm.NumChars.ToString();
                temp[4] = realm.Population.ToString();

                _win_rl.AddItem(temp);
            }
        }

        public void EventHandler(Event e)
        {
            switch (e.eventType)
            {
                case EventType.EVENT_REALMLIST:
                    ShowRealmList((Realm[])e.eventArgs[0]);
                    break;
                case EventType.EVENT_CHAR_LIST:
                    ShowCharList((Character[])e.eventArgs[0]);
                    break;
                case EventType.EVENT_CHAT:
                    ChatQueue queue = (ChatQueue)e.eventArgs[0];
                    _win_chat.Display_Chat((string)e.eventArgs[1], queue.Message, (ChatMsg)queue.Type, queue.Channel);
                    break;
                case EventType.EVENT_CHANNEL_JOINED:
                    _win_chat.AddChannel((string)e.eventArgs[0]);
                    break;
                case EventType.EVENT_CHANNEL_LEFT:
                    _win_chat.DelChannel((string)e.eventArgs[0]);
                    break;
                case EventType.EVENT_SELF_MOVED:
                    _win_move.UpdatePosition((Coordinate)e.eventArgs[0]);
                    break;
                case EventType.EVENT_LOCATION_UPDATE:
                    locationStatusLabel.Text = (String)e.eventArgs[0];
                    break;
                default:
                    break;
            }
        }

        public void ShowCharList(Character[] Chars)
        {
            _win_CharList.Show(this);

            foreach (Character Char in Chars)
            {
                string[] temp = new string[6];
                temp[0] = String.Format("{0}", Char.GUID);
                temp[1] = Char.Name;
                temp[2] = String.Format("{0}", Char.Race);
                temp[3] = String.Format("{0}", Char.Class);
                temp[4] = String.Format("{0}", Char.Level);

                _win_CharList.AddItem(temp);
            }
        }

        #region Menu Events

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Close the form
            this.Close();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options options = new Options();
            options.Show(this);
        }

        private void BoogieBot_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Disconnect();
        }

        public void connect_clicked(object sender, System.EventArgs args)
        {
            Connect();
        }

        public void move_clicked(object sender, System.EventArgs args)
        {
            _win_move.Show();
        }

        private void disconnect_clicked(object sender, System.EventArgs args)
        {
            Disconnect();
        }

        private void about_clicked(object sender, System.EventArgs args)
        {
            _win_about.ShowDialog(this);
        }

        private void chat_clicked(object sender, System.EventArgs args)
        {
            _win_chat.Show();
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            Disconnect();
        }

        private void tileCount_MenuItem_Click(object sender, EventArgs e)
        {
        }

        private void WaterHeight_MenuItem_Click(object sender, EventArgs e)
        {
        }

        private void Ztest_MenuItem_Click(object sender, EventArgs e)
        {
        }

        private void objectCount_MenuItem_Click(object sender, EventArgs e)
        {
            int count = BoogieCore.world.DEBUG_ObjectCount();
            BoogieCore.Log(LogType.System, "World: {0} objects stored in the world.", count);
        }

        private void objectNames_MenuItem_Click(object sender, EventArgs e)
        {
            String names = BoogieCore.world.DEBUG_ObjectNames();
            BoogieCore.Log(LogType.System, "World: Listing names:\n{0}", names);
        }

        private void playerObjectDump_MenuItem_Click(object sender, EventArgs e)
        {
            Common.Object mObj = BoogieCore.world.getPlayerObject();
            BoogieCore.Log(LogType.System, "{0}", mObj);
        }

        private void playerClassDump_MenuItem_Click(object sender, EventArgs e)
        {
            BoogieCore.Log(LogType.System, "{0}", BoogieCore.Player);
        }

        private void getMail_MenuItem_Click(object sender, EventArgs e)
        {
            BoogieCore.world.getMail();
        }
        #endregion
    }
}