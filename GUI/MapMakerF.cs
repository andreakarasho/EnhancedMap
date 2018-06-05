using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnhancedMap.Core;
using EnhancedMap.GUI.MapMakerSteps;

namespace EnhancedMap.GUI
{
    public partial class MapMakerF : CustomForm
    {
        private KindClientStep _kindClientStep;
        private OriginalMapStep _originalMapStep;
        private CustomMapStep _customMapStep;
        private CreationMapsStep _creationMapsStep;

        public MapMakerF()
        {
            InitializeComponent();
            this.MinimumSize = this.MaximumSize = this.Size;
            this.MaximizeBox = false;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            IsMaking = false;

            MapsManager.RemoveMapsFiles();
            Global.Maps.Clear();

            _kindClientStep = new KindClientStep(StepMapChoosen)
            {
                Parent = this.panel1
            };

            this.FormClosing += (sender, e) =>
            {
                if (IsMaking)
                {
                    DialogResult = DialogResult.OK;
                    return;
                }

                if (MessageBox.Show("Do you want really quit?", "", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    DialogResult = DialogResult.Abort;
                }
            };
        }

        public bool IsMaking { get; private set; }

        public void CloseSafe()
        {
            IsMaking = true;
            this.Close();
        }


        private void StepMapChoosen(int choise)
        {
            switch (choise)
            {
                case 0: // classic
                    _kindClientStep.Visible = false;
                    _originalMapStep = new OriginalMapStep(StepGenerateMaps)
                    {
                        Parent = this.panel1
                    };
                    break;
                case 1: // custom
                    _kindClientStep.Visible = false;
                    _customMapStep = new CustomMapStep(StepGenerateMaps)
                    {
                        Parent = this.panel1
                    };
                    break;
                default:
                    break;
            }
        }

        private async void StepGenerateMaps(int choise, List<MapEntry> maps)
        {
            switch (choise)
            {
                case 0: // download
                    _originalMapStep.Visible = false;
                    _creationMapsStep = new CreationMapsStep()
                    {
                        Parent = panel1
                    };

                    await _creationMapsStep.ExecuteAction(choise, maps);

                    //Global.Maps.AddRange(maps);
                    foreach (MapEntry map in maps)
                        Global.Maps[map.Index] = map;

                    MapsManager.SaveMaps();
                    CloseSafe();
                    break;
                case 1: // generate classic
                    {
                        _originalMapStep.Visible = false;

                        
                        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                        folderBrowserDialog.Description = "Select Ultima Online directory";
                        while (true)
                        {
                            DialogResult result = folderBrowserDialog.ShowDialog();
                            if (result == DialogResult.Cancel)
                                Process.GetCurrentProcess().Kill();
                            else if (result == DialogResult.OK)
                            {
                                DirectoryInfo info = new DirectoryInfo(folderBrowserDialog.SelectedPath);
                                if (info.Exists && info.EnumerateFiles().Where(s => (s.Name.Contains("map") || s.Name.Contains("facet")) && (s.Extension == ".uop" || s.Extension == ".mul" || s.Extension == ".dds")).Count() > 0)
                                {
                                    Global.UOPath = folderBrowserDialog.SelectedPath;
                                    break;
                                }
                                else
                                {
                                    MessageBox.Show("Wrong directory selected.");
                                }
                            }
                        }

                        _creationMapsStep = new CreationMapsStep()
                        {
                            Parent = panel1
                        };
                
                        await _creationMapsStep.ExecuteAction(choise, maps);
                        foreach (MapEntry map in maps)
                            Global.Maps[map.Index] = map;
                        MapsManager.SaveMaps();
                        CloseSafe();
                    }
                    break;
                case 2: // generate custom
                    {
                        _customMapStep.Visible = false;
                        
                        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                        folderBrowserDialog.Description = "Select Ultima Online directory";

                        while (true)
                        {
                            DialogResult result = folderBrowserDialog.ShowDialog();
                            if (result == DialogResult.Cancel)
                                Process.GetCurrentProcess().Kill();
                            else if (result == DialogResult.OK)
                            {
                                DirectoryInfo info = new DirectoryInfo(folderBrowserDialog.SelectedPath);
                                if (info.Exists && info.EnumerateFiles().Where(s => (s.Name.Contains("map") || s.Name.Contains("facet")) && (s.Extension == ".uop" || s.Extension == ".mul" || s.Extension == ".dds")).Count() > 0)
                                {
                                    Global.UOPath = folderBrowserDialog.SelectedPath;
                                    break;
                                }
                                else
                                {
                                    MessageBox.Show("Wrong directory selected.");
                                }
                            }
                        }

                        _creationMapsStep = new CreationMapsStep()
                        {
                            Parent = panel1
                        };


                        await _creationMapsStep.ExecuteAction(choise, maps);
                        foreach (MapEntry map in maps)
                            Global.Maps[map.Index] = map;
                        MapsManager.SaveMaps();
                        CloseSafe();
                    }
                    break;
                case 3: // back
                    if (panel1.Contains(_originalMapStep))
                        panel1.Controls.Remove(_originalMapStep);
                    else if (panel1.Contains(_customMapStep))
                        panel1.Controls.Remove(_customMapStep);

                    _kindClientStep.Visible = true;
                    break;
            }
        }
    }
}
