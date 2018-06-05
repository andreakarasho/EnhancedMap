using Aga.Controls;
using EnhancedMap.Core;
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
    public partial class SharedLabelF : CustomForm
    {
        public SharedLabelF()
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.MinimumSize = this.MaximumSize = this.Size;
            this.MaximizeBox = false;
            this.Text = "Shared Label Creator";

            void hanlder(object sender, EventArgs e)
            {
                if (sender is NumericTextBox tb && string.IsNullOrEmpty(tb.Text))
                {
                    tb.Text = "0";
                }               
            }

            foreach (MapEntry m in Global.Maps)
            {
                comboBoxMap.Items.Add(m.Index);
                if (comboBoxMap.Items.Count -1 == Global.Facet)
                {
                    comboBoxMap.SelectedIndex = comboBoxMap.Items.Count - 1;
                }
            }

            numericTextBoxX.Text = MouseManager.Location.X.ToString();
            numericTextBoxY.Text = MouseManager.Location.Y.ToString();

            numericTextBoxX.TextChanged += hanlder;
            numericTextBoxY.TextChanged += hanlder;

            this.customButtonSend.Click += (sender, e) =>
            {
                if (Core.Network.NetworkManager.SocketClient.IsConnected)
                {
                    Core.Network.NetworkManager.SocketClient.Send(new Core.Network.PSharedLabel((ushort)numericTextBoxX.IntValue, (ushort)numericTextBoxY.IntValue, (byte)comboBoxMap.SelectedIndex, textBoxDescription.Text));
                    RenderObjectsManager.AddSharedLabel(new Core.MapObjects.SharedLabelObject(Global.PlayerInstance, (short)numericTextBoxX.IntValue, (short)numericTextBoxY.IntValue, (byte)comboBoxMap.SelectedIndex, textBoxDescription.Text));
                }
            };
        }
    }
}
