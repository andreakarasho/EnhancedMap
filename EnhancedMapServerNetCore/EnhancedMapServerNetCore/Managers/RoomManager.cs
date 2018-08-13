using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using EnhancedMapServerNetCore.Internals;
using EnhancedMapServerNetCore.Logging;

namespace EnhancedMapServerNetCore.Managers
{
    public static class RoomManager
    {
        private static readonly ConcurrentDictionary<string, Room> _rooms = new ConcurrentDictionary<string, Room>();

        public static ICollection<Room> Rooms => _rooms.Values;

        public static void Init()
        {
            Core.ServerInizialized += (sender, e) => { Load(); };

            Core.ServerShuttingDown += (sender, e) =>
            {
                if (e) // crashed
                {
                }
            };
        }

        public static Room Get(string name)
        {
            _rooms.TryGetValue(name, out var room);
            return room;
        }

        public static void Add(string name, string password = "")
        {
            Room room = Get(name);
            if (room != null)
                Log.Message(LogTypes.Warning, $"Room '{name}' already exists.");
            else
            {
                room = new Room(name, password);
                if (!_rooms.TryAdd(name, room))
                    Log.Message(LogTypes.Panic, "Impossible to add room '" + name + "'.");
            }
        }

        public static void Add(Room room)
        {
            if (Get(room.Name) != null)
                Log.Message(LogTypes.Warning, $"Room already exists '{room.Name}'");
            else
            {
                if (!_rooms.TryAdd(room.Name, room))
                    Log.Message(LogTypes.Panic, $"Impossible to add room '{room.Name}'");
            }
        }

        public static void Remove(string name)
        {
            if (!_rooms.TryRemove(name, out var room))
                Log.Message(LogTypes.Panic, "Room '" + name + "' not exists.");
        }

        public static void Load(bool isbackup = false)
        {
            Log.Message(LogTypes.Trace, "Loading rooms...");

            _rooms.Clear();

            string folderPath = Path.Combine(Core.RootPath, isbackup ? "Backup" : "Data");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string path = Path.Combine(folderPath, "Rooms.xml");

            if (!File.Exists(path))
            {
                Log.Message(LogTypes.Warning, $"Rooms files not found at '{path}'");
                return;
            }

            XmlDocument doc = new XmlDocument();
            int loaded = 0;

            try
            {
                doc.Load(path);
                XmlElement root = doc["rooms"];

                foreach (XmlElement room in root.GetElementsByTagName("room"))
                {
                    try
                    {
                        Room a = new Room(room);
                        Add(a);
                        loaded++;
                    }
                    catch
                    {
                        Log.Message(LogTypes.Error, "Impossible to load room");
                    }
                }
            }
            catch (Exception ex)
            {
                if (!isbackup)
                {
                    Log.Message(LogTypes.Error, "Rooms.xml file is corrupted. Trying to restore from backup");
                    Load(true);
                    Log.Message(LogTypes.Error, "Rooms.xml restored.");
                }
                else
                    Log.Message(LogTypes.Error, "Impossible to restore Rooms.xml");

                return;
            }

            Log.Message(LogTypes.Trace, $"{loaded} rooms loaded.");
        }

        public static void Save(bool isbackup = false)
        {
            string path = Path.Combine(Core.RootPath, isbackup ? "Backup" : "Data");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, "Rooms.xml");

            XmlWriterSettings settings = new XmlWriterSettings {Indent = true, IndentChars = "\t"};

            Log.Message(LogTypes.Trace, (isbackup ? "Backup: " : "") + "Saving rooms...");

            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("rooms");

                foreach (Room a in _rooms.Values)
                    a.Save(writer);

                writer.WriteEndElement();
            }

            Log.Message(LogTypes.Trace, (isbackup ? "Backup: " : "") + "Rooms saved.");
        }
    }
}