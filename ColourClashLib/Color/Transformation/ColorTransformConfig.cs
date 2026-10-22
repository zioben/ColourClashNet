using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using System.ComponentModel;
using static ColourClashNet.Color.Transformation.ColorTransformCbmAmiga;
using static ColourClashNet.Color.Transformation.ColorTransformCbm64;
using static ColourClashNet.Color.Transformation.ColorTransformAmstradCPC;
using static ColourClashNet.Color.Transformation.ColorTransformZxSpectrum;

namespace ColourClashNet.Color.Transformation
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    /// <summary>
    /// Typed configuration for ColorTransformBase and derived classes.
    /// Replaces the previous Dictionary-based property bag.
    /// </summary>
    public class ColorTransformConfig
    {

        // ── Core ────────────────────────────────────────────────────────────
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; }
            = ColorDistanceEvaluationMode.RGB;

        public ColorQuantizationMode QuantizationMode { get; set; }
            = ColorQuantizationMode.RGB888;

        public int MaxColorsWanted { get; set; } = 256;
        public bool ShowTileBorders { get; set; } = true;
        public int TileBorderColor { get; set; } = ColorIntExt.FromRGB(255, 0, 255);

        // ── Scanline ─────────────────────────────────────────────────────────
        public int ScanLineMaxColorsPerChunk { get; set; } = 0;
        public int ScanLineChunkHeight { get; set; } = 1;
        public ColorTransformScanLine.ScanlinePaletteMode ScanlinePaletteMode { get; set; } = ColorTransformScanLine.ScanlinePaletteMode.IndependentChunkPalette;
        public ColorTransformType ScanLineInternalTransformationModel { get; set; } = ColorTransformType.ColorReductionFast;
  

        // ── Palette ─────────────────────────────────────────────────────────
        private object paletteLock = new object();
        public bool ReferencePaletteWriteLock { get; private set; } = false;
        public Palette ReferencePalette { get; private set; } = new Palette();
        public Palette ColorBackgroundPalette { get; set; } = new Palette();
        public int ColorBackgroundReplacementValue { get; set; } = ColorIntExt.FromRGB(0, 0, 0);


        
        // ── Clustering ────────────────────────────────────────────────────────────
        public int ClusterTrainingLoop { get; set; } = 10;
        public ColorSelectionMode ClusterColorSelectionMode { get; set; } = ColorSelectionMode.EvaluateColorMean;
        public ClusterPaletteInitializationMode ClusterPaletteInitializationMode { get; set; } = ClusterPaletteInitializationMode.ImagePalette;

        // ── ColorMean ────────────────────────────────────────────────────────────
        public ColorSelectionMode MedianCutColorSelectionMode { get; set; } = ColorSelectionMode.EvaluateColorMean;

        // ── Dithering ────────────────────────────────────────────────────────
        public DitherConfig DitheringCfg { get; set; } = new DitherConfig();

        // ── HSV adjustments ──────────────────────────────────────────────────
        public double HsvHueShift { get; set; } = 0.0;
        public double HsvSaturationMultFactor { get; set; } = 1.0;
        public double HsvBrightnessMultFactor { get; set; } = 1.0;

        // ── Platform-specific: C64 ───────────────────────────────────────────
        public ColorTransformCbm64.C64VideoMode C64VideoMode { get; set; }
            = ColorTransformCbm64.C64VideoMode.Multicolor;
        public ColorTransformCbm64.C64DitheringMode C64DitheringMode { get; set; }
            = ColorTransformCbm64.C64DitheringMode.PreDitherImage;

        // ── Platform-specific: Amiga ─────────────────────────────────────────
        public ColorTransformCbmAmiga.EnumAmigaVideoMode AmigaVideoMode { get; set; }
            = ColorTransformCbmAmiga.EnumAmigaVideoMode.Ham6;

        public ColorSelectionMode AmigaColorSelectionMode { get; set; } = ColorSelectionMode.UseColorPalette;
        public ColorTransformType AmigaInternalTransformationModel { get; set; } = ColorTransformType.ColorReductionFast;


        // ── Platform-specific: CPC ───────────────────────────────────────────
        public ColorTransformAmstradCPC.CPCVideoMode CPCVideoMode { get; set; }
            = ColorTransformAmstradCPC.CPCVideoMode.Mode0;

        // ── Platform-specific: CGA ───────────────────────────────────

        public ColorTransformCGA.CGAVideoMode CGAVideoMode { get; set; }
            = ColorTransformCGA.CGAVideoMode.Mode4_0L;

        public ColorTransformCGA.CGAOutputMode CGAOutputMode { get; set; }
            = ColorTransformCGA.CGAOutputMode.ProcessedImage;

        // ── Platform-specific: ZX Spectrum ───────────────────────────────────
        public ColorTransformZxSpectrum.ZxPaletteMode ZxPaletteMode { get; set; }
            = ColorTransformZxSpectrum.ZxPaletteMode.BothPalettes;

        public int ZxPaletteInColorSeedLow { get; set; } = 192;
        public int ZxPaletteInColorSeedHigh { get; set; } = 255;
        public bool ZxIncludeBlackInHighColorImage { get; set; } = false;
        public bool ZxDitherLowColorImage { get; set; } = false;
        public bool ZxDitherHighColorImage { get; set; } = false;
        public ColorTransformZxSpectrum.ZxAutotuneMode ZxAutotuneMode { get; set; } = ColorTransformZxSpectrum.ZxAutotuneMode.None;


        /// <summary>
        /// Creates a indipendent copy of the current ColorTransformConfig instance.
        /// </summary>
        /// <returns>cloned instance</returns>
        public ColorTransformConfig Clone()
        {
            lock (paletteLock)
            {
                var ret = base.MemberwiseClone() as ColorTransformConfig;
                ret.paletteLock = new object();
                if (ReferencePalette != null)
                {
                    ret.ReferencePalette = new Palette().Create(this.ReferencePalette);
                }
                else
                {
                    ReferencePalette = new Palette();
                }
                if (ColorBackgroundPalette != null)
                {
                    ret.ColorBackgroundPalette = new Palette().Create(this.ColorBackgroundPalette);
                }
                else
                {
                    ret.ColorBackgroundPalette = new Palette();
                }
                return ret;
            }
        }

        //public ColorTransformConfig WithColorMean(ColorSelectionMode colorMeanMode)
        //{
        //    ColorMeanColorSelectionMode = colorMeanMode;
        //    return this;
        //}


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

        public ColorTransformConfig WithColorDistanceEvaluationMode(ColorDistanceEvaluationMode mode)
        {
            ColorDistanceEvaluationMode = mode;
            return this;
        }

        public ColorTransformConfig WithBackgroundColorReplacement(Palette colorPalette, int replacementColor)
        {
            ColorBackgroundPalette = new Palette().Create(colorPalette);
            ColorBackgroundReplacementValue = replacementColor;
            return this;
        }

        public ColorTransformConfig WithQuantizationMode(ColorQuantizationMode mode)
        {
            QuantizationMode = mode;
            return this;
        }

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
        public ColorTransformConfig WithDithering(DitherConfig cfg)
        {
            DitheringCfg = cfg?.Clone() as DitherConfig ?? throw new ArgumentNullException(nameof(cfg));
            return this;
        }

        public ColorTransformConfig WithDitheringType(ColorDithering ditheringType)
        {
            DitheringCfg.DitheringType = ditheringType;
            return this;
        }
        public ColorTransformConfig WithDitheringStrength(double strength)
        {
            DitheringCfg.DitheringStrength = strength;
            return this;
        }
        public ColorTransformConfig WithDitheringFx(ColorDitheringFx fx)
        {
            DitheringCfg.DitheringFx = fx;
            return this;
        }

        public ColorTransformConfig WithAmigaScreenMode(EnumAmigaVideoMode videoMode, ColorTransformType colorTransformationMode, ColorSelectionMode colorMeanMode)
        {
            AmigaVideoMode = videoMode;
            AmigaInternalTransformationModel = colorTransformationMode;
            AmigaColorSelectionMode = colorMeanMode;
            return this;
        }

        public ColorTransformConfig WithC64ScreenMode(C64VideoMode mode, C64DitheringMode ditheringMode, bool showTileBorers)
        {
            C64VideoMode = mode;
            C64DitheringMode = ditheringMode;
            ShowTileBorders = showTileBorers;
            return this;
        }
        public ColorTransformConfig WithCpcVideoMode(CPCVideoMode videoMode)
        {
            CPCVideoMode = videoMode;
            return this;
        }

        public ColorTransformConfig WithClustering(int maxColorsWanted, int trainingLoop, ColorSelectionMode colorMeanMode)
        {
            MaxColorsWanted = maxColorsWanted;
            ClusterTrainingLoop = trainingLoop;
            ClusterColorSelectionMode = colorMeanMode;
            return this;
        }

        public ColorTransformConfig WithFastReduction(int maxColorsWanted)
        {
            MaxColorsWanted = maxColorsWanted;
            return this;
        }

        public ColorTransformConfig WithMaxColorWanted(int maxColors)
        {
            MaxColorsWanted = maxColors;
            return this;
        }

        public ColorTransformConfig WithMedianCut(int maxColors )
        {
            MaxColorsWanted = maxColors;
            return this;
        }
        public ColorTransformConfig WithScanline(int chunkHeight, ColorTransformScanLine.ScanlinePaletteMode scanlineReductionMode, int maxColorWanted, int maxColorChangePerLine, ColorTransformType internalTransformtionMode )
        {
            ScanlinePaletteMode = scanlineReductionMode;  
            MaxColorsWanted = maxColorWanted;
            ScanLineMaxColorsPerChunk = maxColorChangePerLine;
            ScanLineInternalTransformationModel = internalTransformtionMode;
            return this;
        }

        public ColorTransformConfig WithMedianCut(ColorSelectionMode colorMeanMode)
        {
            MedianCutColorSelectionMode = colorMeanMode;
            return this;
        }

        public ColorTransformConfig WithZxScreenMode(ZxPaletteMode paletteMode, int lowColorInSeed, int highColorInSeed )
        {
            ZxPaletteMode = paletteMode;
            ZxPaletteInColorSeedLow = lowColorInSeed;
            ZxPaletteInColorSeedHigh = highColorInSeed;
            return this;
        }
        public ColorTransformConfig WithZxProcessing(ZxAutotuneMode autotuneMode, bool ditherLowColorImage, bool ditherHighColorImage, bool includeBlackInHighColor, bool showTileBorders)
        {
            ZxDitherLowColorImage = ditherLowColorImage;
            ZxDitherHighColorImage = ditherHighColorImage;
            ZxIncludeBlackInHighColorImage = includeBlackInHighColor;
            ZxAutotuneMode = autotuneMode;
            ShowTileBorders = showTileBorders;
            return this;
        }
        public ColorTransformConfig WithHSV(double hueShift, double saturationMultFactor, double brightnessMultFactor)
        {
            HsvHueShift = (int)hueShift;
            HsvSaturationMultFactor = saturationMultFactor;
            HsvBrightnessMultFactor = brightnessMultFactor;
            return this;
        }       
    }
}