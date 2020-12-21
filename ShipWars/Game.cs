using System;
using System.Drawing;
using System.Linq;
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
        private readonly Player _player, _enemy;
        private bool _isReady;
        private string _message;
        private int _alpha = 255;
        private bool _messageFlag;
        private bool _playing; // true => player, false => enemy

        public bool PlayAgain;
        public bool BackToMainMenu;

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
            var randomButton = new Button()
            {
                Name = @"RandomButton",
                Text = @"Generate Random Fleet",
                AutoSize = true,
                Font = new Font("ALGERIAN", ShipWarsForm.CanvasSize.Height / 24f, FontStyle.Italic | FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                FlatAppearance =
                    {BorderSize = 0},
            };
            randomButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
            randomButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            randomButton.MouseEnter += (sender, args) => randomButton.ForeColor = Color.Aquamarine;
            randomButton.MouseLeave += (sender, args) => randomButton.ForeColor = Color.Black;
            randomButton.MouseClick += (sender, args) => _player.GenerateRandomFleet();
            randomButton.MouseUp += (sender, args) =>
            {
                if (Form.ActiveForm != null) Form.ActiveForm.ActiveControl = null;
            };
            randomButton.Size = TextRenderer.MeasureText(randomButton.Text, randomButton.Font);
            randomButton.Location = new Point(
                (ShipWarsForm.CanvasSize.Width - randomButton.Width) / 2,
                (int)(ShipWarsForm.CanvasSize.Height * 0.80f)
            );
            ShipWarsForm.Collection.Add(randomButton);

            var b = new Button()
            {
                Text = @"S T A R T",
                Font = new Font("ALGERIAN", ShipWarsForm.CanvasSize.Height / 24f, FontStyle.Italic | FontStyle.Bold),
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
                    _message = Messages.NotReady;
                    _messageFlag = true;
                }
                else
                {
                    _playing = true;
                    b.Enabled = false;
                    b.Visible = false;
                    randomButton.Visible = randomButton.Enabled = false;
                    randomButton.Dispose();
                    ShipsToBoard();
                    b.Dispose();
                }
            };
            b.MouseUp += (ppp, eee) =>
            {
                if (Form.ActiveForm != null) Form.ActiveForm.ActiveControl = null;
            };
            b.Location = new Point(
                (ShipWarsForm.CanvasSize.Width - b.Width) / 2,
                (int)(ShipWarsForm.CanvasSize.Height * 0.87f)
            );
            ShipWarsForm.Collection.Add(b);
            Form.ActiveForm?.Focus();
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
                foreach (var cell in ship.IndexPoints.Select(index =>
                    _gameBoard.Board[index.Y + GameBoard.BoardSize][index.X]))
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
                _message = Messages.SelectFromEnemy;
                return;
            }

            if (selectedCell.Destroyed)
            {
                _messageFlag = true;
                _alpha = 255;
                _message = Messages.CellAlreadyDestroyed;
                return;
            }

            _gameBoard.MouseDown(e);
            if (selectedCell.Cratif)
                _enemy.HealthPoints--;
            if (_enemy.HealthPoints == 0) GameOver();
            EnemyDoStuff(e);
        }

        private void EnemyDoStuff(MouseEventArgs e)
        {
            if (!_playing) return;
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
            if (_player.HealthPoints == 0) GameOver();
        }

        private void GameOver()
        {
            _alpha = 255;
            _messageFlag = true;
            _message = (_player.HealthPoints == 0) ? Messages.YouLost : Messages.YouWon;
            _playing = false;
            var result = MessageBox.Show(@"Care for a rematch mate?", @"Restart", MessageBoxButtons.YesNo);
            switch (result)
            {
                case DialogResult.Yes:
                    PlayAgain = true;
                    break;
                case DialogResult.No:
                    BackToMainMenu = true;
                    break;
            }
        }

        #endregion

        public void Draw(Graphics g)
        {
            _background.Draw(g);
            DrawMessage(g);
            if (!_isReady)
            {
                Player.Draw(g);
                return;
            }

            _gameBoard.Draw(g);
            var font = new Font(FontFamily.GenericMonospace, 28, FontStyle.Bold);
            g.DrawString(@"Player Health: " + _player.HealthPoints + "\n\nEnemy Health: " + _enemy.HealthPoints, font, Brushes.DarkTurquoise, new PointF(0,100));
            DrawGameOver(g);
        }

        private void DrawGameOver(Graphics g)
        {
            if (_playing) return;

            var font = new Font(FontFamily.GenericMonospace, 28, FontStyle.Bold);


            g.FillRectangle(new SolidBrush(Color.FromArgb(160, Color.Black)),
                new Rectangle(new Point(), ShipWarsForm.CanvasSize));
            g.DrawString(
                (_player.HealthPoints == 0) ? Messages.YouLost : Messages.YouWon, font
                ,
                new SolidBrush(Color.FromArgb(_alpha, 255, 0, 0)),
                new PointF(
                    (ShipWarsForm.CanvasSize.Width - g.MeasureString(_message, font).Width) / 2, 50));
        }

        private void DrawMessage(Graphics g)
        {
            if (!_messageFlag) return;
            var font = new Font(FontFamily.GenericMonospace, 28, FontStyle.Bold);
            g.DrawString(
                _message, font,
                new SolidBrush(Color.FromArgb(_alpha, 255, 0, 0)),
                new PointF(
                    (ShipWarsForm.CanvasSize.Width - g.MeasureString(_message, font).Width) / 2, 50));
            _alpha--;
            if (_alpha > 0) return;
            _messageFlag = false;
            _alpha = 255;
        }
    }
}