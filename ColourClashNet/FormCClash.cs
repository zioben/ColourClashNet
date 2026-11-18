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
    /// <summary>
    /// Main window: hosts one <see cref="ColorAnalyzer"/> per tab and the global menus (file, settings, resolution, export).
    /// </summary>
    public partial class FormCClash : Form
    {


        // Resolutions offered in the "Resolution" menu (common retro video modes)
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

        /// <summary>
        /// Checkable setting menu items (quantization, distance, dithering).
        /// </summary>
        List<ToolStripMenuItem> tsItemsSetup = new List<ToolStripMenuItem>();
        /// <summary>
        /// Checkable resolution menu items.
        /// </summary>
        List<ToolStripMenuItem> tsItemsResolution = new List<ToolStripMenuItem>();

        /// <summary>
        /// Analyzer of the selected tab.
        /// </summary>
        ColorAnalyzer? SelectedColorAnalyzer => tabManager?.SelectedTab?.Tag as ColorAnalyzer;

        /// <summary>
        /// Creates the main window.
        /// </summary>
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
        /// <summary>
        /// Creates the tab control and the first tab.
        /// </summary>
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
            tabManager.ResumeLayout();

            CreateTab();
        }

      

        /// <summary>
        /// Adds a new tab with an empty analyzer and selects it.
        /// </summary>
        ColorAnalyzer CreateTab()
        {
            var analyzer = new ColorAnalyzer();
            analyzer.Dock = caTemplate.Dock;
            analyzer.Location = caTemplate.Location;
            analyzer.Name = $"ca.{DateTime.Now.Ticks}";
            analyzer.Size = caTemplate.Size;
            analyzer.ImageCopied += ((s, e) => CreateTab(e.DestBitmap, e.Name));
            analyzer.ImageCreated += ((s, e) => tabManager.SetPageText(analyzer, e.Name.Substring(Math.Max(0, e.Name.Length - 12))));
            var page = tabManager.CreatePage(analyzer);
            tabManager.SelectedTab= page;    

            return analyzer;
        }

        /// <summary>
        /// Adds a new tab and loads <paramref name="srcImage"/> into it.
        /// </summary>
        void CreateTab(Image srcImage, string name) => CreateTab().Create(srcImage, name);

        #endregion


        #region Populating Menu

        /// <summary>
        /// Adds a checkable setting item; enum values ending with "Unknown" are skipped.
        /// </summary>
        void CreateMenuItem(ToolStripMenuItem parentItem, string text, object? tag)
        {
            if( tag == null)
                return; 
            if (tag.ToString().ToLower().EndsWith(".unknown"))
                return;
            var menuItem = new System.Windows.Forms.ToolStripMenuItem();
            menuItem.Name = $"Menu.{tag}";
            menuItem.Size = new System.Drawing.Size(180, 22);
            menuItem.Text = text;
            menuItem.CheckOnClick = true;
            menuItem.Tag = tag;
            menuItem.Click += TsItem_ClickSetup;
            parentItem.DropDownItems.Add(menuItem);
            tsItemsSetup.Add(menuItem);
        }

        /// <summary>
        /// Adds one menu item for each value of the enum <typeparamref name="T"/>.
        /// </summary>
        void InitMenu<T>(ToolStripMenuItem parentItem) where T : System.Enum
        {
            var values = Enum.GetValues(typeof(T));
            foreach (var value in values)
            {
                var text = value.ToString() ?? "null";
                CreateMenuItem(parentItem, text, value);
            }
        }

        /// <summary>
        /// Builds the setting menus.
        /// </summary>
        void InitMenu()
        {
            InitMenu<ColorQuantizationMode>(colorModeToolStripMenuItem);
            InitMenu<ColorDistanceEvaluationMode>(colorDistanceToolStripMenuItem);
            InitMenu<ColorDithering>(ditheringToolStripMenuItem);
            InitMenu<ColorDitheringFx>(ditherFXToolStripMenuItem);
            MenuRebuildSetChecks();
        }

        /// <summary>
        /// Checks the menu item of type <typeparamref name="T"/> whose value is <paramref name="value"/>.
        /// </summary>
        void MenuRebuildSetCheck<T>(string? value)
        {
            if (string.IsNullOrEmpty( value))
                return;
            var tagList = tsItemsSetup.Where(x => x.Tag != null).ToList();
            var typeList = tagList.Where(x => typeof(T) == x.Tag.GetType()).ToList();
            var tsSelected = typeList.Where(x => x.Tag.ToString()==value).ToList();
            foreach (var item in tsSelected)
            {
                item.Checked = true;
            }
        }

        /// <summary>
        /// Checks the menu item of the given resolution ("Original" when null).
        /// </summary>
        void MenuRebuildSetCheck(GraphicsResolution? res)
        {
            if (res == null)
            {
                originalToolStripMenuItem.Checked = true;
            }
            else
            {
                var item = tsItemsResolution.FirstOrDefault(x => x.Tag == res);
                if (item != null)
                    item.Checked = true;
            }
        }

        /// <summary>
        /// Synchronizes the menu checks with the settings of the selected analyzer.
        /// </summary>
        void MenuRebuildSetChecks()
        {
            tsItemsSetup.ForEach(x => x.Checked = false);
            MenuRebuildSetCheck<ColorQuantizationMode>(SelectedColorAnalyzer?.QuantizationMode.ToString());
            MenuRebuildSetCheck<ColorDistanceEvaluationMode>(SelectedColorAnalyzer?.ColorDistanceEvaluationMode.ToString());
            MenuRebuildSetCheck<ColorDithering>(SelectedColorAnalyzer?.DitheringType.ToString());
            MenuRebuildSetCheck<ColorDitheringFx>(SelectedColorAnalyzer?.DitheringFx.ToString());
            tsItemsResolution.ForEach(x => x.Checked = false);
            MenuRebuildSetCheck(SelectedColorAnalyzer?.WantedRes);
        }
        

        /// <summary>
        /// Applies the clicked setting to the selected analyzer.
        /// </summary>
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

        /// <summary>
        /// Applies the clicked resolution to the selected analyzer and reloads its image.
        /// </summary>
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

        /// <summary>
        /// Builds the resolution menu.
        /// </summary>
        void CreateScaleMenuItems()
        {
            originalToolStripMenuItem.Click += TsItem_ClickResolution;
            tsItemsResolution.Add(originalToolStripMenuItem);
            foreach (var res in gfxRes)
            {
                var menuItem = new System.Windows.Forms.ToolStripMenuItem();
                menuItem.Name = "tsi" + res.Name;
                menuItem.Size = new System.Drawing.Size(180, 22);
                menuItem.Text = res.Name;
                menuItem.CheckOnClick = true;
                menuItem.Tag = res;
                menuItem.Click += TsItem_ClickResolution;
                tsmiResolution.DropDownItems.Add(menuItem);
                tsItemsResolution.Add(menuItem);
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

        /// <summary>
        /// Returns the processed image of the selected analyzer.
        /// </summary>
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

        private void pngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageTools.SaveImage(GetProcessedImageData(), sfdExportImage.FileName, ImageSaveFormat.Png);
            }
        }

     
    }
}