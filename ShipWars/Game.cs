using System;
using System.Drawing;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace ShipWars
{
    public static class Messages
    {
        public static string NotReady => @"You must put all your ships on the grid to start";
        public static string SelectFromEnemy => @"You must select a cell from the enemy grid";
        public static string CellAlreadyDestroyed => @"This cell is already destroyed";
        public static string NotYourTurn => @"Wait for your turn";
        public static string YouWon => @"You Won the Game, GZZZZZZZ!!!!";
        public static string YouLost => @"You Lost the game BOOOOOOOOOOOOOOOO!!!";
    }

    public class Game
    {
        private readonly GameBackground _background;
        private readonly GameBoard _gameBoard;
        private Player _player, _enemy;
        private bool _isReady;
        private string Message;
        private int _alpha = 255;
        private bool _messageFlag;
        private bool _playing; // true => player, false => enemy

        public Game()
        {
            _background = new GameBackground();
            _gameBoard = new GameBoard();
            _player = new Player(true);
            _enemy = new Player(false);
            _enemy.GenerateRandomFleet();
            IsReady();
        }


        private void IsReady()
        {
            var RandomButton = new Button()
            {
                Name = @"RandomButton",
                Text = @"Generate Random Fleet",
                AutoSize = true,
                Font = new Font("ALGERIAN", ShipWarsForm._ClientSize.Height / 24f, FontStyle.Italic | FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                FlatAppearance =
                    {BorderSize = 0},
            };
            RandomButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, Color.DarkGray);
            RandomButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            RandomButton.MouseEnter += (sender, args) => RandomButton.ForeColor = Color.Aquamarine;
            RandomButton.MouseLeave += (sender, args) => RandomButton.ForeColor = Color.Black;
            RandomButton.MouseClick += (sender, args) => _player.GenerateRandomFleet();
            RandomButton.Size = TextRenderer.MeasureText(RandomButton.Text, RandomButton.Font);
            RandomButton.Location = new Point(
                (ShipWarsForm._ClientSize.Width - RandomButton.Width) / 2,
                (int) (ShipWarsForm._ClientSize.Height * 0.80f)
            );
            ShipWarsForm._Collection.Add(RandomButton);

            var b = new Button()
            {
                Text = @"S T A R T",
                Font = new Font("ALGERIAN", ShipWarsForm._ClientSize.Height / 24f, FontStyle.Italic | FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = Color.Blue,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true
            };
            b.Size = TextRenderer.MeasureText(b.Text, b.Font);
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.Transparent;
            b.FlatAppearance.MouseDownBackColor = Color.Transparent;
            b.MouseEnter += (s, e) => { b.ForeColor = Color.Red; };
            b.MouseLeave += (s, e) => { b.ForeColor = Color.Blue; };
            b.MouseClick += (s, e) =>
            {
                _isReady = _player.IsReady();
                if (!_isReady)
                {
                    _alpha = 255;
                    Message = Messages.NotReady;
                    _messageFlag = true;
                }
                else
                {
                    _playing = true;
                    b.Enabled = false;
                    b.Visible = false;
                    RandomButton.Visible = RandomButton.Enabled = false;
                    RandomButton.Dispose();
                    ShipsToBoard();
                    b.Dispose();
                }
            };
            b.Location = new Point(
                (ShipWarsForm._ClientSize.Width - b.Width) / 2,
                (int) (ShipWarsForm._ClientSize.Height * 0.87f)
            );
            ShipWarsForm._Collection.Add(b);
        }

        private void ShipsToBoard()
        {
            foreach (var ship in _enemy.BattleShips)
            {
                foreach (var cell in ship.IndexPoints.Select(index => _gameBoard.Board[index.Y][index.X]))
                {
                    cell.Color = new SolidBrush(Color.Transparent);
                    cell.Cratif = true;
                }

                ship.Visible = false;
                ship.Enabled = false;
                ship.Dispose();
            }

            foreach (var ship in _player.BattleShips)
            {
                foreach (var cell in ship.IndexPoints.Select(index => _gameBoard.Board[index.Y + GameBoard.BoardSize][index.X]))
                {
                    cell.Color = new SolidBrush(ship.BackColor);
                    cell.Cratif = true;
                }
                ship.Visible = false;
                ship.Enabled = false;
                ship.Dispose();
            }

            _player.BattleShips = null;
        }

        public void MouseMove(MouseEventArgs e)
        {
            if (_isReady)
                _gameBoard.MouseMove(e);
        }

        public void MouseUp(MouseEventArgs e)
        {
            if (!_isReady) return;

            _gameBoard.MouseUp(e);
        }

        #region POTAT

        public void MouseDown(MouseEventArgs e)
        {
            if (!_isReady || _gameBoard._selectedCell.X == -1 || !_playing) return;
            var selectedCell = _gameBoard.Board[_gameBoard._selectedCell.X][_gameBoard._selectedCell.Y];
            if (_gameBoard._selectedCell.X >= GameBoard.BoardSize)
            {
                _messageFlag = true;
                _alpha = 255;
                Message = Messages.SelectFromEnemy;
                return;
            }

            if (selectedCell.Destroyed)
            {
                _messageFlag = true;
                _alpha = 255;
                Message = Messages.CellAlreadyDestroyed;
                return;
            }

            _gameBoard.MouseDown(e);
            if (selectedCell.Cratif)
            {
                _enemy.HealthPoints--;
                selectedCell.Color = Brushes.Crimson;
                if (_enemy.HealthPoints == 0)
                {
                    _alpha = 255;
                    _messageFlag = true;
                    Message = Messages.YouWon;
                    _playing = false;
                }
            }
            EnemyDoStuff(e);
        }

        private void EnemyDoStuff(MouseEventArgs e)
        {
            var r = new Random();
            REPEAT:
            var col = r.Next(0, GameBoard.BoardSize);
            var row = r.Next(0, GameBoard.BoardSize);
            var selectedCell = _gameBoard.Board[row + GameBoard.BoardSize][col];
            if (selectedCell.Destroyed)
                goto REPEAT;
            selectedCell.MouseClick(e);
            if (!selectedCell.Cratif) return;
            _player.HealthPoints--;
            _alpha = 255;
            _messageFlag = true;
            Message = _player.HealthPoints.ToString();
            selectedCell.Color = Brushes.Crimson;
            if (_player.HealthPoints != 0) return;
            _alpha = 255;
            _messageFlag = true;
            Message = Messages.YouLost;
            _playing = false;
        }

        #endregion

        public void Draw(Graphics g)
        {
            _background.Draw(g);


            if (_isReady)
                _gameBoard.Draw(g);
            else
                _player.Draw(g);

            if (!_messageFlag) return;
            g.DrawString(
                Message, new Font("", 28), new SolidBrush(Color.FromArgb(_alpha, 255, 0, 0)),
                new PointF(
                    (ShipWarsForm._ClientSize.Width - g.MeasureString(Message, new Font("", 28)).Width) / 2, 50));
            _alpha-=5;
            if (_alpha != 0) return;
            _messageFlag = false;
            _alpha = 255;
        }
    }
}