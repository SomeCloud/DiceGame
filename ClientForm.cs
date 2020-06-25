using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;


namespace DiceGame
{
    class ClientForm: Form
    {

        private bool InLobby = true;
        private bool Watcher = false;
        private string Name;
        private int RoomId = -1;

        Client LocalClient;
        Server LocalServer;

        EList<EasyRoom> Notes;

        public ClientForm(string adress, int receivePort, int sendPort) : base()
        {
            LocalClient = new Client(adress, receivePort);
            LocalServer = new Server(adress, sendPort);
        }

        public new void Show()
        {

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
                                    InitGameOverRoom(Room);
                                    break;
                                case GameStatus.Close:
                                    break;
                            }
                        }
                    }
                }
                ), frame);
            };
            base.Show();
            InitMain();
        }

        public new void Close()
        {
            LocalClient.StopReceive();
            LocalServer.StopSending();
            base.Close();
        }

        private void InitMain()
        {
            Controls.Clear();
            ClientSize = new Size(800, 600);
            Text = "Dice Game. Main";

            Watcher = false;

            Button StartServer = new Button() { Parent = this, Location = new Point(ClientRectangle.Width - 260, 10), Size = new Size(250, 40), Text = "Запустить сервер" };
            Button StartRoom = new Button() { Parent = this, Location = new Point(10, 10), Size = new Size(250, 40), Text = "Создать комнату" };

            NotesView Lobbys = new NotesView(Notes) { Parent = this, Location = new Point(10, 60), Height = 530 };

            Lobbys.ConnectEvent += (note, room) => {
                InitConnectToRoomForm(room);
            };

            Lobbys.WatchEvent += (note, room) => {
                InLobby = false;
                RoomId = room.Id;
                Watcher = true;
                Name = "Watcher";
            };

            StartRoom.Click += (object sender, EventArgs e) => {
                InitCreateRoomForm();
            };

        }

        public void InitConnectToRoomForm(EasyRoom room)
        {
            Controls.Clear();
            ClientSize = new Size(800, 600);
            Text = "Dice Game. Waiting room";

            Label RoomTitle = new Label() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 - 195), Size = new Size(500, 40), Font = new Font(Font.FontFamily, 24), Text = room.Name };
            Label PlayerNameInputLabel = new Label() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 + 55), Size = new Size(500, 40), Font = new Font(Font.FontFamily, 12), Text = "Введетие ваш ник" };
            TextBox PlayerNameInput = new TextBox() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 + 105), Size = new Size(500, 40), Font = new Font(Font.FontFamily, 12) };

            Button Done = new Button() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 + 155), Size = new Size(200, 40), Font = new Font(Font.FontFamily, 12), Text = "Готово" };
            Button Back = new Button() { Parent = this, Location = new Point(ClientSize.Width / 2 + 50, ClientSize.Height / 2 + 155), Size = new Size(200, 40), Font = new Font(Font.FontFamily, 12), Text = "Вернуться в лобби" };

            Done.Click += (object sender, EventArgs e) => {
                Name = PlayerNameInput.Text;
                LocalServer.SendFrame(new Frame(room.Id, Name, MessageType.Exchange, GameMessageType.Connect));
                RoomId = room.Id;
                InLobby = false;
            };

            Back.Click += (object sender, EventArgs e) => {
                InLobby = true;
                RoomId = -1;
                InitMain();
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
            TrackBar PlayersCountInput = new TrackBar() { Parent = this, Location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 + 5), Size = new Size(500, 40), Minimum = 2, Maximum = 5 };

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
            Text = "Dice Game. " + room.Name + " - Game";

            GameView gameView = new GameView(room, Name, Watcher) { Parent = this, Location = new Point(10, 10), Height = 400 };

            gameView.ClickRoll += () =>
            {
                LocalServer.SendFrame(new Frame(room.Id, Name, MessageType.Exchange, GameMessageType.Send));
            };

            gameView.ClickStop += () =>
            {
                LocalServer.SendFrame(new Frame(room.Id, Name, MessageType.Exchange, GameMessageType.Wait));
            };

        }

        public void InitGameOverRoom(EasyRoom room)
        {
            Controls.Clear();
            ClientSize = new Size(800, 600);
            Text = "Dice Game. " + room.Name + " - Game over";

            int k = 0;
            List<EasyPlayer> players = room.Players.OrderByDescending(u => u.Score).ToList();

            EList<Label> PlayersList = new EList<Label>();

            Label title = new Label { Parent = this, Location = new Point(10, 10), Width = 780, Text = "Игра окончена. Итоги: ", Font = new Font(Font.FontFamily, 14) };

            foreach (EasyPlayer player in players)
            {
                k++;
                PlayersList.Add(new Label { Parent = this, Location = new Point(10, 60 + k * 40), Width = 780, Text = player.Name + ": " + player.Score, Font = new Font(Font.FontFamily, 14) });
            }

            Button BackToLobby = new Button() { Parent = this, Location = new Point(ClientRectangle.Width / 2 - 150, PlayersList.Last().Location.Y + 60), Size = new Size(300, 40), Text = "Вернуться в лобби", Font = new Font(Font.FontFamily, 14) };

            BackToLobby.Click += (object sender, EventArgs e) => {
                LocalServer.SendFrame(new Frame(room.Id, Name, MessageType.Disconnect, GameMessageType.Undefined));
                InLobby = true;
                RoomId = -1;
                InitMain();
            };

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
