using ColourClashNet.Color.Dithering;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Base of the immutable option records of every color transformation.
    /// <para/>
    /// Holds the settings shared by all transformations; each transformation has its own derived record
    /// (e.g. <see cref="ClusteringOptions"/>, <see cref="ZxOptions"/>) with only the settings it uses.
    /// Records are immutable: change a value with <c>options with { ... }</c>, which keeps the runtime type,
    /// so the same instance can be shared by transformations, tiles and threads without copies.
    /// <para/>
    /// The record type identifies the algorithm: <see cref="ColorTransformBase.CreateColorTransformInterface(TransformOptions)"/>
    /// creates the matching transformation, and <see cref="TransformOptionsSerializer"/> saves and loads presets as JSON.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(IdentityOptions), "Identity")]
    [JsonDerivedType(typeof(BackgroundRemovalOptions), "BackgroundRemoval")]
    [JsonDerivedType(typeof(QuantizationOptions), "Quantization")]
    [JsonDerivedType(typeof(FastOptions), "Fast")]
    [JsonDerivedType(typeof(MedianCutOptions), "MedianCut")]
    [JsonDerivedType(typeof(ClusteringOptions), "Clustering")]
    [JsonDerivedType(typeof(FixedPaletteOptions), "FixedPalette")]
    [JsonDerivedType(typeof(EnhancedPaletteOptions), "EnhancedPalette")]
    [JsonDerivedType(typeof(HsvOptions), "Hsv")]
    [JsonDerivedType(typeof(ScanlineOptions), "Scanline")]
    [JsonDerivedType(typeof(HerculesOptions), "Hercules")]
    [JsonDerivedType(typeof(CgaOptions), "Cga")]
    [JsonDerivedType(typeof(EgaOptions), "Ega")]
    [JsonDerivedType(typeof(VgaOptions), "Vga")]
    [JsonDerivedType(typeof(C64Options), "C64")]
    [JsonDerivedType(typeof(AmigaOptions), "Amiga")]
    [JsonDerivedType(typeof(CpcOptions), "Cpc")]
    [JsonDerivedType(typeof(ZxOptions), "ZxSpectrum")]
    public abstract record TransformOptions
    {
        /// <summary>
        /// Transformation that uses these options.
        /// </summary>
        [JsonIgnore, Browsable(false)]
        public abstract ColorTransformType TransformType { get; }

        /// <summary>
        /// Metric used to compare colors.
        /// </summary>
        [Category("Common"), Description("Metric used to compare colors")]
        public ColorDistanceEvaluationMode DistanceMode { get; init; } = ColorDistanceEvaluationMode.RGB;

        /// <summary>
        /// Dithering applied after the color reduction.
        /// </summary>
        [Category("Common"), Description("Dithering applied after the color reduction")]
        public DitherOptions Dithering { get; init; } = DitherOptions.None;

        /// <summary>
        /// Colors chosen by the user.
        /// <para/>
        /// <see cref="FastOptions"/> and <see cref="ClusteringOptions"/> keep them in the output palette (clustering never moves them);
        /// <see cref="FixedPaletteOptions"/> and <see cref="EnhancedPaletteOptions"/> use them as target palette;
        /// <see cref="MedianCutOptions"/> ignores them.
        /// Hardware transformations (CGA, EGA, C64, ZX ...) use their own palette and ignore it.
        /// </summary>
        [Category("Common"), Description("Colors chosen by the user (fixed colors or target palette)")]
        public IReadOnlyList<int> UserPalette { get; init; } = Array.Empty<int>();

        /// <summary>
        /// Returns the validation errors of the options (empty when valid).
        /// <para/>
        /// Derived records extend the base checks; nested options are validated too.
        /// </summary>
        public virtual IEnumerable<string> Validate()
        {
            if (!Enum.IsDefined(DistanceMode))
                yield return $"{nameof(DistanceMode)} {DistanceMode} is not a valid metric";
            if (Dithering == null)
                yield return $"{nameof(Dithering)} is missing";
            else
                foreach (var error in Dithering.Validate())
                    yield return $"{nameof(Dithering)} : {error}";
            if (UserPalette == null)
                yield return $"{nameof(UserPalette)} is missing (use an empty list)";
        }

        /// <summary>
        /// Copies the common settings (<see cref="DistanceMode"/>, <see cref="Dithering"/>, <see cref="UserPalette"/>)
        /// of this record into <paramref name="nested"/>, keeping its specific settings.
        /// <para/>
        /// Used by composite transformations to configure their nested steps consistently.
        /// </summary>
        /// <typeparam name="T">Nested options type</typeparam>
        /// <param name="nested">Options of the nested step</param>
        /// <returns>A copy of <paramref name="nested"/> with the common settings of this record.</returns>
        public T InheritCommon<T>(T nested) where T : TransformOptions
        {
            ArgumentNullException.ThrowIfNull(nested);
            TransformOptions copy = nested;
            return (T)(copy with { DistanceMode = DistanceMode, Dithering = Dithering, UserPalette = UserPalette });
        }

        /// <summary>
        /// Creates an independent deep copy (JSON round trip).
        /// <para/>
        /// Library code never needs it, because records are immutable; it isolates a snapshot from
        /// in-place edits made through reflection, e.g. by a PropertyGrid.
        /// </summary>
        public TransformOptions DeepClone() => TransformOptionsSerializer.Deserialize(TransformOptionsSerializer.Serialize(this));

        /// <summary>
        /// Creates the default options of a transformation type.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Unsupported transformation type.</exception>
        public static TransformOptions CreateDefault(ColorTransformType type) => type switch
        {
            ColorTransformType.None or ColorTransformType.ColorIdentity => new IdentityOptions(),
            ColorTransformType.ColorRemover => new BackgroundRemovalOptions(),
            ColorTransformType.ColorReductionQuantization => new QuantizationOptions(),
            ColorTransformType.ColorReductionFast => new FastOptions(),
            ColorTransformType.ColorReductionMedianCut => new MedianCutOptions(),
            ColorTransformType.ColorReductionClustering => new ClusteringOptions(),
            ColorTransformType.ColorReductionGenericPalette => new FixedPaletteOptions(),
            ColorTransformType.ColorReductionEnhancedPalette => new EnhancedPaletteOptions(),
            ColorTransformType.ColorReductionSaturation => new HsvOptions(),
            ColorTransformType.ColorReductionScanline => new ScanlineOptions(),
            ColorTransformType.ColorReductionHercules => new HerculesOptions(),
            ColorTransformType.ColorReductionCGA => new CgaOptions(),
            ColorTransformType.ColorReductionEGA => new EgaOptions(),
            ColorTransformType.ColorReductionVGA => new VgaOptions(),
            ColorTransformType.ColorReductionCBM64 => new C64Options(),
            ColorTransformType.ColorReductionHam => new AmigaOptions(),
            ColorTransformType.ColorReductionCPC => new CpcOptions(),
            ColorTransformType.ColorReductionZxSpectrum => new ZxOptions(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Transform type {type} not supported"),
        };

        /// <summary>
        /// Short description used by the PropertyGrid.
        /// </summary>
        public override string ToString() => TransformType.ToString();
    }

    /// <summary>
    /// Base of the general purpose color reductions (fast, median cut, clustering),
    /// which composite transformations use as interchangeable nested steps.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(FastOptions), "Fast")]
    [JsonDerivedType(typeof(MedianCutOptions), "MedianCut")]
    [JsonDerivedType(typeof(ClusteringOptions), "Clustering")]
    public abstract record ColorReductionOptions : TransformOptions
    {
        /// <summary>
        /// Maximum number of output colors.
        /// </summary>
        [Category("Reduction"), Description("Maximum number of output colors")]
        public int MaxColors { get; init; } = 16;

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            if (MaxColors < 1)
                yield return $"{nameof(MaxColors)} must be at least 1 (is {MaxColors})";
        }
    }

    /// <summary>
    /// Shared validation helpers of the option records.
    /// </summary>
    internal static class OptionsValidation
    {
        /// <summary>
        /// Validates nested options, prefixing each error with <paramref name="name"/>.
        /// </summary>
        internal static IEnumerable<string> ValidateNested(string name, TransformOptions? nested)
        {
            if (nested == null)
            {
                yield return $"{name} is missing";
                yield break;
            }
            foreach (var error in nested.Validate())
                yield return $"{name} : {error}";
        }
    }
}
