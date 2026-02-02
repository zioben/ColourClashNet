using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Defaults;
using ColourClashNet.Drawing;
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

        public GraphicsResolution WantedRes { get; set; } = null;

        public ColorManagerConfig Config => oColorManager.Config;

        public Components.ColorManager ColorManager => oColorManager;

        public ColorAnalyzer()
        {
            InitializeComponent();
            oColorManager.Config.BackgroundColorList = GetBkgColors();
            oColorManager.Config.ColorQuantizationMode = ColorQuantizationMode.RGB888;//GetQuantizationMode();
            oColorManager.Config.DitheringAlgorithm = ColorDithering.FloydSteinberg;
            pbBkColor.BackColor = ColorDefaults.DefaultBkgColor;
            CreateComboBox(cbC64VideoMode, Enum.GetNames(typeof(ColorTransformReductionC64.C64VideoMode)).ToList());
            CreateComboBox(cbCpcVideoMode, Enum.GetNames(typeof(ColorTransformReductionCPC.CPCVideoMode)).ToList());
            CreateComboBox(cbAmigaVideoMode, Enum.GetNames(typeof(ColorTransformReductionAmiga.EnumAmigaVideoMode)).ToList());
            CreateComboBox(cbZxPaletteMode, Enum.GetNames(typeof(ColorTransformReductionZxSpectrum.ZxPaletteMode)).ToList());
            CreateComboBox(cbZxAutotuneMode, Enum.GetNames(typeof(ColorTransformReductionZxSpectrum.ZxAutotuneMode)).ToList());
            oBitmapRenderSource.ColorAdded += (s, e) => { BuildBkgPalette(); };
            oBitmapRenderSource.ColorRemoved += (s, e) => { BuildBkgPalette(); };
            oColorManager.OnQuantize += (s, e) => { Invoke( ()=> RefreshData() ); };
            oColorManager.OnProcess += (s, e) => { Invoke(() => { RefreshData(); RebuildParams(e.Transformation); }); };
        }

        public void Preprocess() => oColorManager.PreProcess();

        void CreateComboBox(ComboBox ocb, List<string> lItems)
        {
            ocb.Items.Clear();
            lItems.ForEach(X => ocb.Items.Add(X));
            ocb.SelectedIndex = 0;
        }


        private void RebuildParams(object sender)
        {
            if (sender is ColorTransformReductionZxSpectrum oZX)
            {
                nudZxColorLO.Value = oZX.ZxLowColorInSeed;
                nudZxColorHI.Value = oZX.ZxHighColorInSeed;
            }
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
            if (WantedRes == null)
            {
                return oSrcBmp;
            }
            else
            {
                var oBmp2 = new Bitmap(WantedRes.Width, WantedRes.Height);
                using (var oG = Graphics.FromImage(oBmp2))
                {
                    oG.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    oG.DrawImage(oSrcBmp, new Rectangle(0, 0, oBmp2.Width, oBmp2.Height), new Rectangle(0, 0, oSrcBmp.Width, oSrcBmp.Height), GraphicsUnit.Pixel);
                }
                return oBmp2;
            }
        }

        public void Create()
        {
            oBitmapRenderDest.ResetMouseSelectedColors();
            oBitmapRenderDest.OriginZero();
            oBitmapRenderSource.ResetMouseSelectedColors();
            oBitmapRenderSource.OriginZero();
            //
            oColorManager.Config.BackgroundColorList = GetBkgColors();
            oColorManager.Config.BackgroundColorReplacement = ColorIntExt.FromDrawingColor(ColorDefaults.DefaultBkgColor);
            oColorManager.Create(ResizeBitmap(oLoadedBmp));
            ImageCreated?.Invoke(this, new DataSourceEventArgs()
            {
                Name = oName,
                SourceBitmap = oLoadedBmp,
            });
        }

        public void Create(Image image, string name)
        {
            oLoadedBmp = image as Bitmap;
            oName = name;
            Create();
        }

        public void Create(FileInfo fileInfo)
        {
            var image = Image.FromFile(fileInfo.FullName);
            Create(image, fileInfo.Name);
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
            oColorManager.Config.ZxEqAutotuneMode = (ColorTransformReductionZxSpectrum.ZxAutotuneMode)Enum.Parse(typeof(ColorTransformReductionZxSpectrum.ZxAutotuneMode), cbZxAutotuneMode.SelectedItem.ToString());
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


        private void oColorManager_OnReset(object sender, EventArgs e)
        {
            RefreshData();
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
