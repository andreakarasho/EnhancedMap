using System;
using System.Collections.Generic;
using System.Xml;

namespace EnhancedMapServerNetCore.Internals
{
    public sealed class Room
    {
        public Room(string name, string password = "")
        {
            Name = name;
            Password = password;
            Guid = GuidGenerator.GenerateNew();
            Users = new List<User>();
        }

        public Room(XmlElement node)
        {
            Name = Utility.GetText(node["name"], null);
            Password = Utility.GetText(node["password"], string.Empty);
            if (!Guid.TryParse(Utility.GetText(node["guid"], string.Empty), out var guid))
                guid = GuidGenerator.GenerateNew();
            Guid = guid;
            Users = new List<User>();
        }

        public string Name { get; }
        public string Password { get; set; }
        public Guid Guid { get; }

        public List<User> Users { get; }

        public void Save(XmlWriter xml)
        {
            xml.WriteStartElement("room");

            xml.WriteElementString("name", Name);
            xml.WriteElementString("password", Password);
            xml.WriteElementString("guid", Guid.ToString());

            xml.WriteEndElement();
        }

        public override string ToString()
        {
            return $"{Name} - {Guid}";
        }
    }
}