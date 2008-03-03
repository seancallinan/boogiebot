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
    public partial class Options : Form
    {
        public Options()
        {
            InitializeComponent();

            // Set up combobox from Languages enum
            for(int i=0; i<(int)Languages.NUM_LANGUAGES ; i++)
            {
                if (i == 4 || i == 5) continue;
                Languages lang = (Languages)i;
                comboBox1.Items.Add(lang);
            }

            readFields();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            saveFields();
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void readFields()
        {
            textBox1.Text = BoogieCore.configFile.ReadString("Connection", "User").ToLower();
            textBox2.Text = BoogieCore.configFile.ReadString("Connection", "Pass").ToLower();
            textBox3.Text = BoogieCore.configFile.ReadString("Connection", "DefaultRealm");
            textBox4.Text = BoogieCore.configFile.ReadString("Connection", "DefaultChar");
            comboBox1.SelectedItem = (Languages)BoogieCore.configFile.ReadInteger("Connection", "DefaultLanguage");
        }

        private void saveFields()
        {
            BoogieCore.configFile.Write("Connection", "User", textBox1.Text.ToUpper());
            BoogieCore.configFile.Write("Connection", "Pass", textBox2.Text.ToUpper());
            BoogieCore.configFile.Write("Connection", "DefaultRealm ", textBox3.Text);
            BoogieCore.configFile.Write("Connection", "DefaultChar ", textBox4.Text);
            BoogieCore.configFile.Write("Connection", "DefaultLanguage", (int)comboBox1.SelectedItem);
        }
    }
}