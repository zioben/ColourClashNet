using ColourClashNet.Color;
using System.Text.Json.Serialization;
using ColourClashNet;
using ColourClashProcessing.Threshold;
using System.ComponentModel;

namespace ColourClashProcessing.Blob
{
    /// <summary>
    /// Immutable settings of <see cref="Blob"/>: threshold, connectivity, size filters and rebuild color.
    /// <para/>
    /// Change a value with a <c>with</c> expression, e.g. <c>blob.Options = blob.Options with { MinArea = 10 }</c>.
    /// Size limits set to 0 are disabled.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed record BlobOptions
    {
        /// <summary>
        /// Foreground / background split.
        /// </summary>
        [Category("Threshold"), Description("Foreground / background split")]
        public ThresholdOptions Threshold { get; init; } = new();

        /// <summary>
        /// Pixel connectivity.
        /// </summary>
        [Category("Processing"), Description("Pixel connectivity")]
        public BlobConnectionModes ConnectionMode { get; init; } = BlobConnectionModes.EightConnected;

        /// <summary>Minimum blob width (0 = no limit).</summary>
        [Category("Filtering")]
        public int MinWidth { get; init; } = 0;

        /// <summary>Minimum blob height (0 = no limit).</summary>
        [Category("Filtering")]
        public int MinHeight { get; init; } = 0;

        /// <summary>Minimum blob area (0 = no limit).</summary>
        [Category("Filtering")]
        public int MinArea { get; init; } = 0;

        /// <summary>Maximum blob width (0 = no limit).</summary>
        [Category("Filtering")]
        public int MaxWidth { get; init; } = 0;

        /// <summary>Maximum blob height (0 = no limit).</summary>
        [Category("Filtering")]
        public int MaxHeight { get; init; } = 0;

        /// <summary>Maximum blob area (0 = no limit).</summary>
        [Category("Filtering")]
        public int MaxArea { get; init; } = 0;

        /// <summary>
        /// Background color used when blobs are rebuilt as images.
        /// </summary>
        [Category("Rebuild"), Description("Background color of the rebuilt blob images")]
        [JsonConverter(typeof(ColorJsonConverter))]
        public int BackgroundColor { get; init; } = ColorDefaults.DefaultBkgColorInt;

        /// <summary>
        /// Returns the validation errors of the settings (empty when valid).
        /// </summary>
        public IEnumerable<string> Validate()
        {
            if (Threshold == null)
                yield return $"{nameof(Threshold)} is missing";
            else
                foreach (var error in Threshold.Validate())
                    yield return $"{nameof(Threshold)} : {error}";
            if (!Enum.IsDefined(ConnectionMode))
                yield return $"{nameof(ConnectionMode)} {ConnectionMode} is not valid";
            foreach (var (name, min, max) in new[] { ("Width", MinWidth, MaxWidth), ("Height", MinHeight, MaxHeight), ("Area", MinArea, MaxArea) })
            {
                if (min < 0 || max < 0)
                    yield return $"Min{name} and Max{name} must not be negative";
                else if (max > 0 && min > max)
                    yield return $"Min{name} ({min}) is greater than Max{name} ({max})";
            }
        }
    }
}
