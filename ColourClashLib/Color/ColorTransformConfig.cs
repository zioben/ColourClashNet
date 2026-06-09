using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using static ColourClashNet.Color.Transformation.ColorTransformReductionAmiga;
using static ColourClashNet.Color.Transformation.ColorTransformReductionC64;
using static ColourClashNet.Color.Transformation.ColorTransformReductionCPC;
using static ColourClashNet.Color.Transformation.ColorTransformReductionZxSpectrum;

namespace ColourClashLib.Color
{
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
        public Palette PriorityPalette { get; set; } = new Palette();
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
        public ColorDitheringFx DitheringFx { get; set; } = ColorDitheringFx.Full;

        // ── HSV adjustments ──────────────────────────────────────────────────
        public int HsvHueShift { get; set; } = 0;
        public double HsvSaturationMultFactor { get; set; } = 1.0;
        public double HsvBrightnessMultFactor { get; set; } = 1.0;

        // ── Platform-specific: C64 ───────────────────────────────────────────
        public ColorTransformReductionC64.C64VideoMode C64VideoMode { get; set; }
            = ColorTransformReductionC64.C64VideoMode.BitmapModeMulticolor;
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

        public ColorTransformConfig WithPalette(Palette palette)
        {
            PriorityPalette = palette;
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

        public ColorTransformConfig WithDithering(ColorDithering ditheringType, double strength = 1.0, ColorDitheringFx fx = ColorDitheringFx.Full)
        {
            DitheringType = ditheringType;
            DitheringStrength = strength;
            DitheringFx = fx;
            return this;
        }

        public ColorTransformConfig WithAmigaProperties(EnumAmigaVideoMode mode, EnumHamColorProcessingMode processingMode)
        {
            AmigaVideoMode = mode;
            AmigaProcessingMode = processingMode;
            return this;
        }

        public ColorTransformConfig WithC64ScreenMode(C64VideoMode mode, C64DitheringMode ditheringMode)
        {
            C64VideoMode = mode;
            C64DitheringMode = ditheringMode;
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

        public ColorTransformConfig WithZxSpectrum(int lowColorInSeed, int highColorInSeed, bool ditherLowColorImage, bool ditherHighColorImage, bool includeBlackInHighColor, ZxPaletteMode paletteMode, ZxAutotuneMode autotuneMode)
        {
            this.ZxColLSeed = lowColorInSeed;
            this.ZxColHSeed = highColorInSeed;
            this.ZxDitherLowColorImage = ditherLowColorImage;
            this.ZxDitherHighColorImage= ditherHighColorImage;
            this.ZxIncludeBlackInHighColorImage = includeBlackInHighColor;
            this.ZxPaletteMode = paletteMode;
            this.ZxAutotuneMode = autotuneMode;
            return this;
        }
    }
}