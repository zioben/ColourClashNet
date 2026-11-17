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
            pbIN = new PictureBox();
            pbOUT = new PictureBox();
            propertyGrid1 = new PropertyGrid();
            propertyGrid2 = new PropertyGrid();
            tableLayoutPanel3 = new TableLayoutPanel();
            label9 = new Label();
            label2 = new Label();
            cbPreset = new ComboBox();
            btnLoad = new Button();
            btnSave = new Button();
            label1 = new Label();
            cbColorMode = new ComboBox();
            label6 = new Label();
            label5 = new Label();
            numericUpDown1 = new NumericUpDown();
            btnQuantizer = new Button();
            cbDithering = new ComboBox();
            label3 = new Label();
            label4 = new Label();
            numDitheringStrenght = new NumericUpDown();
            btnBkgRem = new Button();
            btnID = new Button();
            btnReprocess = new Button();
            cbQuantization = new ComboBox();
            cbCgaVideoMode = new ComboBox();
            btnCGA = new Button();
            btnVGA = new Button();
            cbZXAutotuneMode = new ComboBox();
            cbZXPaletteMode = new ComboBox();
            btnSpeccy = new Button();
            btnEGA = new Button();
            cbCPCVideoMode = new ComboBox();
            btnAmstrad = new Button();
            cbC64VideoMode = new ComboBox();
            btnC64 = new Button();
            cbAmigaMode = new ComboBox();
            btnAmiga = new Button();
            btnLumSat = new Button();
            btnPalette = new Button();
            btnCluster = new Button();
            btnMedian = new Button();
            btnFast = new Button();
            label7 = new Label();
            label8 = new Label();
            numLumH = new NumericUpDown();
            numLumS = new NumericUpDown();
            numLumV = new NumericUpDown();
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
            ((System.ComponentModel.ISupportInitialize)pbIN).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbOUT).BeginInit();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDitheringStrenght).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numLumH).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numLumS).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numLumV).BeginInit();
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
            splitContainer1.SplitterDistance = 471;
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
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 134F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1164, 471);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 4;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
            tableLayoutPanel2.Controls.Add(pbIN, 0, 0);
            tableLayoutPanel2.Controls.Add(pbOUT, 1, 0);
            tableLayoutPanel2.Controls.Add(propertyGrid1, 2, 0);
            tableLayoutPanel2.Controls.Add(propertyGrid2, 3, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 137);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(1158, 331);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // pbIN
            // 
            pbIN.BackColor = Color.Black;
            pbIN.Dock = DockStyle.Fill;
            pbIN.Location = new Point(3, 3);
            pbIN.Name = "pbIN";
            pbIN.Size = new Size(273, 325);
            pbIN.TabIndex = 0;
            pbIN.TabStop = false;
            // 
            // pbOUT
            // 
            pbOUT.BackColor = Color.Black;
            pbOUT.Dock = DockStyle.Fill;
            pbOUT.Location = new Point(282, 3);
            pbOUT.Name = "pbOUT";
            pbOUT.Size = new Size(273, 325);
            pbOUT.TabIndex = 1;
            pbOUT.TabStop = false;
            // 
            // propertyGrid1
            // 
            propertyGrid1.BackColor = SystemColors.Control;
            propertyGrid1.Dock = DockStyle.Fill;
            propertyGrid1.Location = new Point(561, 3);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.Size = new Size(294, 325);
            propertyGrid1.TabIndex = 2;
            // 
            // propertyGrid2
            // 
            propertyGrid2.BackColor = SystemColors.Control;
            propertyGrid2.Dock = DockStyle.Fill;
            propertyGrid2.Location = new Point(861, 3);
            propertyGrid2.Name = "propertyGrid2";
            propertyGrid2.Size = new Size(294, 325);
            propertyGrid2.TabIndex = 3;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 13;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.69230843F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.692309F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.692309F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.692309F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.692309F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.692309F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.692309F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.692309F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.692309F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.692309F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.692309F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.692309F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.692309F));
            tableLayoutPanel3.Controls.Add(label9, 6, 1);
            tableLayoutPanel3.Controls.Add(label2, 1, 0);
            tableLayoutPanel3.Controls.Add(cbPreset, 0, 1);
            tableLayoutPanel3.Controls.Add(btnLoad, 0, 2);
            tableLayoutPanel3.Controls.Add(btnSave, 0, 3);
            tableLayoutPanel3.Controls.Add(label1, 0, 0);
            tableLayoutPanel3.Controls.Add(cbColorMode, 2, 0);
            tableLayoutPanel3.Controls.Add(label6, 5, 0);
            tableLayoutPanel3.Controls.Add(label5, 1, 1);
            tableLayoutPanel3.Controls.Add(numericUpDown1, 2, 1);
            tableLayoutPanel3.Controls.Add(btnQuantizer, 5, 2);
            tableLayoutPanel3.Controls.Add(cbDithering, 4, 0);
            tableLayoutPanel3.Controls.Add(label3, 3, 0);
            tableLayoutPanel3.Controls.Add(label4, 3, 1);
            tableLayoutPanel3.Controls.Add(numDitheringStrenght, 4, 1);
            tableLayoutPanel3.Controls.Add(btnBkgRem, 4, 3);
            tableLayoutPanel3.Controls.Add(btnID, 2, 3);
            tableLayoutPanel3.Controls.Add(btnReprocess, 1, 3);
            tableLayoutPanel3.Controls.Add(cbQuantization, 5, 1);
            tableLayoutPanel3.Controls.Add(cbCgaVideoMode, 12, 0);
            tableLayoutPanel3.Controls.Add(btnCGA, 12, 1);
            tableLayoutPanel3.Controls.Add(btnVGA, 12, 3);
            tableLayoutPanel3.Controls.Add(cbZXAutotuneMode, 11, 0);
            tableLayoutPanel3.Controls.Add(cbZXPaletteMode, 11, 1);
            tableLayoutPanel3.Controls.Add(btnSpeccy, 11, 2);
            tableLayoutPanel3.Controls.Add(btnEGA, 12, 2);
            tableLayoutPanel3.Controls.Add(cbCPCVideoMode, 10, 0);
            tableLayoutPanel3.Controls.Add(btnAmstrad, 10, 1);
            tableLayoutPanel3.Controls.Add(cbC64VideoMode, 9, 0);
            tableLayoutPanel3.Controls.Add(btnC64, 9, 1);
            tableLayoutPanel3.Controls.Add(cbAmigaMode, 8, 0);
            tableLayoutPanel3.Controls.Add(btnAmiga, 8, 1);
            tableLayoutPanel3.Controls.Add(btnLumSat, 7, 3);
            tableLayoutPanel3.Controls.Add(btnPalette, 11, 3);
            tableLayoutPanel3.Controls.Add(btnCluster, 10, 3);
            tableLayoutPanel3.Controls.Add(btnMedian, 9, 3);
            tableLayoutPanel3.Controls.Add(btnFast, 8, 3);
            tableLayoutPanel3.Controls.Add(label7, 6, 0);
            tableLayoutPanel3.Controls.Add(label8, 6, 2);
            tableLayoutPanel3.Controls.Add(numLumH, 7, 0);
            tableLayoutPanel3.Controls.Add(numLumS, 7, 1);
            tableLayoutPanel3.Controls.Add(numLumV, 7, 2);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 4;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel3.Size = new Size(1158, 128);
            tableLayoutPanel3.TabIndex = 11;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(537, 32);
            label9.Name = "label9";
            label9.Size = new Size(61, 15);
            label9.TabIndex = 41;
            label9.Text = "Saturation";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(92, 0);
            label2.Name = "label2";
            label2.Size = new Size(62, 30);
            label2.TabIndex = 30;
            label2.Text = "Color Evaluation";
            // 
            // cbPreset
            // 
            cbPreset.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cbPreset.FormattingEnabled = true;
            cbPreset.Location = new Point(3, 35);
            cbPreset.Name = "cbPreset";
            cbPreset.Size = new Size(83, 23);
            cbPreset.TabIndex = 3;
            cbPreset.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // btnLoad
            // 
            btnLoad.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnLoad.Location = new Point(3, 67);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(83, 23);
            btnLoad.TabIndex = 18;
            btnLoad.Text = "LOAD";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSave.Location = new Point(3, 99);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(83, 23);
            btnSave.TabIndex = 22;
            btnSave.Text = "SAVE";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(56, 15);
            label1.TabIndex = 29;
            label1.Text = "Template";
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
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(448, 0);
            label6.Name = "label6";
            label6.Size = new Size(78, 30);
            label6.TabIndex = 36;
            label6.Text = "Quantization Bits";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(92, 32);
            label5.Name = "label5";
            label5.Size = new Size(66, 15);
            label5.TabIndex = 34;
            label5.Text = "Max Colors";
            // 
            // numericUpDown1
            // 
            numericUpDown1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            numericUpDown1.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            numericUpDown1.Location = new Point(181, 35);
            numericUpDown1.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
            numericUpDown1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(83, 23);
            numericUpDown1.TabIndex = 35;
            numericUpDown1.Value = new decimal(new int[] { 16, 0, 0, 0 });
            // 
            // btnQuantizer
            // 
            btnQuantizer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnQuantizer.Location = new Point(448, 67);
            btnQuantizer.Name = "btnQuantizer";
            btnQuantizer.Size = new Size(83, 23);
            btnQuantizer.TabIndex = 4;
            btnQuantizer.Text = "Quantize";
            btnQuantizer.UseVisualStyleBackColor = true;
            btnQuantizer.Click += btnQuantizer_Click;
            // 
            // cbDithering
            // 
            cbDithering.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cbDithering.FormattingEnabled = true;
            cbDithering.Location = new Point(359, 3);
            cbDithering.Name = "cbDithering";
            cbDithering.Size = new Size(83, 23);
            cbDithering.TabIndex = 13;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(270, 0);
            label3.Name = "label3";
            label3.Size = new Size(59, 30);
            label3.TabIndex = 31;
            label3.Text = "Dithering Model";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(270, 32);
            label4.Name = "label4";
            label4.Size = new Size(59, 30);
            label4.TabIndex = 32;
            label4.Text = "Dithering Stength";
            // 
            // numDitheringStrenght
            // 
            numDitheringStrenght.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            numDitheringStrenght.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numDitheringStrenght.Location = new Point(359, 35);
            numDitheringStrenght.Name = "numDitheringStrenght";
            numDitheringStrenght.Size = new Size(83, 23);
            numDitheringStrenght.TabIndex = 21;
            numDitheringStrenght.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // btnBkgRem
            // 
            btnBkgRem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnBkgRem.Location = new Point(359, 99);
            btnBkgRem.Name = "btnBkgRem";
            btnBkgRem.Size = new Size(83, 23);
            btnBkgRem.TabIndex = 5;
            btnBkgRem.Text = "BKG Rem.";
            btnBkgRem.UseVisualStyleBackColor = true;
            btnBkgRem.Click += btnBkgRem_Click;
            // 
            // btnID
            // 
            btnID.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnID.Location = new Point(181, 99);
            btnID.Name = "btnID";
            btnID.Size = new Size(83, 23);
            btnID.TabIndex = 1;
            btnID.Text = "Identity";
            btnID.UseVisualStyleBackColor = true;
            btnID.Click += butID;
            // 
            // btnReprocess
            // 
            btnReprocess.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnReprocess.Location = new Point(92, 99);
            btnReprocess.Name = "btnReprocess";
            btnReprocess.Size = new Size(83, 23);
            btnReprocess.TabIndex = 2;
            btnReprocess.Text = "Reprocess";
            btnReprocess.UseVisualStyleBackColor = true;
            btnReprocess.Click += butReprocess;
            // 
            // cbQuantization
            // 
            cbQuantization.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cbQuantization.FormattingEnabled = true;
            cbQuantization.Location = new Point(448, 35);
            cbQuantization.Name = "cbQuantization";
            cbQuantization.Size = new Size(83, 23);
            cbQuantization.TabIndex = 33;
            cbQuantization.SelectedIndexChanged += cbQuantization_SelectedIndexChanged;
            // 
            // cbCgaVideoMode
            // 
            cbCgaVideoMode.FormattingEnabled = true;
            cbCgaVideoMode.Location = new Point(1071, 3);
            cbCgaVideoMode.Name = "cbCgaVideoMode";
            cbCgaVideoMode.Size = new Size(83, 23);
            cbCgaVideoMode.TabIndex = 28;
            cbCgaVideoMode.SelectedIndexChanged += cbCgaVideoMode_SelectedIndexChanged;
            // 
            // btnCGA
            // 
            btnCGA.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnCGA.Location = new Point(1071, 35);
            btnCGA.Name = "btnCGA";
            btnCGA.Size = new Size(84, 23);
            btnCGA.TabIndex = 12;
            btnCGA.Text = "CGA";
            btnCGA.UseVisualStyleBackColor = true;
            btnCGA.Click += btnCGA_Click;
            // 
            // btnVGA
            // 
            btnVGA.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnVGA.Location = new Point(1071, 99);
            btnVGA.Name = "btnVGA";
            btnVGA.Size = new Size(84, 23);
            btnVGA.TabIndex = 23;
            btnVGA.Text = "VGA";
            btnVGA.UseVisualStyleBackColor = true;
            // 
            // cbZXAutotuneMode
            // 
            cbZXAutotuneMode.FormattingEnabled = true;
            cbZXAutotuneMode.Location = new Point(982, 3);
            cbZXAutotuneMode.Name = "cbZXAutotuneMode";
            cbZXAutotuneMode.Size = new Size(83, 23);
            cbZXAutotuneMode.TabIndex = 27;
            cbZXAutotuneMode.SelectedIndexChanged += cbZXAutotuneMode_SelectedIndexChanged;
            // 
            // cbZXPaletteMode
            // 
            cbZXPaletteMode.FormattingEnabled = true;
            cbZXPaletteMode.Location = new Point(982, 35);
            cbZXPaletteMode.Name = "cbZXPaletteMode";
            cbZXPaletteMode.Size = new Size(83, 23);
            cbZXPaletteMode.TabIndex = 38;
            cbZXPaletteMode.SelectedIndexChanged += cbZXPaletteMode_SelectedIndexChanged;
            // 
            // btnSpeccy
            // 
            btnSpeccy.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSpeccy.Location = new Point(982, 67);
            btnSpeccy.Name = "btnSpeccy";
            btnSpeccy.Size = new Size(83, 23);
            btnSpeccy.TabIndex = 11;
            btnSpeccy.Text = "Spectrum";
            btnSpeccy.UseVisualStyleBackColor = true;
            btnSpeccy.Click += btnSpeccy_Click;
            // 
            // btnEGA
            // 
            btnEGA.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnEGA.Location = new Point(1071, 67);
            btnEGA.Name = "btnEGA";
            btnEGA.Size = new Size(84, 23);
            btnEGA.TabIndex = 9;
            btnEGA.Text = "EGA";
            btnEGA.UseVisualStyleBackColor = true;
            btnEGA.Click += btnEGA_Click;
            // 
            // cbCPCVideoMode
            // 
            cbCPCVideoMode.FormattingEnabled = true;
            cbCPCVideoMode.Location = new Point(893, 3);
            cbCPCVideoMode.Name = "cbCPCVideoMode";
            cbCPCVideoMode.Size = new Size(83, 23);
            cbCPCVideoMode.TabIndex = 25;
            cbCPCVideoMode.SelectedIndexChanged += cbCPCVideoMode_SelectedIndexChanged;
            // 
            // btnAmstrad
            // 
            btnAmstrad.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnAmstrad.Location = new Point(893, 35);
            btnAmstrad.Name = "btnAmstrad";
            btnAmstrad.Size = new Size(83, 23);
            btnAmstrad.TabIndex = 10;
            btnAmstrad.Text = "Amstrad";
            btnAmstrad.UseVisualStyleBackColor = true;
            btnAmstrad.Click += btnAmstrad_Click;
            // 
            // cbC64VideoMode
            // 
            cbC64VideoMode.FormattingEnabled = true;
            cbC64VideoMode.Location = new Point(804, 3);
            cbC64VideoMode.Name = "cbC64VideoMode";
            cbC64VideoMode.Size = new Size(83, 23);
            cbC64VideoMode.TabIndex = 26;
            cbC64VideoMode.SelectedIndexChanged += cbC64VideoMode_SelectedIndexChanged;
            // 
            // btnC64
            // 
            btnC64.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnC64.Location = new Point(804, 35);
            btnC64.Name = "btnC64";
            btnC64.Size = new Size(83, 23);
            btnC64.TabIndex = 8;
            btnC64.Text = "C64";
            btnC64.UseVisualStyleBackColor = true;
            btnC64.Click += btnC64_Click;
            // 
            // cbAmigaMode
            // 
            cbAmigaMode.FormattingEnabled = true;
            cbAmigaMode.Location = new Point(715, 3);
            cbAmigaMode.Name = "cbAmigaMode";
            cbAmigaMode.Size = new Size(83, 23);
            cbAmigaMode.TabIndex = 24;
            cbAmigaMode.SelectedIndexChanged += cbAmigaMode_SelectedIndexChanged;
            // 
            // btnAmiga
            // 
            btnAmiga.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnAmiga.Location = new Point(715, 35);
            btnAmiga.Name = "btnAmiga";
            btnAmiga.Size = new Size(83, 23);
            btnAmiga.TabIndex = 6;
            btnAmiga.Text = "Amiga";
            btnAmiga.UseVisualStyleBackColor = true;
            btnAmiga.Click += btnAmiga_Click;
            // 
            // btnLumSat
            // 
            btnLumSat.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnLumSat.Location = new Point(626, 99);
            btnLumSat.Name = "btnLumSat";
            btnLumSat.Size = new Size(83, 23);
            btnLumSat.TabIndex = 7;
            btnLumSat.Text = "LumSat";
            btnLumSat.UseVisualStyleBackColor = true;
            btnLumSat.Click += btnLumSat_Click;
            // 
            // btnPalette
            // 
            btnPalette.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnPalette.Location = new Point(982, 99);
            btnPalette.Name = "btnPalette";
            btnPalette.Size = new Size(83, 23);
            btnPalette.TabIndex = 17;
            btnPalette.Text = "Fixed PAL";
            btnPalette.UseVisualStyleBackColor = true;
            btnPalette.Click += btnPalette_Click;
            // 
            // btnCluster
            // 
            btnCluster.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnCluster.Location = new Point(893, 99);
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
            btnMedian.Location = new Point(804, 99);
            btnMedian.Name = "btnMedian";
            btnMedian.Size = new Size(83, 23);
            btnMedian.TabIndex = 15;
            btnMedian.Text = "Median";
            btnMedian.UseVisualStyleBackColor = true;
            btnMedian.Click += btnMedian_Click;
            // 
            // btnFast
            // 
            btnFast.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnFast.Location = new Point(715, 99);
            btnFast.Name = "btnFast";
            btnFast.Size = new Size(83, 23);
            btnFast.TabIndex = 16;
            btnFast.Text = "Fast";
            btnFast.UseVisualStyleBackColor = true;
            btnFast.Click += btnFast_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(537, 0);
            label7.Name = "label7";
            label7.Size = new Size(56, 15);
            label7.TabIndex = 39;
            label7.Text = "Hue Shift";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(537, 64);
            label8.Name = "label8";
            label8.Size = new Size(35, 15);
            label8.TabIndex = 40;
            label8.Text = "Value";
            // 
            // numLumH
            // 
            numLumH.Location = new Point(626, 3);
            numLumH.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            numLumH.Minimum = new decimal(new int[] { 360, 0, 0, int.MinValue });
            numLumH.Name = "numLumH";
            numLumH.Size = new Size(83, 23);
            numLumH.TabIndex = 42;
            // 
            // numLumS
            // 
            numLumS.Location = new Point(626, 35);
            numLumS.Name = "numLumS";
            numLumS.Size = new Size(83, 23);
            numLumS.TabIndex = 43;
            numLumS.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // numLumV
            // 
            numLumV.Location = new Point(626, 67);
            numLumV.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numLumV.Name = "numLumV";
            numLumV.Size = new Size(83, 23);
            numLumV.TabIndex = 44;
            numLumV.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // listBox1
            // 
            listBox1.Dock = DockStyle.Fill;
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(0, 0);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(1164, 119);
            listBox1.TabIndex = 0;
            // 
            // bitmapRenderIN
            // 
            bitmapRenderIN.ImageMoveOverControlBorder = false;
            // 
            // bitmapRenderOUT
            // 
            bitmapRenderOUT.ImageMoveOverControlBorder = false;
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
            ((System.ComponentModel.ISupportInitialize)pbIN).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbOUT).EndInit();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDitheringStrenght).EndInit();
            ((System.ComponentModel.ISupportInitialize)numLumH).EndInit();
            ((System.ComponentModel.ISupportInitialize)numLumS).EndInit();
            ((System.ComponentModel.ISupportInitialize)numLumV).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private PictureBox pbIN;
        private PictureBox pbOUT;
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
        private PropertyGrid propertyGrid2;
        private Button btnVGA;
        private ComboBox cbAmigaMode;
        private ComboBox cbCPCVideoMode;
        private ComboBox cbC64VideoMode;
        private ComboBox cbZXAutotuneMode;
        private ComboBox cbCgaVideoMode;
        private Label label1;
        private Label label3;
        private Label label2;
        private Label label4;
        private ComboBox cbQuantization;
        private Label label5;
        private NumericUpDown numericUpDown1;
        private Label label6;
        private ComboBox cbZXPaletteMode;
        private Label label9;
        private Label label7;
        private Label label8;
        private NumericUpDown numLumH;
        private NumericUpDown numLumS;
        private NumericUpDown numLumV;
    }
}
