using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Pipeline;
using System.Text.Json;
using Xunit;

namespace XUnitTester
{
    /// <summary>
    /// Tests of the JSON pipelines: serialization, versioning, validation and execution.
    /// </summary>
    public class TestPipeline
    {
        static ImageData Gradient(int width, int height)
        {
            var matrix = new int[height, width];
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    matrix[r, c] = ColorIntExt.FromRGB(c * 255 / Math.Max(1, width - 1), r * 255 / Math.Max(1, height - 1), 128);
            return new ImageData().Create(matrix);
        }

        static PipelineDefinition Sample() => new()
        {
            Name = "sample",
            Steps = new PipelineStep[]
            {
                new LoadStep { File = "in.png" },
                new CropStep { X = 1, Y = 2, Width = 8, Height = 4 },
                new ResizeStep { Width = 4, Mode = ResizeMode.Nearest },
                new TransformStep { Options = new ClusteringOptions { MaxColors = 4, TrainingLoops = 2 } },
                new SaveStep { File = "out.png", Format = PipelineSaveFormat.Png },
            },
        };

        [Fact]
        public void Json_RoundTrip_KeepsStepsAndNestedOptions()
        {
            var json = PipelineSerializer.Serialize(Sample());
            var copy = PipelineSerializer.Deserialize(json);

            Assert.Equal(json, PipelineSerializer.Serialize(copy));
            Assert.Equal(PipelineDefinition.CurrentVersion, copy.Version);
            var transform = Assert.IsType<TransformStep>(copy.Steps[3]);
            Assert.Equal(4, Assert.IsType<ClusteringOptions>(transform.Options).MaxColors);
            // Only settings are written, not the computed properties
            Assert.DoesNotContain("\"Kind\"", json);
            Assert.DoesNotContain("\"TransformType\"", json);
        }

        [Fact]
        public void Deserialize_AcceptsHandWrittenFiles()
        {
            const string json = """
                // comments, any letter case, $type not first, trailing commas
                {
                  "steps": [
                    { "file": "a.png", "$type": "Load" },
                    { "$type": "Transform", "options": { "maxColors": 3, "$type": "Fast" } },
                  ],
                }
                """;

            var pipeline = PipelineSerializer.Deserialize(json);

            Assert.Equal(1, pipeline.Version);   // missing version: read as 1
            Assert.Equal("a.png", Assert.IsType<LoadStep>(pipeline.Steps[0]).File);
            Assert.Equal(3, Assert.IsType<FastOptions>(Assert.IsType<TransformStep>(pipeline.Steps[1]).Options).MaxColors);
        }

        [Fact]
        public void Deserialize_RefusesNewerVersions()
        {
            var ex = Assert.Throws<JsonException>(() => PipelineSerializer.Deserialize("""{ "Version": 99, "Steps": [] }"""));
            Assert.Contains("newer program", ex.Message);
        }

        [Fact]
        public void Validate_ReportsEveryProblem_WithTheStepNumber()
        {
            var pipeline = new PipelineDefinition
            {
                Steps = new PipelineStep[]
                {
                    new LoadStep(),
                    new ResizeStep(),
                    new TransformStep { Options = new MedianCutOptions { MaxColors = 0 } },
                },
            };

            var errors = pipeline.Validate().ToList();

            Assert.Contains(errors, e => e.StartsWith("step 1 (Load) : File"));
            Assert.Contains(errors, e => e.StartsWith("step 2 (Resize)"));
            Assert.Contains(errors, e => e.StartsWith("step 3 (Transform ColorReductionMedianCut) : MaxColors"));
            Assert.Contains("the pipeline has no steps", new PipelineDefinition().Validate());
        }

        [Fact]
        public void WithInputAndOutput_ReplaceOrAddTheFileSteps()
        {
            var changed = Sample().WithInput("photo.png").WithOutput("result.png");
            Assert.Equal("photo.png", Assert.IsType<LoadStep>(changed.Steps[0]).File);
            Assert.Equal("result.png", Assert.IsType<SaveStep>(changed.Steps[^1]).File);
            Assert.Equal(5, changed.Steps.Count);

            var added = new PipelineDefinition { Steps = new PipelineStep[] { new ResizeStep { Width = 2 } } }
                .WithInput("a.png").WithOutput("b.png");
            Assert.IsType<LoadStep>(added.Steps[0]);
            Assert.IsType<SaveStep>(added.Steps[^1]);
        }

        [Fact]
        public void Run_ChainsTheSteps_OnTheInputImage()
        {
            var pipeline = new PipelineDefinition
            {
                Steps = new PipelineStep[]
                {
                    new CropStep { X = 2, Y = 2, Width = 16, Height = 8 },
                    new ResizeStep { Width = 8, Mode = ResizeMode.Average },
                    new TransformStep { Options = new FastOptions { MaxColors = 4 } },
                },
            };
            var runner = new PipelineRunner();
            var started = new List<int>();
            runner.StepStarted += (s, e) => started.Add(e.Number);

            var result = runner.Run(pipeline, Gradient(32, 16));

            Assert.True(result.IsSuccess, result.Message);
            Assert.Equal(new[] { 1, 2, 3 }, started);
            Assert.Equal(8, result.Image!.Width);
            Assert.Equal(4, result.Image.Height);   // aspect ratio kept
            Assert.True(result.Image.Colors <= 4);
            Assert.False(double.IsNaN(result.Steps[2].TransformationError));
        }

