using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using EnhancedMap.Core;

namespace EnhancedMap.GUI.MapMakerSteps
{
    public partial class CustomMapStep : UserControl
    {
        public CustomMapStep(Action<int, List<MapEntry>> nextAction)
        {
            InitializeComponent();

            CustomMaps = new List<MapEntry>();

            customFlatButtonBack.Click += (sender, e) => { nextAction(3, null); };
            customButtonGenerate.Click += (sender, e) =>
            {
                if (CustomMaps.Count > 0)
                    nextAction(2, CustomMaps);
                else
                    MessageBox.Show("No maps added.");
            };

            customButtonAdd.Click += (sender, e) =>
            {
                string t = textBoxName.Text.Trim();
                if (string.IsNullOrEmpty(t) || !t.ToCharArray().Any(s => char.IsLetter(s)))
                {
                    MessageBox.Show("Please insert a valid name.");
                    return;
                }

                if (CustomMaps.Exists(s => s.Index == numericUpDownIndex.Value))
                {
                    MessageBox.Show($"A map with index: {numericUpDownIndex.Value} already exist.");
                    return;
                }

                if (numericUpDownWidth.Value <= 0 || numericUpDownHeight.Value <= 0)
                {
                    MessageBox.Show("Please insert a valid map size");
                    return;
                }

                MapEntry map = new MapEntry((int) numericUpDownIndex.Value, CustomMaps.Count, (int) numericUpDownWidth.Value, (int) numericUpDownHeight.Value, textBoxName.Text);

                ListViewItem item = new ListViewItem(map.Index.ToString()) {Tag = map};
                item.SubItems.Add(map.Name);
                item.SubItems.Add(map.Width.ToString());
                item.SubItems.Add(map.Height.ToString());

                listViewMaps.Items.Add(item);

                CustomMaps.Add(map);
                ClearUI();
            };

            customButtonRemove.Click += (sender, e) =>
            {
                if (listViewMaps.SelectedIndices.Count == 0) return;

                MapEntry map = (MapEntry) listViewMaps.SelectedItems[0].Tag;
                CustomMaps.Remove(map);
                listViewMaps.SelectedItems[0].Remove();
            };
        }

        public List<MapEntry> CustomMaps { get; }


        private void ClearUI()
        {
            textBoxName.Clear();
            numericUpDownWidth.Value = numericUpDownHeight.Value = numericUpDownIndex.Value = 0;
        }
    }
}