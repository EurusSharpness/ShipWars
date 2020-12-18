using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ShipWars
{
    public class AssembleFleet
    {
        private const int NumberOfShips = 6; // 3x1, 4x1, 4x1, 5x1, 5x2, 6x1
        public BattleShip[] BattleShips;

        public AssembleFleet()
        {
            BattleShips = new BattleShip[NumberOfShips];
            InitShips();
        }

        public BattleShip[] GetFleet()
        {
            return BattleShips;
        }

        private void InitShips()
        {
            BattleShips[0] = new BattleShip(1, 3, 30, new Point(30, 50), Color.LightGreen);
            BattleShips[1] = new BattleShip(1, 4, 30, new Point(90, 50), Color.BlueViolet);
            BattleShips[2] = new BattleShip(1, 4, 30, new Point(150, 50), Color.Goldenrod);
            BattleShips[3] = new BattleShip(5, 1, 30, new Point(30, 200), Color.Brown);
            BattleShips[4] = new BattleShip(5, 2, 30, new Point(30, 260), Color.DarkCyan);
            BattleShips[5] = new BattleShip(6, 1, 30, new Point(30, 340), Color.Tomato);

            ShipWarsForm._Collection.AddRange(BattleShips);
        }


        public void Draw(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)),
                new Rectangle(0, 0, ShipWarsForm._ClientSize.Width, ShipWarsForm._ClientSize.Height));

            var dt = 30;
            var dy = ShipWarsForm._ClientSize.Height / 2 - (GameBoard.BoardSize / 2) * dt;
            var dx = 2 * ShipWarsForm._ClientSize.Width / 3 - (GameBoard.BoardSize / 2) * dt;
            for (var i = 0; i < GameBoard.BoardSize; i++)
            for (var j = 0; j < GameBoard.BoardSize; j++)
                g.DrawRectangle(new Pen(Color.Black, 2), new Rectangle(new Point(
                    dx + i * dt, dy + j * dt), new Size(dt, dt)));
        }

        public bool IsReady()
        {
            return BattleShips.All(ship => ship.Colum != -1);
        }

        public void MouseMove(MouseEventArgs e)
        {
        }

        public void MouseUp(MouseEventArgs e)
        {
        }

        public void MouseDown(MouseEventArgs e)
        {
        }

        public sealed class BattleShip : Button
        {
            public List<Point> IndexPoints;
            public int Row, Colum, Hitpoints;
            private static bool[,] taken = new bool[GameBoard.BoardSize, GameBoard.BoardSize];
            private readonly Point _originLocation;
            private Point _originCursor;
            private Point _originControl;
            private bool _btnDragging;
            private bool _doTurn;
            private int _dt;

            public BattleShip(int width, int height, int size, Point location, Color c)
            {
                Row = Colum = -1;
                IndexPoints = new List<Point>();
                Hitpoints = width * height;
                _dt = size;
                Width = width * size;
                Height = height * size;
                _originLocation = location;
                FlatStyle = FlatStyle.Flat;
                Enabled = true;
                Visible = true;
                BackColor = c;
                Location = location;
                MouseDown += b_MouseDown;
                MouseMove += b_MouseMove;
                MouseUp += b_MouseUp;
            }

            private void b_MouseUp(object sender, MouseEventArgs e)
            {
                // remove the button from the Taken places.
                RemoveFomBoard();
                _btnDragging = false;
                Capture = false;
                // Remove focus from controller
                if (Form.ActiveForm != null) Form.ActiveForm.ActiveControl = null;

                if (_doTurn)
                    (Height, Width) = (Width, Height);

                // Board starting points.
                var dy = ShipWarsForm._ClientSize.Height / 2 - GameBoard.BoardSize / 2 * _dt;
                var dx = 2 * ShipWarsForm._ClientSize.Width / 3 - GameBoard.BoardSize / 2 * _dt;



                // Check Bounds + Error value.
                // Error value = 1 square to each direction (Left, Top, Right, Bottom)
                var errorValue = _dt;

                var deltaLeft = dx - Location.X;
                var deltaTop = dy - Location.Y;
                var deltaRight = (Location.X + Width) - (dx + GameBoard.BoardSize * _dt);
                var deltaBottom = (Location.Y + Height) - (dy + GameBoard.BoardSize * _dt);

                if (deltaTop > errorValue || deltaLeft > errorValue || deltaRight > errorValue ||
                    deltaBottom > errorValue)
                {
                    Location = _originLocation;
                    Row = Colum = -1;
                    return;
                }

                // Check if the user moved the ship more than half a square.
                var moveRight = ((Location.X - dx) / (double) _dt) - (Location.X - dx) / _dt;
                if (moveRight >= 0.5)
                    moveRight = 1;
                var moveDown = ((Location.Y - dy) / (double) _dt) - (Location.Y - dy) / _dt;
                if (moveDown >= 0.5)
                    moveDown = 1;

                // if the ship is outside the bounds then don't move it back in bounds.
                if (deltaRight > 0)
                    moveRight = 0;
                if (deltaBottom > 0)
                    moveDown = 0;

                Row = (int) moveDown + (Location.Y - dy) / _dt;
                Colum = (int) moveRight + (Location.X - dx) / _dt;
                Location = new Point(dx + Colum * _dt, dy + Row * _dt);

                AddToBoard(Row, Colum);
            }

            private void b_MouseDown(object sender, MouseEventArgs e)
            {
                Capture = true;
                _originCursor = Cursor.Position;
                _originControl = Location;
                _btnDragging = true;
                _doTurn = true;
            }

            private void b_MouseMove(object sender, MouseEventArgs e)
            {
                if (!_btnDragging) return;
                var deltaX = _originCursor.X - Cursor.Position.X;
                var deltaY = _originCursor.Y - Cursor.Position.Y;
                if (Math.Abs(deltaX) > 2 || Math.Abs(deltaY) > 2)
                    _doTurn = false;
                Left = _originControl.X - (_originCursor.X - Cursor.Position.X);
                Top = _originControl.Y - (_originCursor.Y - Cursor.Position.Y);
            }

            private void RemoveFomBoard()
            {
                if (Colum == -1) return;
                int w = Width / 30;
                int h = Height / 30;
                for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                    taken[Colum + j, Row + i] = false;
                IndexPoints.Clear();
            }

            private void AddToBoard(int row, int col)
            {
                int w = Width / _dt;
                int h = Height / _dt;
                for (int i = 0; i < h; i++)
                {
                    for (int j = 0; j < w; j++)
                    {
                        if (!taken[col + j, row + i]) continue;
                        Location = _originLocation;
                        return;
                    }
                }

                for (int i = 0; i < h; i++)
                {
                    for (int j = 0; j < w; j++)
                    {
                        taken[Colum + j, Row + i] = true;
                        IndexPoints.Add(new Point(Colum + j, Row + i));
                    }
                }
            }
        }
    }
}