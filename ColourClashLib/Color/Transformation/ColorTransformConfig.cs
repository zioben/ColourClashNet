using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using System.ComponentModel;
using ColourClashLib.Color.Transformation;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Typed configuration shared by <see cref="ColorTransformBase"/> and all derived transformations.
    /// <para/>
    /// Each transformation reads only the settings of its own category; the object is shown in a
    /// PropertyGrid and copied with <see cref="Clone"/> when assigned to a transformation.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ColorTransformConfig
    {

        // ── Core ────────────────────────────────────────────────────────────

        /// <summary>
        /// Metric used to compare colors.
        /// </summary>
        [Browsable(true),Category("Shared")]
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; }
            = ColorDistanceEvaluationMode.RGB;

        /// <summary>
        /// Channel bit depth of the target hardware.
        /// </summary>
        [Browsable(true), Category("Shared")]
        public ColorQuantizationMode QuantizationMode { get; set; }
            = ColorQuantizationMode.RGB888;

        /// <summary>
        /// Maximum number of output colors.
        /// </summary>
        [Browsable(true), Category("Shared")]
        public int MaxColorsWanted { get; set; } = 256;

        /// <summary>
        /// Marks the tile grid on tile based transformations.
        /// </summary>
        [Browsable(true), Category("Shared")]
        public bool ShowTileBorders { get; set; } = true;

        /// <summary>
        /// Color used to mark the tile grid.
        /// </summary>
        [Browsable(true), Category("Shared")]
        public int TileBorderColor { get; set; } = ColorIntExt.FromRGB(255, 0, 255);

        // ── Palette ─────────────────────────────────────────────────────────
        private object paletteLock = new object();

        /// <summary>
        /// True once a reference palette has been set; further writes require <c>forcePaletteOverwrite</c>.
        /// </summary>
        [Browsable(true), Category("Shared")]
        public bool ReferencePaletteWriteLock { get; private set; } = false;

        /// <summary>
        /// Palette that drives the transformation (fixed hardware or user palette).
        /// </summary>
        [Browsable(true), Category("Shared")]
        public Palette ReferencePalette { get; private set; } = new Palette();

        // ── Dithering ────────────────────────────────────────────────────────

        /// <summary>
        /// Dithering settings.
        /// </summary>
        [Browsable(true), Category("Shared")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public DitherConfig DitheringCfg { get; set; } = new DitherConfig();

        // ── Scanline ─────────────────────────────────────────────────────────

        /// <summary>
        /// Maximum number of colors that can change between two consecutive chunks (0 = no limit).
        /// </summary>
        [Browsable(true), Category("Scanline")]
        public int ScanlineMaxColorsPerChunk { get; set; } = 0;

        /// <summary>
        /// Number of lines that share the same palette.
        /// </summary>
        [Browsable(true), Category("Scanline")]
        public int ScanlineChunkHeight { get; set; } = 1;

        /// <summary>
        /// How chunk palettes are computed.
        /// </summary>
        [Browsable(true), Category("Scanline")]
        public ColorTransformScanLine.ScanlinePaletteMode ScanlinePaletteMode { get; set; } = ColorTransformScanLine.ScanlinePaletteMode.IndependentChunkPalette;

        /// <summary>
        /// Color reduction used to compute each chunk palette.
        /// </summary>
        [Browsable(true), Category("Scanline")]
        public ColorTransformType ScanlineInternalTransformationModel { get; set; } = ColorTransformType.ColorReductionFast;
  
        // ── Background ─────────────────────────────────────────────────────────

        /// <summary>
        /// Colors to be treated as background.
        /// </summary>
        [Browsable(true), Category("Background Replacement")]
        public Palette ColorBackgroundPalette { get; set; } = new Palette();

        /// <summary>
        /// Color that replaces the background colors.
        /// </summary>
        [Browsable(true), Category("Background Replacement")]
        public int ColorBackgroundReplacementValue { get; set; } = ColorIntExt.FromRGB(0, 0, 0);


        // ── Clustering ────────────────────────────────────────────────────────────

        /// <summary>
        /// Number of clustering iterations.
        /// </summary>
        [Browsable(true), Category("Cluster")]
        public int ClusterTrainingLoop { get; set; } = 10;

        /// <summary>
        /// How the representative color of a cluster is selected.
        /// </summary>
        [Browsable(true), Category("Cluster")]
        public ColorSelectionMode ClusterColorSelectionMode { get; set; } = ColorSelectionMode.EvaluateColorMean;

        /// <summary>
        /// How the initial clusters are seeded.
        /// </summary>
        [Browsable(true), Category("Cluster")]
        public ClusterPaletteInitializationMode ClusterPaletteInitializationMode { get; set; } = ClusterPaletteInitializationMode.ImagePalette;
       
        // ── ColorMean ────────────────────────────────────────────────────────────

        /// <summary>
        /// How the representative color of each median cut box is selected.
        /// </summary>
        [Browsable(true), Category("Median Cut")]
        public ColorSelectionMode MedianCutColorSelectionMode { get; set; } = ColorSelectionMode.EvaluateColorMean;


        // ── HSV adjustments ──────────────────────────────────────────────────

        /// <summary>
        /// Hue rotation in degrees [-180, 180].
        /// </summary>
        [Browsable(true), Category("HSV")]
        public double HsvHueShift { get; set; } = 0.0;

        /// <summary>
        /// Saturation multiplier.
        /// </summary>
        [Browsable(true), Category("HSV")]
        public double HsvSaturationMultFactor { get; set; } = 1.0;

        /// <summary>
        /// Brightness (value) multiplier.
        /// </summary>
        [Browsable(true), Category("HSV")]
        public double HsvBrightnessMultFactor { get; set; } = 1.0;

        // ── Platform-specific: C64 ───────────────────────────────────────────

        /// <summary>
        /// Commodore 64 video mode.
        /// </summary>
        [Browsable(true), Category("CBM C64")]
        public ColorTransformCbm64.C64VideoMode C64VideoMode { get; set; }
            = ColorTransformCbm64.C64VideoMode.Multicolor;

        /// <summary>
        /// When dithering is applied in the C64 pipeline.
        /// </summary>
        [Browsable(true), Category("CBM C64")]
        public ColorTransformCbm64.C64DitheringMode C64DitheringMode { get; set; }
            = ColorTransformCbm64.C64DitheringMode.PreDitherImage;

        // ── Platform-specific: Amiga ─────────────────────────────────────────

        /// <summary>
        /// Amiga video mode.
        /// </summary>
        [Browsable(true), Category("CBM Amiga")]
        public ColorTransformCbmAmiga.EnumAmigaVideoMode AmigaVideoMode { get; set; }
            = ColorTransformCbmAmiga.EnumAmigaVideoMode.Ham6;

        /// <summary>
        /// How the Amiga base palette colors are selected.
        /// </summary>
        [Browsable(true), Category("CBM Amiga")]
        public ColorSelectionMode AmigaColorSelectionMode { get; set; } = ColorSelectionMode.UseColorPalette;

        /// <summary>
        /// Color reduction used to compute the Amiga base palette.
        /// </summary>
        [Browsable(true), Category("CBM Amiga")]
        public ColorTransformType AmigaInternalTransformationModel { get; set; } = ColorTransformType.ColorReductionFast;


        // ── Platform-specific: CPC ───────────────────────────────────────────

        /// <summary>
        /// Amstrad CPC video mode.
        /// </summary>
        [Browsable(true), Category("Amstrad CPC")]
        public ColorTransformAmstradCPC.CPCVideoMode CPCVideoMode { get; set; }
            = ColorTransformAmstradCPC.CPCVideoMode.Mode0;

        // ── Platform-specific: CGA ───────────────────────────────────

        /// <summary>
        /// IBM CGA video mode.
        /// </summary>
        [Browsable(true), Category("CGA")]
        public ColorTransformCGA.CGAVideoMode CGAVideoMode { get; set; }
            = ColorTransformCGA.CGAVideoMode.Mode4_0L;

        /// <summary>
        /// CGA output: processed RGB image or composite artifact simulation.
        /// </summary>
        [Browsable(true), Category("CGA")]
        public ColorTransformCGA.CGACompositeOutputMode CGAOutputMode { get; set; }
            = ColorTransformCGA.CGACompositeOutputMode.ProcessedImage;

        // ── Platform-specific: ZX Spectrum ───────────────────────────────────

        /// <summary>
        /// ZX Spectrum palettes used (normal, bright or both).
        /// </summary>
        [Browsable(true), Category("ZX Spectrum")]
        public ColorTransformZxSpectrum.ZxPaletteMode ZxPaletteMode { get; set; }
            = ColorTransformZxSpectrum.ZxPaletteMode.BothPalettes;

        /// <summary>
        /// Channel intensity of the normal ZX Spectrum palette as seen in the source image.
        /// </summary>
        [Browsable(true), Category("ZX Spectrum")]
        public int ZxPaletteInColorSeedLow { get; set; } = 192;

        /// <summary>
        /// Channel intensity of the bright ZX Spectrum palette as seen in the source image.
        /// </summary>
        [Browsable(true), Category("ZX Spectrum")]
        public int ZxPaletteInColorSeedHigh { get; set; } = 255;

        /// <summary>
        /// Includes black in the bright palette candidate image.
        /// </summary>
        [Browsable(true), Category("ZX Spectrum")]
        public bool ZxIncludeBlackInHighColorImage { get; set; } = true;

        /// <summary>
        /// Includes black in the bright palette candidate image.
        /// </summary>
        [Browsable(true), Category("ZX Spectrum")]
        public bool ZxIncludeWhiteInHighColorImage { get; set; } = true;

        /// <summary>
        /// Dithers the normal palette candidate image.
        /// </summary>
        [Browsable(true), Category("ZX Spectrum")]
        public bool ZxDitherLowColorImage { get; set; } = false;

        /// <summary>
        /// Dithers the bright palette candidate image.
        /// </summary>
        [Browsable(true), Category("ZX Spectrum")]
        public bool ZxDitherHighColorImage { get; set; } = false;

        /// <summary>
        /// Automatic search of the best seed values.
        /// </summary>
        [Browsable(true), Category("ZX Spectrum")]
        public ColorTransformZxSpectrum.ZxAutotuneMode ZxAutotuneMode { get; set; } = ColorTransformZxSpectrum.ZxAutotuneMode.None;


        /// <summary>
        /// Creates an independent copy; palettes are deep copied.
        /// </summary>
        public ColorTransformConfig Clone()
        {
            lock (paletteLock)
            {
                var clone = base.MemberwiseClone() as ColorTransformConfig;
                clone.paletteLock = new object();
                if (ReferencePalette != null)
                {
                    clone.ReferencePalette = new Palette().Create(this.ReferencePalette);
                }
                else
                {
                    ReferencePalette = new Palette();
                }
                if (ColorBackgroundPalette != null)
                {
                    clone.ColorBackgroundPalette = new Palette().Create(this.ColorBackgroundPalette);
                }
                else
                {
                    clone.ColorBackgroundPalette = new Palette();
                }
                return clone;
            }
        }

        //public ColorTransformConfig WithColorMean(ColorSelectionMode colorMeanMode)
        //{
        //    ColorMeanColorSelectionMode = colorMeanMode;
        //    return this;
        //}


        /// <summary>
        /// Sets and write-locks the reference palette, unless already locked (fluent).
        /// </summary>
        /// <param name="palette">Palette colors</param>
        /// <param name="forcePaletteOverwrite">Overwrite even if locked</param>
        public ColorTransformConfig WithReferencePalette(IEnumerable<int> palette, bool forcePaletteOverwrite=false)
        {
            lock (paletteLock)
            {
                if (!ReferencePaletteWriteLock || forcePaletteOverwrite)
                {
                    ReferencePalette = new Palette().Create(palette);
                    ReferencePaletteWriteLock = true;
                }
            }
            return this;
        }

        /// <summary>
        /// Same as <see cref="WithReferencePalette(IEnumerable{int}, bool)"/> (fluent).
        /// </summary>
        public ColorTransformConfig WithFixedPalette(IEnumerable<int> palette, bool forcePaletteOverwrite = false)
        {
            lock (paletteLock)
            {
                if (!ReferencePaletteWriteLock || forcePaletteOverwrite)
                {
                    ReferencePalette = new Palette().Create(palette);
                    ReferencePaletteWriteLock = true;
                }
            }
            return this;
        }

        /// <summary>
        /// Sets and write-locks the reference palette, unless already locked (fluent).
        /// </summary>
        public ColorTransformConfig WithReferencePalette(Palette palette, bool forcePaletteOverwrite=false)
        {
            lock (paletteLock)
            {
                if (!ReferencePaletteWriteLock || forcePaletteOverwrite)
                {
                    ReferencePalette = new Palette().Create(palette);
                    ReferencePaletteWriteLock = true;
                }
            }
            return this;
        }

        /// <summary>
        /// Sets the color distance metric (fluent).
        /// </summary>
        public ColorTransformConfig WithColorDistanceEvaluationMode(ColorDistanceEvaluationMode mode)
        {
            ColorDistanceEvaluationMode = mode;
            return this;
        }

        /// <summary>
        /// Sets the background colors and their replacement (fluent).
        /// </summary>
        public ColorTransformConfig WithBackgroundColorReplacement(Palette colorPalette, int replacementColor)
        {
            ColorBackgroundPalette = new Palette().Create(colorPalette);
            ColorBackgroundReplacementValue = replacementColor;
            return this;
        }

        /// <summary>
        /// Sets the quantization mode (fluent).
        /// </summary>
        public ColorTransformConfig WithQuantizationMode(ColorQuantizationMode mode)
        {
            QuantizationMode = mode;
            return this;
        }

        /// <summary>
        /// Sets the dithering (fluent).
        /// </summary>
        public ColorTransformConfig WithDithering(ColorDithering ditheringType, double strength, ColorDitheringFx fx)
        {
            DitheringCfg = new DitherConfig
            {
                DitheringType = ditheringType,
                DitheringStrength = strength,
                DitheringFx = fx
            };
            return this;
        }
        /// <summary>
        /// Sets the dithering with a copy of <paramref name="cfg"/> (fluent).
        /// </summary>
        public ColorTransformConfig WithDithering(DitherConfig cfg)
        {
            DitheringCfg = cfg?.Clone() as DitherConfig ?? throw new ArgumentNullException(nameof(cfg));
            return this;
        }

        /// <summary>
        /// Sets the dithering algorithm (fluent).
        /// </summary>
        public ColorTransformConfig WithDitheringType(ColorDithering ditheringType)
        {
            DitheringCfg.DitheringType = ditheringType;
            return this;
        }
        /// <summary>
        /// Sets the dithering strength (fluent).
        /// </summary>
        public ColorTransformConfig WithDitheringStrength(double strength)
        {
            DitheringCfg.DitheringStrength = strength;
            return this;
        }
        /// <summary>
        /// Sets the post-dithering effect (fluent).
        /// </summary>
        public ColorTransformConfig WithDitheringFx(ColorDitheringFx fx)
        {
            DitheringCfg.DitheringFx = fx;
            return this;
        }

        /// <summary>
        /// Sets the Amiga settings (fluent).
        /// </summary>
        public ColorTransformConfig WithAmigaScreenMode(ColorTransformCbmAmiga.EnumAmigaVideoMode videoMode, ColorTransformType colorTransformationMode, ColorSelectionMode colorMeanMode)
        {
            AmigaVideoMode = videoMode;
            AmigaInternalTransformationModel = colorTransformationMode;
            AmigaColorSelectionMode = colorMeanMode;
            return this;
        }

        /// <summary>
        /// Sets the CGA settings (fluent).
        /// </summary>
        public ColorTransformConfig WithCGAScreenMode(ColorTransformCGA.CGAVideoMode videoMode, ColorTransformCGA.CGACompositeOutputMode outputMode)
        {
            CGAVideoMode = videoMode;
            CGAOutputMode = outputMode;
            return this;
        }


        /// <summary>
        /// Sets the C64 settings (fluent).
        /// </summary>
        public ColorTransformConfig WithC64ScreenMode(ColorTransformCbm64.C64VideoMode mode, ColorTransformCbm64.C64DitheringMode ditheringMode, bool showTileBorders)
        {
            C64VideoMode = mode;
            C64DitheringMode = ditheringMode;
            ShowTileBorders = showTileBorders;
            return this;
        }
        /// <summary>
        /// Sets the Amstrad CPC video mode (fluent).
        /// </summary>
        public ColorTransformConfig WithCpcVideoMode(ColorTransformAmstradCPC.CPCVideoMode videoMode)
        {
            CPCVideoMode = videoMode;
            return this;
        }

        /// <summary>
        /// Sets the clustering settings (fluent).
        /// </summary>
        public ColorTransformConfig WithClustering(int maxColorsWanted, int trainingLoop, ColorSelectionMode colorMeanMode)
        {
            MaxColorsWanted = maxColorsWanted;
            ClusterTrainingLoop = trainingLoop;
            ClusterColorSelectionMode = colorMeanMode;
            return this;
        }

        /// <summary>
        /// Sets the settings of the fast (most frequent colors) reduction (fluent).
        /// </summary>
        public ColorTransformConfig WithFastReduction(int maxColorsWanted)
        {
            MaxColorsWanted = maxColorsWanted;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of output colors (fluent).
        /// </summary>
        public ColorTransformConfig WithMaxColorWanted(int maxColors)
        {
            MaxColorsWanted = maxColors;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of median cut colors (fluent).
        /// </summary>
        public ColorTransformConfig WithMedianCut(int maxColors )
        {
            MaxColorsWanted = maxColors;
            return this;
        }
        /// <summary>
        /// Sets the scanline settings (fluent).
        /// </summary>
        /// <param name="chunkHeight">Lines sharing the same palette</param>
        /// <param name="scanlineReductionMode">Chunk palette mode</param>
        /// <param name="maxColorWanted">Colors per chunk</param>
        /// <param name="maxColorChangePerLine">Maximum color changes between chunks</param>
        /// <param name="internalTransformationMode">Reduction used per chunk</param>
        public ColorTransformConfig WithScanline(int chunkHeight, ColorTransformScanLine.ScanlinePaletteMode scanlineReductionMode, int maxColorWanted, int maxColorChangePerLine, ColorTransformType internalTransformationMode )
        {
            WithMaxColorWanted(maxColorWanted);
            ScanlineChunkHeight = chunkHeight;
            ScanlinePaletteMode = scanlineReductionMode;  
            ScanlineMaxColorsPerChunk = maxColorChangePerLine;
            ScanlineInternalTransformationModel = internalTransformationMode;
            return this;
        }

        /// <summary>
        /// Sets the median cut color selection mode (fluent).
        /// </summary>
        public ColorTransformConfig WithMedianCut(ColorSelectionMode colorMeanMode)
        {
            MedianCutColorSelectionMode = colorMeanMode;
            return this;
        }

        /// <summary>
        /// Sets the ZX Spectrum palette settings (fluent).
        /// </summary>
        public ColorTransformConfig WithZxScreenMode( ColorTransformZxSpectrum.ZxPaletteMode paletteMode, int lowColorInSeed, int highColorInSeed )
        {
            ZxPaletteMode = paletteMode;
            ZxPaletteInColorSeedLow = lowColorInSeed;
            ZxPaletteInColorSeedHigh = highColorInSeed;
            return this;
        }
        /// <summary>
        /// Sets the ZX Spectrum processing settings (fluent).
        /// </summary>
        public ColorTransformConfig WithZxProcessing( ColorTransformZxSpectrum.ZxAutotuneMode autotuneMode, bool ditherLowColorImage, bool ditherHighColorImage, bool includeBlackInHighColor, bool includeWhiteInHighColor, bool showTileBorders)
        {
            ZxDitherLowColorImage = ditherLowColorImage;
            ZxDitherHighColorImage = ditherHighColorImage;
            ZxIncludeBlackInHighColorImage = includeBlackInHighColor;
            ZxIncludeWhiteInHighColorImage = includeWhiteInHighColor;
            ZxAutotuneMode = autotuneMode;
            ShowTileBorders = showTileBorders;
            return this;
        }
        /// <summary>
        /// Sets the HSV adjustments; the hue shift is truncated to an integer (fluent).
        /// </summary>
        public ColorTransformConfig WithHSV(double hueShift, double saturationMultFactor, double brightnessMultFactor)
        {
            HsvHueShift = (int)hueShift;
            HsvSaturationMultFactor = saturationMultFactor;
            HsvBrightnessMultFactor = brightnessMultFactor;
            return this;
        }       
    }
}