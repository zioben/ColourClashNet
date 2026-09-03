using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using System.ComponentModel;
using static ColourClashNet.Color.Transformation.ColorTransformReductionAmiga;
using static ColourClashNet.Color.Transformation.ColorTransformReductionC64;
using static ColourClashNet.Color.Transformation.ColorTransformReductionCPC;
using static ColourClashNet.Color.Transformation.ColorTransformReductionZxSpectrum;

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

        // ── Scanline ─────────────────────────────────────────────────────────
        public int MaxColorsPerChunk { get; set; } = 0;
        public int ChunkHeight { get; set; } = 1;
        public ColorTransformReductionScanLine.ScanlineReductionMode ScanlineReductionMode { get; set; } = ColorTransformReductionScanLine.ScanlineReductionMode.IndependentPalettePerLine;

        // ── Palette ─────────────────────────────────────────────────────────
        private object paletteLock = new object();
        public bool ReferencePaletteWriteLock { get; private set; } = false;
        public Palette ReferencePalette { get; private set; } = new Palette();
        public Palette ColorBackgroundList { get; set; } = new Palette();
        public int ColorBackgroundReplacement { get; set; } = ColorIntExt.FromRGB(0, 0, 0);

        // ── Flags ────────────────────────────────────────────────────────────
        public ColorSelectionMode ColorMeanMode { get; set; } = ColorSelectionMode.UseColorPalette;
        public bool UseFixedPalette { get; set; } = false;
        public ColorTransformType InternalTransformationModel { get; set; } = ColorTransformType.ColorReductionFast;
        public int ClusterTrainingLoop { get; set; } = 10;

        // ── Dithering ────────────────────────────────────────────────────────
        public DitherConfig DitheringCfg { get; set; } = new DitherConfig();

        // ── HSV adjustments ──────────────────────────────────────────────────
        public double HsvHueShift { get; set; } = 0.0;
        public double HsvSaturationMultFactor { get; set; } = 1.0;
        public double HsvBrightnessMultFactor { get; set; } = 1.0;

        // ── Platform-specific: C64 ───────────────────────────────────────────
        public ColorTransformReductionC64.C64VideoMode C64VideoMode { get; set; }
            = ColorTransformReductionC64.C64VideoMode.Multicolor;
        public ColorTransformReductionC64.C64DitheringMode C64DitheringMode { get; set; }
            = ColorTransformReductionC64.C64DitheringMode.PreDitherImage;

        // ── Platform-specific: Amiga ─────────────────────────────────────────
        public ColorTransformReductionAmiga.EnumAmigaVideoMode AmigaVideoMode { get; set; }
            = ColorTransformReductionAmiga.EnumAmigaVideoMode.Ham6;

        // ── Platform-specific: CPC ───────────────────────────────────────────
        public ColorTransformReductionCPC.CPCVideoMode CPCVideoMode { get; set; }
            = ColorTransformReductionCPC.CPCVideoMode.Mode0;

        // ── Platform-specific: CGA ───────────────────────────────────

        public ColorTransformReductionCGA.CGAVideoMode CGAVideoMode { get; set; }
            = ColorTransformReductionCGA.CGAVideoMode.Mode4_0L;

        // ── Platform-specific: ZX Spectrum ───────────────────────────────────
        public ColorTransformReductionZxSpectrum.ZxPaletteMode ZxPaletteMode { get; set; }
            = ColorTransformReductionZxSpectrum.ZxPaletteMode.BothPalettes;

        public int ZxPaletteInColorSeedLow { get; set; } = 192;
        public int ZxPaletteInColorSeedHigh { get; set; } = 255;
        public bool ZxIncludeBlackInHighColorImage { get; set; } = false;
        public bool ZxDitherLowColorImage { get; set; } = false;
        public bool ZxDitherHighColorImage { get; set; } = false;
        public ColorTransformReductionZxSpectrum.ZxAutotuneMode ZxAutotuneMode { get; set; } = ColorTransformReductionZxSpectrum.ZxAutotuneMode.None;
        public bool ShowTileBorders { get; set; } = true;
        public int TileBorderColor { get; set; } = ColorIntExt.FromRGB(255, 0, 255);

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
                if (ColorBackgroundList != null)
                {
                    ret.ColorBackgroundList = new Palette().Create(this.ColorBackgroundList);
                }
                else
                {
                    ret.ColorBackgroundList = new Palette();
                }
                return ret;
            }
        }

        public ColorTransformConfig WithColorMean(ColorSelectionMode colorMeanMode)
        {
            ColorMeanMode = colorMeanMode;
            return this;
        }


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
            ColorBackgroundList = new Palette().Create(colorPalette);
            ColorBackgroundReplacement = replacementColor;
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
            InternalTransformationModel = colorTransformationMode;
            ColorMeanMode = colorMeanMode;
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
            ColorMeanMode = colorMeanMode;
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
        public ColorTransformConfig WithScanline(int chunkHeight, ColorTransformReductionScanLine.ScanlineReductionMode scanlineReductionMode, int maxColorWanted, int maxColorChangePerLine, ColorTransformType internalTransformtionMode, ColorSelectionMode colorMeanMode )
        {
            ScanlineReductionMode = scanlineReductionMode;  
            MaxColorsWanted = maxColorWanted;
            MaxColorsPerChunk = maxColorChangePerLine;
            InternalTransformationModel = internalTransformtionMode;
            ColorMeanMode = colorMeanMode;
            return this;
        }

        public ColorTransformConfig WithZxScreenMode(ZxPaletteMode paletteMode, int lowColorInSeed, int highColorInSeed )
        {
            this.ZxPaletteMode = paletteMode;
            this.ZxPaletteInColorSeedLow = lowColorInSeed;
            this.ZxPaletteInColorSeedHigh = highColorInSeed;
            return this;
        }
        public ColorTransformConfig WithZxProcessing(ZxAutotuneMode autotuneMode, bool ditherLowColorImage, bool ditherHighColorImage, bool includeBlackInHighColor, bool showTileBorders)
        {
            this.ZxDitherLowColorImage = ditherLowColorImage;
            this.ZxDitherHighColorImage = ditherHighColorImage;
            this.ZxIncludeBlackInHighColorImage = includeBlackInHighColor;
            this.ZxAutotuneMode = autotuneMode;
            this.ShowTileBorders = showTileBorders;
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