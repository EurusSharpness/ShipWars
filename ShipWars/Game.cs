using System.Drawing;
using System.Management.Instrumentation;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace ShipWars
{
    public class Game
    {
        private readonly GameBackground _background;
        private readonly GameBoard _gameBoard;
        private Player _player, _enemy;
        private bool _isReady;
        private string _notReadyMsg = @"You must put all your ships on the grid to start";
        private int _alpha = 255;
        private bool _notReadyFlag;

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
                    _notReadyFlag = true;
                }
                else
                {
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
                foreach (var index in ship.IndexPoints)
                    _gameBoard.Board[index.Y][index.X].Color = new SolidBrush(ship.BackColor);
                ship.Visible = false;
                ship.Enabled = false;
                ship.Dispose();
            }

            foreach (var ship in _player.BattleShips)
            {
                foreach (var index in ship.IndexPoints)
                    _gameBoard.Board[GameBoard.BoardSize + index.Y][index.X].Color = new SolidBrush(ship.BackColor);
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
            if (_isReady)
                _gameBoard.MouseUp(e);
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (_isReady)
                _gameBoard.MouseDown(e);
        }

        public void Draw(Graphics g)
        {
            _background.Draw(g);


            if (_isReady)
                _gameBoard.Draw(g);
            else
                _player.Draw(g);

            if (!_notReadyFlag) return;
            g.DrawString(
                _notReadyMsg, new Font("", 28), new SolidBrush(Color.FromArgb(_alpha, 255, 0, 0)),
                new PointF(
                    (ShipWarsForm._ClientSize.Width - g.MeasureString(_notReadyMsg, new Font("", 28)).Width) / 2, 50));
            _alpha--;
            if (_alpha == 0)
                _notReadyFlag = false;
        }
    }
}