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
        private Background bg;
        private float dt;
        private int angle = 45;
        private Bitmap _background;

        public Game()
        {
            bg = new Background();
            dt = ShipWarsForm._ClientSize.Width * 0.015f;
            _background = new Bitmap(ShipWarsForm._ClientSize.Width, ShipWarsForm._ClientSize.Height,
                PixelFormat.Format32bppArgb);
            CreateBoard();
        }

        private void CreateBoard()
        {
            _gameBoard = new Cell[BoardSize * 2][]; // Create 2 boards for each player, 14x14 each one.

            var dx = ShipWarsForm._ClientSize.Width * 0.5f;
            var dy = dt * (BoardSize + 2);

            // C^2 = A^2 + B^2 --> C^2 = (dt/2)^2 + (dt/2)^2 --> C = dt / Sqrt(2)
            var d = dt / (float) Math.Sqrt(2);

            // this point is the far top point after rotation
            var origin = new PointF(dx + dt / 2, dy - ((BoardSize - 1) - 0.5f) * dt - d);

            ////////////////UP///////////////////
            ////////////O///^//O////////////////
            /////////Left<--2d-->Right/////////
            ///////////O////v///O/////////////
            /////////////Down////////////////
            for (var k = 0; k < 2; k++)
            {
                dx += (BoardSize + 1) * dt * k;
                for (var i = 0; i < BoardSize; i++)
                {
                    _gameBoard[i + 14 * k] = new Cell[BoardSize];
                    for (var j = BoardSize - 1; j >= 0; j--)
                    {
                        var (x, y) = (dx + dt * i, dy - j * dt);

                        var up = new PointF(origin.X + d * (i - BoardSize + 1 + j + (BoardSize + 1) * k),
                            origin.Y + d * (i + (BoardSize - 1 - j) + (BoardSize + 1) * k));
                        var down = new PointF(up.X, up.Y + 2 * d);
                        var right = new PointF(up.X - d, up.Y + d);
                        var left = new PointF(up.X + d, up.Y + d);


                        _gameBoard[i + BoardSize * k][j] =
                            new Cell(new RectangleF(x, y, dt, dt), new[] {left, up, right, down});
                    }
                }
            }

            RotateRectangle(_gameBoard);
        }

        public void Draw(Graphics g)
        {
            bg.Draw(g);
            DrawGameMatrix(g);
        }

        private void DrawGameMatrix(Graphics g)
        {
            g.DrawImage(_background, 0, 0);

            // TODO: Make every cell aware of mouse movement
            /*if (t.Marked)
                g.FillRectangle(Brushes.Red, t.Rect.X, t.Rect.Y, t.Rect.Width,
                    t.Rect.Height);
            else if (t.MouseOver())
                g.FillRectangle(Brushes.Gold, t.Rect.X, t.Rect.Y, t.Rect.Width,
                    t.Rect.Height);*/
        }

        private void RotateRectangle(IReadOnlyList<Cell[]> r)
        {
            using var g = Graphics.FromImage(_background);
            var m = new Matrix();
            m.RotateAt(angle,
                new PointF(r[0][BoardSize - 1].Rect.Left + (r[BoardSize - 1][BoardSize - 1].Rect.Width / 2),
                    r[0][BoardSize - 1].Rect.Top + (r[BoardSize][BoardSize - 1].Rect.Height / 2)));
            g.Transform = m;
            foreach (var rect in r)
                foreach (var t in rect)
                    g.DrawRectangle(new Pen(Brushes.Black, 2.4f), t.Rect.X, t.Rect.Y,
                        t.Rect.Width, t.Rect.Height);

            g.ResetTransform();
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
                if (_x < ShipWarsForm._ClientSize.Width) _x += 1.5f;
                else _x = 0;

                g.DrawImage(background, _x - ShipWarsForm._ClientSize.Width, 0);

                var ship = Properties.Resources.TestShip;
                g.DrawImage(ship,
                    new RectangleF(500, 500, 13 * 5, 13 * 5),
                    new RectangleF(0, 0, ship.Width, ship.Height),
                    GraphicsUnit.Pixel
                );
                // RotatePicture(g, ship, new PointF(500,500));
            }

            private void RotatePicture(Graphics g, Image im, PointF point)
            {
                var m = new Matrix();
                m.RotateAt(45, new PointF(im.Width / 2f + point.X, im.Height / 2f + point.Y));
                g.Transform = m;
                g.ResetTransform();
            }
        }
    }
}