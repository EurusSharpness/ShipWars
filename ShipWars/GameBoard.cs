using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ShipWars
{
    public class GameBoard
    {
        private readonly Matrix _matrix;

        private readonly float _dt;
        public const int BoardSize = 14;
        private const int Angle = 45;

        public Cell[][] Board;
        public Point _selectedCell;

        public bool IsReady;


        public GameBoard()
        {
            _matrix = new Matrix();
            _dt = Math.Min(ShipWarsForm._ClientSize.Width, ShipWarsForm._ClientSize.Height) * 0.028f;
            _selectedCell = new Point(-1, -1);
            CreateBoard();
        }

        public void MouseMove(MouseEventArgs e)
        {
            for (var i = 0; i < Board.Length; i++)
            {
                for (var j = 0; j < Board[i].Length; j++)
                {
                    if (!Board[i][j].MouseOver()) continue;
                    (_selectedCell.X, _selectedCell.Y) = (i, j);
                    return;
                }
            }

            (_selectedCell.X, _selectedCell.Y) = (-1, -1);
        }

        public void MouseUp(MouseEventArgs e)
        {
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (_selectedCell.X == -1) return;
            Board[_selectedCell.X][_selectedCell.Y].MouseClick(e);
        }

        public void Draw(Graphics g)
        {
            DrawGameBoard(g);
        }

        private void DrawGameBoard(Graphics g) => RotateRectangles(g);


        private void RotateRectangles(Graphics g)
        {
            g.Transform = _matrix;

            if (_selectedCell.X != -1)
                g.FillRectangle(Brushes.Gold, Board[_selectedCell.X][_selectedCell.Y].Rect);

            foreach (var rect in Board)
            {
                foreach (var t in rect)
                {
                    g.FillRectangle(t.Color, t.Rect.X, t.Rect.Y,
                            t.Rect.Width, t.Rect.Height);
                    g.DrawRectangle(new Pen(Brushes.Black, 2.5f), t.Rect.X, t.Rect.Y,
                        t.Rect.Width, t.Rect.Height);
                }
            }

            g.ResetTransform();
        }

        private void CreateBoard()
        {
            Board = new Cell[BoardSize * 2][]; // Create 2 boards for each player, 14x14 each one.

            var dx = ShipWarsForm._ClientSize.Width * 0.5f;
            var dy = _dt * (BoardSize + 2);

            // C^2 = A^2 + B^2 --> C^2 = (dt/2)^2 + (dt/2)^2 --> C = dt / Sqrt(2)
            // Half the radius of the circle around the square that is generated from rotation.
            var r = _dt / (float) Math.Sqrt(2);

            // this point is the Top point of the top left square after rotation
            // X = Center horizontally, Y = Center vertically - r ==> the top point of the square.
            var origin = new PointF(dx + _dt / 2, dy - ((BoardSize - 1) - 0.5f) * _dt - r);

            ////////////////UP///////////////////
            ////////////O///^//O////////////////
            /////////Left<--2d-->Right/////////
            ///////////O////v///O/////////////
            /////////////Down////////////////
            for (var k = 0; k < 2; k++)
            {
                dx += (BoardSize + 1) * _dt * k;
                for (var i = 0; i < BoardSize; i++)
                {
                    Board[i + 14 * k] = new Cell[BoardSize];
                    for (var j = BoardSize - 1; j >= 0; j--)
                    {
                        var (x, y) = (dx + _dt * i, dy - j * _dt);

                        // Enough to calculate 1 point of the 4, the rest we just add/subtract 'r'. 
                        var up = new PointF(origin.X + r * (i - BoardSize + 1 + j + (BoardSize + 1) * k),
                            origin.Y + r * (i + (BoardSize - 1 - j) + (BoardSize + 1) * k));
                        var down = new PointF(up.X, up.Y + 2 * r);
                        var right = new PointF(up.X - r, up.Y + r);
                        var left = new PointF(up.X + r, up.Y + r);

                        Board[i + BoardSize * k][j] =
                            new Cell(new RectangleF(x, y, _dt, _dt), new[] {left, up, right, down});
                    }
                }
            }

            _matrix.RotateAt(Angle,
                new PointF(
                    Board[0][BoardSize - 1].Rect.Left + (Board[BoardSize - 1][BoardSize - 1].Rect.Width / 2),
                    Board[0][BoardSize - 1].Rect.Top + (Board[BoardSize][BoardSize - 1].Rect.Height / 2)));
        }

        public class Cell
        {
            public readonly RectangleF Rect;

            // If the cell is destroyed
            public bool Destroyed;

            // Left, Up, Right, Down.

            private readonly GraphicsPath _path;
            public Brush Color = Brushes.Transparent;
            public Cell(RectangleF rectangleF, PointF[] cords)
            {
                (Rect, Destroyed) = (rectangleF, false);
                _path = new GraphicsPath();
                _path.AddPolygon(cords);
            }

            public bool MouseOver()
            {
                return _path.IsVisible(ShipWarsForm._MouseCords);
            }

            public void MouseClick(MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left) return;
                Destroyed = true;
                Color = Brushes.Red;

            }
        }
    }
}