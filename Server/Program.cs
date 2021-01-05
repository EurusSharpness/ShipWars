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
        // ReSharper disable once ObjectCreationAsStatement
        private static void Main() => new TcpServer();
    }

    public class TcpServer
    {
        private readonly TcpListener _listener;
        private const int Port = 8888;
        private const string IpAddress = "127.0.0.1";

        public TcpServer()
        {
            var iPAddress = IPAddress.Parse(IpAddress);
            _listener = new TcpListener(iPAddress, Port);
            StartServer();
        }

        private void StartServer()
        {
            _listener.Start();
            Console.WriteLine($@"Server is listening on {IpAddress}@{Port.ToString()}");
            var p1 = _listener.AcceptTcpClient();
            Console.WriteLine("Player 1 is connected");
            var p2 = _listener.AcceptTcpClient();
            Console.WriteLine("Player 2 is connected");
            new Thread(RunGame).Start(new Game(p1, p2));
        }

        private static void RunGame(object o)
        {
            if (!(o is Game game))
                return;

            game.StartTheGame();
        }

        private class Game
        {
            // The game has a board and 2 players with some other stuff.
            private const int BoardSize = 14;
            private readonly Cell[][] _gameBoard;
            private int _playerTurn = 1;
            readonly Player _p1, _p2;
            private int _playersReady;
            public Game(TcpClient p1, TcpClient p2)
            {
                _gameBoard = new Cell[BoardSize * 2][];
                for (var i = 0; i < BoardSize * 2; i++)
                    _gameBoard[i] = new Cell[BoardSize];
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
                try
                {
                    var player = (Player) obj;
                    var send = new Message
                    {
                        Code = Codes.GameIsReady
                    };
                    player.Write(send);
                    var receive = player.Read();
                    Console.WriteLine("ahmed " + receive.Code);
                    if (receive.Code != Codes.PlayerIsReady)
                    {
                        CloseGame();
                        return;
                    }

                    _playersReady++;
                    ShipsToBoard(player, receive);
                    if (player.PlayerId == 1)
                    {
                        send = new Message {Code = Codes.YouGoFirst};
                        player.Write(send);
                    }
                    else
                    {
                        send = new Message {Code = Codes.YouGoSecond};
                        player.Write(send);
                    }
                    while (true)
                    {
                        receive = player.Read();
                        if (_playersReady != 2)
                        {
                            send = new Message {Code = Codes.WaitingForPlayer};
                            player.Write(send);
                            continue;
                        }
                        if (receive.Code == Codes.CellClicked)
                        {
                            if (_playerTurn == player.PlayerId)
                            {
                                CellCliecked(receive.Row, receive.Column);
                            }
                            else
                            {
                                send = new Message {Code = Codes.NotYourTurn};
                                player.Write(send);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("something went wrong\n"+e.Message);
                    var send = new Message{Code = Codes.Disconnected};
                    _p1.Write(send);
                    _p2.Write(send);
                    CloseGame();
                }
            }
            
            private void CellCliecked(int row, int col)
            {
                var updateBoard = new Message
                {
                    Code = Codes.UpdateBoard,
                    HasShip = _gameBoard[row][col].HasShip,
                    Row = row + BoardSize * (_playerTurn == _p1.PlayerId ? 0 : 1),
                    Column = col
                };

                _p1.Write(updateBoard);
                updateBoard.Row = row + BoardSize * (_playerTurn == _p1.PlayerId ? 1 : 0);
                _p2.Write(updateBoard);
                if (_playerTurn == _p1.PlayerId)
                {
                    if (_gameBoard[row + BoardSize][col].HasShip)
                        _p2.HitPoints--;
                    if (_p2.HitPoints == 0)
                    {
                        var send = new Message {Code = Codes.YouLost};
                        _p2.Write(send);
                        send.Code = Codes.YouWon;
                        _p1.Write(send);
                        CloseGame();
                        return;
                    }

                    _gameBoard[row][col].Destroyed = true;
                    _playerTurn = 2;
                }
                else
                {
                    if (_gameBoard[row][col].HasShip)
                        _p1.HitPoints--;
                    if (_p1.HitPoints == 0)
                    {
                        var send = new Message {Code = Codes.YouLost};
                        _p1.Write(send);
                        send.Code = Codes.YouWon;
                        _p2.Write(send);
                        CloseGame();
                        return;
                    }

                    _gameBoard[row][col].Destroyed = true;
                    _playerTurn = 1;
                }
            }

            private void ShipsToBoard(Player p, Message message)
            {
                for (var i = 0; i < BoardSize; i++)
                {
                    for (var j = 0; j < BoardSize; j++)
                    {
                        _gameBoard[i + BoardSize * (p.PlayerId - 1)][j].HasShip = message.Matrix[i][j];
                        Console.Write($"{message.Matrix[i][j].ToString()}, ");
                    }
                    Console.WriteLine();
                }
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
                public int HitPoints = 32;

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
                    try
                    {
                        var data = GetBytes(message);
                        _stream.Write(data, 0, data.Length);
                    }
                    catch (Exception)
                    {
                        throw new Exception($"Player {PlayerId.ToString()} Diconnected");
                    }
                }
                
                public Message Read()
                {
                    try
                    {
                        var data = new byte[2048];
                        var len = _stream.Read(data, 0, data.Length);
                        var json = Encoding.ASCII.GetString(data, 0, len);
                        return JsonConvert.DeserializeObject<Message>(json);
                    }
                    catch (Exception)
                    {
                        throw new Exception($"Player {PlayerId.ToString()} Diconnected");
                    }
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