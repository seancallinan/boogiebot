namespace BoogieBot.GUIApp
{
    partial class CharList
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.guid = new System.Windows.Forms.ColumnHeader();
            this.charname = new System.Windows.Forms.ColumnHeader();
            this.race = new System.Windows.Forms.ColumnHeader();
            this.Class = new System.Windows.Forms.ColumnHeader();
            this.level = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.guid,
            this.charname,
            this.race,
            this.Class,
            this.level});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(394, 163);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClicked);
            // 
            // guid
            // 
            this.guid.Text = "GUID";
            this.guid.Width = 113;
            // 
            // charname
            // 
            this.charname.Text = "Name";
            this.charname.Width = 125;
            // 
            // race
            // 
            this.race.Text = "Race";
            this.race.Width = 46;
            // 
            // Class
            // 
            this.Class.Text = "Class";
            this.Class.Width = 50;
            // 
            // level
            // 
            this.level.Text = "Level";
            this.level.Width = 55;
            // 
            // CharList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 163);
            this.Controls.Add(this.listView1);
            this.Name = "CharList";
            this.Text = "CharList";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader guid;
        private System.Windows.Forms.ColumnHeader charname;
        private System.Windows.Forms.ColumnHeader race;
        private System.Windows.Forms.ColumnHeader Class;
        private System.Windows.Forms.ColumnHeader level;
    }
}