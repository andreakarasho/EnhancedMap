using System;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using EnhancedMap.Core.MapObjects;

namespace EnhancedMap.Core.Network
{
    [Flags]
    public enum AccessLevel
    {
        Normal,
        RoomAdmin,
        ServerAdmin
    }

    public static class NetworkManager
    {
        private static readonly Timer _TimerReconnect;

        static NetworkManager()
        {
            SocketClient = new SocketClient();

            SocketClient.Disconnected += (sender, e) =>
            {
                RenderObjectsManager.Get<UserObject>().Where(s => !(s is PlayerObject)).ToList().ForEach(s => s.Dispose());
                RenderObjectsManager.Get<SharedLabelObject>().ToList().ForEach(s => s.Dispose());
            };

            _TimerReconnect = TimerManager.Create(5000, 5000, () =>
            {
                if (!SocketClient.IsConnected)
                    Connect();
            }, false);
        }

        public static SocketClient SocketClient { get; }

        public static void Connect()
        {
            string ip = Global.SettingsCollection["ip"].ToString();
            int port = Global.SettingsCollection["port"].ToInt();

            IPAddress addr = null;
            if ((addr = Resolve(ip)) == IPAddress.None || addr == null)
                MessageBox.Show("Invalid address");
            else if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                MessageBox.Show("Invalid port");
            else if (string.IsNullOrEmpty(Global.SettingsCollection["username"].ToString()))
                MessageBox.Show("Invalid Username");
            else if (string.IsNullOrEmpty(Global.SettingsCollection["password"].ToString()))
                MessageBox.Show("Invalid Password");
            else
            {
                if (_TimerReconnect.IsRunning)
                    _TimerReconnect.Stop();
                SocketClient.Connect(new IPEndPoint(addr, port));
            }
        }

        public static void Disconnect(bool reconnect)
        {
            if (SocketClient.IsConnected || SocketClient.IsRunning)
                SocketClient.Disconnect();

            PacketHandlers.ClearMessages();

            Reconnection(reconnect);
        }

        private static void Reconnection(bool connect)
        {
            if (connect)
                _TimerReconnect.Start();
            else
                _TimerReconnect.Stop();
        }

        private static IPAddress Resolve(string addr)
        {
            var ip = IPAddress.None;

            if (string.IsNullOrEmpty(addr))
                return ip;

            if (!IPAddress.TryParse(addr, out ip))
            {
                try
                {
                    var iphe = Dns.GetHostEntry(addr);
                    if (iphe.AddressList.Length > 0)
                        ip = iphe.AddressList[iphe.AddressList.Length - 1];
                }
                catch
                {
                }
            }

            return ip;
        }
    }
}