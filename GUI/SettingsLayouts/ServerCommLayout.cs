using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnhancedMap.Core.Network;
using EnhancedMap.Core;

namespace EnhancedMap.GUI.SettingsLayouts
{
    public partial class ServerCommLayout : UserControl
    {
        private readonly string[] _commands = new string[]
        {
            "adduser", "addroom", "removeuser", "removeroom", "setroom",
            "kick", "ban", "enableuser", "setpassword", "allusersonline",
            "userinfo", "sendmsg", "setprivileges", "allrooms",
            "statistics", "allusersinroom"
        };

        public ServerCommLayout()
        {
            InitializeComponent();

            comboBox1.Enabled = customButtonSend.Enabled = false;
            comboBox1.Items.AddRange(_commands);
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.SelectedIndexChanged += (sender, e) => 
            {
                panel1.Controls.Clear();

                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        AddUser();
                        break;
                    case 1:
                        AddRoom();
                        break;
                    case 2:
                        RemoveUser();
                        break;
                    case 3:
                        RemoveRoom();
                        break;
                    case 4:
                        SetRoom();
                        break;
                    case 5:
                        Kick();
                        break;
                    case 6:
                        Ban();
                        break;
                    case 7:
                        EnableUser();
                        break;
                    case 8:
                        SetPassword();
                        break;
                    case 9:
                        AllUsersOnline();
                        break;
                    case 10:
                        UserInfo();
                        break;
                    case 11:
                        SendMsg();
                        break;
                    case 12:
                        SetPrivileges();
                        break;
                    case 13:
                        AllRooms();
                        break;
                    case 14:
                        Statitics();
                        break;
                    case 15:
                        AllUsersInRoom();
                        break;
                }
            };

            

            void func(ConnectionStatus status)
            {
                comboBox1.Do(s => s.Enabled = status == ConnectionStatus.Online);
                customButtonSend.Do(s => s.Enabled = status == ConnectionStatus.Online);
            }

            SocketClient.Connected += (sender, e) => func(ConnectionStatus.Online);
            SocketClient.Disconnected += (sender, e) => func(ConnectionStatus.Offline);
            SocketClient.Waiting += (sender, e) => func(ConnectionStatus.Waiting);

            EventSink.ServerResponseMessageEvent += (sender, e) =>
            {
                textBox1.Do(s => 
                {
                    s.AppendText(string.Format("{0}: {1}\r\n", DateTime.Now.ToString("HH:mm:ss"), e.Message));
                });
            };

            customButtonSend.Click += (sender, e) =>
            {
                if (comboBox1.SelectedIndex < 0 || NetworkManager.SocketClient.AccessLevel == AccessLevel.Normal)
                    return;

                string keyword = _commands[comboBox1.SelectedIndex];
                List<string> args = new List<string>();

                foreach (Control c in panel1.Controls)
                {
                    if (c is TextBox)
                    {
                        var tb = (TextBox)c;
                        if (!string.IsNullOrEmpty(tb.Text))
                        {
                            if (tb.Enabled)
                                args.Add(c.Text);
                        }
                        else
                        {
                            if (tb.Enabled)
                            {
                                MessageBox.Show("Fill all textbox");
                                return;
                            }
                        }
                    }
                    else if (c is CheckBox)
                    {
                        if (c.Enabled)
                            args.Add(((CheckBox)c).Checked ? "y" : "n");
                    }
                    else if (c is ComboBox)
                        args.Add(((ComboBox)c).SelectedIndex.ToString());
                }

                textBox1.Clear();
                NetworkManager.SocketClient.Send(new PRemoteMessage(ServerMessageType.Normal, keyword, args));
            };
        }

        private void AddUser()
        {
            labelDescription.Text = "Description: Add a new user";
            int offset = 30;
            var lname = new Label()
            {
                ForeColor = Color.White,
                Text = "Username:",
                Location = new Point(10, 12)
            };

            var lpsw = new Label()
            {
                ForeColor = Color.White,
                Text = "Password:",
                Location = new Point(10, 12 + offset)
            };

            var lroom = new Label()
            {
                ForeColor = Color.White,
            };
            lroom.Text = "Room *:";
            lroom.Location = new Point(10, 12 + offset * 2);

            var lisRoomAdmin = new Label()
            {
                ForeColor = Color.White,
                Text = "Privileges*: ",
                Location = new Point(10, 12 + offset * 3)
            };

            var name = new TextBox
            {
                Location = new Point(113, 9)
            };

            var psw = new TextBox
            {
                Location = new Point(113, 9 + offset)
            };

            var room = new TextBox
            {
                Name = "*",
                Location = new Point(113, 9 + offset * 2)
            };

            ComboBox comboBox = new ComboBox() { Name = "*", Location = new Point(113, 9 + offset * 3), DropDownStyle = ComboBoxStyle.DropDownList };
            comboBox.Items.AddRange(new string[3] { "Normal", "RoomAdmin", "ServerAdmin" });
            comboBox.SelectedIndex = 0;

            if (NetworkManager.SocketClient.AccessLevel != AccessLevel.ServerAdmin)
            {
                if (NetworkManager.SocketClient.AccessLevel == AccessLevel.Normal)
                {
                    lname.Enabled = false;
                    lpsw.Enabled = false;
                    name.Enabled = false;
                    psw.Enabled = false;
                }
                lroom.Enabled = false;
                lisRoomAdmin.Enabled = false;
                room.Enabled = false;
                comboBox.Enabled = false;
            }

            panel1.Controls.AddRange(new Control[]
                        {
                        lname, lpsw, lroom, lisRoomAdmin, name, psw, room, comboBox
                        });
        }

