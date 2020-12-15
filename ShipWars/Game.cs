using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace ShipWars
{
    public class Game
    {
        private const int BoardSize = 14;
        private Cell[][] _gameBoard;
        private readonly Background _bg;
        private readonly float _dt;
        private const int Angle = 45;
        private readonly Bitmap _background;
        private Point _selectedCell;

        public Game()
        {
            _bg = new Background();
            _dt = ShipWarsForm._ClientSize.Width * 0.015f;
            _background = new Bitmap(ShipWarsForm._ClientSize.Width, ShipWarsForm._ClientSize.Height,
                PixelFormat.Format32bppArgb);
            _selectedCell = new Point(-1, -1);

            CreateBoard();
        }

        public void MouseMove(MouseEventArgs e)
        {
            for (var i = 0; i < _gameBoard.Length; i++)
            {
                for (var j = 0; j < _gameBoard[i].Length; j++)
                {
                    if (!_gameBoard[i][j].MouseOver()) continue;
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
            if(_selectedCell.X == -1) return;
            _gameBoard[_selectedCell.X][_selectedCell.Y].MouseClick(e);
            Console.WriteLine(_selectedCell);
        }

        public void Draw(Graphics g)
        {
            _bg.Draw(g);
            DrawGameMatrix(g);
        }

        private void DrawGameMatrix(Graphics g)
        {
            RotateRectangle(_gameBoard, g);

            if (_selectedCell.X == -1) return;
            var s = _gameBoard[_selectedCell.X][_selectedCell.Y].Rect;
            RotateRectangle(s, g);

        }

        private void RotateRectangle(IReadOnlyList<Cell[]> r, Graphics g)
        {
            var m = new Matrix();
            m.RotateAt(Angle,
                new PointF(r[0][BoardSize - 1].Rect.Left + (r[BoardSize - 1][BoardSize - 1].Rect.Width / 2),
                    r[0][BoardSize - 1].Rect.Top + (r[BoardSize][BoardSize - 1].Rect.Height / 2)));
            g.Transform = m;
            foreach (var rect in r)
            {
                foreach (var t in rect)
                {
                    if (t.Marked)
                        g.FillRectangle(Brushes.Red, t.Rect.X, t.Rect.Y,
                            t.Rect.Width, t.Rect.Height);
                    g.DrawRectangle(new Pen(Brushes.Black, 2.4f), t.Rect.X, t.Rect.Y,
                        t.Rect.Width, t.Rect.Height);
                }
            }
            g.ResetTransform();
        }

        private void RotateRectangle(RectangleF r, Graphics g)
        {
            var m = new Matrix();
            m.RotateAt(Angle,
                new PointF(
                    _gameBoard[0][BoardSize - 1].Rect.Left + (_gameBoard[BoardSize - 1][BoardSize - 1].Rect.Width / 2),
                    _gameBoard[0][BoardSize - 1].Rect.Top + (_gameBoard[BoardSize][BoardSize - 1].Rect.Height / 2)));

            g.Transform = m;
            g.FillRectangle(Brushes.Gold, r);
            g.ResetTransform();
        }

        private void CreateBoard()
        {
            _gameBoard = new Cell[BoardSize * 2][]; // Create 2 boards for each player, 14x14 each one.

            var dx = ShipWarsForm._ClientSize.Width * 0.5f;
            var dy = _dt * (BoardSize + 2);

            // C^2 = A^2 + B^2 --> C^2 = (dt/2)^2 + (dt/2)^2 --> C = dt / Sqrt(2)
            var d = _dt / (float) Math.Sqrt(2);

            // this point is the far top point after rotation
            var origin = new PointF(dx + _dt / 2, dy - ((BoardSize - 1) - 0.5f) * _dt - d);

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
                    _gameBoard[i + 14 * k] = new Cell[BoardSize];
                    for (var j = BoardSize - 1; j >= 0; j--)
                    {
                        var (x, y) = (dx + _dt * i, dy - j * _dt);

                        var up = new PointF(origin.X + d * (i - BoardSize + 1 + j + (BoardSize + 1) * k),
                            origin.Y + d * (i + (BoardSize - 1 - j) + (BoardSize + 1) * k));
                        var down = new PointF(up.X, up.Y + 2 * d);
                        var right = new PointF(up.X - d, up.Y + d);
                        var left = new PointF(up.X + d, up.Y + d);


                        _gameBoard[i + BoardSize * k][j] =
                            new Cell(new RectangleF(x, y, _dt, _dt), new[] {left, up, right, down});
                    }
                }
            }
        }

        private class Cell
        {
            public readonly RectangleF Rect;

            // If the cell is destroyed
            public bool Marked;

            // Left, Up, Right, Down.
            public readonly PointF[] Cords;

            private readonly GraphicsPath _path = new GraphicsPath();

            public Cell(RectangleF rectangleF, PointF[] cords)
            {
                (Rect, Cords, Marked) = (rectangleF, cords, false);
                _path.AddPolygon(cords);
            }

            public bool MouseOver()
            {
                return _path.IsVisible(ShipWarsForm._MouseCords);
            }

            public void MouseClick(MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                    Marked = true;
            }
        }

        private class Background
        {
            private float _x;
            private Bitmap background;

            public Background()
            {
                CreateBackground();
            }

            private void CreateBackground()
            {
                background = new Bitmap(ShipWarsForm._ClientSize.Width * 2, ShipWarsForm._ClientSize.Height,
                    PixelFormat.Format32bppArgb);
                using var g = Graphics.FromImage(background);
                var image = Properties.Resources.WaterBackground;
                int x = 0, y = 0;
                while (y <= ShipWarsForm._ClientSize.Height)
                {
                    while (x <= ShipWarsForm._ClientSize.Width)
                    {
                        g.DrawImage(image, new RectangleF(x, y, image.Width + 0.5f, image.Height),
                            new RectangleF(0, 0, image.Width, image.Height),
                            GraphicsUnit.Pixel);
                        g.DrawImage(image,
                            new RectangleF(ShipWarsForm._ClientSize.Width + x, y, image.Width + 0.5f,
                                image.Height),
                            new RectangleF(0, 0, image.Width, image.Height),
                            GraphicsUnit.Pixel);
                        x += image.Width;
                    }

                    x = 0;
                    y += image.Height;
                }
            }

            public void Draw(Graphics g)
            {
                _x = (_x < ShipWarsForm._ClientSize.Width) ? _x + 1.5f : 0;
                g.DrawImage(background, _x - ShipWarsForm._ClientSize.Width, 0);
            }
        }
    }
}