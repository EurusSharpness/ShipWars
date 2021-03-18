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
        /// <summary> Ship numbers, type of the ship: 3x1, 4x1, 4x1, 5x1, 5x2, 6x1 </summary>
        public const int NumberOfShips = 6; 
        /// <summary> The ships of the player and their location on board </summary>
        public BattleShip[] BattleShips;
        /// <summary> the size of the cell </summary>
        public const int Dt = 30; 
        /// <summary> The board starting Y </summary>
        private static int _dy;
        /// <summary> The board starting X </summary>
        private static int _dx;
        /// <summary> Which places are taken from the board </summary>
        private readonly bool[,] _takenCells = new bool[GameBoard.BoardSize, GameBoard.BoardSize];
        /// <summary> To Show or not to Show the buttons </summary>
        private readonly bool _show;
        /// <summary> Number of lives the player has, 3+4+4+5+10+6 </summary>
        public int HealthPoints = 32;

        public Button randomFleet;  
        /// <summary>
        /// 
        /// </summary>
        /// <param name="show">Are you the enemy or not?</param>
        public Player(bool show)
        {
            _show = show;
            BattleShips = new BattleShip[NumberOfShips];
            _dy = ShipWarsForm.CanvasSize.Height / 2 - (GameBoard.BoardSize / 2) * Dt;
            _dx = 2 * ShipWarsForm.CanvasSize.Width / 3 - (GameBoard.BoardSize / 2) * Dt;
            GRF_Button();
            InitShips();
        }

        private void GRF_Button()
        {
            randomFleet = new Button()
            {
                Name = @"RandomButton",
                Text = @"Generate Random Fleet",
                AutoSize = true,
                Font = new Font("ALGERIAN", ShipWarsForm.CanvasSize.Height / 24f, FontStyle.Italic | FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                FlatAppearance = {BorderSize = 0},
            };
            randomFleet.FlatAppearance.MouseDownBackColor = Color.Transparent;
            randomFleet.FlatAppearance.MouseOverBackColor = Color.Transparent;
            randomFleet.MouseEnter += (sender, args) => randomFleet.ForeColor = Color.Gold;
            randomFleet.MouseLeave += (sender, args) => randomFleet.ForeColor = Color.Black;
            randomFleet.MouseClick += (sender, args) => GenerateRandomFleet();
            randomFleet.MouseDown += (sender, args) => randomFleet.ForeColor = Color.Gold;
            randomFleet.MouseUp += (sender, args) =>
            {
                if (Form.ActiveForm != null) Form.ActiveForm.ActiveControl = null;
            };
            randomFleet.Size = TextRenderer.MeasureText(randomFleet.Text, randomFleet.Font);
            randomFleet.Location = new Point(
                (ShipWarsForm.CanvasSize.Width - randomFleet.Width) / 2,
                (int) (ShipWarsForm.CanvasSize.Height * 0.80f)
            );
        }
        
        /// <summary>
        /// Create the buttons at their starting locations, and set their properties.
        /// </summary>
        private void InitShips()
        {
            BattleShips[0] = new BattleShip(1, 3, new Point(30, 50), Color.LightGreen, new Bitmap(Properties.Resources.Ship3x1));
            BattleShips[1] = new BattleShip(1, 4, new Point(90, 50), Color.DarkOliveGreen, new Bitmap(Properties.Resources.Ship4x1V1));
            BattleShips[2] = new BattleShip(1, 4, new Point(150, 50), Color.Chartreuse, new Bitmap(Properties.Resources.Ship4x1V2));
            BattleShips[3] = new BattleShip(1, 5, new Point(30, 200), Color.SpringGreen, new Bitmap(Properties.Resources.Ship5x1));
            BattleShips[4] = new BattleShip(2, 5, new Point(90, 200), Color.Goldenrod, new Bitmap(Properties.Resources.Ship5x2));
            BattleShips[5] = new BattleShip(6, 1, new Point(30, 380), Color.Chocolate, new Bitmap(Properties.Resources.Ship6x1));
            
            // Add mouse functionality to each button.
            foreach (var ship in BattleShips)
                ship.MouseUp += b_MouseUp;
            
            if (!_show) return;

            foreach (var ship in BattleShips)
                ShipWarsForm.Collection.Add(ship);
            ShipWarsForm.Collection.Add(randomFleet);
        }

        private void b_MouseUp(object sender, MouseEventArgs e)
        {
            var b = sender as BattleShip;
            // remove the button from the Taken places.
            RemoveFomBoard(b);
            b?.ButtonClick();

            AddToBoard(b);
        }

        /// <summary>
        /// If the button is inside the board then Remove the given ship from the board.
        /// </summary>
        /// <param name="b">The Ship to remove</param>
        private void RemoveFomBoard(BattleShip b)
        {
            if (b.Column == -1) return;
            foreach (var p in b.IndexPoints)
                _takenCells[p.X, p.Y] = false;
            b.IndexPoints.Clear();
        }

        /// <summary>
        /// Check if the given ship can ge set where the player wants... the spot not taken.
        /// </summary>
        /// <param name="b">The ship to check</param>
        /// <returns></returns>
        private bool CheckShip(BattleShip b)
        {
            try
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
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
            b.Location = new Point(_dx + b.Column * Dt, _dy + b.Row * Dt);

            return true;
        }

        /// <summary>
        /// Add the ship to the board if its legal.
        /// </summary>
        /// <param name="b">The ship to add</param>
        
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
            // ReSharper disable PossibleLossOfFraction
            var moveRight = ((b.Location.X - _dx) / (double) Dt) - (b.Location.X - _dx) / Dt;
            if (moveRight >= 0.5)
                moveRight = 1;
            var moveDown = ((b.Location.Y - _dy) / (double) Dt) - (b.Location.Y - _dy) / Dt;
            // ReSharper restore PossibleLossOfFraction
            if (moveDown >= 0.5)
                moveDown = 1;

            // if the ship is outside the bounds then don't move it back in bounds.
            if (deltaRight > 0)
                moveRight = 0;
            if (deltaBottom > 0)
                moveDown = 0;
            
            // Set the row and column of the button relatively to the board.
            b.Row = (int) moveDown + (b.Location.Y - _dy) / Dt;
            b.Column = (int) moveRight + (b.Location.X - _dx) / Dt;

            // Check if its legit or not.
            CheckShip(b);
        }

        /// <summary>
        /// Add the ship using the given row and column
        /// </summary>
        /// <param name="b">The Ship to add</param>
        /// <param name="row">Vertical index on the board</param>
        /// <param name="col">Horizontal index on the board</param>
        /// <returns>True if the ship was successfully set in place, false otherwise </returns>
        private bool AddShip(BattleShip b, int row, int col)
        {
            // start a new.
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

        /// <summary>
        /// Generate a random fleet for the player.
        /// </summary>
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
                // Do turn, if the ship is horizontal or vertical
                var flag = rand.Next(0, 2) == 0;
                if (flag)
                    ship.Rotate();
                // Pick a point that does not exceed the board max size, 13.
                var col = rand.Next(0, GameBoard.BoardSize - ship.Width / Dt + 1);
                var row = rand.Next(0, GameBoard.BoardSize - ship.Height / Dt + 1);
                // Check if it was a legit move or not.
                if (AddShip(ship, row, col)) continue;
                if (flag) // return the state of the ship
                {
                    ship.Rotate();
                }

                goto REPEAT; // pick another location.
            }
        }

        /// <summary>
        /// Draw the board
        /// </summary>
        /// <param name="g">The graphics boi</param>
        public static void Draw(Graphics g)
        {
            for (var i = 0; i < GameBoard.BoardSize; i++)
            {
                for (var j = 0; j < GameBoard.BoardSize; j++)
                {
                    g.DrawRectangle(new Pen(Color.Black, 2), new Rectangle(new Point(
                        _dx + i * Dt, _dy + j * Dt), new Size(Dt, Dt)));
                }
            }
        }

        /// <summary>
        /// Return true if all ships are in the board
        /// </summary>
        /// <returns></returns>
        public bool IsReady()
        {
            return BattleShips.All(ship => ship.Column != -1);
        }

        public sealed class BattleShip : Button
        {
            /// <summary>The list of index the ship took from the board.</summary>
            public readonly List<Point> IndexPoints;
            /// <summary> Location on board </summary>
            public int Row, Column;
            /// <summary>Starting location</summary>
            private readonly Point _originLocation;
            /// <summary>Cursor location when clicked down</summary>
            private Point _originCursor;
            /// <summary>Location when clicked down</summary>
            private Point _originControl;
            /// <summary>If the button being dragged</summary>
            private bool _btnDragging;
            /// <summary>Flip the ship</summary>
            private bool _doTurn;
            //
            public Bitmap shipImage;
            /// <summary>
            /// Initialize the ship with standard proprieties.
            /// </summary>
            /// <param name="width">Relative to the board, how many cells wide</param>
            /// <param name="height">Relative to the board, how many cells high</param>
            /// <param name="location">Starting location.</param>
            /// <param name="c">The color of the ship</param>
            public BattleShip(int width, int height, Point location, Color c, Bitmap img = null)
            {
                Row = Column = -1;
                IndexPoints = new List<Point>();
                Width = width * Dt;
                Height = height * Dt;
                _originLocation = location;
                FlatStyle = FlatStyle.Flat;
                Enabled = true;
                Visible = true;
                BackColor = Color.Transparent;
                ResizeRedraw = false;
                Location = location;
                DoubleBuffered = true;
                shipImage = img;
                if(img != null)
                    Image = new Bitmap(img, new Size(Math.Min(Width, Height), Math.Max(Width, Height)));
                if(Height < Width)
                    Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                
                //ImageAlign = ContentAlignment.TopCenter;
                MouseDown += B_MouseDown;
                MouseMove += B_MouseMove;
                FlatStyle = FlatStyle.Flat;
                ForeColor = Color.Black;
                BackColor = Color.Transparent;
                FlatAppearance.BorderSize = 0;
                FlatAppearance.MouseDownBackColor = Color.Transparent;
                FlatAppearance.MouseOverBackColor = Color.Transparent;
            }

            /// <summary>
            /// Rotate the ship.
            /// </summary>
            public void ButtonClick()
            {
                _btnDragging = false;
                Capture = false;
                // Remove focus from controller
                if (Form.ActiveForm != null) Form.ActiveForm.ActiveControl = null;
                if (_doTurn)
                {
                    Rotate();
                }
            }

            /// <summary>
            /// Set the original location of the mouse to calculate when the button being dragged.
            /// </summary>
            private void B_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left) return;
                Capture = true;
                _originCursor = ShipWarsForm.MouseCords;
                _originControl = Location;
                _btnDragging = true;
                _doTurn = true;
            }
            
            /// <summary>
            /// Change the button location according the mouse.
            /// </summary>
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

            /// <summary>
            /// Reset the button location and indexes.
            /// </summary>
            public void Reset()
            {
                Row = Column = -1;
                Location = _originLocation;
                IndexPoints.Clear();
            }

            internal void Rotate()
            {
                (Height, Width) = (Width, Height);
                Image = new Bitmap(shipImage, new Size(Math.Min(Width, Height), Math.Max(Width, Height)));
                if (Height < Width)
                    Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
        }
    }
}