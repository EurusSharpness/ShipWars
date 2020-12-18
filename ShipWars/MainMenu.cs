using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShipWars
{
    public enum Menus
    {
        Main,
        Start,
        Help,
        Settings,
        Game,
        Win,
        Lose,
        Pause
    };

    public class MainMenu
    {
        private Button _start, _help, _settings;
        private Button _back, _vsComp, _vsPlayer;

        public Menus Menu;

        private const string Instructions =
            "1: Place your ships, your ships can be rotated sideways\n" +
            "2: Destroy enemy ships by selecting one of the undestroyed cells on the enemy map.\n" +
            "3: Destroy the enemy ships before they destroy you.";

        public MainMenu()
        {
            Menu = Menus.Main;
            CreateButtons();
        }

        public void Draw(Graphics g)
        {
            var x = 0;
            switch (Menu)
            {
                case Menus.Main:
                    x = 1;
                    DrawMainPage(g);
                    break;

                case Menus.Start:
                    x = 2;
                    DrawStartPage(g);
                    break;

                case Menus.Help:
                    x = 3;
                    DrawHelpPage(g);
                    break;

                case Menus.Settings:
                    x = 4;
                    DrawSettingsPage(g);
                    break;

                case Menus.Game:
                    x = 5;
                    DrawGamePage(g);
                    break;

                case Menus.Pause:
                    x = 6;
                    DrawPausePage(g);
                    break;

                case Menus.Win:
                    x = 7;
                    DrawWinPage(g);
                    break;

                case Menus.Lose:
                    x = 8;
                    DrawLosePage(g);
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

            _settings.Location = new Point(GetHalf(ShipWarsForm._ClientSize,
                TextRenderer.MeasureText(_settings.Text, _settings.Font)));

            _start.Location = new Point((ShipWarsForm._ClientSize.Width - _start.Width) / 2,
                (ShipWarsForm._ClientSize.Height - _start.Height) / 2 - _settings.Height);

            _help.Location =
                new Point((ShipWarsForm._ClientSize.Width - _help.Width) / 2,
                    (ShipWarsForm._ClientSize.Height - _help.Height) / 2 + _settings.Height);

            _help.Visible = true;
            _start.Visible = _settings.Visible = true;
            _start.Enabled = _settings.Enabled = _help.Enabled = true;
        }

        private void DrawHelpPage(Graphics g)
        {
            _back.Visible = _back.Enabled = true;
            var format1 = new StringFormat(StringFormatFlags.NoClip)
            {
                LineAlignment = StringAlignment.Near,
                Alignment = StringAlignment.Near
            };

            g.DrawString(Instructions,
                new Font("", ShipWarsForm._ClientSize.Width * 0.015f),
                Brushes.Red,
                new Rectangle(new Point(0, 100),
                    new Size(ShipWarsForm._ClientSize.Width, ShipWarsForm._ClientSize.Height)),
                format1);
        }

        private void DrawSettingsPage(Graphics g)
        {
            _back.Visible = _back.Enabled = true;
        }

        private void DrawStartPage(Graphics g)
        {
            _vsComp.Location = new Point(
                (ShipWarsForm._ClientSize.Width - _vsComp.Width) / 2,
                (ShipWarsForm._ClientSize.Height - _vsComp.Height) / 2 - _vsComp.Height);
            _vsPlayer.Location = new Point(
                (ShipWarsForm._ClientSize.Width - _vsPlayer.Width) / 2,
                (ShipWarsForm._ClientSize.Height - _vsPlayer.Height) / 2 + _vsPlayer.Height);

            _back.Visible = _vsComp.Visible = _vsPlayer.Visible = true;
            _vsComp.Enabled = _back.Enabled = _vsPlayer.Enabled = true;
        }

        private void DrawGamePage(Graphics g)
        {
            _back.Visible = _back.Enabled = true;
        }

        private void DrawPausePage(Graphics g)
        {
            _back.Visible = _back.Enabled = true;
        }

        private void DrawWinPage(Graphics g)
        {
            _back.Visible = _back.Enabled = true;
        }

        private void DrawLosePage(Graphics g)
        {
            _back.Visible = _back.Enabled = true;
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

            ShipWarsForm._Collection.Add(_start);
            ShipWarsForm._Collection.Add(_settings);
            ShipWarsForm._Collection.Add(_help);
        }

        private void CreateStartPage()
        {
            _vsComp = BasicButton("Player Vs Computer");
            _vsPlayer = BasicButton("Player vs Player");

            _vsComp.MouseClick += (sender, args) =>
            {
                Menu = Menus.Game;
                Disable();
            };
            _vsPlayer.MouseClick += (sender, args) =>
            {
                Menu = Menus.Game;
                Disable();
            };

            ShipWarsForm._Collection.Add(_vsComp);
            ShipWarsForm._Collection.Add(_vsPlayer);
            ShipWarsForm._Collection.Add(_back);
        }

        private void CreateHelpPage()
        {
        }

        private void CreateSettingsPage()
        {
        }

        private void CreateGamePage()
        {
        }

        private void CreateWinPage()
        {
        }

        private void CreateLosePage()
        {
        }

        private void CreatePausePage()
        {
        }

        private void CreateButtons()
        {
            CreateBackButton();
            CreateMainPage();
            CreateStartPage();
            CreateSettingsPage();
            CreateHelpPage();
            CreateGamePage();
            CreateWinPage();
            CreateLosePage();
            CreatePausePage();
        }

        private void CreateBackButton()
        {
            _back = BasicButton("<==");
            _back.MouseClick += (sender, args) =>
            {
                Disable();
                if (Menu == Menus.Game)
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
                Font = new Font("ALGERIAN", ShipWarsForm._ClientSize.Height / 20f, FontStyle.Italic | FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = Color.Blue,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Enabled = false,
                Visible = false,
                Size = TextRenderer.MeasureText(text, new Font("ALGERIAN", 24, FontStyle.Italic | FontStyle.Bold))
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