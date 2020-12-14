using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace ShipWars
{
    public class Game
    {
        private const int BOARD_SIZE = 14;
        private Cell[][] vv;
        private Background bg;
        private float dt;
        private int angle = 45;

        public Game()
        {
            bg = new Background();
            dt = ShipWarsForm._ClientSize.Width * 0.015f;
            CreateBoard();
        }

        private void CreateBoard()
        {
            vv = new Cell[BOARD_SIZE * 2][]; // Create 2 boards for each player, 14x14 each one.

            var dx = ShipWarsForm._ClientSize.Width * 0.5f;
            var dy = dt * (BOARD_SIZE + 2);

            // C^2 = A^2 + B^2 --> C^2 = (dt/2)^2 + (dt/2)^2 --> C = dt / Sqrt(2)
            var d = dt / (float) Math.Sqrt(2);

            // this point is the far top point after rotation
            var origin = new PointF(dx + dt / 2,  dy - ((BOARD_SIZE - 1) - 0.5f) * dt - d);

            ////////////////UP///////////////////
            ////////////O///^//O////////////////
            /////////Left<--2d-->Right/////////
            ///////////O////v///O/////////////
            /////////////Down////////////////
            for (var k = 0; k < 2; k++)
            {
                dx += (BOARD_SIZE + 1) * dt * k;
                for (var i = 0; i < BOARD_SIZE; i++)
                {
                    vv[i + 14 * k] = new Cell[BOARD_SIZE];
                    for (var j = BOARD_SIZE - 1; j >= 0; j--)
                    {
                        var (x, y) = (dx + dt * i, dy - j * dt);

                        var up = new PointF(origin.X + d * (i - BOARD_SIZE + 1 + j + (BOARD_SIZE + 1) * k),
                            origin.Y + d * (i + (BOARD_SIZE - 1 - j) + (BOARD_SIZE + 1) * k));
                        var down = new PointF(up.X, up.Y + 2 * d);
                        var right = new PointF(up.X - d, up.Y + d);
                        var left = new PointF(up.X + d, up.Y + d);


                        vv[i + BOARD_SIZE * k][j] =
                            new Cell(new RectangleF(x, y, dt, dt), new[] {left, up, right, down});
                    }
                }
            }
        }

        public void Draw(Graphics g)
        {
            bg.MoveBackground();
            bg.Draw(g);
            DrawGameMatrix(g);
        }

        private void DrawGameMatrix(Graphics g) => RotateRectangle(g, vv);

        private void RotateRectangle(Graphics g, IReadOnlyList<Cell[]> r)
        {
            var m = new Matrix();
            m.RotateAt(angle,
                new PointF(r[0][BOARD_SIZE - 1].Rect.Left + (r[BOARD_SIZE - 1][BOARD_SIZE - 1].Rect.Width / 2),
                    r[0][BOARD_SIZE - 1].Rect.Top + (r[BOARD_SIZE][BOARD_SIZE - 1].Rect.Height / 2)));
            g.Transform = m;
            foreach (var rect in r)
            {
                foreach (var t in rect)
                {
                    if (t.Marked)
                        g.FillRectangle(Brushes.Red, t.Rect.X, t.Rect.Y, t.Rect.Width,
                            t.Rect.Height);
                    if(t.MouseOver())
                        g.FillRectangle(Brushes.Gold, t.Rect.X, t.Rect.Y, t.Rect.Width,
                            t.Rect.Height);
                    g.DrawRectangle(new Pen(Brushes.Black, 2.4f), t.Rect.X, t.Rect.Y,
                        t.Rect.Width, t.Rect.Height);
                }
            }

            g.ResetTransform();
        }

        private class Cell
        {
            public RectangleF Rect;
            public bool Marked;
            public PointF[] Cords;
            private const int Up = 0, Down = 1, Left = 2, Right = 3;

            public Cell(RectangleF rectangleF, PointF[] cords) =>
                (Rect, Cords, Marked) = (rectangleF, cords, false);

            public bool MouseOver()
            {
                return IsInPolygon(Cords, ShipWarsForm._MouseCords);
            }

            private bool IsInPolygon(PointF[] polygon, Point testPoint)
            {
                // Make a GraphicsPath containing the polygon.
                var path = new GraphicsPath();
                path.AddPolygon(polygon);
                // See if the point is inside the path.
                return path.IsVisible(testPoint);
            }
        }

        private class Background
        {
            private float _x;

            public Background()
            {
            }

            public void MoveBackground()
            {
                if (_x < ShipWarsForm._ClientSize.Width) _x += 1.5f;
                else _x = 0;
            }

            public void Draw(Graphics g)
            {
                var image = Properties.Resources.WaterBackground;
                int x = 0, y = 0;
                while (y <= ShipWarsForm._ClientSize.Height)
                {
                    while (x <= ShipWarsForm._ClientSize.Width)
                    {
                        g.DrawImage(image, new RectangleF(x + _x, y, image.Width + 0.5f, image.Height),
                            new RectangleF(0, 0, image.Width, image.Height),
                            GraphicsUnit.Pixel);
                        g.DrawImage(image,
                            new RectangleF(-ShipWarsForm._ClientSize.Width + _x + x, y, image.Width + 0.5f,
                                image.Height),
                            new RectangleF(0, 0, image.Width, image.Height),
                            GraphicsUnit.Pixel);
                        x += image.Width;
                    }

                    x = 0;
                    y += image.Height;
                }
            }
        }

        private float DegreeToRadian(float angle)
        {
            return (float) (Math.PI * angle / 180.0);
        }
    }
}