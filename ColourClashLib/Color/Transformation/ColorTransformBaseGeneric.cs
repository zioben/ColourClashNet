namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Base class of the transformations, typed on their option record.
    /// <para/>
    /// <see cref="Options"/> is the single source of the settings: configure a transformation with
    /// <c>new ColorTransformClustering { Options = new ClusteringOptions { MaxColors = 16 } }</c>
    /// or change a value with <c>transform.Options = transform.Options with { MaxColors = 8 }</c>.
    /// </summary>
    /// <typeparam name="TOptions">Option record of the transformation</typeparam>
    public abstract class ColorTransformBase<TOptions> : ColorTransformBase where TOptions : TransformOptions, new()
    {
        TOptions options = new();

        /// <summary>
        /// Current options of the transformation.
        /// </summary>
        /// <exception cref="ArgumentNullException">Set to null.</exception>
        public new TOptions Options
        {
            get => options;
            set => options = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        protected override TransformOptions GetOptions() => options;

        /// <inheritdoc/>
        protected override void SetOptionsCore(TransformOptions newOptions)
        {
            options = newOptions as TOptions
                ?? throw new ArgumentException($"{GetType().Name} expects {typeof(TOptions).Name}, got {newOptions.GetType().Name}", nameof(newOptions));
        }
    }
}
