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
        private readonly AssembleFleet _fleet;
        private bool _isReady;
        private string __notReadyMsg = @"You must put all your ships on the grid to start";
        private int alpha = 255;
        private bool _notReadyFlag;

        public Game()
        {
            _background = new GameBackground();
            _gameBoard = new GameBoard();
            _fleet = new AssembleFleet();
            IsReady();
        }

        private void IsReady()
        {
            Button b = new Button()
            {
                Text = @"S T A R T",
                Font = new Font("ALGERIAN", ShipWarsForm._ClientSize.Height / 20f, FontStyle.Italic | FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = Color.Blue,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Enabled = true,
                Visible = true,
                Size = TextRenderer.MeasureText(@"S T A R T",
                    new Font("ALGERIAN", 24, FontStyle.Italic | FontStyle.Bold))
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.Transparent;
            b.FlatAppearance.MouseDownBackColor = Color.Transparent;
            b.MouseEnter += (s, e) => { b.ForeColor = Color.Red; };
            b.MouseLeave += (s, e) => { b.ForeColor = Color.Blue; };
            b.MouseClick += (s, e) =>
            {
                _isReady = _fleet.IsReady();
                if (!_isReady)
                {
                    alpha = 255;
                    _notReadyFlag = true;
                }
                else
                {
                    b.Enabled = false;
                    b.Visible = false;

                    ShipsToBoard();
                    b.Dispose();
                }
            };
            b.Location = new Point(
                (ShipWarsForm._ClientSize.Width - b.Width) / 2,
                (int) (ShipWarsForm._ClientSize.Height * 0.83f)
            );
            ShipWarsForm._Collection.Add(b);
        }

        private void ShipsToBoard()
        {
            foreach (var ship in _fleet.BattleShips)
            {
                foreach (var index in ship.IndexPoints)
                    _gameBoard.Board[GameBoard.BoardSize + index.Y][index.X].Color = new SolidBrush(ship.BackColor);
                ship.Visible = false;
                ship.Enabled = false;
                ship.Dispose();
            }

            _fleet.BattleShips = null;
        }

        public void MouseMove(MouseEventArgs e)
        {
            if (_isReady)
                _gameBoard.MouseMove(e);
            else
                _fleet.MouseMove(e);
        }

        public void MouseUp(MouseEventArgs e)
        {
            if (_isReady)
                _gameBoard.MouseUp(e);
            else
                _fleet.MouseUp(e);
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (_isReady)
                _gameBoard.MouseDown(e);
            else
                _fleet.MouseDown(e);
        }

        public void Draw(Graphics g)
        {
            _background.Draw(g);


            if (_isReady)
                _gameBoard.Draw(g);
            else
                _fleet.Draw(g);

            if (!_notReadyFlag) return;
            g.DrawString(
                __notReadyMsg, new Font("", 28), new SolidBrush(Color.FromArgb(alpha, 255, 0, 0)),
                new PointF(
                    (ShipWarsForm._ClientSize.Width - g.MeasureString(__notReadyMsg, new Font("", 28)).Width) / 2, 50));
            alpha--;
            if (alpha == 0)
                _notReadyFlag = false;
        }
    }
}