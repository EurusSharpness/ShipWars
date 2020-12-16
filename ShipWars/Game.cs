using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ShipWars
{
    public class Game
    {
        public GameBoard GameBoard;
        public Game()
        {
            GameBoard = new GameBoard();
        }

        public void Draw(Graphics g)
        {
            GameBoard.Draw(g);
        }
    }
}