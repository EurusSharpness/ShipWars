using System;
using System.Drawing;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Server;
using Message = Server.Message;

namespace ShipWars
{
    public class OnlineGame : Game
    {
        private TcpClient _client;
        private readonly NetworkStream _stream;
        private Codes _status;
        private bool  assembleFleet;
        private readonly Thread _writer;

        public OnlineGame()
        {
            _background = new GameBackground();
            _client = new TcpClient();
            _message = "Connecting to the server";
            _client.Connect("localhost", 8888);
            _message = "Connected\nWaiting for another player to join";
            _player = new Player(true);
            IsReady();
            foreach (var ship in _player.BattleShips)
            {
                ship.Hide();
            }

            StartButton.Hide();
            Start_Click();
            _player.randomFleet.Hide();
            _stream = _client.GetStream();
            new Thread(ReaderHandler).Start();
            _writer = new Thread(WriterHandler);
        }


        private void Init()
        {
            _background = new GameBackground();
            _gameBoard = new GameBoard();
            _enemy = new Player(false);
            StartButton.Show();
            _player.randomFleet.Show();
        }

        private void ShowPlayerBoard()
        {
            foreach (var ship in _player.BattleShips)
            {
                ship.Show();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void Start_Click()
        {
            StartButton.Click += (sender, args) =>
            {
                _isReady = _player.IsReady();
                if (!_isReady)
                {
                    _alpha = 255;
                    _message = Messages.NotReady;
                    _messageFlag = true;
                }
                else
                {
                    StartButton.Enabled = false;
                    StartButton.Visible = false;
                    ShipWarsForm.Collection.RemoveByKey("RandomButton");
                    ShipsToBoard();
                    StartButton.Dispose();
                    var send = new Message {Code = Codes.PlayerIsReady, Matrix = new bool[GameBoard.BoardSize][]};
                    for (var i = 0; i < GameBoard.BoardSize; i++)
                    {
                        send.Matrix[i] = new bool[GameBoard.BoardSize];
                        for (var j = 0; j < GameBoard.BoardSize; j++)
                        {
                            send.Matrix[i][j] = _gameBoard.Board[i + GameBoard.BoardSize][j].ShipOverMe;
                        }
                    }
                    Write(send);
                }
            };
        }

        private void ReaderHandler()
        {
            while (_client?.Connected != null && (bool) _client?.Connected)
                try
                {
                    var received = Read();
                    if (received == null)
                        continue;
                    _message = ($"received Code = {received.Code}");
                    _status = received.Code;
                    switch (_status)
                    {
                        case Codes.GameIsReady when _writer.ThreadState == ThreadState.Unstarted:
                            Init();
                            ShowPlayerBoard();
                            assembleFleet = true;
                            _writer.Start();
                            break;
                        case Codes.WaitingForPlayer:
                            break;
                        case Codes.UpdateBoard:
                            _gameBoard.Board[received.Row][received.Column].ShipOverMe = received.HasShip;
                            _gameBoard.Board[received.Row][received.Column].MouseClick();
                            break;
                        case Codes.YouLost:
                        case Codes.YouWon:
                            _playing = false;
                            _writer.Abort();
                            break;
                        case Codes.PlayerIsReady:
                            break;
                        case Codes.YouGoFirst:
                            break;
                        case Codes.YouGoSecond:
                            break;
                        case Codes.CellClicked:
                            break;
                        case Codes.NotYourTurn:
                            _message = Messages.NotYourTurn;
                            break;
                        case Codes.CellDestroyed:
                            break;
                        case Codes.Disconnected:
                            break;
                        case Codes.StartPlaying:
                            assembleFleet = false;
                            _playing = true;
                            break;
                    }
                }
                catch (Exception e)
                {
                    _message = ($"Lost connection to the server!\n{e.Message}");
                    _client.Close();
                    return;
                }
        }


        private void WriterHandler()
        {
            
        }


        public override void MouseDown(MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Left) return;
            if (!_isReady || _gameBoard.SelectedCell.X == -1 || !_playing) return;
            var selectedCell = _gameBoard.Board[_gameBoard.SelectedCell.X][_gameBoard.SelectedCell.Y];
            if (selectedCell.Destroyed)
            {
                _message += Messages.CellAlreadyDestroyed;
                return;
            }
            if (_gameBoard.SelectedCell.X >= GameBoard.BoardSize)
            {
                _messageFlag = true;
                _alpha = 255;
                _message = Messages.SelectFromEnemy;
                return;
            }

            var send = new Message
                {Code = Codes.CellClicked, Column = _gameBoard.SelectedCell.Y, Row = _gameBoard.SelectedCell.X};
            Write(send);
        }

        public override void Draw(Graphics g)
        {
            _background.Draw(g);
            DrawMsg(g);
            if (assembleFleet)
                Player.Draw(g);
            if(_playing)
                _gameBoard.Draw(g);
        }

        private void DrawMsg(Graphics g)
        {
            var font = new Font(FontFamily.GenericMonospace, 28, FontStyle.Bold);
            var format1 = new StringFormat(StringFormatFlags.NoClip)
            {
                LineAlignment = StringAlignment.Near,
                Alignment = StringAlignment.Near
            };

            g.DrawString(_message,
                font,
                Brushes.Red,
                new Rectangle(new Point(0, 100),
                    new Size(ShipWarsForm.CanvasSize.Width, ShipWarsForm.CanvasSize.Height)),
                format1);
        }

        #region ClientStuff

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Write(Message message)
        {
            try
            {
                var data = GetBytes(message);
                _stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                throw new Exception($"Server or Client are not doing fine\n{e.Message}\n{e.StackTrace}");
            }
        }

        private Message Read()
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
                return null;
            }
        }

        private string DecodeJSon(byte[] read, int len)
        {
            return Encoding.ASCII.GetString(read, 0, len);
        }

        public byte[] GetBytes(Message message)
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message).ToCharArray());
        }

        #endregion
    }
}