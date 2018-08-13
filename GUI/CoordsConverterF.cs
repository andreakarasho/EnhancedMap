using System;
using System.Drawing;
using System.Windows.Forms;
using Aga.Controls;
using EnhancedMap.Core;

namespace EnhancedMap.GUI
{
    public partial class CoordsConverterF : CustomForm
    {
        private EventHandler _handlerGO, _handerReset;

        private CONVERTER_MODE _mode;

        public CoordsConverterF()
        {
            InitializeComponent();
            MaximizeBox = false;
            MaximumSize = MinimumSize = Size;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);


            _mode = CONVERTER_MODE.LATLONG_TO_XY;
            CreateUI_XY();

            customFlatButtonSwitch.Click += (sender, e) => { Switch(); };
        }

        private void Switch()
        {
            panel1.Controls.Clear();

            if (_handlerGO != null)
                customButtonGo.Click -= _handlerGO;
            if (_handerReset != null)
                customButtonReset.Click -= _handerReset;

            if (_mode == CONVERTER_MODE.LATLONG_TO_XY)
            {
                _mode = CONVERTER_MODE.XY_TO_LATLONG;
                CreateUI_LATLONG();
                customFlatButtonSwitch.Text = "To X,Y";
            }
            else
            {
                _mode = CONVERTER_MODE.LATLONG_TO_XY;
                CreateUI_XY();
                customFlatButtonSwitch.Text = "To Lat/Long";
            }
        }

        private void CreateUI_XY()
        {
            Label labelX = new Label {Text = "X:", Location = new Point(5, 5), ForeColor = Color.White};

            NumericTextBox textBoxX = new NumericTextBox {Location = new Point(5 + labelX.Width, 5), Text = "0"};

            Label labelY = new Label {Text = "Y:", Location = new Point(5, labelX.Height + 5), ForeColor = Color.White};

            NumericTextBox textBoxY = new NumericTextBox {Location = new Point(labelY.Width + 5, labelY.Height + 5), Text = "0"};


            Label labelMap = new Label {Text = "Map:", Location = new Point(5, labelY.Height + labelX.Height + 5), ForeColor = Color.White};

            ComboBox comboBoxMap = new ComboBox {Location = new Point(labelMap.Width + 5, labelY.Height + labelX.Height + 5), DropDownStyle = ComboBoxStyle.DropDownList};


            foreach (MapEntry map in Global.Maps) comboBoxMap.Items.Add(map.Index);

            comboBoxMap.SelectedIndex = 0;

            panel1.Controls.Add(labelX);
            panel1.Controls.Add(textBoxX);

            panel1.Controls.Add(labelY);
            panel1.Controls.Add(textBoxY);

            panel1.Controls.Add(labelMap);
            panel1.Controls.Add(comboBoxMap);

            _handlerGO = delegate
            {
                if (!Global.FreeView)
                    Global.FreeView = true;
                Global.X = textBoxX.Text.Length == 0 ? 0 : textBoxX.Text.ToInt();
                Global.Y = textBoxY.Text.Length == 0 ? 0 : textBoxY.Text.ToInt();
                if (comboBoxMap.SelectedIndex != Global.Facet)
                    Global.Facet = comboBoxMap.SelectedIndex;
            };

            customButtonGo.Click += _handlerGO;

            _handerReset = delegate
            {
                if (Global.FreeView)
                    Global.FreeView = false;
                textBoxX.Text = textBoxY.Text = "0";
                comboBoxMap.SelectedIndex = 0;
            };

            customButtonReset.Click += _handerReset;
        }