        [Fact]
        public void Run_StopsAtTheFailingStep()
        {
            var pipeline = new PipelineDefinition
            {
                Steps = new PipelineStep[]
                {
                    new LoadStep { File = "this-file-does-not-exist.png" },
                    new ResizeStep { Width = 2 },
                },
            };

            var result = new PipelineRunner().Run(pipeline);

            Assert.False(result.IsSuccess);
            Assert.Contains("step 1 (Load) failed", result.Message);
            Assert.Single(result.Steps);
        }

        [Fact]
        public void Run_WithoutAnImage_FailsWithAClearMessage()
        {
            var pipeline = new PipelineDefinition { Steps = new PipelineStep[] { new ResizeStep { Width = 2 } } };

            var result = new PipelineRunner().Run(pipeline);

            Assert.False(result.IsSuccess);
            Assert.Contains("no image", result.Message);
        }

        [Fact]
        public void Run_CanBeCancelled()
        {
            var pipeline = new PipelineDefinition { Steps = new PipelineStep[] { new ResizeStep { Width = 2 } } };
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var result = new PipelineRunner().Run(pipeline, Gradient(4, 4), cts.Token);

            Assert.False(result.IsSuccess);
            Assert.Contains("cancelled", result.Message);
        }

        [Fact]
        public void RoiStep_ExtractsTheShape_WithTheOutsideColor()
        {
            var pipeline = new PipelineDefinition
            {
                Steps = new PipelineStep[]
                {
                    new RoiStep { Shape = RoiStepShape.Ellipse, X = 0, Y = 0, Width = 8, Height = 8, OutsideColor = 0x123456 },
                },
            };

            var result = new PipelineRunner().Run(pipeline, Gradient(16, 16));

            Assert.True(result.IsSuccess, result.Message);
            Assert.Equal(8, result.Image!.Width);
            Assert.Equal(0x123456, result.Image.GetMatrix()[0, 0]);   // corner outside the ellipse
            Assert.NotEqual(0x123456, result.Image.GetMatrix()[4, 4]);
            Assert.Contains("at least 3", new RoiStep { Shape = RoiStepShape.Polygon }.Validate().First());
        }

        [Fact]
        public void BlobStep_StacksTheSprites_AndDropsTheSmallOnes()
        {
            // Two 3x3 sprites and a 1 pixel speck on a magenta background
            int magenta = 0xFF00FF;
            var matrix = new int[8, 16];
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 16; c++)
                    matrix[r, c] = magenta;
            for (int r = 1; r < 4; r++)
                for (int c = 1; c < 4; c++)
                {
                    matrix[r, c] = 0x00FF00;
                    matrix[r + 3, c + 8] = 0x0000FF;
                }
            matrix[7, 14] = 0xFFFFFF;
            var pipeline = new PipelineDefinition
            {
                Steps = new PipelineStep[]
                {
                    new BlobStep { Options = new ColourClashProcessing.Blob.BlobOptions { MinArea = 4 }, Padding = 1 },
                },
            };

            var result = new PipelineRunner().Run(pipeline, new ImageData().Create(matrix));

            Assert.True(result.IsSuccess, result.Message);
            Assert.StartsWith("2 blobs", result.Steps[0].Message);
        }

        [Fact]
        public void Colors_AreWrittenAsHex_AndReadFromHexOrNumbers()
        {
            var options = new BackgroundRemovalOptions { BackgroundColors = new[] { 0xFF00FF }, ReplacementColor = 0x102030 };

            var json = TransformOptionsSerializer.Serialize(options);
            Assert.Contains("\"#FF00FF\"", json);
            Assert.Contains("\"#102030\"", json);

            var fromNumbers = Assert.IsType<BackgroundRemovalOptions>(TransformOptionsSerializer.Deserialize(
                """{ "$type": "BackgroundRemoval", "BackgroundColors": [ 16711935, "0x00FF00", "0000FF" ], "ReplacementColor": "#102030" }"""));
            Assert.Equal(new[] { 0xFF00FF, 0x00FF00, 0x0000FF }, fromNumbers.BackgroundColors);
            Assert.Equal(0x102030, fromNumbers.ReplacementColor);
            Assert.Throws<JsonException>(() => TransformOptionsSerializer.Deserialize("""{ "$type": "BackgroundRemoval", "ReplacementColor": "#XYZ" }"""));
        }

        [Fact]
        public void ExamplePipelines_AreValid()
        {
            // The examples live in the repository: walk up from the test binaries to find them
            var folder = AppContext.BaseDirectory;
            while (folder != null && !Directory.Exists(Path.Combine(folder, "ColourClash", "PipelineExamples")))
                folder = Path.GetDirectoryName(folder);
            Assert.SkipWhen(folder == null, "PipelineExamples folder not found: the tests are not running from the repository");
            var files = Directory.GetFiles(Path.Combine(folder!, "ColourClash", "PipelineExamples"), "*.json", SearchOption.AllDirectories);
            Assert.NotEmpty(files);
            foreach (var file in files)
            {
                var errors = PipelineSerializer.Load(file).Validate().ToList();
                Assert.True(errors.Count == 0, $"{file} : {string.Join("; ", errors)}");
            }
        }

        [Fact]
        public void Resize_Nearest_KeepsColors_AndAverage_MixesThem()
        {
            var matrix = new int[1, 2] { { 0x000000, 0xFFFFFF } };
            var image = new ImageData().Create(matrix);

            var nearest = ImageTools.Resize(image, 4, 1, false).GetMatrix();
            Assert.Equal(new[] { 0x000000, 0x000000, 0xFFFFFF, 0xFFFFFF }, nearest.Cast<int>());

            var average = ImageTools.Resize(image, 1, 1, true).GetMatrix();
            Assert.Equal(ColorIntExt.FromRGB(127, 127, 127), average[0, 0]);
        }
    }
}
