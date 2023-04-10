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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColorAnalyzer));
            openFileDialog1 = new OpenFileDialog();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            bitmapIndexedToolStripMenuItem = new ToolStripMenuItem();
            indexedPNGToolStripMenuItem = new ToolStripMenuItem();
            bitmapToolStripMenuItem = new ToolStripMenuItem();
            pNGToolStripMenuItem = new ToolStripMenuItem();
            amigaToolStripMenuItem = new ToolStripMenuItem();
            iFFILBMToolStripMenuItem = new ToolStripMenuItem();
            bitplaneToolStripMenuItem1 = new ToolStripMenuItem();
            rawBitplaneAndOCSCopperlistToolStripMenuItem = new ToolStripMenuItem();
            sourceCodeToolStripMenuItem = new ToolStripMenuItem();
            colorModeToolStripMenuItem = new ToolStripMenuItem();
            colorDistanceToolStripMenuItem = new ToolStripMenuItem();
            ditheringToolStripMenuItem = new ToolStripMenuItem();
            scMain = new SplitContainer();
            tableLayoutPanel1 = new TableLayoutPanel();
            propertyGrid1 = new PropertyGrid();
            tableLayoutPanel2 = new TableLayoutPanel();
            panel2 = new Panel();
            rbLayoutH = new RadioButton();
            gbLayoutV = new RadioButton();
            pbBkColor = new PictureBox();
            label3 = new Label();
            panel3 = new Panel();
            nudColorsWanted = new NumericUpDown();
            label2 = new Label();
            panClustering = new Panel();
            label6 = new Label();
            nudClusterLoop = new NumericUpDown();
            btnReduceColorCluster = new Button();
            panScanline = new Panel();
            label7 = new Label();
            nudScanlineTrainEpochs = new NumericUpDown();
            chkScanLineCluster = new CheckBox();
            btnReduceColorsScanline = new Button();
            panel5 = new Panel();
            nudDitheringStrenght = new NumericUpDown();
            tbDitherStrenght = new TrackBar();
            label5 = new Label();
            panQuantitative = new Panel();
            btnReduceColors = new Button();
            panEGA = new Panel();
            btnReduceColorsEga = new Button();
            panC64 = new Panel();
            lblC64VideoMode = new Label();
            cbC64VideoMode = new ComboBox();
            btnReduceColorsC64v1 = new Button();
            panZX = new Panel();
            nudZxColorHI = new NumericUpDown();
            nudZxColorLO = new NumericUpDown();
            label1 = new Label();
            label4 = new Label();
            btnReduceColorsZx = new Button();
            panCPC = new Panel();
            lblCpcVideoMode = new Label();
            cbCpcVideoMode = new ComboBox();
            btnReduceColorCPC = new Button();
            panel10 = new Panel();
            lblHue = new Label();
            nudHue = new NumericUpDown();
            nudBright = new NumericUpDown();
            lblBright = new Label();
            nudSat = new NumericUpDown();
            lblSat = new Label();
            btnChromaAdapt = new Button();
            scLayout = new SplitContainer();
            pictureBoxSrc = new PictureBox();
            pictureBoxProc = new PictureBox();
            oBitmapRenderSource = new Controls.BitmapRender(components);
            bitmapRender1 = new Controls.BitmapRender(components);
            bitmapRender2 = new Controls.BitmapRender(components);
            oBitmapRenderDest = new Controls.BitmapRender(components);
            sfdExportImage = new SaveFileDialog();
            oColorTransformer = new Controls.ColorTransformer(components);
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)scMain).BeginInit();
            scMain.Panel1.SuspendLayout();
            scMain.Panel2.SuspendLayout();
            scMain.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbBkColor).BeginInit();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudColorsWanted).BeginInit();
            panClustering.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudClusterLoop).BeginInit();
            panScanline.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudScanlineTrainEpochs).BeginInit();
            panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudDitheringStrenght).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tbDitherStrenght).BeginInit();
            panQuantitative.SuspendLayout();
            panEGA.SuspendLayout();
            panC64.SuspendLayout();
            panZX.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudZxColorHI).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudZxColorLO).BeginInit();
            panCPC.SuspendLayout();
            panel10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudHue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudBright).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSat).BeginInit();
            ((System.ComponentModel.ISupportInitialize)scLayout).BeginInit();
            scLayout.Panel1.SuspendLayout();
            scLayout.Panel2.SuspendLayout();
            scLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSrc).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxProc).BeginInit();
            SuspendLayout();
            // 
            // openFileDialog1
            // 
            openFileDialog1.Filter = "Bmp|*.bmp|Png|*.png|Jpg|*.jpg|All Files|*.*";
            openFileDialog1.FilterIndex = 4;
            openFileDialog1.Title = "Load Image";
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, colorModeToolStripMenuItem, colorDistanceToolStripMenuItem, ditheringToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1134, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem, openToolStripMenuItem, toolStripMenuItem1, exportToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            newToolStripMenuItem.Image = (Image)resources.GetObject("newToolStripMenuItem.Image");
            newToolStripMenuItem.ImageTransparentColor = Color.Magenta;
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            newToolStripMenuItem.Size = new Size(146, 22);
            newToolStripMenuItem.Text = "&New";
            newToolStripMenuItem.Click += newToolStripMenuItem_Click;
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Image = (Image)resources.GetObject("openToolStripMenuItem.Image");
            openToolStripMenuItem.ImageTransparentColor = Color.Magenta;
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            openToolStripMenuItem.Size = new Size(146, 22);
            openToolStripMenuItem.Text = "&Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Image = (Image)resources.GetObject("toolStripMenuItem1.Image");
            toolStripMenuItem1.ImageTransparentColor = Color.Magenta;
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.ShortcutKeys = Keys.Control | Keys.S;
            toolStripMenuItem1.Size = new Size(146, 22);
            toolStripMenuItem1.Text = "&Save";
            toolStripMenuItem1.Click += toolStripMenuItem1_Click;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { bitmapIndexedToolStripMenuItem, indexedPNGToolStripMenuItem, bitmapToolStripMenuItem, pNGToolStripMenuItem, amigaToolStripMenuItem });
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(146, 22);
            exportToolStripMenuItem.Text = "Export";
            // 
            // bitmapIndexedToolStripMenuItem
            // 
            bitmapIndexedToolStripMenuItem.Name = "bitmapIndexedToolStripMenuItem";
            bitmapIndexedToolStripMenuItem.Size = new Size(157, 22);
            bitmapIndexedToolStripMenuItem.Text = "Bitmap Indexed";
            bitmapIndexedToolStripMenuItem.Click += bitmapIndexedToolStripMenuItem_Click;
            // 
            // indexedPNGToolStripMenuItem
            // 
            indexedPNGToolStripMenuItem.Name = "indexedPNGToolStripMenuItem";
            indexedPNGToolStripMenuItem.Size = new Size(157, 22);
            indexedPNGToolStripMenuItem.Text = "PNG Indexed";
            // 
            // bitmapToolStripMenuItem
            // 
            bitmapToolStripMenuItem.Name = "bitmapToolStripMenuItem";
            bitmapToolStripMenuItem.Size = new Size(157, 22);
            bitmapToolStripMenuItem.Text = "Bitmap";
            bitmapToolStripMenuItem.Click += bitmapToolStripMenuItem_Click;
            // 
            // pNGToolStripMenuItem
            // 
            pNGToolStripMenuItem.Name = "pNGToolStripMenuItem";
            pNGToolStripMenuItem.Size = new Size(157, 22);
            pNGToolStripMenuItem.Text = "PNG";
            pNGToolStripMenuItem.Click += pNGToolStripMenuItem_Click;
            // 
            // amigaToolStripMenuItem
            // 
            amigaToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { iFFILBMToolStripMenuItem, bitplaneToolStripMenuItem1, rawBitplaneAndOCSCopperlistToolStripMenuItem, sourceCodeToolStripMenuItem });
            amigaToolStripMenuItem.Name = "amigaToolStripMenuItem";
            amigaToolStripMenuItem.Size = new Size(157, 22);
            amigaToolStripMenuItem.Text = "Amiga";
            // 
            // iFFILBMToolStripMenuItem
            // 
            iFFILBMToolStripMenuItem.Name = "iFFILBMToolStripMenuItem";
            iFFILBMToolStripMenuItem.Size = new Size(224, 22);
            iFFILBMToolStripMenuItem.Text = "IFF-ILBM";
            // 
            // bitplaneToolStripMenuItem1
            // 
            bitplaneToolStripMenuItem1.Name = "bitplaneToolStripMenuItem1";
            bitplaneToolStripMenuItem1.Size = new Size(224, 22);
            bitplaneToolStripMenuItem1.Text = "Raw Bitplane";
            bitplaneToolStripMenuItem1.Click += bitplaneToolStripMenuItem1_Click;
            // 
            // rawBitplaneAndOCSCopperlistToolStripMenuItem
            // 
            rawBitplaneAndOCSCopperlistToolStripMenuItem.Name = "rawBitplaneAndOCSCopperlistToolStripMenuItem";
            rawBitplaneAndOCSCopperlistToolStripMenuItem.Size = new Size(224, 22);
            rawBitplaneAndOCSCopperlistToolStripMenuItem.Text = "Raw Bitplane And Copperlist";
            // 
            // sourceCodeToolStripMenuItem
            // 
            sourceCodeToolStripMenuItem.Name = "sourceCodeToolStripMenuItem";
            sourceCodeToolStripMenuItem.Size = new Size(224, 22);
            sourceCodeToolStripMenuItem.Text = "SourceCode";
            // 
            // colorModeToolStripMenuItem
            // 
            colorModeToolStripMenuItem.Name = "colorModeToolStripMenuItem";
            colorModeToolStripMenuItem.Size = new Size(87, 20);
            colorModeToolStripMenuItem.Tag = "FullColor";
            colorModeToolStripMenuItem.Text = "Quantization";
            // 
            // colorDistanceToolStripMenuItem
            // 
            colorDistanceToolStripMenuItem.Name = "colorDistanceToolStripMenuItem";
            colorDistanceToolStripMenuItem.Size = new Size(93, 20);
            colorDistanceToolStripMenuItem.Text = "ColorDistance";
            // 
            // ditheringToolStripMenuItem
            // 
            ditheringToolStripMenuItem.Name = "ditheringToolStripMenuItem";
            ditheringToolStripMenuItem.Size = new Size(68, 20);
            ditheringToolStripMenuItem.Text = "Dithering";
            // 
            // scMain
            // 
            scMain.Dock = DockStyle.Fill;
            scMain.Location = new Point(0, 24);
            scMain.Name = "scMain";
            // 
            // scMain.Panel1
            // 
            scMain.Panel1.Controls.Add(tableLayoutPanel1);
            // 
            // scMain.Panel2
            // 
            scMain.Panel2.Controls.Add(scLayout);
            scMain.Size = new Size(1134, 791);
            scMain.SplitterDistance = 368;
            scMain.TabIndex = 1;
            scMain.SplitterMoved += scMain_SplitterMoved;
            scMain.SizeChanged += scMain_SizeChanged;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(propertyGrid1, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 603F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(368, 791);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // propertyGrid1
            // 
            propertyGrid1.Dock = DockStyle.Fill;
            propertyGrid1.Location = new Point(3, 606);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.Size = new Size(362, 182);
            propertyGrid1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(panel2, 0, 0);
            tableLayoutPanel2.Controls.Add(panel3, 0, 1);
            tableLayoutPanel2.Controls.Add(panClustering, 0, 4);
            tableLayoutPanel2.Controls.Add(panScanline, 0, 5);
            tableLayoutPanel2.Controls.Add(panel5, 0, 2);
            tableLayoutPanel2.Controls.Add(panQuantitative, 0, 3);
            tableLayoutPanel2.Controls.Add(panEGA, 0, 6);
            tableLayoutPanel2.Controls.Add(panC64, 0, 7);
            tableLayoutPanel2.Controls.Add(panZX, 0, 8);
            tableLayoutPanel2.Controls.Add(panCPC, 0, 9);
            tableLayoutPanel2.Controls.Add(panel10, 0, 10);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 11;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(362, 597);
            tableLayoutPanel2.TabIndex = 8;
            // 
            // panel2
            // 
            panel2.Controls.Add(rbLayoutH);
            panel2.Controls.Add(gbLayoutV);
            panel2.Controls.Add(pbBkColor);
            panel2.Controls.Add(label3);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 0);
            panel2.Margin = new Padding(0);
            panel2.Name = "panel2";
            panel2.Size = new Size(362, 54);
            panel2.TabIndex = 28;
            // 
            // rbLayoutH
            // 
            rbLayoutH.AutoSize = true;
            rbLayoutH.Checked = true;
            rbLayoutH.Location = new Point(237, 28);
            rbLayoutH.Name = "rbLayoutH";
            rbLayoutH.Size = new Size(119, 19);
            rbLayoutH.TabIndex = 14;
            rbLayoutH.TabStop = true;
            rbLayoutH.Text = "Horizontal Layout";
            rbLayoutH.UseVisualStyleBackColor = true;
            rbLayoutH.CheckedChanged += rbLayoutH_CheckedChanged;
            // 
            // gbLayoutV
            // 
            gbLayoutV.AutoSize = true;
            gbLayoutV.Location = new Point(237, 3);
            gbLayoutV.Name = "gbLayoutV";
            gbLayoutV.Size = new Size(102, 19);
            gbLayoutV.TabIndex = 13;
            gbLayoutV.Text = "Vertical Layout";
            gbLayoutV.UseVisualStyleBackColor = true;
            gbLayoutV.CheckedChanged += gbLayoutV_CheckedChanged;
            // 
            // pbBkColor
            // 
            pbBkColor.Location = new Point(138, 3);
            pbBkColor.Name = "pbBkColor";
            pbBkColor.Size = new Size(47, 48);
            pbBkColor.TabIndex = 12;
            pbBkColor.TabStop = false;
            pbBkColor.DoubleClick += pbBkColor_DoubleClick;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(3, 18);
            label3.Name = "label3";
            label3.Size = new Size(103, 15);
            label3.TabIndex = 11;
            label3.Text = "Background Color";
            // 
            // panel3
            // 
            panel3.Controls.Add(nudColorsWanted);
            panel3.Controls.Add(label2);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(0, 54);
            panel3.Margin = new Padding(0);
            panel3.Name = "panel3";
            panel3.Size = new Size(362, 54);
            panel3.TabIndex = 29;
            // 
            // nudColorsWanted
            // 
            nudColorsWanted.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            nudColorsWanted.Location = new Point(138, 7);
            nudColorsWanted.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
            nudColorsWanted.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            nudColorsWanted.Name = "nudColorsWanted";
            nudColorsWanted.Size = new Size(65, 23);
            nudColorsWanted.TabIndex = 2;
            nudColorsWanted.Value = new decimal(new int[] { 16, 0, 0, 0 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 9);
            label2.Name = "label2";
            label2.Size = new Size(80, 15);
            label2.TabIndex = 10;
            label2.Text = "Color Wanted";
            // 
            // panClustering
            // 
            panClustering.Controls.Add(label6);
            panClustering.Controls.Add(nudClusterLoop);
            panClustering.Controls.Add(btnReduceColorCluster);
            panClustering.Dock = DockStyle.Fill;
            panClustering.Location = new Point(0, 216);
            panClustering.Margin = new Padding(0);
            panClustering.Name = "panClustering";
            panClustering.Size = new Size(362, 54);
            panClustering.TabIndex = 33;
            // 
            // label6
            // 
            label6.Location = new Point(217, 8);
            label6.Name = "label6";
            label6.Size = new Size(51, 30);
            label6.TabIndex = 20;
            label6.Text = "Train epochs";
            // 
            // nudClusterLoop
            // 
            nudClusterLoop.Location = new Point(274, 13);
            nudClusterLoop.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudClusterLoop.Minimum = new decimal(new int[] { 30, 0, 0, 0 });
            nudClusterLoop.Name = "nudClusterLoop";
            nudClusterLoop.Size = new Size(65, 23);
            nudClusterLoop.TabIndex = 6;
            nudClusterLoop.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // btnReduceColorCluster
            // 
            btnReduceColorCluster.Location = new Point(3, -1);
            btnReduceColorCluster.Name = "btnReduceColorCluster";
            btnReduceColorCluster.Size = new Size(129, 50);
            btnReduceColorCluster.TabIndex = 5;
            btnReduceColorCluster.Text = "Clustering Reduction";
            btnReduceColorCluster.UseVisualStyleBackColor = true;
            btnReduceColorCluster.Click += btnReduceColorCluster_Click;
            // 
            // panScanline
            // 
            panScanline.Controls.Add(label7);
            panScanline.Controls.Add(nudScanlineTrainEpochs);
            panScanline.Controls.Add(chkScanLineCluster);
            panScanline.Controls.Add(btnReduceColorsScanline);
            panScanline.Dock = DockStyle.Fill;
            panScanline.Location = new Point(0, 270);
            panScanline.Margin = new Padding(0);
            panScanline.Name = "panScanline";
            panScanline.Size = new Size(362, 54);
            panScanline.TabIndex = 32;
            // 
            // label7
            // 
            label7.Location = new Point(217, 8);
            label7.Name = "label7";
            label7.Size = new Size(51, 30);
            label7.TabIndex = 22;
            label7.Text = "Train epochs";
            // 
            // nudScanlineTrainEpochs
            // 
            nudScanlineTrainEpochs.Location = new Point(274, 13);
            nudScanlineTrainEpochs.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudScanlineTrainEpochs.Minimum = new decimal(new int[] { 30, 0, 0, 0 });
            nudScanlineTrainEpochs.Name = "nudScanlineTrainEpochs";
            nudScanlineTrainEpochs.Size = new Size(65, 23);
            nudScanlineTrainEpochs.TabIndex = 21;
            nudScanlineTrainEpochs.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // chkScanLineCluster
            // 
            chkScanLineCluster.Location = new Point(138, 5);
            chkScanLineCluster.Name = "chkScanLineCluster";
            chkScanLineCluster.Size = new Size(83, 37);
            chkScanLineCluster.TabIndex = 13;
            chkScanLineCluster.Text = "Clustering Scanline";
            chkScanLineCluster.UseVisualStyleBackColor = true;
            // 
            // btnReduceColorsScanline
            // 
            btnReduceColorsScanline.Location = new Point(3, 0);
            btnReduceColorsScanline.Name = "btnReduceColorsScanline";
            btnReduceColorsScanline.Size = new Size(129, 48);
            btnReduceColorsScanline.TabIndex = 9;
            btnReduceColorsScanline.Text = "Scanline Reduction";
            btnReduceColorsScanline.UseVisualStyleBackColor = true;
            btnReduceColorsScanline.Click += btnReduceColorsScanline_Click;
            // 
            // panel5
            // 
            panel5.Controls.Add(nudDitheringStrenght);
            panel5.Controls.Add(tbDitherStrenght);
            panel5.Controls.Add(label5);
            panel5.Dock = DockStyle.Fill;
            panel5.Location = new Point(0, 108);
            panel5.Margin = new Padding(0);
            panel5.Name = "panel5";
            panel5.Size = new Size(362, 54);
            panel5.TabIndex = 31;
            // 
            // nudDitheringStrenght
            // 
            nudDitheringStrenght.DecimalPlaces = 2;
            nudDitheringStrenght.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            nudDitheringStrenght.Location = new Point(274, 10);
            nudDitheringStrenght.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            nudDitheringStrenght.Name = "nudDitheringStrenght";
            nudDitheringStrenght.Size = new Size(65, 23);
            nudDitheringStrenght.TabIndex = 21;
            nudDitheringStrenght.ThousandsSeparator = true;
            nudDitheringStrenght.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudDitheringStrenght.ValueChanged += nudDitheringStrenght_ValueChanged;
            // 
            // tbDitherStrenght
            // 
            tbDitherStrenght.Location = new Point(71, 3);
            tbDitherStrenght.Maximum = 100;
            tbDitherStrenght.Name = "tbDitherStrenght";
            tbDitherStrenght.Size = new Size(197, 45);
            tbDitherStrenght.TabIndex = 11;
            tbDitherStrenght.TickFrequency = 10;
            tbDitherStrenght.TickStyle = TickStyle.Both;
            tbDitherStrenght.Value = 100;
            tbDitherStrenght.Scroll += tbDitherStrenght_Scroll;
            // 
            // label5
            // 
            label5.Location = new Point(3, 5);
            label5.Name = "label5";
            label5.Size = new Size(62, 40);
            label5.TabIndex = 20;
            label5.Text = "Dithering Strenght";
            // 
            // panQuantitative
            // 
            panQuantitative.Controls.Add(btnReduceColors);
            panQuantitative.Dock = DockStyle.Fill;
            panQuantitative.Location = new Point(0, 162);
            panQuantitative.Margin = new Padding(0);
            panQuantitative.Name = "panQuantitative";
            panQuantitative.Size = new Size(362, 54);
            panQuantitative.TabIndex = 30;
            // 
            // btnReduceColors
            // 
            btnReduceColors.Location = new Point(3, -1);
            btnReduceColors.Name = "btnReduceColors";
            btnReduceColors.Size = new Size(129, 49);
            btnReduceColors.TabIndex = 3;
            btnReduceColors.Text = "Quantitative color reduction";
            btnReduceColors.UseVisualStyleBackColor = true;
            btnReduceColors.Click += btnReduceColors_Click;
            // 
            // panEGA
            // 
            panEGA.Controls.Add(btnReduceColorsEga);
            panEGA.Dock = DockStyle.Fill;
            panEGA.Location = new Point(0, 324);
            panEGA.Margin = new Padding(0);
            panEGA.Name = "panEGA";
            panEGA.Size = new Size(362, 54);
            panEGA.TabIndex = 34;
            // 
            // btnReduceColorsEga
            // 
            btnReduceColorsEga.Location = new Point(3, 0);
            btnReduceColorsEga.Name = "btnReduceColorsEga";
            btnReduceColorsEga.Size = new Size(129, 48);
            btnReduceColorsEga.TabIndex = 15;
            btnReduceColorsEga.Text = "To EGA";
            btnReduceColorsEga.UseVisualStyleBackColor = true;
            btnReduceColorsEga.Click += btnReduceColorsEga_Click;
            // 
            // panC64
            // 
            panC64.Controls.Add(lblC64VideoMode);
            panC64.Controls.Add(cbC64VideoMode);
            panC64.Controls.Add(btnReduceColorsC64v1);
            panC64.Dock = DockStyle.Fill;
            panC64.Location = new Point(0, 378);
            panC64.Margin = new Padding(0);
            panC64.Name = "panC64";
            panC64.Size = new Size(362, 54);
            panC64.TabIndex = 35;
            // 
            // lblC64VideoMode
            // 
            lblC64VideoMode.Location = new Point(140, 16);
            lblC64VideoMode.Name = "lblC64VideoMode";
            lblC64VideoMode.Size = new Size(83, 21);
            lblC64VideoMode.TabIndex = 23;
            lblC64VideoMode.Text = "Video Mode";
            // 
            // cbC64VideoMode
            // 
            cbC64VideoMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cbC64VideoMode.FormattingEnabled = true;
            cbC64VideoMode.Location = new Point(230, 14);
            cbC64VideoMode.Name = "cbC64VideoMode";
            cbC64VideoMode.Size = new Size(121, 23);
            cbC64VideoMode.TabIndex = 19;
            // 
            // btnReduceColorsC64v1
            // 
            btnReduceColorsC64v1.Location = new Point(3, 0);
            btnReduceColorsC64v1.Name = "btnReduceColorsC64v1";
            btnReduceColorsC64v1.Size = new Size(129, 48);
            btnReduceColorsC64v1.TabIndex = 18;
            btnReduceColorsC64v1.Text = "To C64";
            btnReduceColorsC64v1.UseVisualStyleBackColor = true;
            btnReduceColorsC64v1.Click += BtnReduceColorsC64v1_Click;
            // 
            // panZX
            // 
            panZX.Controls.Add(nudZxColorHI);
            panZX.Controls.Add(nudZxColorLO);
            panZX.Controls.Add(label1);
            panZX.Controls.Add(label4);
            panZX.Controls.Add(btnReduceColorsZx);
            panZX.Dock = DockStyle.Fill;
            panZX.Location = new Point(0, 432);
            panZX.Margin = new Padding(0);
            panZX.Name = "panZX";
            panZX.Size = new Size(362, 54);
            panZX.TabIndex = 27;
            // 
            // nudZxColorHI
            // 
            nudZxColorHI.Location = new Point(301, 5);
            nudZxColorHI.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudZxColorHI.Name = "nudZxColorHI";
            nudZxColorHI.Size = new Size(50, 23);
            nudZxColorHI.TabIndex = 17;
            nudZxColorHI.Value = new decimal(new int[] { 255, 0, 0, 0 });
            // 
            // nudZxColorLO
            // 
            nudZxColorLO.Location = new Point(195, 5);
            nudZxColorLO.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudZxColorLO.Name = "nudZxColorLO";
            nudZxColorLO.Size = new Size(46, 23);
            nudZxColorLO.TabIndex = 16;
            nudZxColorLO.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(138, 7);
            label1.Name = "label1";
            label1.Size = new Size(51, 15);
            label1.TabIndex = 18;
            label1.Text = "ColorLO";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(247, 7);
            label4.Name = "label4";
            label4.Size = new Size(48, 15);
            label4.TabIndex = 19;
            label4.Text = "ColorHI";
            // 
            // btnReduceColorsZx
            // 
            btnReduceColorsZx.Location = new Point(3, 0);
            btnReduceColorsZx.Name = "btnReduceColorsZx";
            btnReduceColorsZx.Size = new Size(129, 48);
            btnReduceColorsZx.TabIndex = 14;
            btnReduceColorsZx.Text = "To ZX";
            btnReduceColorsZx.UseVisualStyleBackColor = true;
            btnReduceColorsZx.Click += btnReduceColorsZx_Click;
            // 
            // panCPC
            // 
            panCPC.Controls.Add(lblCpcVideoMode);
            panCPC.Controls.Add(cbCpcVideoMode);
            panCPC.Controls.Add(btnReduceColorCPC);
            panCPC.Dock = DockStyle.Fill;
            panCPC.Location = new Point(0, 486);
            panCPC.Margin = new Padding(0);
            panCPC.Name = "panCPC";
            panCPC.Size = new Size(362, 54);
            panCPC.TabIndex = 36;
            // 
            // lblCpcVideoMode
            // 
            lblCpcVideoMode.Location = new Point(140, 16);
            lblCpcVideoMode.Name = "lblCpcVideoMode";
            lblCpcVideoMode.Size = new Size(83, 21);
            lblCpcVideoMode.TabIndex = 23;
            lblCpcVideoMode.Text = "Video Mode";
            // 
            // cbCpcVideoMode
            // 
            cbCpcVideoMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cbCpcVideoMode.FormattingEnabled = true;
            cbCpcVideoMode.Location = new Point(230, 14);
            cbCpcVideoMode.Name = "cbCpcVideoMode";
            cbCpcVideoMode.Size = new Size(121, 23);
            cbCpcVideoMode.TabIndex = 19;
            // 
            // btnReduceColorCPC
            // 
            btnReduceColorCPC.Location = new Point(3, 0);
            btnReduceColorCPC.Name = "btnReduceColorCPC";
            btnReduceColorCPC.Size = new Size(129, 48);
            btnReduceColorCPC.TabIndex = 18;
            btnReduceColorCPC.Text = "To Amstrad CPC";
            btnReduceColorCPC.UseVisualStyleBackColor = true;
            btnReduceColorCPC.Click += btnReduceColorCPC_Click;
            // 
            // panel10
            // 
            panel10.Controls.Add(lblHue);
            panel10.Controls.Add(nudHue);
            panel10.Controls.Add(nudBright);
            panel10.Controls.Add(lblBright);
            panel10.Controls.Add(nudSat);
            panel10.Controls.Add(lblSat);
            panel10.Controls.Add(btnChromaAdapt);
            panel10.Dock = DockStyle.Fill;
            panel10.Location = new Point(0, 540);
            panel10.Margin = new Padding(0);
            panel10.Name = "panel10";
            panel10.Size = new Size(362, 57);
            panel10.TabIndex = 28;
            // 
            // lblHue
            // 
            lblHue.AutoSize = true;
            lblHue.Location = new Point(286, 9);
            lblHue.Name = "lblHue";
            lblHue.Size = new Size(29, 15);
            lblHue.TabIndex = 22;
            lblHue.Text = "Hue";
            // 
            // nudHue
            // 
            nudHue.Increment = new decimal(new int[] { 30, 0, 0, 0 });
            nudHue.Location = new Point(286, 31);
            nudHue.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            nudHue.Name = "nudHue";
            nudHue.Size = new Size(65, 23);
            nudHue.TabIndex = 21;
            // 
            // nudBright
            // 
            nudBright.DecimalPlaces = 2;
            nudBright.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            nudBright.Location = new Point(205, 31);
            nudBright.Name = "nudBright";
            nudBright.Size = new Size(65, 23);
            nudBright.TabIndex = 20;
            nudBright.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblBright
            // 
            lblBright.AutoSize = true;
            lblBright.Location = new Point(138, 33);
            lblBright.Name = "lblBright";
            lblBright.Size = new Size(62, 15);
            lblBright.TabIndex = 19;
            lblBright.Text = "Brightness";
            // 
            // nudSat
            // 
            nudSat.DecimalPlaces = 2;
            nudSat.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            nudSat.Location = new Point(205, 7);
            nudSat.Name = "nudSat";
            nudSat.Size = new Size(65, 23);
            nudSat.TabIndex = 16;
            nudSat.Value = new decimal(new int[] { 15, 0, 0, 65536 });
            // 
            // lblSat
            // 
            lblSat.AutoSize = true;
            lblSat.Location = new Point(138, 9);
            lblSat.Name = "lblSat";
            lblSat.Size = new Size(61, 15);
            lblSat.TabIndex = 18;
            lblSat.Text = "Saturation";
            // 
            // btnChromaAdapt
            // 
            btnChromaAdapt.Location = new Point(3, 0);
            btnChromaAdapt.Name = "btnChromaAdapt";
            btnChromaAdapt.Size = new Size(129, 54);
            btnChromaAdapt.TabIndex = 14;
            btnChromaAdapt.Text = "Chroma Adapter";
            btnChromaAdapt.UseVisualStyleBackColor = true;
            btnChromaAdapt.Click += btnChromaAdapt_Click;
            // 
            // scLayout
            // 
            scLayout.Dock = DockStyle.Fill;
            scLayout.Location = new Point(0, 0);
            scLayout.Name = "scLayout";
            scLayout.Orientation = Orientation.Horizontal;
            // 
            // scLayout.Panel1
            // 
            scLayout.Panel1.Controls.Add(pictureBoxSrc);
            // 
            // scLayout.Panel2
            // 
            scLayout.Panel2.Controls.Add(pictureBoxProc);
            scLayout.Size = new Size(762, 791);
            scLayout.SplitterDistance = 395;
            scLayout.TabIndex = 0;
            // 
            // pictureBoxSrc
            // 
            pictureBoxSrc.BackColor = Color.Black;
            pictureBoxSrc.Dock = DockStyle.Fill;
            pictureBoxSrc.Location = new Point(0, 0);
            pictureBoxSrc.Name = "pictureBoxSrc";
            pictureBoxSrc.Size = new Size(762, 395);
            pictureBoxSrc.TabIndex = 0;
            pictureBoxSrc.TabStop = false;
            pictureBoxSrc.MouseDoubleClick += pictureBox1_MouseDoubleClick;
            // 
            // pictureBoxProc
            // 
            pictureBoxProc.BackColor = Color.Black;
            pictureBoxProc.Dock = DockStyle.Fill;
            pictureBoxProc.Location = new Point(0, 0);
            pictureBoxProc.Name = "pictureBoxProc";
            pictureBoxProc.Size = new Size(762, 392);
            pictureBoxProc.TabIndex = 0;
            pictureBoxProc.TabStop = false;
            // 
            // oBitmapRenderSource
            // 
            oBitmapRenderSource.Control = pictureBoxSrc;
            oBitmapRenderSource.Image = null;
            oBitmapRenderSource.ImageBlockScroll = false;
            oBitmapRenderSource.ImageMoveOverControlBorder = true;
            oBitmapRenderSource.ImageZoomManual = 1F;
            oBitmapRenderSource.ImageZoomMode = ColourClashNet.Controls.BitmapRender.EnumZoom.Zoom1;
            oBitmapRenderSource.MouseMovingButton = MouseButtons.Left;
            // 
            // bitmapRender1
            // 
            bitmapRender1.Control = pictureBoxSrc;
            bitmapRender1.Image = null;
            bitmapRender1.ImageBlockScroll = false;
            bitmapRender1.ImageMoveOverControlBorder = true;
            bitmapRender1.ImageZoomManual = 1F;
            bitmapRender1.ImageZoomMode = ColourClashNet.Controls.BitmapRender.EnumZoom.Zoom1;
            bitmapRender1.MouseMovingButton = MouseButtons.Left;
            // 
            // bitmapRender2
            // 
            bitmapRender2.Control = pictureBoxSrc;
            bitmapRender2.Image = null;
            bitmapRender2.ImageBlockScroll = false;
            bitmapRender2.ImageMoveOverControlBorder = true;
            bitmapRender2.ImageZoomManual = 1F;
            bitmapRender2.ImageZoomMode = ColourClashNet.Controls.BitmapRender.EnumZoom.Zoom1;
            bitmapRender2.MouseMovingButton = MouseButtons.Left;
            // 
            // oBitmapRenderDest
            // 
            oBitmapRenderDest.Control = pictureBoxProc;
            oBitmapRenderDest.Image = null;
            oBitmapRenderDest.ImageBlockScroll = false;
            oBitmapRenderDest.ImageMoveOverControlBorder = true;
            oBitmapRenderDest.ImageZoomManual = 1F;
            oBitmapRenderDest.ImageZoomMode = ColourClashNet.Controls.BitmapRender.EnumZoom.Zoom1;
            oBitmapRenderDest.MouseMovingButton = MouseButtons.Left;
            // 
            // sfdExportImage
            // 
            sfdExportImage.AddExtension = false;
            // 
            // oColorTransformer
            // 
            oColorTransformer.BackgroundColorList = (List<int>)resources.GetObject("oColorTransformer.BackgroundColorList");
            oColorTransformer.BackgroundColorReplacement = 0;
            oColorTransformer.BrightnessEnhancement = 1D;
            oColorTransformer.C64ScreenMode = Colors.Transformation.ColorTransformReductionC64.C64ScreenMode.Multicolor;
            oColorTransformer.ClusteringTrainingLoop = 30;
            oColorTransformer.ClusteringUseMeanColor = true;
            oColorTransformer.ColorDistanceEvaluationMode = Colors.ColorDistanceEvaluationMode.RGB;
            oColorTransformer.ColorQuantizationMode = Colors.ColorQuantizationMode.Unknown;
            oColorTransformer.ColorsMax = 16;
            oColorTransformer.ColorTransformAlgorithm = Colors.ColorTransform.None;
            oColorTransformer.CPCScreenMode = Colors.Transformation.ColorTransformReductionCPC.CPCScreenMode.Mode0;
            oColorTransformer.DitheringAlgorithm = Colors.ColorDithering.Ordered_4x4;
            oColorTransformer.DitheringStrenght = 1D;
            oColorTransformer.HueOffset = 0D;
            oColorTransformer.SaturationEnhancement = 1.5D;
            oColorTransformer.ScanlineClustering = true;
            oColorTransformer.ZxEqColorHI = 255;
            oColorTransformer.ZxEqColorLO = 128;
            oColorTransformer.OnReset += oColorTransformer_OnReset;
            oColorTransformer.OnCreate += oColorTransformer_OnCreate;
            oColorTransformer.OnQuantize += oColorTransformer_OnQuantize;
            oColorTransformer.OnProcess += oColorTransformer_OnProcess;
            // 
            // ColorAnalyzer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(scMain);
            Controls.Add(menuStrip1);
            Name = "ColorAnalyzer";
            Size = new Size(1134, 815);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            scMain.Panel1.ResumeLayout(false);
            scMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)scMain).EndInit();
            scMain.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbBkColor).EndInit();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudColorsWanted).EndInit();
            panClustering.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudClusterLoop).EndInit();
            panScanline.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudScanlineTrainEpochs).EndInit();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudDitheringStrenght).EndInit();
            ((System.ComponentModel.ISupportInitialize)tbDitherStrenght).EndInit();
            panQuantitative.ResumeLayout(false);
            panEGA.ResumeLayout(false);
            panC64.ResumeLayout(false);
            panZX.ResumeLayout(false);
            panZX.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudZxColorHI).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudZxColorLO).EndInit();
            panCPC.ResumeLayout(false);
            panel10.ResumeLayout(false);
            panel10.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudHue).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudBright).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSat).EndInit();
            scLayout.Panel1.ResumeLayout(false);
            scLayout.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)scLayout).EndInit();
            scLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxSrc).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxProc).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private OpenFileDialog openFileDialog1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private SplitContainer scMain;
        private PictureBox pictureBoxSrc;
        private ToolStripMenuItem exportToolStripMenuItem;
        private TableLayoutPanel tableLayoutPanel1;
        private PropertyGrid propertyGrid1;
        private TableLayoutPanel tableLayoutPanel2;
        private Label label2;
        private Button btnReduceColors;
        private NumericUpDown nudColorsWanted;
        private Button btnReduceColorCluster;
        private NumericUpDown nudClusterLoop;
        private Button btnReduceColorsScanline;
        private Label label3;
        private PictureBox pbBkColor;
        private Controls.BitmapRender oBitmapRenderSource;

        private CheckBox chkScanLineCluster;
        private ToolStripMenuItem colorDistanceToolStripMenuItem;
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
        private SplitContainer scLayout;
        private PictureBox pictureBoxProc;
        private Controls.BitmapRender bitmapRender1;
        private Controls.BitmapRender bitmapRender2;
        private Controls.BitmapRender oBitmapRenderDest;
        private SaveFileDialog sfdExportImage;
        private Button btnReduceColorsZx;
        private Controls.ColorTransformer oColorTransformer;
        private ToolStripMenuItem ditheringToolStripMenuItem;
        private ToolStripMenuItem colorModeToolStripMenuItem;
        private NumericUpDown nudZxColorLO;
        private NumericUpDown nudZxColorHI;
        private Button btnReduceColorsC64v1;
        private Button btnReduceColorsEga;
        private Label label1;
        private Label label4;
        private Panel panZX;
        private Panel panel2;
        private Panel panel3;
        private Panel panQuantitative;
        private Panel panel5;
        private NumericUpDown nudDitheringStrenght;
        private TrackBar tbDitherStrenght;
        private Label label5;
        private Panel panScanline;
        private Panel panClustering;
        private Label label6;
        private Label label7;
        private NumericUpDown nudScanlineTrainEpochs;
        private Panel panEGA;
        private Panel panC64;
        private RadioButton rbLayoutH;
        private RadioButton gbLayoutV;
        private Label lblC64VideoMode;
        private ComboBox cbC64VideoMode;
        private Panel panCPC;
        private Label lblCpcVideoMode;
        private ComboBox cbCpcVideoMode;
        private Button btnReduceColorCPC;
        private Panel panel10;
        private NumericUpDown nudHue;
        private NumericUpDown nudBright;
        private Label lblBright;
        private NumericUpDown nudSat;
        private Label lblSat;
        private Button btnChromaAdapt;
        private Label lblHue;
    }
}
