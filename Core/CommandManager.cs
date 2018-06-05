using EnhancedMap.Core.MapObjects;
using EnhancedMap.Core.Network;
using EnhancedMap.Diagnostic;
using EnhancedMap.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMap.Core
{
    public static class CommandManager
    {
        const string PREFIX = "--";

        private static Dictionary<string, Action<string, string, string, string,string,string,string,string,string> > _commands { get; } = new Dictionary<string, Action<string, string, string, string, string, string, string, string, string>>()
        {
            { "panic", (string a1, string a2, string a3, string a4, string a5, string a6, string a7, string a8, string a9) =>
                {
                    Global.PlayerInstance.InPanic = true;
                    if (Global.SettingsCollection["panicsounds"].ToBool())
                        SoundsManager.Play(SOUNDS_TYPE.PANIC);
                }
            },
            { "unpanic", (string a1, string a2, string a3, string a4, string a5, string a6, string a7, string a8, string a9) => 
                {
                    Global.PlayerInstance.InPanic = false;
                }
            },
            { "goto", (string a1, string a2, string a3, string a4, string a5, string a6, string a7, string a8, string a9) =>
                {
                    if (!Global.FreeView)
                        Global.FreeView = true;
                    if (int.TryParse(a3, out int map) && map < Global.Maps.Length && map >= 0)
                    {
                        Global.X = a1.ToInt();
                        Global.Y = a2.ToInt();
                        Global.Facet = map;
                    }
                }
            },
            { "setlabel", (string a1, string a2, string a3, string a4, string a5, string a6, string a7, string a8, string a9) => 
                {
                     if (short.TryParse(a1, out short x) &&
                        short.TryParse(a2, out short y)
                        && int.TryParse(a3, out int map) && ((map < Global.Maps.Length && map >= 0) || map == 7))
                    {
                        string name = a4.Trim().ToLower();
                        BuildSet set = FilesManager.BuildSets.FirstOrDefault(s => s.Name.ToLower() == name);
                        if (set != null)
                        {
                            BuildingEntry entry = new BuildingEntry(set, string.Empty, new Position(x, y), map)
                            {
                                IsEnabled = true,
                                IsUOAM = false,
                            };

                            set.Entries.Add(entry);

                            RenderObjectsManager.AddBuilding(new BuildingObject(entry));
                        }
                    }

                }
            },
            {
                "track", (string a1, string a2, string a3, string a4, string a5, string a6, string a7, string a8, string a9) => 
                {
                    if (!string.IsNullOrEmpty(a1))
                    {
                        UserObject u =  RenderObjectsManager.GetUser(a1);
                        if (u != null && !u.IsDisposing)
                            Global.TrackedUser = u is PlayerObject ? null : u;
                    }
                }
            },
            {
                "who", (string a1, string a2, string a3, string a4, string a5, string a6, string a7, string a8, string a9) =>
                {
                    UserObject[] users = RenderObjectsManager.Get<UserObject>();
                    for (int i = 0; i < users.Length; i++)
                    {
                        UserObject u = users[i];
                        if (u != null && !u.IsDisposing)
                            UOClientManager.SysMessage(string.Format("- {0}: {1}  {2}", u.Name, u.Position, u.Map), 83);
                    }
                }
            },
            {
                "addmarker", (string a1, string a2, string a3, string a4, string a5, string a6, string a7, string a8, string a9) =>
                {
                    if (short.TryParse(a1, out short x) &&
                        short.TryParse(a2, out short y))
                    {
                        RenderObjectsManager.AddMarkerObject(new MarkerObject(Global.PlayerInstance, x, y));
                    }
                }
            },
           /* {
                "removemarker", (string a1, string a2, string a3, string a4, string a5, string a6, string a7, string a8, string a9) =>
                {

                }
            },*/

        };

        public static void DoCommand(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                string[] entries = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (_commands.TryGetValue(entries[0], out Action<string, string, string, string, string, string, string, string, string> action))
                {
                    Logger.Log("Command: " + entries[0]);

                    switch (entries.Length)
                    {
                        case 1:
                            action("", "", "", "", "", "", "", "", "");
                            break;
                        case 2:
                            action(entries[1], "", "", "", "", "", "", "", "");
                            break;
                        case 3:
                            action(entries[1], entries[2], "", "", "", "", "", "", "");
                            break;
                        case 4:
                            action(entries[1], entries[2], entries[3], "", "", "", "", "", "");
                            break;
                        case 5:
                            action(entries[1], entries[2], entries[3], entries[4], "", "", "", "", "");
                            break;
                        case 6:
                            action(entries[1], entries[2], entries[3], entries[4], entries[5], "", "", "", "");
                            break;
                        case 7:
                            action(entries[1], entries[2], entries[3], entries[4], entries[5], entries[6], "", "", "");
                            break;
                        case 8:
                            action(entries[1], entries[2], entries[3], entries[4], entries[5], entries[6], entries[7], "", "");
                            break;
                        case 9:
                            action(entries[1], entries[2], entries[3], entries[4], entries[5], entries[6], entries[7], entries[8], "");
                            break;
                        case 10:
                            action(entries[1], entries[2], entries[3], entries[4], entries[5], entries[6], entries[7], entries[8], entries[9]);
                            break;
                    }
                }
                else
                {
                    if (NetworkManager.SocketClient.IsConnected)
                    {
                        NetworkManager.SocketClient.Send(new PChatMessage(text, Global.PlayerInstance.Hue.Color));                       
                    }
                }
            }
            else
                Logger.Warn("Empty command text");
        }
    }
}
