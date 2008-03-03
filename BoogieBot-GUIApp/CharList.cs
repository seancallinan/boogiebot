using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using BoogieBot.Common;

namespace BoogieBot.GUIApp
{
    public partial class CharList : Form
    {
        public CharList()
        {
            InitializeComponent();
        }

        public void AddItem(string[] Realm)
        {
            listView1.Items.Add(new ListViewItem(Realm));
        }

        private void listView1_DoubleClicked(object sender, EventArgs e)
        {
            BoogieCore.Log(LogType.SystemDebug, "Attempting to login character {0}", listView1.SelectedItems[0].SubItems[1].Text);
            BoogieCore.WorldServerClient.LoginChar(UInt64.Parse(listView1.SelectedItems[0].SubItems[0].Text));
            this.Hide();
        }
    }
}