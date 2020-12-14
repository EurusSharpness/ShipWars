using System;

namespace ShipWars
{
    partial class ShipWarsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Invalidator = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // Invalidator
            // 
            this.Invalidator.Enabled = true;
            this.Invalidator.Interval = 10;
            this.Invalidator.Tick += new System.EventHandler(this.Invalidator_Tick);
            // 
            // ShipWarsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(416, 135);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ShipWarsForm";
            this.Text = "ShipWars";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ShipWarsForm_KeyDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ShipWarsForm_MouseMove);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer Invalidator;
    }
}