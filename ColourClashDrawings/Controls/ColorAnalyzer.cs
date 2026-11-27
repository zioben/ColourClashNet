using ColourClashProcessing.Threshold;
using ColourClashNet.Color.Dithering;
using ColourClashLib;
using ColourClashLib.Color.Transformation;
using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Drawing;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
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
        static readonly string sC = nameof(ColorAnalyzer);





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
        int panel3MaxWidth = 400;

        FileInfo currentFileInfo = null;
        String currentName = "";
        // Short name of the last transformation applied, used to name the copies (e.g. "photo [CBM64]")
        String lastTransformName = "";

        /// <summary>
        /// Name shown for the loaded image: the file name without extension, or the name given to a copy.
        /// </summary>
        public string DisplayName => string.IsNullOrWhiteSpace(currentName) ? "" : Path.GetFileNameWithoutExtension(currentName.Trim());

        /// <summary>
        /// Short name of a transformation type for titles: the "ColorReduction" prefix is removed
        /// (e.g. <c>ColorReductionCBM64</c> becomes <c>CBM64</c>).
        /// </summary>
        public static string ShortTransformName(ColorTransformType type)
        {
            var name = type.ToString();
            const string prefix = "ColorReduction";
            return name.StartsWith(prefix) && name.Length > prefix.Length ? name[prefix.Length..] : name;
        }

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
            ApplyPreprocessingSettings();
            // The hue is a rotation: the control wraps around instead of stopping at 0 or 360
            nudHue.Minimum = -nudHue.Increment;
            nudHue.Maximum = 360;
            nudHue.ValueChanged += (s, e) => WrapHueControl();
            CreateComboBox(cbC64VideoMode, Enum.GetNames(typeof(ColorTransformCbm64.C64VideoMode)).ToList());
            CreateComboBox(cbCpcVideoMode, Enum.GetNames(typeof(ColorTransformAmstradCPC.CPCVideoMode)).ToList());
            CreateComboBox(cbAmigaVideoMode, Enum.GetNames(typeof(ColorTransformCbmAmiga.EnumAmigaVideoMode)).ToList());
            CreateComboBox(cbZxPaletteMode, Enum.GetNames(typeof(ColorTransformZxSpectrum.ZxPaletteMode)).ToList());
            CreateComboBox(cbZxAutotuneMode, Enum.GetNames(typeof(ColorTransformZxSpectrum.ZxAutotuneMode)).ToList());
            CreateComboBox(cbCgaVideoMode, Enum.GetNames(typeof(ColorTransformCGA.CGAVideoMode)).ToList());
            CreateComboBox(cbCgaVideoOutput, Enum.GetNames(typeof(ColorTransformCGA.CGACompositeOutputMode)).ToList());
            CreateComboBox(cbBlobConnectionMode, Enum.GetNames(typeof(BlobConnectionModes)).ToList());
            pbBkgColor.BackColor = System.Drawing.Color.FromArgb(255, ColorDefaults.DefaultBkgColorInt.ToR(), ColorDefaults.DefaultBkgColorInt.ToG(), ColorDefaults.DefaultBkgColorInt.ToB());
            CreateShapeTools();
            bitmapRenderSource.ColorAdded += (s, e) => { BuildBkgPalette(); };
            bitmapRenderSource.ColorRemoved += (s, e) => { BuildBkgPalette(); };
            colorManager.OnQuantize += (s, e) => { Invoke(() => RefreshData()); };
            colorManager.OnProcess += (s, e) => { Invoke(() => { lastTransformName = e.Transformation == null ? "" : ShortTransformName(e.Transformation.Type); RefreshData(); RebuildParams(e.Transformation); }); };
        }

        /// <summary>
        /// Applies the UI settings and runs the pre-processing again.
        /// </summary>
        public void Preprocess()
        {
            ApplyPreprocessingSettings();
            colorManager.PreProcess();
        }

        /// <summary>
        /// Keeps the hue control in [0, 360): stepping up from the last value goes back to 0, stepping down from 0 goes to the last value.
        /// </summary>
        void WrapHueControl()
        {
            if (nudHue.Value >= 360)
                nudHue.Value -= 360;
            else if (nudHue.Value < 0)
                nudHue.Value += 360;
        }

        #region Shapes (regions of interest)

        // True while a shape or a tool is copied from one image view to the other (prevents the echo)
        bool syncingShapes;

        /// <summary>
        /// Wires the shape toolbar and keeps the two image views in sync: the same drawing tool,
        /// and the same region of interest (drawn or edited on either view, shown on both).
        /// </summary>
        void CreateShapeTools()
        {
            // Icons drawn at run time; the button texts remain as tooltips
            tsbShapeFreehand.Image = ShapeIcons.Freehand();
            tsbShapeRectangle.Image = ShapeIcons.Rectangle();
            tsbShapeEllipse.Image = ShapeIcons.Ellipse();
            tsbShapePolygon.Image = ShapeIcons.Polygon();
            tsbShapeReset.Image = ShapeIcons.Reset();
            tsbShapeExtract.Image = ShapeIcons.Extract();

            var toolButtons = new Dictionary<ToolStripButton, Components.BitmapRender.EnumShapeTool>
            {
                [tsbShapeFreehand] = Components.BitmapRender.EnumShapeTool.Freehand,
                [tsbShapeRectangle] = Components.BitmapRender.EnumShapeTool.Rectangle,
                [tsbShapeEllipse] = Components.BitmapRender.EnumShapeTool.Ellipse,
                [tsbShapePolygon] = Components.BitmapRender.EnumShapeTool.Polygon,
            };
            // The tool buttons work as radio buttons: clicking the checked one turns the tool off
            foreach (var (button, tool) in toolButtons)
            {
                button.Click += (s, e) => SetShapeTool(button.Checked ? tool : Components.BitmapRender.EnumShapeTool.None);
            }
            tsbShapeReset.Click += (s, e) => bitmapRenderSource.ResetShape();
            // The status bar shows the ROI size in image pixels (the size of the extracted image)
            bitmapRenderSource.ShapeChanged += (s, e) =>
            {
                var size = bitmapRenderSource.ShapePixelSize;
                tsShapeSize.Text = size.IsEmpty ? "-" : $"{size.Width} x {size.Height}";
            };
            tsbShapeExtract.Click += (s, e) => ExtractShape(bitmapRenderSource);

            foreach (var render in new[] { bitmapRenderSource, bitmapRenderDest })
            {
                var other = render == bitmapRenderSource ? bitmapRenderDest : bitmapRenderSource;
                render.ShapeChanged += (s, e) => SyncShape(render, other);
                // A tool chosen from the context menu of either view updates the toolbar and the other view
                render.ShapeToolChanged += (s, e) => SetShapeTool(render.ShapeTool);
                render.ShapeExtractRequested += (s, e) => ExtractShape(render);
            }

            void SetShapeTool(Components.BitmapRender.EnumShapeTool tool)
            {
                if (syncingShapes)
                    return;
                syncingShapes = true;
                bitmapRenderSource.ShapeTool = tool;
                bitmapRenderDest.ShapeTool = tool;
                foreach (var (button, buttonTool) in toolButtons)
                    button.Checked = buttonTool == tool;
                syncingShapes = false;
            }
        }

        /// <summary>
        /// Copies the shape of <paramref name="from"/> to <paramref name="to"/>.
        /// </summary>
        void SyncShape(Components.BitmapRender from, Components.BitmapRender to)
        {
            if (syncingShapes)
                return;
            syncingShapes = true;
            to.Shape = from.Shape;
            syncingShapes = false;
        }

        /// <summary>
        /// Opens the region of interest of <paramref name="render"/> in a new tab.
        /// <para/>
        /// The pixels outside the shape take the background replacement color, so the background removal
        /// can drop them when the new image is processed.
        /// </summary>
        void ExtractShape(Components.BitmapRender render)
        {
            string sM = nameof(ExtractShape);
            try
            {
                var extracted = render.ExtractShape(GetBkgColorReplacement());
                if (extracted == null)
                {
                    LogMan.Warning(sC, sM, "no region of interest to extract (draw one with the right mouse button)");
                    return;
                }
                var bitmap = ImageToolsGDI.ImageDataToGdiImage(extracted);
                LogMan.Debug(sC, sM, $"extracted {render.Shape?.ShapeType} ROI of {extracted.Width}x{extracted.Height}");
                ImageCopied?.Invoke(this, new CopyDataEventArgs()
                {
                    Name = $"{DisplayName} [ROI]",
                    SourceBitmap = render.Image,
                    DestBitmap = bitmap,
                    DestBitmapRoi = new Rectangle(0, 0, extracted.Width, extracted.Height),
                });
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, "shape extraction failed", ex);
            }
        }

        #endregion

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
                // Show the seeds of the result: the best ones when the autotune is enabled
                nudZxColorLO.Value = zxTransform.BestSeedLow;
                nudZxColorHI.Value = zxTransform.BestSeedHigh;
            }
        }




        /// <summary>
        /// Rebuilds the background color swatches from the colors picked on the source image and reprocesses.
        /// </summary>
        void BuildBkgPalette()
        {
            var bkgPalette = GetBkgPalette();
            palViewerBkg.SetPalette(bkgPalette);
            ApplyPreprocessingSettings();
            colorManager.PreProcess();
        }

        void ResetBkgPalette()
        {
            palViewerBkg.ResetPalette();
            bitmapRenderSource.SelectedColors.Clear();
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
            // Show only the options of the selected transformation
            pgColorProcessorConfig.SelectedObject = colorManager?.Pipeline.Transform;
            palViewerOutput.SetPalette(colorManager?.DataProcessed?.ColorPalette);
            ShowImage();
        }

        Bitmap loadedBmp;

        /// <summary>
        /// True when an image has been loaded into the control.
        /// </summary>
        public bool HasImage => loadedBmp != null;

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
            if (WantedRes == null || WantedRes.Width == 0 || WantedRes.Height ==0)
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
            // A new image: the region of interest of the previous one is meaningless (the other view follows)
            bitmapRenderSource.ResetShape();
            //
            ApplyPreprocessingSettings();
            colorManager.Create(ResizeBitmap(loadedBmp));
            lastTransformName = "";
            ImageCreated?.Invoke(this, new DataSourceEventArgs()
            {
                Name = DisplayName,
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
        Palette GetBkgPalette()
        {
            var colors = new List<int>();
            foreach (var item in bitmapRenderSource.SelectedColors)
                colors.Add(ColorIntExtGdi.FromDrawingColor(item));
            return new Palette().Create(colors);
        }

        /// <summary>
        /// Returns the color that replaces the background colors.
        /// </summary>
        int GetBkgColorReplacement()
        {
            return ColorIntExtGdi.FromDrawingColor(pbBkgColor.BackColor);
        }



        /// <summary>
        /// Copies the pre-processing settings of the UI (background colors, quantization) into the pipeline.
        /// </summary>
        private void ApplyPreprocessingSettings()
        {
            colorManager.Pipeline = colorManager.Pipeline with
            {
                Background = new BackgroundRemovalOptions
                {
                    BackgroundColors = GetBkgPalette().ToList(),
                    ReplacementColor = GetBkgColorReplacement(),
                },
                Quantization = QuantizationMode,
            };
        }

        /// <summary>
        /// Builds the options of the transformation <paramref name="type"/> from the UI controls.
        /// <para/>
        /// Common settings (metric and dithering) come from the hosting form; each transformation
        /// reads only the controls of its own section.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Transformation not available in the UI.</exception>
        private TransformOptions BuildTransformOptions(ColorTransformType type)
        {
            var dithering = new DitherOptions
            {
                DitheringType = DitheringType,
                DitheringStrength = (double)nudDitheringStrenght.Value,
                DitheringFx = DitheringFx,
            };
            int colorsWanted = (int)nudColorsWanted.Value;
            int trainingLoops = (int)nudClusterLoop.Value;
            var clustering = new ClusteringOptions
            {
                MaxColors = colorsWanted,
                TrainingLoops = trainingLoops,
                Selection = ColorSelectionMode.UseColorPalette,
            };

            TransformOptions options = type switch
            {
                ColorTransformType.ColorReductionClustering => clustering,
                ColorTransformType.ColorReductionMedianCut => new MedianCutOptions { MaxColors = colorsWanted },
                ColorTransformType.ColorReductionScanline => new ScanlineOptions
                {
                    ChunkHeight = (int)numLinesPerBlock.Value,
                    PaletteMode = chkScanlineSharedPal.Checked
                        ? ColorTransformScanLine.ScanlinePaletteMode.SharedChunksPalette
                        : ColorTransformScanLine.ScanlinePaletteMode.IndependentChunkPalette,
                    ChunkReduction = clustering with { MaxColors = (int)nudScanlineLineColors.Value },
                    SharedReduction = clustering,
                },
                ColorTransformType.ColorReductionVGA => new VgaOptions
                {
                    Reduction = clustering with { MaxColors = Math.Min(256, colorsWanted), Selection = ColorSelectionMode.EvaluateColorMean },
                },
                ColorTransformType.ColorReductionHercules => new HerculesOptions(),
                ColorTransformType.ColorReductionEGA => new EgaOptions(),
                ColorTransformType.ColorReductionCGA => new CgaOptions
                {
                    VideoMode = ParseCombo<ColorTransformCGA.CGAVideoMode>(cbCgaVideoMode),
                    OutputMode = ParseCombo<ColorTransformCGA.CGACompositeOutputMode>(cbCgaVideoOutput),
                },
                ColorTransformType.ColorReductionSaturation => new HsvOptions
                {
                    HueShift = (double)nudHue.Value,
                    SaturationFactor = (double)nudSat.Value,
                    BrightnessFactor = (double)nudBright.Value,
                },
                ColorTransformType.ColorReductionCBM64 => new C64Options
                {
                    VideoMode = ParseCombo<ColorTransformCbm64.C64VideoMode>(cbC64VideoMode),
                    ShowTileBorders = chkShowTileBorders.Checked,
                    TileReduction = new C64Options().TileReduction with { TrainingLoops = trainingLoops },
                },
                ColorTransformType.ColorReductionCPC => new CpcOptions
                {
                    VideoMode = ParseCombo<ColorTransformAmstradCPC.CPCVideoMode>(cbCpcVideoMode),
                },
                ColorTransformType.ColorReductionZxSpectrum => new ZxOptions
                {
                    PaletteMode = ParseCombo<ColorTransformZxSpectrum.ZxPaletteMode>(cbZxPaletteMode),
                    SeedLow = (int)nudZxColorLO.Value,
                    SeedHigh = (int)nudZxColorHI.Value,
                    Autotune = ParseCombo<ColorTransformZxSpectrum.ZxAutotuneMode>(cbZxAutotuneMode),
                    DitherLowColorImage = chkZxDitherImageLo.Checked,
                    DitherHighColorImage = chkZxDitherHI.Checked,
                    IncludeBlackInHighColor = chkZxBlackHI.Checked,
                    IncludeWhiteInHighColor = chkZxWhiteHI.Checked,
                },
                ColorTransformType.ColorReductionHam => new AmigaOptions
                {
                    VideoMode = ParseCombo<ColorTransformCbmAmiga.EnumAmigaVideoMode>(cbAmigaVideoMode),
                    BasePaletteReduction = clustering,
                },
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"{type} is not available in the UI"),
            };
            return options with { DistanceMode = ColorDistanceEvaluationMode, Dithering = dithering };
        }

        /// <summary>
        /// Parses the selected item of a combo box filled with the names of <typeparamref name="T"/>.
        /// </summary>
        static T ParseCombo<T>(ComboBox comboBox) where T : struct, Enum
            => Enum.Parse<T>(comboBox.SelectedItem?.ToString() ?? default(T).ToString());

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
            string sM = nameof(ReduceColorsAsync);
            // async void: an unhandled exception here would terminate the application
            try
            {
                ApplyPreprocessingSettings();
                await colorManager.ProcessColorsAsync(BuildTransformOptions(type));
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"{type} processing failed", ex);
            }
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



        void FixPanelSize()
        {
            if (splitMainA_BC.Panel1.Width > panel1MaxWidth)
            {
                splitMainA_BC.SplitterDistance = panel1MaxWidth;
            }
            if (splitMainB_C.Panel2.Width > panel3MaxWidth)
            {
                splitMainB_C.SplitterDistance = splitMainB_C.Width - panel3MaxWidth;
            }
        }

        private void scMain_SplitterMoved(object sender, SplitterEventArgs e)
        {
            FixPanelSize();
        }

        private void scMain_SizeChanged(object sender, EventArgs e)
        {
            FixPanelSize();
        }

        private void splitMainB_C_SplitterMoved(object sender, SplitterEventArgs e)
        {
            FixPanelSize();
        }

        private void splitMainB_C_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            FixPanelSize();
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
            // The copy is named after the source image and the transformation that produced it
            var copyName = string.IsNullOrEmpty(lastTransformName) ? DisplayName : $"{DisplayName} [{lastTransformName}]";
            ImageCopied?.Invoke(this, new CopyDataEventArgs()
            {
                Name = copyName,
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
            // Background colors picked on the source image are the threshold reference;
            // the rebuilt images use the same replacement color as the background removal
            var background = ColorManager.Pipeline.Background;
            blobManager.BlobOptions = new BlobOptions
            {
                Threshold = new ThresholdOptions
                {
                    BackgroundColors = background.BackgroundColors,
                    Tolerance = (double)numBlobThreshold.Value,
                    InvertResults = chkBlobInvertSelection.Checked,
                },
                ConnectionMode = ParseCombo<BlobConnectionModes>(cbBlobConnectionMode),
                MinWidth = (int)numBlobMinW.Value,
                MinHeight = (int)numBlobMinH.Value,
                MinArea = (int)numBlobMinA.Value,
                MaxWidth = (int)numBlobMaxW.Value,
                MaxHeight = (int)numBlobMaxH.Value,
                MaxArea = (int)numBlobMaxA.Value,
                BackgroundColor = background.ReplacementColor,
            };
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

        private void btnResetBackground_Click(object sender, EventArgs e)
        {
            ResetBkgPalette();
            Preprocess();
        }
    }
}
