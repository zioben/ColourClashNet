using ColourClashNet.Imaging;
using ColourClashNet.Log;

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

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Base class of the transformations based on a color map: every source color is replaced by the color
    /// chosen for it in <see cref="ColorTransformBase.TransformationMap"/>.
    /// <para/>
    /// Derived classes only build the map in <see cref="CreateTransformationMap"/>; applying it is done here.
    /// A derived class may still override <see cref="ExecuteTransform"/> (e.g. for special video modes) and
    /// call <c>base.ExecuteTransform</c> for the plain map.
    /// </summary>
    /// <typeparam name="TOptions">Option record of the transformation</typeparam>
    public abstract class ColorTransformMapBase<TOptions> : ColorTransformBase<TOptions> where TOptions : TransformOptions, new()
    {
        static readonly string sC = nameof(ColorTransformMapBase<TOptions>);

        /// <summary>
        /// Builds <see cref="ColorTransformBase.TransformationMap"/> from <see cref="ColorTransformBase.ImageSource"/>.
        /// </summary>
        /// <returns>A valid result, or an error result.</returns>
        protected abstract ColorTransformResult CreateTransformationMap(CancellationToken token);

        /// <summary>
        /// Builds the map, then applies it to the source image.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token)
        {
            string sM = nameof(ExecuteTransform);
            var mapResult = CreateTransformationMap(token);
            if (mapResult == null || !mapResult.IsSuccess)
            {
                LogMan.Error(sC, sM, $"{Type} : {nameof(CreateTransformationMap)} failed : {mapResult?.Message}");
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : {nameof(CreateTransformationMap)} failed", mapResult);
            }
            return ApplyTransformationMap(token);
        }

        /// <summary>
        /// Applies <see cref="ColorTransformBase.TransformationMap"/> to <see cref="ColorTransformBase.ImageSource"/>.
        /// </summary>
        /// <returns>A valid result with the mapped image, or an error result.</returns>
        protected ColorTransformResult ApplyTransformationMap(CancellationToken token)
        {
            string sM = nameof(ApplyTransformationMap);
            try
            {
                LogTrace(sC, sM, $"{Type} : applying the transformation map ({TransformationMap.Colors} colors)");
                var processedImage = TransformationMap.Transform(ImageSource, token);
                ImageData.AssertValid(processedImage);
                return ColorTransformResult.CreateValidResult(ImageSource, processedImage);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : transformation map not applicable", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : transformation map not applicable", ex);
            }
        }
    }
}
