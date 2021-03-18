using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ShipWars
{
    /// <summary>
    /// Create a moving background.
    /// </summary>
    public class GameBackground
    {
        private float _x = 0;
        private Bitmap _background;
        Timer t;
        public GameBackground()
        {
            CreateBackground();
            t = new Timer();
            t.Interval = 10;
            t.Tick += T_Tick;
            t.Start();
        }

        private void T_Tick(object sender, System.EventArgs e)
        {
            _x = (_x < ShipWarsForm.CanvasSize.Width) ? _x + 0.8f : 0;
        }

        /// <summary>
        /// Create an image (2 x Screen_Width, Screen_Height)
        /// </summary>
        private void CreateBackground()
        {
            _background = new Bitmap(ShipWarsForm.CanvasSize.Width * 2, ShipWarsForm.CanvasSize.Height,
                PixelFormat.Format24bppRgb);
            using var g = Graphics.FromImage(_background);
            var image = Properties.Resources.WaterBackground;
            int x = 0, y = 0;
            while (y <= ShipWarsForm.CanvasSize.Height)
            {
                while (x <= ShipWarsForm.CanvasSize.Width)
                {
                    g.DrawImage(image, new RectangleF(x, y, image.Width + 0.5f, image.Height),
                        new RectangleF(0, 0, image.Width, image.Height),
                        GraphicsUnit.Pixel);
                    g.DrawImage(image,
                        new RectangleF(ShipWarsForm.CanvasSize.Width + x, y, image.Width + 0.5f,
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
            g.DrawImage(_background, _x - ShipWarsForm.CanvasSize.Width, 0);
        }
    }
}