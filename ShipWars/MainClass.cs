using System;
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
            Game = new Game();
        }

        public void Draw(Graphics g)
        {
            if(MainMenu.Menu == Menus.Game)
                Game.Draw(g);
            else MainMenu.Draw(g);
        }

        public void MouseMove(MouseEventArgs e)
        {
            if(MainMenu.Menu == Menus.Game)
                Game.MouseMove(e);
        }

        public void MouseUp(MouseEventArgs e)
        {
            if (MainMenu.Menu == Menus.Game)
                Game.MouseUp(e);
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (MainMenu.Menu == Menus.Game)
                Game.MouseDown(e);
        }

    }
}