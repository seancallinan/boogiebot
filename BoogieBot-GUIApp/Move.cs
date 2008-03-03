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
    public partial class Move : Form
    {
        public Move()
        {
            InitializeComponent();
        }

        public void UpdatePosition(Coordinate c)
        {
            curx.Text = String.Format("{0}", c.X);
            cury.Text = String.Format("{0}", c.Y);
            curz.Text = String.Format("{0}", c.Z);
            curo.Text = String.Format("{0}", c.O);
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            Coordinate c = BoogieCore.world.getPlayerObject().coord;
            UpdatePosition(c);
        }

        private void btnWarp_Click(object sender, EventArgs e)
        {
            BoogieCore.world.warpPlayerTo( this.toCoordinate() );
        }

        private void btnCopyDown_Click(object sender, EventArgs e)
        {
            upx.Text = curx.Text;
            upy.Text = cury.Text;
            upz.Text = curz.Text;
            upo.Text = curo.Text;
        }

        private Coordinate toCoordinate()
        {
            return new Coordinate(float.Parse(upx.Text), float.Parse(upy.Text), float.Parse(upz.Text), float.Parse(upo.Text));
        }
    }
}