using System.Text.Json.Serialization;
using ColourClashNet.Color;
using System.ComponentModel;

namespace ColourClashProcessing.Threshold
{
    /// <summary>
    /// Immutable settings of <see cref="GlobalThreshold"/>.
    /// <para/>
    /// Change a value with a <c>with</c> expression: instances are shared freely.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed record ThresholdOptions
    {
        /// <summary>
        /// Background reference colors (magenta by default).
        /// </summary>
        [Category("Threshold"), Description("Background reference colors")]
        [JsonConverter(typeof(ColorListJsonConverter))]
        public IReadOnlyList<int> BackgroundColors { get; init; } = new[] { ColorIntExt.FromRGB(255, 0, 255) };

        /// <summary>
        /// Maximum squared RGB distance from a background color for a pixel to be considered background.
        /// </summary>
        [Category("Threshold"), Description("Maximum squared RGB distance from a background color")]
        public double Tolerance { get; init; } = 0.0;

        /// <summary>
        /// Swaps foreground and background.
        /// </summary>
        [Category("Threshold"), Description("Swaps foreground and background")]
        public bool InvertResults { get; init; } = false;

        /// <summary>
        /// Returns the validation errors of the settings (empty when valid).
        /// </summary>
        public IEnumerable<string> Validate()
        {
            if (BackgroundColors == null || BackgroundColors.Count == 0)
                yield return $"{nameof(BackgroundColors)} must contain at least one color";
            if (double.IsNaN(Tolerance) || Tolerance < 0)
                yield return $"{nameof(Tolerance)} must not be negative (is {Tolerance})";
        }
    }
}
