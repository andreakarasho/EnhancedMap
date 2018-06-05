using EnhancedMap.Core;
using EnhancedMap.Core.MapObjects;
using EnhancedMap.Core.Network;
using EnhancedMap.Diagnostic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnhancedMap.GUI
{
    public partial class ChatF : CustomForm
    {
        private ChatEntry _lastChatEntry = ChatEntry.Invalid;

        public ChatF()
        {
            InitializeComponent();
            this.MinimumSize = new Size(this.MinimumSize.Width, 150);
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.Text = "Chat";

            ChatManager.MessageWrited += ChatManager_MessageWrited;

            this.Load += (sender, e) =>
            {
                int chatX = Global.SettingsCollection["chatX"].ToInt();
                int chatY = Global.SettingsCollection["chatY"].ToInt();

                if (chatX <= -1 || chatY <= -1)
                    Location = new Point(0, 0);
                else
                    Location = new Point(chatX, chatY);


                int chatW = Global.SettingsCollection["chatW"].ToInt();
                int chatH = Global.SettingsCollection["chatH"].ToInt();
                if (chatW <= -1 || chatH <= -1)
                    this.Size = new Size(250, 250);
                else
                    this.Size = new Size(chatW, chatH);
            };

            textBoxWriteMsg.Enabled = false;
            textBoxWriteMsg.MaxLength = 100;
            textBoxWriteMsg.BackColor = ColorsTable.Black3;
           

            SocketClient.Connected += (sender, e) => textBoxWriteMsg.Do((s) => { s.Enabled = true; s.BackColor = ColorsTable.Black0; });
            SocketClient.Disconnected += (sender, e) => textBoxWriteMsg.Do((s) => { s.Enabled = false; s.BackColor = ColorsTable.Black3; });
            SocketClient.Waiting += (sender, e) => textBoxWriteMsg.Do((s) => { s.Enabled = false; s.BackColor = ColorsTable.Black3; });


            this.FormClosing += (sender, e) => { e.Cancel = true; this.Hide(); };
            richTextBoxChat.Font = textBoxWriteMsg.Font = new Font(Font.Name, Global.SettingsCollection["chatfontsize"].ToInt(), FontStyle.Regular);
            textBoxWriteMsg.KeyDown += (sender, e) =>
            {
                Keys k = e.KeyCode;

                bool isenter = k == Keys.Enter;
                bool newline = e.Shift && isenter;

                if (isenter && !newline)
                {
                    // send msg
                    e.SuppressKeyPress = true;
                    if (string.IsNullOrEmpty(textBoxWriteMsg.Text))
                        return;

                    if (NetworkManager.SocketClient.IsConnected)
                    {
                        string t = textBoxWriteMsg.Text.TrimEnd();
                      //  lock (Chat.MessagesQueue)
                        //    Chat.MessagesQueue.Enqueue(t);
                        NetworkManager.SocketClient.Send(new PChatMessage(t, Global.PlayerInstance.Hue.Color));
                    }
                    else
                    {
                        //ChatManager.Add("EnhancedMap", "You are offline :(");
                    }

                    this.textBoxWriteMsg.Clear();
                }
                else if (newline)
                {
                    e.SuppressKeyPress = true;
                    textBoxWriteMsg.AppendText("\r\n");
                }
            };

            /*textBoxWriteMsg.TextChanged += (sender, e) =>
            {
                if (textBoxWriteMsg.Lines.Length > 1)
                {
                    richTextBoxChat.Size = new Size(this.Width, this.Height - (textBoxWriteMsg.Height + 80));
                    textBoxWriteMsg.Size = new Size(this.Width, 80);

                }
            };*/
        }

        public void AppendText(string text)
        {
            if (textBoxWriteMsg.Enabled)
            {
                textBoxWriteMsg.AppendText(text);
                if (!textBoxWriteMsg.Focused)
                    textBoxWriteMsg.Focus();
            }
        }

        private void ChatManager_MessageWrited(object sender, ChatEntry e)
        {
            this.richTextBoxChat.Do(s =>
            {
                UserObject user = RenderObjectsManager.GetUser(e.Name);
                if (user == null)
                {
                    Logger.Error("[CHAT] User '" + e.Name + "' not found");
                }
                else
                {
                    string msg;  

                    //if (_lastChatEntry.Name != e.Name && DateTime.Now.Minute > _lastChatEntry.Time.Minute)
                    {
                        msg = string.Format("[{0}] {1}:   {2}\r\n", e.Time.ToString("HH:mm"), e.Name, e.Message);
                        _lastChatEntry = e;
                    }
                    /*else
                    {
                        msg = string.Format("{0}{1}\r\n", new string(' ', _lastChatEntry.Time.ToString("HH:mm").Length), e.Message);
                    }
                   */
                    if (s.TextLength + msg.Length >= s.MaxLength)
                        s.Clear();

                    s.AppendText(msg, user.Hue.Color);

                    if (LastLineVisible(s))
                        s.ScrollToCaret();
                }
            });
        }

        public void ChangeFontSize()
        {
             richTextBoxChat.Font = textBoxWriteMsg.Font = new Font(richTextBoxChat.Font.Name, Global.SettingsCollection["chatfontsize"].ToInt(), FontStyle.Regular);
             richTextBoxChat.Clear();
        }

        private bool LastLineVisible(RichTextBox textbox)
        {
            Point lowPoint = new Point(3, textbox.ClientSize.Height - 3);
            int lastline = textbox.Lines.Count() - 2;
            int charOnLastvisibleLine = textbox.GetCharIndexFromPosition(lowPoint);
            int lastVisibleLine = textbox.GetLineFromCharIndex(charOnLastvisibleLine);
            return lastVisibleLine >= lastline;
        }
    }
}
