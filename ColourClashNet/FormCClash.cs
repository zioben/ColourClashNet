using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Components;
using ColourClashNet.Controls;
using ColourClashNet.Drawing;
using ColourClashNet.Imaging;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static ColourClashNet.Color.Transformation.ColorTransformReductionC64;
using static ColourClashNet.Color.Transformation.ColorTransformReductionCPC;
using static ColourClashNet.Color.Transformation.ColorTransformReductionZxSpectrum;

namespace ColourClashNet
{
    public partial class FormCClash : Form
    {


        List<GraphicsResolution> lGfxRes = new List<GraphicsResolution>()
        {
            new GraphicsResolution { Width=160, Height = 144 },
            new GraphicsResolution { Width=176, Height = 184 },
            new GraphicsResolution { Width=240, Height = 160 },
            new GraphicsResolution { Width=256, Height = 160 },
            new GraphicsResolution { Width=256, Height = 192 },
            new GraphicsResolution { Width=256, Height = 200 },
            new GraphicsResolution { Width=256, Height = 224 },
            new GraphicsResolution { Width=256, Height = 240 },
            new GraphicsResolution { Width=320, Height = 200 },
            new GraphicsResolution { Width=320, Height = 240 },
            new GraphicsResolution { Width=320, Height = 256 },
            new GraphicsResolution { Width=320, Height = 200 },
            new GraphicsResolution { Width=320, Height = 224 },
            new GraphicsResolution { Width=320, Height = 240 },
            new GraphicsResolution { Width=320, Height = 256 },
            new GraphicsResolution { Width=336, Height = 224 },
            new GraphicsResolution { Width=512, Height = 242 },
            new GraphicsResolution { Width=512, Height = 448 },
            new GraphicsResolution { Width=640, Height = 200 },
            new GraphicsResolution { Width=640, Height = 224 },
            new GraphicsResolution { Width=640, Height = 240 },
            new GraphicsResolution { Width=640, Height = 256 },
            new GraphicsResolution { Width=640, Height = 400 },
            new GraphicsResolution { Width=640, Height = 448 },
            new GraphicsResolution { Width=640, Height = 480 },
            new GraphicsResolution { Width=640, Height = 512 },
            new GraphicsResolution { Width=800, Height = 600 },
            new GraphicsResolution { Width=1024, Height = 768 },
        };

        List<ToolStripMenuItem> lTsItemsSetup = new List<ToolStripMenuItem>();
        List<ToolStripMenuItem> lTsItemsResolution = new List<ToolStripMenuItem>();

        ColorAnalyzer selectedColorAnalyzer { get; set; }

        public FormCClash()
        {
            InitializeComponent();
        }

        private void FormCClash_Load(object sender, EventArgs e)
        {
            InitMenu();
            CreateScaleMenuItems();
            CreateTabManager();
        }

        #region Tab Management

        TabControlExt oTabManager;
        void CreateTabManager()
        {
            oTabManager = new TabControlExt();
            oTabManager.SuspendLayout();
            oTabManager.Dock = DockStyle.Fill;
            oTabManager.Location = new Point(0, 0);
            oTabManager.Name = "tabControl1";
            oTabManager.SelectedIndex = 0;
            oTabManager.Size = new Size(1184, 793);
            oTabManager.TabIndex = 0;

            toolStripContainer1.ContentPanel.Controls.Add(oTabManager);

            oTabManager.TabPages.Clear();
            oTabManager.ControlAdded += ((s, e) => RefreahWorkingTab());
            oTabManager.TabIndexChanged += ((s, e) => RefreahWorkingTab());
            oTabManager.TabClosed += ((s, e) => RefreahWorkingTab());
            oTabManager.ResumeLayout();

            CreateTab();
        }

        void RefreahWorkingTab()
        {
            var tp = oTabManager.SelectedTab;
            if (tp == null)
                selectedColorAnalyzer = null;
            else
                selectedColorAnalyzer = tp.Tag as ColorAnalyzer;
            // RebuildMenu();
        }

        ColorAnalyzer CreateTab()
        {
            var caNew = new ColorAnalyzer();
            caNew.Dock = caTemplate.Dock;
            caNew.Location = caTemplate.Location;
            caNew.Name = $"ca.{DateTime.Now.Ticks}";
            caNew.Size = caTemplate.Size;
            caNew.ImageCopied += ((s, e) => CreateTab(e.DestBitmap, e.Name));
            caNew.ImageCreated += ((s, e) => oTabManager.SetPageText(caNew, e.Name.Substring(Math.Max(0, e.Name.Length - 12))));
            oTabManager.CreatePage(caNew);
            return caNew;
        }

        void CreateTab(Image oSrcImage, string sName) => CreateTab().Create(oSrcImage, sName);

        #endregion


        #region Populating Menu

        void MenuRebulidSetCheck(string sItem)
        {
            var ts = lTsItemsSetup.FirstOrDefault(X => X.Tag?.ToString() == sItem);
            if (ts != null)
                ts.Checked = true;
        }

        void MenuRebulidSetCheck(GraphicsResolution oRes)
        {
            if (oRes == null)
            {
                originalToolStripMenuItem.Checked = true;
            }
            else
            {
                var ts = lTsItemsResolution.FirstOrDefault(X => X.Tag == oRes);
                if (ts != null)
                    ts.Checked = true;
            }
        }

