using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Defaults;
using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColourClashNet.Controls
{
    public partial class ColorAnalyzer : UserControl
    {


        public class GraphicsResolution
        {
            public string Name => $"{Width}x{Height}";
            public int Width { get; set; }
            public int Height { get; set; }
        }

        public class CopyDataEventArgs : EventArgs
        {
            public string Name { get; set; }
            public Image SourceBitmap { get; set; }
            public Image DestBitmap { get; set; }
            public Rectangle DestBitmapRoi { get; set; }
        }

        public class DataSourceEventArgs : EventArgs
        {
            public String Name { get; set; }
            public Image SourceBitmap { get; set; }
            public FileInfo SourceFile { get; set; }
        }


        public event EventHandler<CopyDataEventArgs> ImageCopied;
        public event EventHandler<DataSourceEventArgs> ImageCreated;

        int iPanel1MaxWidth = 400;

        FileInfo oFileInfo = null;
        String oName = "";


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


        public ColorAnalyzer()
        {
            InitializeComponent();
            oColorManager.Config.BackgroundColorList = GetBkgColors();
            oColorManager.Config.ColorQuantizationMode = ColorQuantizationMode.RGB888;//GetQuantizationMode();
            oColorManager.Config.DitheringAlgorithm = ColorDithering.FloydSteinberg;
            pbBkColor.BackColor = ColorDefaults.DefaultBkgColor;
            InitMenu();
            CreateComboBox(cbC64VideoMode, Enum.GetNames(typeof(ColorTransformReductionC64.C64VideoMode)).ToList());
            CreateComboBox(cbCpcVideoMode, Enum.GetNames(typeof(ColorTransformReductionCPC.CPCVideoMode)).ToList());
            CreateComboBox(cbAmigaVideoMode, Enum.GetNames(typeof(ColorTransformReductionAmiga.EnumAmigaVideoMode)).ToList());
            CreateComboBox(cbZxPaletteMode, Enum.GetNames(typeof(ColorTransformReductionZxSpectrum.ZxPaletteMode)).ToList());
            oBitmapRenderSource.ColorAdded += (s, e) => { BuildBkgPalette(); };
            oBitmapRenderSource.ColorRemoved += (s, e) => { BuildBkgPalette(); };
            oColorManager.OnQuantize += (s, e) => { Invoke( ()=> RefreshData() ); };
            oColorManager.OnProcess += (s, e) => { Invoke(() => { RefreshData(); RebuildParams(e.Transformation); }); };
            CreateScaleMenuItems();
        }

        private void RebuildParams(object sender)
        {
            if (sender is ColorTransformReductionZxSpectrum oZX)
            {
                nudZxColorLO.Value = oZX.ZxLowColorInSeed;
                nudZxColorHI.Value = oZX.ZxHighColorInSeed;
            }
        }

        List<ToolStripMenuItem> lTsItems = new List<ToolStripMenuItem>();
        List<ToolStripMenuItem> lTsItemsRes = new List<ToolStripMenuItem>();

        void RebulidSetCheck(string sItem)
        {
            var ts = lTsItems.FirstOrDefault(X => X.Tag?.ToString() == sItem);
            if (ts != null)
                ts.Checked = true;
        }
        void RebulidSetCheck(GraphicsResolution oRes)
        {
            if (oRes == null)
            {
                originalToolStripMenuItem.Checked = true;
            }
            else
            {
                var ts = lTsItemsRes.FirstOrDefault(X => X.Tag == oRes);
                if (ts != null)
                    ts.Checked = true;
            }
        }

        void RebuildChecks()
        {
            lTsItems.ForEach(X => X.Checked = false);
            RebulidSetCheck(oColorManager.Config.ColorQuantizationMode.ToString());
            RebulidSetCheck(oColorManager.Config.ColorDistanceEvaluationMode.ToString());
            RebulidSetCheck(oColorManager.Config.DitheringAlgorithm.ToString());
            lTsItemsRes.ForEach(X => X.Checked = false);
            RebulidSetCheck(oWantedRes);
        }

        private void TsItem_Click(object? sender, EventArgs e)
        {
            var oTS = sender as ToolStripMenuItem;
            var oTag = oTS.Tag;
            if (oTag is ColorQuantizationMode)
            {
                oColorManager.Config.ColorQuantizationMode = (ColorQuantizationMode)oTag;
                oColorManager.PreProcess();
            }
            else if (oTag is ColorDistanceEvaluationMode)
            {
                oColorManager.Config.ColorDistanceEvaluationMode = (ColorDistanceEvaluationMode)oTag;
            }
            else if (oTag is ColorDithering)
            {
                oColorManager.Config.DitheringAlgorithm = (ColorDithering)oTag;
            }
            RebuildChecks();
        }

        GraphicsResolution oWantedRes = null;

        private void TsItem_ClickRes(object? sender, EventArgs e)
        {
            var oTS = sender as ToolStripMenuItem;
            var oTag = oTS.Tag;
            oWantedRes = null;
            if (oTag is GraphicsResolution oGfxRes)
            {
                oWantedRes = oGfxRes;
            }
            RebuildChecks();
            oColorManager.Create(ResizeBitmap(oLoadedBmp));
        }


        void CreateScaleMenuItems()
        {
            originalToolStripMenuItem.Click += TsItem_ClickRes;
            lTsItemsRes.Add(originalToolStripMenuItem);
            foreach (var res in lGfxRes)
            {
                var tsItem = new System.Windows.Forms.ToolStripMenuItem();
                tsItem.Name = "tsi" + res.Name;
                tsItem.Size = new System.Drawing.Size(180, 22);
                tsItem.Text = res.Name;
                tsItem.CheckOnClick = true;
                tsItem.Tag = res;
                tsItem.Click += TsItem_ClickRes;
                tsmiResolution.DropDownItems.Add(tsItem);
                lTsItemsRes.Add(tsItem);
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
            tsItem.Click += TsItem_Click;
            oTsBase.DropDownItems.Add(tsItem);
            lTsItems.Add(tsItem);
        }

        void CreateComboBox(ComboBox ocb, List<string> lItems)
        {
            ocb.Items.Clear();
            lItems.ForEach(X => ocb.Items.Add(X));
            ocb.SelectedIndex = 0;
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
            RebuildChecks();
        }

        void BuildBkgPalette()
        {
            pbBkColor.Image = null;
            var lBkgColor = oBitmapRenderSource.SelectedColors;
            if (lBkgColor.Count > 0)
            {
                Bitmap oBmp = new Bitmap((int)lBkgColor.Count * 16, (int)16);
                using (var g = Graphics.FromImage(oBmp))
                {
                    int index = 0;
                    for (int i = 0; i < lBkgColor.Count; i++)
                    {
                        var item = lBkgColor[i];
                        using (var oBrush = new SolidBrush(item))
                        {
                            g.FillRectangle(oBrush, i * 16, 0, 16, 16);
                        }
                        index++;
                    }
                    ;
                }
                pbBkColor.SizeMode = PictureBoxSizeMode.StretchImage;
                pbBkColor.Image = oBmp;
            }
            oColorManager.Config.BackgroundColorList = GetBkgColors();
            oColorManager.Config.BackgroundColorReplacement = ColorIntExt.FromDrawingColor(ColorDefaults.DefaultBkgColor);
            oColorManager.PreProcess();
        }

        void ShowImage()
        {
            oBitmapRenderSource.Image = oColorManager.ImageSource?.Clone() as Image;
            oBitmapRenderDest.Image = oColorManager.ImageProcessed?.Clone() as Image;
            Invalidate();
        }

        void RefreshData()
        {
            pgColorProcessor.SelectedObject = oColorManager;
            ShowImage();
        }

        Bitmap oLoadedBmp;

        public Bitmap ResizeBitmap(Bitmap oSrcBmp)
        {
            if (oSrcBmp == null)
            {
                return null;
            }
            if (oWantedRes == null)
            {
                return oSrcBmp;
            }
            else
            {
                var oBmp2 = new Bitmap(oWantedRes.Width, oWantedRes.Height);
                using (var oG = Graphics.FromImage(oBmp2))
                {
                    oG.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    oG.DrawImage(oSrcBmp, new Rectangle(0, 0, oBmp2.Width, oBmp2.Height), new Rectangle(0, 0, oSrcBmp.Width, oSrcBmp.Height), GraphicsUnit.Pixel);
                }
                return oBmp2;
            }
        }

        public void Create(Image oImage, string sName)
        {
            oLoadedBmp = oImage as Bitmap;
            oBitmapRenderDest.ResetMouseSelectedColors();
            oBitmapRenderDest.OriginZero();
            oBitmapRenderSource.ResetMouseSelectedColors();
            oBitmapRenderSource.OriginZero();
            //
            oColorManager.Config.BackgroundColorList = GetBkgColors();
            oColorManager.Config.BackgroundColorReplacement = ColorIntExt.FromDrawingColor(ColorDefaults.DefaultBkgColor);
            oColorManager.Create(ResizeBitmap(oLoadedBmp));
            oName = sName;  
            ImageCreated?.Invoke(this, new DataSourceEventArgs()
            {
                 Name = oName,
                 SourceBitmap = oLoadedBmp,
            });            
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ofdSelectImage.ShowDialog() == DialogResult.OK)
            {
                oFileInfo = new FileInfo(ofdSelectImage.FileName);
                Create(Bitmap.FromFile(oFileInfo.FullName), oFileInfo.Name);
            }
        }


        List<int> GetBkgColors()
        {
            var oRet = new List<int>();
            foreach (var item in oBitmapRenderSource.SelectedColors)
                oRet.Add(ColorIntExt.FromDrawingColor(item));
            return oRet;
        }




        private void SetToControl()
        {
            oColorManager.Config.BackgroundColorList = GetBkgColors();
            oColorManager.Config.BackgroundColorReplacement = ColorIntExt.FromDrawingColor(ColorDefaults.DefaultBkgColor);

            oColorManager.Config.ColorsMax = (int)nudColorsWanted.Value;
            oColorManager.Config.ScanlineClustering = chkScanLineCluster.Checked;
            oColorManager.Config.ScanlineColorsMax = (int)nudScanlineLineColors.Value;
            oColorManager.Config.ScanlineSharedPalette = chkScanlineSharedPal.Checked;

            oColorManager.Config.ClusteringTrainingLoop = (int)nudClusterLoop.Value;
            oColorManager.Config.ClusteringUseMeanColor = true;
            oColorManager.Config.DitheringStrenght = (double)nudDitheringStrenght.Value;
            oColorManager.Config.SaturationEnhancement = (double)nudSat.Value;
            oColorManager.Config.BrightnessEnhancement = (double)nudBright.Value;
            oColorManager.Config.HsvHueOffset = (double)nudHue.Value;
            oColorManager.Config.C64ScreenMode = (ColorTransformReductionC64.C64VideoMode)Enum.Parse(typeof(ColorTransformReductionC64.C64VideoMode), cbC64VideoMode.SelectedItem.ToString());
            oColorManager.Config.CPCScreenMode = (ColorTransformReductionCPC.CPCVideoMode)Enum.Parse(typeof(ColorTransformReductionCPC.CPCVideoMode), cbCpcVideoMode.SelectedItem.ToString());
            oColorManager.Config.ZxEqAutotune = (bool)chkZxAutotune.Checked;
            oColorManager.Config.ZxEqColorLO = (int)nudZxColorLO.Value;
            oColorManager.Config.ZxEqColorHI = (int)nudZxColorHI.Value;
            oColorManager.Config.ZxIncludeBlackHI = chkZxBlackHI.Checked;
            oColorManager.Config.ZxEqDitherHI = chkZxDitherHI.Checked;
            oColorManager.Config.ZxPaletteMode = (ColorTransformReductionZxSpectrum.ZxPaletteMode)Enum.Parse(typeof(ColorTransformReductionZxSpectrum.ZxPaletteMode), cbZxPaletteMode.SelectedItem.ToString());
            oColorManager.Config.AmigaScreenMode = (ColorTransformReductionAmiga.EnumAmigaVideoMode)Enum.Parse(typeof(ColorTransformReductionAmiga.EnumAmigaVideoMode), cbAmigaVideoMode.SelectedItem.ToString());
        }

        private async void btnReduceColors_Click(object sender, EventArgs e)
        {
            SetToControl();
            await oColorManager.ProcessColorsAsync(ColorTransformType.ColorReductionMedianCut);
        }
        private async void btnReduceColorsScanline_Click(object sender, EventArgs e)
        {
            SetToControl();
            await oColorManager.ProcessColorsAsync(ColorTransformType.ColorReductionScanline);
        }

        private async void btnReduceColorCluster_Click(object sender, EventArgs e)
        {
            SetToControl();
            await oColorManager.ProcessColorsAsync(ColorTransformType.ColorReductionClustering);
        }

        private void pbBkColor_DoubleClick(object sender, EventArgs e)
        {
            //if (lBkgColor.Count > 0)
            //{
            //    lBkgColor.RemoveAt(lBkgColor.Count - 1);
            //    Reprocess();
            //}
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            oColorManager.Reset();
        }

        private void toolStripMenuItemSave_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageToolsGDI.GdiImageToFile(oColorManager.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageExportFormat.Png);
            }
        }


        private void oColorManager_OnReset(object sender, EventArgs e)
        {
            RefreshData();
        }


        private void bitmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageToolsGDI.GdiImageToFile(oColorManager.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageExportFormat.Bmp);
            }
        }

        private void pNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageToolsGDI.GdiImageToFile(oColorManager.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageExportFormat.Png);
            }
        }

        private void bitmapIndexedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                // oColorManager.WriteBitmapIndex(sfdExportImage.FileName, ImageWidthAlignMode.MultiplePixel16);
            }
        }
        private void bitplaneToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                // oColorManager.WriteBitplane(sfdExportImage.FileName, ImageWidthAlignMode.MultiplePixel16, false);
            }
        }





        private async void btnReduceColorsZx_Click(object sender, EventArgs e)
        {
            SetToControl();
            await oColorManager.ProcessColorsAsync(ColorTransformType.ColorReductionZxSpectrum);
        }


        private async void btnReduceColorsEga_Click(object sender, EventArgs e)
        {
            SetToControl();
            await oColorManager.ProcessColorsAsync(ColorTransformType.ColorReductionEga);
        }

        private async void BtnReduceColorsC64v1_Click(object sender, EventArgs e)
        {
            SetToControl();
            await oColorManager.ProcessColorsAsync(ColorTransformType.ColorReductionCBM64);
        }

        private async void btnReduceColorCPC_Click(object sender, EventArgs e)
        {
            SetToControl();
            await oColorManager.ProcessColorsAsync(ColorTransformType.ColorReductionCPC);
        }

        private async void btnChromaAdapt_Click(object sender, EventArgs e)
        {
            SetToControl();
            await oColorManager.ProcessColorsAsync(ColorTransformType.ColorReductionSaturation);
        }

        private async void btnReduceHam_Click(object sender, EventArgs e)
        {
            SetToControl();
            await oColorManager.ProcessColorsAsync(ColorTransformType.ColorReductionHam);
        }

        bool bDitherStrenghtUpdating = false;
        private void tbDitherStrenght_Scroll(object sender, EventArgs e)
        {
            if (bDitherStrenghtUpdating)
                return;
            bDitherStrenghtUpdating = true;
            nudDitheringStrenght.Value = (decimal)tbDitherStrenght.Value / 100.0M;
            bDitherStrenghtUpdating = false;
        }

        private void nudDitheringStrenght_ValueChanged(object sender, EventArgs e)
        {
            if (bDitherStrenghtUpdating)
                return;
            bDitherStrenghtUpdating = true;
            tbDitherStrenght.Value = (int)Math.Max(0, Math.Min(100, nudDitheringStrenght.Value * 100));
            bDitherStrenghtUpdating = false;
        }

        private void gbLayoutV_CheckedChanged(object sender, EventArgs e)
        {
            splitRenderLayout.Orientation = Orientation.Vertical;
        }

        private void rbLayoutH_CheckedChanged(object sender, EventArgs e)
        {
            splitRenderLayout.Orientation = Orientation.Horizontal;
        }

        private void scMain_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (splitMain.Panel1.Width > iPanel1MaxWidth)
            {
                splitMain.SplitterDistance = iPanel1MaxWidth;
            }
        }

        private void scMain_SizeChanged(object sender, EventArgs e)
        {
            if (splitMain.Panel1.Width > iPanel1MaxWidth)
            {
                splitMain.SplitterDistance = iPanel1MaxWidth;
            }
        }

        private void oBitmapRenderSource_MouseMove(object sender, MouseEventArgs e)
        {
            var oPointC = oBitmapRenderSource.MouseControlCoordinates;
            tsMouseCoord.Text = $"X:{oPointC.X} Y:{oPointC.Y}";
            var oPointI = oBitmapRenderSource.MouseImageCoordinatesClip;
            tsImageCoord.Text = $"X:{oPointI.X:f2} Y:{oPointI.Y:f2}";
            tsColorData.BackColor = oBitmapRenderSource.MouseImageColor;
        }

        private void zxBtnReset_Click(object sender, EventArgs e)
        {
            if (nudZxColorLO.Value <= 128)
                nudZxColorLO.Value = 0;
            else
                nudZxColorLO.Value = 128;
            nudZxColorHI.Value = 255;
        }

        private void newContainerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var oS = this.oColorManager.ImageSource;
            var oD = this.oColorManager.ImageProcessed;
            if (oS == null || oD == null)
            {
                return;
            }
            ImageCopied?.Invoke(this, new CopyDataEventArgs()
            {
                Name = oName,
                SourceBitmap = oS,
                DestBitmap = oD,
                DestBitmapRoi = new Rectangle(0, 0, oD.Width, oD.Height)
            }); 
        }
    }
}
