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

    public abstract class Game
    {
        protected GameBackground _background;
        protected GameBoard _gameBoard;
        protected Player _player;
        protected Player _enemy;
        protected bool _isReady;
        protected string _message;
        protected int _alpha = 255;
        protected bool _messageFlag;
        protected bool _playing;

        public bool PlayAgain;
        public bool BackToMainMenu;
        protected Button StartButton;
        public Game()
        {
            
        }

        /// <summary>
        /// Create the "Generate Random Fleet" and "Start" Buttons and add functionality to them
        /// </summary>
        protected void IsReady()
        {
            StartButton = new Button()
            {
                Text = @"S T A R T",
                Font = new Font("ALGERIAN", ShipWarsForm.CanvasSize.Height / 24f, FontStyle.Italic | FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = Color.Blue,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true
            };
            StartButton.Size = TextRenderer.MeasureText(StartButton.Text, StartButton.Font);
            StartButton.FlatAppearance.BorderSize = 0;
            StartButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            StartButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, Color.LightGray);
            StartButton.MouseEnter += (s, e) => { StartButton.ForeColor = Color.Red; };
            StartButton.MouseLeave += (s, e) => { StartButton.ForeColor = Color.Blue; };
            
            StartButton.MouseUp += (ppp, eee) =>
            {
                if (Form.ActiveForm != null) Form.ActiveForm.ActiveControl = null;
            };
            StartButton.Location = new Point(
                (ShipWarsForm.CanvasSize.Width - StartButton.Width) / 2,
                (int) (ShipWarsForm.CanvasSize.Height * 0.87f)
            );
            ShipWarsForm.Collection.Add(StartButton);
            Form.ActiveForm?.Focus();
        }

        /// <summary>
        /// Add the player and enemy ships to the game board.
        /// </summary>
        protected virtual void ShipsToBoard()
        {
            foreach (var ship in _enemy.BattleShips)
            {
                foreach (var cell in ship.IndexPoints.Select(index => _gameBoard.Board[index.Y][index.X]))
                {
                    // Shall not see the Enemy ships.
                    cell.Color = new SolidBrush(Color.Transparent);
                    cell.ShipOverMe = true;
                }

                ship.Dispose();
            }

            foreach (var ship in _player.BattleShips)
            {
                foreach (var cell in ship.IndexPoints.Select(index =>
                    _gameBoard.Board[index.Y + GameBoard.BoardSize][index.X]))
                {
                    cell.Color = new SolidBrush(ship.BackColor);
                    cell.ShipOverMe = true;
                }

                ship.Dispose();
            }

            _player.BattleShips = null;
        }

        public void MouseMove(MouseEventArgs e)
        {
            if (!_isReady) return;
            _gameBoard.MouseMove(e);
        }

        public void MouseUp(MouseEventArgs e)
        {
            if (!_isReady) return;
            _gameBoard.MouseUp(e);
        }


        /// <summary>
        /// Check if the clicked cell is in board and not destroyed.
        /// If the Cell had a ship over it then reduce the other player by 1.
        /// If the other player HitPoints reached 0 then start
        /// <seealso cref="GameOver"/>.
        /// </summary>
        public abstract void MouseDown(MouseEventArgs e);


        /// <summary>
        /// Open a dialog and wait for the player to choose,
        /// <para>Yes: Start from setting the ships.</para>
        /// No: Go back to main menu.
        /// </summary>
        protected void GameOver()
        {
            _alpha = 255;
            _messageFlag = true;
            _message = (_player.HealthPoints == 0) ? Messages.YouLost : Messages.YouWon;
            _playing = false;
            var result = MessageBox.Show(@"Care for a rematch mate?", @"Restart", MessageBoxButtons.YesNo);
            PlayAgain = result == DialogResult.Yes;
            BackToMainMenu = !PlayAgain;
        }

        public abstract void Draw(Graphics g);

        protected void DrawGameOver(Graphics g)
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

        protected void DrawMessage(Graphics g)
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