using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace DiceGame
{
    class NotesView: Panel
    {
        //  Событие при получении данных от сервера
        public delegate void OnConnectEvent(GameNoteView NoteView, EasyRoom room);
        public event OnConnectEvent ConnectEvent;
        public delegate void OnWatchEvent(GameNoteView NoteView, EasyRoom room);
        public event OnWatchEvent WatchEvent;

        EList<GameNoteView> Rows;
        EList<EasyRoom> Notes;
        
        public NotesView(EList<EasyRoom> notes) : base()
        {
            Width = 780;
            AutoScroll = true;
            Notes = notes;

            Rows = new EList<GameNoteView>();

            GameNoteView row;

            for (int i = 0; i < notes.Count; i++)
            {
                GameNoteView temp;
                if (i > 0)
                {
                    temp = new GameNoteView(notes[i]) { Parent = this, Location = new Point(0, Rows.Last().Location.Y + 60) };
                }
                else
                {
                    temp = new GameNoteView(notes[i]) { Parent = this, Location = new Point(0, 0) };
                }
                temp.ConnectEvent += (r) => {
                    ConnectEvent?.Invoke(r, notes[i]);
                };
                temp.WatchEvent += (r) => {
                    WatchEvent?.Invoke(r, notes[i]);
                };
                Rows.Add(temp);
            }

            // обновление списка лобби при добвление новых записей
            Notes.AfterAddEvent += (item) => {
                if (Rows.Count > 0)
                {
                    row = new GameNoteView() { Parent = this, Location = new Point(0, Rows.Last().Location.Y + 60) };
                    if (row.InvokeRequired) row.Invoke(new Action<EasyRoom>((s) => row.SetSource(s)), item);
                    else row.SetSource(item);
                }
                else
                {

                    row = new GameNoteView() { Parent = this, Location = new Point(0, 0) };

                    if (row.InvokeRequired) row.Invoke(new Action<EasyRoom>((s) => row.SetSource(s)), item);
                    else row.SetSource(item);
                }
                row.ConnectEvent += (r) => {
                    ConnectEvent?.Invoke(r, item);
                };
                row.WatchEvent += (r) => {
                    WatchEvent?.Invoke(r, item);
                };
                Rows.Add(row);
            };

            // обновляем список лобби при удалении записи (при отключении сервера / завершении игры на сервере)
            Notes.BeforeRemoveEvent += (item) => {
                row = new GameNoteView(item);
                for (int i = Rows.Count - 1; i > Rows.IndexOf(row) ; i--)
                {
                    if (Rows[i].Equals(row) == false)
                    {
                        Rows[i].Location = Rows[i - 1].Location;
                    }
                }
                Rows.Remove(row);
            };

        }

        // Добавляем запись игры, если таковой нет в лобби
        public void AddNote(EasyRoom note)
        {
            if (IsExists(note.Id) == false)
            {
                Notes.Add(note);
            }
        }

        // Удаляем запись игры, если таковая имеется
        public void RemoveNote(EasyRoom note)
        {
            if (IsExists(note.Id) == true)
            {
                Notes.Remove(note);
            }
        }

        // Проверка записи на наличие в списке лобби
        private bool IsExists(int id)
        {
            foreach (EasyRoom note in Notes)
            {
                if (note.Id == id)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
