namespace ColourClashNet
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            colorAnalyzer1 = new ColorAnalyzer();
            SuspendLayout();
            // 
            // colorAnalyzer1
            // 
            colorAnalyzer1.Dock = DockStyle.Fill;
            colorAnalyzer1.Location = new Point(0, 0);
            colorAnalyzer1.Name = "colorAnalyzer1";
            colorAnalyzer1.Size = new Size(1184, 817);
            colorAnalyzer1.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 817);
            Controls.Add(colorAnalyzer1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private ColorAnalyzer colorAnalyzer1;
    }
}