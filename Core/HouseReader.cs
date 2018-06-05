using EnhancedMap.Core.MapObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EnhancedMap.Core
{
    public class GameHouse
    {
        public Position Size;
        public uint ID;

        public GameHouse(uint id,  Position size)
        {
            ID = id; Size = size;
        }

        public GameHouse(uint id, short w, short h) : this(id, new Position(w, h))
        {
            
        }
    }

    public static class HouseReader
    {
        const string GAME_HOUSES_XML = "gamehouses.xml";

        private static Dictionary<uint, GameHouse> _gameHouses = new Dictionary<uint, GameHouse>()
        {
            // prefabric
            { 0x64, new GameHouse(0x64, 8, 8) },
            { 0x65, new GameHouse(0x65, 8, 8) },
            { 0x66, new GameHouse(0x66, 8, 8) },
            { 0x67, new GameHouse(0x67, 8, 8) },
            { 0x68, new GameHouse(0x68, 8, 8) },
            { 0x69, new GameHouse(0x69, 8, 8) },
            { 0x6A, new GameHouse(0x6A, 8, 8) },
            { 0x6B, new GameHouse(0x6B, 8, 8) },
            { 0x6C, new GameHouse(0x6C, 8, 8) },
            { 0x6D, new GameHouse(0x6D, 8, 8) },
            { 0x6E, new GameHouse(0x6E, 8, 8) },
            { 0x6F, new GameHouse(0x6F, 8, 8) },
            { 0x70, new GameHouse(0x70, 8, 8) },
            { 0x71, new GameHouse(0x71, 8, 8) },
            { 0x72, new GameHouse(0x72, 8, 8) },
            { 0x73, new GameHouse(0x73, 8, 8) },
            { 0x74, new GameHouse(0x74, 15, 15) },
            { 0x75, new GameHouse(0x75, 15, 15) },
            { 0x76, new GameHouse(0x76, 15, 15) },
            { 0x77, new GameHouse(0x77, 15, 15) },
            { 0x78, new GameHouse(0x78, 15, 15) },
            { 0x79, new GameHouse(0x79, 15, 15) },
            { 0x7A, new GameHouse(0x7A, 24, 16) },
            { 0x7B, new GameHouse(0x7B, 24, 16) },
            { 0x7C, new GameHouse(0x7C, 24, 24) },
            { 0x7D, new GameHouse(0x7D, 24, 24) },
            { 0x7E, new GameHouse(0x7E, 31, 32) },
            { 0x7F, new GameHouse(0x7F, 31, 32) },
            { 0x87, new GameHouse(0x87, 16, 15) },
            { 0x8C, new GameHouse(0x8C, 16, 15) },
            { 0x8D, new GameHouse(0x8D, 16, 15) },
            { 0x96, new GameHouse(0x96, 15, 15) },
            { 0x98, new GameHouse(0x98, 8, 8) },
            { 0x9A, new GameHouse(0x9A, 8, 14) },
            { 0x9C, new GameHouse(0x9C, 12, 10) },
            { 0x9E, new GameHouse(0x9E, 12, 12) },
            { 0xA0, new GameHouse(0xA0, 8, 8) },
            { 0xA2, new GameHouse(0xA2, 7, 8) },

            // foundations
            { 0x13EC, new GameHouse(0x13EC, 7, 7) },
            { 0x13ED, new GameHouse(0x13ED, 7, 8) },
            { 0x13EE, new GameHouse(0x13EE, 7, 9) },
            { 0x13EF, new GameHouse(0x13EF, 7, 10) },
            { 0x13F0, new GameHouse(0x13F0, 7, 11) },
            { 0x13F1, new GameHouse(0x13F1, 7, 12) },

            { 0x13F8, new GameHouse(0x13F8, 8, 7)  },
            { 0x13F9, new GameHouse(0x13F9, 8, 8)  },
            { 0x13FA, new GameHouse(0x13FA, 8, 9)  },
            { 0x13FB, new GameHouse(0x13FB, 8, 10) },
            { 0x13FC, new GameHouse(0x13FC, 8, 11) },
            { 0x13FD, new GameHouse(0x13FD, 8, 12) },
            { 0x13FE, new GameHouse(0x13FE, 8, 13) },

            { 0x1404, new GameHouse(0x1404, 9, 7)  },
            { 0x1405, new GameHouse(0x1405, 9, 8)  },
            { 0x1406, new GameHouse(0x1406, 9, 9)  },
            { 0x1407, new GameHouse(0x1407, 9, 10) },
            { 0x1408, new GameHouse(0x1408, 9, 11) },
            { 0x1409, new GameHouse(0x1409, 9, 12) },
            { 0x140A, new GameHouse(0x140A, 9, 13) },
            { 0x140B, new GameHouse(0x140B, 9, 14) },

            { 0x1410, new GameHouse(0x1410, 10, 7)  },
            { 0x1411, new GameHouse(0x1411, 10, 8)  },
            { 0x1412, new GameHouse(0x1412, 10, 9)  },
            { 0x1413, new GameHouse(0x1413, 10, 10) },
            { 0x1414, new GameHouse(0x1414, 10, 11) },
            { 0x1415, new GameHouse(0x1415, 10, 12) },
            { 0x1416, new GameHouse(0x1416, 10, 13) },
            { 0x1417, new GameHouse(0x1417, 10, 14) },
            { 0x1418, new GameHouse(0x1418, 10, 15) },

            { 0x141C, new GameHouse(0x141C, 11, 7)  },
            { 0x141D, new GameHouse(0x141D, 11, 8)  },
            { 0x141E, new GameHouse(0x141E, 11, 9)  },
            { 0x141F, new GameHouse(0x141F, 11, 10) },
            { 0x1420, new GameHouse(0x1420, 11, 11) },
            { 0x1421, new GameHouse(0x1421, 11, 12) },
            { 0x1422, new GameHouse(0x1422, 11, 13) },
            { 0x1423, new GameHouse(0x1423, 11, 14) },
            { 0x1424, new GameHouse(0x1424, 11, 15) },
            { 0x1425, new GameHouse(0x1425, 11, 16) },

            { 0x1428, new GameHouse(0x1428, 12, 7)  },
            { 0x1429, new GameHouse(0x1429, 12, 8)  },
            { 0x142A, new GameHouse(0x142A, 12, 9)  },
            { 0x142B, new GameHouse(0x142B, 12, 10) },
            { 0x142C, new GameHouse(0x142C, 12, 11) },
            { 0x142D, new GameHouse(0x142D, 12, 12) },
            { 0x142E, new GameHouse(0x142E, 12, 13) },
            { 0x142F, new GameHouse(0x142F, 12, 14) },
            { 0x1430, new GameHouse(0x1430, 12, 15) },
            { 0x1431, new GameHouse(0x1431, 12, 16) },
            { 0x1432, new GameHouse(0x1432, 12, 17) },

            { 0x1435, new GameHouse(0x1435, 13, 8) },
            { 0x1436, new GameHouse(0x1436, 13, 9) },
            { 0x1437, new GameHouse(0x1437, 13, 10) },
            { 0x1438, new GameHouse(0x1438, 13, 11) },
            { 0x1439, new GameHouse(0x1439, 13, 12) },
            { 0x143A, new GameHouse(0x143A, 13, 13) },
            { 0x143B, new GameHouse(0x143B, 13, 14) },
            { 0x143C, new GameHouse(0x143C, 13, 15) },
            { 0x143D, new GameHouse(0x143D, 13, 16) },
            { 0x143E, new GameHouse(0x143E, 13, 17) },
            { 0x143F, new GameHouse(0x143F, 13, 18) },

            { 0x1442, new GameHouse(0x1442, 14, 9)  },
            { 0x1443, new GameHouse(0x1443, 14, 10) },
            { 0x1444, new GameHouse(0x1444, 14, 11) },
            { 0x1445, new GameHouse(0x1445, 14, 12) },
            { 0x1446, new GameHouse(0x1446, 14, 13) },
            { 0x1447, new GameHouse(0x1447, 14, 14) },
            { 0x1448, new GameHouse(0x1448, 14, 15) },
            { 0x1449, new GameHouse(0x1449, 14, 16) },
            { 0x144A, new GameHouse(0x144A, 14, 17) },
            { 0x144B, new GameHouse(0x144B, 14, 18) },

            { 0x144F, new GameHouse(0x144F, 15, 10) },
            { 0x1450, new GameHouse(0x1450, 15, 11) },
            { 0x1451, new GameHouse(0x1451, 15, 12) },
            { 0x1452, new GameHouse(0x1452, 15, 13) },
            { 0x1453, new GameHouse(0x1453, 15, 14) },
            { 0x1454, new GameHouse(0x1454, 15, 15) },
            { 0x1455, new GameHouse(0x1455, 15, 16) },
            { 0x1456, new GameHouse(0x1456, 15, 17) },
            { 0x1457, new GameHouse(0x1457, 15, 18) },

            { 0x145C, new GameHouse(0x145C, 16, 11) },
            { 0x145D, new GameHouse(0x145D, 16, 12) },
            { 0x145E, new GameHouse(0x145E, 16, 13) },
            { 0x145F, new GameHouse(0x145F, 16, 14) },
            { 0x1460, new GameHouse(0x1460, 16, 15) },
            { 0x1461, new GameHouse(0x1461, 16, 16) },
            { 0x1462, new GameHouse(0x1462, 16, 17) },
            { 0x1463, new GameHouse(0x1463, 16, 18) },

            { 0x1469, new GameHouse(0x1469, 17, 12) },
            { 0x146A, new GameHouse(0x146A, 17, 13) },
            { 0x146B, new GameHouse(0x146B, 17, 14) },
            { 0x146C, new GameHouse(0x146C, 17, 15) },
            { 0x146D, new GameHouse(0x146D, 17, 16) },
            { 0x146E, new GameHouse(0x146E, 17, 17) },
            { 0x146F, new GameHouse(0x146F, 17, 18) },

            { 0x1476, new GameHouse(0x1476, 18, 13) },
            { 0x1477, new GameHouse(0x1477, 18, 14) },
            { 0x1478, new GameHouse(0x1478, 18, 15) },
            { 0x1479, new GameHouse(0x1479, 18, 16) },
            { 0x147A, new GameHouse(0x147A, 18, 17) },
            { 0x147B, new GameHouse(0x147B, 18, 18) },
        };

        public static GameHouse GetHouse(uint id)
        {
            _gameHouses.TryGetValue(id, out GameHouse house);
            return house;
        }

        public static void SaveXml()
        {
            /*XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t"
            };
            using (XmlWriter xml = XmlWriter.Create(GAME_HOUSES_XML, settings))
            {
                xml.WriteStartDocument(true);
                xml.WriteStartElement("gamehouses");

                foreach (GameHouse house in _gameHouses.Values.ToList())
                {
                    xml.WriteStartElement("house");
                    xml.WriteAttributeString("id", "0x" + house.ID.ToString("X"));
                    xml.WriteAttributeString("w", house.Size.X.ToString());
                    xml.WriteAttributeString("h", house.Size.Y.ToString());
                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
            }*/
        }

        public static void LoadXml()
        {
            /*if (!File.Exists(GAME_HOUSES_XML))
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(GAME_HOUSES_XML);
            XmlElement root = doc["gamehouses"];
            foreach (XmlElement e in root)
            {
                GameHouse house = new GameHouse(uint.Parse(e.ToText("id").Remove(0, 2), System.Globalization.NumberStyles.HexNumber), e.ToText("w").ToShort(), e.ToText("h").ToShort());
                _gameHouses[house.ID] = house;
            }*/
        }
    }
}
