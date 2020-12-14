using System.Drawing;

namespace ShipWars
{
    public class MainClass
    {
        public MainMenu MainMenu;
        public Game Game;
        public MainClass()
        {
            MainMenu = new MainMenu();
            Game = new Game();
        }

        public void Draw(Graphics g)
        {
            if(MainMenu.Menu == Menus.Game)
                Game.Draw(g);
            else MainMenu.Draw(g);
        }
    }
}