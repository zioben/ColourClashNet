namespace ColourClashNet.Controls
{
    partial class ProcessingViewer
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

        #region Component Designer generated code

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
            tableLayoutPanel3 = new TableLayoutPanel();
            picImageTemp = new PictureBox();
            lbHistory = new ListBox();
            groupBox1 = new GroupBox();
            groupBox2 = new GroupBox();
            picImageSource = new PictureBox();
            btnAbort = new Button();
            lblPerc = new Label();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picImageTemp).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picImageSource).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 2);
            tableLayoutPanel1.Controls.Add(lbHistory, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 200F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            tableLayoutPanel1.Size = new Size(576, 405);
            tableLayoutPanel1.TabIndex = 0;
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
            tableLayoutPanel2.Location = new Point(0, 357);
            tableLayoutPanel2.Margin = new Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(576, 48);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // lblProgress
            // 
            lblProgress.AutoSize = true;
            lblProgress.Dock = DockStyle.Fill;
            lblProgress.Location = new Point(0, 0);
            lblProgress.Margin = new Padding(0);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(64, 48);
            lblProgress.TabIndex = 0;
            lblProgress.Text = "Progress";
            lblProgress.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pbarComplete
            // 
            pbarComplete.Dock = DockStyle.Fill;
            pbarComplete.Location = new Point(67, 3);
            pbarComplete.Name = "pbarComplete";
            pbarComplete.Size = new Size(314, 42);
            pbarComplete.TabIndex = 1;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Controls.Add(groupBox2, 0, 0);
            tableLayoutPanel3.Controls.Add(groupBox1, 1, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Size = new Size(570, 151);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // picImageTemp
            // 
            picImageTemp.BackColor = System.Drawing.Color.Black;
            picImageTemp.Dock = DockStyle.Fill;
            picImageTemp.Location = new Point(0, 16);
            picImageTemp.Name = "picImageTemp";
            picImageTemp.Size = new Size(283, 133);
            picImageTemp.TabIndex = 0;
            picImageTemp.TabStop = false;
            // 
            // lbHistory
            // 
            lbHistory.Dock = DockStyle.Fill;
            lbHistory.FormattingEnabled = true;
            lbHistory.Location = new Point(1, 158);
            lbHistory.Margin = new Padding(1);
            lbHistory.Name = "lbHistory";
            lbHistory.Size = new Size(574, 198);
            lbHistory.TabIndex = 1;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(picImageTemp);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(286, 1);
            groupBox1.Margin = new Padding(1);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(0);
            groupBox1.Size = new Size(283, 149);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Processed Image";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(picImageSource);
            groupBox2.Dock = DockStyle.Fill;
            groupBox2.Location = new Point(1, 1);
            groupBox2.Margin = new Padding(1);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(0);
            groupBox2.Size = new Size(283, 149);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Original Image";
            // 
            // picImageSource
            // 
            picImageSource.BackColor = System.Drawing.Color.Black;
            picImageSource.Dock = DockStyle.Fill;
            picImageSource.Location = new Point(0, 16);
            picImageSource.Name = "picImageSource";
            picImageSource.Size = new Size(283, 133);
            picImageSource.TabIndex = 0;
            picImageSource.TabStop = false;
            // 
            // btnAbort
            // 
            btnAbort.Dock = DockStyle.Fill;
            btnAbort.Location = new Point(449, 1);
            btnAbort.Margin = new Padding(1);
            btnAbort.Name = "btnAbort";
            btnAbort.Size = new Size(126, 46);
            btnAbort.TabIndex = 3;
            btnAbort.Text = "Abort Processing";
            btnAbort.UseVisualStyleBackColor = true;
            btnAbort.Click += btnAbort_Click;
            // 
            // lblPerc
            // 
            lblPerc.AutoSize = true;
            lblPerc.Dock = DockStyle.Fill;
            lblPerc.Location = new Point(384, 0);
            lblPerc.Margin = new Padding(0);
            lblPerc.Name = "lblPerc";
            lblPerc.Size = new Size(64, 48);
            lblPerc.TabIndex = 2;
            lblPerc.Text = "%";
            lblPerc.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ProcessingViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Name = "ProcessingViewer";
            Size = new Size(576, 405);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picImageTemp).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picImageSource).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Label lblProgress;
        private ProgressBar pbarComplete;
        private TableLayoutPanel tableLayoutPanel3;
        private PictureBox picImageTemp;
        private ListBox lbHistory;
        private GroupBox groupBox2;
        private PictureBox picImageSource;
        private GroupBox groupBox1;
        private Label lblPerc;
        private Button btnAbort;
    }
}
