using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Server
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            new TcpServer();
        }
    }

    public class TcpServer
    {
        private readonly TcpListener Listener;
        private const int Port = 8888;
        private const string IpAddress = "127.0.0.1";

        public TcpServer()
        {
            var iPAddress = IPAddress.Parse(IpAddress);
            Listener = new TcpListener(iPAddress, Port);
            StartServer();
        }

        private void StartServer()
        {
            Listener.Start();
            Console.WriteLine($@"Server is listening on {IpAddress}@{Port.ToString()}");
            var p1 = Listener.AcceptTcpClient();
            var send = GetBytes(new Message {Code = Codes.WaitingForPlayer});
            Console.WriteLine($"Player 1 is connected");
            p1.GetStream().Write(send, 0, send.Length);
            var p2 = Listener.AcceptTcpClient();
            Console.WriteLine($"Player 2 is connected");
            p2.GetStream().Write(send, 0, send.Length);
            new Thread(RunGame).Start(new Game(p1, p2));
        }

        private void RunGame(object o)
        {
            if (!(o is Game game))
                return;

            game.StartTheGame();
        }

        private class Game
        {
            // The game has a board and 2 players with some other stuff.
            private const int BoardSize = 14;
            private Cell[][] GameBoard;
            Player _p1, _p2;

            public Game(TcpClient p1, TcpClient p2)
            {
                GameBoard = new Cell[BoardSize * 2][];
                for (var i = 0; i < BoardSize * 2; i++)
                    GameBoard[i] = new Cell[BoardSize];
                _p1 = new Player(1, p1);
                _p2 = new Player(2, p2);
            }

            public void StartTheGame()
            {
                new Thread(HandlePlayer).Start(_p1);
                new Thread(HandlePlayer).Start(_p2);
            }

            private void HandlePlayer(object obj)
            {
                var player = (Player) obj;
                var send = new Message
                {
                    Code = Codes.GameIsReady
                };
                player.Write(send);
                var receive = player.Read();
                if (receive.Code != Codes.PlayerIsReady)
                {
                    CloseGame();
                    return;
                }

                ShipsToBoard(player, receive);
                try
                {
                    while (true)
                    {
                        
                    }
                }
                catch (Exception)
                {
                    CloseGame();
                }
            }

            private void ShipsToBoard(Player p, Message message)
            {
                for (var i = 0; i < BoardSize * 2; i++)
                for (var j = 0; j < BoardSize; j++)
                    GameBoard[i + BoardSize * (p.PlayerId - 1)][j].HasShip = message.Matrix[i][j];
            }

            private void CloseGame()
            {
                _p1.Client.Close();
                _p2.Client.Close();
                _p1.Client.Dispose();
                _p2.Client.Dispose();
            }

            private class Player
            {
                public readonly TcpClient Client;
                public readonly int PlayerId;
                private readonly NetworkStream _stream;

                public Player(int playerId, TcpClient client)
                {
                    PlayerId = playerId;
                    Client = client;
                    _stream = Client.GetStream();
                }

                public override bool Equals(object obj)
                {
                    return obj is Player player && PlayerId == player.PlayerId;
                }

                public override int GetHashCode()
                {
                    return 2108858624 + PlayerId.GetHashCode();
                }

                public void Write(Message message)
                {
                    var data = GetBytes(message);
                    _stream.Write(data, 0, data.Length);
                }

                public Message Read()
                {
                    var data = new byte[1024];
                    var len = _stream.Read(data, 0, data.Length);
                    var json = Encoding.ASCII.GetString(data, 0, len);
                    return JsonConvert.DeserializeObject<Message>(json);
                }
            }

            private struct Cell
            {
                public bool Destroyed, HasShip;

                public Cell(bool hasShip = false, bool destroyed = false)
                {
                    HasShip = hasShip;
                    Destroyed = destroyed;
                }
            }
        }

        public static byte[] GetBytes(Message message)
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message).ToCharArray());
        }
    }
}