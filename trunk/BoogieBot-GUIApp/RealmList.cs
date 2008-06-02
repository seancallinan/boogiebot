using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;

using BoogieBot.Common;

namespace BoogieBot.GUIApp
{
    public partial class RealmList : Form
    {
        public RealmList()
        {
            InitializeComponent();
        }

        public void AddItem(string[] Realm)
        {
            listView1.Items.Add(new ListViewItem(Realm));
        }

        private void listView1_DoubleClicked(object sender, EventArgs e)
        {
            string[] address = listView1.SelectedItems[0].SubItems[1].Text.Split(':');
            IPAddress WSAddr = Dns.GetHostEntry(address[0]).AddressList[0];//IPAddress.Parse(address[0]);//Dns.GetHostEntry(address[0]).AddressList[0];
            int WSPort = Int32.Parse(address[1]);
            BoogieCore.ConnectToWorldServer(new IPEndPoint(WSAddr, WSPort));
            this.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] address = listView1.SelectedItems[0].SubItems[1].Text.Split(':');
            IPAddress WSAddr = Dns.GetHostEntry(address[0]).AddressList[0];
            int WSPort = Int32.Parse(address[1]);
            BoogieCore.ConnectToWorldServer(new IPEndPoint(WSAddr, WSPort));
            this.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}