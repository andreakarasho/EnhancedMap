using System;
using System.Drawing;
using System.Windows.Forms;
using Aga.Controls;
using EnhancedMap.Core;
using EnhancedMap.Core.MapObjects;
using EnhancedMap.Core.Network;

namespace EnhancedMap.GUI
{
    public partial class SharedLabelF : CustomForm
    {
        public SharedLabelF()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            MinimumSize = MaximumSize = Size;
            MaximizeBox = false;
            Text = "Shared Label Creator";

            void hanlder(object sender, EventArgs e)
            {
                if (sender is NumericTextBox tb && string.IsNullOrEmpty(tb.Text)) tb.Text = "0";
            }

            foreach (MapEntry m in Global.Maps)
            {
                comboBoxMap.Items.Add(m.Index);
                if (comboBoxMap.Items.Count - 1 == Global.Facet) comboBoxMap.SelectedIndex = comboBoxMap.Items.Count - 1;
            }

            numericTextBoxX.Text = MouseManager.Location.X.ToString();
            numericTextBoxY.Text = MouseManager.Location.Y.ToString();

            numericTextBoxX.TextChanged += hanlder;
            numericTextBoxY.TextChanged += hanlder;

            customButtonSend.Click += (sender, e) =>
            {
                if (NetworkManager.SocketClient.IsConnected)
                {
                    NetworkManager.SocketClient.Send(new PSharedLabel((ushort) numericTextBoxX.IntValue, (ushort) numericTextBoxY.IntValue, (byte) comboBoxMap.SelectedIndex, textBoxDescription.Text));
                    RenderObjectsManager.AddSharedLabel(new SharedLabelObject(Global.PlayerInstance, (short) numericTextBoxX.IntValue, (short) numericTextBoxY.IntValue, (byte) comboBoxMap.SelectedIndex, textBoxDescription.Text));
                }
            };
        }
    }
}