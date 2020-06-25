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
    class ServerForm: Form
    {

        private delegate void OnMoveEvent(Room room);
        private event OnMoveEvent MoveEvent;

        private RichTextBox Cnsl;
        private Client LocalClient;

        Thread LobbyThread;

        List<Room> Rooms;

        public ServerForm(string adress, int receivePort, int sendPort) : base()
        {
            LocalClient = new Client(adress, receivePort);
            Rooms = new List<Room>();

            LobbyThread = new Thread(new ParameterizedThreadStart((object obj) =>
            {
                Server LobbyServer;
                MoveEvent += (room) => {
                    LobbyServer = new Server(adress, sendPort);
                    LobbyServer.SendFrame(new Frame(room.Id, room.ToEasyRoom(), MessageType.Exchange, GameMessageType.Send));
                };
                while (true)
                {
                    for (int i = 0; i < Rooms.Count; i++)
                    {
                        LobbyServer = new Server(adress, sendPort);
                        LobbyServer.SendFrame(new Frame(Rooms[i].Id, Rooms[i].ToEasyRoom(), MessageType.Exchange, GameMessageType.Send));
                    }
                    Thread.Sleep(3000);
                }
            }))
            { Name = "ServerThread", IsBackground = true };
            LobbyThread.Start();

            LocalClient.StartReceive("ServerReceiver");


            Thread ProcessThread = new Thread(new ParameterizedThreadStart((object obj) =>
            {
                LocalClient.Receive += (frame) =>
                {
                    if (InvokeRequired) Invoke(new Action<Frame>((s) => ProcessMessage(frame)), frame);
                };
            }))
            { Name = "ProcessThread", IsBackground = true };
            ProcessThread.Start();


            Show();
        }

        public new void Close()
        {
            LocalClient.StopReceive();
            base.Close();
        }

        public void ProcessMessage(Frame frame)
        {
            switch (frame.MsgType)
            {
                case MessageType.Connect:
                    break;
                case MessageType.Exchange:
                    Room Room;
                    switch (frame.GameMsgType)
                    {
                        case GameMessageType.CreateGame:

                            if (IsContaninsRoom(frame.RoomId, out Room) == false)
                            {
                                Room = ((CreateRoom)frame.Data).ToRoom(Rooms.Count + 1);
                                Rooms.Add(Room);
                                Cnsl.AppendText("[SYSTEM]: создана комната " + Room.Name + " с идентификатором " + Room.Id + "\n");
                                Cnsl.AppendText("[" + Room.Name + "]: подключился игрок " + Room.ActivePlayer.Name + "\n");
                            }
                            break;
                        case GameMessageType.Connect:
                            if (IsContaninsRoom(frame.RoomId, out Room) == true)
                            {
                                if (Room.AddPlayer((string)frame.Data))
                                {
                                    Cnsl.AppendText("[" + Room.Name + "]: подключился игрок " + (string)frame.Data + "\n");
                                    MoveEvent?.Invoke(Room);
                                }
                            }
                            break;
                        case GameMessageType.Send:
                            if (IsContaninsRoom(frame.RoomId, out Room) == true)
                            {
                                int move = new Random(Convert.ToInt32((int)DateTime.Now.Ticks)).Next(1, 7);
                                if (move == 1)
                                {
                                    Cnsl.AppendText("[" + Room.Name + "]: игрок " + Room.ActivePlayer.Name + "сделал ход и ему выпало 1. Ход переходит к следующему игроку\n");
                                    Room.ActivePlayer.LastRound.Add(move);
                                    Room.NextPlayer();
                                    Room.ActivePlayer.LastRound.Clear();
                                }
                                else
                                {
                                    if (Room.ActivePlayer.Score + Room.ActivePlayer.LastRound.Sum() + move >= 100)
                                    {
                                        Room.ActivePlayer.LastRound.Add(move);
                                        Room.ActivePlayer.Rounds.Add(Room.ActivePlayer.LastRound.Sum());
                                        Room.ActivePlayer.LastRound.Clear();
                                        Cnsl.AppendText("[" + Room.Name + "]: игрок " + Room.ActivePlayer.Name + "сделал ход " + move + ", и победил в игре со счетом " + Room.ActivePlayer.Score + Room.ActivePlayer.LastRound.Sum() + move + "\n");
                                        Room.Status = GameStatus.Over;
                                    }
                                    else
                                    {
                                        Cnsl.AppendText("[" + Room.Name + "]: игрок " + Room.ActivePlayer.Name + "сделал ход и ему выпало " + move + "\n");
                                        Room.ActivePlayer.LastRound.Add(move);
                                    }
                                }
                                MoveEvent?.Invoke(Room);
                            }
                            break;
                        case GameMessageType.Wait:
                            if (IsContaninsRoom(frame.RoomId, out Room) == true)
                            {
                                if (Room.ActivePlayer.Name == frame.Data.ToString())
                                {
                                    Room.ActivePlayer.Rounds.Add(Room.ActivePlayer.LastRound.Sum());
                                    Room.NextPlayer();
                                    Room.ActivePlayer.LastRound.Clear();
                                    MoveEvent?.Invoke(Room);
                                }
                            }
                            break;
                    }
                    break;
                case MessageType.Disconnect:
                    if (IsContaninsRoom(frame.RoomId, out Room) == true)
                    {
                        if (Room.Players.Count < 2)
                        {
                            Cnsl.AppendText("[" + Room.Name + "]: игрок " + (string)frame.Data + " покидает игру\n");
                            Cnsl.AppendText("[SYSTEM]: комната " + Room.Name + " расформирована\n");
                            Rooms.Remove(Room);
                        }
                        else
                        {
                            Cnsl.AppendText("[" + Room.Name + "]: игрок " + (string)frame.Data + " покидает игру\n");
                            Room.RemovePlayer((string)frame.Data);
                            MoveEvent?.Invoke(Room);
                        }
                    }
                    break;
            }
        }

        private bool IsContaninsRoom(int id, out Room Room)
        {
            foreach (Room room in Rooms)
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

        public void SetConsole()
        {
            Cnsl = new RichTextBox() { Parent = this, Location = new Point(10, 10), Size = new Size(ClientSize.Width - 20, ClientSize.Height - 20) };
        }

    }
}