        void MenuRebuildSetChecks()
        {
            lTsItemsSetup.ForEach(X => X.Checked = false);
            MenuRebulidSetCheck(selectedColorAnalyzer?.Config.ColorQuantizationMode.ToString());
            MenuRebulidSetCheck(selectedColorAnalyzer?.Config.ColorDistanceEvaluationMode.ToString());
            MenuRebulidSetCheck(selectedColorAnalyzer?.Config.DitheringAlgorithm.ToString());
            lTsItemsResolution.ForEach(X => X.Checked = false);
            MenuRebulidSetCheck(selectedColorAnalyzer?.WantedRes);
        }

        private void TsItem_ClickSetup(object? sender, EventArgs e)
        {
            var oTS = sender as ToolStripMenuItem;
            var oTag = oTS.Tag;
            if (oTag is ColorQuantizationMode)
            {
                selectedColorAnalyzer.Config.ColorQuantizationMode = (ColorQuantizationMode)oTag;
                selectedColorAnalyzer.Preprocess();
            }
            else if (oTag is ColorDistanceEvaluationMode)
            {
                selectedColorAnalyzer.Config.ColorDistanceEvaluationMode = (ColorDistanceEvaluationMode)oTag;
            }
            else if (oTag is ColorDithering)
            {
                selectedColorAnalyzer.Config.DitheringAlgorithm = (ColorDithering)oTag;
            }
            MenuRebuildSetChecks();
        }

        private void TsItem_ClickResolution(object? sender, EventArgs e)
        {
            var oTS = sender as ToolStripMenuItem;
            var oTag = oTS.Tag;
            if (oTag is GraphicsResolution oGfxRes && selectedColorAnalyzer != null)
            {
                selectedColorAnalyzer.WantedRes = oGfxRes;
            }
            MenuRebuildSetChecks();
            selectedColorAnalyzer?.Create();
        }

        void CreateScaleMenuItems()
        {
            originalToolStripMenuItem.Click += TsItem_ClickResolution;
            lTsItemsResolution.Add(originalToolStripMenuItem);
            foreach (var res in lGfxRes)
            {
                var tsItem = new System.Windows.Forms.ToolStripMenuItem();
                tsItem.Name = "tsi" + res.Name;
                tsItem.Size = new System.Drawing.Size(180, 22);
                tsItem.Text = res.Name;
                tsItem.CheckOnClick = true;
                tsItem.Tag = res;
                tsItem.Click += TsItem_ClickResolution;
                tsmiResolution.DropDownItems.Add(tsItem);
                lTsItemsResolution.Add(tsItem);
            }
        }

        void CreateMenuItem(ToolStripMenuItem oTsBase, object oItem)
        {
            if (oItem.ToString() == "Unknown")
                return;
            var tsItem = new System.Windows.Forms.ToolStripMenuItem();
            tsItem.Name = oItem.ToString();
            tsItem.Size = new System.Drawing.Size(180, 22);
            tsItem.Text = oItem.ToString();
            tsItem.CheckOnClick = true;
            tsItem.Tag = oItem;
            tsItem.Click += TsItem_ClickSetup;
            oTsBase.DropDownItems.Add(tsItem);
            lTsItemsSetup.Add(tsItem);
        }


        void InitMenu<T>(ToolStripMenuItem oTsBase) where T : System.Enum
        {
            var lColorMode = Enum.GetValues(typeof(T));
            foreach (var X in lColorMode)
            {
                CreateMenuItem(oTsBase, X);
            };
        }

        void InitMenu()
        {
            InitMenu<ColorQuantizationMode>(colorModeToolStripMenuItem);
            InitMenu<ColorDistanceEvaluationMode>(colorDistanceToolStripMenuItem);
            InitMenu<ColorDithering>(ditheringToolStripMenuItem);
            MenuRebuildSetChecks();
        }


        #endregion


      

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateTab();
        }

        private void TabManager_TabClosing(object sender, ColourClashNet.Controls.TabControlExt.TabEventArgs e)
        {
            if (MessageBox.Show($"Close {e.TabText}?", "Closing Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                e.Canceled = true;
            }
        }

        private void toolStripMenuItemNew_Click(object sender, EventArgs e)
        {
            CreateTab();
        }

        private void toolStripMenuItemOpen_Click(object sender, EventArgs e)
        {
            if (ofdSelectImage.ShowDialog() == DialogResult.OK)
            {
                var oFileInfo = new FileInfo(ofdSelectImage.FileName);
                selectedColorAnalyzer?.Create(oFileInfo);
            }
        }

        private void toolStripMenuItemSave_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageToolsGDI.GdiImageToFile(selectedColorAnalyzer?.ColorManager?.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageExportFormat.Png);
            }
        }

        private void newContainerToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void bitmapIndexedToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void indexedPNGToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void bitmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageToolsGDI.GdiImageToFile(selectedColorAnalyzer?.ColorManager?.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageExportFormat.Bmp);
            }
        }

        private void pNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageToolsGDI.GdiImageToFile(selectedColorAnalyzer?.ColorManager?.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageExportFormat.Png);
            }
        }

     
    }
}