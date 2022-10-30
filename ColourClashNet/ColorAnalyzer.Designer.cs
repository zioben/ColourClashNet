namespace ColourClashNet
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
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bitmapIndexedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.indexedPNGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bitmapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pNGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.amigaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iFFILBMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bitplaneToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.rawBitplaneAndOCSCopperlistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sourceCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rGB3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rGB4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rGB5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hiColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorDistanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rGBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aLLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.button1 = new System.Windows.Forms.Button();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.pictureBoxSrc = new System.Windows.Forms.PictureBox();
            this.pictureBoxProc = new System.Windows.Forms.PictureBox();
            this.oBitmapRenderSource = new ColourClashNet.Controls.BitmapRender(this.components);
            this.bitmapRender1 = new ColourClashNet.Controls.BitmapRender(this.components);
            this.bitmapRender2 = new ColourClashNet.Controls.BitmapRender(this.components);
            this.oBitmapRenderDest = new ColourClashNet.Controls.BitmapRender(this.components);
            this.sfdExportImage = new System.Windows.Forms.SaveFileDialog();
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
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSrc)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProc)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Bmp|*.bmp|Png|*.png|Jpg|*.jpg|All Files|*.*";
            this.openFileDialog1.FilterIndex = 4;
            this.openFileDialog1.Title = "Load Image";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.colorModeToolStripMenuItem,
            this.colorDistanceToolStripMenuItem});
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
            this.toolStripMenuItem1,
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
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
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
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItem1.Image")));
            this.toolStripMenuItem1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.toolStripMenuItem1.Size = new System.Drawing.Size(146, 22);
            this.toolStripMenuItem1.Text = "&Save";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bitmapIndexedToolStripMenuItem,
            this.indexedPNGToolStripMenuItem,
            this.bitmapToolStripMenuItem,
            this.pNGToolStripMenuItem,
            this.amigaToolStripMenuItem});
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.exportToolStripMenuItem.Text = "Export";
            // 
            // bitmapIndexedToolStripMenuItem
            // 
            this.bitmapIndexedToolStripMenuItem.Name = "bitmapIndexedToolStripMenuItem";
            this.bitmapIndexedToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.bitmapIndexedToolStripMenuItem.Text = "Bitmap Indexed";
            this.bitmapIndexedToolStripMenuItem.Click += new System.EventHandler(this.bitmapIndexedToolStripMenuItem_Click);
            // 
            // indexedPNGToolStripMenuItem
            // 
            this.indexedPNGToolStripMenuItem.Name = "indexedPNGToolStripMenuItem";
            this.indexedPNGToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.indexedPNGToolStripMenuItem.Text = "PNG Indexed";
            // 
            // bitmapToolStripMenuItem
            // 
            this.bitmapToolStripMenuItem.Name = "bitmapToolStripMenuItem";
            this.bitmapToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.bitmapToolStripMenuItem.Text = "Bitmap";
            this.bitmapToolStripMenuItem.Click += new System.EventHandler(this.bitmapToolStripMenuItem_Click);
            // 
            // pNGToolStripMenuItem
            // 
            this.pNGToolStripMenuItem.Name = "pNGToolStripMenuItem";
            this.pNGToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.pNGToolStripMenuItem.Text = "PNG";
            this.pNGToolStripMenuItem.Click += new System.EventHandler(this.pNGToolStripMenuItem_Click);
            // 
            // amigaToolStripMenuItem
            // 
            this.amigaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.iFFILBMToolStripMenuItem,
            this.bitplaneToolStripMenuItem1,
            this.rawBitplaneAndOCSCopperlistToolStripMenuItem,
            this.sourceCodeToolStripMenuItem});
            this.amigaToolStripMenuItem.Name = "amigaToolStripMenuItem";
            this.amigaToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.amigaToolStripMenuItem.Text = "Amiga";
            // 
            // iFFILBMToolStripMenuItem
            // 
            this.iFFILBMToolStripMenuItem.Name = "iFFILBMToolStripMenuItem";
            this.iFFILBMToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.iFFILBMToolStripMenuItem.Text = "IFF-ILBM";
            // 
            // bitplaneToolStripMenuItem1
            // 
            this.bitplaneToolStripMenuItem1.Name = "bitplaneToolStripMenuItem1";
            this.bitplaneToolStripMenuItem1.Size = new System.Drawing.Size(224, 22);
            this.bitplaneToolStripMenuItem1.Text = "Raw Bitplane";
            this.bitplaneToolStripMenuItem1.Click += new System.EventHandler(this.bitplaneToolStripMenuItem1_Click);
            // 
            // rawBitplaneAndOCSCopperlistToolStripMenuItem
            // 
            this.rawBitplaneAndOCSCopperlistToolStripMenuItem.Name = "rawBitplaneAndOCSCopperlistToolStripMenuItem";
            this.rawBitplaneAndOCSCopperlistToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.rawBitplaneAndOCSCopperlistToolStripMenuItem.Text = "Raw Bitplane And Copperlist";
            // 
            // sourceCodeToolStripMenuItem
            // 
            this.sourceCodeToolStripMenuItem.Name = "sourceCodeToolStripMenuItem";
            this.sourceCodeToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.sourceCodeToolStripMenuItem.Text = "SourceCode";
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
            // colorDistanceToolStripMenuItem
            // 
            this.colorDistanceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rGBToolStripMenuItem,
            this.hSVToolStripMenuItem,
            this.aLLToolStripMenuItem});
            this.colorDistanceToolStripMenuItem.Name = "colorDistanceToolStripMenuItem";
            this.colorDistanceToolStripMenuItem.Size = new System.Drawing.Size(93, 20);
            this.colorDistanceToolStripMenuItem.Text = "ColorDistance";
            // 
            // rGBToolStripMenuItem
            // 
            this.rGBToolStripMenuItem.Checked = true;
            this.rGBToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rGBToolStripMenuItem.Name = "rGBToolStripMenuItem";
            this.rGBToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.rGBToolStripMenuItem.Text = "RGB";
            this.rGBToolStripMenuItem.Click += new System.EventHandler(this.ResetTsColorDistance);
            // 
            // hSVToolStripMenuItem
            // 
            this.hSVToolStripMenuItem.Name = "hSVToolStripMenuItem";
            this.hSVToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.hSVToolStripMenuItem.Text = "HSV";
            this.hSVToolStripMenuItem.Click += new System.EventHandler(this.ResetTsColorDistance);
            // 
            // aLLToolStripMenuItem
            // 
            this.aLLToolStripMenuItem.Name = "aLLToolStripMenuItem";
            this.aLLToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.aLLToolStripMenuItem.Text = "ALL";
            this.aLLToolStripMenuItem.Click += new System.EventHandler(this.ResetTsColorDistance);
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
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1021, 646);
            this.splitContainer1.SplitterDistance = 277;
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(277, 646);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(3, 286);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(271, 357);
            this.propertyGrid1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65.34954F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34.65046F));
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
            this.tableLayoutPanel2.Controls.Add(this.button1, 0, 6);
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(271, 277);
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
            this.cbImage.Location = new System.Drawing.Point(180, 3);
            this.cbImage.Name = "cbImage";
            this.cbImage.Size = new System.Drawing.Size(88, 23);
            this.cbImage.TabIndex = 7;
            this.cbImage.SelectedIndexChanged += new System.EventHandler(this.cbImage_SelectedIndexChanged);
            // 
            // btnReduceColors
            // 
            this.btnReduceColors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReduceColors.Location = new System.Drawing.Point(3, 105);
            this.btnReduceColors.Name = "btnReduceColors";
            this.btnReduceColors.Size = new System.Drawing.Size(171, 28);
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
            this.numericUpDown1.Location = new System.Drawing.Point(180, 37);
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
            this.numericUpDown1.Size = new System.Drawing.Size(88, 23);
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
            this.btnReduceColorCluster.Size = new System.Drawing.Size(171, 28);
            this.btnReduceColorCluster.TabIndex = 5;
            this.btnReduceColorCluster.Text = "Clustering color reduction";
            this.btnReduceColorCluster.UseVisualStyleBackColor = true;
            this.btnReduceColorCluster.Click += new System.EventHandler(this.btnReduceColorCluster_Click);
            // 
            // nudClusterLoop
            // 
            this.nudClusterLoop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudClusterLoop.Location = new System.Drawing.Point(180, 173);
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
            this.nudClusterLoop.Size = new System.Drawing.Size(88, 23);
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
            this.btnReduceColorsScanline.Size = new System.Drawing.Size(171, 28);
            this.btnReduceColorsScanline.TabIndex = 9;
            this.btnReduceColorsScanline.Text = "Scanline color reduction";
            this.btnReduceColorsScanline.UseVisualStyleBackColor = true;
            this.btnReduceColorsScanline.Click += new System.EventHandler(this.btnReduceColorsScanline_Click);
            // 
            // pbBkColor
            // 
            this.pbBkColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbBkColor.Location = new System.Drawing.Point(180, 71);
            this.pbBkColor.Name = "pbBkColor";
            this.pbBkColor.Size = new System.Drawing.Size(88, 28);
            this.pbBkColor.TabIndex = 12;
            this.pbBkColor.TabStop = false;
            this.pbBkColor.DoubleClick += new System.EventHandler(this.pbBkColor_DoubleClick);
            // 
            // chkScanLineCluster
            // 
            this.chkScanLineCluster.AutoSize = true;
            this.chkScanLineCluster.Location = new System.Drawing.Point(180, 139);
            this.chkScanLineCluster.Name = "chkScanLineCluster";
            this.chkScanLineCluster.Size = new System.Drawing.Size(88, 19);
            this.chkScanLineCluster.TabIndex = 13;
            this.chkScanLineCluster.Text = "Clustering Scanline";
            this.chkScanLineCluster.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 207);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 14;
            this.button1.Text = "To ZX";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.pictureBoxSrc);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.pictureBoxProc);
            this.splitContainer2.Size = new System.Drawing.Size(740, 646);
            this.splitContainer2.SplitterDistance = 336;
            this.splitContainer2.TabIndex = 0;
            // 
            // pictureBoxSrc
            // 
            this.pictureBoxSrc.BackColor = System.Drawing.Color.Black;
            this.pictureBoxSrc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxSrc.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxSrc.Name = "pictureBoxSrc";
            this.pictureBoxSrc.Size = new System.Drawing.Size(740, 336);
            this.pictureBoxSrc.TabIndex = 0;
            this.pictureBoxSrc.TabStop = false;
            this.pictureBoxSrc.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDoubleClick);
            // 
            // pictureBoxProc
            // 
            this.pictureBoxProc.BackColor = System.Drawing.Color.Black;
            this.pictureBoxProc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxProc.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxProc.Name = "pictureBoxProc";
            this.pictureBoxProc.Size = new System.Drawing.Size(740, 306);
            this.pictureBoxProc.TabIndex = 0;
            this.pictureBoxProc.TabStop = false;
            // 
            // oBitmapRenderSource
            // 
            this.oBitmapRenderSource.Control = this.pictureBoxSrc;
            this.oBitmapRenderSource.Image = null;
            this.oBitmapRenderSource.ImageBlockScroll = false;
            this.oBitmapRenderSource.ImageMoveOverControlBorder = true;
            this.oBitmapRenderSource.ImageZoomManual = 1F;
            this.oBitmapRenderSource.ImageZoomMode = ColourClashNet.Controls.BitmapRender.EnumZoom.Zoom1;
            this.oBitmapRenderSource.MouseMovingButton = System.Windows.Forms.MouseButtons.Left;
            // 
            // bitmapRender1
            // 
            this.bitmapRender1.Control = this.pictureBoxSrc;
            this.bitmapRender1.Image = null;
            this.bitmapRender1.ImageBlockScroll = false;
            this.bitmapRender1.ImageMoveOverControlBorder = true;
            this.bitmapRender1.ImageZoomManual = 1F;
            this.bitmapRender1.ImageZoomMode = ColourClashNet.Controls.BitmapRender.EnumZoom.Zoom1;
            this.bitmapRender1.MouseMovingButton = System.Windows.Forms.MouseButtons.Left;
            // 
            // bitmapRender2
            // 
            this.bitmapRender2.Control = this.pictureBoxSrc;
            this.bitmapRender2.Image = null;
            this.bitmapRender2.ImageBlockScroll = false;
            this.bitmapRender2.ImageMoveOverControlBorder = true;
            this.bitmapRender2.ImageZoomManual = 1F;
            this.bitmapRender2.ImageZoomMode = ColourClashNet.Controls.BitmapRender.EnumZoom.Zoom1;
            this.bitmapRender2.MouseMovingButton = System.Windows.Forms.MouseButtons.Left;
            // 
            // oBitmapRenderDest
            // 
            this.oBitmapRenderDest.Control = this.pictureBoxProc;
            this.oBitmapRenderDest.Image = null;
            this.oBitmapRenderDest.ImageBlockScroll = false;
            this.oBitmapRenderDest.ImageMoveOverControlBorder = true;
            this.oBitmapRenderDest.ImageZoomManual = 1F;
            this.oBitmapRenderDest.ImageZoomMode = ColourClashNet.Controls.BitmapRender.EnumZoom.Zoom1;
            this.oBitmapRenderDest.MouseMovingButton = System.Windows.Forms.MouseButtons.Left;
            // 
            // sfdExportImage
            // 
            this.sfdExportImage.AddExtension = false;
            // 
            // oColorTransformer
            // 
            this.oColorTransformer.BackgroundColorList = ((System.Collections.Generic.List<int>)(resources.GetObject("oColorTransformer.BackgroundColorList")));
            this.oColorTransformer.BackgroundColorOut = 0;
            this.oColorTransformer.ColorQuantizationMode = ColourClashNet.Colors.ColorQuantizationMode.Unknown;
            this.oColorTransformer.Dithering = ColourClashNet.Colors.ColorDithering.Ordered_4x4;
            this.oColorTransformer.OnReset += new System.EventHandler(this.oColorTransformer_OnReset);
            this.oColorTransformer.OnCreate += new System.EventHandler(this.oColorTransformer_OnCreate);
            this.oColorTransformer.OnQuantize += new System.EventHandler<ColourClashNet.Controls.ColorTransformer.EventArgsTransformation>(this.oColorTransformer_OnQuantize);
            this.oColorTransformer.OnProcess += new System.EventHandler<ColourClashNet.Controls.ColorTransformer.EventArgsTransformation>(this.oColorTransformer_OnProcess);
            // 
            // ColorAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "ColorAnalyzer";
            this.Size = new System.Drawing.Size(1021, 670);
            this.Load += new System.EventHandler(this.ColorAnalyzer_Load);
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
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSrc)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProc)).EndInit();
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
        private PictureBox pictureBoxSrc;
        private ToolStripMenuItem exportToolStripMenuItem;
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
        private Controls.BitmapRender oBitmapRenderSource;
       
        private CheckBox chkScanLineCluster;
        private ToolStripMenuItem colorDistanceToolStripMenuItem;
        private ToolStripMenuItem rGBToolStripMenuItem;
        private ToolStripMenuItem hSVToolStripMenuItem;
        private ToolStripMenuItem aLLToolStripMenuItem;
        private ToolStripMenuItem bitmapIndexedToolStripMenuItem;
        private ToolStripMenuItem indexedPNGToolStripMenuItem;
        private ToolStripMenuItem bitmapToolStripMenuItem;
        private ToolStripMenuItem pNGToolStripMenuItem;
        private ToolStripMenuItem amigaToolStripMenuItem;
        private ToolStripMenuItem iFFILBMToolStripMenuItem;
        private ToolStripMenuItem bitplaneToolStripMenuItem1;
        private ToolStripMenuItem sourceCodeToolStripMenuItem;
        private ToolStripMenuItem rawBitplaneAndOCSCopperlistToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem1;
        private SplitContainer splitContainer2;
        private PictureBox pictureBoxProc;
        private Controls.BitmapRender bitmapRender1;
        private Controls.BitmapRender bitmapRender2;
        private Controls.BitmapRender oBitmapRenderDest;
        private SaveFileDialog sfdExportImage;
        private Button button1;
        private Controls.ColorTransformer oColorTransformer;
    }
}
