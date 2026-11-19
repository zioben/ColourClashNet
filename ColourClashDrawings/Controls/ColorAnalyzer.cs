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
    /// <summary>
    /// Main working control: shows the source and the processed image side by side and exposes
    /// the settings and the commands of every color transformation and of the blob extraction.
    /// </summary>
    public partial class ColorAnalyzer : UserControl
    {




        /// <summary>
        /// Event data raised when the current result is copied to a new container.
        /// </summary>
        public class CopyDataEventArgs : EventArgs
        {
            /// <summary>Image name.</summary>
            public string Name { get; set; }
            /// <summary>Source image.</summary>
            public Image SourceBitmap { get; set; }
            /// <summary>
            /// Processed image.
            /// </summary>
            public Image DestBitmap { get; set; }
            /// <summary>
            /// Area of the processed image to copy.
            /// </summary>
            public Rectangle DestBitmapRoi { get; set; }
        }

        /// <summary>
        /// Event data raised when a new source image is loaded.
        /// </summary>
        public class DataSourceEventArgs : EventArgs
        {
            /// <summary>Image name.</summary>
            public String Name { get; set; }
            /// <summary>Loaded image.</summary>
            public Image SourceBitmap { get; set; }
            /// <summary>
            /// Source file, if any.
            /// </summary>
            public FileInfo SourceFile { get; set; }
        }


        /// <summary>
        /// Raised when the user copies the current result to a new container.
        /// </summary>
        public event EventHandler<CopyDataEventArgs> ImageCopied;
        /// <summary>
        /// Raised when a new source image is loaded.
        /// </summary>
        public event EventHandler<DataSourceEventArgs> ImageCreated;

        int panel1MaxWidth = 400;

        FileInfo currentFileInfo = null;
        String currentName = "";

        /// <summary>
        /// Target resolution: the source image is resized to it when loaded (null = keep the original size).
        /// </summary>
        [DefaultValue(null)]
        public GraphicsResolution WantedRes { get; set; } = null;

        /// <summary>
        /// Quantization applied in the pre-processing.
        /// </summary>
        [DefaultValue(ColorQuantizationMode.RGB888)]
        public ColorQuantizationMode QuantizationMode { get; set; } = ColorQuantizationMode.RGB888;

        /// <summary>
        /// Dithering algorithm.
        /// </summary>
        [DefaultValue(ColorDithering.FloydSteinberg)]
        public ColorDithering DitheringType { get; set; } = ColorDithering.FloydSteinberg;

        /// <summary>
        /// Post-dithering effect.
        /// </summary>
        [DefaultValue(ColorDitheringFx.None)]
        public ColorDitheringFx DitheringFx { get; set; } = ColorDitheringFx.None;

        /// <summary>
        /// Color distance metric.
        /// </summary>
        [DefaultValue(ColorDistanceEvaluationMode.RGB)]
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.RGB;

        /// <summary>
        /// Blob detector.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Blob BlobAnalysis { get; set; } = new Blob();


        /// <summary>
        /// Processing pipeline of this control.
        /// </summary>
        public Components.ColorTransformManager ColorManager => colorManager;

        /// <summary>
        /// Creates the control and initializes combo boxes and event wiring.
        /// </summary>
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
            pbBkgColor.BackColor = System.Drawing.Color.FromArgb(255, ColorDefaults.DefaultBkgColorInt.ToR(), ColorDefaults.DefaultBkgColorInt.ToG(), ColorDefaults.DefaultBkgColorInt.ToB());
            bitmapRenderSource.ColorAdded += (s, e) => { BuildBkgPalette(); };
            bitmapRenderSource.ColorRemoved += (s, e) => { BuildBkgPalette(); };
            colorManager.OnQuantize += (s, e) => { Invoke(() => RefreshData()); };
            colorManager.OnProcess += (s, e) => { Invoke(() => { RefreshData(); RebuildParams(e.Transformation); }); };
            blobManager.OnPreProcess += (s, e) => { blobManager.WithProcessingParams(colorManager.TransformConfig.ColorBackgroundPalette, colorManager.TransformConfig.ColorBackgroundReplacementValue); };
        }

        /// <summary>
        /// Applies the UI settings and runs the pre-processing again.
        /// </summary>
        public void Preprocess()
        {
            SetControlToConfig();
            colorManager.PreProcess();
        }

        /// <summary>
        /// Fills a combo box and selects the first item.
        /// </summary>
        void CreateComboBox(ComboBox comboBox, List<string> items)
        {
            comboBox.Items.Clear();
            items.ForEach(x => comboBox.Items.Add(x));
            comboBox.SelectedIndex = 0;
        }


        /// <summary>
        /// Updates the UI with the parameters found by a transformation (e.g. ZX autotune seeds).
        /// </summary>
        private void RebuildParams(object sender)
        {
            if (sender is ColorTransformZxSpectrum zxTransform)
            {
                nudZxColorLO.Value = zxTransform.ZxPaletteInColorSeedLow;
                nudZxColorHI.Value = zxTransform.ZxPaletteInColorSeedHigh;
            }
        }




        /// <summary>
        /// Rebuilds the background color swatches from the colors picked on the source image and reprocesses.
        /// </summary>
        void BuildBkgPalette()
        {
            pbBkgColorList.Image = null;
            var bkgColors = bitmapRenderSource.SelectedColors;
            if (bkgColors.Count > 0)
            {
                Bitmap bmp = new Bitmap((int)bkgColors.Count * 16, (int)16);
                // Draw one 16x16 swatch per background color
                using (var g = Graphics.FromImage(bmp))
                {
                    for (int i = 0; i < bkgColors.Count; i++)
                    {
                        var item = bkgColors[i];
                        using (var brush = new SolidBrush(item))
                        {
                            g.FillRectangle(brush, i * 16, 0, 16, 16);
                        }
                    }
                }
                pbBkgColorList.SizeMode = PictureBoxSizeMode.StretchImage;
                pbBkgColorList.Image = bmp;
            }
            colorManager.TransformConfig.WithBackgroundColorReplacement(GetBkgColors(), ColorDefaults.DefaultBkgColorInt);
            colorManager.PreProcess();
        }

        /// <summary>
        /// Shows copies of the source and processed images.
        /// </summary>
        void ShowImage()
        {
            bitmapRenderSource.Image = colorManager.ImageSource?.Clone() as Image;
            bitmapRenderDest.Image = colorManager.ImageProcessed?.Clone() as Image;
            Invalidate();
        }

        /// <summary>
        /// Refreshes property grids and images.
        /// </summary>
        void RefreshData()
        {
            pgColorProcessor.SelectedObject = colorManager;
            pgColorProcessorConfig.SelectedObject = colorManager?.TransformConfig;
            ShowImage();
        }

        Bitmap loadedBmp;

        /// <summary>
        /// Resizes a bitmap to <see cref="WantedRes"/>.
        /// </summary>
        /// <returns>The resized bitmap, or <paramref name="srcBmp"/> when no resolution is set.</returns>
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
                var resizedBmp = new Bitmap(WantedRes.Width, WantedRes.Height);
                using (var graphics = Graphics.FromImage(resizedBmp))
                {
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    graphics.DrawImage(srcBmp, new Rectangle(0, 0, resizedBmp.Width, resizedBmp.Height), new Rectangle(0, 0, srcBmp.Width, srcBmp.Height), GraphicsUnit.Pixel);
                }
                return resizedBmp;
            }
        }

        /// <summary>
        /// Reloads the current bitmap into the pipeline.
        /// </summary>
        public void Create()
        {
            bitmapRenderDest.ResetMouseSelectedColors();
            bitmapRenderDest.OriginZero();
            bitmapRenderSource.ResetMouseSelectedColors();
            bitmapRenderSource.OriginZero();
            //
            colorManager.TransformConfig.WithBackgroundColorReplacement(GetBkgColors(), ColorIntExtGdi.FromDrawingColor(pbBkgColor.BackColor));
            colorManager.Create(ResizeBitmap(loadedBmp));
            ImageCreated?.Invoke(this, new DataSourceEventArgs()
            {
                Name = currentName,
                SourceBitmap = loadedBmp,
            });
        }

        /// <summary>
        /// Loads an image into the control.
        /// </summary>
        public void Create(ImageData image, string name)
        {
            loadedBmp = ImageToolsGDI.ImageDataToGdiImage(image) as Bitmap;
            currentName = name;
            Create();
        }

        /// <summary>
        /// Loads an image into the control.
        /// </summary>
        public void Create(Image image, string name)
        {
            loadedBmp = image as Bitmap;
            currentName = name;
            Create();
        }

        /// <summary>
        /// Loads an image file into the control.
        /// </summary>
        public void Create(FileInfo fileInfo)
        {
            var image = ImageTools.LoadImage(fileInfo.FullName);
            Create(image, fileInfo.Name);
        }



        /// <summary>
        /// Returns the background colors picked on the source image.
        /// </summary>
        Palette GetBkgColors()
        {
            var colors = new List<int>();
            foreach (var item in bitmapRenderSource.SelectedColors)
                colors.Add(ColorIntExtGdi.FromDrawingColor(item));
            return new Palette().Create(colors);
        }




        /// <summary>
        /// Copies all the UI settings into the transformation configuration.
        /// </summary>
        private void SetControlToConfig()
        {
            colorManager.TransformConfig.WithBackgroundColorReplacement(
                GetBkgColors(),
                ColorIntExtGdi.FromDrawingColor(pbBkgColor.BackColor));
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
            // Dithering type and effect are set by the hosting form
            colorManager.TransformConfig.WithDithering(
                DitheringType,
                (double)nudDitheringStrenght.Value,
                DitheringFx);
            //
            colorManager.TransformConfig.WithHSV(
                (double)nudHue.Value,
                (double)nudSat.Value,
                (double)nudBright.Value);
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
                 chkZxDitherImageLo.Checked,
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
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorManager.Reset();
        }


        private void colorManager_OnReset(object sender, EventArgs e)
        {
            RefreshData();
        }


        /// <summary>
        /// Applies the UI settings and runs the transformation <paramref name="type"/> asynchronously.
        /// </summary>
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

        private void bitmapRenderSource_MouseMove(object sender, MouseEventArgs e)
        {
            var controlPoint = bitmapRenderSource.MouseControlCoordinates;
            tsMouseCoord.Text = $"X:{controlPoint.X} Y:{controlPoint.Y}";
            var imagePoint = bitmapRenderSource.MouseImageCoordinatesClip;
            tsImageCoord.Text = $"X:{imagePoint.X:f2} Y:{imagePoint.Y:f2}";
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

        /// <summary>
        /// Arranges the source and processed views horizontally or vertically.
        /// </summary>
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

        private void btnChangeBkg_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = pbBkgColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                pbBkgColor.BackColor = colorDialog1.Color;
                Preprocess();
            }
        }

    }
}
