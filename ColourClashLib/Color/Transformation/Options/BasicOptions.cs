using System.Text.Json.Serialization;
using System.ComponentModel;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Options of <see cref="ColorTransformIdentity"/> (no specific settings).
    /// </summary>
    public sealed record IdentityOptions : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorIdentity;
    }

    /// <summary>
    /// Options of <see cref="ColorTransformBkgRemover"/>.
    /// </summary>
    public sealed record BackgroundRemovalOptions : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorRemover;

        /// <summary>
        /// Colors treated as background.
        /// </summary>
        [Category("Background"), Description("Colors treated as background")]
        [JsonConverter(typeof(ColorListJsonConverter))]
        public IReadOnlyList<int> BackgroundColors { get; init; } = Array.Empty<int>();

        /// <summary>
        /// Color that replaces the background colors.
        /// </summary>
        [Category("Background"), Description("Color that replaces the background colors")]
        [JsonConverter(typeof(ColorJsonConverter))]
        public int ReplacementColor { get; init; } = ColorIntExt.FromRGB(0, 0, 0);

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            if (BackgroundColors == null)
                yield return $"{nameof(BackgroundColors)} is missing (use an empty list)";
        }
    }

    /// <summary>
    /// Options of <see cref="ColorTransformQuantization"/>.
    /// </summary>
    public sealed record QuantizationOptions : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionQuantization;

        /// <summary>
        /// Target channel bit depth.
        /// </summary>
        [Category("Quantization"), Description("Target channel bit depth")]
        public ColorQuantizationMode Mode { get; init; } = ColorQuantizationMode.RGB888;

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            if (!Enum.IsDefined(Mode))
                yield return $"{nameof(Mode)} {Mode} is not valid";
        }
    }

    /// <summary>
    /// Options of <see cref="ColorTransformFixedPalette"/>: the target palette is <see cref="TransformOptions.UserPalette"/>.
    /// </summary>
    public sealed record FixedPaletteOptions : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionGenericPalette;

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            if (UserPalette != null && UserPalette.Count == 0)
                yield return $"{nameof(UserPalette)} is empty: the fixed palette needs at least one color";
        }
    }

    /// <summary>
    /// Options of <see cref="ColorTransformFixedPaletteEnhanced"/>: <see cref="TransformOptions.UserPalette"/>
    /// plus the mean of every pair of its colors.
    /// </summary>
    public sealed record EnhancedPaletteOptions : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionEnhancedPalette;

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            if (UserPalette != null && UserPalette.Count == 0)
                yield return $"{nameof(UserPalette)} is empty: the enhanced palette needs at least one color";
        }
    }

    /// <summary>
    /// Options of <see cref="ColorTransformLumSat"/>.
    /// </summary>
    public sealed record HsvOptions : TransformOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionSaturation;

        /// <summary>
        /// Hue rotation in degrees: any finite value, wrapped on the color wheel (e.g. 390 = 30, -90 = 270).
        /// </summary>
        [Category("HSV"), Description("Hue rotation in degrees")]
        public double HueShift { get; init; } = 0.0;

        /// <summary>
        /// Saturation multiplier (the result is capped at 100%).
        /// </summary>
        [Category("HSV"), Description("Saturation multiplier")]
        public double SaturationFactor { get; init; } = 1.0;

        /// <summary>
        /// Brightness (value) multiplier (the result is capped at 100%).
        /// </summary>
        [Category("HSV"), Description("Brightness multiplier")]
        public double BrightnessFactor { get; init; } = 1.0;

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            if (!double.IsFinite(HueShift))
                yield return $"{nameof(HueShift)} must be a finite number (is {HueShift})";
            if (double.IsNaN(SaturationFactor) || SaturationFactor < 0)
                yield return $"{nameof(SaturationFactor)} must not be negative (is {SaturationFactor})";
            if (double.IsNaN(BrightnessFactor) || BrightnessFactor < 0)
                yield return $"{nameof(BrightnessFactor)} must not be negative (is {BrightnessFactor})";
        }
    }
}
