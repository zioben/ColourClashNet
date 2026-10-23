namespace ColourClashNet.Controls
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
            splitMainA_BC = new SplitContainer();
            layoutInput = new TableLayoutPanel();
            gbSetup = new GroupBox();
            layoutConfig = new TableLayoutPanel();
            panelSelectColors = new Panel();
            chkShowTileBorders = new CheckBox();
            nudColorsWanted = new NumericUpDown();
            label2 = new Label();
            pbBkColor = new PictureBox();
            label3 = new Label();
            panelDithering = new Panel();
            nudDitheringStrenght = new NumericUpDown();
            tbDitherStrenght = new TrackBar();
            label5 = new Label();
            tabControlMain = new TabControl();
            tpMainGeneric = new TabPage();
            gbHsvTransform = new GroupBox();
            lblHue = new Label();
            nudHue = new NumericUpDown();
            lblSat = new Label();
            btnChromaAdapt = new Button();
            nudBright = new NumericUpDown();
            lblBright = new Label();
            nudSat = new NumericUpDown();
            gbClusteringTransform = new GroupBox();
            label6 = new Label();
            nudClusterLoop = new NumericUpDown();
            btnReduceColorCluster = new Button();
            gbMeanTransform = new GroupBox();
            btnReduceColors = new Button();
            tpMainIndexed = new TabPage();
            gbVgaTransform = new GroupBox();
            gbEgaTransform = new GroupBox();
            btnReduceColorsEga = new Button();
            gbCgaTransform = new GroupBox();
            gbHerculeTransform = new GroupBox();
            tpMainCbm = new TabPage();
            tabCbm = new TabControl();
            tpCbm64 = new TabPage();
            cbC64VideoMode = new ComboBox();
            lblC64VideoMode = new Label();
            btnReduceColorsC64v1 = new Button();
            tpCbmAmiga = new TabPage();
            gcHamTransform = new GroupBox();
            btnReduceAmiga = new Button();
            label8 = new Label();
            cbAmigaVideoMode = new ComboBox();
            gcScanlineTransform = new GroupBox();
            numLinesPerBlock = new NumericUpDown();
            label12 = new Label();
            btnReduceColorsScanline = new Button();
            chkScanlineSharedPal = new CheckBox();
            label9 = new Label();
            nudScanlineTrainEpochs = new NumericUpDown();
            label7 = new Label();
            nudScanlineLineColors = new NumericUpDown();
            tpMainSinclair = new TabPage();
            label11 = new Label();
            cbZxAutotuneMode = new ComboBox();
            cbZxPaletteMode = new ComboBox();
            label10 = new Label();
            chkZxDitherImageLo = new CheckBox();
            zxBtnReset = new Button();
            chkZxDitherHI = new CheckBox();
            btnReduceColorsZx = new Button();
            chkZxBlackHI = new CheckBox();
            label1 = new Label();
            nudZxColorHI = new NumericUpDown();
            label4 = new Label();
            nudZxColorLO = new NumericUpDown();
            tpMainAmstrad = new TabPage();
            cbCpcVideoMode = new ComboBox();
            lblCpcVideoMode = new Label();
            btnReduceColorCPC = new Button();
            splitMainB_C = new SplitContainer();
            layoutRender = new TableLayoutPanel();
            ssImageData = new StatusStrip();
            tsLayout = new ToolStripDropDownButton();
            horizontalToolStripMenuItem = new ToolStripMenuItem();
            verticalToolStripMenuItem = new ToolStripMenuItem();
            tsImageLabel = new ToolStripStatusLabel();
            tsImageCoord = new ToolStripStatusLabel();
            tsMouseLabel = new ToolStripStatusLabel();
            tsMouseCoord = new ToolStripStatusLabel();
            tsColorLabel = new ToolStripStatusLabel();
            tsColorData = new ToolStripStatusLabel();
            tsProgressLabel = new ToolStripStatusLabel();
            tsProcessingProgress = new ToolStripProgressBar();
            splitRenderLayout = new SplitContainer();
            gbImageSource = new GroupBox();
            pictureBoxSrc = new PictureBox();
            gbImageProcessed = new GroupBox();
            pictureBoxProc = new PictureBox();
            splitProcessing = new SplitContainer();
            gbColorProcessor = new GroupBox();
            pgColorProcessor = new PropertyGrid();
            gbConfig = new GroupBox();
            pgColorProcessorConfig = new PropertyGrid();
            oBitmapRenderSource = new ColourClashNet.Components.BitmapRender(components);
            oBitmapRenderDest = new ColourClashNet.Components.BitmapRender(components);
            oColorManager = new ColourClashNet.Components.ColorManager(components);
            ((System.ComponentModel.ISupportInitialize)splitMainA_BC).BeginInit();
            splitMainA_BC.Panel1.SuspendLayout();
            splitMainA_BC.Panel2.SuspendLayout();
            splitMainA_BC.SuspendLayout();
            layoutInput.SuspendLayout();
            gbSetup.SuspendLayout();
            layoutConfig.SuspendLayout();
            panelSelectColors.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudColorsWanted).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbBkColor).BeginInit();
            panelDithering.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudDitheringStrenght).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tbDitherStrenght).BeginInit();
            tabControlMain.SuspendLayout();
            tpMainGeneric.SuspendLayout();
            gbHsvTransform.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudHue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudBright).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSat).BeginInit();
            gbClusteringTransform.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudClusterLoop).BeginInit();
            gbMeanTransform.SuspendLayout();
            tpMainIndexed.SuspendLayout();
            gbEgaTransform.SuspendLayout();
            tpMainCbm.SuspendLayout();
            tabCbm.SuspendLayout();
            tpCbm64.SuspendLayout();
            tpCbmAmiga.SuspendLayout();
            gcHamTransform.SuspendLayout();
            gcScanlineTransform.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numLinesPerBlock).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudScanlineTrainEpochs).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudScanlineLineColors).BeginInit();
            tpMainSinclair.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudZxColorHI).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudZxColorLO).BeginInit();
            tpMainAmstrad.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMainB_C).BeginInit();
            splitMainB_C.Panel1.SuspendLayout();
            splitMainB_C.Panel2.SuspendLayout();
            splitMainB_C.SuspendLayout();
            layoutRender.SuspendLayout();
            ssImageData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitRenderLayout).BeginInit();
            splitRenderLayout.Panel1.SuspendLayout();
            splitRenderLayout.Panel2.SuspendLayout();
            splitRenderLayout.SuspendLayout();
            gbImageSource.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSrc).BeginInit();
            gbImageProcessed.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxProc).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitProcessing).BeginInit();
            splitProcessing.Panel1.SuspendLayout();
            splitProcessing.Panel2.SuspendLayout();
            splitProcessing.SuspendLayout();
            gbColorProcessor.SuspendLayout();
            gbConfig.SuspendLayout();
            SuspendLayout();
            // 
            // splitMainA_BC
            // 
            splitMainA_BC.Dock = DockStyle.Fill;
            splitMainA_BC.Location = new Point(0, 0);
            splitMainA_BC.Name = "splitMainA_BC";
            // 
            // splitMainA_BC.Panel1
            // 
            splitMainA_BC.Panel1.Controls.Add(layoutInput);
            // 
            // splitMainA_BC.Panel2
            // 
            splitMainA_BC.Panel2.Controls.Add(splitMainB_C);
            splitMainA_BC.Size = new Size(1134, 753);
            splitMainA_BC.SplitterDistance = 337;
            splitMainA_BC.TabIndex = 1;
            splitMainA_BC.SplitterMoved += scMain_SplitterMoved;
            splitMainA_BC.SizeChanged += scMain_SizeChanged;
            // 
            // layoutInput
            // 
            layoutInput.ColumnCount = 1;
            layoutInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layoutInput.Controls.Add(gbSetup, 0, 0);
            layoutInput.Dock = DockStyle.Fill;
            layoutInput.Location = new Point(0, 0);
            layoutInput.Name = "layoutInput";
            layoutInput.RowCount = 1;
            layoutInput.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            layoutInput.Size = new Size(337, 753);
            layoutInput.TabIndex = 1;
            // 
            // gbSetup
            // 
            gbSetup.Controls.Add(layoutConfig);
            gbSetup.Dock = DockStyle.Fill;
            gbSetup.Location = new Point(3, 3);
            gbSetup.Name = "gbSetup";
            gbSetup.Size = new Size(331, 747);
            gbSetup.TabIndex = 0;
            gbSetup.TabStop = false;
            gbSetup.Text = "Processing Input";
            // 
            // layoutConfig
            // 
            layoutConfig.ColumnCount = 1;
            layoutConfig.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutConfig.Controls.Add(panelSelectColors, 0, 0);
            layoutConfig.Controls.Add(panelDithering, 0, 1);
            layoutConfig.Controls.Add(tabControlMain, 0, 2);
            layoutConfig.Dock = DockStyle.Fill;
            layoutConfig.Location = new Point(3, 19);
            layoutConfig.Name = "layoutConfig";
            layoutConfig.RowCount = 3;
            layoutConfig.RowStyles.Add(new RowStyle(SizeType.Absolute, 69F));
            layoutConfig.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            layoutConfig.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutConfig.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            layoutConfig.Size = new Size(325, 725);
            layoutConfig.TabIndex = 9;
            // 
            // panelSelectColors
            // 
            panelSelectColors.Controls.Add(chkShowTileBorders);
            panelSelectColors.Controls.Add(nudColorsWanted);
            panelSelectColors.Controls.Add(label2);
            panelSelectColors.Controls.Add(pbBkColor);
            panelSelectColors.Controls.Add(label3);
            panelSelectColors.Dock = DockStyle.Fill;
            panelSelectColors.Location = new Point(0, 0);
            panelSelectColors.Margin = new Padding(0);
            panelSelectColors.Name = "panelSelectColors";
            panelSelectColors.Size = new Size(325, 69);
            panelSelectColors.TabIndex = 29;
            // 
            // chkShowTileBorders
            // 
            chkShowTileBorders.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chkShowTileBorders.AutoSize = true;
            chkShowTileBorders.Location = new Point(198, 9);
            chkShowTileBorders.Name = "chkShowTileBorders";
            chkShowTileBorders.Size = new Size(120, 19);
            chkShowTileBorders.TabIndex = 13;
            chkShowTileBorders.Text = "Show Tile Borders";
            chkShowTileBorders.UseVisualStyleBackColor = true;
            // 
            // nudColorsWanted
            // 
            nudColorsWanted.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudColorsWanted.Increment = new decimal(new int[] { 4, 0, 0, 0 });
            nudColorsWanted.Location = new Point(120, 7);
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
            label2.Location = new Point(7, 9);
            label2.Name = "label2";
            label2.Size = new Size(80, 15);
            label2.TabIndex = 10;
            label2.Text = "Color Wanted";
            // 
            // pbBkColor
            // 
            pbBkColor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pbBkColor.Location = new Point(155, 34);
            pbBkColor.Name = "pbBkColor";
            pbBkColor.Size = new Size(163, 30);
            pbBkColor.TabIndex = 12;
            pbBkColor.TabStop = false;
            pbBkColor.DoubleClick += pbBkColor_DoubleClick;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(7, 41);
            label3.Name = "label3";
            label3.Size = new Size(142, 15);
            label3.TabIndex = 11;
            label3.Text = "Background Color Palette";
            // 
            // panelDithering
            // 
            panelDithering.Controls.Add(nudDitheringStrenght);
            panelDithering.Controls.Add(tbDitherStrenght);
            panelDithering.Controls.Add(label5);
            panelDithering.Dock = DockStyle.Fill;
            panelDithering.Location = new Point(0, 69);
            panelDithering.Margin = new Padding(0);
            panelDithering.Name = "panelDithering";
            panelDithering.Size = new Size(325, 60);
            panelDithering.TabIndex = 31;
            // 
            // nudDitheringStrenght
            // 
            nudDitheringStrenght.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudDitheringStrenght.DecimalPlaces = 2;
            nudDitheringStrenght.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            nudDitheringStrenght.Location = new Point(249, 16);
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
            tbDitherStrenght.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tbDitherStrenght.Location = new Point(71, 9);
            tbDitherStrenght.Maximum = 100;
            tbDitherStrenght.Name = "tbDitherStrenght";
            tbDitherStrenght.Size = new Size(172, 45);
            tbDitherStrenght.TabIndex = 11;
            tbDitherStrenght.TickFrequency = 10;
            tbDitherStrenght.TickStyle = TickStyle.Both;
            tbDitherStrenght.Value = 100;
            tbDitherStrenght.Scroll += tbDitherStrenght_Scroll;
            // 
            // label5
            // 
            label5.Location = new Point(3, 11);
            label5.Name = "label5";
            label5.Size = new Size(62, 40);
            label5.TabIndex = 20;
            label5.Text = "Dithering Strenght";
            // 
            // tabControlMain
            // 
            tabControlMain.Controls.Add(tpMainGeneric);
            tabControlMain.Controls.Add(tpMainIndexed);
            tabControlMain.Controls.Add(tpMainCbm);
            tabControlMain.Controls.Add(tpMainSinclair);
            tabControlMain.Controls.Add(tpMainAmstrad);
            tabControlMain.Dock = DockStyle.Fill;
            tabControlMain.Location = new Point(3, 132);
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(319, 590);
            tabControlMain.TabIndex = 32;
            // 
            // tpMainGeneric
            // 
            tpMainGeneric.Controls.Add(gbHsvTransform);
            tpMainGeneric.Controls.Add(gbClusteringTransform);
            tpMainGeneric.Controls.Add(gbMeanTransform);
            tpMainGeneric.Location = new Point(4, 24);
            tpMainGeneric.Name = "tpMainGeneric";
            tpMainGeneric.Padding = new Padding(3);
            tpMainGeneric.Size = new Size(311, 562);
            tpMainGeneric.TabIndex = 0;
            tpMainGeneric.Text = "Base";
            tpMainGeneric.UseVisualStyleBackColor = true;
            // 
            // gbHsvTransform
            // 
            gbHsvTransform.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbHsvTransform.Controls.Add(lblHue);
            gbHsvTransform.Controls.Add(nudHue);
            gbHsvTransform.Controls.Add(lblSat);
            gbHsvTransform.Controls.Add(btnChromaAdapt);
            gbHsvTransform.Controls.Add(nudBright);
            gbHsvTransform.Controls.Add(lblBright);
            gbHsvTransform.Controls.Add(nudSat);
            gbHsvTransform.Location = new Point(6, 228);
            gbHsvTransform.Name = "gbHsvTransform";
            gbHsvTransform.Size = new Size(299, 199);
            gbHsvTransform.TabIndex = 25;
            gbHsvTransform.TabStop = false;
            gbHsvTransform.Text = "HSV Transformation";
            // 
            // lblHue
            // 
            lblHue.AutoSize = true;
            lblHue.Location = new Point(8, 35);
            lblHue.Name = "lblHue";
            lblHue.Size = new Size(29, 15);
            lblHue.TabIndex = 22;
            lblHue.Text = "Hue";
            // 
            // nudHue
            // 
            nudHue.Increment = new decimal(new int[] { 30, 0, 0, 0 });
            nudHue.Location = new Point(87, 33);
            nudHue.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            nudHue.Name = "nudHue";
            nudHue.Size = new Size(65, 23);
            nudHue.TabIndex = 21;
            // 
            // lblSat
            // 
            lblSat.AutoSize = true;
            lblSat.Location = new Point(8, 72);
            lblSat.Name = "lblSat";
            lblSat.Size = new Size(61, 15);
            lblSat.TabIndex = 18;
            lblSat.Text = "Saturation";
            // 
            // btnChromaAdapt
            // 
            btnChromaAdapt.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnChromaAdapt.Location = new Point(6, 139);
            btnChromaAdapt.Name = "btnChromaAdapt";
            btnChromaAdapt.Size = new Size(287, 54);
            btnChromaAdapt.TabIndex = 14;
            btnChromaAdapt.Text = "Transform";
            btnChromaAdapt.UseVisualStyleBackColor = true;
            btnChromaAdapt.Click += btnChromaAdapt_Click;
            // 
            // nudBright
            // 
            nudBright.DecimalPlaces = 2;
            nudBright.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            nudBright.Location = new Point(87, 103);
            nudBright.Name = "nudBright";
            nudBright.Size = new Size(65, 23);
            nudBright.TabIndex = 20;
            nudBright.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblBright
            // 
            lblBright.AutoSize = true;
            lblBright.Location = new Point(8, 105);
            lblBright.Name = "lblBright";
            lblBright.Size = new Size(62, 15);
            lblBright.TabIndex = 19;
            lblBright.Text = "Brightness";
            // 
            // nudSat
            // 
            nudSat.DecimalPlaces = 2;
            nudSat.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            nudSat.Location = new Point(87, 70);
            nudSat.Name = "nudSat";
            nudSat.Size = new Size(65, 23);
            nudSat.TabIndex = 16;
            nudSat.Value = new decimal(new int[] { 15, 0, 0, 65536 });
            // 
            // gbClusteringTransform
            // 
            gbClusteringTransform.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbClusteringTransform.Controls.Add(label6);
            gbClusteringTransform.Controls.Add(nudClusterLoop);
            gbClusteringTransform.Controls.Add(btnReduceColorCluster);
            gbClusteringTransform.Location = new Point(6, 106);
            gbClusteringTransform.Name = "gbClusteringTransform";
            gbClusteringTransform.Size = new Size(299, 116);
            gbClusteringTransform.TabIndex = 24;
            gbClusteringTransform.TabStop = false;
            gbClusteringTransform.Text = "Clustering Reduction";
            // 
            // label6
            // 
            label6.Location = new Point(8, 29);
            label6.Name = "label6";
            label6.Size = new Size(134, 23);
            label6.TabIndex = 20;
            label6.Text = "Clustering train epochs";
            // 
            // nudClusterLoop
            // 
            nudClusterLoop.Location = new Point(148, 27);
            nudClusterLoop.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudClusterLoop.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            nudClusterLoop.Name = "nudClusterLoop";
            nudClusterLoop.Size = new Size(78, 23);
            nudClusterLoop.TabIndex = 6;
            nudClusterLoop.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // btnReduceColorCluster
            // 
            btnReduceColorCluster.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnReduceColorCluster.Location = new Point(6, 58);
            btnReduceColorCluster.Name = "btnReduceColorCluster";
            btnReduceColorCluster.Size = new Size(287, 50);
            btnReduceColorCluster.TabIndex = 5;
            btnReduceColorCluster.Text = "Process";
            btnReduceColorCluster.UseVisualStyleBackColor = true;
            btnReduceColorCluster.Click += btnReduceColorCluster_Click;
            // 
            // gbMeanTransform
            // 
            gbMeanTransform.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbMeanTransform.Controls.Add(btnReduceColors);
            gbMeanTransform.Location = new Point(6, 6);
            gbMeanTransform.Name = "gbMeanTransform";
            gbMeanTransform.Size = new Size(299, 94);
            gbMeanTransform.TabIndex = 23;
            gbMeanTransform.TabStop = false;
            gbMeanTransform.Text = "Color Mean Reduction";
            // 
            // btnReduceColors
            // 
            btnReduceColors.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnReduceColors.Location = new Point(6, 39);
            btnReduceColors.Name = "btnReduceColors";
            btnReduceColors.Size = new Size(287, 49);
            btnReduceColors.TabIndex = 3;
            btnReduceColors.Text = "Process";
            btnReduceColors.UseVisualStyleBackColor = true;
            btnReduceColors.Click += btnReduceColors_Click;
            // 
            // tpMainIndexed
            // 
            tpMainIndexed.Controls.Add(gbVgaTransform);
            tpMainIndexed.Controls.Add(gbEgaTransform);
            tpMainIndexed.Controls.Add(gbCgaTransform);
            tpMainIndexed.Controls.Add(gbHerculeTransform);
            tpMainIndexed.Location = new Point(4, 24);
            tpMainIndexed.Name = "tpMainIndexed";
            tpMainIndexed.Padding = new Padding(3);
            tpMainIndexed.Size = new Size(311, 562);
            tpMainIndexed.TabIndex = 4;
            tpMainIndexed.Text = "Indexed";
            tpMainIndexed.UseVisualStyleBackColor = true;
            // 
            // gbVgaTransform
            // 
            gbVgaTransform.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbVgaTransform.Location = new Point(6, 333);
            gbVgaTransform.Name = "gbVgaTransform";
            gbVgaTransform.Size = new Size(299, 200);
            gbVgaTransform.TabIndex = 19;
            gbVgaTransform.TabStop = false;
            gbVgaTransform.Text = "VGA Videocard";
            // 
            // gbEgaTransform
            // 
            gbEgaTransform.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbEgaTransform.Controls.Add(btnReduceColorsEga);
            gbEgaTransform.Location = new Point(6, 227);
            gbEgaTransform.Name = "gbEgaTransform";
            gbEgaTransform.Size = new Size(299, 200);
            gbEgaTransform.TabIndex = 18;
            gbEgaTransform.TabStop = false;
            gbEgaTransform.Text = "EGA Videocard";
            // 
            // btnReduceColorsEga
            // 
            btnReduceColorsEga.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnReduceColorsEga.Location = new Point(6, 22);
            btnReduceColorsEga.Name = "btnReduceColorsEga";
            btnReduceColorsEga.Size = new Size(287, 48);
            btnReduceColorsEga.TabIndex = 15;
            btnReduceColorsEga.Text = "Process";
            btnReduceColorsEga.UseVisualStyleBackColor = true;
            btnReduceColorsEga.Click += btnReduceColorsEga_Click;
            // 
            // gbCgaTransform
            // 
            gbCgaTransform.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbCgaTransform.Location = new Point(6, 112);
            gbCgaTransform.Name = "gbCgaTransform";
            gbCgaTransform.Size = new Size(299, 200);
            gbCgaTransform.TabIndex = 17;
            gbCgaTransform.TabStop = false;
            gbCgaTransform.Text = "CGA Videocard";
            // 
            // gbHerculeTransform
            // 
            gbHerculeTransform.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbHerculeTransform.Location = new Point(6, 6);
            gbHerculeTransform.Name = "gbHerculeTransform";
            gbHerculeTransform.Size = new Size(299, 200);
            gbHerculeTransform.TabIndex = 16;
            gbHerculeTransform.TabStop = false;
            gbHerculeTransform.Text = "Hercules Videocard";
            // 
            // tpMainCbm
            // 
            tpMainCbm.Controls.Add(tabCbm);
            tpMainCbm.Location = new Point(4, 24);
            tpMainCbm.Name = "tpMainCbm";
            tpMainCbm.Padding = new Padding(3);
            tpMainCbm.Size = new Size(311, 562);
            tpMainCbm.TabIndex = 1;
            tpMainCbm.Text = "Cbm";
            tpMainCbm.UseVisualStyleBackColor = true;
            // 
            // tabCbm
            // 
            tabCbm.Controls.Add(tpCbm64);
            tabCbm.Controls.Add(tpCbmAmiga);
            tabCbm.Dock = DockStyle.Fill;
            tabCbm.Location = new Point(3, 3);
            tabCbm.Name = "tabCbm";
            tabCbm.SelectedIndex = 0;
            tabCbm.Size = new Size(305, 556);
            tabCbm.TabIndex = 0;
            // 
            // tpCbm64
            // 
            tpCbm64.Controls.Add(cbC64VideoMode);
            tpCbm64.Controls.Add(lblC64VideoMode);
            tpCbm64.Controls.Add(btnReduceColorsC64v1);
            tpCbm64.Location = new Point(4, 24);
            tpCbm64.Name = "tpCbm64";
            tpCbm64.Padding = new Padding(3);
            tpCbm64.Size = new Size(297, 528);
            tpCbm64.TabIndex = 0;
            tpCbm64.Text = "C64";
            tpCbm64.UseVisualStyleBackColor = true;
            // 
            // cbC64VideoMode
            // 
            cbC64VideoMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cbC64VideoMode.FormattingEnabled = true;
            cbC64VideoMode.Location = new Point(98, 22);
            cbC64VideoMode.Name = "cbC64VideoMode";
            cbC64VideoMode.Size = new Size(193, 23);
            cbC64VideoMode.TabIndex = 19;
            // 
            // lblC64VideoMode
            // 
            lblC64VideoMode.Location = new Point(9, 25);
            lblC64VideoMode.Name = "lblC64VideoMode";
            lblC64VideoMode.Size = new Size(83, 21);
            lblC64VideoMode.TabIndex = 23;
            lblC64VideoMode.Text = "Video Mode";
            // 
            // btnReduceColorsC64v1
            // 
            btnReduceColorsC64v1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnReduceColorsC64v1.Location = new Point(9, 66);
            btnReduceColorsC64v1.Name = "btnReduceColorsC64v1";
            btnReduceColorsC64v1.Size = new Size(282, 48);
            btnReduceColorsC64v1.TabIndex = 18;
            btnReduceColorsC64v1.Text = "Process";
            btnReduceColorsC64v1.UseVisualStyleBackColor = true;
            btnReduceColorsC64v1.Click += BtnReduceColorsC64v1_Click;
            // 
            // tpCbmAmiga
            // 
            tpCbmAmiga.Controls.Add(gcHamTransform);
            tpCbmAmiga.Controls.Add(gcScanlineTransform);
            tpCbmAmiga.Location = new Point(4, 24);
            tpCbmAmiga.Name = "tpCbmAmiga";
            tpCbmAmiga.Padding = new Padding(3);
            tpCbmAmiga.Size = new Size(297, 528);
            tpCbmAmiga.TabIndex = 1;
            tpCbmAmiga.Text = "Amiga";
            tpCbmAmiga.UseVisualStyleBackColor = true;
            // 
            // gcHamTransform
            // 
            gcHamTransform.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gcHamTransform.Controls.Add(btnReduceAmiga);
            gcHamTransform.Controls.Add(label8);
            gcHamTransform.Controls.Add(cbAmigaVideoMode);
            gcHamTransform.Location = new Point(10, 214);
            gcHamTransform.Name = "gcHamTransform";
            gcHamTransform.Size = new Size(281, 127);
            gcHamTransform.TabIndex = 27;
            gcHamTransform.TabStop = false;
            gcHamTransform.Text = "Amiga Modes Transform";
            // 
            // btnReduceAmiga
            // 
            btnReduceAmiga.Location = new Point(6, 73);
            btnReduceAmiga.Name = "btnReduceAmiga";
            btnReduceAmiga.Size = new Size(271, 48);
            btnReduceAmiga.TabIndex = 18;
            btnReduceAmiga.Text = "Process";
            btnReduceAmiga.UseVisualStyleBackColor = true;
            btnReduceAmiga.Click += btnReduceHam_Click;
            // 
            // label8
            // 
            label8.Location = new Point(6, 36);
            label8.Name = "label8";
            label8.Size = new Size(83, 21);
            label8.TabIndex = 23;
            label8.Text = "Video Mode";
            // 
            // cbAmigaVideoMode
            // 
            cbAmigaVideoMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cbAmigaVideoMode.FormattingEnabled = true;
            cbAmigaVideoMode.Location = new Point(125, 33);
            cbAmigaVideoMode.Name = "cbAmigaVideoMode";
            cbAmigaVideoMode.Size = new Size(101, 23);
            cbAmigaVideoMode.TabIndex = 19;
            // 
            // gcScanlineTransform
            // 
            gcScanlineTransform.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gcScanlineTransform.Controls.Add(numLinesPerBlock);
            gcScanlineTransform.Controls.Add(label12);
            gcScanlineTransform.Controls.Add(btnReduceColorsScanline);
            gcScanlineTransform.Controls.Add(chkScanlineSharedPal);
            gcScanlineTransform.Controls.Add(label9);
            gcScanlineTransform.Controls.Add(nudScanlineTrainEpochs);
            gcScanlineTransform.Controls.Add(label7);
            gcScanlineTransform.Controls.Add(nudScanlineLineColors);
            gcScanlineTransform.Location = new Point(10, 22);
            gcScanlineTransform.Name = "gcScanlineTransform";
            gcScanlineTransform.Size = new Size(281, 186);
            gcScanlineTransform.TabIndex = 26;
            gcScanlineTransform.TabStop = false;
            gcScanlineTransform.Text = "Scanline for Copper";
            // 
            // numLinesPerBlock
            // 
            numLinesPerBlock.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            numLinesPerBlock.Location = new Point(125, 44);
            numLinesPerBlock.Maximum = new decimal(new int[] { 768, 0, 0, 0 });
            numLinesPerBlock.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numLinesPerBlock.Name = "numLinesPerBlock";
            numLinesPerBlock.Size = new Size(65, 23);
            numLinesPerBlock.TabIndex = 27;
            numLinesPerBlock.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label12
            // 
            label12.Location = new Point(6, 46);
            label12.Name = "label12";
            label12.Size = new Size(113, 18);
            label12.TabIndex = 26;
            label12.Text = "Lines per block";
            // 
            // btnReduceColorsScanline
            // 
            btnReduceColorsScanline.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnReduceColorsScanline.Location = new Point(6, 132);
            btnReduceColorsScanline.Name = "btnReduceColorsScanline";
            btnReduceColorsScanline.Size = new Size(269, 48);
            btnReduceColorsScanline.TabIndex = 9;
            btnReduceColorsScanline.Text = "Process";
            btnReduceColorsScanline.UseVisualStyleBackColor = true;
            btnReduceColorsScanline.Click += btnReduceColorsScanline_Click;
            // 
            // chkScanlineSharedPal
            // 
            chkScanlineSharedPal.Checked = true;
            chkScanlineSharedPal.CheckState = CheckState.Checked;
            chkScanlineSharedPal.Location = new Point(6, 22);
            chkScanlineSharedPal.Name = "chkScanlineSharedPal";
            chkScanlineSharedPal.Size = new Size(269, 21);
            chkScanlineSharedPal.TabIndex = 24;
            chkScanlineSharedPal.Text = "Use share palette";
            chkScanlineSharedPal.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            label9.Location = new Point(6, 77);
            label9.Name = "label9";
            label9.Size = new Size(113, 18);
            label9.TabIndex = 25;
            label9.Text = "Max Colors per line";
            // 
            // nudScanlineTrainEpochs
            // 
            nudScanlineTrainEpochs.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudScanlineTrainEpochs.Location = new Point(125, 104);
            nudScanlineTrainEpochs.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudScanlineTrainEpochs.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            nudScanlineTrainEpochs.Name = "nudScanlineTrainEpochs";
            nudScanlineTrainEpochs.Size = new Size(65, 23);
            nudScanlineTrainEpochs.TabIndex = 21;
            nudScanlineTrainEpochs.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label7
            // 
            label7.Location = new Point(6, 106);
            label7.Name = "label7";
            label7.Size = new Size(106, 18);
            label7.TabIndex = 22;
            label7.Text = "Training Epochs";
            // 
            // nudScanlineLineColors
            // 
            nudScanlineLineColors.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudScanlineLineColors.Location = new Point(125, 72);
            nudScanlineLineColors.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudScanlineLineColors.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudScanlineLineColors.Name = "nudScanlineLineColors";
            nudScanlineLineColors.Size = new Size(65, 23);
            nudScanlineLineColors.TabIndex = 23;
            nudScanlineLineColors.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // tpMainSinclair
            // 
            tpMainSinclair.Controls.Add(label11);
            tpMainSinclair.Controls.Add(cbZxAutotuneMode);
            tpMainSinclair.Controls.Add(cbZxPaletteMode);
            tpMainSinclair.Controls.Add(label10);
            tpMainSinclair.Controls.Add(chkZxDitherImageLo);
            tpMainSinclair.Controls.Add(zxBtnReset);
            tpMainSinclair.Controls.Add(chkZxDitherHI);
            tpMainSinclair.Controls.Add(btnReduceColorsZx);
            tpMainSinclair.Controls.Add(chkZxBlackHI);
            tpMainSinclair.Controls.Add(label1);
            tpMainSinclair.Controls.Add(nudZxColorHI);
            tpMainSinclair.Controls.Add(label4);
            tpMainSinclair.Controls.Add(nudZxColorLO);
            tpMainSinclair.Location = new Point(4, 24);
            tpMainSinclair.Name = "tpMainSinclair";
            tpMainSinclair.Padding = new Padding(3);
            tpMainSinclair.Size = new Size(311, 562);
            tpMainSinclair.TabIndex = 2;
            tpMainSinclair.Text = "Sinclair";
            tpMainSinclair.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            label11.Location = new Point(13, 151);
            label11.Name = "label11";
            label11.Size = new Size(106, 21);
            label11.TabIndex = 28;
            label11.Text = "Autotune Mode";
            // 
            // cbZxAutotuneMode
            // 
            cbZxAutotuneMode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cbZxAutotuneMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cbZxAutotuneMode.FormattingEnabled = true;
            cbZxAutotuneMode.Location = new Point(125, 148);
            cbZxAutotuneMode.Name = "cbZxAutotuneMode";
            cbZxAutotuneMode.Size = new Size(174, 23);
            cbZxAutotuneMode.TabIndex = 27;
            // 
            // cbZxPaletteMode
            // 
            cbZxPaletteMode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cbZxPaletteMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cbZxPaletteMode.FormattingEnabled = true;
            cbZxPaletteMode.Location = new Point(125, 9);
            cbZxPaletteMode.Name = "cbZxPaletteMode";
            cbZxPaletteMode.Size = new Size(174, 23);
            cbZxPaletteMode.TabIndex = 25;
            // 
            // label10
            // 
            label10.Location = new Point(10, 12);
            label10.Name = "label10";
            label10.Size = new Size(83, 21);
            label10.TabIndex = 26;
            label10.Text = "Palette Mode";
            // 
            // chkZxDitherImageLo
            // 
            chkZxDitherImageLo.AutoSize = true;
            chkZxDitherImageLo.Checked = true;
            chkZxDitherImageLo.CheckState = CheckState.Checked;
            chkZxDitherImageLo.Location = new Point(13, 182);
            chkZxDitherImageLo.Name = "chkZxDitherImageLo";
            chkZxDitherImageLo.Size = new Size(112, 19);
            chkZxDitherImageLo.TabIndex = 24;
            chkZxDitherImageLo.Text = "Dither Image LO";
            chkZxDitherImageLo.UseVisualStyleBackColor = true;
            // 
            // zxBtnReset
            // 
            zxBtnReset.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            zxBtnReset.Location = new Point(13, 97);
            zxBtnReset.Name = "zxBtnReset";
            zxBtnReset.Size = new Size(286, 34);
            zxBtnReset.TabIndex = 23;
            zxBtnReset.Text = "Reset Seed Color";
            zxBtnReset.UseVisualStyleBackColor = true;
            zxBtnReset.Click += zxBtnReset_Click;
            // 
            // chkZxDitherHI
            // 
            chkZxDitherHI.AutoSize = true;
            chkZxDitherHI.Checked = true;
            chkZxDitherHI.CheckState = CheckState.Checked;
            chkZxDitherHI.Location = new Point(13, 207);
            chkZxDitherHI.Name = "chkZxDitherHI";
            chkZxDitherHI.Size = new Size(109, 19);
            chkZxDitherHI.TabIndex = 21;
            chkZxDitherHI.Text = "Dither Image HI";
            chkZxDitherHI.UseVisualStyleBackColor = true;
            // 
            // btnReduceColorsZx
            // 
            btnReduceColorsZx.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnReduceColorsZx.Location = new Point(10, 257);
            btnReduceColorsZx.Name = "btnReduceColorsZx";
            btnReduceColorsZx.Size = new Size(289, 48);
            btnReduceColorsZx.TabIndex = 14;
            btnReduceColorsZx.Text = "Process";
            btnReduceColorsZx.UseVisualStyleBackColor = true;
            btnReduceColorsZx.Click += btnReduceColorsZx_Click;
            // 
            // chkZxBlackHI
            // 
            chkZxBlackHI.AutoSize = true;
            chkZxBlackHI.Checked = true;
            chkZxBlackHI.CheckState = CheckState.Checked;
            chkZxBlackHI.Location = new Point(13, 232);
            chkZxBlackHI.Name = "chkZxBlackHI";
            chkZxBlackHI.Size = new Size(143, 19);
            chkZxBlackHI.TabIndex = 20;
            chkZxBlackHI.Text = "Use Black in HI palette";
            chkZxBlackHI.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 40);
            label1.Name = "label1";
            label1.Size = new Size(51, 15);
            label1.TabIndex = 18;
            label1.Text = "ColorLO";
            // 
            // nudZxColorHI
            // 
            nudZxColorHI.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            nudZxColorHI.Location = new Point(125, 67);
            nudZxColorHI.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudZxColorHI.Name = "nudZxColorHI";
            nudZxColorHI.Size = new Size(101, 23);
            nudZxColorHI.TabIndex = 17;
            nudZxColorHI.Value = new decimal(new int[] { 255, 0, 0, 0 });
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(13, 70);
            label4.Name = "label4";
            label4.Size = new Size(48, 15);
            label4.TabIndex = 19;
            label4.Text = "ColorHI";
            // 
            // nudZxColorLO
            // 
            nudZxColorLO.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            nudZxColorLO.Location = new Point(125, 38);
            nudZxColorLO.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudZxColorLO.Name = "nudZxColorLO";
            nudZxColorLO.Size = new Size(100, 23);
            nudZxColorLO.TabIndex = 16;
            nudZxColorLO.Value = new decimal(new int[] { 160, 0, 0, 0 });
            // 
            // tpMainAmstrad
            // 
            tpMainAmstrad.Controls.Add(cbCpcVideoMode);
            tpMainAmstrad.Controls.Add(lblCpcVideoMode);
            tpMainAmstrad.Controls.Add(btnReduceColorCPC);
            tpMainAmstrad.Location = new Point(4, 24);
            tpMainAmstrad.Name = "tpMainAmstrad";
            tpMainAmstrad.Padding = new Padding(3);
            tpMainAmstrad.Size = new Size(311, 562);
            tpMainAmstrad.TabIndex = 3;
            tpMainAmstrad.Text = "Amstrad";
            tpMainAmstrad.UseVisualStyleBackColor = true;
            // 
            // cbCpcVideoMode
            // 
            cbCpcVideoMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cbCpcVideoMode.FormattingEnabled = true;
            cbCpcVideoMode.Location = new Point(100, 28);
            cbCpcVideoMode.Name = "cbCpcVideoMode";
            cbCpcVideoMode.Size = new Size(101, 23);
            cbCpcVideoMode.TabIndex = 19;
            // 
            // lblCpcVideoMode
            // 
            lblCpcVideoMode.Location = new Point(11, 31);
            lblCpcVideoMode.Name = "lblCpcVideoMode";
            lblCpcVideoMode.Size = new Size(83, 21);
            lblCpcVideoMode.TabIndex = 23;
            lblCpcVideoMode.Text = "Video Mode";
            // 
            // btnReduceColorCPC
            // 
            btnReduceColorCPC.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnReduceColorCPC.Location = new Point(8, 84);
            btnReduceColorCPC.Name = "btnReduceColorCPC";
            btnReduceColorCPC.Size = new Size(299, 48);
            btnReduceColorCPC.TabIndex = 18;
            btnReduceColorCPC.Text = "Process";
            btnReduceColorCPC.UseVisualStyleBackColor = true;
            btnReduceColorCPC.Click += btnReduceColorCPC_Click;
            // 
            // splitMainB_C
            // 
            splitMainB_C.Dock = DockStyle.Fill;
            splitMainB_C.Location = new Point(0, 0);
            splitMainB_C.Name = "splitMainB_C";
            // 
            // splitMainB_C.Panel1
            // 
            splitMainB_C.Panel1.Controls.Add(layoutRender);
            // 
            // splitMainB_C.Panel2
            // 
            splitMainB_C.Panel2.Controls.Add(splitProcessing);
            splitMainB_C.Size = new Size(793, 753);
            splitMainB_C.SplitterDistance = 603;
            splitMainB_C.TabIndex = 0;
            // 
            // layoutRender
            // 
            layoutRender.ColumnCount = 1;
            layoutRender.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutRender.Controls.Add(ssImageData, 0, 1);
            layoutRender.Controls.Add(splitRenderLayout, 0, 0);
            layoutRender.Dock = DockStyle.Fill;
            layoutRender.Location = new Point(0, 0);
            layoutRender.Name = "layoutRender";
            layoutRender.RowCount = 2;
            layoutRender.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutRender.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            layoutRender.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            layoutRender.Size = new Size(603, 753);
            layoutRender.TabIndex = 5;
            // 
            // ssImageData
            // 
            ssImageData.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ssImageData.Dock = DockStyle.None;
            ssImageData.Items.AddRange(new ToolStripItem[] { tsLayout, tsImageLabel, tsImageCoord, tsMouseLabel, tsMouseCoord, tsColorLabel, tsColorData, tsProgressLabel, tsProcessingProgress });
            ssImageData.Location = new Point(0, 728);
            ssImageData.Name = "ssImageData";
            ssImageData.Size = new Size(603, 22);
            ssImageData.TabIndex = 1;
            ssImageData.Text = "statusStrip1";
            // 
            // tsLayout
            // 
            tsLayout.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsLayout.DropDownItems.AddRange(new ToolStripItem[] { horizontalToolStripMenuItem, verticalToolStripMenuItem });
            tsLayout.Image = (Image)resources.GetObject("tsLayout.Image");
            tsLayout.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsLayout.Name = "tsLayout";
            tsLayout.Size = new Size(88, 20);
            tsLayout.Text = "Panel Layout";
            // 
            // horizontalToolStripMenuItem
            // 
            horizontalToolStripMenuItem.Name = "horizontalToolStripMenuItem";
            horizontalToolStripMenuItem.Size = new Size(129, 22);
            horizontalToolStripMenuItem.Text = "Horizontal";
            horizontalToolStripMenuItem.Click += horizontalToolStripMenuItem_Click;
            // 
            // verticalToolStripMenuItem
            // 
            verticalToolStripMenuItem.Checked = true;
            verticalToolStripMenuItem.CheckState = CheckState.Checked;
            verticalToolStripMenuItem.Name = "verticalToolStripMenuItem";
            verticalToolStripMenuItem.Size = new Size(129, 22);
            verticalToolStripMenuItem.Text = "Vertical";
            verticalToolStripMenuItem.Click += verticalToolStripMenuItem_Click;
            // 
            // tsImageLabel
            // 
            tsImageLabel.Name = "tsImageLabel";
            tsImageLabel.Size = new Size(81, 17);
            tsImageLabel.Text = "Image Coords";
            // 
            // tsImageCoord
            // 
            tsImageCoord.AutoSize = false;
            tsImageCoord.Name = "tsImageCoord";
            tsImageCoord.Size = new Size(80, 17);
            tsImageCoord.Text = "(:)";
            // 
            // tsMouseLabel
            // 
            tsMouseLabel.Name = "tsMouseLabel";
            tsMouseLabel.Size = new Size(88, 17);
            tsMouseLabel.Text = "Control Coords";
            // 
            // tsMouseCoord
            // 
            tsMouseCoord.AutoSize = false;
            tsMouseCoord.Name = "tsMouseCoord";
            tsMouseCoord.Size = new Size(80, 17);
            tsMouseCoord.Text = "(:)";
            // 
            // tsColorLabel
            // 
            tsColorLabel.Name = "tsColorLabel";
            tsColorLabel.Size = new Size(36, 17);
            tsColorLabel.Text = "Color";
            // 
            // tsColorData
            // 
            tsColorData.AutoSize = false;
            tsColorData.BackColor = System.Drawing.Color.Black;
            tsColorData.Name = "tsColorData";
            tsColorData.Size = new Size(22, 17);
            // 
            // tsProgressLabel
            // 
            tsProgressLabel.Name = "tsProgressLabel";
            tsProgressLabel.Size = new Size(52, 17);
            tsProgressLabel.Text = "Progress";
            // 
            // tsProcessingProgress
            // 
            tsProcessingProgress.Name = "tsProcessingProgress";
            tsProcessingProgress.Size = new Size(100, 16);
            // 
            // splitRenderLayout
            // 
            splitRenderLayout.Dock = DockStyle.Fill;
            splitRenderLayout.Location = new Point(3, 3);
            splitRenderLayout.Name = "splitRenderLayout";
            // 
            // splitRenderLayout.Panel1
            // 
            splitRenderLayout.Panel1.Controls.Add(gbImageSource);
            // 
            // splitRenderLayout.Panel2
            // 
            splitRenderLayout.Panel2.Controls.Add(gbImageProcessed);
            splitRenderLayout.Size = new Size(597, 722);
            splitRenderLayout.SplitterDistance = 306;
            splitRenderLayout.TabIndex = 0;
            // 
            // gbImageSource
            // 
            gbImageSource.Controls.Add(pictureBoxSrc);
            gbImageSource.Dock = DockStyle.Fill;
            gbImageSource.Location = new Point(0, 0);
            gbImageSource.Margin = new Padding(0);
            gbImageSource.Name = "gbImageSource";
            gbImageSource.Padding = new Padding(0);
            gbImageSource.Size = new Size(306, 722);
            gbImageSource.TabIndex = 0;
            gbImageSource.TabStop = false;
            gbImageSource.Text = "Source Image";
            // 
            // pictureBoxSrc
            // 
            pictureBoxSrc.BackColor = System.Drawing.Color.Black;
            pictureBoxSrc.Dock = DockStyle.Fill;
            pictureBoxSrc.Location = new Point(0, 16);
            pictureBoxSrc.Margin = new Padding(0);
            pictureBoxSrc.Name = "pictureBoxSrc";
            pictureBoxSrc.Size = new Size(306, 706);
            pictureBoxSrc.TabIndex = 0;
            pictureBoxSrc.TabStop = false;
            // 
            // gbImageProcessed
            // 
            gbImageProcessed.Controls.Add(pictureBoxProc);
            gbImageProcessed.Dock = DockStyle.Fill;
            gbImageProcessed.Location = new Point(0, 0);
            gbImageProcessed.Margin = new Padding(0);
            gbImageProcessed.Name = "gbImageProcessed";
            gbImageProcessed.Padding = new Padding(0);
            gbImageProcessed.Size = new Size(287, 722);
            gbImageProcessed.TabIndex = 0;
            gbImageProcessed.TabStop = false;
            gbImageProcessed.Text = "Processed Image";
            // 
            // pictureBoxProc
            // 
            pictureBoxProc.BackColor = System.Drawing.Color.Black;
            pictureBoxProc.Dock = DockStyle.Fill;
            pictureBoxProc.Location = new Point(0, 16);
            pictureBoxProc.Margin = new Padding(0);
            pictureBoxProc.Name = "pictureBoxProc";
            pictureBoxProc.Size = new Size(287, 706);
            pictureBoxProc.TabIndex = 0;
            pictureBoxProc.TabStop = false;
            // 
            // splitProcessing
            // 
            splitProcessing.Dock = DockStyle.Fill;
            splitProcessing.Location = new Point(0, 0);
            splitProcessing.Name = "splitProcessing";
            splitProcessing.Orientation = Orientation.Horizontal;
            // 
            // splitProcessing.Panel1
            // 
            splitProcessing.Panel1.Controls.Add(gbColorProcessor);
            // 
            // splitProcessing.Panel2
            // 
            splitProcessing.Panel2.Controls.Add(gbConfig);
            splitProcessing.Size = new Size(186, 753);
            splitProcessing.SplitterDistance = 376;
            splitProcessing.TabIndex = 2;
            // 
            // gbColorProcessor
            // 
            gbColorProcessor.Controls.Add(pgColorProcessor);
            gbColorProcessor.Dock = DockStyle.Fill;
            gbColorProcessor.Location = new Point(0, 0);
            gbColorProcessor.Name = "gbColorProcessor";
            gbColorProcessor.Size = new Size(186, 376);
            gbColorProcessor.TabIndex = 0;
            gbColorProcessor.TabStop = false;
            gbColorProcessor.Text = "Results";
            // 
            // pgColorProcessor
            // 
            pgColorProcessor.BackColor = SystemColors.Control;
            pgColorProcessor.Dock = DockStyle.Fill;
            pgColorProcessor.Location = new Point(3, 19);
            pgColorProcessor.Name = "pgColorProcessor";
            pgColorProcessor.Size = new Size(180, 354);
            pgColorProcessor.TabIndex = 0;
            // 
            // gbConfig
            // 
            gbConfig.Controls.Add(pgColorProcessorConfig);
            gbConfig.Dock = DockStyle.Fill;
            gbConfig.Location = new Point(0, 0);
            gbConfig.Name = "gbConfig";
            gbConfig.Size = new Size(186, 373);
            gbConfig.TabIndex = 1;
            gbConfig.TabStop = false;
            gbConfig.Text = "Configuration-Debug";
            // 
            // pgColorProcessorConfig
            // 
            pgColorProcessorConfig.BackColor = SystemColors.Control;
            pgColorProcessorConfig.Dock = DockStyle.Fill;
            pgColorProcessorConfig.Location = new Point(3, 19);
            pgColorProcessorConfig.Name = "pgColorProcessorConfig";
            pgColorProcessorConfig.Size = new Size(180, 351);
            pgColorProcessorConfig.TabIndex = 0;
            // 
            // oBitmapRenderSource
            // 
            oBitmapRenderSource.Control = pictureBoxSrc;
            oBitmapRenderSource.ImageZoomMode = Components.BitmapRender.EnumZoom.Zoom1;
            oBitmapRenderSource.MouseMove += oBitmapRenderSource_MouseMove;
            // 
            // oBitmapRenderDest
            // 
            oBitmapRenderDest.Control = pictureBoxProc;
            oBitmapRenderDest.ImageZoomMode = Components.BitmapRender.EnumZoom.Zoom1;
            // 
            // ColorAnalyzer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitMainA_BC);
            Name = "ColorAnalyzer";
            Size = new Size(1134, 753);
            splitMainA_BC.Panel1.ResumeLayout(false);
            splitMainA_BC.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMainA_BC).EndInit();
            splitMainA_BC.ResumeLayout(false);
            layoutInput.ResumeLayout(false);
            gbSetup.ResumeLayout(false);
            layoutConfig.ResumeLayout(false);
            panelSelectColors.ResumeLayout(false);
            panelSelectColors.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudColorsWanted).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbBkColor).EndInit();
            panelDithering.ResumeLayout(false);
            panelDithering.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudDitheringStrenght).EndInit();
            ((System.ComponentModel.ISupportInitialize)tbDitherStrenght).EndInit();
            tabControlMain.ResumeLayout(false);
            tpMainGeneric.ResumeLayout(false);
            gbHsvTransform.ResumeLayout(false);
            gbHsvTransform.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudHue).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudBright).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSat).EndInit();
            gbClusteringTransform.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudClusterLoop).EndInit();
            gbMeanTransform.ResumeLayout(false);
            tpMainIndexed.ResumeLayout(false);
            gbEgaTransform.ResumeLayout(false);
            tpMainCbm.ResumeLayout(false);
            tabCbm.ResumeLayout(false);
            tpCbm64.ResumeLayout(false);
            tpCbmAmiga.ResumeLayout(false);
            gcHamTransform.ResumeLayout(false);
            gcScanlineTransform.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numLinesPerBlock).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudScanlineTrainEpochs).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudScanlineLineColors).EndInit();
            tpMainSinclair.ResumeLayout(false);
            tpMainSinclair.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudZxColorHI).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudZxColorLO).EndInit();
            tpMainAmstrad.ResumeLayout(false);
            splitMainB_C.Panel1.ResumeLayout(false);
            splitMainB_C.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMainB_C).EndInit();
            splitMainB_C.ResumeLayout(false);
            layoutRender.ResumeLayout(false);
            layoutRender.PerformLayout();
            ssImageData.ResumeLayout(false);
            ssImageData.PerformLayout();
            splitRenderLayout.Panel1.ResumeLayout(false);
            splitRenderLayout.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitRenderLayout).EndInit();
            splitRenderLayout.ResumeLayout(false);
            gbImageSource.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxSrc).EndInit();
            gbImageProcessed.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxProc).EndInit();
            splitProcessing.Panel1.ResumeLayout(false);
            splitProcessing.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitProcessing).EndInit();
            splitProcessing.ResumeLayout(false);
            gbColorProcessor.ResumeLayout(false);
            gbConfig.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private SplitContainer splitMainA_BC;
        private PictureBox pictureBoxSrc;
        private Components.BitmapRender oBitmapRenderSource;
        private Components.ColorManager oColorManager;
        private SplitContainer splitRenderLayout;
        private PictureBox pictureBoxProc;
        private Components.BitmapRender oBitmapRenderDest;
        private TableLayoutPanel layoutConfig;
        private Panel panelSelectColors;
        private NumericUpDown nudColorsWanted;
        private Label label2;
        private PictureBox pbBkColor;
        private Label label3;
        private Panel panelDithering;
        private NumericUpDown nudDitheringStrenght;
        private TrackBar tbDitherStrenght;
        private Label label5;
        private StatusStrip ssImageData;
        private ToolStripStatusLabel tsImageLabel;
        private ToolStripStatusLabel tsImageCoord;
        private ToolStripStatusLabel tsMouseLabel;
        private ToolStripStatusLabel tsMouseCoord;
        private ToolStripStatusLabel tsColorLabel;
        private ToolStripStatusLabel tsColorData;
        private CheckBox chkShowTileBorders;
        private SplitContainer splitMainB_C;
        private GroupBox gbSetup;
        private TableLayoutPanel layoutInput;
        private TableLayoutPanel layoutProcessing;
        private GroupBox gbColorProcessor;
        private PropertyGrid pgColorProcessor;
        private GroupBox gbConfig;
        private PropertyGrid pgColorProcessorConfig;
        private TableLayoutPanel layoutRender;
        private TabControl tabControlMain;
        private TabPage tpMainGeneric;
        private NumericUpDown nudBright;
        private NumericUpDown nudHue;
        private Label lblBright;
        private Label lblHue;
        private NumericUpDown nudSat;
        private Label label6;
        private Label lblSat;
        private Button btnReduceColorCluster;
        private NumericUpDown nudClusterLoop;
        private Button btnReduceColors;
        private Button btnChromaAdapt;
        private TabPage tpMainIndexed;
        private Button btnReduceColorsEga;
        private TabPage tpMainCbm;
        private TabControl tabCbm;
        private TabPage tpCbm64;
        private ComboBox cbC64VideoMode;
        private Label lblC64VideoMode;
        private Button btnReduceColorsC64v1;
        private TabPage tpCbmAmiga;
        private Button btnReduceAmiga;
        private ComboBox cbAmigaVideoMode;
        private Label label8;
        private Label label9;
        private NumericUpDown nudScanlineTrainEpochs;
        private NumericUpDown nudScanlineLineColors;
        private Label label7;
        private CheckBox chkScanlineSharedPal;
        private Button btnReduceColorsScanline;
        private TabPage tpMainSinclair;
        private Label label11;
        private ComboBox cbZxAutotuneMode;
        private ComboBox cbZxPaletteMode;
        private Label label10;
        private CheckBox chkZxDitherImageLo;
        private Button zxBtnReset;
        private CheckBox chkZxDitherHI;
        private Button btnReduceColorsZx;
        private CheckBox chkZxBlackHI;
        private Label label1;
        private NumericUpDown nudZxColorHI;
        private Label label4;
        private NumericUpDown nudZxColorLO;
        private TabPage tpMainAmstrad;
        private ComboBox cbCpcVideoMode;
        private Label lblCpcVideoMode;
        private Button btnReduceColorCPC;
        private GroupBox gbImageSource;
        private GroupBox gbImageProcessed;
        private ToolStripStatusLabel tsProgressLabel;
        private ToolStripProgressBar tsProcessingProgress;
        private ToolStripDropDownButton tsLayout;
        private ToolStripMenuItem horizontalToolStripMenuItem;
        private ToolStripMenuItem verticalToolStripMenuItem;
        private GroupBox gbClusteringTransform;
        private GroupBox gbMeanTransform;
        private GroupBox gbHsvTransform;
        private GroupBox gbVgaTransform;
        private GroupBox gbEgaTransform;
        private GroupBox gbCgaTransform;
        private GroupBox gbHerculeTransform;
        private GroupBox gcHamTransform;
        private GroupBox gcScanlineTransform;
        private SplitContainer splitProcessing;
        private NumericUpDown numLinesPerBlock;
        private Label label12;
    }
}
