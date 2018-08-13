using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EnhancedMap.Core;

namespace EnhancedMap.GUI.MapMakerSteps
{
    public partial class OriginalMapStep : UserControl
    {
        public OriginalMapStep(Action<int, List<MapEntry>> nextAction)
        {
            InitializeComponent();

            List<MapEntry> entries = new List<MapEntry> {new MapEntry(0, 0, 7168, 4096, "Felucca"), new MapEntry(1, 1, 7168, 4096, "Trammel"), new MapEntry(2, 2, 2304, 1600, "Ilshenar"), new MapEntry(3, 3, 2560, 2048, "Malas"), new MapEntry(4, 4, 1448, 1448, "Tokuno"), new MapEntry(5, 5, 1280, 4096, "TerMur")};

            customFlatButtonBack.Click += (sender, e) => { nextAction(3, null); };
            customButtonDownload.Click += (sender, e) => { nextAction(0, entries); };
            customButtonGenerate.Click += (sender, e) => { nextAction(1, entries); };
        }
    }
}