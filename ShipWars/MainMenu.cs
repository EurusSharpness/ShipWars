using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ShipWars
{
    public enum Menus
    {
        Main,
        Start,
        Help,
        Settings,
        GameOffline,
        GameOnline,
        Win,
        Lose,
        Pause
    };

    public class MainMenu
    {
        private Button _start, _help, _settings;
        private Button _back, _vsComp, _vsPlayer;

        public Menus Menu;
        
        private Bitmap MainBackgound_img, HelpBackground_img;

        /*private const string Instructions =
            "1: Place your ships, your ships can be rotated sideways (by clicking on them)\n" +
            "2: Destroy enemy ships by selecting one of the undestroyed cells on the enemy map.\n" +
            "3: Destroy the enemy ships before they destroy you.";*/

        public MainMenu()
        {
            Menu = Menus.Main;
            CreateImages();
            CreateButtons();
        }
        private void CreateImages()
        {
            MainBackgound_img = new Bitmap(ShipWarsForm.CanvasSize.Width, ShipWarsForm.CanvasSize.Height,
                PixelFormat.Format24bppRgb);
            var g = Graphics.FromImage(MainBackgound_img);
            var image = Properties.Resources.MainBG2;
            g.DrawImage(image, new RectangleF(new Point(0, 0), new SizeF(ShipWarsForm.CanvasSize.Width, ShipWarsForm.CanvasSize.Height)));

            HelpBackground_img = new Bitmap(ShipWarsForm.CanvasSize.Width, ShipWarsForm.CanvasSize.Height,
                PixelFormat.Format24bppRgb);
            g = Graphics.FromImage(HelpBackground_img);
            image = Properties.Resources.HelpPage;
            g.DrawImage(image, new RectangleF(new Point(0, 0), new SizeF(ShipWarsForm.CanvasSize.Width, ShipWarsForm.CanvasSize.Height)));
        }
        public void Draw(Graphics g)
        {
            switch (Menu)
            {
                case Menus.Main:
                    DrawMainPage(g);
                    break;

                case Menus.Start:
                    DrawStartPage(g);
                    break;

                case Menus.Help:
                    DrawHelpPage(g);
                    break;

                case Menus.Settings:
                    DrawSettingsPage(g);
                    break;

                case Menus.GameOffline:
                    DrawGamePage(g);
                    break;

                default:
                    throw new IndexOutOfRangeException();
            }

            //g.DrawString($"{x}", new Font("", 16), new SolidBrush(Color.Black), new PointF(0, 0));
        }

        #region DrawPages

        private void DrawMainPage(Graphics g)
        {
            // Relocate them to make sure its relocated to the center if the user changed the client size.
            g.DrawImage(MainBackgound_img, new PointF(0, 0));
            _settings.Location = new Point(GetHalf(ShipWarsForm.CanvasSize,
                TextRenderer.MeasureText(_settings.Text, _settings.Font)));

            _start.Location = new Point((ShipWarsForm.CanvasSize.Width - _start.Width) / 2,
                (ShipWarsForm.CanvasSize.Height - _start.Height) / 2 - _settings.Height);

            _help.Location =
                new Point((ShipWarsForm.CanvasSize.Width - _help.Width) / 2,
                    (ShipWarsForm.CanvasSize.Height - _help.Height) / 2 + _settings.Height);

            _help.Visible = true;
            _start.Visible = _settings.Visible = true;
            _start.Enabled = _settings.Enabled = _help.Enabled = true;
        }

        private void DrawHelpPage(Graphics g)
        {
            _back.Visible = _back.Enabled = true;
            // var format1 = new StringFormat(StringFormatFlags.NoClip)
            // {
            //     LineAlignment = StringAlignment.Near,
            //     Alignment = StringAlignment.Near
            // };
            //
            // g.DrawString(Instructions,
            //     new Font("", ShipWarsForm.CanvasSize.Width * 0.015f),
            //     Brushes.DarkSeaGreen,
            //     new Rectangle(new Point(0, 100),
            //         new Size(ShipWarsForm.CanvasSize.Width, ShipWarsForm.CanvasSize.Height)),
            //     format1);
            var Size = TextRenderer.MeasureText("LE HELPO PAGE",
                new Font("Arial", 30, FontStyle.Italic | FontStyle.Bold));

            g.DrawString("LE HELPO PAGE", new Font("Arial", 30, FontStyle.Bold | FontStyle.Italic), Brushes.MidnightBlue, new PointF(
                ShipWarsForm.CanvasSize.Width / 2 - Size.Width / 2,  20
                ));
            g.DrawImage(HelpBackground_img, new PointF(0,100));
        }

        private void DrawSettingsPage(Graphics g)
        {
            _back.Visible = _back.Enabled = true;
        }

        private void DrawStartPage(Graphics g)
        {
            //g.DrawImage(Properties.Resources.StartBackground, new RectangleF(new PointF(0,0), new SizeF(ShipWarsForm.CanvasSize.Width, ShipWarsForm.CanvasSize.Height)));
            g.FillRectangle(Brushes.LightSkyBlue, new Rectangle(new Point(0,0),new Size(ShipWarsForm.CanvasSize.Width, ShipWarsForm.CanvasSize.Height) ));
            g.DrawImage(MainBackgound_img, new PointF(0, 0));
            _vsComp.Location = new Point(
                (ShipWarsForm.CanvasSize.Width - _vsComp.Width) / 2,
                (ShipWarsForm.CanvasSize.Height - _vsComp.Height) / 2 - _vsComp.Height);
            _vsPlayer.Location = new Point(
                (ShipWarsForm.CanvasSize.Width - _vsPlayer.Width) / 2,
                (ShipWarsForm.CanvasSize.Height - _vsPlayer.Height) / 2 + _vsPlayer.Height);

            _back.Visible = _vsComp.Visible = _vsPlayer.Visible = true;
            _vsComp.Enabled = _back.Enabled = _vsPlayer.Enabled = true;
        }

        private void DrawGamePage(Graphics g)
        {
            //_back.Visible = _back.Enabled = true;
        }

        #endregion DrawPages

        #region CreateButtons

        private void CreateMainPage()
        {
            // Set basic functions for the buttons
            _start = BasicButton("Start");
            _settings = BasicButton("Settings");
            _help = BasicButton("Help");

            _start.MouseClick += (sender, args) =>
            {
                Menu = Menus.Start;
                Disable();
            };
            _settings.MouseClick += (sender, args) =>
            {
                Menu = Menus.Settings;
                Disable();
            };
            _help.MouseClick += (sender, args) =>
            {
                Menu = Menus.Help;
                Disable();
            };

            // Set location for the buttons

            ShipWarsForm.Collection.Add(_start);
            ShipWarsForm.Collection.Add(_settings);
            ShipWarsForm.Collection.Add(_help);
        }

        private void CreateStartPage()
        {
            _vsComp = BasicButton("Player Vs Computer");
            _vsPlayer = BasicButton("Player vs Player");

            _vsComp.MouseClick += (sender, args) =>
            {
                Menu = Menus.GameOffline;
                Disable();
            };
            _vsPlayer.MouseClick += (sender, args) =>
            {
                Menu = Menus.GameOnline;
                Disable();
                // MessageBox.Show(@"Work in progress...", @"NO!");
            };

            ShipWarsForm.Collection.Add(_vsComp);
            ShipWarsForm.Collection.Add(_vsPlayer);
            ShipWarsForm.Collection.Add(_back);
        }

        private void CreateHelpPage()
        {
        }

        private void CreateSettingsPage()
        {
        }

        private void CreateButtons()
        {
            CreateBackButton();
            CreateMainPage();
            CreateStartPage();
            CreateSettingsPage();
            CreateHelpPage();
        }

        private void CreateBackButton()
        {
            _back = BasicButton("↫");
            _back.Font = new Font("Times New Roman", ShipWarsForm.CanvasSize.Height / 12f,
                FontStyle.Italic | FontStyle.Bold);

            _back.MouseClick += (sender, args) =>
            {
                Disable();
                if (Menu == Menus.GameOffline || Menu == Menus.GameOnline)
                {
                    var r = MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo);
                    if (r != DialogResult.Yes) return;
                    Menu = Menus.Main;
                    _back.Visible = _back.Enabled = false;
                }
                else
                {
                    Menu = Menus.Main;
                    _back.Visible = _back.Enabled = false;
                }
            };
        }

        private void Disable()
        {
            _vsComp.Visible = _vsPlayer.Visible = _back.Visible = false;
            _vsComp.Enabled = _vsPlayer.Enabled = _back.Enabled = false;
            _start.Visible = _settings.Visible = _help.Visible = false;
            _start.Enabled = _settings.Enabled = _help.Enabled = false;
        }

        private Button BasicButton(string text)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Times New Roman", ShipWarsForm.CanvasSize.Height / 20f, FontStyle.Italic | FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = Color.Blue,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Enabled = false,
                Visible = false,
                Size = TextRenderer.MeasureText(text, new Font("Times New Roman", 24, FontStyle.Italic | FontStyle.Bold))
            };

            // Make it transparent
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.Transparent;
            b.FlatAppearance.MouseDownBackColor = Color.Transparent;

            // Add functionality
            b.MouseEnter += (s, e) => { b.ForeColor = Color.Red; };
            b.MouseLeave += (s, e) => { b.ForeColor = Color.Blue; };
            return b;
        }

        private Size GetHalf(Size a, Size b) => new Size((a.Width - b.Width) / 2, (a.Height - b.Height) / 2);

        #endregion CreateButtons
    }
}