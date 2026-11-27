using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging.Shapes;
using ColourClashProcessing.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ColourClashNet.Pipeline
{
    /// <summary>
    /// Processing pipeline stored as JSON: an ordered list of steps (load, crop, resize, region of interest,
    /// transformations, blob extraction, save).
    /// <para/>
    /// Pipelines are immutable records, like the transformation options: change them with <c>with</c> expressions.
    /// Run them with <see cref="PipelineRunner"/>, save and load them with <see cref="PipelineSerializer"/>.
    /// </summary>
    public sealed record PipelineDefinition
    {
        /// <summary>
        /// Format version written by this program. Files with a higher version are refused.
        /// </summary>
        public const int CurrentVersion = 1;

        /// <summary>
        /// Format version of the file.
        /// </summary>
        public int Version { get; init; } = CurrentVersion;

        /// <summary>
        /// Pipeline name.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Free description.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Steps, run in order.
        /// </summary>
        public IReadOnlyList<PipelineStep> Steps { get; init; } = Array.Empty<PipelineStep>();

        /// <summary>
        /// Checks the version and every step.
        /// </summary>
        /// <returns>One message per problem; empty when the pipeline is valid.</returns>
        public IEnumerable<string> Validate()
        {
            if (Version < 1 || Version > CurrentVersion)
                yield return $"{nameof(Version)} {Version} is not supported (this program reads versions 1 to {CurrentVersion})";
            if (Steps == null || Steps.Count == 0)
            {
                yield return "the pipeline has no steps";
                yield break;
            }
            for (int i = 0; i < Steps.Count; i++)
            {
                var step = Steps[i];
                if (step == null)
                {
                    yield return $"step {i + 1} is missing";
                    continue;
                }
                foreach (var error in step.Validate())
                    yield return $"step {i + 1} ({step.Kind}) : {error}";
            }
        }

        /// <summary>
        /// Returns the pipeline reading <paramref name="fileName"/>: the first load step is changed,
        /// or a load step is added at the beginning when there is none.
        /// </summary>
        public PipelineDefinition WithInput(string fileName)
        {
            var steps = Steps.ToList();
            int index = steps.FindIndex(s => s is LoadStep);
            if (index >= 0)
                steps[index] = ((LoadStep)steps[index]) with { File = fileName };
            else
                steps.Insert(0, new LoadStep { File = fileName });
            return this with { Steps = steps };
        }

        /// <summary>
        /// Returns the pipeline writing <paramref name="fileName"/>: the last save step is changed,
        /// or a save step is added at the end when there is none.
        /// </summary>
        public PipelineDefinition WithOutput(string fileName)
        {
            var steps = Steps.ToList();
            int index = steps.FindLastIndex(s => s is SaveStep);
            if (index >= 0)
                steps[index] = ((SaveStep)steps[index]) with { File = fileName };
            else
                steps.Add(new SaveStep { File = fileName });
            return this with { Steps = steps };
        }
    }

    /// <summary>
    /// One step of a <see cref="PipelineDefinition"/>. The step type is stored in the <c>$type</c> JSON property.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(LoadStep), "Load")]
    [JsonDerivedType(typeof(CropStep), "Crop")]
    [JsonDerivedType(typeof(ResizeStep), "Resize")]
    [JsonDerivedType(typeof(RoiStep), "Roi")]
    [JsonDerivedType(typeof(TransformStep), "Transform")]
    [JsonDerivedType(typeof(BlobStep), "Blob")]
    [JsonDerivedType(typeof(SaveStep), "Save")]
    public abstract record PipelineStep
    {
        /// <summary>
        /// Optional label, shown in the logs and in the progress messages.
        /// </summary>
        public string Label { get; init; } = string.Empty;

        /// <summary>
        /// Short description of the step kind (e.g. <c>Transform ColorReductionCBM64</c>).
        /// </summary>
        [JsonIgnore]
        public abstract string Kind { get; }

        /// <summary>
        /// Checks the step settings.
        /// </summary>
        /// <returns>One message per problem; empty when the step is valid.</returns>
        public abstract IEnumerable<string> Validate();

        /// <summary>
        /// Text used in logs and messages: <see cref="Kind"/> and the label, when set.
        /// <para/>
        /// Sealed: otherwise every derived record would generate its own long ToString with all the properties.
        /// </summary>
        public sealed override string ToString() => string.IsNullOrWhiteSpace(Label) ? Kind : $"{Kind} '{Label}'";
    }

    /// <summary>
    /// Loads an image file (PNG, BMP, JPG ...); it replaces the current image.
    /// </summary>
    public sealed record LoadStep : PipelineStep
    {
        /// <summary>
        /// Image file; a relative path is relative to the folder of the pipeline file.
        /// </summary>
        public string File { get; init; } = string.Empty;

        /// <inheritdoc/>
        public override string Kind => "Load";

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            if (string.IsNullOrWhiteSpace(File))
                yield return $"{nameof(File)} is missing";
        }
    }

    /// <summary>
    /// Keeps a rectangular area of the current image; the parts outside the image are black.
    /// </summary>
    public sealed record CropStep : PipelineStep
    {
        /// <summary>Left coordinate.</summary>
        public int X { get; init; }
        /// <summary>Top coordinate.</summary>
        public int Y { get; init; }
        /// <summary>Width of the area.</summary>
        public int Width { get; init; }
        /// <summary>Height of the area.</summary>
        public int Height { get; init; }

        /// <inheritdoc/>
        public override string Kind => "Crop";

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            if (Width <= 0 || Height <= 0)
                yield return $"size {Width}x{Height} must be positive";
        }
    }

    /// <summary>
    /// How <see cref="ResizeStep"/> computes the pixels.
    /// </summary>
    public enum ResizeMode
    {
        /// <summary>Nearest source pixel: keeps the original colors (pixel art, enlargements).</summary>
        Nearest,
        /// <summary>Mean of the covered source pixels: smoother reductions of photos.</summary>
        Average,
    }

    /// <summary>
    /// Resizes the current image. A size of 0 keeps the aspect ratio from the other one.
    /// </summary>
    public sealed record ResizeStep : PipelineStep
    {
        /// <summary>New width; 0 to compute it from <see cref="Height"/> keeping the aspect ratio.</summary>
        public int Width { get; init; }
        /// <summary>New height; 0 to compute it from <see cref="Width"/> keeping the aspect ratio.</summary>
        public int Height { get; init; }
        /// <summary>Resampling mode.</summary>
        public ResizeMode Mode { get; init; } = ResizeMode.Average;

        /// <inheritdoc/>
        public override string Kind => "Resize";

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            if (Width < 0 || Height < 0)
                yield return $"size {Width}x{Height} must not be negative";
            if (Width == 0 && Height == 0)
                yield return "set at least one of Width and Height";
            if (!Enum.IsDefined(Mode))
                yield return $"{nameof(Mode)} {Mode} is not valid";
        }
    }

    /// <summary>
    /// Shapes of <see cref="RoiStep"/>.
    /// </summary>
    public enum RoiStepShape
    {
        /// <summary>Rectangle <c>X, Y, Width, Height</c>: every pixel is kept.</summary>
        Rectangle,
        /// <summary>Ellipse inscribed in <c>X, Y, Width, Height</c>.</summary>
        Ellipse,
        /// <summary>Polygon with the vertices <c>Points</c>.</summary>
        Polygon,
    }

    /// <summary>
    /// Vertex of a polygonal region of interest, in image coordinates.
    /// </summary>
    public sealed record RoiPoint(float X, float Y);

    /// <summary>
    /// Keeps a region of interest of the current image: the image becomes the bounding box of the shape, and the
    /// pixels outside the shape take <see cref="OutsideColor"/> (a rectangle keeps every pixel).
    /// </summary>
    public sealed record RoiStep : PipelineStep
    {
        /// <summary>Shape of the region.</summary>
        public RoiStepShape Shape { get; init; } = RoiStepShape.Rectangle;
        /// <summary>Left coordinate of the box (rectangle, ellipse).</summary>
        public int X { get; init; }
        /// <summary>Top coordinate of the box (rectangle, ellipse).</summary>
        public int Y { get; init; }
        /// <summary>Width of the box (rectangle, ellipse).</summary>
        public int Width { get; init; }
        /// <summary>Height of the box (rectangle, ellipse).</summary>
        public int Height { get; init; }
        /// <summary>Vertices of the polygon.</summary>
        public IReadOnlyList<RoiPoint> Points { get; init; } = Array.Empty<RoiPoint>();
        /// <summary>
        /// Color of the pixels outside the shape (default: the background color, so a background removal can drop them).
        /// </summary>
        [JsonConverter(typeof(ColorJsonConverter))]
        public int OutsideColor { get; init; } = ColorDefaults.DefaultBkgColorInt;

        /// <inheritdoc/>
        public override string Kind => $"Roi {Shape}";

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            if (!Enum.IsDefined(Shape))
                yield return $"{nameof(Shape)} {Shape} is not valid";
            if (Shape == RoiStepShape.Polygon)
            {
                if (Points == null || Points.Count < 3)
                    yield return $"a polygon needs at least 3 {nameof(Points)}";
            }
            else if (Width <= 0 || Height <= 0)
            {
                yield return $"size {Width}x{Height} must be positive";
            }
        }

        /// <summary>
        /// Builds the shape of the region.
        /// </summary>
        public RoiShape ToShape() => Shape switch
        {
            RoiStepShape.Ellipse => new EllipseRoiShape(new System.Drawing.RectangleF(X, Y, Width, Height)),
            RoiStepShape.Polygon => new PolygonRoiShape(Points.Select(p => new System.Drawing.PointF(p.X, p.Y)).ToArray()),
            _ => new RectangleRoiShape(new System.Drawing.RectangleF(X, Y, Width, Height)),
        };
    }

    /// <summary>
    /// Extracts the sprites (blobs) of a sprite sheet: the current image becomes the blobs stacked vertically,
    /// and each blob can also be saved as a separate image.
    /// </summary>
    public sealed record BlobStep : PipelineStep
    {
        /// <summary>
        /// Detection settings: background colors and tolerance, connectivity, size filters.
        /// </summary>
        public BlobOptions Options { get; init; } = new();

        /// <summary>
        /// Pixels between the stacked blobs.
        /// </summary>
        public int Padding { get; init; } = 1;

        /// <summary>
        /// When set, every blob is also saved in this folder (relative to the pipeline file), as PNG.
        /// </summary>
        public string ExportFolder { get; init; } = string.Empty;

        /// <summary>
        /// File name prefix of the exported blobs.
        /// </summary>
        public string ExportPrefix { get; init; } = "sprite";

        /// <inheritdoc/>
        public override string Kind => "Blob";

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            if (Options == null)
            {
                yield return $"{nameof(Options)} are missing";
                yield break;
            }
            foreach (var error in Options.Validate())
                yield return error;
            if (Padding < 0)
                yield return $"{nameof(Padding)} must not be negative (is {Padding})";
        }
    }

    /// <summary>
    /// Runs a color transformation on the current image; the options record selects the algorithm.
    /// </summary>
    public sealed record TransformStep : PipelineStep
    {
        /// <summary>
        /// Options of the transformation (any <see cref="TransformOptions"/> record).
        /// </summary>
        public TransformOptions? Options { get; init; }

        /// <inheritdoc/>
        public override string Kind => $"Transform {Options?.TransformType}";

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            if (Options == null)
            {
                yield return $"{nameof(Options)} are missing";
                yield break;
            }
            foreach (var error in Options.Validate())
                yield return error;
        }
    }

    /// <summary>
    /// File formats of <see cref="SaveStep"/>.
    /// </summary>
    public enum PipelineSaveFormat
    {
        /// <summary>From the file extension (PNG when unknown).</summary>
        Auto,
        /// <summary>PNG.</summary>
        Png,
        /// <summary>24 bit BMP.</summary>
        Bmp,
        /// <summary>JPG (lossy: not suited to reduced palettes).</summary>
        Jpg,
        /// <summary>8 bit indexed BMP.</summary>
        IndexedBmp,
    }

    /// <summary>
    /// Saves the current image; the pipeline goes on with the same image.
    /// </summary>
    public sealed record SaveStep : PipelineStep
    {
        /// <summary>
        /// Output file; a relative path is relative to the folder of the pipeline file.
        /// </summary>
        public string File { get; init; } = string.Empty;

        /// <summary>
        /// File format.
        /// </summary>
        public PipelineSaveFormat Format { get; init; } = PipelineSaveFormat.Auto;

        /// <summary>
        /// JPG quality [1, 100].
        /// </summary>
        public int Quality { get; init; } = 90;

        /// <inheritdoc/>
        public override string Kind => "Save";

        /// <inheritdoc/>
        public override IEnumerable<string> Validate()
        {
            if (string.IsNullOrWhiteSpace(File))
                yield return $"{nameof(File)} is missing";
            if (!Enum.IsDefined(Format))
                yield return $"{nameof(Format)} {Format} is not valid";
            if (Quality < 1 || Quality > 100)
                yield return $"{nameof(Quality)} must be in [1, 100] (is {Quality})";
        }
    }
}
