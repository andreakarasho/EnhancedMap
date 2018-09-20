using System;
using System.Net;
using System.Xml;

namespace EnhancedMapServerNetCore.Configuration
{
    public enum CREDENTIAL_SYSTEM
    {
        USERNAME_AND_ID,
        ONLY_PASSWORD
    }

    public sealed class Config
    {
        public Config()
        {
        }

        public Config(XmlElement xml)
        {
            XmlElement set = xml["setting"];

            Port = Convert.ToUInt16(Utility.GetText(set["port"], "8887"));
            if (Port < IPEndPoint.MinPort || Port > IPEndPoint.MaxPort)
                Port = 8887;

            KickTimer = Convert.ToUInt32(Utility.GetText(set["kicktimer"], "5"));
            if (KickTimer <= 0)
                KickTimer = 5;

            int cred = Convert.ToInt32(Utility.GetText(set["credentialssystem"], "0"));
            if (cred == 0 || cred == 1)
                CredentialsSystem = (CREDENTIAL_SYSTEM) cred;

            MaxSimultaneConnections = Convert.ToUInt16(Utility.GetText(set["maxsimultaneconnections"], "10"));
            MaxActiveConnections = Convert.ToUInt16(Utility.GetText(set["maxactiveconnections"], "1000"));
        }


        public ushort Port { get; set; } = 8887;
        public uint KickTimer { get; set; } = 5;
        public CREDENTIAL_SYSTEM CredentialsSystem { get; set; }
        public ushort MaxSimultaneConnections { get; set; } = 10;
        public ushort MaxActiveConnections { get; set; } = 1000;


        public void Save(XmlWriter writer)
        {
            writer.WriteStartElement("setting");

            writer.WriteElementString("port", Port.ToString());
            writer.WriteElementString("kicktimer", KickTimer.ToString());
            writer.WriteElementString("credentialssystem", ((int) CredentialsSystem).ToString());
            writer.WriteElementString("maxsimultaneconnections", MaxSimultaneConnections.ToString());
            writer.WriteElementString("maxactiveconnections", MaxActiveConnections.ToString());

            writer.WriteEndElement();
        }
    }
}