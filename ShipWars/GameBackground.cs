using System.Drawing;
using System.Drawing.Imaging;

namespace ShipWars
{
    class GameBackground
    {
        private float _x;
        private Bitmap _background;

        public GameBackground()
        {
            CreateBackground();
        }

        private void CreateBackground()
        {
            _background = new Bitmap(ShipWarsForm._ClientSize.Width * 2, ShipWarsForm._ClientSize.Height,
                PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(_background);
            var image = Properties.Resources.WaterBackground;
            int x = 0, y = 0;
            while (y <= ShipWarsForm._ClientSize.Height)
            {
                while (x <= ShipWarsForm._ClientSize.Width)
                {
                    g.DrawImage(image, new RectangleF(x, y, image.Width + 0.5f, image.Height),
                        new RectangleF(0, 0, image.Width, image.Height),
                        GraphicsUnit.Pixel);
                    g.DrawImage(image,
                        new RectangleF(ShipWarsForm._ClientSize.Width + x, y, image.Width + 0.5f,
                            image.Height),
                        new RectangleF(0, 0, image.Width, image.Height),
                        GraphicsUnit.Pixel);
                    x += image.Width;
                }

                x = 0;
                y += image.Height;
            }
        }

        public void Draw(Graphics g)
        {
            _x = (_x < ShipWarsForm._ClientSize.Width) ? _x + 1.5f : 0;
            g.DrawImage(_background, _x - ShipWarsForm._ClientSize.Width, 0);
        }
    }
}