        private void CreateUI_LATLONG()
        {
            Label labelLat = new Label {Text = "Lat:", ForeColor = Color.White, Location = new Point(5, 5), Width = 50};

            NumericTextBox textBoxDegreesLat = new NumericTextBox {Text = "0", TextAlign = HorizontalAlignment.Center, Width = 50, Location = new Point(5 + labelLat.Width, 5)};

            Label labelFirst = new Label {Text = "°", ForeColor = Color.White, Location = new Point(5 + labelLat.Width + textBoxDegreesLat.Width, 5), Width = 25};

            NumericTextBox textBoxMinLat = new NumericTextBox {Text = "0", TextAlign = HorizontalAlignment.Center, Width = 50, Location = new Point(5 + labelLat.Width + textBoxDegreesLat.Width + labelFirst.Width, 5)};

            Label labelSec = new Label {Text = "'", ForeColor = Color.White, Location = new Point(5 + labelLat.Width + textBoxDegreesLat.Width + labelFirst.Width + textBoxMinLat.Width, 5), Width = 25};

            ComboBox comboBoxLat = new ComboBox {DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(5 + labelSec.Width + labelLat.Width + textBoxDegreesLat.Width + labelFirst.Width + textBoxMinLat.Width, 5), Width = 50};

            comboBoxLat.Items.Add("N");
            comboBoxLat.Items.Add("S");
            comboBoxLat.SelectedIndex = 0;


            Label labelLong = new Label {Text = "Long:", ForeColor = Color.White, Location = new Point(5, 35), Width = 50};

            NumericTextBox textBoxDegreesLong = new NumericTextBox {Text = "0", TextAlign = HorizontalAlignment.Center, Width = 50, Location = new Point(5 + labelLat.Width, 35)};

            Label labelFirst1 = new Label {Text = "°", ForeColor = Color.White, Location = new Point(5 + labelLat.Width + textBoxDegreesLat.Width, 35), Width = 25};

            NumericTextBox textBoxMinLong = new NumericTextBox {Text = "0", TextAlign = HorizontalAlignment.Center, Width = 50, Location = new Point(5 + labelLat.Width + textBoxDegreesLat.Width + labelFirst.Width, 35)};

            Label labelSec1 = new Label {Text = "'", ForeColor = Color.White, Location = new Point(5 + labelLat.Width + textBoxDegreesLat.Width + labelFirst.Width + textBoxMinLat.Width, 35), Width = 25};

            ComboBox comboBoxLong = new ComboBox {DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(5 + labelSec.Width + labelLat.Width + textBoxDegreesLat.Width + labelFirst.Width + textBoxMinLat.Width, 35), Width = 50};

            comboBoxLong.Items.Add("W");
            comboBoxLong.Items.Add("E");
            comboBoxLong.SelectedIndex = 0;


            panel1.Controls.Add(labelLat);
            panel1.Controls.Add(textBoxDegreesLat);
            panel1.Controls.Add(labelFirst);
            panel1.Controls.Add(textBoxMinLat);
            panel1.Controls.Add(labelSec);
            panel1.Controls.Add(comboBoxLat);

            panel1.Controls.Add(labelLong);
            panel1.Controls.Add(textBoxDegreesLong);
            panel1.Controls.Add(labelFirst1);
            panel1.Controls.Add(textBoxMinLong);
            panel1.Controls.Add(labelSec1);
            panel1.Controls.Add(comboBoxLong);

            Label labelMap = new Label {Text = "Map:", ForeColor = Color.White, Location = new Point(5, 65), Width = 50};
            ComboBox comboBoxMap = new ComboBox {DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(55, 65)};

            foreach (MapEntry map in Global.Maps) comboBoxMap.Items.Add(map.Index);
            comboBoxMap.SelectedIndex = 0;

            panel1.Controls.Add(labelMap);
            panel1.Controls.Add(comboBoxMap);

            _handlerGO = delegate
            {
                if (!Global.FreeView)
                    Global.FreeView = true;

                int degreeLat = textBoxDegreesLat.Text.Length == 0 ? 0 : textBoxDegreesLat.Text.ToInt();
                int minLat = textBoxMinLat.Text.Length == 0 ? 0 : textBoxMinLat.Text.ToInt();
                int degreeLong = textBoxDegreesLong.Text.Length == 0 ? 0 : textBoxDegreesLong.Text.ToInt();
                int minLong = textBoxMinLong.Text.Length == 0 ? 0 : textBoxMinLong.Text.ToInt();

                Point point = Utility.GetLatLong(degreeLat, minLat, degreeLong, minLong, comboBoxLat.SelectedItem.ToString(), comboBoxLong.SelectedItem.ToString());

                if (comboBoxMap.SelectedIndex != Global.Facet)
                    Global.Facet = comboBoxMap.SelectedIndex;
                Global.X = point.X;
                Global.Y = point.Y;
            };

            customButtonGo.Click += _handlerGO;

            _handerReset = delegate
            {
                if (Global.FreeView)
                    Global.FreeView = false;
                textBoxDegreesLat.Text = textBoxMinLat.Text = textBoxDegreesLong.Text = textBoxMinLong.Text = "0";
                comboBoxMap.SelectedIndex = 0;
                comboBoxLat.SelectedIndex = 0;
                comboBoxLong.SelectedIndex = 0;
            };

            customButtonReset.Click += _handerReset;
        }

        private enum CONVERTER_MODE
        {
            XY_TO_LATLONG,
            LATLONG_TO_XY
        }
    }
}