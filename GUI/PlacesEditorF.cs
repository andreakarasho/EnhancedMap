using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EnhancedMap.Core;
using EnhancedMap.Core.MapObjects;

namespace EnhancedMap.GUI
{
    public partial class PlacesEditorF : CustomForm
    {
        public PlacesEditorF()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            MaximumSize = MinimumSize = Size;
            Text = "Places Editor";

            textX.KeyPress += (sender, e) =>
            {
                if (!char.IsNumber(e.KeyChar) && (Keys) e.KeyChar != Keys.Back) e.Handled = true;
            };
            textY.KeyPress += (sender, e) =>
            {
                if (!char.IsNumber(e.KeyChar) && (Keys) e.KeyChar != Keys.Back) e.Handled = true;
            };

            ComboBoxCategory.SelectedIndexChanged += (sender, e) =>
            {
                customFlatButtonAdd.Enabled = ComboBoxCategory.SelectedText != FilesManager.BuildSets[ComboBoxCategory.SelectedIndex].Name;
                PictureBox1.Image = FilesManager.BuildSets[ComboBoxCategory.SelectedIndex].Image;
            };
            FilesManager.BuildSets.ForEach(s => ComboBoxCategory.Items.Add(s.Name));

            Global.Maps.ForEach(s => ComboBoxFacet.Items.Add(s.Name));
            if (Global.Maps.Length > 2 && Global.Maps[0]?.Name.ToLower() == "felucca" && Global.Maps[1]?.Name.ToLower() == "trammel")
                ComboBoxFacet.Items.Add("Fel/Tram");

            customFlatButtonCancel.Click += (sender, e) =>
            {
                if (customFlatButtonAdd.Tag is BuildingEntry build)
                {
                    build.Parent.Entries.Remove(build);
                    BuildingObject buildingObj = RenderObjectsManager.Get<BuildingObject>().FirstOrDefault(s => s.Entry == build);
                    if (buildingObj != null)
                        buildingObj.Dispose();

                    Close();
                }
                else if (customFlatButtonAdd.Tag is HouseEntry house)
                {
                    HouseObject houseObj = RenderObjectsManager.Get<HouseObject>().FirstOrDefault(s => s.Entry == house);
                    if (houseObj != null)
                        houseObj.Dispose();

                    FilesManager.Houses.Remove(house);
                    Close();
                }
                else if (customFlatButtonAdd.Tag is null)
                {
                }
            };

            customFlatButtonAdd.Click += (sender, e) =>
            {
                if (customFlatButtonAdd.Tag is BuildingEntry build)
                {
                    build.Description = textDescription.Text;
                    build.Location.X = textX.Text.ToShort();
                    build.Location.Y = textY.Text.ToShort();

                    int facet = ComboBoxFacet.SelectedIndex;
                    if (Global.Maps.Length > 2 && Global.Maps[0]?.Name.ToLower() == "felucca" && Global.Maps[1]?.Name.ToLower() == "trammel")
                    {
                        if (facet == 6)
                            facet = 7;
                    }

                    build.Map = facet;
                    build.Parent.Entries.Remove(build);
                    build.Parent = FilesManager.BuildSets[ComboBoxCategory.SelectedIndex];
                    FilesManager.BuildSets[ComboBoxCategory.SelectedIndex].Entries.Add(build);
                    build.IsEnabled = true;

                    RenderObjectsManager.AddBuilding(new BuildingObject(build));
                    Close();
                }
                else if (customFlatButtonAdd.Tag is HouseEntry house)
                {
                }
                else if (customFlatButtonAdd.Tag is null)
                {
                    int facet = ComboBoxFacet.SelectedIndex;
                    if (Global.Maps.Length > 2 && Global.Maps[0]?.Name.ToLower() == "felucca" && Global.Maps[1]?.Name.ToLower() == "trammel")
                    {
                        if (facet == 6)
                            facet = 7;
                    }

                    BuildingEntry entry = new BuildingEntry(FilesManager.BuildSets[ComboBoxCategory.SelectedIndex], textDescription.Text, new Position(textX.Text.ToShort(), textY.Text.ToShort()), facet);
                    FilesManager.BuildSets[ComboBoxCategory.SelectedIndex].Entries.Add(entry);
                    entry.IsEnabled = true;
                    RenderObjectsManager.AddBuilding(new BuildingObject(entry));

                    Close();
                }
            };

            textX.Text = MouseManager.Location.X.ToString();
            textY.Text = MouseManager.Location.Y.ToString();
            ComboBoxFacet.SelectedIndex = Global.Facet;
        }


        public PlacesEditorF(HouseEntry entry) : this()
        {
            customFlatButtonAdd.Enabled = false;
            textDescription.Text = entry.Description;
            textX.Text = entry.Location.X.ToString();
            textY.Text = entry.Location.Y.ToString();
            ComboBoxFacet.SelectedIndex = entry.Map;

            textDescription.TextChanged += (sender, e) => { customFlatButtonAdd.Enabled = entry.Description != textDescription.Text; };
            textX.TextChanged += (sender, e) => { customFlatButtonAdd.Enabled = entry.Location.X.ToString() != textX.Text; };
            textY.TextChanged += (sender, e) => { customFlatButtonAdd.Enabled = entry.Location.Y.ToString() != textY.Text; };
            ComboBoxFacet.SelectedIndexChanged += (sender, e) => { customFlatButtonAdd.Enabled = ComboBoxFacet.SelectedIndex != entry.Map; };
            checkBoxShoNameEver.Enabled = false;
            ComboBoxCategory.Enabled = false;

            customFlatButtonAdd.Tag = entry;
        }

        public PlacesEditorF(BuildingEntry entry) : this()
        {
            customFlatButtonAdd.Enabled = false;
            textDescription.Text = entry.Description;
            textX.Text = entry.Location.X.ToString();
            textY.Text = entry.Location.Y.ToString();
            ComboBoxFacet.SelectedIndex = entry.Map == 7 ? 6 : entry.Map;
            ComboBoxCategory.SelectedIndex = FilesManager.BuildSets.IndexOf(entry.Parent);
            checkBoxShoNameEver.Checked = entry.ShowName;

            textDescription.TextChanged += (sender, e) => { customFlatButtonAdd.Enabled = entry.Description != textDescription.Text; };
            textX.TextChanged += (sender, e) => { customFlatButtonAdd.Enabled = entry.Location.X.ToString() != textX.Text; };
            textY.TextChanged += (sender, e) => { customFlatButtonAdd.Enabled = entry.Location.Y.ToString() != textY.Text; };
            ComboBoxFacet.SelectedIndexChanged += (sender, e) => { customFlatButtonAdd.Enabled = ComboBoxFacet.SelectedIndex != entry.Map; };
            checkBoxShoNameEver.CheckedChanged += (sender, e) =>
            {
                customFlatButtonAdd.Enabled = checkBoxShoNameEver.Checked != entry.ShowName;
                entry.ShowName = checkBoxShoNameEver.Checked;
            };

            customFlatButtonAdd.Tag = entry;
        }

        private void Verify()
        {
        }
    }
}