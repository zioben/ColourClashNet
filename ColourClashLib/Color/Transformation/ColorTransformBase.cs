using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Tile;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Base class of all color transformations.
    /// <para/>
    /// <see cref="Process"/> runs the same pipeline for every transformation, as a sequence of short steps:
    /// load the images, validate the options, <see cref="ExecuteTransform"/> (the single hook of the derived classes),
    /// dithering against <see cref="ImageReference"/>, error evaluation. Transformations based on a color map
    /// derive from <see cref="ColorTransformMapBase{TOptions}"/> and only build the map.
    /// <para/>
    /// An instance is not thread safe: it processes one image at a time. Run concurrent work on separate instances
    /// (the pipeline and the nested transformations always create their own).
    /// </summary>
    public abstract partial class ColorTransformBase : IColorTransform
    {
        static string sC = nameof(ColorTransformBase);

        #region Events

        /// <inheritdoc/>
        public event EventHandler<ColorProcessingEventArgs>? StartProcessing;
        /// <inheritdoc/>
        public event EventHandler<ColorProcessingEventArgs>? Creating;
        /// <inheritdoc/>
        public event EventHandler<ColorProcessingEventArgs>? Created;
        /// <inheritdoc/>
        public event EventHandler<ColorProcessingEventArgs>? Processing;
        /// <inheritdoc/>
        public event EventHandler<ColorProcessingEventArgs>? Progress;
        /// <inheritdoc/>
        public event EventHandler<ColorProcessingEventArgs>? Dithering;
        /// <inheritdoc/>
        public event EventHandler<ColorProcessingEventArgs>? Completed;
        /// <inheritdoc/>
        public event EventHandler? Destroying;
        /// <inheritdoc/>
        public event EventHandler? Destroyed;

        #endregion

        #region Options

        /// <summary>
        /// Returns the current options (typed in <see cref="ColorTransformBase{TOptions}"/>).
        /// </summary>
        protected abstract TransformOptions GetOptions();

        /// <summary>
        /// Replaces the current options.
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="options"/> is not of the type expected by the transformation.</exception>
        protected abstract void SetOptionsCore(TransformOptions options);

        /// <inheritdoc/>
        public TransformOptions Options => GetOptions();

        /// <inheritdoc/>
        public IColorTransform SetOptions(TransformOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            SetOptionsCore(options);
            return this;
        }

        /// <summary>
        /// Colors chosen by the user (<see cref="TransformOptions.UserPalette"/>) as a <see cref="Palette"/>.
        /// </summary>
        protected Palette UserPalette => new Palette().Create(GetOptions().UserPalette ?? Array.Empty<int>());

        /// <summary>
        /// Dithering settings of the current options.
        /// </summary>
        protected DitherOptions DitheringOptions => GetOptions().Dithering ?? DitherOptions.None;

        /// <inheritdoc/>
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode => GetOptions().DistanceMode;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public ColorTransformType Type => GetOptions()?.TransformType ?? ColorTransformType.None;

        /// <inheritdoc/>
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string Description { get; protected init; } = "";

        /// <inheritdoc/>
        public ImageData ImageReference { get; private set; } = new ImageData();
        /// <inheritdoc/>
        public ImageData ImageSource { get; private set; } = new ImageData();
        /// <inheritdoc/>
        public ImageData ImageOutput { get; private set; } = new ImageData();

        /// <inheritdoc/>
        public ColorTransformationMap TransformationMap { get; protected set; } = new ColorTransformationMap();

        /// <inheritdoc/>
        public double TransformationError { get; private set; } = double.NaN;

        /// <inheritdoc/>
        public double ProcessingTimeMilliseconds => chrono.ElapsedMilliseconds;

        /// <inheritdoc/>
        /// <remarks>
        /// Reset to false at the start of every run; a derived class sets it inside <see cref="ExecuteTransform"/>
        /// when it dithers by itself or when dithering must be disabled in its current mode.
        /// </remarks>
        public bool BypassDithering { get; protected set; }

        /// <summary>
        /// Enables the intermediate images in the <see cref="Progress"/> events (disabled for the tiles: too costly).
        /// </summary>
        public bool EnablePreviewEvents { get; set; } = true;

        /// <summary>
        /// Enables the debug and trace logs of the processing steps (errors and exceptions are always logged).
        /// <para/>
        /// Disabled for the transformations of the tiles: hundreds of tiles would flood the log.
        /// Nested transformations attached with <see cref="ForwardEventsFrom{T}(T)"/> inherit the value.
        /// </summary>
        public bool LogProcessingSteps { get; set; } = true;

        readonly StopWatch chrono = new StopWatch();
        bool disposed;

        #endregion

        #region Logging helpers

        /// <summary>
        /// Logs a debug message of a processing step, when <see cref="LogProcessingSteps"/> is enabled.
        /// </summary>
        protected void LogDebug(string sClass, string sMethod, string message)
        {
            if (LogProcessingSteps)
                LogMan.Debug(sClass, sMethod, message);
        }

        /// <summary>
        /// Logs a trace (verbose) message of a processing step, when <see cref="LogProcessingSteps"/> is enabled.
        /// </summary>
        protected void LogTrace(string sClass, string sMethod, string message)
        {
            if (LogProcessingSteps)
                LogMan.Trace(sClass, sMethod, message);
        }

        /// <summary>
        /// True when trace messages of the processing steps are logged: use it to skip building expensive messages.
        /// </summary>
        protected bool IsTraceLogged => LogProcessingSteps && LogMan.IsTraceEnabled;

        #endregion

        #region Hook

        /// <summary>
        /// The single hook of the derived classes: produces the color-reduced image from <see cref="ImageSource"/>.
        /// <para/>
        /// Set <see cref="BypassDithering"/> here when the final dithering must be skipped.
        /// Report progress with <see cref="RaiseProgressEvent"/>.
        /// </summary>
        /// <param name="token">Cancellation token of the run</param>
        /// <returns>A valid result with the reduced image in <see cref="ColorTransformResult.DataOut"/>, or an error result.</returns>
        protected abstract ColorTransformResult ExecuteTransform(CancellationToken token);

        #endregion

        #region Events helpers

        /// <summary>
        /// Builds the event data of this transformation.
        /// </summary>
        internal ColorProcessingEventArgs CreateTransformEventArgs(CancellationTokenSource? tokenSource, ColorTransformResult? result = null)
            => new ColorProcessingEventArgs()
            {
                Transformation = this,
                CancellationTokenSource = tokenSource,
                ProcessingResults = result,
                TransformationMap = TransformationMap,
            };

        /// <summary>
        /// Raises <paramref name="handler"/>; a failing subscriber is logged and never breaks the processing.
        /// </summary>
        void RaiseEvent(EventHandler<ColorProcessingEventArgs>? handler, string eventName, ColorProcessingEventArgs args)
        {
            if (handler == null)
                return;
            try
            {
                handler.Invoke(this, args);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, nameof(RaiseEvent), $"{Type} : error in a {eventName} event handler", ex);
            }
        }

        /// <summary>
        /// Raises <see cref="Progress"/>.
        /// </summary>
        protected void RaiseProgressEvent(ColorProcessingEventArgs args) => Progress?.Invoke(this, args);

        /// <summary>
        /// Raises <see cref="Progress"/> on behalf of a nested step (child transformation or tile manager).
        /// <para/>
        /// The event data is copied and re-issued by this instance, so subscribers of the outermost transformation
        /// receive the progress of every nested level. Nothing is allocated when nobody is listening.
        /// </summary>
        /// <param name="sender">Nested step that raised the event</param>
        /// <param name="args">Event data of the nested step</param>
        protected void ForwardNestedEvent(object? sender, ColorProcessingEventArgs args)
        {
            if (Progress == null || args == null)
                return;
            RaiseProgressEvent(new ColorProcessingEventArgs()
            {
                Transformation = this,
                ProcessingResults = args.ProcessingResults,
                CancellationTokenSource = args.CancellationTokenSource,
                TransformationMap = args.TransformationMap,
                CompletedPercent = args.CompletedPercent,
            });
        }

        /// <summary>
        /// Subscribes to the <see cref="Progress"/> event of a nested transformation and forwards it
        /// through <see cref="ForwardNestedEvent"/>; the nested transformation also inherits <see cref="LogProcessingSteps"/>.
        /// </summary>
        /// <typeparam name="T">Nested transformation type</typeparam>
        /// <param name="child">Nested transformation</param>
        /// <returns><paramref name="child"/>, to allow fluent use.</returns>
        protected T ForwardEventsFrom<T>(T child) where T : IColorTransform
        {
            if (child != null)
            {
                child.Progress += ForwardNestedEvent;
                if (child is ColorTransformBase childBase)
                    childBase.LogProcessingSteps = LogProcessingSteps;
            }
            return child;
        }

        /// <summary>
        /// Subscribes to the progress event of a <see cref="TileManager"/> and forwards it through <see cref="ForwardNestedEvent"/>.
        /// </summary>
        /// <param name="tileManager">Tile manager used by this transformation</param>
        /// <returns><paramref name="tileManager"/>, to allow fluent use.</returns>
        protected TileManager ForwardEventsFrom(TileManager tileManager)
        {
            if (tileManager != null)
            {
                tileManager.Progress += ForwardNestedEvent;
            }
            return tileManager;
        }

        #endregion

        #region Processing

        /// <inheritdoc/>
        public ColorTransformResult Process(ImageData sourceImage, ImageData? referenceImage = null, CancellationToken token = default)
        {
            string sM = nameof(Process);
            ObjectDisposedException.ThrowIf(disposed, this);

            chrono.Start();
            TransformationError = double.NaN;
            BypassDithering = false;
            // The token source is published in the events: cancelling it (e.g. an abort button) stops this run.
            // It is not disposed here, because subscribers may keep it after the run.
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var runToken = cts.Token;

            ColorTransformResult result;
            try
            {
                // The handler may change the options: they are read only after this event
                RaiseEvent(StartProcessing, nameof(StartProcessing), CreateTransformEventArgs(cts));
                runToken.ThrowIfCancellationRequested();

                result = LoadImages(sourceImage, referenceImage, cts)
                    ?? ValidateOptions()
                    ?? Transform(cts, runToken);
            }
            catch (OperationCanceledException ex)
            {
                LogMan.Warning(sC, sM, $"{Type} : processing cancelled after {chrono.ElapsedMilliseconds:f0} ms");
                result = ColorTransformResult.CreateErrorResult(sC, sM, sourceImage, null, $"{Type} : processing cancelled", ex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : processing failed", ex);
                result = ColorTransformResult.CreateErrorResult(sC, sM, sourceImage, null, $"{Type} : processing failed", ex);
            }

            chrono.Update();
            result.TransformationError = TransformationError;
            result.ProcessingTimeMilliseconds = chrono.ElapsedMilliseconds;
            if (result.IsSuccess)
                LogDebug(sC, sM, $"{Type} : processing completed in {chrono.ElapsedMilliseconds:f0} ms : {ImageOutput.Colors} colors, error {TransformationError:g4}");
            else
                LogDebug(sC, sM, $"{Type} : processing ended with errors in {chrono.ElapsedMilliseconds:f0} ms");

            RaiseEvent(Completed, nameof(Completed), CreateTransformEventArgs(cts, result));
            return result;
        }

        /// <inheritdoc/>
        public Task<ColorTransformResult> ProcessAsync(ImageData sourceImage, ImageData? referenceImage = null, CancellationToken token = default)
            => Task.Run(() => Process(sourceImage, referenceImage, token), token);

        /// <summary>
        /// Step 1: loads the source and reference images, between the <see cref="Creating"/> and <see cref="Created"/> events.
        /// </summary>
        /// <returns>Null on success, otherwise the error result.</returns>
        ColorTransformResult? LoadImages(ImageData sourceImage, ImageData? referenceImage, CancellationTokenSource cts)
        {
            string sM = nameof(LoadImages);
            if (sourceImage?.matrix == null || (referenceImage != null && referenceImage.matrix == null))
            {
                LogMan.Error(sC, sM, $"{Type} : invalid {(sourceImage?.matrix == null ? "source" : "reference")} image");
                return ColorTransformResult.CreateErrorResult(sC, sM, sourceImage, null, $"{Type} : invalid {(sourceImage?.matrix == null ? "source" : "reference")} image");
            }

            RaiseEvent(Creating, nameof(Creating), CreateTransformEventArgs(cts));
            LogTrace(sC, sM, $"{Type} : source {sourceImage}, reference {(referenceImage == null ? "same as source" : referenceImage.ToString())}");
            ImageSource = new ImageData().Create(sourceImage);
            ImageReference = referenceImage == null ? ImageSource : new ImageData().Create(referenceImage);
            ImageOutput = new ImageData();
            TransformationMap.Reset();
            RaiseEvent(Created, nameof(Created), CreateTransformEventArgs(cts));
            return null;
        }

        /// <summary>
        /// Step 2: validates the options of the run.
        /// </summary>
        /// <returns>Null when valid, otherwise the error result listing every problem.</returns>
        ColorTransformResult? ValidateOptions()
        {
            string sM = nameof(ValidateOptions);
            var errors = GetOptions()?.Validate().ToList() ?? new List<string> { "options are missing" };
            if (errors.Count == 0)
                return null;
            var message = $"{Type} : invalid options : {string.Join("; ", errors)}";
            LogMan.Error(sC, sM, message);
            return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, message);
        }

        /// <summary>
        /// Steps 3-5: <see cref="Processing"/> event, <see cref="ExecuteTransform"/>, dithering and error evaluation.
        /// </summary>
        ColorTransformResult Transform(CancellationTokenSource cts, CancellationToken token)
        {
            string sM = nameof(Transform);
            LogDebug(sC, sM, $"{Type} : processing started : {ImageSource.Columns}x{ImageSource.Rows}, {ImageSource.Colors} colors, metric {ColorDistanceEvaluationMode}, dithering {DitheringOptions.DitheringType}");
            if (IsTraceLogged)
                LogMan.Trace(sC, sM, $"{Type} : options {GetOptions()}");
            RaiseEvent(Processing, nameof(Processing), CreateTransformEventArgs(cts));

            var transformResult = ExecuteTransform(token);
            if (transformResult == null || !transformResult.IsSuccess || transformResult.DataOut == null)
            {
                // Propagate the nested error (message, output and exception)
                LogMan.Error(sC, sM, $"{Type} : {nameof(ExecuteTransform)} failed : {transformResult?.Message}");
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : {nameof(ExecuteTransform)} failed", transformResult);
            }
            LogTrace(sC, sM, $"{Type} : reduced image {transformResult.DataOut.Colors} colors{(TransformationMap.Colors > 0 ? $", map {TransformationMap.Colors} colors" : "")} ({chrono.ElapsedMilliseconds:f0} ms)");

            var ditherResult = ApplyDithering(transformResult, cts, token);
            if (!ditherResult.IsSuccess)
                return ditherResult;

            ImageOutput = ditherResult.DataOut!;
            TransformationError = ColorIntExt.EvaluateError(ImageSource, ImageOutput, ColorDistanceEvaluationMode, token);
            return ColorTransformResult.CreateValidResult(ImageSource, ImageOutput);
        }

        /// <summary>
        /// Step 4: dithers the reduced image against <see cref="ImageReference"/>, raising <see cref="Dithering"/> first.
        /// <para/>
        /// Skipped when <see cref="BypassDithering"/> is set, when dithering is disabled, or when the reduced image
        /// still has more than 256 colors.
        /// </summary>
        /// <returns>A valid result with the final image, or the dithering error result.</returns>
        ColorTransformResult ApplyDithering(ColorTransformResult transformResult, CancellationTokenSource cts, CancellationToken token)
        {
            string sM = nameof(ApplyDithering);
            var reducedImage = transformResult.DataOut!;
            if (BypassDithering || DitheringOptions.DitheringType == ColorDithering.None)
                return ColorTransformResult.CreateValidResult(ImageSource, reducedImage);

            // Dithering needs a small output palette: count the real colors of the reduced image
            int colors = reducedImage.ColorPalette.ToList().Count(rgb => rgb.IsColor());
            if (colors > 256)
            {
                LogMan.Warning(sC, sM, $"{Type} : {DitheringOptions.DitheringType} skipped : too many colors to dither ({colors})");
                return ColorTransformResult.CreateValidResult(ImageSource, reducedImage);
            }

            RaiseEvent(Dithering, nameof(Dithering), CreateTransformEventArgs(cts, transformResult));
            LogDebug(sC, sM, $"{Type} : dithering {DitheringOptions.DitheringType} (strength {DitheringOptions.DitheringStrength}) on {colors} colors");
            var dithering = DitherBase.CreateDitherInterface(DitheringOptions);
            var ditherResult = dithering.Dither(ImageReference, reducedImage, ColorDistanceEvaluationMode, token);
            if (ditherResult == null || !ditherResult.IsSuccess || ditherResult.DataOut == null)
            {
                LogMan.Error(sC, sM, $"{Type} : dithering {DitheringOptions.DitheringType} failed : {ditherResult?.Message}");
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, reducedImage, $"{Type} : dithering error : {ditherResult?.Message}", ditherResult?.Exception);
            }
            return ColorTransformResult.CreateValidResult(ImageSource, ditherResult.DataOut);
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Raises <see cref="Destroying"/>, releases the images and detaches every subscriber, then raises <see cref="Destroyed"/>.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;
            Destroying?.Invoke(this, EventArgs.Empty);
            Dispose(true);
            disposed = true;
            var destroyed = Destroyed;
            StartProcessing = null;
            Creating = null;
            Created = null;
            Processing = null;
            Progress = null;
            Dithering = null;
            Completed = null;
            Destroying = null;
            Destroyed = null;
            destroyed?.Invoke(this, EventArgs.Empty);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the resources of the transformation; override it to release those of a derived class.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ImageSource = new ImageData();
                ImageReference = ImageSource;
                ImageOutput = new ImageData();
                TransformationMap.Reset();
            }
        }

        #endregion
    }
}
