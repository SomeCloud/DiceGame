using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceGame
{
    public enum GameStatus
    {
        Wait,
        Game,
        Over,
        Close
    }

    // Полный класс данных комнаты
    [Serializable]
    public class Room
    {
        public int Id;
        public string Name;
        public int MaxPlayerCount;
        public List<Player> Players;
        public Player ActivePlayer;
        public GameStatus Status;

        public Room(int id, string name, int count)
        {
            Id = id;
            Name = name;
            MaxPlayerCount = count;
            Players = new List<Player>();
            Status = GameStatus.Wait;
        }

        public EasyRoom ToEasyRoom()
        {
            List<EasyPlayer> players = new List<EasyPlayer>();
            foreach (Player player in Players)
            {
                players.Add(player.ToEasyPlayer());
            }
            return new EasyRoom(Id, Name, MaxPlayerCount, players, ActivePlayer.Name, Status);
        }

        public bool AddPlayer(string name)
        {
            Player player;
            if (IsPlayerContains(name, out player) == false)
            {
                player = new Player(name);
                if (Players.Count == 0)
                {
                    ActivePlayer = player;
                }
                if (Players.Count == MaxPlayerCount - 1)
                {
                    Status = GameStatus.Game;
                }
                Players.Add(player);
                return true;
            }
            return false;
        }

        public bool IsPlayerContains(string name, out Player player)
        {
            foreach (Player pl in Players)
            {
                if (pl.Name == name)
                {
                    player = pl;
                    return true;
                }
            }
            player = null;
            return false;
        }

        public void RemovePlayer(string name)
        {
            foreach (Player player in Players)
            {
                if (player.Name == name)
                {
                    Players.Remove(player);
                    if (Players.Count <= 0)
                    {
                        Status = GameStatus.Close;
                    }
                }
            }
        }

        public void NextPlayer()
        {
            for (int i = 0; i < Players.Count; i++)
            {

                if (Players[i] == ActivePlayer)
                {
                    if (i + 1 < Players.Count)
                    {
                        ActivePlayer = Players[i + 1];
                        break;
                    }
                    else
                    {
                        ActivePlayer = Players[0];
                        break;
                    }
                }
            }
        }


    }

    // Для обмена данными внутри сети (облегченный вариант)
    [Serializable]
    public class EasyRoom
    {

        public delegate void OnChangeRoomStatusEvent(GameStatus status);
        public event OnChangeRoomStatusEvent ChangeRoomStatusEvent;

        public int Id;
        public string Name;
        public int MaxCount;
        public List<EasyPlayer> Players;
        public string ActivePlayer;
        public GameStatus Status;

        public GameStatus SetGameStatus
        {
            set
            {
                Status = value;
                ChangeRoomStatusEvent?.Invoke(value);
            }
            get => Status;
        }

        public EasyRoom(int id, string name, int count, List<EasyPlayer> players, string activeplayer, GameStatus status)
        {
            Id = id;
            Name = name;
            MaxCount = count;
            Players = players;
            ActivePlayer = activeplayer;
            Status = status;
        }


    }

    // Для отправки запроса на создание комнаты
    [Serializable]
    public class CreateRoom
    {
        public string Name;
        public string Player;
        public int Max;

        public CreateRoom(string name, string player, int max)
        {
            Name = name;
            Player = player;
            Max = max;
        }

        public Room ToRoom(int id)
        {
            Room room = new Room(id, Name, Max);
            room.AddPlayer(Player);
            return room;
        }
    }
}
