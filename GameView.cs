using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace DiceGame
{
    class GameView: Panel
    {
        delegate bool lam();
        public delegate void OnClickRollEvent();
        public delegate void OnClickStopEvent();
        public event OnClickRollEvent ClickRoll;
        public event OnClickStopEvent ClickStop;
        EList<PlayerNoteView> Playerrows;
        List<EasyPlayer> Notes;
        Button RollButton;
        Button StopButton;


        public GameView(EasyRoom room, string localPlayer, bool IsWatcher) : base()
        {
            Width = 780;
            AutoScroll = true;
            Notes = room.Players;
            lam s = () => { return false; };

            Playerrows = new EList<PlayerNoteView>();

            for (int i = 0; i < room.Players.Count; i++)
            {
                s = () => { if (room.Players[i].Name == room.ActivePlayer) return true; else return false; };
                PlayerNoteView pl;
                if (i > 0)
                {
                    pl = new PlayerNoteView(room.Players[i], s()) { Parent = this, Location = new Point(0, Playerrows.Last().Location.Y + 60) };
                }
                else
                {
                    pl = new PlayerNoteView(room.Players[i], s()) { Parent = this, Location = new Point(0, 0) };
                }

                Playerrows.Add(pl);
            }

            /*foreach (EasyPlayer player in Notes) 
            {
                s = ()=> { if (player.Name == room.ActivePlayer) return true; else return false; };
                PlayerNoteView pl;
                if (player.Id > 1)
                {
                    pl = new PlayerNoteView(player, s()) { Parent = this, Location = new Point(0, Playerrows.Last().Location.Y + 60) };
                }
                else 
                {
                    pl = new PlayerNoteView(player, s()) { Parent = this, Location = new Point(0, 0) };
                }

                Playerrows.Add(pl);
            }*/

            s = () => { if (room.ActivePlayer == localPlayer) return true; else return false; };

            RollButton = new Button() { Parent = this, Location = new Point(10, Playerrows.Last().Location.Y + 80), Size = new Size(200, 40), Text = "Roll", Enabled = s(), Visible = !IsWatcher };
            StopButton = new Button() { Parent = this, Location = new Point(220, Playerrows.Last().Location.Y + 80), Size = new Size(200, 40), Text = "Штап", Enabled = s(), Visible = !IsWatcher };

            RollButton.Click += (object sender, EventArgs e) => {
                ClickRoll?.Invoke();
            };

            StopButton.Click += (object sender, EventArgs e) => {
                ClickStop?.Invoke();
            };
        }


        public void Update(EasyRoom room, string localPlayer, bool IsWatcher) 
        {
            lam s = () => { return false; };
            Playerrows.Clear();
            /*foreach (EasyPlayer player in Notes)
            {
                s = () => { if (player == room.ActivePlayer) return true; else return false; };
                PlayerNoteView pl;
                if (player.Id > 1)
                {
                    pl = new PlayerNoteView(player, s()) { Parent = this, Location = new Point(0, Playerrows.Last().Location.Y + 60) };
                }
                else
                {
                    pl = new PlayerNoteView(player, s()) { Parent = this, Location = new Point(0, 0) };
                }


                Playerrows.Add(pl);
            }*/

            for (int i = 0; i < room.Players.Count; i++)
            {
                s = () => { if (room.Players[i].Name == room.ActivePlayer) return true; else return false; };
                PlayerNoteView pl;
                if (i > 0)
                {
                    pl = new PlayerNoteView(room.Players[i], s()) { Parent = this, Location = new Point(0, Playerrows.Last().Location.Y + 60) };
                }
                else
                {
                    pl = new PlayerNoteView(room.Players[i], s()) { Parent = this, Location = new Point(0, 0) };
                }

                Playerrows.Add(pl);
            }

            s = () => { if (room.ActivePlayer == localPlayer) return true; else return false; };
            RollButton = new Button() { Parent = this, Location = new Point(10, Playerrows.Last().Location.Y + 80), Size = new Size(200, 40), Text = "Roll", Enabled = s() };
            StopButton = new Button() { Parent = this, Location = new Point(220, Playerrows.Last().Location.Y + 80), Size = new Size(200, 40), Text = "Штап", Enabled = s() };

            RollButton.Click += (object sender, EventArgs e) => {
                ClickRoll?.Invoke();
            };

            StopButton.Click += (object sender, EventArgs e) => {
                ClickStop?.Invoke();
            };
        }
    }
}
