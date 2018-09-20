using System;
using System.Globalization;
using System.Xml;
using EnhancedMapServerNetCore.Cryptography;
using EnhancedMapServerNetCore.Managers;

namespace EnhancedMapServerNetCore.Internals
{
    public sealed class Account
    {
        private bool _isKicked;

        public Account(string name, string password, Room room, ACCOUNT_LEVEL level, bool enabled)
        {
            Name = name;
            CryptPassword(password);
            Guid = GuidGenerator.GenerateNew();
            AccountLevel = level;
            IsKicked = false;
            IsBanned = !enabled;
            Room = room;
        }

        public Account(XmlElement node)
        {
            Name = Utility.GetText(node["username"], string.Empty);
            CryptedPassword = Utility.GetText(node["password"], string.Empty);

            if (!Guid.TryParse(Utility.GetText(node["guid"], string.Empty), out var guid))
                guid = GuidGenerator.GenerateNew();
            Guid = guid;

            Room = RoomManager.Get(Utility.GetText(node["room"], string.Empty));

            switch (Utility.GetText(node["type"], string.Empty))
            {
                default:
                case "Normal":
                    AccountLevel = ACCOUNT_LEVEL.NORMAL;
                    break;
                case "RoomAdmin":
                    AccountLevel = ACCOUNT_LEVEL.ROOM_ADMIN;
                    break;
                case "ServerAdmin":
                    AccountLevel = ACCOUNT_LEVEL.SERVER_ADMIN;
                    break;
            }

            IsBanned = !Convert.ToBoolean(Utility.GetText(node["enabled"], "false"));
            IsKicked = Convert.ToBoolean(Utility.GetText(node["kicked"], "false"));

            if (!DateTime.TryParseExact(Utility.GetText(node["kicktime"], null), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var kicktime))
                kicktime = DateTime.MinValue;
            KickTime = kicktime;
        }

        public Guid Guid { get; }
        public string Name { get; }
        public string CryptedPassword { get; set; }
        public ACCOUNT_LEVEL AccountLevel { get; set; }
        public bool IsKicked { get; set; }

        public bool IsBanned
        {
            get => _isKicked;
            set
            {
                if (_isKicked = value) KickTime = DateTime.Now.AddSeconds(SettingsManager.Configuration.KickTimer);
                else KickTime = DateTime.MinValue;
            }
        }

        public DateTime KickTime { get; private set; }
        public Room Room { get; set; }


        public bool IsPasswordGood(string psw)
        {
            return CryptedPassword == SHA1.Protect(Name + psw);
        }

        public void CryptPassword(string input)
        {
            CryptedPassword = SHA1.Protect(Name + input);
        }


        public void Save(XmlWriter xml)
        {
            xml.WriteStartElement("account");

            xml.WriteElementString("username", Name);
            xml.WriteElementString("password", CryptedPassword);
            xml.WriteElementString("room", Room.Name);
            // xml.WriteElementString("roomguid", Room.Guid.ToString());
            xml.WriteElementString("guid", Guid.ToString());

            switch (AccountLevel)
            {
                case ACCOUNT_LEVEL.NORMAL:
                    xml.WriteElementString("type", "Normal");
                    break;
                case ACCOUNT_LEVEL.ROOM_ADMIN:
                    xml.WriteElementString("type", "RoomAdmin");
                    break;
                case ACCOUNT_LEVEL.SERVER_ADMIN:
                    xml.WriteElementString("type", "ServerAdmin");
                    break;
            }

            xml.WriteElementString("enabled", (!IsBanned).ToString());
            xml.WriteElementString("kicked", IsKicked.ToString());
            xml.WriteElementString("kicktime", KickTime.ToString());

            xml.WriteEndElement();
        }
    }
}