namespace BoogieBot.GUIApp
{
    partial class CharCreation
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
            this.name = new System.Windows.Forms.TextBox();
            this.race = new System.Windows.Forms.ComboBox();
            this.plyclass = new System.Windows.Forms.ComboBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // name
            // 
            this.name.Location = new System.Drawing.Point(12, 3);
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(110, 20);
            this.name.TabIndex = 2;
            this.name.Text = "Name";
            // 
            // race
            // 
            this.race.FormattingEnabled = true;
            this.race.Items.AddRange(new object[] {
            "Human",
            "Dwarf",
            "Night Elf",
            "Gnome",
            "Orc",
            "Undead",
            "Tauren",
            "Troll"});
            this.race.Location = new System.Drawing.Point(12, 29);
            this.race.Name = "race";
            this.race.Size = new System.Drawing.Size(110, 21);
            this.race.TabIndex = 3;
            this.race.Text = "Race";
            this.race.SelectedIndexChanged += new System.EventHandler(this.race_SelectedIndexChanged);
            // 
            // plyclass
            // 
            this.plyclass.FormattingEnabled = true;
            this.plyclass.Location = new System.Drawing.Point(12, 56);
            this.plyclass.Name = "plyclass";
            this.plyclass.Size = new System.Drawing.Size(111, 21);
            this.plyclass.TabIndex = 4;
            this.plyclass.Text = "Class";
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(12, 87);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(109, 24);
            this.btnCreate.TabIndex = 5;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // CharCreation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(135, 123);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.plyclass);
            this.Controls.Add(this.race);
            this.Controls.Add(this.name);
            this.Name = "CharCreation";
            this.Text = "Create";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox name;
        private System.Windows.Forms.ComboBox race;
        private System.Windows.Forms.ComboBox plyclass;
        private System.Windows.Forms.Button btnCreate;
    }
}