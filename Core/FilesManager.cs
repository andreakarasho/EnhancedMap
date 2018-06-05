using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using EnhancedMap.Core.MapObjects;
using System.Text.RegularExpressions;

namespace EnhancedMap.Core
{
    public class BuildSet
    {
        public BuildSet(string name, Bitmap img, bool smart)
        {
            Name = name; Image = img; IsSmart = smart;
            Entries = new List<BuildingEntry>();
            IsEnabled = true;
        }

        public string Name;
        public Bitmap Image;
        public bool IsSmart;
        public List<BuildingEntry> Entries;
        public bool IsEnabled;
    }

    public class BuildingEntry
    {
        public BuildingEntry(BuildSet parent, string descr, Position loc, int map)
        {
            Parent = parent; Description = descr; Location = loc; Map = map;
            IsEnabled = true;
            ShowName = false;
            IsTown = false;
            IsUOAM = false;
        }

        public BuildSet Parent;
        public string Description;
        public Position Location;
        public int Map;
        public bool IsEnabled;
        public bool ShowName;
        public bool IsTown;

        public bool IsUOAM;

        public string ToFileFormat()
        {
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}",
                Description, Location.X, Location.Y, Map,
                (ShowName ? "true" : "false"));
        }
    }

    public class HouseEntry
    {
        public HouseEntry(string descr, ushort graphic, Position loc, Size size, int map)
        {
            Description = descr; Graphic = graphic; Location = loc; Size = size; Map = map;
        }

        public string Description;
        public Position Location;
        public Size Size;
        public int Map;
        public ushort Graphic;
    }

    public class GuardlineEntry
    {
        public GuardlineEntry(Position pos, Size size, int map)
        {
            Location = pos; Size = size; Map = map;
        }

        public Position Location;
        public Size Size;
        public int Map;
    }



    public static class FilesManager
    {
        private static readonly DirectoryInfo _definitionsPath = new DirectoryInfo("Definitions");
        private static readonly DirectoryInfo _iconsPath = new DirectoryInfo("Icon");

        private static readonly string[] _smarts =
        {
            "TOWN", "MOONGATE", "BRIDGE", "DOCKS", "DUNGEON", "EXIT", "GATE", "GRAVEYARD",
            "LANDMARK", "OTHER", "POINT", "SHRINE", "STAIRS", "TELEPORT", "TERRAIN", "TREASURE"
        };

        private static readonly string[] _ignored = { "GUARDLINES.txt", "HOUSES.txt" };

        public static List<BuildSet> BuildSets { get; } = new List<BuildSet>();
        public static List<HouseEntry> Houses { get; } = new List<HouseEntry>();
        public static List<GuardlineEntry> Guardlines { get; } = new List<GuardlineEntry>();

        public static void Load()
        {
            BuildSets.Clear();
            Houses.Clear();
            Guardlines.Clear();

            FileInfo[] files = _definitionsPath.GetFiles("*.txt").Where(s => !_ignored.Contains(s.Name)).ToArray();
            Bitmap[] images = new Bitmap[files.Length];

            FileInfo[] icons = _iconsPath.GetFiles("*.png");

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo definition = files[i];
                FileInfo icon = icons.FirstOrDefault(s => s.Name.Contains(Path.GetFileNameWithoutExtension(definition.Name)));

                if (icon == null)
                {
                    continue; // found an icon
                }

                Image img = Image.FromFile(icon.FullName);
                images[i] = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppPArgb);

                using (Graphics g = Graphics.FromImage(images[i]))
                {
                    g.PageUnit = GraphicsUnit.Pixel;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawImage(img, 0, 0, img.Width, img.Height);
                }

                string setName = Path.GetFileNameWithoutExtension(definition.Name);
                BuildSet buildSet = new BuildSet(setName, images[i], !_smarts.Contains(setName));
                bool istown = setName.ToLower() == "town";

                using (StreamReader reader = new StreamReader(definition.OpenRead()))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (string.IsNullOrEmpty(line))
                            continue;

                        string[] parts = line.Split('\t');

                        if (parts.Length >= 5)
                        {
                            bool enabled = parts[0] == "+";
                            string description = parts[1].TrimEnd().TrimStart();
                            Position position = Position.Parse(parts[2] + "." + parts[3]);
                            int map = parts[4].ToInt();

                            BuildingEntry entry = new BuildingEntry(buildSet, description, position, map)
                            {
                                IsEnabled = enabled,
                                IsTown = istown
                            };

                           
                            if (parts.Length >= 6)
                            {
                                entry.ShowName = parts[5] == "true";
                            }

                            buildSet.Entries.Add(entry);
                        }

                    }
                }

                BuildSets.Add(buildSet);
            }

            // patch for towns
            BuildSet townSet = BuildSets.FirstOrDefault(s => s.Name.ToLower() == "town");
            if (townSet != null)
            {
                BuildSets.Remove(townSet);
                BuildSets.Add(townSet);
            }
            // end patch

            // load houses
            FileInfo housesDef = _definitionsPath.GetFiles().FirstOrDefault(s => s.Name == _ignored[1]);
            if (housesDef != null)
            {
                using (StreamReader reader = new StreamReader(housesDef.FullName))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;

                        string[] data = line.Split('\t');
                        if (data.Length <= 0 || data.Length < 5)
                            continue;


                        if (!ushort.TryParse(data[0], out ushort graphic))
                            graphic = ushort.Parse(data[0], System.Globalization.NumberStyles.HexNumber);

                        Position loc = Position.Parse(data[1] + "." + data[2]);
                        Size size = new Size(data[3].ToInt(), data[4].ToInt());
                        int map = data[5].ToInt();
                        string descr = data[6];

                        HouseEntry house = new HouseEntry(descr, graphic, loc, size, map);

                        Houses.Add(house);
                    }
                }
            }
            else
                new FileInfo(Path.Combine(_definitionsPath.FullName, _ignored[1])).Create().Close();

            // load guardlines
            FileInfo guardlinesDef = _definitionsPath.GetFiles().FirstOrDefault(s => s.Name == _ignored[0]);
            if (guardlinesDef != null)
            {
                using (StreamReader reader = new StreamReader(guardlinesDef.FullName))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;

                        string[] data = line.Split('\t');
                        if (data.Length <= 0 || data.Length < 5)
                            continue;

                        if (data[0].StartsWith("#"))
                            continue;

                        Position loc = Position.Parse(data[0] + "." + data[1]);
                        // ignore data[2]
                        Size size = new Size(data[3].ToInt(), data[4].ToInt());
                        int map = data[5].ToInt();

                        GuardlineEntry guardline = new GuardlineEntry(loc, size, map);

                        Guardlines.Add(guardline);
                    }
                }
            }
            else
                new FileInfo(Path.Combine(_definitionsPath.FullName, _ignored[0])).Create().Close();


            ParseUOAM( );
        }

        public static void Save()
        {
            foreach (BuildSet set in BuildSets)
            {
                FileInfo file = new FileInfo(Path.Combine(_definitionsPath.FullName, set.Name + ".txt"));
                if (set.Entries.Count <= 0 && !file.Exists)
                {
                    file.Create().Close();
                }

                using (StreamWriter writer = new StreamWriter(file.FullName, false))
                {
                    foreach (BuildingEntry entry in set.Entries)
                    {
                        if (!entry.IsUOAM)
                            writer.WriteLine((entry.IsEnabled ? "+" : "-") + "\t" + entry.ToFileFormat());
                    }
                }
            }

            using (StreamWriter writer = new StreamWriter(Path.Combine(_definitionsPath.FullName, "HOUSES.txt")))
            {
                foreach (HouseEntry house in Houses)
                {
                    writer.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", 
                        house.Graphic.ToString("X"), house.Location.X, house.Location.Y, house.Size.Width, house.Size.Height, house.Map, house.Description));
                }
            }
        }

        public static bool ParseUOAM()
        {
            DirectoryInfo dir = new DirectoryInfo("Definitions");
            FileInfo[] files = dir.GetFiles("*.map");

            Regex regex = new Regex("^([\\+-])(.*):\\s+(\\d+)\\s+(\\d+)\\s+(\\d+)\\s+(.*)$");

            foreach (FileInfo file in files)
            {
                using (StreamReader reader = new StreamReader(File.OpenRead(file.FullName)))
                {

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;

                        Match match = regex.Match(line);

                        if (match.Success)
                        {
                            string type = match.Groups[2].ToString();
                            int x = match.Groups[3].ToString().ToInt();
                            int y = match.Groups[4].ToString().ToInt();
                            int map = match.Groups[5].ToString().ToInt();
                            string descr = match.Groups[6].ToString();

                            switch (type)
                            {
                                case "Minax's Fortress":
                                case "point of interest":
                                    type = "POINT";
                                    break;
                                case "marble patio":
                                    type = "MARPATIO";
                                    break;
                                case "theater":
                                    type = "THEATRE";
                                    break;
                                case "teleporter":
                                    type = "TELEPORT";
                                    break;
                            }

                            bool ok = true;
                            foreach (BuildSet s in BuildSets)
                            {
                                if (s.Entries.FirstOrDefault(j => j.Location.X == x && j.Location.Y == y) != null)
                                {
                                    ok = false;
                                    break;
                                }
                            }

                            if (!ok)
                                continue;

                            BuildSet set = BuildSets.FirstOrDefault(s => s.Name.ToLower().Contains(type.Replace(" ", "").ToLower()));
                            if (set != null)
                            {
                                set.Entries.Add(new BuildingEntry(set, descr, new Position((short)x, (short)y), map) { IsUOAM = true });
                            }

                        }

                     
                    }
                }
            }

            return true;
        }
    }
}
