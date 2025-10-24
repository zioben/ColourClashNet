namespace ModuleTester
{
    partial class FormTester
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
            components = new System.ComponentModel.Container();
            splitContainer1 = new SplitContainer();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            propertyGrid1 = new PropertyGrid();
            flowLayoutPanel1 = new FlowLayoutPanel();
            comboBox1 = new ComboBox();
            btnID = new Button();
            btnQuantizer = new Button();
            btnBkgRem = new Button();
            btnAmiga = new Button();
            btnLumSat = new Button();
            btnC64 = new Button();
            btnEGA = new Button();
            btnAmstrad = new Button();
            Reprocess = new Button();
            listBox1 = new ListBox();
            bitmapRender1 = new ColourClashNet.Controls.BitmapRender(components);
            bitmapRender2 = new ColourClashNet.Controls.BitmapRender(components);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(listBox1);
            splitContainer1.Size = new Size(883, 566);
            splitContainer1.SplitterDistance = 399;
            splitContainer1.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 1);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(883, 399);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
            tableLayoutPanel2.Controls.Add(pictureBox1, 0, 0);
            tableLayoutPanel2.Controls.Add(pictureBox2, 1, 0);
            tableLayoutPanel2.Controls.Add(propertyGrid1, 2, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 43);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(877, 333);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Black;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(3, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(282, 327);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = SystemColors.ControlDarkDark;
            pictureBox2.Dock = DockStyle.Fill;
            pictureBox2.Location = new Point(291, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(282, 327);
            pictureBox2.TabIndex = 1;
            pictureBox2.TabStop = false;
            // 
            // propertyGrid1
            // 
            propertyGrid1.Dock = DockStyle.Fill;
            propertyGrid1.Location = new Point(579, 3);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.Size = new Size(295, 327);
            propertyGrid1.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(comboBox1);
            flowLayoutPanel1.Controls.Add(btnID);
            flowLayoutPanel1.Controls.Add(btnQuantizer);
            flowLayoutPanel1.Controls.Add(btnBkgRem);
            flowLayoutPanel1.Controls.Add(btnAmiga);
            flowLayoutPanel1.Controls.Add(btnLumSat);
            flowLayoutPanel1.Controls.Add(btnC64);
            flowLayoutPanel1.Controls.Add(btnEGA);
            flowLayoutPanel1.Controls.Add(btnAmstrad);
            flowLayoutPanel1.Controls.Add(Reprocess);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(3, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(877, 34);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(3, 3);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(121, 23);
            comboBox1.TabIndex = 3;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // btnID
            // 
            btnID.Location = new Point(130, 3);
            btnID.Name = "btnID";
            btnID.Size = new Size(75, 23);
            btnID.TabIndex = 1;
            btnID.Text = "Identity";
            btnID.UseVisualStyleBackColor = true;
            btnID.Click += butID;
            // 
            // btnQuantizer
            // 
            btnQuantizer.Location = new Point(211, 3);
            btnQuantizer.Name = "btnQuantizer";
            btnQuantizer.Size = new Size(75, 23);
            btnQuantizer.TabIndex = 4;
            btnQuantizer.Text = "Quantizer";
            btnQuantizer.UseVisualStyleBackColor = true;
            btnQuantizer.Click += btnQuantizer_Click;
            // 
            // btnBkgRem
            // 
            btnBkgRem.Location = new Point(292, 3);
            btnBkgRem.Name = "btnBkgRem";
            btnBkgRem.Size = new Size(75, 23);
            btnBkgRem.TabIndex = 5;
            btnBkgRem.Text = "BKG Rem.";
            btnBkgRem.UseVisualStyleBackColor = true;
            btnBkgRem.Click += btnBkgRem_Click;
            // 
            // btnAmiga
            // 
            btnAmiga.Location = new Point(373, 3);
            btnAmiga.Name = "btnAmiga";
            btnAmiga.Size = new Size(75, 23);
            btnAmiga.TabIndex = 6;
            btnAmiga.Text = "Amiga";
            btnAmiga.UseVisualStyleBackColor = true;
            btnAmiga.Click += btnAmiga_Click;
            // 
            // btnLumSat
            // 
            btnLumSat.Location = new Point(454, 3);
            btnLumSat.Name = "btnLumSat";
            btnLumSat.Size = new Size(75, 23);
            btnLumSat.TabIndex = 7;
            btnLumSat.Text = "LumSat";
            btnLumSat.UseVisualStyleBackColor = true;
            btnLumSat.Click += btnLumSat_Click;
            // 
            // btnC64
            // 
            btnC64.Location = new Point(535, 3);
            btnC64.Name = "btnC64";
            btnC64.Size = new Size(75, 23);
            btnC64.TabIndex = 8;
            btnC64.Text = "C64";
            btnC64.UseVisualStyleBackColor = true;
            btnC64.Click += btnC64_Click;
            // 
            // btnEGA
            // 
            btnEGA.Location = new Point(616, 3);
            btnEGA.Name = "btnEGA";
            btnEGA.Size = new Size(75, 23);
            btnEGA.TabIndex = 9;
            btnEGA.Text = "EGA";
            btnEGA.UseVisualStyleBackColor = true;
            btnEGA.Click += btnEGA_Click;
            // 
            // btnAmstrad
            // 
            btnAmstrad.Location = new Point(697, 3);
            btnAmstrad.Name = "btnAmstrad";
            btnAmstrad.Size = new Size(75, 23);
            btnAmstrad.TabIndex = 10;
            btnAmstrad.Text = "Amstrad";
            btnAmstrad.UseVisualStyleBackColor = true;
            btnAmstrad.Click += btnAmstrad_Click;
            // 
            // Reprocess
            // 
            Reprocess.Location = new Point(778, 3);
            Reprocess.Name = "Reprocess";
            Reprocess.Size = new Size(75, 23);
            Reprocess.TabIndex = 2;
            Reprocess.Text = "Reprocess";
            Reprocess.UseVisualStyleBackColor = true;
            Reprocess.Click += butReprocess;
            // 
            // listBox1
            // 
            listBox1.Dock = DockStyle.Fill;
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(0, 0);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(883, 163);
            listBox1.TabIndex = 0;
            // 
            // bitmapRender1
            // 
            bitmapRender1.Control = null;
            bitmapRender1.Image = null;
            bitmapRender1.ImageBlockScroll = false;
            bitmapRender1.ImageMoveOverControlBorder = false;
            bitmapRender1.ImageZoomManual = 1F;
            bitmapRender1.ImageZoomMode = ColourClashNet.Controls.BitmapRender.EnumZoom.Stretch;
            bitmapRender1.MouseImageFollowing = true;
            bitmapRender1.MouseMovingButton = MouseButtons.Left;
            bitmapRender1.MouseSelectButton = MouseButtons.Right;
            // 
            // bitmapRender2
            // 
            bitmapRender2.Control = null;
            bitmapRender2.Image = null;
            bitmapRender2.ImageBlockScroll = false;
            bitmapRender2.ImageMoveOverControlBorder = false;
            bitmapRender2.ImageZoomManual = 1F;
            bitmapRender2.ImageZoomMode = ColourClashNet.Controls.BitmapRender.EnumZoom.Stretch;
            bitmapRender2.MouseImageFollowing = true;
            bitmapRender2.MouseMovingButton = MouseButtons.Left;
            bitmapRender2.MouseSelectButton = MouseButtons.Right;
            // 
            // FormTester
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(883, 566);
            Controls.Add(splitContainer1);
            Name = "FormTester";
            Text = "Form1";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private ListBox listBox1;
        private ColourClashNet.Controls.BitmapRender bitmapRender1;
        private ColourClashNet.Controls.BitmapRender bitmapRender2;
        private Button btnID;
        private PropertyGrid propertyGrid1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button Reprocess;
        private ComboBox comboBox1;
        private Button btnQuantizer;
        private Button btnBkgRem;
        private Button btnAmiga;
        private Button btnLumSat;
        private Button btnC64;
        private Button btnEGA;
        private Button btnAmstrad;
    }
}
