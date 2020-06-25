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
        ClientForm Client;

        Client LocalClient;
        Server LocalServer;

        EList<EasyRoom> Notes;

        public Form1()
        {
            InitializeComponent();            
            InitMain();
        }


        private void InitMain()
        {
            Controls.Clear();
            ClientSize = new Size(400, 200);
            Text = "Dice Game. Main";

            Button StartClient = new Button() { Parent = this, Location = new Point((ClientRectangle.Width / 2) - 150, (ClientRectangle.Height / 2) - 50), Size = new Size(300, 40), Text = "Запустить клиент" };
            Button StartServer = new Button() { Parent = this, Location = new Point((ClientRectangle.Width / 2) - 150, (ClientRectangle.Height / 2) + 10), Size = new Size(300, 40), Text = "Запустить сервер" };


            StartClient.Click += (object sender, EventArgs e) => {
                if (!(Client is null)) Client.Close(); 
                Client = new ClientForm(GroupAdress, ClientReceive, ServerReceive) { Size = new Size(800, 600), Text = "Client" };
                Client.Show();
                Client.FormClosing += (object esender, FormClosingEventArgs ee) => {
                    StartClient.Enabled = true;
                };
                StartClient.Enabled = false;
            };

            StartServer.Click += (object sender, EventArgs e) => {
                if (!(Server is null)) Server.Close();
                Server = new ServerForm(GroupAdress, ServerReceive, ClientReceive) { Size = new Size(400, 600), Text = "Sever" };
                Server.SetConsole();
                Server.FormClosing += (object esender, FormClosingEventArgs ee) => {
                    StartServer.Enabled = true;
                };
                StartServer.Enabled = false;
            };

        }
    }
}
