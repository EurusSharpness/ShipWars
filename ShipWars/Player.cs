using System;
using System.Collections.Generic;
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
        private bool[,] takenCells = new bool[GameBoard.BoardSize, GameBoard.BoardSize];
        private bool _show;
        public Player(bool show)
        {
            _show = show;
            BattleShips = new BattleShip[NumberOfShips];
            _dy = ShipWarsForm._ClientSize.Height / 2 - (GameBoard.BoardSize / 2) * Dt;
            _dx = 2 * ShipWarsForm._ClientSize.Width / 3 - (GameBoard.BoardSize / 2) * Dt;
            InitShips();
        }

       

        private void InitShips()
        {
            BattleShips[0] = new BattleShip(1, 3, 30, new Point(30, 50), Color.LightGreen);
            BattleShips[1] = new BattleShip(1, 4, 30, new Point(90, 50), Color.BlueViolet);
            BattleShips[2] = new BattleShip(1, 4, 30, new Point(150, 50), Color.Goldenrod);
            BattleShips[3] = new BattleShip(5, 1, 30, new Point(30, 200), Color.Brown);
            BattleShips[4] = new BattleShip(5, 2, 30, new Point(30, 260), Color.DarkCyan);
            BattleShips[5] = new BattleShip(6, 1, 30, new Point(30, 340), Color.Tomato);

            foreach (var ship in BattleShips)
                ship.MouseUp += b_MouseUp;
            if(_show)
                ShipWarsForm._Collection.AddRange(BattleShips);
        }
        private void b_MouseUp(object sender, MouseEventArgs e)
        {
            var b = sender as BattleShip;
            // remove the button from the Taken places.
            RemoveFomBoard(b);
            b?.MouseClick();

            AddToBoard(b);
        }

        private void RemoveFomBoard(BattleShip b)
        {
            if (b.Colum == -1) return;
            foreach (var p in b.IndexPoints)
                takenCells[p.X, p.Y] = false;
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
                    if (!takenCells[b.Colum + j, b.Row + i]) continue;
                        b.Reset();
                        return false;

                }
            }

            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    takenCells[b.Colum + j, b.Row + i] = true;
                    b.IndexPoints.Add(new Point(b.Colum + j, b.Row + i));
                }
            }

            b.Location = new Point(_dx + b.Colum * Dt, _dy + b.Row * Dt);
            return true;
        }


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
            b.Colum = (int)moveRight + (b.Location.X - _dx) / Dt;

            CheckShip(b);
        }

        private bool AddShip(BattleShip b,int row, int col)
        {
            RemoveFomBoard(b);

            var r = b.Row;
            var c = b.Colum;

            b.Row = row;
            b.Colum = col;
            if (CheckShip(b))
                return true;

            (b.Row, b.Colum) = (r, c);
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

        public void Draw(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)),
                new Rectangle(0, 0, ShipWarsForm._ClientSize.Width, ShipWarsForm._ClientSize.Height));


            for (var i = 0; i < GameBoard.BoardSize; i++)
            for (var j = 0; j < GameBoard.BoardSize; j++)
                g.DrawRectangle(new Pen(Color.Black, 2), new Rectangle(new Point(
                    _dx + i * Dt, _dy + j * Dt), new Size(Dt, Dt)));
        }

        public bool IsReady()
        {
            return BattleShips.All(ship => ship.Colum != -1);
        }

        public sealed class BattleShip : Button
        {
            public List<Point> IndexPoints;
            public int Row, Colum, Hitpoints;
            private static readonly bool[,] Taken = new bool[GameBoard.BoardSize, GameBoard.BoardSize];
            private readonly Point _originLocation;
            private Point _originCursor;
            private Point _originControl;
            private bool _btnDragging;
            private bool _doTurn;
            private readonly int _dt;


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
            }

            public void MouseClick()
            {
                _btnDragging = false;
                Capture = false;
                // Remove focus from controller
                if (Form.ActiveForm != null) Form.ActiveForm.ActiveControl = null;

                if (_doTurn)
                    (Height, Width) = (Width, Height);
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

            public void Reset()
            {
                Row = Colum = -1;
                Location = _originLocation;
                IndexPoints.Clear();
            }
        }
    }
}