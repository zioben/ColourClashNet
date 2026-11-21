using System.ComponentModel;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Options of <see cref="ColorTransformFast"/>: keeps the most frequent colors.
    /// </summary>
    public sealed record FastOptions : ColorReductionOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionFast;
    }

    /// <summary>
    /// Options of <see cref="ColorTransformMedianCut"/>.
    /// </summary>
    public sealed record MedianCutOptions : ColorReductionOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionMedianCut;

        /// <summary>
        /// How the representative color of each median cut box is selected.
        /// </summary>
        [Category("Median Cut"), Description("Representative color of each box")]
        public ColorSelectionMode Selection { get; init; } = ColorSelectionMode.EvaluateColorMean;

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            if (MaxColors > 256)
                yield return $"{nameof(MaxColors)} must be at most 256 (is {MaxColors})";
            if (!Enum.IsDefined(Selection))
                yield return $"{nameof(Selection)} {Selection} is not valid";
        }
    }

    /// <summary>
    /// Options of <see cref="ColorTransformClustering"/> (K-means).
    /// </summary>
    public sealed record ClusteringOptions : ColorReductionOptions
    {
        /// <inheritdoc/>
        public override ColorTransformType TransformType => ColorTransformType.ColorReductionClustering;

        /// <summary>
        /// Number of clustering iterations.
        /// </summary>
        [Category("Clustering"), Description("Number of clustering iterations")]
        public int TrainingLoops { get; init; } = 10;

        /// <summary>
        /// How the color of each cluster is recomputed.
        /// </summary>
        [Category("Clustering"), Description("How the color of each cluster is recomputed")]
        public ColorSelectionMode Selection { get; init; } = ColorSelectionMode.EvaluateColorMean;

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            foreach (var error in base.Validate())
                yield return error;
            if (TrainingLoops < 1)
                yield return $"{nameof(TrainingLoops)} must be at least 1 (is {TrainingLoops})";
            if (!Enum.IsDefined(Selection))
                yield return $"{nameof(Selection)} {Selection} is not valid";
        }
    }
}
