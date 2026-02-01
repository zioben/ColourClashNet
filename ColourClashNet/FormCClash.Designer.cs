using ColourClashNet.Controls;

namespace ColourClashNet
{
    partial class FormCClash
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCClash));
            caTemplate = new ColorAnalyzer();
            tableLayoutPanel1 = new TableLayoutPanel();
            imageList1 = new ImageList(components);
            menuStripMain = new MenuStrip();
            toolStripMenuItem1 = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripMenuItem();
            toolStripMenuItemSave = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            newContainerToolStripMenuItem = new ToolStripMenuItem();
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
            tsmiResolution = new ToolStripMenuItem();
            originalToolStripMenuItem = new ToolStripMenuItem();
            toolStripContainer1 = new ToolStripContainer();
            
            sfdExportImage = new SaveFileDialog();
            ofdSelectImage = new OpenFileDialog();
            
            menuStripMain.SuspendLayout();
            toolStripContainer1.ContentPanel.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();
            toolStripContainer1.SuspendLayout();
           
            SuspendLayout();
            // 
            // caTemplate
            // 
            caTemplate.Dock = DockStyle.Fill;
            caTemplate.Location = new Point(3, 3);
            caTemplate.Name = "caTemplate";
            caTemplate.Size = new Size(1164, 737);
            caTemplate.TabIndex = 0;
            caTemplate.WantedRes = null;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 108F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1184, 817);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "Close.png");
            // 
            // menuStripMain
            // 
            menuStripMain.Dock = DockStyle.None;
            menuStripMain.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1, colorModeToolStripMenuItem, colorDistanceToolStripMenuItem, ditheringToolStripMenuItem, tsmiResolution });
            menuStripMain.Location = new Point(0, 0);
            menuStripMain.Name = "menuStripMain";
            menuStripMain.Size = new Size(1184, 24);
            menuStripMain.TabIndex = 2;
            menuStripMain.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem2, toolStripMenuItem3, toolStripMenuItemSave, exportToolStripMenuItem });
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(37, 20);
            toolStripMenuItem1.Text = "&File";
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Image = (Image)resources.GetObject("toolStripMenuItem2.Image");
            toolStripMenuItem2.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.ShortcutKeys = Keys.Control | Keys.N;
            toolStripMenuItem2.Size = new Size(180, 22);
            toolStripMenuItem2.Text = "&New";
            toolStripMenuItem2.Click += toolStripMenuItemNew_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Image = (Image)resources.GetObject("toolStripMenuItem3.Image");
            toolStripMenuItem3.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.ShortcutKeys = Keys.Control | Keys.O;
            toolStripMenuItem3.Size = new Size(180, 22);
            toolStripMenuItem3.Text = "&Open";
            toolStripMenuItem3.Click += toolStripMenuItemOpen_Click;
            // 
            // toolStripMenuItemSave
            // 
            toolStripMenuItemSave.Image = (Image)resources.GetObject("toolStripMenuItemSave.Image");
            toolStripMenuItemSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripMenuItemSave.Name = "toolStripMenuItemSave";
            toolStripMenuItemSave.ShortcutKeys = Keys.Control | Keys.S;
            toolStripMenuItemSave.Size = new Size(180, 22);
            toolStripMenuItemSave.Text = "&Save";
            toolStripMenuItemSave.Click += toolStripMenuItemSave_Click;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newContainerToolStripMenuItem, bitmapIndexedToolStripMenuItem, indexedPNGToolStripMenuItem, bitmapToolStripMenuItem, pNGToolStripMenuItem, amigaToolStripMenuItem });
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(180, 22);
            exportToolStripMenuItem.Text = "Export";
            // 
            // newContainerToolStripMenuItem
            // 
            newContainerToolStripMenuItem.Name = "newContainerToolStripMenuItem";
            newContainerToolStripMenuItem.Size = new Size(180, 22);
            newContainerToolStripMenuItem.Text = "New Container";
            newContainerToolStripMenuItem.Click += newContainerToolStripMenuItem_Click;
            // 
            // bitmapIndexedToolStripMenuItem
            // 
            bitmapIndexedToolStripMenuItem.Name = "bitmapIndexedToolStripMenuItem";
            bitmapIndexedToolStripMenuItem.Size = new Size(180, 22);
            bitmapIndexedToolStripMenuItem.Text = "Bitmap Indexed";
            bitmapIndexedToolStripMenuItem.Click += bitmapIndexedToolStripMenuItem_Click;
            // 
            // indexedPNGToolStripMenuItem
            // 
            indexedPNGToolStripMenuItem.Name = "indexedPNGToolStripMenuItem";
            indexedPNGToolStripMenuItem.Size = new Size(180, 22);
            indexedPNGToolStripMenuItem.Text = "PNG Indexed";
            indexedPNGToolStripMenuItem.Click += indexedPNGToolStripMenuItem_Click;
            // 
            // bitmapToolStripMenuItem
            // 
            bitmapToolStripMenuItem.Name = "bitmapToolStripMenuItem";
            bitmapToolStripMenuItem.Size = new Size(180, 22);
            bitmapToolStripMenuItem.Text = "Bitmap";
            bitmapToolStripMenuItem.Click += bitmapToolStripMenuItem_Click;
            // 
            // pNGToolStripMenuItem
            // 
            pNGToolStripMenuItem.Name = "pNGToolStripMenuItem";
            pNGToolStripMenuItem.Size = new Size(180, 22);
            pNGToolStripMenuItem.Text = "PNG";
            pNGToolStripMenuItem.Click += pNGToolStripMenuItem_Click;
            // 
            // amigaToolStripMenuItem
            // 
            amigaToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { iFFILBMToolStripMenuItem, bitplaneToolStripMenuItem1, rawBitplaneAndOCSCopperlistToolStripMenuItem, sourceCodeToolStripMenuItem });
            amigaToolStripMenuItem.Name = "amigaToolStripMenuItem";
            amigaToolStripMenuItem.Size = new Size(180, 22);
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
            // tsmiResolution
            // 
            tsmiResolution.DropDownItems.AddRange(new ToolStripItem[] { originalToolStripMenuItem });
            tsmiResolution.Name = "tsmiResolution";
            tsmiResolution.Size = new Size(75, 20);
            tsmiResolution.Text = "Resolution";
            // 
            // originalToolStripMenuItem
            // 
            originalToolStripMenuItem.Checked = true;
            originalToolStripMenuItem.CheckState = CheckState.Checked;
            originalToolStripMenuItem.Name = "originalToolStripMenuItem";
            originalToolStripMenuItem.Size = new Size(203, 22);
            originalToolStripMenuItem.Text = "Samen as bitmap source";
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            toolStripContainer1.ContentPanel.Size = new Size(1184, 793);
            toolStripContainer1.Dock = DockStyle.Fill;
            toolStripContainer1.Location = new Point(0, 0);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new Size(1184, 817);
            toolStripContainer1.TabIndex = 3;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            toolStripContainer1.TopToolStripPanel.Controls.Add(menuStripMain);
           
            // 
            // sfdExportImage
            // 
            sfdExportImage.AddExtension = false;
            // 
            // ofdSelectImage
            // 
            ofdSelectImage.Filter = "Bmp|*.bmp|Png|*.png|Jpg|*.jpg|All Files|*.*";
            ofdSelectImage.FilterIndex = 4;
            ofdSelectImage.Title = "Load Image";
            // 
            // tabControl1
            // 
            // 
            // tabPage3
            // 
           
            // 
            // FormCClash
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 817);
            Controls.Add(toolStripContainer1);
            Controls.Add(tableLayoutPanel1);
            MainMenuStrip = menuStripMain;
            Name = "FormCClash";
            Text = "Form1";
            Load += FormCClash_Load;
            menuStripMain.ResumeLayout(false);
            menuStripMain.PerformLayout();
            toolStripContainer1.ContentPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.PerformLayout();
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            
            ResumeLayout(false);
        }

        #endregion


        private TableLayoutPanel tableLayoutPanel1;
        private ColorAnalyzer caTemplate;
        private ImageList imageList1;
        private MenuStrip menuStripMain;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItemSave;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem newContainerToolStripMenuItem;
        private ToolStripMenuItem bitmapIndexedToolStripMenuItem;
        private ToolStripMenuItem indexedPNGToolStripMenuItem;
        private ToolStripMenuItem bitmapToolStripMenuItem;
        private ToolStripMenuItem pNGToolStripMenuItem;
        private ToolStripMenuItem amigaToolStripMenuItem;
        private ToolStripMenuItem iFFILBMToolStripMenuItem;
        private ToolStripMenuItem bitplaneToolStripMenuItem1;
        private ToolStripMenuItem rawBitplaneAndOCSCopperlistToolStripMenuItem;
        private ToolStripMenuItem sourceCodeToolStripMenuItem;
        private ToolStripMenuItem colorModeToolStripMenuItem;
        private ToolStripMenuItem colorDistanceToolStripMenuItem;
        private ToolStripMenuItem ditheringToolStripMenuItem;
        private ToolStripMenuItem tsmiResolution;
        private ToolStripMenuItem originalToolStripMenuItem;
        private ToolStripContainer toolStripContainer1;       
        private SaveFileDialog sfdExportImage;
        private OpenFileDialog ofdSelectImage;
      
        
    }
}