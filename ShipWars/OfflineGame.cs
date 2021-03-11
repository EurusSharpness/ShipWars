using System.Windows.Forms;
using System;
using System.Drawing;

namespace ShipWars
{
    public class OfflineGame : Game
    {
        public OfflineGame()
        {
            _background = new GameBackground();
            _gameBoard = new GameBoard();
            _player = new Player(true);
            _enemy = new Player(false);
            _enemy.GenerateRandomFleet();

            IsReady();
            
            StartButton.MouseClick += (s, e) =>
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
                    StartButton.Enabled = false;
                    StartButton.Visible = false;
                    ShipWarsForm.Collection.RemoveByKey("RandomButton");
                    ShipsToBoard();
                    StartButton.Dispose();
                }
            };
        }
        public override void MouseDown(MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Left) return;
            if (!_isReady || _gameBoard.SelectedCell.X == -1 || !_playing) return;
            var selectedCell = _gameBoard.Board[_gameBoard.SelectedCell.X][_gameBoard.SelectedCell.Y];
            if (_gameBoard.SelectedCell.X >= GameBoard.BoardSize)
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
            if (selectedCell.ShipOverMe)
                _enemy.HealthPoints--;
            if (_enemy.HealthPoints == 0) GameOver();
            EnemyDoStuff(e);
        }
        
        /// <summary>
        /// Enemy chooses a random cell on the board and click it if it was click-able.
        /// </summary>
        private void EnemyDoStuff(MouseEventArgs e)
        {
            if (!_playing) return;
            var r = new Random();
            REPEAT: // Choose another cell.
            var col = r.Next(0, GameBoard.BoardSize);
            var row = r.Next(0, GameBoard.BoardSize);
            var selectedCell = _gameBoard.Board[row + GameBoard.BoardSize][col];
            if (selectedCell.Destroyed)
                goto REPEAT;
            selectedCell.MouseClick(e);
            if (!selectedCell.ShipOverMe) return;
            _player.HealthPoints--;
            if (_player.HealthPoints == 0) GameOver();
        }
        
        public override void Draw(Graphics g)
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
            g.DrawString(@"Player Health: " + _player.HealthPoints + "\n\nEnemy Health: " + _enemy.HealthPoints, font,
                Brushes.GhostWhite, new PointF(0, 100));
            DrawGameOver(g);
        }
    }
}