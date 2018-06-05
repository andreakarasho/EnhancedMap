using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aga.Controls.Tree;
using System.Collections;
using System.Xml;
using EnhancedMap.Core.MapObjects;
using EnhancedMap.Core;

namespace EnhancedMap.GUI.SettingsLayouts
{
    public partial class LabelsLayout : UserControl
    {
        private TreeModel _model;

        public LabelsLayout()
        {
            InitializeComponent();

            _model = new TreeModel();
            treeViewAdv1.Model = _model;

            this.customButtonCollapseAll.Click += (sender, e) => { treeViewAdv1.CollapseAll(); };
            this.customButtonExpandAll.Click += (sender, e) => { treeViewAdv1.ExpandAll(); };
            this.customButtonEnableAll.Click += (sender, e) => { _model.Nodes.OfType<MyRoot>().ToList().ForEach(s => { s.State = CheckState.Checked; s.Nodes.OfType<MyNode>().ToList().ForEach(k => k.State = CheckState.Checked); treeViewAdv1.Invalidate(); }); };
            this.customButtonDisableAll.Click += (sender, e) => { _model.Nodes.OfType<MyRoot>().ToList().ForEach(s => { s.State = CheckState.Unchecked; s.Nodes.OfType<MyNode>().ToList().ForEach(k => k.State = CheckState.Unchecked); }); treeViewAdv1.Invalidate(); };
            this.customButtonHideNamesAll.Click += (sender, e) => { _model.Nodes.OfType<MyRoot>().ToList().ForEach(s => { s.Nodes.OfType<MyNode>().ToList().ForEach(k => k.ShowName = false); }); treeViewAdv1.Invalidate(); };
            this.customButtonShowNamesAll.Click += (sender, e) => { _model.Nodes.OfType<MyRoot>().ToList().ForEach(s => { s.Nodes.OfType<MyNode>().ToList().ForEach(k => k.ShowName = true); }); treeViewAdv1.Invalidate(); };
        }


        public void LoadXML(XmlElement root)
        {
            foreach (XmlElement node in root["labels"])
            {
                string name = node.ToText("name");
                if (!string.IsNullOrEmpty(name))
                {
                    BuildSet set = FilesManager.BuildSets.FirstOrDefault(s => s.Name == name);
                    if (set != null)
                        set.IsEnabled = node.ToText("enabled").ToBool();
                }
            }
        }

        public void SaveXML(XmlWriter writer)
        {
            writer.WriteStartElement("labels");

            foreach (BuildSet set in FilesManager.BuildSets)
            {
                writer.WriteStartElement("label");
                writer.WriteAttributeString("name", set.Name);
                writer.WriteAttributeString("enabled", set.IsEnabled.ToString());
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public void BuildList()
        {
            _model.Nodes.Clear();
            treeViewAdv1.BeginUpdate();
            foreach (BuildSet set in FilesManager.BuildSets)
            {
                CheckState state = set.IsEnabled ? CheckState.Checked : CheckState.Unchecked;
                MyRoot root = (MyRoot)AddRoot(set.Name, set.Image, state, set);

                foreach (BuildingEntry label in set.Entries)
                {
                    Node child = AddChild(label.Description, label.Location.X.ToString(), label.Location.Y.ToString(), label.Map.ToString(), label.IsEnabled, label.ShowName, root, label);

                    /*if (state == CheckState.Unchecked && label.IsEnabled)
                    {
                        state = CheckState.Checked;
                    }
                    else if (state.HasFlag(CheckState.Checked) && !label.IsEnabled)
                    {
                        state = CheckState.Indeterminate;
                    }*/
                }
            }
            treeViewAdv1.EndUpdate();

            treeViewAdv1.ExpandAll();
        }

        private Node AddChild(string name, string x, string y, string facet, bool enabled, bool showname, Node parent, BuildingEntry entry)
        {
            Node node = new MyNode(name, x, y, facet, enabled, showname)
            {
                Tag = entry
            };
            parent.Nodes.Add(node);
            return node;
        }

        private Node AddRoot(string name, Bitmap img, CheckState enabled, BuildSet set)
        {
            MyRoot node = new MyRoot(name, img, enabled)
            {
                Tag = set
            };
            _model.Nodes.Add(node);
            return node;
        }


        private class MyRoot : Node
        {
            private CheckState _state;
            public MyRoot(string name, Bitmap img, CheckState enabled) 
            {
                Name = name;  Icon = img; _state = enabled;
            }

            public CheckState State
            {
                get => _state;
                set
                {
                    if (value == CheckState.Indeterminate)
                        _state = CheckState.Unchecked;
                    else
                        _state = value;

                    BuildSet entry = ((BuildSet)Tag);
                    entry.IsEnabled = _state != CheckState.Unchecked;
                }
            }
            public Bitmap Icon { get; }
            public string Name { get; }
        }

        private class MyNode : Node
        {
            private CheckState _state;
            private bool _showName;

            public MyNode(string name, string x, string y, string facet, bool enabled, bool showname) 
            {
                Name = name; X = x; Y = y; Facet = facet;
                _state = enabled ? CheckState.Checked : CheckState.Unchecked; _showName = showname;
            }

            public CheckState State
            {
                get => _state;
                set
                {
                    if (value == CheckState.Indeterminate)
                        _state = CheckState.Unchecked;
                    else
                        _state = value;

                    BuildingEntry entry = ((BuildingEntry)Tag);
                    entry.IsEnabled = _state == CheckState.Checked;

                    /*if (_state == CheckState.Unchecked)
                    {
                        ((MyRoot)Parent).State = CheckState.Indeterminate;
                    }
                    else if (_state == CheckState.Checked)
                    {
                        if (((MyRoot)Parent).Nodes.Any(s => ((MyNode)s).State == CheckState.Checked))
                        {
                            ((MyRoot)Parent).State = CheckState.Checked;
                        }
                        else
                            ((MyRoot)Parent).State = CheckState.Indeterminate;
                    }*/
                }
            }
            public string Name { get; }
            public string X { get; }
            public string Y { get; }
            public string Facet { get; }
            public bool ShowName
            {
                get => _showName;
                set
                {
                    BuildingEntry entry = ((BuildingEntry)Tag);
                    entry.ShowName = _showName = value;
                }
            }

        }

    }
}
