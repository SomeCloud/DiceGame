using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace DiceGame
{
    // класс записи-вида для формы лобби
    class GameNoteView: Panel
    {
        // событие вызываемое при нажатии на кнопку подключения к серверу
        public delegate void OnConnectEvent(GameNoteView note);
        public event OnConnectEvent ConnectEvent;
        public delegate void OnWatchEvent(GameNoteView note);
        public event OnWatchEvent WatchEvent;

        // класс-котроллер (по-крайней мере им задумывался)
        EasyRoom Source;

        private Panel Status;
        private Label PlayersCount;
        private Label RoomName;
        private Button Connect;
        private Button Watch;

        public GameNoteView(): base()
        {

        }

        public GameNoteView(EasyRoom note) : base()
        {
            Source = note;
            SetAll();
        }

        // обновляем данные записи (при получении фрейма с новым статусом сервера)
        public void SetSource(EasyRoom note)
        {
            Source = note;
            SetAll();
        }

        // отображаем данные записи
        private void SetAll()
        {
            Size = new Size(760, 50);

            Status = new Panel() { Parent = this, Location = new Point(0, 0), Size = new Size(30, 50), BackColor = Color.IndianRed };
            RoomName = new Label() { Parent = this, Location = new Point(40, 10), Size = new Size(300, 30), Font = new Font(Font.FontFamily, 18), Text = Source.Name };
            PlayersCount = new Label() { Parent = this, Location = new Point(450, 10), Size = new Size(100, 30), Font = new Font(Font.FontFamily, 18), Text = Source.Players.Count + "/" + Source.MaxCount };
            Watch = new Button() { Parent = this, Location = new Point(550, 0), Size = new Size(100, 50), Font = new Font(Font.FontFamily, 9), Text = "Наблюдать" };
            Connect = new Button() { Parent = this, Location = new Point(660, 0), Size = new Size(100, 50), Font = new Font(Font.FontFamily, 9), Text = "Подключиться" };

            SetStatusColor(Source.Status);

            // вызываем собтие, если пользователь нажал на кнопку "Подключиться"
            Connect.Click += (object sender, EventArgs e) => {
                ConnectEvent?.Invoke(this);
            };

            // вызываем собтие, если пользователь нажал на кнопку "Наблюдать"
            Watch.Click += (object sender, EventArgs e) => {
                WatchEvent?.Invoke(this);
            };

            // если статус сервера изменился - обновляем данные
            Source.ChangeRoomStatusEvent += (status) => {
                SetStatusColor(status);
            };
        }

        private void SetStatusColor(GameStatus status)
        {
            switch (status)
            {
                case GameStatus.Game:
                    Status.BackColor = Color.IndianRed;
                    Connect.Enabled = false;
                    break;
                case GameStatus.Wait:
                    Status.BackColor = Color.LightGreen;
                    Connect.Enabled = true;
                    break;
            }
        }

    }
}
