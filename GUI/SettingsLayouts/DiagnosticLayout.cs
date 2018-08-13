using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using EnhancedMap.Core;
using EnhancedMap.Core.Network;
using EnhancedMap.Diagnostic;

namespace EnhancedMap.GUI.SettingsLayouts
{
    public partial class DiagnosticLayout : UserControl
    {
        public DiagnosticLayout()
        {
            InitializeComponent();

            Logger.MessageWrited += Logger_MessageWrited;
            uint outData = 0;
            uint inData = 0;

            TimerManager.Create(1000, () =>
            {
                var input = inData;
                var output = outData;

                outData = NetworkManager.SocketClient.TotalOut;
                inData = NetworkManager.SocketClient.TotalIn;

                labelData.Do(s => s.Text = string.Format("Network data:\r\n-IN: {0}\r\n-OUT: {1}", Utility.GetSizeAdaptive(inData - input), Utility.GetSizeAdaptive(outData - output)));
            }, true);

            customButtonSaveLog.Click += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(richTextBox.Text)) File.AppendAllText("enhancedmap-log.txt", string.Format("{0}: \r\n{1}\r\n\r\n", DateTime.Now, richTextBox.Text));
            };
        }


        public void LoadXML(XmlElement root)
        {
            Global.SettingsCollection["autoscroll"] = checkBoxAutoScroll.Checked = root["diagnostic"].ToText("autoscroll", Global.SettingsCollection["autoscroll"].ToString()).ToBool();
        }

        public void SaveXML(XmlWriter writer)
        {
            writer.WriteStartElement("diagnostic");

            writer.WriteAttributeString("autoscroll", checkBoxAutoScroll.Checked.ToString());

            writer.WriteEndElement();

            Global.SettingsCollection["autoscroll"] = checkBoxAutoScroll.Checked;
        }

        public void LoadDefault()
        {
            checkBoxAutoScroll.Checked = Global.SettingsCollection["autoscroll"].ToBool();
        }

        private void Logger_MessageWrited(object sender, MessageLogger e)
        {
            richTextBox.Do(s =>
            {
                switch (e.Severity)
                {
                    case MESSAGE_SEVERITY.NORMAL:
                        s.AppendText(e.ComposeMsg() + "\r\n", Color.White);
                        break;
                    case MESSAGE_SEVERITY.GOOD:
                        s.AppendText(e.ComposeMsg() + "\r\n", Color.LimeGreen);
                        break;
                    case MESSAGE_SEVERITY.WARN:
                        s.AppendText(e.ComposeMsg() + "\r\n", Color.Yellow);
                        break;
                    case MESSAGE_SEVERITY.ERROR:
                        s.AppendText(e.ComposeMsg() + "\r\n", Color.Red);
                        break;
                }

                if (checkBoxAutoScroll.Checked)
                {
                    s.SelectionStart = s.TextLength;
                    s.ScrollToCaret();
                }
            });
        }
    }
}