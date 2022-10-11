﻿namespace ColourClashNet
{
    partial class ColorAnalyzer
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColorAnalyzer));
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bitplaneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rGB3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rGB4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rGB5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hiColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbImage = new System.Windows.Forms.ComboBox();
            this.btnReduceColors = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.btnReduceColorCluster = new System.Windows.Forms.Button();
            this.nudClusterLoop = new System.Windows.Forms.NumericUpDown();
            this.btnReduceColorsScanline = new System.Windows.Forms.Button();
            this.pbBkColor = new System.Windows.Forms.PictureBox();
            this.chkScanLineCluster = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.oBitmapRender = new ColourClashNet.Controls.BitmapRender(this.components);
            this.oColorTransformer = new ColourClashNet.Controls.ColorTransformer(this.components);
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudClusterLoop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBkColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Bmp|*.bmp|Png|*.png|Jpg|*.jpg|All Files|*.*";
            this.openFileDialog1.FilterIndex = 2;
            this.openFileDialog1.Title = "Load Image";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.colorModeToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1021, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.exportToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
            this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.newToolStripMenuItem.Text = "&New";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.imageToolStripMenuItem,
            this.bitplaneToolStripMenuItem,
            this.sourceToolStripMenuItem});
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.exportToolStripMenuItem.Text = "Export";
            // 
            // imageToolStripMenuItem
            // 
            this.imageToolStripMenuItem.Name = "imageToolStripMenuItem";
            this.imageToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.imageToolStripMenuItem.Text = "Image";
            // 
            // bitplaneToolStripMenuItem
            // 
            this.bitplaneToolStripMenuItem.Name = "bitplaneToolStripMenuItem";
            this.bitplaneToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.bitplaneToolStripMenuItem.Text = "Bitplane";
            // 
            // sourceToolStripMenuItem
            // 
            this.sourceToolStripMenuItem.Name = "sourceToolStripMenuItem";
            this.sourceToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.sourceToolStripMenuItem.Text = "Source Code";
            // 
            // colorModeToolStripMenuItem
            // 
            this.colorModeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fullColorToolStripMenuItem,
            this.rGB3ToolStripMenuItem,
            this.rGB4ToolStripMenuItem,
            this.rGB5ToolStripMenuItem,
            this.hiColorToolStripMenuItem});
            this.colorModeToolStripMenuItem.Name = "colorModeToolStripMenuItem";
            this.colorModeToolStripMenuItem.Size = new System.Drawing.Size(79, 20);
            this.colorModeToolStripMenuItem.Tag = "FullColor";
            this.colorModeToolStripMenuItem.Text = "ColorMode";
            // 
            // fullColorToolStripMenuItem
            // 
            this.fullColorToolStripMenuItem.Checked = true;
            this.fullColorToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fullColorToolStripMenuItem.Name = "fullColorToolStripMenuItem";
            this.fullColorToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.fullColorToolStripMenuItem.Tag = "FullColor";
            this.fullColorToolStripMenuItem.Text = "FullColor";
            this.fullColorToolStripMenuItem.Click += new System.EventHandler(this.ResetTsColor);
            // 
            // rGB3ToolStripMenuItem
            // 
            this.rGB3ToolStripMenuItem.Name = "rGB3ToolStripMenuItem";
            this.rGB3ToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.rGB3ToolStripMenuItem.Tag = "RGB3";
            this.rGB3ToolStripMenuItem.Text = "RGB333";
            this.rGB3ToolStripMenuItem.Click += new System.EventHandler(this.ResetTsColor);
            // 
            // rGB4ToolStripMenuItem
            // 
            this.rGB4ToolStripMenuItem.Name = "rGB4ToolStripMenuItem";
            this.rGB4ToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.rGB4ToolStripMenuItem.Tag = "RGB4";
            this.rGB4ToolStripMenuItem.Text = "RGB444";
            this.rGB4ToolStripMenuItem.Click += new System.EventHandler(this.ResetTsColor);
            // 
            // rGB5ToolStripMenuItem
            // 
            this.rGB5ToolStripMenuItem.Name = "rGB5ToolStripMenuItem";
            this.rGB5ToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.rGB5ToolStripMenuItem.Tag = "RGB5";
            this.rGB5ToolStripMenuItem.Text = "RGB555";
            this.rGB5ToolStripMenuItem.Click += new System.EventHandler(this.ResetTsColor);
            // 
            // hiColorToolStripMenuItem
            // 
            this.hiColorToolStripMenuItem.Name = "hiColorToolStripMenuItem";
            this.hiColorToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.hiColorToolStripMenuItem.Text = "HiColor";
            this.hiColorToolStripMenuItem.Click += new System.EventHandler(this.ResetTsColor);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pictureBox1);
            this.splitContainer1.Size = new System.Drawing.Size(1021, 646);
            this.splitContainer1.SplitterDistance = 355;
            this.splitContainer1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.propertyGrid1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 283F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(355, 646);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(3, 286);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(349, 357);
            this.propertyGrid1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.cbImage, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnReduceColors, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.numericUpDown1, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnReduceColorCluster, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.nudClusterLoop, 1, 5);
            this.tableLayoutPanel2.Controls.Add(this.btnReduceColorsScanline, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.pbBkColor, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.chkScanLineCluster, 1, 4);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 8;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(349, 277);
            this.tableLayoutPanel2.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 15);
            this.label3.TabIndex = 11;
            this.label3.Text = "Background Color";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 15);
            this.label2.TabIndex = 10;
            this.label2.Text = "Color Wanted";
            // 
            // cbImage
            // 
            this.cbImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbImage.FormattingEnabled = true;
            this.cbImage.Items.AddRange(new object[] {
            "Original Image",
            "Quantized Image",
            "Color Reduced Image"});
            this.cbImage.Location = new System.Drawing.Point(177, 3);
            this.cbImage.Name = "cbImage";
            this.cbImage.Size = new System.Drawing.Size(169, 23);
            this.cbImage.TabIndex = 7;
            this.cbImage.SelectedIndexChanged += new System.EventHandler(this.cbImage_SelectedIndexChanged);
            // 
            // btnReduceColors
            // 
            this.btnReduceColors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReduceColors.Location = new System.Drawing.Point(3, 105);
            this.btnReduceColors.Name = "btnReduceColors";
            this.btnReduceColors.Size = new System.Drawing.Size(168, 28);
            this.btnReduceColors.TabIndex = 3;
            this.btnReduceColors.Text = "Quantitative color reduction";
            this.btnReduceColors.UseVisualStyleBackColor = true;
            this.btnReduceColors.Click += new System.EventHandler(this.btnReduceColors_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDown1.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDown1.Location = new System.Drawing.Point(177, 37);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(169, 23);
            this.numericUpDown1.TabIndex = 2;
            this.numericUpDown1.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 15);
            this.label1.TabIndex = 8;
            this.label1.Text = "Show Image";
            // 
            // btnReduceColorCluster
            // 
            this.btnReduceColorCluster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReduceColorCluster.Location = new System.Drawing.Point(3, 173);
            this.btnReduceColorCluster.Name = "btnReduceColorCluster";
            this.btnReduceColorCluster.Size = new System.Drawing.Size(168, 28);
            this.btnReduceColorCluster.TabIndex = 5;
            this.btnReduceColorCluster.Text = "Clustering color reduction";
            this.btnReduceColorCluster.UseVisualStyleBackColor = true;
            this.btnReduceColorCluster.Click += new System.EventHandler(this.btnReduceColorCluster_Click);
            // 
            // nudClusterLoop
            // 
            this.nudClusterLoop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudClusterLoop.Location = new System.Drawing.Point(177, 173);
            this.nudClusterLoop.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudClusterLoop.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudClusterLoop.Name = "nudClusterLoop";
            this.nudClusterLoop.Size = new System.Drawing.Size(169, 23);
            this.nudClusterLoop.TabIndex = 6;
            this.nudClusterLoop.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // btnReduceColorsScanline
            // 
            this.btnReduceColorsScanline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReduceColorsScanline.Location = new System.Drawing.Point(3, 139);
            this.btnReduceColorsScanline.Name = "btnReduceColorsScanline";
            this.btnReduceColorsScanline.Size = new System.Drawing.Size(168, 28);
            this.btnReduceColorsScanline.TabIndex = 9;
            this.btnReduceColorsScanline.Text = "Scanline color reduction";
            this.btnReduceColorsScanline.UseVisualStyleBackColor = true;
            this.btnReduceColorsScanline.Click += new System.EventHandler(this.btnReduceColorsScanline_Click);
            // 
            // pbBkColor
            // 
            this.pbBkColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbBkColor.Location = new System.Drawing.Point(177, 71);
            this.pbBkColor.Name = "pbBkColor";
            this.pbBkColor.Size = new System.Drawing.Size(169, 28);
            this.pbBkColor.TabIndex = 12;
            this.pbBkColor.TabStop = false;
            this.pbBkColor.DoubleClick += new System.EventHandler(this.pbBkColor_DoubleClick);
            // 
            // chkScanLineCluster
            // 
            this.chkScanLineCluster.AutoSize = true;
            this.chkScanLineCluster.Location = new System.Drawing.Point(177, 139);
            this.chkScanLineCluster.Name = "chkScanLineCluster";
            this.chkScanLineCluster.Size = new System.Drawing.Size(127, 19);
            this.chkScanLineCluster.TabIndex = 13;
            this.chkScanLineCluster.Text = "Clustering Scanline";
            this.chkScanLineCluster.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Black;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(662, 646);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseClick);
            // 
            // oBitmapRender
            // 
            this.oBitmapRender.Control = this.pictureBox1;
            this.oBitmapRender.Image = null;
            this.oBitmapRender.ImageBlockScroll = false;
            this.oBitmapRender.ImageMoveOverControlBorder = false;
            this.oBitmapRender.ImageZoomManual = 1F;
            this.oBitmapRender.ImageZoomMode = ColourClashNet.Controls.BitmapRender.EnumZoom.ZoomQ;
            this.oBitmapRender.MouseMovingButton = System.Windows.Forms.MouseButtons.None;
            // 
            // ColorAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "ColorAnalyzer";
            this.Size = new System.Drawing.Size(1021, 670);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudClusterLoop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBkColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OpenFileDialog openFileDialog1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private SplitContainer splitContainer1;
        private PictureBox pictureBox1;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem imageToolStripMenuItem;
        private ToolStripMenuItem bitplaneToolStripMenuItem;
        private ToolStripMenuItem sourceToolStripMenuItem;
        private ToolStripMenuItem colorModeToolStripMenuItem;
        private ToolStripMenuItem fullColorToolStripMenuItem;
        private ToolStripMenuItem rGB3ToolStripMenuItem;
        private ToolStripMenuItem rGB4ToolStripMenuItem;
        private ToolStripMenuItem rGB5ToolStripMenuItem;
        private TableLayoutPanel tableLayoutPanel1;
        private PropertyGrid propertyGrid1;
        private TableLayoutPanel tableLayoutPanel2;
        private Label label2;
        private ComboBox cbImage;
        private Button btnReduceColors;
        private NumericUpDown numericUpDown1;
        private Label label1;
        private Button btnReduceColorCluster;
        private NumericUpDown nudClusterLoop;
        private Button btnReduceColorsScanline;
        private Label label3;
        private PictureBox pbBkColor;
        private ToolStripMenuItem hiColorToolStripMenuItem;
        private Controls.BitmapRender oBitmapRender;
        private Controls.ColorTransformer oColorTransformer;
        private CheckBox chkScanLineCluster;
    }
}
