using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceGame
{
    public class Player
    {

        public string Name;
        public List<int> Rounds;
        public List<int> LastRound;

        public int Score { get => Rounds.Sum(); }

        public Player(string name)
        {
            Name = name;
            Rounds = new List<int>();
            LastRound = new List<int>();
        }

        public EasyPlayer ToEasyPlayer()
        {
            return new EasyPlayer(Name, Score, LastRound.LastOrDefault());
        }

    }

    [Serializable]
    public class EasyPlayer
    {
        public string Name;
        public int Score;
        public int Move;

        public EasyPlayer(string name, int score, int move)
        {
            Name = name;
            Score = score;
            Move = move;
        }

    }
}
