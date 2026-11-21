using System.ComponentModel;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Options of the complete processing pipeline: background removal, quantization, then the selected transformation.
    /// <para/>
    /// Pre-processing settings live here, not in the options of each transformation.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed record PipelineOptions
    {
        /// <summary>
        /// Background removal applied first (never dithered).
        /// </summary>
        [Category("Pre-processing"), Description("Background removal applied first")]
        public BackgroundRemovalOptions Background { get; init; } = new();

        /// <summary>
        /// Quantization applied after the background removal (never dithered).
        /// </summary>
        [Category("Pre-processing"), Description("Quantization applied after the background removal")]
        public ColorQuantizationMode Quantization { get; init; } = ColorQuantizationMode.RGB888;

        /// <summary>
        /// Transformation applied to the pre-processed image.
        /// </summary>
        [Category("Transformation"), Description("Transformation applied to the pre-processed image")]
        public TransformOptions Transform { get; init; } = new ClusteringOptions();

        /// <summary>
        /// Returns the validation errors of all the stages (empty when valid).
        /// </summary>
        public IEnumerable<string> Validate()
        {
            foreach (var error in OptionsValidation.ValidateNested(nameof(Background), Background))
                yield return error;
            if (!Enum.IsDefined(Quantization))
                yield return $"{nameof(Quantization)} {Quantization} is not valid";
            foreach (var error in OptionsValidation.ValidateNested(nameof(Transform), Transform))
                yield return error;
        }
    }
}
