namespace BoogieBot.GUIApp
{
    partial class BoogieBot
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.botToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.chatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.movementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.errorOutputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.systemOutputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.systemDebugMessagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.packetCommunicationOutputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileReadingDebugOutputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.terrainManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tileCount_MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.WaterHeight_MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Ztest_MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wmoManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.worldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectCount_MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectNames_MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playerObjectDumpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.getMail_MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.output = new System.Windows.Forms.RichTextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.connectedStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.locationStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.playerClassDumpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.MenuBar;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.botToolStripMenuItem,
            this.debugToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(582, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.optionsToolStripMenuItem.Text = "&Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(119, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // botToolStripMenuItem
            // 
            this.botToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem,
            this.toolStripMenuItem2,
            this.chatToolStripMenuItem,
            this.movementToolStripMenuItem});
            this.botToolStripMenuItem.Name = "botToolStripMenuItem";
            this.botToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.botToolStripMenuItem.Text = "&Bot";
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.startToolStripMenuItem.Text = "&Connect";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.connect_clicked);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Enabled = false;
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.stopToolStripMenuItem.Text = "&Disconnect";
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.disconnect_clicked);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(134, 6);
            // 
            // chatToolStripMenuItem
            // 
            this.chatToolStripMenuItem.Name = "chatToolStripMenuItem";
            this.chatToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.chatToolStripMenuItem.Text = "&Chat";
            this.chatToolStripMenuItem.Click += new System.EventHandler(this.chat_clicked);
            // 
            // movementToolStripMenuItem
            // 
            this.movementToolStripMenuItem.Name = "movementToolStripMenuItem";
            this.movementToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.movementToolStripMenuItem.Text = "Movement";
            this.movementToolStripMenuItem.Click += new System.EventHandler(this.move_clicked);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.errorOutputToolStripMenuItem,
            this.systemOutputToolStripMenuItem,
            this.systemDebugMessagesToolStripMenuItem,
            this.packetCommunicationOutputToolStripMenuItem,
            this.fileReadingDebugOutputToolStripMenuItem,
            this.toolStripMenuItem4,
            this.worldToolStripMenuItem,
            this.terrainManagerToolStripMenuItem,
            this.wmoManagerToolStripMenuItem,
            this.getMail_MenuItem,
            this.playerClassDumpToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.debugToolStripMenuItem.Text = "&Debug";
            // 
            // errorOutputToolStripMenuItem
            // 
            this.errorOutputToolStripMenuItem.Checked = true;
            this.errorOutputToolStripMenuItem.CheckOnClick = true;
            this.errorOutputToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.errorOutputToolStripMenuItem.Name = "errorOutputToolStripMenuItem";
            this.errorOutputToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.errorOutputToolStripMenuItem.Text = "Error Output";
            // 
            // systemOutputToolStripMenuItem
            // 
            this.systemOutputToolStripMenuItem.Checked = true;
            this.systemOutputToolStripMenuItem.CheckOnClick = true;
            this.systemOutputToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.systemOutputToolStripMenuItem.Name = "systemOutputToolStripMenuItem";
            this.systemOutputToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.systemOutputToolStripMenuItem.Text = "System Output";
            // 
            // systemDebugMessagesToolStripMenuItem
            // 
            this.systemDebugMessagesToolStripMenuItem.Checked = true;
            this.systemDebugMessagesToolStripMenuItem.CheckOnClick = true;
            this.systemDebugMessagesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.systemDebugMessagesToolStripMenuItem.Name = "systemDebugMessagesToolStripMenuItem";
            this.systemDebugMessagesToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.systemDebugMessagesToolStripMenuItem.Text = "System Debug Output";
            // 
            // packetCommunicationOutputToolStripMenuItem
            // 
            this.packetCommunicationOutputToolStripMenuItem.CheckOnClick = true;
            this.packetCommunicationOutputToolStripMenuItem.Name = "packetCommunicationOutputToolStripMenuItem";
            this.packetCommunicationOutputToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.packetCommunicationOutputToolStripMenuItem.Text = "Network / Packet Comunication";
            // 
            // fileReadingDebugOutputToolStripMenuItem
            // 
            this.fileReadingDebugOutputToolStripMenuItem.CheckOnClick = true;
            this.fileReadingDebugOutputToolStripMenuItem.Name = "fileReadingDebugOutputToolStripMenuItem";
            this.fileReadingDebugOutputToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.fileReadingDebugOutputToolStripMenuItem.Text = "File Reading Debug Output";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(231, 6);
            // 
            // terrainManagerToolStripMenuItem
            // 
            this.terrainManagerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tileCount_MenuItem,
            this.WaterHeight_MenuItem,
            this.Ztest_MenuItem});
            this.terrainManagerToolStripMenuItem.Name = "terrainManagerToolStripMenuItem";
            this.terrainManagerToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.terrainManagerToolStripMenuItem.Text = "TerrainManager";
            // 
            // tileCount_MenuItem
            // 
            this.tileCount_MenuItem.Name = "tileCount_MenuItem";
            this.tileCount_MenuItem.Size = new System.Drawing.Size(173, 22);
            this.tileCount_MenuItem.Text = "Tile Count";
            this.tileCount_MenuItem.Click += new System.EventHandler(this.tileCount_MenuItem_Click);
            // 
            // WaterHeight_MenuItem
            // 
            this.WaterHeight_MenuItem.Name = "WaterHeight_MenuItem";
            this.WaterHeight_MenuItem.Size = new System.Drawing.Size(173, 22);
            this.WaterHeight_MenuItem.Text = "Water Height Test";
            this.WaterHeight_MenuItem.Click += new System.EventHandler(this.WaterHeight_MenuItem_Click);
            // 
            // Ztest_MenuItem
            // 
            this.Ztest_MenuItem.Name = "Ztest_MenuItem";
            this.Ztest_MenuItem.Size = new System.Drawing.Size(173, 22);
            this.Ztest_MenuItem.Text = "Z Test";
            this.Ztest_MenuItem.Click += new System.EventHandler(this.Ztest_MenuItem_Click);
            // 
            // wmoManagerToolStripMenuItem
            // 
            this.wmoManagerToolStripMenuItem.Name = "wmoManagerToolStripMenuItem";
            this.wmoManagerToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.wmoManagerToolStripMenuItem.Text = "wmoManager";
            // 
            // worldToolStripMenuItem
            // 
            this.worldToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.objectCount_MenuItem,
            this.objectNames_MenuItem,
            this.playerObjectDumpToolStripMenuItem});
            this.worldToolStripMenuItem.Name = "worldToolStripMenuItem";
            this.worldToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.worldToolStripMenuItem.Text = "World";
            // 
            // objectCount_MenuItem
            // 
            this.objectCount_MenuItem.Name = "objectCount_MenuItem";
            this.objectCount_MenuItem.Size = new System.Drawing.Size(180, 22);
            this.objectCount_MenuItem.Text = "Object Count";
            this.objectCount_MenuItem.Click += new System.EventHandler(this.objectCount_MenuItem_Click);
            // 
            // objectNames_MenuItem
            // 
            this.objectNames_MenuItem.Name = "objectNames_MenuItem";
            this.objectNames_MenuItem.Size = new System.Drawing.Size(180, 22);
            this.objectNames_MenuItem.Text = "Object Names";
            this.objectNames_MenuItem.Click += new System.EventHandler(this.objectNames_MenuItem_Click);
            // 
            // playerObjectDumpToolStripMenuItem
            // 
            this.playerObjectDumpToolStripMenuItem.Name = "playerObjectDumpToolStripMenuItem";
            this.playerObjectDumpToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.playerObjectDumpToolStripMenuItem.Text = "Player WoWObject Dump";
            this.playerObjectDumpToolStripMenuItem.Click += new System.EventHandler(this.playerObjectDump_MenuItem_Click);
            // 
            // getMail_MenuItem
            // 
            this.getMail_MenuItem.Name = "getMail_MenuItem";
            this.getMail_MenuItem.Size = new System.Drawing.Size(234, 22);
            this.getMail_MenuItem.Text = "Get Mail!";
            this.getMail_MenuItem.Click += new System.EventHandler(this.getMail_MenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.about_clicked);
            // 
            // output
            // 
            this.output.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.output.Location = new System.Drawing.Point(0, 24);
            this.output.Name = "output";
            this.output.ReadOnly = true;
            this.output.Size = new System.Drawing.Size(582, 387);
            this.output.TabIndex = 1;
            this.output.Text = "";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectedStatusLabel,
            this.toolStripStatusLabel2,
            this.locationStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 411);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(582, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // connectedStatusLabel
            // 
            this.connectedStatusLabel.Name = "connectedStatusLabel";
            this.connectedStatusLabel.Size = new System.Drawing.Size(75, 17);
            this.connectedStatusLabel.Text = "Disconnected.";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(11, 17);
            this.toolStripStatusLabel2.Text = "|";
            this.toolStripStatusLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // locationStatusLabel
            // 
            this.locationStatusLabel.Name = "locationStatusLabel";
            this.locationStatusLabel.Size = new System.Drawing.Size(15, 17);
            this.locationStatusLabel.Text = "()";
            // 
            // playerClassDumpToolStripMenuItem
            // 
            this.playerClassDumpToolStripMenuItem.Name = "playerClassDumpToolStripMenuItem";
            this.playerClassDumpToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.playerClassDumpToolStripMenuItem.Text = "Player Class Dump";
            this.playerClassDumpToolStripMenuItem.Click += new System.EventHandler(this.playerClassDump_MenuItem_Click);
            // 
            // BoogieBot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 433);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.output);
            this.Controls.Add(this.menuStrip1);
            this.Icon = global::BoogieBot.GUIApp.Properties.Resources.Icon1;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "BoogieBot";
            this.Text = "BoogieBot GUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BoogieBot_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem botToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.RichTextBox output;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem chatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem movementToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem errorOutputToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem systemOutputToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem systemDebugMessagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem packetCommunicationOutputToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileReadingDebugOutputToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel connectedStatusLabel;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem terrainManagerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem WaterHeight_MenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel locationStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem Ztest_MenuItem;
        private System.Windows.Forms.ToolStripMenuItem wmoManagerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem worldToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem objectCount_MenuItem;
        private System.Windows.Forms.ToolStripMenuItem objectNames_MenuItem;
        private System.Windows.Forms.ToolStripMenuItem tileCount_MenuItem;
        private System.Windows.Forms.ToolStripMenuItem getMail_MenuItem;
        private System.Windows.Forms.ToolStripMenuItem playerObjectDumpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playerClassDumpToolStripMenuItem;
    }
}

