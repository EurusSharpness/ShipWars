using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ShipWars
{
    public class GameBoard
    {
        private readonly Matrix _matrix;

        
        public readonly float _cellSize;
        public const int BoardSize = 14;
        private const int Angle = 45;
        private readonly float _dx;
        private readonly float _dy;
        public Cell[][] Board;
        public Point SelectedCell;
        public PlayerShips[] playerShips;
        public GameBoard()
        {
            _matrix = new Matrix();
            _cellSize = Math.Min(ShipWarsForm.CanvasSize.Width, ShipWarsForm.CanvasSize.Height) * 0.029f;
            //_cellSize = 20;
            SelectedCell = new Point(-1, -1);
            _dy = _cellSize * (BoardSize + 2);
            _dx = ShipWarsForm.CanvasSize.Width * 0.5f;
            playerShips = new PlayerShips[Player.NumberOfShips];
            CreateBoard();
        }

        public void MouseMove(MouseEventArgs e)
        {
        }

        public void MouseUp(MouseEventArgs e)
        {
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (SelectedCell.X == -1) return;
            Board[SelectedCell.X][SelectedCell.Y].MouseClick(e);
        }

        public void Draw(Graphics g)
        {
            DrawGameBoard(g);
        }

        private void DrawGameBoard(Graphics g) => RotateRectangles(g);

        /// <summary>
        /// Rotate the board by 45°.
        /// The rotation is from the Top-Left square and tilt it 45° Clock-wise.
        /// </summary>
        private void RotateRectangles(Graphics g)
        {
            g.Transform = _matrix;
            var i = 0;
            var flag = false;
            var p = new Pen(Brushes.Black, 2.5f);
            
            foreach (var rect in Board)
            {
                var j = 0;
                foreach (var t in rect)
                {
                    if (t.MouseOver())
                    {
                        SelectedCell.X = i;
                        SelectedCell.Y = j;
                        g.FillRectangle(Brushes.Gold, t.Rect);
                        flag = true;
                    }
                    g.FillRectangle(t.Color, t.Rect);
                    g.DrawRectangle(p, t.Rect.X, t.Rect.Y,
                        t.Rect.Width, t.Rect.Height);
                    j++;
                }
                i++;
            }

            if (!flag)
                (SelectedCell.X, SelectedCell.Y) = (-1, -1); // Cell not selected

            // <---- Draw Ships ----->
            
            foreach(var ship in playerShips)
            {
                if (ship == null) continue;
                g.DrawImage(ship.Image, ship.rectangle);
            }

            // return drawings to normal.
            g.ResetTransform();
        }

        private void CreateBoard()
        {
            Board = new Cell[BoardSize * 2][]; // Create 2 boards for each player, 14x14 each one.

            var tempDx = _dx;
            var tempDy = _dy;
            // C^2 = A^2 + B^2 --> C^2 = (dt/2)^2 + (dt/2)^2 --> C = dt / Sqrt(2)
            // Half the radius of the circle around the square that is generated from rotation.
            var r = _cellSize / (float)Math.Sqrt(2);

            // this point is the Top point of the top left square after rotation
            // X = Center horizontally, Y = Center vertically - r ==> the top point of the square.
            var origin = new PointF(_dx + _cellSize / 2, _dy - ((BoardSize - 1) - 0.5f) * _cellSize - r);

            ////////////////UP///////////////////
            ////////////O///^//O////////////////
            /////////Left<--2d-->Right/////////
            ///////////O////v///O/////////////
            /////////////Down////////////////
            for (var k = 0; k < 2; k++)
            {
                tempDx += (BoardSize + 1) * _cellSize * k;
                for (var i = 0; i < BoardSize; i++)
                {
                    Board[i + 14 * k] = new Cell[BoardSize];
                    for (var j = BoardSize - 1; j >= 0; j--)
                    {
                        var (x, y) = (tempDx + _cellSize * i, _dy - j * _cellSize);

                        // Enough to calculate 1 point of the 4, the rest we just add/subtract 'r'. 
                        var up = new PointF(origin.X + r * (i - BoardSize + 1 + j + (BoardSize + 1) * k),
                            origin.Y + r * (i + (BoardSize - 1 - j) + (BoardSize + 1) * k));
                        var down = new PointF(up.X, up.Y + 2 * r);
                        var right = new PointF(up.X - r, up.Y + r);
                        var left = new PointF(up.X + r, up.Y + r);

                        Board[i + BoardSize * k][j] =
                            new Cell(new RectangleF(x, y, _cellSize, _cellSize), new[] { left, up, right, down });
                    }
                }
            }

            // Set the matrix rotation° and starting X,Y.
            _matrix.RotateAt(Angle,
                new PointF(
                    Board[0][BoardSize - 1].Rect.Left + (Board[BoardSize - 1][BoardSize - 1].Rect.Width / 2),
                    Board[0][BoardSize - 1].Rect.Top + (Board[BoardSize][BoardSize - 1].Rect.Height / 2)));
        }

        public class Cell
        {
            /// <summary>Rectangle that represents the cell.</summary>
            public readonly RectangleF Rect;
            
            /// <summary>If there is a ship on the cell.</summary>
            public bool ShipOverMe;

            /// <summary>If the cell is destroyed.</summary>
            public bool Destroyed;

            /// <summary>The path between the 4 corners of the cell.</summary>
            public readonly GraphicsPath Path;
            /// <summary>Cell back color.</summary>
            public Brush Color = Brushes.Transparent;
            public Cell(RectangleF rectangleF, PointF[] cords)
            {
                (Rect, Destroyed) = (rectangleF, false);
                Path = new GraphicsPath();
                Path.AddPolygon(cords);
            }

            /// <returns>True if the mouse is over the cell, false otherwise</returns>
            public bool MouseOver()
            {
                return Path.IsVisible(ShipWarsForm.MouseCords);
            }
            
            /// <summary>Destroy the cell and check if it had a ship over it</summary>
            public void MouseClick(MouseEventArgs e = null)
            {
                Destroyed = true;
                Color = (ShipOverMe) ? Brushes.DarkRed : Brushes.DeepSkyBlue;
            }
        }

        public class PlayerShips
        {
            public Point Cords { get; set; }
            public Size size { get; set; }
            public Image Image { get; set; }

            public RectangleF rectangle { get; set; }

        }
    }
}