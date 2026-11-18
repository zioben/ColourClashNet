using ColourClashNet.Color;
using ColourClashNet.Components;
using ColourClashNet.Controls;
using ColourClashNet.Drawing;
using ColourClashNet.Imaging;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace ColourClashNet
{
    public partial class FormCClash : Form
    {


        List<GraphicsResolution> gfxRes = new List<GraphicsResolution>()
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

        List<ToolStripMenuItem> tsItemsSetup = new List<ToolStripMenuItem>();
        List<ToolStripMenuItem> tsItemsResolution = new List<ToolStripMenuItem>();

        ColorAnalyzer? SelectedColorAnalyzer => tabManager?.SelectedTab?.Tag as ColorAnalyzer;

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

        TabControlExt tabManager;
        void CreateTabManager()
        {
            tabManager = new TabControlExt();
            tabManager.SuspendLayout();
            tabManager.Dock = DockStyle.Fill;
            tabManager.Location = new Point(0, 0);
            tabManager.Name = "tabControl1";
            tabManager.SelectedIndex = 0;
            tabManager.Size = new Size(1184, 793);
            tabManager.TabIndex = 0;

            toolStripContainer1.ContentPanel.Controls.Add(tabManager);

            tabManager.TabPages.Clear();
            tabManager.ControlAdded += ((s, e)
                => MenuRebuildSetChecks());
            tabManager.SelectedIndexChanged += ((s, e)
                => MenuRebuildSetChecks());
//            tabManager.TabIndexChanged += ((s, e) 
  //              => MenuRebuildSetChecks());
     //       tabManager.TabClosed += ((s, e) 
       //         => MenuRebuildSetChecks());
            tabManager.ResumeLayout();

            CreateTab();
        }

      

        ColorAnalyzer CreateTab()
        {
            var caNew = new ColorAnalyzer();
            caNew.Dock = caTemplate.Dock;
            caNew.Location = caTemplate.Location;
            caNew.Name = $"ca.{DateTime.Now.Ticks}";
            caNew.Size = caTemplate.Size;
            caNew.ImageCopied += ((s, e) => CreateTab(e.DestBitmap, e.Name));
            caNew.ImageCreated += ((s, e) => tabManager.SetPageText(caNew, e.Name.Substring(Math.Max(0, e.Name.Length - 12))));
            var tp = tabManager.CreatePage(caNew);
            tabManager.SelectedTab= tp;    

            return caNew;
        }

        void CreateTab(Image srcImage, string name) => CreateTab().Create(srcImage, name);

        #endregion


        #region Populating Menu

        void CreateMenuItem(ToolStripMenuItem parentItem, string text, object? tag)
        {
            if( tag == null)
                return; 
            if (tag.ToString().ToLower().EndsWith(".unknown"))
                return;
            var tsItem = new System.Windows.Forms.ToolStripMenuItem();
            tsItem.Name = $"Menu.{tag}";
            tsItem.Size = new System.Drawing.Size(180, 22);
            tsItem.Text = text;
            tsItem.CheckOnClick = true;
            tsItem.Tag = tag;
            tsItem.Click += TsItem_ClickSetup;
            parentItem.DropDownItems.Add(tsItem);
            tsItemsSetup.Add(tsItem);
        }

        //string ToTag<T>(string value)
        //{
        //    return $"{typeof(T).Name}.{value}";
        //}

        void InitMenu<T>(ToolStripMenuItem parentItem) where T : System.Enum
        {
            var colorMode = Enum.GetValues(typeof(T));
            foreach (var x in colorMode)
            {
                var s = x.ToString() ?? "null";
                CreateMenuItem(parentItem, s, x);
            };
        }

        void InitMenu()
        {
            InitMenu<ColorQuantizationMode>(colorModeToolStripMenuItem);
            InitMenu<ColorDistanceEvaluationMode>(colorDistanceToolStripMenuItem);
            InitMenu<ColorDithering>(ditheringToolStripMenuItem);
            InitMenu<ColorDitheringFx>(ditherFXToolStripMenuItem);
            MenuRebuildSetChecks();
        }

        void MenuRebulidSetCheck<T>(string? value)
        {
            if (string.IsNullOrEmpty( value))
                return;
            //var sTag = ToTag<T>(value ?? "null");
            var tagList = tsItemsSetup.Where(x => x.Tag != null).ToList();
            var typeList = tagList.Where(x => typeof(T) == x.Tag.GetType()).ToList();
            var tsSelected = typeList.Where(x => x.Tag.ToString()==value).ToList();
            foreach (var ts in tsSelected)
            {
                ts.Checked = true;
            }
        }

        void MenuRebulidSetCheck(GraphicsResolution? res)
        {
            if (res == null)
            {
                originalToolStripMenuItem.Checked = true;
            }
            else
            {
                var ts = tsItemsResolution.FirstOrDefault(x => x.Tag == res);
                if (ts != null)
                    ts.Checked = true;
            }
        }

        void MenuRebuildSetChecks()
        {
            tsItemsSetup.ForEach(x => x.Checked = false);
            MenuRebulidSetCheck<ColorQuantizationMode>(SelectedColorAnalyzer?.QuantizationMode.ToString());
            MenuRebulidSetCheck<ColorDistanceEvaluationMode>(SelectedColorAnalyzer?.ColorDistanceEvaluationMode.ToString());
            MenuRebulidSetCheck<ColorDithering>(SelectedColorAnalyzer?.DitheringType.ToString());
            MenuRebulidSetCheck<ColorDitheringFx>(SelectedColorAnalyzer?.DitheringFx.ToString());
            tsItemsResolution.ForEach(x => x.Checked = false);
            MenuRebulidSetCheck(SelectedColorAnalyzer?.WantedRes);
        }
        

        private void TsItem_ClickSetup(object? sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            var tag = menuItem.Tag;
            if (tag is ColorQuantizationMode)
            {
                SelectedColorAnalyzer.QuantizationMode = (ColorQuantizationMode)tag;
                SelectedColorAnalyzer.Preprocess();
            }
            else if (tag is ColorDistanceEvaluationMode)
            {
                SelectedColorAnalyzer.ColorDistanceEvaluationMode = (ColorDistanceEvaluationMode)tag;
            }
            else if (tag is ColorDithering)
            {
                SelectedColorAnalyzer.DitheringType = (ColorDithering)tag;
            }
            else if (tag is ColorDitheringFx)
            {
                SelectedColorAnalyzer.DitheringFx = (ColorDitheringFx)tag;
            }

            MenuRebuildSetChecks();
        }

        private void TsItem_ClickResolution(object? sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            var tag = menuItem.Tag;
            if (tag is GraphicsResolution resolution && SelectedColorAnalyzer != null)
            {
                SelectedColorAnalyzer.WantedRes = resolution;
            }
            MenuRebuildSetChecks();
            SelectedColorAnalyzer?.Create();
        }

        void CreateScaleMenuItems()
        {
            originalToolStripMenuItem.Click += TsItem_ClickResolution;
            tsItemsResolution.Add(originalToolStripMenuItem);
            foreach (var res in gfxRes)
            {
                var tsItem = new System.Windows.Forms.ToolStripMenuItem();
                tsItem.Name = "tsi" + res.Name;
                tsItem.Size = new System.Drawing.Size(180, 22);
                tsItem.Text = res.Name;
                tsItem.CheckOnClick = true;
                tsItem.Tag = res;
                tsItem.Click += TsItem_ClickResolution;
                tsmiResolution.DropDownItems.Add(tsItem);
                tsItemsResolution.Add(tsItem);
            }
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
                var fileInfo = new FileInfo(ofdSelectImage.FileName);
                SelectedColorAnalyzer?.Create(fileInfo);
            }
        }

        ImageData GetProcessedImageData() => ImageToolsGDI.GdiImageToImageData(SelectedColorAnalyzer?.ColorManager?.ImageProcessed);

        private void toolStripMenuItemSave_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageTools.SaveImage(GetProcessedImageData(), sfdExportImage.FileName, ImageSaveFormat.Png);
            }
        }

        private void newContainerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var image = SelectedColorAnalyzer?.ColorManager?.ImageProcessed?.Clone() as Bitmap;
            var name = SelectedColorAnalyzer.Name;
            if (image == null)
                return;
            CreateTab();
            SelectedColorAnalyzer?.Create(image, name + "_out");

        }

        private void bitmapIndexedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var image = SelectedColorAnalyzer?.ColorManager?.ImageProcessed?.Clone() as Bitmap;
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageTools.SaveImageAsIndexedBitmap(GetProcessedImageData(), sfdExportImage.FileName);
            }
        }

        private void indexedPNGToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void bitmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageTools.SaveImage(GetProcessedImageData(), sfdExportImage.FileName, ImageSaveFormat.Bmp);
            }
        }

        private void pNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageTools.SaveImage(GetProcessedImageData(), sfdExportImage.FileName, ImageSaveFormat.Png);
            }
        }

     
    }
}