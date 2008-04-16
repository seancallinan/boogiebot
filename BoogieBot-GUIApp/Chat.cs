using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

using BoogieBot.Common;

namespace BoogieBot.GUIApp
{
    public partial class Chat : Form
    {
        delegate void SetChatCallback(string username, string message, ChatMsg type, string from);
        delegate void SetText(string text);

        Languages defaultLanguage;

        public Chat()
        {
            InitializeComponent();
            this.type.SelectedIndex = 0;
       }

        public void Display_Chat(string username, string message, ChatMsg type, string from)
        {
            string text = "";
            string u = username;
            string m = message;
            ChatMsg t = type;
            string c = from;

            switch (t)
            {
                case ChatMsg.CHAT_MSG_SAY:
                    text = String.Format("<{0}> says: {1}\r\n", u, m);
                    break;
                case ChatMsg.CHAT_MSG_WHISPER:
                    text = String.Format("[{0}] whispers: {1}\r\n", u, m);
                    break;
                case ChatMsg.CHAT_MSG_WHISPER_INFORM:
                    text = String.Format("You whisper to [{0}]: {1}\r\n", u, m);
                    break;
                case ChatMsg.CHAT_MSG_YELL:
                    text = String.Format("[{0}] yells: {1}\r\n", u, m);
                    break;
                case ChatMsg.CHAT_MSG_CHANNEL:
                    text = String.Format("[{0}] <{1}>: {2}\r\n", c, u, m);
                    break;
                case ChatMsg.CHAT_MSG_SYSTEM:
                    text = String.Format("SYSTEM: {0}\r\n", m);
                    break;
                case ChatMsg.CHAT_MSG_EMOTE:
                    text = String.Format("[{0}] emotes: {1}\r\n", u, m);
                    break;
                default:
                    text = String.Format("CHAT TYPE {2} - [{0}] says: {1}\r\n", u, m, t);
                    break;
             }

             AddText(text);
        }

        public void AddText(string str)
        {
            if (output.InvokeRequired)
            {
                output.Invoke(new SetText(AddText), new object[] { str });
            }
            else
            {
                lock (output)
                {
                    output.AppendText(str);
                    output.SelectionStart = output.Text.Length;
                    output.ScrollToCaret();
                }
            }
        }


        public void AddChannel(string channel)
        {
            foreach (string item in type.Items)
            {
                if (item.ToLower() == channel.ToLower())
                    return;
            }

            type.Items.Add(channel);
        }

        public void DelChannel(string channel)
        {
            foreach (string item in type.Items)
            {
                if (item.ToLower() == channel.ToLower())
                {
                    type.Items.Remove(item.ToString());
                    return;
                }
            }
        }

        private void UpdateList()
        {
            type.Items.Clear();
            type.Items.Add("Say");
            type.Items.Add("Yell");
            type.Items.Add("Act");

            if (BoogieCore.WorldServerClient != null)
            {
                foreach (string channel in BoogieCore.WorldServerClient.ChannelList)
                {
                    type.Items.Add(channel);
                }
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void type_SelectedIndexChanged(object sender, EventArgs e)
        {
            //UpdateList();
        }

        private void Input_KeyDown(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                defaultLanguage = (Languages)BoogieCore.configFile.ReadInteger("Connection", "DefaultLanguage");

                e.Handled = true;
                if (input.Text != "")
                {
                    string cmd = "", arguments = "";

                    string regx = @"^\/(\w+)\s*(.*)\s*$";
                    Match match = Regex.Match(input.Text, regx);

                    if (match.Success)
                    {
                        BoogieCore.Log(LogType.SystemDebug, "RegExp match! Count: {0} Matches: ", match.Captures.Count + 1);
                        for (int i = 1; i <= match.Captures.Count + 1; i++)
                            BoogieCore.Log(LogType.SystemDebug, "Match #{0} = \"{1}\" ", i, match.Groups[i].Value);

                        cmd = match.Groups[1].Value;
                        arguments = match.Groups[2].Value;
                    }

                    if (cmd == "update")
                    {
                        UpdateList();
                        input.Text = "";
                        return;
                    }

                    if (cmd == "join")
                    {
                        BoogieCore.WorldServerClient.JoinChannel(arguments, null);
                        input.Text = "";
                        return;
                    }

                    if (cmd == "part" || cmd == "leave")
                    {
                        BoogieCore.WorldServerClient.PartChannel(arguments);
                        input.Text = "";
                        return;
                    }

                    if (cmd == "whisper" || cmd == "w")
                    {
                        string regx2 = @"(\w+)\s*(.*)\s*$";
                        Match match2 = Regex.Match(arguments, regx2);
                        string user = null, msg = null;

                        if (match2.Success)
                        {
                            user = match2.Groups[1].Value;
                            msg = match2.Groups[2].Value;
                            if (user.Length > 2 && msg.Length >= 1)
                            {
                                BoogieCore.WorldServerClient.SendChatMsg(ChatMsg.CHAT_MSG_WHISPER, defaultLanguage, msg, user);
                            }
                            
                        }
                        return;
                    }

                    if (type.SelectedItem.ToString() != "")
                    {
                        if (type.SelectedItem.ToString().ToLower() == "say")
                        {
                            BoogieCore.WorldServerClient.SendChatMsg(ChatMsg.CHAT_MSG_SAY, defaultLanguage, input.Text);
                            input.Text = "";
                            return;
                        }
                        if (type.SelectedItem.ToString().ToLower() == "act")
                        {
                            BoogieCore.WorldServerClient.SendChatMsg(ChatMsg.CHAT_MSG_EMOTE, defaultLanguage, input.Text);
                            input.Text = "";
                            return;
                        }
                        if (type.SelectedItem.ToString().ToLower() == "yell")
                        {
                            BoogieCore.WorldServerClient.SendChatMsg(ChatMsg.CHAT_MSG_YELL, defaultLanguage, input.Text);
                            input.Text = "";
                            return;
                        }

                        BoogieCore.WorldServerClient.SendChatMsg(ChatMsg.CHAT_MSG_CHANNEL, defaultLanguage, input.Text, type.SelectedItem.ToString());
                        input.Text = "";
                        return;


                    }
                }
            }
        }

        private void input_TextChanged(object sender, EventArgs e)
        {

        }
    }
}