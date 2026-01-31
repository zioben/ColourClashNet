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
            tableLayoutPanel3 = new TableLayoutPanel();
            btnSave = new Button();
            btnReprocess = new Button();
            btnEGA = new Button();
            btnCGA = new Button();
            btnPalette = new Button();
            btnSpeccy = new Button();
            btnC64 = new Button();
            btnAmiga = new Button();
            btnAmstrad = new Button();
            btnCluster = new Button();
            btnMedian = new Button();
            btnLumSat = new Button();
            btnFast = new Button();
            btnQuantizer = new Button();
            btnID = new Button();
            btnBkgRem = new Button();
            numDitheringStrenght = new NumericUpDown();
            cbDithering = new ComboBox();
            cbColorMode = new ComboBox();
            btnLoad = new Button();
            cbPreset = new ComboBox();
            listBox1 = new ListBox();
            bitmapRenderIN = new ColourClashNet.Components.BitmapRender(components);
            bitmapRenderOUT = new ColourClashNet.Components.BitmapRender(components);
            openLoadImage = new OpenFileDialog();
            timerLog = new System.Windows.Forms.Timer(components);
            saveImageDialog = new SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numDitheringStrenght).BeginInit();
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
            splitContainer1.Size = new Size(1164, 594);
            splitContainer1.SplitterDistance = 418;
            splitContainer1.TabIndex = 2;
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
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(1164, 418);
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
            tableLayoutPanel2.Location = new Point(3, 67);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(1158, 348);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Black;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(3, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(423, 342);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Black;
            pictureBox2.Dock = DockStyle.Fill;
            pictureBox2.Location = new Point(432, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(423, 342);
            pictureBox2.TabIndex = 1;
            pictureBox2.TabStop = false;
            // 
            // propertyGrid1
            // 
            propertyGrid1.Dock = DockStyle.Fill;
            propertyGrid1.Location = new Point(861, 3);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.Size = new Size(294, 342);
            propertyGrid1.TabIndex = 2;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 13;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.69230747F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.69230747F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.69230747F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.69230747F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.69230747F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.69230747F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.69230747F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.69230747F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.69230747F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.69230747F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.69230747F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.69230747F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.69230747F));
            tableLayoutPanel3.Controls.Add(btnSave, 1, 1);
            tableLayoutPanel3.Controls.Add(btnReprocess, 12, 1);
            tableLayoutPanel3.Controls.Add(btnEGA, 11, 1);
            tableLayoutPanel3.Controls.Add(btnCGA, 11, 0);
            tableLayoutPanel3.Controls.Add(btnPalette, 10, 0);
            tableLayoutPanel3.Controls.Add(btnSpeccy, 9, 1);
            tableLayoutPanel3.Controls.Add(btnC64, 8, 1);
            tableLayoutPanel3.Controls.Add(btnAmiga, 8, 0);
            tableLayoutPanel3.Controls.Add(btnAmstrad, 9, 0);
            tableLayoutPanel3.Controls.Add(btnCluster, 7, 0);
            tableLayoutPanel3.Controls.Add(btnMedian, 7, 1);
            tableLayoutPanel3.Controls.Add(btnLumSat, 6, 0);
            tableLayoutPanel3.Controls.Add(btnFast, 6, 1);
            tableLayoutPanel3.Controls.Add(btnQuantizer, 5, 0);
            tableLayoutPanel3.Controls.Add(btnID, 4, 0);
            tableLayoutPanel3.Controls.Add(btnBkgRem, 4, 1);
            tableLayoutPanel3.Controls.Add(numDitheringStrenght, 3, 1);
            tableLayoutPanel3.Controls.Add(cbDithering, 3, 0);
            tableLayoutPanel3.Controls.Add(cbColorMode, 2, 0);
            tableLayoutPanel3.Controls.Add(btnLoad, 1, 0);
            tableLayoutPanel3.Controls.Add(cbPreset, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Size = new Size(1158, 58);
            tableLayoutPanel3.TabIndex = 11;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSave.Location = new Point(92, 32);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(83, 23);
            btnSave.TabIndex = 22;
            btnSave.Text = "SAVE";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnReprocess
            // 
            btnReprocess.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnReprocess.Location = new Point(1071, 32);
            btnReprocess.Name = "btnReprocess";
            btnReprocess.Size = new Size(84, 23);
            btnReprocess.TabIndex = 2;
            btnReprocess.Text = "Reprocess";
            btnReprocess.UseVisualStyleBackColor = true;
            btnReprocess.Click += butReprocess;
            // 
            // btnEGA
            // 
            btnEGA.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnEGA.Location = new Point(982, 32);
            btnEGA.Name = "btnEGA";
            btnEGA.Size = new Size(83, 23);
            btnEGA.TabIndex = 9;
            btnEGA.Text = "EGA";
            btnEGA.UseVisualStyleBackColor = true;
            btnEGA.Click += btnEGA_Click;
            // 
            // btnCGA
            // 
            btnCGA.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnCGA.Location = new Point(982, 3);
            btnCGA.Name = "btnCGA";
            btnCGA.Size = new Size(83, 23);
            btnCGA.TabIndex = 12;
            btnCGA.Text = "CGA";
            btnCGA.UseVisualStyleBackColor = true;
            btnCGA.Click += btnCGA_Click;
            // 
            // btnPalette
            // 
            btnPalette.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnPalette.Location = new Point(893, 3);
            btnPalette.Name = "btnPalette";
            btnPalette.Size = new Size(83, 23);
            btnPalette.TabIndex = 17;
            btnPalette.Text = "Fixed PAL";
            btnPalette.UseVisualStyleBackColor = true;
            btnPalette.Click += btnPalette_Click;
            // 
            // btnSpeccy
            // 
            btnSpeccy.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSpeccy.Location = new Point(804, 32);
            btnSpeccy.Name = "btnSpeccy";
            btnSpeccy.Size = new Size(83, 23);
            btnSpeccy.TabIndex = 11;
            btnSpeccy.Text = "Spectrum";
            btnSpeccy.UseVisualStyleBackColor = true;
            btnSpeccy.Click += btnSpeccy_Click;
            // 
            // btnC64
            // 
            btnC64.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnC64.Location = new Point(715, 32);
            btnC64.Name = "btnC64";
            btnC64.Size = new Size(83, 23);
            btnC64.TabIndex = 8;
            btnC64.Text = "C64";
            btnC64.UseVisualStyleBackColor = true;
            btnC64.Click += btnC64_Click;
            // 
            // btnAmiga
            // 
            btnAmiga.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnAmiga.Location = new Point(715, 3);
            btnAmiga.Name = "btnAmiga";
            btnAmiga.Size = new Size(83, 23);
            btnAmiga.TabIndex = 6;
            btnAmiga.Text = "Amiga";
            btnAmiga.UseVisualStyleBackColor = true;
            btnAmiga.Click += btnAmiga_Click;
            // 
            // btnAmstrad
            // 
            btnAmstrad.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnAmstrad.Location = new Point(804, 3);
            btnAmstrad.Name = "btnAmstrad";
            btnAmstrad.Size = new Size(83, 23);
            btnAmstrad.TabIndex = 10;
            btnAmstrad.Text = "Amstrad";
            btnAmstrad.UseVisualStyleBackColor = true;
            btnAmstrad.Click += btnAmstrad_Click;
            // 
            // btnCluster
            // 
            btnCluster.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnCluster.Location = new Point(626, 3);
            btnCluster.Name = "btnCluster";
            btnCluster.Size = new Size(83, 23);
            btnCluster.TabIndex = 14;
            btnCluster.Text = "Clustering";
            btnCluster.UseVisualStyleBackColor = true;
            btnCluster.Click += btnCluster_Click;
            // 
            // btnMedian
            // 
            btnMedian.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnMedian.Location = new Point(626, 32);
            btnMedian.Name = "btnMedian";
            btnMedian.Size = new Size(83, 23);
            btnMedian.TabIndex = 15;
            btnMedian.Text = "Median";
            btnMedian.UseVisualStyleBackColor = true;
            btnMedian.Click += btnMedian_Click;
            // 
            // btnLumSat
            // 
            btnLumSat.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnLumSat.Location = new Point(537, 3);
            btnLumSat.Name = "btnLumSat";
            btnLumSat.Size = new Size(83, 23);
            btnLumSat.TabIndex = 7;
            btnLumSat.Text = "LumSat";
            btnLumSat.UseVisualStyleBackColor = true;
            btnLumSat.Click += btnLumSat_Click;
            // 
            // btnFast
            // 
            btnFast.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnFast.Location = new Point(537, 32);
            btnFast.Name = "btnFast";
            btnFast.Size = new Size(83, 23);
            btnFast.TabIndex = 16;
            btnFast.Text = "Fast";
            btnFast.UseVisualStyleBackColor = true;
            btnFast.Click += btnFast_Click;
            // 
            // btnQuantizer
            // 
            btnQuantizer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnQuantizer.Location = new Point(448, 3);
            btnQuantizer.Name = "btnQuantizer";
            btnQuantizer.Size = new Size(83, 23);
            btnQuantizer.TabIndex = 4;
            btnQuantizer.Text = "Quantizer";
            btnQuantizer.UseVisualStyleBackColor = true;
            btnQuantizer.Click += btnQuantizer_Click;
            // 
            // btnID
            // 
            btnID.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnID.Location = new Point(359, 3);
            btnID.Name = "btnID";
            btnID.Size = new Size(83, 23);
            btnID.TabIndex = 1;
            btnID.Text = "Identity";
            btnID.UseVisualStyleBackColor = true;
            btnID.Click += butID;
            // 
            // btnBkgRem
            // 
            btnBkgRem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnBkgRem.Location = new Point(359, 32);
            btnBkgRem.Name = "btnBkgRem";
            btnBkgRem.Size = new Size(83, 23);
            btnBkgRem.TabIndex = 5;
            btnBkgRem.Text = "BKG Rem.";
            btnBkgRem.UseVisualStyleBackColor = true;
            btnBkgRem.Click += btnBkgRem_Click;
            // 
            // numDitheringStrenght
            // 
            numDitheringStrenght.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            numDitheringStrenght.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numDitheringStrenght.Location = new Point(270, 32);
            numDitheringStrenght.Name = "numDitheringStrenght";
            numDitheringStrenght.Size = new Size(83, 23);
            numDitheringStrenght.TabIndex = 21;
            numDitheringStrenght.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // cbDithering
            // 
            cbDithering.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cbDithering.FormattingEnabled = true;
            cbDithering.Location = new Point(270, 3);
            cbDithering.Name = "cbDithering";
            cbDithering.Size = new Size(83, 23);
            cbDithering.TabIndex = 13;
            // 
            // cbColorMode
            // 
            cbColorMode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cbColorMode.FormattingEnabled = true;
            cbColorMode.Location = new Point(181, 3);
            cbColorMode.Name = "cbColorMode";
            cbColorMode.Size = new Size(83, 23);
            cbColorMode.TabIndex = 19;
            // 
            // btnLoad
            // 
            btnLoad.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnLoad.Location = new Point(92, 3);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(83, 23);
            btnLoad.TabIndex = 18;
            btnLoad.Text = "LOAD";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // cbPreset
            // 
            cbPreset.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cbPreset.FormattingEnabled = true;
            cbPreset.Location = new Point(3, 3);
            cbPreset.Name = "cbPreset";
            cbPreset.Size = new Size(83, 23);
            cbPreset.TabIndex = 3;
            cbPreset.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // listBox1
            // 
            listBox1.Dock = DockStyle.Fill;
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(0, 0);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(1164, 172);
            listBox1.TabIndex = 0;
            // 
            // bitmapRenderIN
            // 
            bitmapRenderIN.Control = null;
            bitmapRenderIN.Image = null;
            bitmapRenderIN.ImageBlockScroll = false;
            bitmapRenderIN.ImageMoveOverControlBorder = false;
            bitmapRenderIN.ImageZoomManual = 1F;
            bitmapRenderIN.ImageZoomMode = ColourClashNet.Components.BitmapRender.EnumZoom.Stretch;
            bitmapRenderIN.MouseImageFollowing = true;
            bitmapRenderIN.MouseMovingButton = MouseButtons.Left;
            bitmapRenderIN.MouseSelectButton = MouseButtons.Right;
            // 
            // bitmapRenderOUT
            // 
            bitmapRenderOUT.Control = null;
            bitmapRenderOUT.Image = null;
            bitmapRenderOUT.ImageBlockScroll = false;
            bitmapRenderOUT.ImageMoveOverControlBorder = false;
            bitmapRenderOUT.ImageZoomManual = 1F;
            bitmapRenderOUT.ImageZoomMode = ColourClashNet.Components.BitmapRender.EnumZoom.Stretch;
            bitmapRenderOUT.MouseImageFollowing = true;
            bitmapRenderOUT.MouseMovingButton = MouseButtons.Left;
            bitmapRenderOUT.MouseSelectButton = MouseButtons.Right;
            // 
            // timerLog
            // 
            timerLog.Enabled = true;
            timerLog.Interval = 500;
            timerLog.Tick += oTimerLog_Tick;
            // 
            // FormTester
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1164, 594);
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
            tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numDitheringStrenght).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private ListBox listBox1;
        private ColourClashNet.Components.BitmapRender bitmapRenderIN;
        private ColourClashNet.Components.BitmapRender bitmapRenderOUT;
        private Button btnID;
        private PropertyGrid propertyGrid1;
        private Button btnReprocess;
        private ComboBox cbPreset;
        private Button btnQuantizer;
        private Button btnBkgRem;
        private Button btnAmiga;
        private Button btnLumSat;
        private Button btnC64;
        private Button btnEGA;
        private Button btnAmstrad;
        private TableLayoutPanel tableLayoutPanel3;
        private ComboBox cbDithering;
        private Button btnSpeccy;
        private Button btnCGA;
        private Button btnCluster;
        private Button btnMedian;
        private Button btnFast;
        private Button btnPalette;
        private Button btnLoad;
        private OpenFileDialog openLoadImage;
        private ComboBox cbColorMode;
        private System.Windows.Forms.Timer timerLog;
        private NumericUpDown numDitheringStrenght;
        private Button btnSave;
        private SaveFileDialog saveImageDialog;
    }
}
