namespace BoogieBot.GUIApp
{
    partial class Move
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
            this.curx = new System.Windows.Forms.TextBox();
            this.cury = new System.Windows.Forms.TextBox();
            this.curz = new System.Windows.Forms.TextBox();
            this.curo = new System.Windows.Forms.TextBox();
            this.upx = new System.Windows.Forms.TextBox();
            this.upy = new System.Windows.Forms.TextBox();
            this.upz = new System.Windows.Forms.TextBox();
            this.upo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnWarp = new System.Windows.Forms.Button();
            this.btnCopyDown = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // curx
            // 
            this.curx.Location = new System.Drawing.Point(13, 32);
            this.curx.Name = "curx";
            this.curx.ReadOnly = true;
            this.curx.Size = new System.Drawing.Size(83, 20);
            this.curx.TabIndex = 0;
            // 
            // cury
            // 
            this.cury.Location = new System.Drawing.Point(102, 32);
            this.cury.Name = "cury";
            this.cury.ReadOnly = true;
            this.cury.Size = new System.Drawing.Size(86, 20);
            this.cury.TabIndex = 1;
            // 
            // curz
            // 
            this.curz.Location = new System.Drawing.Point(199, 32);
            this.curz.Name = "curz";
            this.curz.ReadOnly = true;
            this.curz.Size = new System.Drawing.Size(85, 20);
            this.curz.TabIndex = 2;
            // 
            // curo
            // 
            this.curo.Location = new System.Drawing.Point(294, 32);
            this.curo.Name = "curo";
            this.curo.ReadOnly = true;
            this.curo.Size = new System.Drawing.Size(49, 20);
            this.curo.TabIndex = 3;
            // 
            // upx
            // 
            this.upx.Location = new System.Drawing.Point(13, 85);
            this.upx.Name = "upx";
            this.upx.Size = new System.Drawing.Size(83, 20);
            this.upx.TabIndex = 4;
            // 
            // upy
            // 
            this.upy.Location = new System.Drawing.Point(102, 85);
            this.upy.Name = "upy";
            this.upy.Size = new System.Drawing.Size(86, 20);
            this.upy.TabIndex = 5;
            // 
            // upz
            // 
            this.upz.Location = new System.Drawing.Point(199, 85);
            this.upz.Name = "upz";
            this.upz.Size = new System.Drawing.Size(85, 20);
            this.upz.TabIndex = 6;
            // 
            // upo
            // 
            this.upo.Location = new System.Drawing.Point(294, 85);
            this.upo.Name = "upo";
            this.upo.Size = new System.Drawing.Size(49, 20);
            this.upo.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Current Position";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Enter position to warp to:";
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(362, 29);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(84, 22);
            this.btnUpdate.TabIndex = 10;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnWarp
            // 
            this.btnWarp.Location = new System.Drawing.Point(466, 85);
            this.btnWarp.Name = "btnWarp";
            this.btnWarp.Size = new System.Drawing.Size(84, 22);
            this.btnWarp.TabIndex = 11;
            this.btnWarp.Text = "Warp!";
            this.btnWarp.UseVisualStyleBackColor = true;
            this.btnWarp.Click += new System.EventHandler(this.btnWarp_Click);
            // 
            // btnCopyDown
            // 
            this.btnCopyDown.Location = new System.Drawing.Point(362, 85);
            this.btnCopyDown.Name = "btnCopyDown";
            this.btnCopyDown.Size = new System.Drawing.Size(84, 22);
            this.btnCopyDown.TabIndex = 14;
            this.btnCopyDown.Text = "Copy Down";
            this.btnCopyDown.UseVisualStyleBackColor = true;
            this.btnCopyDown.Click += new System.EventHandler(this.btnCopyDown_Click);
            // 
            // Move
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 132);
            this.Controls.Add(this.btnCopyDown);
            this.Controls.Add(this.btnWarp);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.upo);
            this.Controls.Add(this.upz);
            this.Controls.Add(this.upy);
            this.Controls.Add(this.upx);
            this.Controls.Add(this.curo);
            this.Controls.Add(this.curz);
            this.Controls.Add(this.cury);
            this.Controls.Add(this.curx);
            this.Name = "Move";
            this.Text = "Move";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.OnClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox curx;
        private System.Windows.Forms.TextBox cury;
        private System.Windows.Forms.TextBox curz;
        private System.Windows.Forms.TextBox curo;
        private System.Windows.Forms.TextBox upx;
        private System.Windows.Forms.TextBox upy;
        private System.Windows.Forms.TextBox upz;
        private System.Windows.Forms.TextBox upo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnWarp;
        private System.Windows.Forms.Button btnCopyDown;
    }
}