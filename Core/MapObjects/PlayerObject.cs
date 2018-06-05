using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnhancedMap.Core.MapObjects
{
    public class PlayerObject : UserObject, IMessageFilter
    {
        public PlayerObject(string name) : base(name)
        {
            LifeTime = TimeSpan.FromMilliseconds(-1);
            Application.AddMessageFilter(this);
        }

        public void SetName(string name) => Name = name;

        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)MSG_RECV.FACET_CHANGED:
                    Map = (byte)m.WParam;
                    if (!Global.IsFacetChanged && !Global.FreeView)
                        Global.Facet = Map;
                    break;
                case (int)MSG_RECV.HP_MAXHP:
                    Hits.Set((ushort)(m.LParam.ToInt32()), (ushort)(m.WParam.ToInt32() & 65535));
                    break;
                case (int)MSG_RECV.MANA_MAXMANA:
                    Mana.Set((ushort)(m.LParam.ToInt32()), (ushort)(m.WParam.ToInt32() & 65535));
                    break;
                case (int)MSG_RECV.STAM_MAXSTAM:
                    Stamina.Set((ushort)(m.LParam.ToInt32()), (ushort)(m.WParam.ToInt32() & 65535));
                    break;
            }

            return false;
        }

        public bool Update()
        {
            bool result = false;
            if (UOClientManager.IsAttached)
            {
                int x = 0; int y = 0;
                if (Global.SettingsCollection["clienttype"].ToInt() == 1) // enhanced client
                {


                    try
                    {
                        using (StreamReader reader = new StreamReader(File.Open(Global.UOPath + "\\logs\\pos.log", FileMode.Open, FileAccess.Read)))
                        {
                            string line = reader.ReadLine();
                            // [MAP]|[X]|[Y]|[HITS]|[MAXHITS]|[STAM]|[MAXSTAM]|[MANA]|[MAXMANA]
                            if (line.Length >= 3)
                            {
                                int startIdx = line.IndexOf("XY:");
                                int endIdx = line.IndexOf("!");
                                line = line.Substring(startIdx + 3, endIdx - (startIdx + 3));
                                string[] args = line.Split('|');
                                int map = args[0].ToInt();
                                if (Map != map)
                                    Map = (byte)map;
                                x = args[1].ToInt();
                                y = args[2].ToInt();
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                else
                {
                    int loc = Native.SendMessage(UOClientManager.hWnd, (int)MSG_SEND.GET_LOCATION_INFO, 0, 0);
                    if (loc > 0)
                    {
                        x = (loc & 65535);
                        y = loc >> 16;
                    }
                }
                        
                if (Position.X != x || Position.Y != y)
                {
                    UpdatePosition(x, y);

                    if (Global.FreeView && Global.SettingsCollection["centerplayer"].ToBool())
                    {
                        Global.FreeView = false;
                    }

                    if (Position.X == 0 && Position.Y == 0)
                    {
                        Reset();
                        return true;
                    }

                    result = true;
                }

                if (InGame)
                {
                    int facet = -1;
                    int hits = -1;
                    int maxhits = -1;
                    int stamina = -1;
                    int maxstamina = -1;
                    int mana = -1;
                    int maxmana = -1;
                    string status = string.Empty;

                    if (OEUO_Manager.IsOpen)
                    {
                        facet = OEUO_Manager.CursKind;
                        hits = OEUO_Manager.Hits;
                        maxhits = OEUO_Manager.MaxHits;
                        stamina = OEUO_Manager.Stamina;
                        maxstamina = OEUO_Manager.MaxStamina;
                        mana = OEUO_Manager.Mana;
                        maxmana = OEUO_Manager.MaxMana;
                        status = OEUO_Manager.CharStatus.ToLower();
                    }
                    else
                    {
                        int fmap = Native.SendMessage(UOClientManager.hWnd, (int)MSG_SEND.GET_FACET, 0, 0); // facet
                        int allhp = Native.SendMessage(UOClientManager.hWnd, (int)MSG_SEND.GET_HP, 0, 0); // hp
                        int allstam = Native.SendMessage(UOClientManager.hWnd, (int)MSG_SEND.GET_STAM, 0, 0); // stam
                        int allmana = Native.SendMessage(UOClientManager.hWnd, (int)MSG_SEND.GET_MANA, 0, 0); // mana
                        int flags = Native.SendMessage(UOClientManager.hWnd, (int)MSG_SEND.GET_FLAGS, 0, 0); // flags


                        if (allhp > 0)
                        {
                            hits = (allhp & 65535);
                            maxhits = allhp >> 16;
                        }

                        if (allstam > 0)
                        {
                            stamina = (allstam & 65535);
                            maxstamina = allstam >> 16;
                        }

                        if (allmana > 0)
                        {
                            mana = (allmana & 65535);
                            maxmana = allmana >> 16;
                        }
                    }

                    if (facet != -1 && facet != Map)
                    {
                        Map = (byte)facet;
                        result = true;
                    }

                    if (hits != -1 && maxhits != -1 && (Hits.Min != hits || Hits.Max != maxhits))
                    {
                        Hits.Set((ushort)hits, (ushort)maxhits);
                        result = true;
                    }

                    if ( stamina != -1 && maxstamina != -1 && (Stamina.Min != stamina || Stamina.Max != maxstamina))
                    {
                        Stamina.Set((ushort)stamina, (ushort)maxstamina);
                        result = true;
                    }

                    if (mana != -1 && maxmana != -1 && (Mana.Min != mana || Mana.Max != maxmana))
                    {
                        Mana.Set((ushort)mana, (ushort)maxmana);
                        result = true;
                    }

                    bool ispoisoned = false;
                    bool isyellowhits = false;
                    bool isparalyzed = false;
                    bool ishidden = false;

                    for (int i = 0; i < status.Length; i++)
                    {
                        char c = status[i];

                        switch (c)
                        {
                            case 'c':
                                if (!IsPoisoned)
                                {
                                    IsPoisoned = true;
                                    result = true;
                                }
                                ispoisoned = true;
                                break;
                            case 'd':
                                if (!IsYellowHits)
                                {
                                    IsYellowHits = result = true;
                                }
                                isyellowhits = true;
                                break;
                            case 'a':
                                if (!IsParalyzed)
                                {
                                    IsParalyzed = result = true;
                                }
                                isparalyzed = true;
                                break;
                            case 'h':
                                if (!IsHidden)
                                {
                                    IsHidden = result = true;
                                }
                                ishidden = true;
                                break;
                        }
                    }


                    if (!ispoisoned && IsPoisoned)
                    {
                        IsPoisoned = false; result = true;
                    }
                    if (!isyellowhits & IsYellowHits)
                    {
                        IsYellowHits = false; result = true;
                    }
                    if (!isparalyzed && IsParalyzed)
                    {
                        IsParalyzed = false; result = true;
                    }
                    if (!ishidden && IsHidden)
                    {
                        IsHidden = false; result = true;
                    }

                    if (Hits.Min == 0 && Hits.Max > 0 && Mana.Min == 0 && Mana.Max > 0)
                    {
                        if (!IsDead)
                        {
                            IsDead = true; result = true;
                            RenderObjectsManager.AddDeathObject(new DeathObject(this, Position, Map));
                        }
                    }
                    else if (Hits.Min > 0 && Hits.Max > 0 && Mana.Min > 0 && Mana.Max > 0 && IsDead)
                    {
                        IsDead = false; result = true;
                    }
                }
                else if (OEUO_Manager.ClientHwnd != IntPtr.Zero && OEUO_Manager.CliNr == 0)
                    OEUO_Manager.Attach(OEUO_Manager.ClientIndex);
            }
            else
            {
                Reset();
            }

            if (result)
            {
                SendData();
            }

            return result;
        }

        public void SendData()
        {
            byte flags = 0x00;

            if (IsPoisoned)
                flags = (byte)FLAGS_PROPERTY.POISONED;
            if (IsYellowHits)
                flags = (byte)FLAGS_PROPERTY.YELLOWHITS;
            if (IsParalyzed)
                flags = (byte)FLAGS_PROPERTY.PARALYZED;
            if (IsDead)
                flags = (byte)FLAGS_PROPERTY.DEAD;
            if (IsHidden)
                flags = (byte)FLAGS_PROPERTY.HIDDEN;

            if (Network.NetworkManager.SocketClient.IsConnected)
                Network.NetworkManager.SocketClient.Send(new Network.PPlayerData((ushort)Position.X, (ushort)Position.Y,
                    Map,
                    Hits.Min, Stamina.Min, Mana.Min,
                    Hits.Max, Stamina.Max, Mana.Max,
                    flags,
                    /*unused*/ false,
                    InPanic,
                    Hue.Color.ToArgb(),
                    Font.Name,
                    Font.Size,
                    (byte)Font.Style));
        }

        private void Reset()
        {
            if (IsPoisoned)
                IsPoisoned = false;
            if (IsYellowHits)
                IsYellowHits = false;
            if (IsParalyzed)
                IsParalyzed = false;
            if (IsDead)
                IsDead = false;
            if (IsHidden)
                IsHidden = false;

            if (Hits.Min != 0 || Hits.Max != 0)
                Hits.Reset();
            if (Mana.Min != 0 || Mana.Max != 0)
                Mana.Reset();
            if (Stamina.Min != 0 || Stamina.Max != 0)
                Stamina.Reset();

            if (Map != Global.FirstValidFacet)
                Map = (byte)Global.FirstValidFacet;
            if (Position.X != 0 || Position.Y != 0)
                UpdatePosition(0, 0);
        }
    }
}
