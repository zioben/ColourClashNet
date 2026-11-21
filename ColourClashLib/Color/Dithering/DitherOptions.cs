using System.ComponentModel;

namespace ColourClashNet.Color.Dithering
{
    /// <summary>
    /// Immutable dithering settings.
    /// <para/>
    /// Copy with a <c>with</c> expression to change a value: instances are shared freely between transformations.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed record DitherOptions
    {
        /// <summary>Dithering algorithm.</summary>
        public ColorDithering DitheringType { get; init; } = ColorDithering.None;

        /// <summary>Dithering strength multiplier in [0, 1] (1.0 = nominal).</summary>
        public double DitheringStrength { get; init; } = 1.0;

        /// <summary>Optional post-dithering effect.</summary>
        public ColorDitheringFx DitheringFx { get; init; } = ColorDitheringFx.None;

        /// <summary>No dithering.</summary>
        public static DitherOptions None { get; } = new();

        /// <summary>
        /// Returns the validation errors of the settings (empty when valid).
        /// </summary>
        public IEnumerable<string> Validate()
        {
            if (!Enum.IsDefined(DitheringType))
                yield return $"{nameof(DitheringType)} {DitheringType} is not a valid dithering";
            if (double.IsNaN(DitheringStrength) || DitheringStrength < 0 || DitheringStrength > 1)
                yield return $"{nameof(DitheringStrength)} must be in [0, 1] (is {DitheringStrength})";
            if (!Enum.IsDefined(DitheringFx))
                yield return $"{nameof(DitheringFx)} {DitheringFx} is not a valid effect";
        }

        /// <inheritdoc/>
        public override string ToString()
            => $"{DitheringType} ({DitheringStrength:0.##}){(DitheringFx != ColorDitheringFx.None ? $" + {DitheringFx}" : "")}";
    }
}
