using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace DiceGame
{
    public partial class Form1 : Form
    {

        public const string GroupAdress = "224.2.2.4";
        public const int ClientReceive = 8000;
        public const int ServerReceive = 8001;

        private bool InLobby = true;
        private bool Watcher = false;
        private string Name;
        private int RoomId = -1;

        ServerForm Server;

        Client LocalClient;
        Server LocalServer;

        EList<EasyRoom> Notes;

        public Form1()
        {
            InitializeComponent();

            LocalClient = new Client(GroupAdress, ClientReceive);
            LocalServer = new Server(GroupAdress, ServerReceive);

            LocalClient.StartReceive("ClientReceiver");

            Notes = new EList<EasyRoom>();

            LocalClient.Receive += (frame) => {
                if (InvokeRequired) Invoke(new Action<Frame>((s) => {
                    EasyRoom Room;
                    if (InLobby == true)
                    {
                        if (IsContaninsRoom(frame.RoomId, out Room, Notes) == false)
                        {
                            Notes.Add((EasyRoom)frame.Data);
                        }
                        else
                        {
                            Room = (EasyRoom)frame.Data;
                        }
                    }
                    else
                    {

                        Room = (EasyRoom)frame.Data;
                        if ((frame.RoomId == RoomId || Room.ActivePlayer == Name) && frame.GameMsgType.Equals(GameMessageType.Send))
                        {
                            RoomId = frame.RoomId;
                            switch (Room.Status)
                            {
                                case GameStatus.Wait:
                                    InitWaitingRoom(Room);
                                    break;
                                case GameStatus.Game:
                                    InitGameRoom(Room);
                                    break;
                                case GameStatus.Over:
                                    break;
                                case GameStatus.Close:
                                    break;
                            }
                        }
                    }
                }
                ), frame);
            };

            InitMain();
        }


        private void InitMain()
        {
            Controls.Clear();
            ClientSize = new Size(800, 600);
            Text = "Dice Game. Main";

            Button StartServer = new Button() { Parent = this, Location = new Point(ClientRectangle.Width - 260, 10), Size = new Size(250, 40), Text = "Запустить сервер" };
            Button StartRoom = new Button() { Parent = this, Location = new Point(10, 10), Size = new Size(250, 40), Text = "Создать комнату" };

            NotesView Lobbys = new NotesView(Notes) { Parent = this, Location = new Point(10, 60), Height = 530 };        

            StartServer.Click += (object sender, EventArgs e) => {
                if (!(Server is null)) Server.Close();
                Server = new ServerForm(GroupAdress, ServerReceive, ClientReceive) { Size = new Size(400, 600), Text = "Sever" };
                Server.SetConsole();
                Server.FormClosing += (object esender, FormClosingEventArgs ee) => {
                    StartServer.Enabled = true;
                };
                StartServer.Enabled = false;
            };

            Lobbys.ConnectEvent += (note, room) => {
                InLobby = false;
                RoomId = room.Id;
            };

            Lobbys.WatchEvent += (note, room) => {
                InLobby = false;
                Watcher = true;
                RoomId = room.Id;
            };

            StartRoom.Click += (object sender, EventArgs e) => {
                InitCreateRoomForm();            
            };

        }

        public void InitCreateRoomForm()
        {
            Controls.Clear();
            ClientSize = new Size(800, 600);
            Text = "Dice Game. Create room";

            Label RoomTitle = new Label() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 - 195), Size = new Size(500, 40), Font = new Font(Font.FontFamily, 24), Text = "Настройки создания комнаты" };

            Label RoomNameInputLabel = new Label() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 - 145), Size = new Size(500, 40), Font = new Font(Font.FontFamily, 12), Text = "Введетие название для комнаты" };
            TextBox RoomNameInput = new TextBox() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 - 95), Size = new Size(500, 40), Font = new Font(Font.FontFamily, 12) };

            Label PlayersCountInputLabel = new Label() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 - 45), Size = new Size(500, 40), Font = new Font(Font.FontFamily, 12), Text = "Количество игроков в игре: 2" };
            TrackBar PlayersCountInput = new TrackBar() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 + 5), Size = new Size(500, 40), Minimum = 1, Maximum = 5 };

            Label PlayerNameInputLabel = new Label() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 + 55), Size = new Size(500, 40), Font = new Font(Font.FontFamily, 12), Text = "Введетие ваш ник" };
            TextBox PlayerNameInput = new TextBox() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 + 105), Size = new Size(500, 40), Font = new Font(Font.FontFamily, 12) };

            Button Done = new Button() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 + 155), Size = new Size(200, 40), Font = new Font(Font.FontFamily, 12), Text = "Готово" };
            Button Back = new Button() { Parent = this, Location = new Point(ClientSize.Width / 2 + 50, ClientSize.Height / 2 + 155), Size = new Size(200, 40), Font = new Font(Font.FontFamily, 12), Text = "Вернуться в лобби" };

            PlayersCountInput.ValueChanged += (object sender, EventArgs e) => {
                PlayersCountInputLabel.Text = "Количество игроков в игре: " + PlayersCountInput.Value;
            };

            Done.Click += (object sender, EventArgs e) => {
                Name = PlayerNameInput.Text;
                InLobby = false;
                LocalServer.SendFrame(new Frame(0, new CreateRoom(RoomNameInput.Text, Name, PlayersCountInput.Value), MessageType.Exchange, GameMessageType.CreateGame));
            };

            Back.Click += (object sender, EventArgs e) => {
                InitMain();
            };

        }

        public void InitWaitingRoom(EasyRoom room)
        {
            Controls.Clear();
            ClientSize = new Size(800, 600);
            Text = "Dice Game. Waiting room";

            Label RoomTitle = new Label() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 - 195), Size = new Size(500, 40), Font = new Font(Font.FontFamily, 24), Text = room.Name };

            Label PlyersCount = new Label() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 - 125), Size = new Size(500, 40), Font = new Font(Font.FontFamily, 12), Text = "Игроки: " + room.Players.Count + "/2" };
            Label PlayerName = new Label() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 - 85), Size = new Size(500, 40), Font = new Font(Font.FontFamily, 12), Text = "Ваш ник: " + Name };
            Label GameStatus = new Label() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 - 45), Size = new Size(500, 40), Font = new Font(Font.FontFamily, 12), Text = "Ожидаем подключения игроков" };

            Button Back = new Button() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 + 155), Size = new Size(500, 40), Font = new Font(Font.FontFamily, 12), Text = "Вернуться в лобби" };

            Back.Click += (object sender, EventArgs e) => {
                LocalServer.SendFrame(new Frame(room.Id, Name, MessageType.Disconnect, GameMessageType.Undefined));
                InLobby = true;
                RoomId = -1;
                InitMain();
            };
        }

        public void InitGameRoom(EasyRoom room)
        {
            Controls.Clear();
            ClientSize = new Size(800, 600);
            Text = "Dice Game. " + room.Name;
        }

        private bool IsContaninsRoom(int id, out EasyRoom Room, EList<EasyRoom> notes)
        {
            foreach (EasyRoom room in notes)
            {
                if (room.Id == id)
                {
                    Room = room;
                    return true;
                }
            }
            Room = null;
            return false;
        }

    }
}
