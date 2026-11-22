namespace ModuleTester
{
    partial class ProcessingForm
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
            pvProcessing = new ColourClashNet.Controls.ProcessingViewer();
            SuspendLayout();
            // 
            // pvProcessing
            // 
            pvProcessing.Dock = DockStyle.Fill;
            pvProcessing.Location = new Point(0, 0);
            pvProcessing.Name = "pvProcessing";
            pvProcessing.Size = new Size(841, 589);
            pvProcessing.TabIndex = 0;
            // 
            // ProcessingForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(841, 589);
            ControlBox = false;
            Controls.Add(pvProcessing);
            Name = "ProcessingForm";
            Text = "FormAdvancer";
            ResumeLayout(false);
        }

        #endregion

        private ColourClashNet.Controls.ProcessingViewer pvProcessing;
    }
}