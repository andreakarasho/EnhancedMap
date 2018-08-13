using System;
using System.IO;
using System.Xml;
using EnhancedMapServerNetCore.Configuration;
using EnhancedMapServerNetCore.Logging;

namespace EnhancedMapServerNetCore.Managers
{
    public static class SettingsManager
    {
        public static Config Configuration { get; private set; }

        public static void Init()
        {
            Core.ServerInizialized += (sender, e) => { Load(); };

            Core.ServerShuttingDown += (sender, e) => { };
        }

        public static void Load(bool isbackup = false)
        {
            Log.Message(LogTypes.Trace, "Loading server settings...");

            string folderPath = Path.Combine(Core.RootPath, isbackup ? "Backup" : "Data");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string path = Path.Combine(folderPath, "Server.xml");

            if (!File.Exists(path))
            {
                Configuration = new Config();
                Log.Message(LogTypes.Warning, $"Server settings file not found at '{path}'");
                return;
            }

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load(path);
                XmlElement root = doc["settings"];
                Configuration = new Config(root);
            }
            catch (Exception ex)
            {
                if (!isbackup)
                {
                    Log.Message(LogTypes.Error, "Server.xml file is corrupted. Trying to restore from backup");
                    Load(true);
                    Log.Message(LogTypes.Error, "Server.xml restored.");
                }
                else
                {
                    Log.Message(LogTypes.Error, "Impossible to restore Server.xml. Load default.");
                    Configuration = new Config();
                }

                return;
            }

            Log.Message(LogTypes.Trace, "Server settings loaded.");
        }

        public static void Save(bool isbackup = false)
        {
            string path = Path.Combine(Core.RootPath, isbackup ? "Backup" : "Data");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, "Server.xml");

            XmlWriterSettings settings = new XmlWriterSettings {Indent = true, IndentChars = "\t"};

            Log.Message(LogTypes.Trace, (isbackup ? "Backup: " : "") + "Saving settings...");

            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("settings");

                Configuration.Save(writer);

                writer.WriteEndElement();
            }

            Log.Message(LogTypes.Trace, (isbackup ? "Backup: " : "") + "Settings saved.");
        }
    }
}