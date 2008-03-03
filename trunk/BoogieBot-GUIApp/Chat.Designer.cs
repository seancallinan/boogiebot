using System;
using System.Windows.Forms;
using System.Drawing;

namespace BoogieBot.GUIApp
{
    partial class Chat
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Chat));
            this.type = new System.Windows.Forms.ComboBox();
            this.input = new System.Windows.Forms.TextBox();
            this.output = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // type
            // 
            this.type.Items.AddRange(new object[] {
            "Say",
            "Yell",
            "Act"});
            this.type.Location = new System.Drawing.Point(-1, 418);
            this.type.Name = "type";
            this.type.Size = new System.Drawing.Size(112, 21);
            this.type.TabIndex = 0;
            this.type.SelectedIndexChanged += new System.EventHandler(this.type_SelectedIndexChanged);
            // 
            // input
            // 
            this.input.Location = new System.Drawing.Point(117, 418);
            this.input.Name = "input";
            this.input.Size = new System.Drawing.Size(536, 20);
            this.input.TabIndex = 1;
            this.input.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Input_KeyDown);
            // 
            // output
            // 
            this.output.Location = new System.Drawing.Point(-1, 2);
            this.output.Name = "output";
            this.output.ReadOnly = true;
            this.output.Size = new System.Drawing.Size(654, 410);
            this.output.TabIndex = 2;
            this.output.Text = "";
            // 
            // Chat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(653, 446);
            this.Controls.Add(this.output);
            this.Controls.Add(this.input);
            this.Controls.Add(this.type);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Chat";
            this.Text = "Chat";
            this.Closing += new System.ComponentModel.CancelEventHandler(OnClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox type;
        private System.Windows.Forms.TextBox input;
        private System.Windows.Forms.RichTextBox output;
    }
}