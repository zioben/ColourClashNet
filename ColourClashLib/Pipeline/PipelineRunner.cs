using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ColourClashNet.Pipeline
{
    /// <summary>
    /// Outcome of one pipeline step.
    /// </summary>
    public sealed class PipelineStepResult
    {
        /// <summary>Step position, from 1.</summary>
        public int Number { get; init; }
        /// <summary>The step.</summary>
        public PipelineStep Step { get; init; } = null!;
        /// <summary>True when the step completed.</summary>
        public bool IsSuccess { get; init; }
        /// <summary>Result or error message.</summary>
        public string Message { get; init; } = string.Empty;
        /// <summary>Duration, in milliseconds.</summary>
        public double ProcessingTimeMilliseconds { get; init; }
        /// <summary>Mean color error of a transformation step; NaN for the other steps.</summary>
        public double TransformationError { get; init; } = double.NaN;
    }

    /// <summary>
    /// Outcome of a pipeline run.
    /// </summary>
    public sealed class PipelineResult
    {
        /// <summary>True when every step completed.</summary>
        public bool IsSuccess { get; init; }
        /// <summary>Summary, or the error of the failed step.</summary>
        public string Message { get; init; } = string.Empty;
        /// <summary>Image after the last completed step.</summary>
        public ImageData? Image { get; init; }
        /// <summary>Results of the steps that were run.</summary>
        public IReadOnlyList<PipelineStepResult> Steps { get; init; } = Array.Empty<PipelineStepResult>();
        /// <summary>Exception that stopped the run, if any.</summary>
        public Exception? Exception { get; init; }
        /// <summary>Duration of the run, in milliseconds.</summary>
        public double ProcessingTimeMilliseconds { get; init; }
    }

    /// <summary>
    /// Event data of the step events of <see cref="PipelineRunner"/>.
    /// </summary>
    public sealed class PipelineStepEventArgs : EventArgs
    {
        /// <summary>Step position, from 1.</summary>
        public int Number { get; init; }
        /// <summary>Number of steps.</summary>
        public int Count { get; init; }
        /// <summary>The step.</summary>
        public PipelineStep Step { get; init; } = null!;
        /// <summary>Result of the step; null in <see cref="PipelineRunner.StepStarted"/>.</summary>
        public PipelineStepResult? Result { get; init; }
    }

    /// <summary>
    /// Runs a <see cref="PipelineDefinition"/>: each step works on the image produced by the previous one.
    /// <para/>
    /// The run stops at the first failing step; errors and cancellation are returned in the result, never thrown.
    /// </summary>
    public sealed class PipelineRunner
    {
        static readonly string sC = nameof(PipelineRunner);

        /// <summary>
        /// Folder used to resolve the relative paths of the load and save steps (usually the folder of the pipeline file).
        /// The current folder when empty.
        /// </summary>
        public string BaseDirectory { get; init; } = string.Empty;

        /// <summary>
        /// Raised before each step.
        /// </summary>
        public event EventHandler<PipelineStepEventArgs>? StepStarted;

        /// <summary>
        /// Raised after each step, also when it fails.
        /// </summary>
        public event EventHandler<PipelineStepEventArgs>? StepCompleted;

        /// <summary>
        /// Progress of the transformation steps (forwarded <see cref="IColorTransform.Progress"/> events).
        /// </summary>
        public event EventHandler<ColorProcessingEventArgs>? Progress;

        /// <summary>
        /// Runs <paramref name="pipeline"/>.
        /// </summary>
        /// <param name="pipeline">Pipeline to run</param>
        /// <param name="input">Starting image, used until a load step replaces it (optional when the pipeline starts with a load step)</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The result, with the last image and the result of every step.</returns>
        public PipelineResult Run(PipelineDefinition pipeline, ImageData? input = null, CancellationToken token = default)
        {
            string sM = nameof(Run);
            var chrono = Stopwatch.StartNew();
            var stepResults = new List<PipelineStepResult>();
            var image = input;

            var errors = pipeline?.Validate().ToList() ?? new List<string> { "pipeline is missing" };
            if (errors.Count > 0)
            {
                var message = $"invalid pipeline : {string.Join("; ", errors)}";
                LogMan.Error(sC, sM, message);
                return Fail(message, null);
            }

            LogMan.Message(sC, sM, $"pipeline '{pipeline!.Name}' started : {pipeline.Steps.Count} steps");
            try
            {
                for (int i = 0; i < pipeline.Steps.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var step = pipeline.Steps[i];
                    StepStarted?.Invoke(this, new PipelineStepEventArgs { Number = i + 1, Count = pipeline.Steps.Count, Step = step });
                    LogMan.Debug(sC, sM, $"step {i + 1}/{pipeline.Steps.Count} : {step}");

                    var stepChrono = Stopwatch.StartNew();
                    var (nextImage, stepMessage, error) = RunStep(step, image, token);
                    var stepResult = new PipelineStepResult
                    {
                        Number = i + 1,
                        Step = step,
                        IsSuccess = nextImage != null,
                        Message = stepMessage,
                        ProcessingTimeMilliseconds = stepChrono.Elapsed.TotalMilliseconds,
                        TransformationError = error,
                    };
                    stepResults.Add(stepResult);
                    StepCompleted?.Invoke(this, new PipelineStepEventArgs { Number = i + 1, Count = pipeline.Steps.Count, Step = step, Result = stepResult });

                    if (nextImage == null)
                    {
                        var message = $"step {i + 1} ({step}) failed : {stepMessage}";
                        LogMan.Error(sC, sM, message);
                        return Fail(message, null);
                    }
                    image = nextImage;
                }
            }
            catch (OperationCanceledException ex)
            {
                LogMan.Warning(sC, sM, $"pipeline '{pipeline.Name}' cancelled");
                return Fail("pipeline cancelled", ex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"pipeline '{pipeline.Name}' failed", ex);
                return Fail($"pipeline failed : {ex.Message}", ex);
            }

            LogMan.Message(sC, sM, $"pipeline '{pipeline.Name}' completed in {chrono.Elapsed.TotalMilliseconds:f0} ms : {image!.Width}x{image.Height}, {image.Colors} colors");
            return new PipelineResult
            {
                IsSuccess = true,
                Message = $"{stepResults.Count} steps completed",
                Image = image,
                Steps = stepResults,
                ProcessingTimeMilliseconds = chrono.Elapsed.TotalMilliseconds,
            };

            PipelineResult Fail(string message, Exception? ex) => new()
            {
                IsSuccess = false,
                Message = message,
                Image = image,
                Steps = stepResults,
                Exception = ex,
                ProcessingTimeMilliseconds = chrono.Elapsed.TotalMilliseconds,
            };
        }

        /// <summary>
        /// Runs <see cref="Run"/> on a worker thread.
        /// </summary>
        public Task<PipelineResult> RunAsync(PipelineDefinition pipeline, ImageData? input = null, CancellationToken token = default)
            => Task.Run(() => Run(pipeline, input, token), token);

        /// <summary>
        /// Runs one step.
        /// </summary>
        /// <returns>The new image (null on failure), a message and the transformation error (NaN when not applicable).</returns>
        (ImageData? image, string message, double error) RunStep(PipelineStep step, ImageData? image, CancellationToken token)
        {
            if (step is LoadStep load)
            {
                var path = ResolvePath(load.File);
                var loaded = File.Exists(path) ? ImageTools.LoadImage(path) : null;
                return loaded == null
                    ? (null, $"cannot load {path}", double.NaN)
                    : (loaded, $"loaded {path} : {loaded.Width}x{loaded.Height}, {loaded.Colors} colors", double.NaN);
            }

            if (image == null)
                return (null, "there is no image yet: add a load step or give an input image", double.NaN);

            switch (step)
            {
                case CropStep crop:
                    {
                        var cropped = image.Extract(crop.X, crop.Y, crop.Width, crop.Height);
                        return (cropped, $"cropped to {crop.Width}x{crop.Height} at ({crop.X},{crop.Y})", double.NaN);
                    }
                case ResizeStep resize:
                    {
                        // A zero size keeps the aspect ratio from the other one
                        int width = resize.Width > 0 ? resize.Width : Math.Max(1, (int)Math.Round((double)image.Width * resize.Height / image.Height));
                        int height = resize.Height > 0 ? resize.Height : Math.Max(1, (int)Math.Round((double)image.Height * resize.Width / image.Width));
                        var resized = ImageTools.Resize(image, width, height, resize.Mode == ResizeMode.Average);
                        return (resized, $"resized to {width}x{height} ({resize.Mode})", double.NaN);
                    }
                case TransformStep transformStep:
                    {
                        using var transform = ColorTransformBase.CreateTransform(transformStep.Options!);
                        transform.Progress += (s, e) => Progress?.Invoke(this, e);
                        var result = transform.Process(image, null, token);
                        token.ThrowIfCancellationRequested();
                        return result.IsSuccess
                            ? (result.DataOut, $"{transform.Type} : {result.DataOut!.Colors} colors, error {result.TransformationError:g4}", result.TransformationError)
                            : (null, result.Message, double.NaN);
                    }
                case SaveStep save:
                    {
                        var path = ResolvePath(save.File);
                        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
                        if (!string.IsNullOrEmpty(directory))
                            Directory.CreateDirectory(directory);
                        bool saved = save.Format switch
                        {
                            PipelineSaveFormat.IndexedBmp => ImageTools.SaveImageAsIndexedBitmap(image, path),
                            PipelineSaveFormat.Auto => ImageTools.SaveImage(image, path, FormatFromExtension(path), save.Quality),
                            PipelineSaveFormat.Bmp => ImageTools.SaveImage(image, path, ImageSaveFormat.Bmp, save.Quality),
                            PipelineSaveFormat.Jpg => ImageTools.SaveImage(image, path, ImageSaveFormat.Jpg, save.Quality),
                            _ => ImageTools.SaveImage(image, path, ImageSaveFormat.Png, save.Quality),
                        };
                        return saved ? (image, $"saved {path}", double.NaN) : (null, $"cannot save {path}", double.NaN);
                    }
                default:
                    return (null, $"step {step.GetType().Name} is not supported", double.NaN);
            }
        }

        /// <summary>
        /// Resolves a relative path against <see cref="BaseDirectory"/>.
        /// </summary>
        string ResolvePath(string fileName)
            => Path.IsPathRooted(fileName) || string.IsNullOrEmpty(BaseDirectory) ? fileName : Path.Combine(BaseDirectory, fileName);

        /// <summary>
        /// File format from the extension; PNG when unknown.
        /// </summary>
        static ImageSaveFormat FormatFromExtension(string fileName)
            => Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".bmp" => ImageSaveFormat.Bmp,
                ".jpg" or ".jpeg" => ImageSaveFormat.Jpg,
                _ => ImageSaveFormat.Png,
            };
    }
}
