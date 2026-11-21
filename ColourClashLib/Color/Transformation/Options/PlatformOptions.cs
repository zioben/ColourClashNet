using System.ComponentModel;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Options of <see cref="ColorTransformScanLine"/> (palette per group of lines).
    /// </summary>
    public sealed record ScanlineOptions : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionScanline;

        /// <summary>
        /// Number of lines that share the same palette.
        /// </summary>
        [Category("Scanline"), Description("Number of lines that share the same palette")]
        public int ChunkHeight { get; init; } = 1;

        /// <summary>
        /// How the chunk palettes are computed.
        /// </summary>
        [Category("Scanline"), Description("How the chunk palettes are computed")]
        public ColorTransformScanLine.ScanlinePaletteMode PaletteMode { get; init; } = ColorTransformScanLine.ScanlinePaletteMode.IndependentChunkPalette;

        /// <summary>
        /// Reduction applied to each chunk; its <see cref="ColorReductionOptions.MaxColors"/> is the number of colors per chunk.
        /// </summary>
        [Category("Scanline"), Description("Reduction applied to each chunk")]
        public ColorReductionOptions ChunkReduction { get; init; } = new ClusteringOptions { MaxColors = 8 };

        /// <summary>
        /// Reduction that computes the global palette in <see cref="ColorTransformScanLine.ScanlinePaletteMode.SharedChunksPalette"/> mode.
        /// </summary>
        [Category("Scanline"), Description("Reduction that computes the shared palette")]
        public ColorReductionOptions SharedReduction { get; init; } = new FastOptions { MaxColors = 16 };

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            if (ChunkHeight < 1)
                yield return $"{nameof(ChunkHeight)} must be at least 1 (is {ChunkHeight})";
            if (!Enum.IsDefined(PaletteMode))
                yield return $"{nameof(PaletteMode)} {PaletteMode} is not valid";
            foreach (var error in OptionsValidation.ValidateNested(nameof(ChunkReduction), ChunkReduction))
                yield return error;
            if (PaletteMode == ColorTransformScanLine.ScanlinePaletteMode.SharedChunksPalette)
                foreach (var error in OptionsValidation.ValidateNested(nameof(SharedReduction), SharedReduction))
                    yield return error;
        }
    }

    /// <summary>
    /// Options of <see cref="ColorTransformHercules"/> (no specific settings).
    /// </summary>
    public sealed record HerculesOptions : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionHercules;
    }

    /// <summary>
    /// Options of <see cref="ColorTransformEGA"/> (no specific settings).
    /// </summary>
    public sealed record EgaOptions : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionEGA;
    }

    /// <summary>
    /// Options of <see cref="ColorTransformCGA"/>.
    /// </summary>
    public sealed record CgaOptions : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionCGA;

        /// <summary>
        /// CGA video mode.
        /// </summary>
        [Category("CGA"), Description("CGA video mode")]
        public ColorTransformCGA.CGAVideoMode VideoMode { get; init; } = ColorTransformCGA.CGAVideoMode.Mode4_0L;

        /// <summary>
        /// Output image type.
        /// </summary>
        [Category("CGA"), Description("Output image type")]
        public ColorTransformCGA.CGACompositeOutputMode OutputMode { get; init; } = ColorTransformCGA.CGACompositeOutputMode.ProcessedImage;

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            if (!Enum.IsDefined(VideoMode))
                yield return $"{nameof(VideoMode)} {VideoMode} is not valid";
            if (!Enum.IsDefined(OutputMode))
                yield return $"{nameof(OutputMode)} {OutputMode} is not valid";
        }
    }

    /// <summary>
    /// Options of <see cref="ColorTransformVGA"/>: RGB666 quantization followed by <see cref="Reduction"/>.
    /// </summary>
    public sealed record VgaOptions : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionVGA;

        /// <summary>
        /// Reduction to the VGA palette (256 colors by default).
        /// </summary>
        [Category("VGA"), Description("Reduction to the VGA palette")]
        public ColorReductionOptions Reduction { get; init; } = new ClusteringOptions { MaxColors = 256, TrainingLoops = 10, Selection = ColorSelectionMode.EvaluateColorMean };

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            foreach (var error in OptionsValidation.ValidateNested(nameof(Reduction), Reduction))
                yield return error;
            if (Reduction != null && Reduction.MaxColors > 256)
                yield return $"{nameof(Reduction)} : {nameof(ColorReductionOptions.MaxColors)} must be at most 256 for VGA (is {Reduction.MaxColors})";
        }
    }

    /// <summary>
    /// Options of <see cref="ColorTransformCbm64"/>.
    /// </summary>
    public sealed record C64Options : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionCBM64;

        /// <summary>
        /// Commodore 64 video mode.
        /// </summary>
        [Category("C64"), Description("Commodore 64 video mode")]
        public ColorTransformCbm64.C64VideoMode VideoMode { get; init; } = ColorTransformCbm64.C64VideoMode.Multicolor;

        /// <summary>
        /// Marks the tile grid on the output image.
        /// </summary>
        [Category("C64"), Description("Marks the tile grid on the output image")]
        public bool ShowTileBorders { get; init; } = false;

        /// <summary>
        /// Color used to mark the tile grid.
        /// </summary>
        [Category("C64"), Description("Color used to mark the tile grid")]
        public int TileBorderColor { get; init; } = ColorIntExt.FromRGB(255, 0, 255);

        /// <summary>
        /// Clustering applied to each tile; the number of colors is set by the video mode.
        /// </summary>
        [Category("C64"), Description("Clustering applied to each tile")]
        public ClusteringOptions TileReduction { get; init; } = new ClusteringOptions { TrainingLoops = 10, Selection = ColorSelectionMode.UseColorPalette };

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            if (!Enum.IsDefined(VideoMode))
                yield return $"{nameof(VideoMode)} {VideoMode} is not valid";
            if (VideoMode == ColorTransformCbm64.C64VideoMode.CharsetMulticolor)
                yield return $"{nameof(VideoMode)} {VideoMode} is not implemented yet";
            foreach (var error in OptionsValidation.ValidateNested(nameof(TileReduction), TileReduction))
                yield return error;
        }
    }

    /// <summary>
    /// Options of <see cref="ColorTransformCbmAmiga"/>.
    /// </summary>
    public sealed record AmigaOptions : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionHam;

        /// <summary>
        /// Amiga video mode.
        /// </summary>
        [Category("Amiga"), Description("Amiga video mode")]
        public ColorTransformCbmAmiga.EnumAmigaVideoMode VideoMode { get; init; } = ColorTransformCbmAmiga.EnumAmigaVideoMode.Ham6;

        /// <summary>
        /// Reduction that computes the base palette; the number of colors is set by the video mode
        /// (16 for HAM6, 64 for HAM8, 32 for EHB, where clustering settings are reused).
        /// </summary>
        [Category("Amiga"), Description("Reduction that computes the base palette")]
        public ColorReductionOptions BasePaletteReduction { get; init; } = new ClusteringOptions { Selection = ColorSelectionMode.UseColorPalette };

        /// <summary>
        /// HAM only: following pixels included in the error of each pixel choice, to reduce the color fringes
        /// (0 = current pixel only; each step multiplies the work by up to 4).
        /// </summary>
        [Category("Amiga"), Description("HAM: following pixels evaluated for each choice (0-4), reduces the fringes")]
        public int HamLookahead { get; init; } = 2;

        /// <summary>
        /// Maximum value of <see cref="HamLookahead"/>.
        /// </summary>
        public const int MaxHamLookahead = 4;

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            if (!Enum.IsDefined(VideoMode))
                yield return $"{nameof(VideoMode)} {VideoMode} is not valid";
            if (HamLookahead < 0 || HamLookahead > MaxHamLookahead)
                yield return $"{nameof(HamLookahead)} must be in [0, {MaxHamLookahead}] (is {HamLookahead})";
            foreach (var error in OptionsValidation.ValidateNested(nameof(BasePaletteReduction), BasePaletteReduction))
                yield return error;
        }
    }

    /// <summary>
    /// Options of <see cref="ColorTransformAmstradCPC"/>.
    /// </summary>
    public sealed record CpcOptions : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionCPC;

        /// <summary>
        /// Amstrad CPC video mode.
        /// </summary>
        [Category("Amstrad CPC"), Description("Amstrad CPC video mode")]
        public ColorTransformAmstradCPC.CPCVideoMode VideoMode { get; init; } = ColorTransformAmstradCPC.CPCVideoMode.Mode0;

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            if (!Enum.IsDefined(VideoMode))
                yield return $"{nameof(VideoMode)} {VideoMode} is not valid";
        }
    }

    /// <summary>
    /// Options of <see cref="ColorTransformZxSpectrum"/>.
    /// </summary>
    public sealed record ZxOptions : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionZxSpectrum;

        /// <summary>
        /// ZX Spectrum palettes used.
        /// </summary>
        [Category("ZX Spectrum"), Description("Palettes used")]
        public ColorTransformZxSpectrum.ZxPaletteMode PaletteMode { get; init; } = ColorTransformZxSpectrum.ZxPaletteMode.BothPalettes;

        /// <summary>
        /// Channel intensity of the normal palette as seen in the source image (clamped to [8, <see cref="SeedHigh"/>]).
        /// </summary>
        [Category("ZX Spectrum"), Description("Normal palette intensity in the source image")]
        public int SeedLow { get; init; } = 192;

        /// <summary>
        /// Channel intensity of the bright palette as seen in the source image (clamped to [<see cref="SeedLow"/>, 255]).
        /// </summary>
        [Category("ZX Spectrum"), Description("Bright palette intensity in the source image")]
        public int SeedHigh { get; init; } = 255;

        /// <summary>
        /// Channel intensity of the real normal ZX colors.
        /// </summary>
        [Category("ZX Spectrum"), Description("Real normal color intensity")]
        public int OutputLow { get; init; } = 0xD8;

        /// <summary>
        /// Channel intensity of the real bright ZX colors.
        /// </summary>
        [Category("ZX Spectrum"), Description("Real bright color intensity")]
        public int OutputHigh { get; init; } = 0xFF;

        /// <summary>
        /// Includes black in the bright palette.
        /// </summary>
        [Category("ZX Spectrum"), Description("Includes black in the bright palette")]
        public bool IncludeBlackInHighColor { get; init; } = true;

        /// <summary>
        /// Includes white in the bright palette.
        /// </summary>
        [Category("ZX Spectrum"), Description("Includes white in the bright palette")]
        public bool IncludeWhiteInHighColor { get; init; } = true;

        /// <summary>
        /// Dithers the normal palette image.
        /// </summary>
        [Category("ZX Spectrum"), Description("Dithers the normal palette image")]
        public bool DitherLowColorImage { get; init; } = false;

        /// <summary>
        /// Dithers the bright palette image.
        /// </summary>
        [Category("ZX Spectrum"), Description("Dithers the bright palette image")]
        public bool DitherHighColorImage { get; init; } = false;

        /// <summary>
        /// Automatic search of the input seeds.
        /// </summary>
        [Category("ZX Spectrum"), Description("Automatic search of the input seeds")]
        public ColorTransformZxSpectrum.ZxAutotuneMode Autotune { get; init; } = ColorTransformZxSpectrum.ZxAutotuneMode.None;

        /// <summary>
        /// Clustering applied to each 8x8 attribute cell (2 colors).
        /// </summary>
        [Category("ZX Spectrum"), Description("Clustering applied to each attribute cell")]
        public ClusteringOptions CellReduction { get; init; } = new ClusteringOptions { MaxColors = 2, TrainingLoops = 6, Selection = ColorSelectionMode.UseColorPalette };

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            if (!Enum.IsDefined(PaletteMode))
                yield return $"{nameof(PaletteMode)} {PaletteMode} is not valid";
            if (!Enum.IsDefined(Autotune))
                yield return $"{nameof(Autotune)} {Autotune} is not valid";
            if (OutputLow < 0 || OutputLow > 255 || OutputHigh < 0 || OutputHigh > 255)
                yield return $"{nameof(OutputLow)} and {nameof(OutputHigh)} must be in [0, 255]";
            foreach (var error in OptionsValidation.ValidateNested(nameof(CellReduction), CellReduction))
                yield return error;
        }
    }
}
