using ColourClashLib;
using ColourClashLib.Color.Transformation;
using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Drawing;
using ColourClashNet.Imaging;
using ColourClashProcessing.Blob;
using System;
using System.Collections.Concurrent;
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

        int panel1MaxWidth = 400;

        FileInfo currentFileInfo = null;
        String currentName = "";

        [DefaultValue(null)]
        public GraphicsResolution WantedRes { get; set; } = null;

        [DefaultValue(ColorQuantizationMode.RGB888)]
        public ColorQuantizationMode QuantizationMode { get; set; } = ColorQuantizationMode.RGB888;

        [DefaultValue(ColorDithering.FloydSteinberg)]
        public ColorDithering DitheringType { get; set; } = ColorDithering.FloydSteinberg;

        [DefaultValue(ColorDitheringFx.None)]
        public ColorDitheringFx DitheringFx { get; set; } = ColorDitheringFx.None;

        [DefaultValue(ColorDistanceEvaluationMode.RGB)]
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.RGB;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Blob BlobAnalysis { get; set; } = new Blob();

        //public ColorTransformConfig Config => colorManager.Config;

        public Components.ColorTransformManager ColorManager => colorManager;

        public ColorAnalyzer()
        {
            InitializeComponent();
            colorManager.TransformConfig
                .WithBackgroundColorReplacement(GetBkgColors(), ColorDefaults.DefaultBkgColorInt)
                .WithQuantizationMode(QuantizationMode)
                .WithColorDistanceEvaluationMode(ColorDistanceEvaluationMode)
                .WithDithering(DitheringType, 1.0, DitheringFx);
            CreateComboBox(cbC64VideoMode, Enum.GetNames(typeof(ColorTransformCbm64.C64VideoMode)).ToList());
            CreateComboBox(cbCpcVideoMode, Enum.GetNames(typeof(ColorTransformAmstradCPC.CPCVideoMode)).ToList());
            CreateComboBox(cbAmigaVideoMode, Enum.GetNames(typeof(ColorTransformCbmAmiga.EnumAmigaVideoMode)).ToList());
            CreateComboBox(cbZxPaletteMode, Enum.GetNames(typeof(ColorTransformZxSpectrum.ZxPaletteMode)).ToList());
            CreateComboBox(cbZxAutotuneMode, Enum.GetNames(typeof(ColorTransformZxSpectrum.ZxAutotuneMode)).ToList());
            CreateComboBox(cbCgaVideoMode, Enum.GetNames(typeof(ColorTransformCGA.CGAVideoMode)).ToList());
            CreateComboBox(cbCgaVideoOutput, Enum.GetNames(typeof(ColorTransformCGA.CGACompositeOutputMode)).ToList());
            CreateComboBox(cbBlobConnectionMode, Enum.GetNames(typeof(BlobConnectionModes)).ToList());
            bitmapRenderSource.ColorAdded += (s, e) => { BuildBkgPalette(); };
            bitmapRenderSource.ColorRemoved += (s, e) => { BuildBkgPalette(); };
            colorManager.OnQuantize += (s, e) => { Invoke(() => RefreshData()); };
            colorManager.OnProcess += (s, e) => { Invoke(() => { RefreshData(); RebuildParams(e.Transformation); }); };
            blobManager.OnPreProcess += (s, e) => { blobManager.WithProcessingParams(colorManager.TransformConfig.ColorBackgroundPalette, colorManager.TransformConfig.ColorBackgroundReplacementValue); };
        }                

        public void Preprocess()
        {
            SetControlToConfig();
            colorManager.PreProcess();
        }

        void CreateComboBox(ComboBox ocb, List<string> items)
        {
            ocb.Items.Clear();
            items.ForEach(x => ocb.Items.Add(x));
            ocb.SelectedIndex = 0;
        }


        private void RebuildParams(object sender)
        {
            if (sender is ColorTransformZxSpectrum zxTransform)
            {
                nudZxColorLO.Value = zxTransform.ZxPaletteInColorSeedLow;
                nudZxColorHI.Value = zxTransform.ZxPaletteInColorSeedHigh;
            }
        }




        void BuildBkgPalette()
        {
            pbBkColor.Image = null;
            var bkgColor = bitmapRenderSource.SelectedColors;
            if (bkgColor.Count > 0)
            {
                Bitmap bmp = new Bitmap((int)bkgColor.Count * 16, (int)16);
                using (var g = Graphics.FromImage(bmp))
                {
                    int index = 0;
                    for (int i = 0; i < bkgColor.Count; i++)
                    {
                        var item = bkgColor[i];
                        using (var brush = new SolidBrush(item))
                        {
                            g.FillRectangle(brush, i * 16, 0, 16, 16);
                        }
                        index++;
                    }
                    ;
                }
                pbBkColor.SizeMode = PictureBoxSizeMode.StretchImage;
                pbBkColor.Image = bmp;
            }
            colorManager.TransformConfig.WithBackgroundColorReplacement(GetBkgColors(), ColorDefaults.DefaultBkgColorInt);
            colorManager.PreProcess();
        }

        void ShowImage()
        {
            bitmapRenderSource.Image = colorManager.ImageSource?.Clone() as Image;
            bitmapRenderDest.Image = colorManager.ImageProcessed?.Clone() as Image;
            Invalidate();
        }

        void RefreshData()
        {
            pgColorProcessor.SelectedObject = colorManager;
            pgColorProcessorConfig.SelectedObject = colorManager?.TransformConfig;
            ShowImage();
        }

        Bitmap loadedBmp;

        public Bitmap ResizeBitmap(Bitmap srcBmp)
        {
            if (srcBmp == null)
            {
                return null;
            }
            if (WantedRes == null)
            {
                return srcBmp;
            }
            else
            {
                var bmp2 = new Bitmap(WantedRes.Width, WantedRes.Height);
                using (var graphics = Graphics.FromImage(bmp2))
                {
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    graphics.DrawImage(srcBmp, new Rectangle(0, 0, bmp2.Width, bmp2.Height), new Rectangle(0, 0, srcBmp.Width, srcBmp.Height), GraphicsUnit.Pixel);
                }
                return bmp2;
            }
        }

        public void Create()
        {
            bitmapRenderDest.ResetMouseSelectedColors();
            bitmapRenderDest.OriginZero();
            bitmapRenderSource.ResetMouseSelectedColors();
            bitmapRenderSource.OriginZero();
            //
            colorManager.TransformConfig.WithBackgroundColorReplacement(GetBkgColors(), ColorDefaults.DefaultBkgColorInt);
            colorManager.Create(ResizeBitmap(loadedBmp));
            ImageCreated?.Invoke(this, new DataSourceEventArgs()
            {
                Name = currentName,
                SourceBitmap = loadedBmp,
            });
        }

        public void Create(ImageData image, string name)
        {
            loadedBmp = ImageToolsGDI.ImageDataToGdiImage(image) as Bitmap;
            currentName = name;
            Create();
        }

        public void Create(Image image, string name)
        {
            loadedBmp = image as Bitmap;
            currentName = name;
            Create();
        }

        public void Create(FileInfo fileInfo)
        {
            var image = ImageTools.LoadImage(fileInfo.FullName);
            Create(image, fileInfo.Name);
        }



        Palette GetBkgColors()
        {
            var ret = new List<int>();
            foreach (var item in bitmapRenderSource.SelectedColors)
                ret.Add(ColorIntExtGdi.FromDrawingColor(item));
            return new Palette().Create(ret);
        }




        private void SetControlToConfig()
        {
            colorManager.TransformConfig.WithBackgroundColorReplacement(
                GetBkgColors(),
                ColorDefaults.DefaultBkgColorInt);
            //
            colorManager.TransformConfig.WithQuantizationMode(QuantizationMode);
            //
            colorManager.TransformConfig.WithColorDistanceEvaluationMode(ColorDistanceEvaluationMode);
            //
            colorManager.TransformConfig.WithScanline(
                (int)numLinesPerBlock.Value,
                chkScanlineSharedPal.Checked ? ColorTransformScanLine.ScanlinePaletteMode.SharedChunksPalette : ColorTransformScanLine.ScanlinePaletteMode.IndependentChunkPalette,
                (int)nudColorsWanted.Value,
                (int)nudScanlineLineColors.Value,
                ColorTransformType.ColorReductionClustering);
            //
            colorManager.TransformConfig.WithClustering(
                (int)nudColorsWanted.Value,
                (int)nudClusterLoop.Value,
                ColorSelectionMode.UseColorPalette);
            // Dithering viene settato dalla form
            colorManager.TransformConfig.WithDithering(
                DitheringType,
                (double)nudDitheringStrenght.Value,
                DitheringFx);
            //
            colorManager.TransformConfig.WithHSV(
                (double)nudSat.Value,
                (double)nudBright.Value,
                (double)nudHue.Value);
            //
            colorManager.TransformConfig.WithC64ScreenMode(
               (ColorTransformCbm64.C64VideoMode)Enum.Parse(typeof(ColorTransformCbm64.C64VideoMode), cbC64VideoMode.SelectedItem.ToString()),
               ColorTransformCbm64.C64DitheringMode.PreDitherImage,
               chkShowTileBorders.Checked);
            //
            colorManager.TransformConfig.WithCpcVideoMode(
                (ColorTransformAmstradCPC.CPCVideoMode)Enum.Parse(typeof(ColorTransformAmstradCPC.CPCVideoMode), cbCpcVideoMode.SelectedItem.ToString()));
            //
            colorManager.TransformConfig.WithZxScreenMode(
                 (ColorTransformZxSpectrum.ZxPaletteMode)Enum.Parse(typeof(ColorTransformZxSpectrum.ZxPaletteMode), cbZxPaletteMode.SelectedItem.ToString()),
                 (int)nudZxColorLO.Value,
                 (int)nudZxColorHI.Value);
            //
            colorManager.TransformConfig.WithZxProcessing(
                 (ColorTransformZxSpectrum.ZxAutotuneMode)Enum.Parse(typeof(ColorTransformZxSpectrum.ZxAutotuneMode), cbZxAutotuneMode.SelectedItem.ToString()),
                 chkZxBlackHI.Checked,
                 chkZxDitherHI.Checked,
                 chkZxBlackHI.Checked,
                 chkShowTileBorders.Checked);
            //
            colorManager.TransformConfig.WithAmigaScreenMode(
                (ColorTransformCbmAmiga.EnumAmigaVideoMode)Enum.Parse(typeof(ColorTransformCbmAmiga.EnumAmigaVideoMode),
                 cbAmigaVideoMode.SelectedItem.ToString()),
                 ColorTransformType.ColorReductionClustering,
                 ColorSelectionMode.UseColorPalette
                 );

            colorManager.TransformConfig.WithCGAScreenMode(
                (ColorTransformCGA.CGAVideoMode)Enum.Parse(typeof(ColorTransformCGA.CGAVideoMode), cbCgaVideoMode.SelectedItem.ToString()),
                (ColorTransformCGA.CGACompositeOutputMode)Enum.Parse(typeof(ColorTransformCGA.CGACompositeOutputMode), cbCgaVideoOutput.SelectedItem.ToString())
                );



        }

        private void pbBkColor_DoubleClick(object sender, EventArgs e)
        {
            //if (bkgColor.Count > 0)
            //{
            //    bkgColor.RemoveAt(bkgColor.Count - 1);
            //    Reprocess();
            //}
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorManager.Reset();
        }


        private void oColorManager_OnReset(object sender, EventArgs e)
        {
            RefreshData();
        }


        private async void ReduceColorsAsync(ColorTransformType type)
        {
            SetControlToConfig();
            await colorManager.ProcessColorsAsync(type);
        }

        private async void btnReduceColors_Click(object sender, EventArgs e)
            => ReduceColorsAsync(ColorTransformType.ColorReductionMedianCut);

        private async void btnReduceColorsScanline_Click(object sender, EventArgs e)
            => ReduceColorsAsync(ColorTransformType.ColorReductionScanline);

        private async void btnReduceColorCluster_Click(object sender, EventArgs e)
            => ReduceColorsAsync(ColorTransformType.ColorReductionClustering);


        private async void btnReduceColorsZx_Click(object sender, EventArgs e)
            => ReduceColorsAsync(ColorTransformType.ColorReductionZxSpectrum);

        private async void btnReduceColorsHercules_Click(object sender, EventArgs e)
            => ReduceColorsAsync(ColorTransformType.ColorReductionHercules);

        private async void btnReduceColorsCGA_Click(object sender, EventArgs e)
            => ReduceColorsAsync(ColorTransformType.ColorReductionCGA);
        private async void btnReduceColorsEga_Click(object sender, EventArgs e)
            => ReduceColorsAsync(ColorTransformType.ColorReductionEGA);

        private async void btnReduceColorsVGA_Click(object sender, EventArgs e)
            => ReduceColorsAsync(ColorTransformType.ColorReductionVGA);


        private async void btnReduceColorsC64v1_Click(object sender, EventArgs e)
            => ReduceColorsAsync(ColorTransformType.ColorReductionCBM64);


        private async void btnReduceColorCPC_Click(object sender, EventArgs e)
            => ReduceColorsAsync(ColorTransformType.ColorReductionCPC);

        private async void btnChromaAdapt_Click(object sender, EventArgs e)
            => ReduceColorsAsync(ColorTransformType.ColorReductionSaturation);


        private async void btnReduceHam_Click(object sender, EventArgs e)
            => ReduceColorsAsync(ColorTransformType.ColorReductionHam);


        bool ditherStrengthUpdating = false;
        private void tbDitherStrenght_Scroll(object sender, EventArgs e)
        {
            if (ditherStrengthUpdating)
                return;
            ditherStrengthUpdating = true;
            nudDitheringStrenght.Value = (decimal)tbDitherStrenght.Value / 100.0M;
            ditherStrengthUpdating = false;
        }

        private void nudDitheringStrenght_ValueChanged(object sender, EventArgs e)
        {
            if (ditherStrengthUpdating)
                return;
            ditherStrengthUpdating = true;
            tbDitherStrenght.Value = (int)Math.Max(0, Math.Min(100, nudDitheringStrenght.Value * 100));
            ditherStrengthUpdating = false;
        }




        private void scMain_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (splitMainA_BC.Panel1.Width > panel1MaxWidth)
            {
                splitMainA_BC.SplitterDistance = panel1MaxWidth;
            }
            if (splitMainB_C.Panel2.Width > panel1MaxWidth)
            {
                splitMainB_C.SplitterDistance = panel1MaxWidth;
            }
        }

        private void scMain_SizeChanged(object sender, EventArgs e)
        {
            if (splitMainA_BC.Panel1.Width > panel1MaxWidth)
            {
                splitMainA_BC.SplitterDistance = panel1MaxWidth;
            }
            if (splitMainB_C.Panel2.Width > panel1MaxWidth)
            {
                splitMainB_C.SplitterDistance = panel1MaxWidth;
            }
        }

        private void oBitmapRenderSource_MouseMove(object sender, MouseEventArgs e)
        {
            var pointC = bitmapRenderSource.MouseControlCoordinates;
            tsMouseCoord.Text = $"X:{pointC.X} Y:{pointC.Y}";
            var pointI = bitmapRenderSource.MouseImageCoordinatesClip;
            tsImageCoord.Text = $"X:{pointI.X:f2} Y:{pointI.Y:f2}";
            tsColorData.BackColor = bitmapRenderSource.MouseImageColor;
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
            var source = this.colorManager.ImageSource;
            var processed = this.colorManager.ImageProcessed;
            if (source == null || processed == null)
            {
                return;
            }
            ImageCopied?.Invoke(this, new CopyDataEventArgs()
            {
                Name = currentName,
                SourceBitmap = source,
                DestBitmap = processed,
                DestBitmapRoi = new Rectangle(0, 0, processed.Width, processed.Height)
            });
        }

        void SetRenderPanelLayout(bool horizontalLayout)
        {
            splitRenderLayout.Orientation = horizontalLayout ? Orientation.Horizontal : Orientation.Vertical;
            horizontalToolStripMenuItem.Checked = horizontalLayout;
            verticalToolStripMenuItem.Checked = !horizontalLayout;
        }

        private void horizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetRenderPanelLayout(true);
        }


        private void verticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetRenderPanelLayout(false);
        }

        private async void btnProcessBlob_Click(object sender, EventArgs e)
        {
            blobManager.BlobConfig.WithProcessing((BlobConnectionModes)Enum.Parse(typeof(BlobConnectionModes), cbBlobConnectionMode.SelectedItem?.ToString() ?? string.Empty));
            blobManager.BlobConfig.WithFiltering((int)numBlobMinW.Value, (int)numBlobMinH.Value, (int)numBlobMinA.Value, (int)numBlobMaxW.Value, (int)numBlobMaxH.Value, (int)numBlobMaxA.Value);
            blobManager.BlobConfig.WithThreshold(ColorManager.TransformConfig.ColorBackgroundPalette, (double)numBlobThreshold.Value, chkBlobInvertSelection.Checked);
            var dataOut = await blobManager.ProcessBlobAsync(colorManager.DataSourceX);
            colorManager.InjectOutput(dataOut);
            RefreshData();
        }

        private void btnBlobSaveFrames_Click(object sender, EventArgs e)
        {
            if (saveFileDialogBlobs.ShowDialog() == DialogResult.OK)
            {
                var path = Path.GetDirectoryName(saveFileDialogBlobs.FileName);
                var prefix = Path.GetFileNameWithoutExtension(saveFileDialogBlobs.FileName);
                BlobTools.ExportImages(blobManager.BlobAnalysis, path, prefix);
            }
        }
    }
}