        private void AddRoom()
        {
            labelDescription.Text = "Description: Add a new room";

            var lname = new Label()
            {
                ForeColor = Color.White,
                Text = "Room name *:",
                Location = new Point(10, 12)
            };

            var name = new TextBox
            {
                Name = "*",
                Location = new Point(113, 9)
            };

            if (NetworkManager.SocketClient.AccessLevel != AccessLevel.ServerAdmin)
            {
                lname.Enabled = false;
                name.Enabled = false;
            }

            panel1.Controls.AddRange(new Control[]
            {
                        lname, name
            });
        }

        private void RemoveUser()
        {
            labelDescription.Text = "Description: Remove an user";

            var lname = new Label()
            {
                ForeColor = Color.White,
                Text = "Username:",
                Location = new Point(10, 12)

            };

            var name = new TextBox
            {
                Location = new Point(113, 9)
            };

            if (NetworkManager.SocketClient.AccessLevel == AccessLevel.Normal)
            {
                lname.Enabled = false;
                name.Enabled = false;
            }

            panel1.Controls.AddRange(new Control[]
            {
                        lname, name
            });

        }

        private void RemoveRoom()
        {
            labelDescription.Text = "Description: Remove a room";

            var lname = new Label()
            {
                ForeColor = Color.White,
                Text = "Room name *:",
                Location = new Point(10, 12)
            };

            var name = new TextBox
            {
                Name = "*",
                Location = new Point(113, 9)
            };

            if (NetworkManager.SocketClient.AccessLevel != AccessLevel.ServerAdmin)
            {
                lname.Enabled = false;
                name.Enabled = false;
            }

            panel1.Controls.AddRange(new Control[]
            {
                        lname, name
            });
        }

        private void SetRoom()
        {
            labelDescription.Text = "Description: Assigns an existing room to an user";
            int offset = 30;

            var lname = new Label()
            {
                ForeColor = Color.White,
                Text = "Username *:",
                Location = new Point(10, 12)
            };

            var lroomname = new Label()
            {
                ForeColor = Color.White,
                Text = "Room Name *:",
                Location = new Point(10, 12 + offset)
            };

            var name = new TextBox
            {
                Name = "*",
                Location = new Point(113, 9)
            };

            var roomname = new TextBox
            {
                Name = "*",
                Location = new Point(113, 9 + offset)
            };

            if (NetworkManager.SocketClient.AccessLevel != AccessLevel.ServerAdmin)
            {
                lname.Enabled = false;
                lroomname.Enabled = false;
                name.Enabled = false;
                roomname.Enabled = false;
            }

            panel1.Controls.AddRange(new Control[]
            {
                        lname, lroomname, name, roomname
            });

        }

        private void Kick()
        {
            labelDescription.Text = "Description: Kick an user from the server";

            var lname = new Label()
            {
                ForeColor = Color.White,
                Text = "Username:",
                Location = new Point(10, 12)
            };

            var name = new TextBox
            {
                Name = "*",
                Location = new Point(113, 9)
            };

            if (NetworkManager.SocketClient.AccessLevel == AccessLevel.Normal)
            {
                lname.Enabled = false;
                name.Enabled = false;
            }

            panel1.Controls.AddRange(new Control[]
            {
                        lname, name
            });

        }

        private void Ban()
        {
            labelDescription.Text = "Description: Ban and kick an user from the server";

            var lname = new Label()
            {
                ForeColor = Color.White,
                Text = "Username:",
                Location = new Point(10, 12)
            };

            var name = new TextBox
            {
                Location = new Point(113, 9)
            };

            if (NetworkManager.SocketClient.AccessLevel == AccessLevel.Normal)
            {
                lname.Enabled = false;
                name.Enabled = false;
            }

            panel1.Controls.AddRange(new Control[]
            {
                        lname, name
            });
        }

