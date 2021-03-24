using System;
using System.Drawing;
using System.Windows.Forms;
using System.Media;

namespace ShipWars
{
    public class MainClass
    {
        public MainMenu MainMenu;
        public Game Game;
        SoundPlayer soundPlayer;
        private bool flag = false;
        public MainClass()
        {
            MainMenu = new MainMenu();
            Game = null;
            soundPlayer = new SoundPlayer(Properties.Resources.PlayLeMusic);
            //soundPlayer.PlayLooping();
        }

        public void Draw(Graphics g)
        {
            if (MainMenu.Menu == Menus.GameOffline)
            {
                Game ??= new OfflineGame();
                if (Game.PlayAgain)
                    Game = new OfflineGame();
                if (Game.BackToMainMenu)
                {
                    Game = null;
                    MainMenu.Menu = Menus.Main;
                    return;
                }
                Game.Draw(g);
            }
            if (MainMenu.Menu == Menus.GameOnline)
            {
                try
                {
                    Game ??= new OnlineGame();
                    if (Game.PlayAgain)
                        Game = new OnlineGame();
                    if (Game.BackToMainMenu)
                    {
                        Game = null;
                        MainMenu.Menu = Menus.Main;
                        return;
                    }
                    Game.Draw(g);
                }
                catch (Exception)
                {
                    MainMenu.Menu = Menus.Main;
                    MessageBox.Show(@"Server is offline, try again later ☺", @"STOOOOOP", MessageBoxButtons.OK);
                }
            }
            else MainMenu.Draw(g);
        }

        public void MouseMove(MouseEventArgs e)
        {
            if (MainMenu.Menu == Menus.GameOffline || MainMenu.Menu == Menus.GameOnline)
                Game?.MouseMove(e);
        }

        public void MouseUp(MouseEventArgs e)
        {
            if (MainMenu.Menu == Menus.GameOffline || MainMenu.Menu == Menus.GameOnline)
                Game?.MouseUp(e);
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (MainMenu.Menu == Menus.GameOffline || MainMenu.Menu == Menus.GameOnline)
                Game?.MouseDown(e);
        }

        public void KeyDown(KeyEventArgs e)
        {
            if (MainMenu.Menu == Menus.GameOffline || MainMenu.Menu == Menus.GameOnline)
                Game?.KeyDown(e);
        }
    }
}