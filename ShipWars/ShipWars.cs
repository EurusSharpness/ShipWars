using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ShipWars
{
    public partial class ShipWarsForm : Form
    {
        public static Size _ClientSize;
        public static Control.ControlCollection _Collection;
        public static Point _MouseCords;
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
            ClientSize = new Size((int)(Screen.PrimaryScreen.Bounds.Width * 0.8), (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.8));
            _Collection = Controls;
            _ClientSize = ClientSize;
            _mainClass = new MainClass();

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
            _ClientSize = ClientSize;
            _mainClass.Draw(e.Graphics);
            e.Graphics.DrawString($"X: {_MouseCords}", new Font("", 16), Brushes.Black, 0, 0);
        }

        private void Invalidator_Tick(object sender, EventArgs e)
        {
            // (_MouseCords.X, _MouseCords.Y) = (-Location.X + Cursor.Position.X - 10, -Location.Y + Cursor.Position.Y - 30);
            _MouseCords = PointToClient(Cursor.Position);
            Invalidate();
        }

        private void ShipWarsForm_MouseMove(object sender, MouseEventArgs e)
        {
            _mainClass.MouseMove(e);
        }

        private void ShipWarsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                if (MessageBox.Show(@"Are you sure you want to exit?", @"Exit", MessageBoxButtons.OKCancel) ==
                 DialogResult.OK)
                    Close();
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