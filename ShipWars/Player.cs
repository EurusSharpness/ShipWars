using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ShipWars
{
    public class Player
    {
        private const int NumberOfShips = 6; // 3x1, 4x1, 4x1, 5x1, 5x2, 6x1
        public BattleShip[] BattleShips;

        private const int Dt = 30;
        private static int _dy;
        private static int _dx;
        private readonly bool[,] _takenCells = new bool[GameBoard.BoardSize, GameBoard.BoardSize];
        private readonly bool _show;
        public int HealthPoints = 32;

        public Player(bool show)
        {
            _show = show;
            BattleShips = new BattleShip[NumberOfShips];
            _dy = ShipWarsForm.CanvasSize.Height / 2 - (GameBoard.BoardSize / 2) * Dt;
            _dx = 2 * ShipWarsForm.CanvasSize.Width / 3 - (GameBoard.BoardSize / 2) * Dt;
            InitShips();
        }


        private void InitShips()
        {
            BattleShips[0] = new BattleShip(1, 3, 30, new Point(30, 50), Color.LightGreen);
            BattleShips[1] = new BattleShip(1, 4, 30, new Point(90, 50), Color.DarkOliveGreen);
            BattleShips[2] = new BattleShip(1, 4, 30, new Point(150, 50), Color.Chartreuse);
            BattleShips[3] = new BattleShip(5, 1, 30, new Point(30, 200), Color.SpringGreen);
            BattleShips[4] = new BattleShip(5, 2, 30, new Point(30, 260), Color.Goldenrod);
            BattleShips[5] = new BattleShip(6, 1, 30, new Point(30, 340), Color.Chocolate);

            foreach (var ship in BattleShips)
                ship.MouseUp += b_MouseUp;
            if (_show)
                foreach (var ship in BattleShips)
                    ShipWarsForm.Collection.Add(ship);
        }

        private void b_MouseUp(object sender, MouseEventArgs e)
        {
            var b = sender as BattleShip;
            // remove the button from the Taken places.
            RemoveFomBoard(b);
            b?.ButtonClick();

            AddToBoard(b);
        }

        private void RemoveFomBoard(BattleShip b)
        {
            if (b.Column == -1) return;
            foreach (var p in b.IndexPoints)
                _takenCells[p.X, p.Y] = false;
            b.IndexPoints.Clear();
        }

        private bool CheckShip(BattleShip b)
        {
            var w = b.Width / Dt;
            var h = b.Height / Dt;
            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    if (!_takenCells[b.Column + j, b.Row + i]) continue;
                    b.Reset();
                    return false;
                }
            }

            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    _takenCells[b.Column + j, b.Row + i] = true;
                    b.IndexPoints.Add(new Point(b.Column + j, b.Row + i));
                }
            }

            b.Location = new Point(_dx + b.Column * Dt, _dy + b.Row * Dt);
            return true;
        }


        [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
        private void AddToBoard(BattleShip b)
        {
            // Check Bounds + Error value.
            // Error value = 1 square to each direction (Left, Top, Right, Bottom)
            const int errorValue = Dt;

            var deltaLeft = _dx - b.Location.X;
            var deltaTop = _dy - b.Location.Y;
            var deltaRight = (b.Location.X + b.Width) - (_dx + GameBoard.BoardSize * Dt);
            var deltaBottom = (b.Location.Y + b.Height) - (_dy + GameBoard.BoardSize * Dt);

            if (deltaTop > errorValue || deltaLeft > errorValue || deltaRight > errorValue ||
                deltaBottom > errorValue)
            {
                b.Reset();
                return;
            }

            // Check if the user moved the ship more than half a square.
            var moveRight = ((b.Location.X - _dx) / (double)Dt) - (b.Location.X - _dx) / Dt;
            if (moveRight >= 0.5)
                moveRight = 1;
            var moveDown = ((b.Location.Y - _dy) / (double)Dt) - (b.Location.Y - _dy) / Dt;
            if (moveDown >= 0.5)
                moveDown = 1;

            // if the ship is outside the bounds then don't move it back in bounds.
            if (deltaRight > 0)
                moveRight = 0;
            if (deltaBottom > 0)
                moveDown = 0;

            b.Row = (int)moveDown + (b.Location.Y - _dy) / Dt;
            b.Column = (int)moveRight + (b.Location.X - _dx) / Dt;

            CheckShip(b);
        }

        private bool AddShip(BattleShip b, int row, int col)
        {
            RemoveFomBoard(b);

            var r = b.Row;
            var c = b.Column;

            b.Row = row;
            b.Column = col;
            if (CheckShip(b))
                return true;

            (b.Row, b.Column) = (r, c);
            return false;
        }

        public void GenerateRandomFleet()
        {
            var rand = new Random();

            foreach (var ship in BattleShips)
            {
                RemoveFomBoard(ship);
                ship.Reset();
            }

            foreach (var ship in BattleShips)
            {
            REPEAT:
                // Do turn
                var flag = rand.Next(0, 2) == 0;
                if (flag)
                    (ship.Width, ship.Height) = (ship.Height, ship.Width);
                var col = rand.Next(0, GameBoard.BoardSize - ship.Width / Dt + 1);
                var row = rand.Next(0, GameBoard.BoardSize - ship.Height / Dt + 1);

                if (AddShip(ship, row, col)) continue;
                if (flag)
                    (ship.Width, ship.Height) = (ship.Height, ship.Width);

                goto REPEAT;
            }
        }

        public static void Draw(Graphics g)
        {
            for (var i = 0; i < GameBoard.BoardSize; i++)
                for (var j = 0; j < GameBoard.BoardSize; j++)
                    g.DrawRectangle(new Pen(Color.Black, 2), new Rectangle(new Point(
                        _dx + i * Dt, _dy + j * Dt), new Size(Dt, Dt)));
        }

        public bool IsReady()
        {
            return BattleShips.All(ship => ship.Column != -1);
        }

        public sealed class BattleShip : Button
        {
            public readonly List<Point> IndexPoints;
            public int Row, Column, Hitpoints;
            private readonly Point _originLocation;
            private Point _originCursor;
            private Point _originControl;
            private bool _btnDragging;
            private bool _doTurn;


            public BattleShip(int width, int height, int size, Point location, Color c)
            {
                Row = Column = -1;
                IndexPoints = new List<Point>();
                Hitpoints = width * height;
                Width = width * size;
                Height = height * size;
                _originLocation = location;
                FlatStyle = FlatStyle.Flat;
                Enabled = true;
                Visible = true;
                BackColor = c;
                ResizeRedraw = false;
                Location = location;
                DoubleBuffered = true;
                MouseDown += B_MouseDown;
                MouseMove += B_MouseMove;
            }

            public void ButtonClick()
            {
                _btnDragging = false;
                Capture = false;
                // Remove focus from controller
                if (Form.ActiveForm != null) Form.ActiveForm.ActiveControl = null;
                if (_doTurn)
                    (Height, Width) = (Width, Height);
            }

            private void B_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left) return;
                Capture = true;
                _originCursor = ShipWarsForm.MouseCords;
                _originControl = Location;
                _btnDragging = true;
                _doTurn = true;
            }

            private void B_MouseMove(object sender, MouseEventArgs e)
            {
                if (!_btnDragging || e.Button != MouseButtons.Left) return;
                var deltaX = _originCursor.X - ShipWarsForm.MouseCords.X;
                var deltaY = _originCursor.Y - ShipWarsForm.MouseCords.Y;
                if (Math.Abs(deltaX) > 2 || Math.Abs(deltaY) > 2)
                    _doTurn = false;
                Left = _originControl.X - (_originCursor.X - ShipWarsForm.MouseCords.X);
                Top = _originControl.Y - (_originCursor.Y - ShipWarsForm.MouseCords.Y);
            }

            public void Reset()
            {
                Row = Column = -1;
                Location = _originLocation;
                IndexPoints.Clear();
            }
        }
    }
}