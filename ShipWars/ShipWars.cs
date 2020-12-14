using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShipWars
{
    public partial class Form1 : Form
    {
        public static Size _ClientSize;
        public static Control.ControlCollection _Collection;
        public Form1()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _ClientSize = ClientSize;
            _Collection = Controls;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void Invalidator_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}