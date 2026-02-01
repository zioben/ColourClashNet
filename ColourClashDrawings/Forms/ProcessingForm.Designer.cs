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
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            lblProgress = new Label();
            pbarComplete = new ProgressBar();
            lblPerc = new Label();
            btnAbort = new Button();
            tableLayoutPanel3 = new TableLayoutPanel();
            picImageTemp = new PictureBox();
            lbHistory = new ListBox();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picImageTemp).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            tableLayoutPanel1.Size = new Size(515, 293);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 4;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128F));
            tableLayoutPanel2.Controls.Add(lblProgress, 0, 0);
            tableLayoutPanel2.Controls.Add(pbarComplete, 1, 0);
            tableLayoutPanel2.Controls.Add(lblPerc, 2, 0);
            tableLayoutPanel2.Controls.Add(btnAbort, 3, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 248);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(509, 42);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // lblProgress
            // 
            lblProgress.AutoSize = true;
            lblProgress.Dock = DockStyle.Fill;
            lblProgress.Location = new Point(3, 0);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(58, 42);
            lblProgress.TabIndex = 0;
            lblProgress.Text = "Progress";
            lblProgress.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pbarComplete
            // 
            pbarComplete.Dock = DockStyle.Fill;
            pbarComplete.Location = new Point(67, 3);
            pbarComplete.Name = "pbarComplete";
            pbarComplete.Size = new Size(247, 36);
            pbarComplete.TabIndex = 1;
            // 
            // lblPerc
            // 
            lblPerc.AutoSize = true;
            lblPerc.Dock = DockStyle.Fill;
            lblPerc.Location = new Point(320, 0);
            lblPerc.Name = "lblPerc";
            lblPerc.Size = new Size(58, 42);
            lblPerc.TabIndex = 2;
            lblPerc.Text = "%";
            lblPerc.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnAbort
            // 
            btnAbort.Dock = DockStyle.Fill;
            btnAbort.Location = new Point(384, 3);
            btnAbort.Name = "btnAbort";
            btnAbort.Size = new Size(122, 36);
            btnAbort.TabIndex = 3;
            btnAbort.Text = "Abort Processing";
            btnAbort.UseVisualStyleBackColor = true;
            btnAbort.Click += btnAbort_Click;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37.329155F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62.670845F));
            tableLayoutPanel3.Controls.Add(picImageTemp, 1, 0);
            tableLayoutPanel3.Controls.Add(lbHistory, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(509, 239);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // picImageTemp
            // 
            picImageTemp.BackColor = Color.Black;
            picImageTemp.Dock = DockStyle.Fill;
            picImageTemp.Location = new Point(193, 3);
            picImageTemp.Name = "picImageTemp";
            picImageTemp.Size = new Size(313, 233);
            picImageTemp.SizeMode = PictureBoxSizeMode.Zoom;
            picImageTemp.TabIndex = 0;
            picImageTemp.TabStop = false;
            // 
            // lbHistory
            // 
            lbHistory.Dock = DockStyle.Fill;
            lbHistory.FormattingEnabled = true;
            lbHistory.ItemHeight = 15;
            lbHistory.Location = new Point(3, 3);
            lbHistory.Name = "lbHistory";
            lbHistory.Size = new Size(184, 233);
            lbHistory.TabIndex = 1;
            // 
            // ProcessingForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(515, 293);
            ControlBox = false;
            Controls.Add(tableLayoutPanel1);
            Name = "ProcessingForm";
            Text = "FormAdvancer";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picImageTemp).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Label lblProgress;
        private ProgressBar pbarComplete;
        private Label lblPerc;
        private Button btnAbort;
        private TableLayoutPanel tableLayoutPanel3;
        private PictureBox picImageTemp;
        private ListBox lbHistory;
    }
}