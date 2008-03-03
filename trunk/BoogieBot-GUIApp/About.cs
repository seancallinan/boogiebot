using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BoogieBot.GUIApp
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            webBrowser1.DocumentText = global::BoogieBot.GUIApp.Properties.Resources.credits;
            this.Closing += new CancelEventHandler(OnClosing);
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}