        private void EnableUser()
        {
            labelDescription.Text = "Description: Enable an user to connect (after a ban)";

            var lname = new Label()
            {
                ForeColor = Color.White,
                Text = "Username:",
                Location = new Point(10, 12)
            };

            var name = new TextBox
            {
                Location = new Point(113, 9)
            };

            if (NetworkManager.SocketClient.AccessLevel == AccessLevel.Normal)
            {
                lname.Enabled = false;
                name.Enabled = false;
            }

            panel1.Controls.AddRange(new Control[]
            {
                        lname, name
            });
        }

        private void SetPassword()
        {
            labelDescription.Text = "Description: Set a new password for an user";
            int offset = 30;

            var lname = new Label()
            {
                ForeColor = Color.White,
                Text = "Username:",
                Location = new Point(10, 12)
            };

            var lnewpsw = new Label()
            {
                ForeColor = Color.White,
                Text = "New password:",
                Location = new Point(10, 12 + offset)
            };

            var name = new TextBox
            {
                Location = new Point(113, 9)
            };

            var newpsw = new TextBox
            {
                Location = new Point(113, 9 + offset)
            };

            if (NetworkManager.SocketClient.AccessLevel == AccessLevel.Normal)
            {
                lname.Enabled = false;
                lnewpsw.Enabled = false;
                name.Enabled = false;
                newpsw.Enabled = false;
            }

            panel1.Controls.AddRange(new Control[]
            {
                        lname, lnewpsw, name, newpsw
            });
        }

        private void AllUsersOnline()
        {
            labelDescription.Text = "Description: Get all User online";
        }

        private void UserInfo()
        {
            labelDescription.Text = "Description: Get info from an user";

            var lname = new Label()
            {
                ForeColor = Color.White,
                Text = "Username *:",
                Location = new Point(10, 12)
            };

            var name = new TextBox
            {
                Name = "*",
                Location = new Point(113, 9)
            };

            if (NetworkManager.SocketClient.AccessLevel != AccessLevel.ServerAdmin)
            {
                lname.Enabled = false;
                name.Enabled = false;
            }
            panel1.Controls.AddRange(new Control[]
            {
                        lname, name
            });
        }

        private void SendMsg()
        {
            labelDescription.Text = "Description: Send a game message to all User";

            var lname = new Label()
            {
                ForeColor = Color.White,
                Text = "Message *:",
                Location = new Point(10, 12)
            };

            var name = new TextBox
            {
                Name = "*",
                Location = new Point(113, 9)
            };

            if (NetworkManager.SocketClient.AccessLevel != AccessLevel.ServerAdmin)
            {
                lname.Enabled = false;
                name.Enabled = false;
            }
            panel1.Controls.AddRange(new Control[]
            {
                        lname, name
            });
        }

        private void SetPrivileges()
        {
            labelDescription.Text = "Description: Set account access level";
            int offset = 30;

            var lname = new Label()
            {
                ForeColor = Color.White,
                Text = "Username *:",
                Location = new Point(10, 12)
            };

            var lisRoomAdmin = new Label()
            {
                ForeColor = Color.White,
                Text = "Privileges*: ",
                Location = new Point(10, 12 + offset)
            };

            var name = new TextBox
            {
                Name = "*",
                Location = new Point(113, 9)
            };

            ComboBox comboBox = new ComboBox() { Name = "*", Location = new Point(113, 12 + offset), DropDownStyle = ComboBoxStyle.DropDownList };
            comboBox.Items.AddRange(new string[3] { "Normal", "RoomAdmin", "ServerAdmin" });
            comboBox.SelectedIndex = 0;

            if (NetworkManager.SocketClient.AccessLevel != AccessLevel.ServerAdmin)
            {
                lname.Enabled = name.Enabled = comboBox.Enabled = false;
            }
            panel1.Controls.AddRange(new Control[]
            {
                    lname,lisRoomAdmin, name, comboBox
            });
        }

        private void AllRooms()
        {
            labelDescription.Text = "Description: Get all rooms";
        }

        private void Statitics()
        {
            labelDescription.Text =
                            "Description: Get statistics about in/out packets from server (if -profile arg is enabled)";
        }

        private void AllUsersInRoom()
        {
            labelDescription.Text = "Description: Get all users for a specific room";

            var lname = new Label()
            {
                ForeColor = Color.White,
                Text = "Room:",
                Location = new Point(10, 12)
            };

            var name = new TextBox
            {
                Name = "*",
                Location = new Point(113, 9)
            };

            if (NetworkManager.SocketClient.AccessLevel < AccessLevel.ServerAdmin)
            {
                lname.Enabled = false;
                name.Enabled = false;
            }

            panel1.Controls.AddRange(new Control[] { lname, name });

        }

    }
}
