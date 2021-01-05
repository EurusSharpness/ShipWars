using System.Drawing;
using System.Windows.Forms;

namespace ShipWars
{
    public class MainClass
    {
        public MainMenu MainMenu;
        public Game Game;

        public MainClass()
        {
            MainMenu = new MainMenu();
            Game = null;
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
    }
}