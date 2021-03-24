using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ShipWars
{
    public partial class ShipWarsForm : Form
    {
        public static Size CanvasSize;
        public static Control.ControlCollection Collection;
        public static Point MouseCords;
        private MainClass _mainClass;

        public ShipWarsForm()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Name = "Main";
            Activate();
            ClientSize = new Size(1500, 800);
            Collection = Controls;
            CanvasSize = ClientSize;
            _mainClass = new MainClass();
            ShowInTaskbar = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            AutoSize = false;
            Focus();
        }
        
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            /*e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;*/
            CanvasSize = ClientSize;
            _mainClass.Draw(e.Graphics);

            //e.Graphics.DrawString($"X: {MouseCords}", new Font("", 16), Brushes.Black, 0, 0);
        }

        private void Invalidator_Tick(object sender, EventArgs e)
        {
            // (_MouseCords.X, _MouseCords.Y) = (-Location.X + Cursor.Position.X - 10, -Location.Y + Cursor.Position.Y - 30);
            MouseCords = PointToClient(MousePosition);
            Invalidate();
        }

        private void ShipWarsForm_MouseMove(object sender, MouseEventArgs e)
        {
            _mainClass.MouseMove(e);
        }

        private void ShipWarsForm_KeyDown(object sender, KeyEventArgs e)
        {
            _mainClass.KeyDown(e);
            /*if (e.KeyCode == Keys.Escape)
                if (MessageBox.Show(@"Are you sure you want to exit?", @"Exit", MessageBoxButtons.OKCancel) ==
                 DialogResult.OK)
                    Close();*/
        }

        private void ShipWarsForm_MouseUp(object sender, MouseEventArgs e)
        {
            _mainClass.MouseUp(e);
        }

        private void ShipWarsForm_MouseDown(object sender, MouseEventArgs e)
        {
            _mainClass.MouseDown(e);
        }
    }
}