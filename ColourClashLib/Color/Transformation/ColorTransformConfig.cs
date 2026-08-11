using ColourClashNet.Color;
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
        public int MaxColorChangePerLine { get; set; } = 0;

        // ── Palette ─────────────────────────────────────────────────────────
        public Palette ReferencePalette { get; set; } = new Palette();
        public Palette ColorBackgroundList { get; set; } = new Palette();
        public int ColorBackgroundReplacement { get; set; } = ColorIntExt.FromRGB(0, 0, 0);

        // ── Flags ────────────────────────────────────────────────────────────
        public bool UseColorMean { get; set; } = false;
        public bool UseFixedPalette { get; set; } = false;
        public bool UseSharedPalette { get; set; } = false;
        public bool UseClustering { get; set; } = false;
        public int ClusterTrainingLoop { get; set; } = 10;

        // ── Dithering ────────────────────────────────────────────────────────
        public ColorDithering DitheringType { get; set; } = ColorDithering.None;
        public double DitheringStrength { get; set; } = 1.0;
        public ColorDitheringFx DitheringFx { get; set; } = ColorDitheringFx.None;

        // ── HSV adjustments ──────────────────────────────────────────────────
        public int HsvHueShift { get; set; } = 0;
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
        public EnumHamColorProcessingMode AmigaProcessingMode { get; private set; }
        public ColorTransformReductionAmiga.EnumHamColorProcessingMode AmigaHamColorProcessingMode { get; set; }
            = ColorTransformReductionAmiga.EnumHamColorProcessingMode.Detailed;

        // ── Platform-specific: CPC ───────────────────────────────────────────
        public ColorTransformReductionCPC.CPCVideoMode CPCVideoMode { get; set; }
            = ColorTransformReductionCPC.CPCVideoMode.Mode0;

        // ── Platform-specific: ZX Spectrum ───────────────────────────────────
        public ColorTransformReductionZxSpectrum.ZxPaletteMode ZxPaletteMode { get; set; }
            = ColorTransformReductionZxSpectrum.ZxPaletteMode.Both;

        public int ZxColLSeed { get; set; } = 192;
        public int ZxColHSeed { get; set; } = 255;
        public bool ZxIncludeBlackInHighColorImage { get; set; } = false;
        public bool ZxDitherLowColorImage { get; set; } = false;
        public bool ZxDitherHighColorImage { get; set; } = false;
        public ColorTransformReductionZxSpectrum.ZxAutotuneMode ZxAutotuneMode { get; set; } = ColorTransformReductionZxSpectrum.ZxAutotuneMode.None;
        public bool ShowTileBorders { get; set; } = true;

        /// <summary>
        /// Creates a indipendent copy of the current ColorTransformConfig instance.
        /// </summary>
        /// <returns>cloned instance</returns>
        public ColorTransformConfig Clone()
        {
            var ret = base.MemberwiseClone() as ColorTransformConfig;
            if (ReferencePalette != null)
            {
                ret.ReferencePalette = new Palette().Create(this.ReferencePalette);
            }
            if(ColorBackgroundList != null)
            {
                ret.ColorBackgroundList = new Palette().Create(this.ColorBackgroundList);
            };
            return ret;
        }

        public ColorTransformConfig WithReferencePalette(Palette palette)
        {
            ReferencePalette = palette;
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
            DitheringType = ditheringType;
            DitheringStrength = strength;
            DitheringFx = fx;
            return this;
        }
        public ColorTransformConfig WithDitheringType(ColorDithering ditheringType)
        {
            DitheringType = ditheringType;
            return this;
        }
        public ColorTransformConfig WithDitheringStrength(double strength)
        {
            DitheringStrength = strength;
            return this;
        }
        public ColorTransformConfig WithDitheringFx(ColorDitheringFx fx)
        {
            DitheringFx = fx;
            return this;
        }

        public ColorTransformConfig WithAmigaScreenMode(EnumAmigaVideoMode mode, EnumHamColorProcessingMode processingMode)
        {
            AmigaVideoMode = mode;
            AmigaProcessingMode = processingMode;
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

        public ColorTransformConfig WithClustering(int maxColorsWanted, int trainingLoop, bool useClusterColorMean)
        {
            MaxColorsWanted = maxColorsWanted;
            ClusterTrainingLoop = trainingLoop;
            UseColorMean = useClusterColorMean;
            return this;
        }

        public ColorTransformConfig WithFastReduction(int maxColorsWanted)
        {
            MaxColorsWanted = maxColorsWanted;
            return this;
        }

        public ColorTransformConfig WithMedianCut(int maxColors, bool useColorMean)
        {
            MaxColorsWanted = maxColors;
            UseColorMean = useColorMean;
            return this;
        }
        public ColorTransformConfig WithScanline(bool createSharedPalette, int colorsMaxWanted, int lineReductionMaxColors, bool lineReductionClustering, bool useColorMean)
        {
            UseSharedPalette = createSharedPalette;
            MaxColorsWanted = colorsMaxWanted;
            MaxColorChangePerLine = lineReductionMaxColors;
            UseClustering = lineReductionClustering;
            UseColorMean = useColorMean;
            return this;
        }

        public ColorTransformConfig WithZxScreenMode(ZxPaletteMode paletteMode, int lowColorInSeed, int highColorInSeed )
        {
            this.ZxPaletteMode = paletteMode;
            this.ZxColLSeed = lowColorInSeed;
            this.ZxColHSeed = highColorInSeed;
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