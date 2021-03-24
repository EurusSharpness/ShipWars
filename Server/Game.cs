using System;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    public class Game
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
                var player = (Player)obj;
                var send = new Message
                {
                    Code = Codes.GameIsReady
                };
                player.Write(send);
                var receive = player.Read();
                if (receive == null)
                {
                    throw new NullReferenceException();
                }
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
                    send = new Message { Code = Codes.YouGoFirst };
                    player.Write(send);
                }
                else
                {
                    send = new Message { Code = Codes.YouGoSecond };
                    Console.WriteLine($"This is the line before the game starts");
                    player.Write(send);
                }
                send = new Message { Code = Codes.WaitingForPlayer };
                player.Write(send);
                while (_playersReady < 2) { }
                Console.WriteLine($"Out of the while fellas {player.PlayerId}");
                send = new Message { Code = Codes.StartPlaying };
                player.Write(send);
                while (true)
                {
                    receive = player.Read();
                    if (receive == null)
                        continue;
                    Console.WriteLine(receive.Code);
                    if (receive.Code == Codes.CellClicked)
                    {
                        if (_playerTurn == player.PlayerId)
                        {
                            CellCliecked(receive.Row, receive.Column);
                            Console.WriteLine($"Player #{player.PlayerId} has clicked the cell {receive.Row} {receive.Column} {_gameBoard[receive.Row][receive.Column].HasShip}\n" +
                                              $"Player #1 HP = {_p1.HitPoints}   Player #2 HP = {_p2.HitPoints}");
                        }
                        else
                        {
                            send = new Message { Code = Codes.NotYourTurn };
                            player.Write(send);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"something went wrong\n{e.Message}\n{e.StackTrace}");
                var send = new Message { Code = Codes.Disconnected };
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
                Row = row + BoardSize * (_playerTurn == _p1.PlayerId ? 0 : 1),
                HasShip = _gameBoard[row + (_playerTurn == 1 ? BoardSize : 0)][col].HasShip,
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
                    var send = new Message { Code = Codes.YouLost };
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
                    var send = new Message { Code = Codes.YouLost };
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
            Console.WriteLine("got here\n----------------------------------------------------------");

            for (var i = 0; i < BoardSize; i++)
            {
                for (var j = 0; j < BoardSize; j++)
                {
                    Console.Write($"{message.Matrix[i][j].ToString()}, ");
                    _gameBoard[i + BoardSize * (p.PlayerId - 1)][j] = new Cell(message.Matrix[i][j], false);
                }
                Console.WriteLine();
            }
            Console.WriteLine("----------------------------------------------------------");
        }

        private void CloseGame()
        {
            _p1.Client.Close();
            _p2.Client.Close();
            _p1.Client.Dispose();
            _p2.Client.Dispose();
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
}