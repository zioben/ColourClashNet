using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using Xunit;

namespace XUnitTester
{
    /// <summary>
    /// Tests of the immutable option records that configure the transformations.
    /// </summary>
    public class TestTransformOptions
    {
        public static IEnumerable<object[]> AllTransformTypes() =>
            Enum.GetValues<ColorTransformType>().Select(t => new object[] { t });

        [Theory]
        [MemberData(nameof(AllTransformTypes))]
        public void DefaultOptions_AreValid_AndCreateTheMatchingTransformation(ColorTransformType type)
        {
            var options = TransformOptions.CreateDefault(type);
            var transform = ColorTransformBase.CreateTransform(options);

            Assert.Same(options, transform.Options);
            var expected = type == ColorTransformType.None ? ColorTransformType.ColorIdentity : type;
            Assert.Equal(expected, options.TransformType);
            Assert.Equal(expected, transform.Type);

            // The palette mappings need a user palette: every other default must be valid as is
            var errors = options.Validate().ToList();
            if (options is FixedPaletteOptions || options is EnhancedPaletteOptions)
                Assert.NotEmpty(errors);
            else
                Assert.Empty(errors);
        }

        [Theory]
        [MemberData(nameof(AllTransformTypes))]
        public void Json_RoundTrip_KeepsTypeAndValues(ColorTransformType type)
        {
            var options = TransformOptions.CreateDefault(type) with
            {
                DistanceMode = ColorDistanceEvaluationMode.LAB,
                Dithering = new DitherOptions { DitheringType = ColorDithering.Stucki, DitheringStrength = 0.5 },
                UserPalette = new List<int> { 0x112233, 0x445566 },
            };

            var json = TransformOptionsSerializer.Serialize(options);
            var copy = TransformOptionsSerializer.Deserialize(json);

            Assert.Equal(options.GetType(), copy.GetType());
            Assert.Equal(json, TransformOptionsSerializer.Serialize(copy));
            Assert.Contains("\"Stucki\"", json);   // enums are written by name
        }

        [Fact]
        public void Json_RoundTrip_KeepsNestedPolymorphicOptions()
        {
            var options = new ScanlineOptions
            {
                ChunkHeight = 2,
                PaletteMode = ColorTransformScanLine.ScanlinePaletteMode.SharedChunksPalette,
                ChunkReduction = new MedianCutOptions { MaxColors = 4 },
                SharedReduction = new ClusteringOptions { MaxColors = 12, TrainingLoops = 3 },
            };

            var copy = Assert.IsType<ScanlineOptions>(TransformOptionsSerializer.Deserialize(TransformOptionsSerializer.Serialize(options)));

            Assert.Equal(4, Assert.IsType<MedianCutOptions>(copy.ChunkReduction).MaxColors);
            var shared = Assert.IsType<ClusteringOptions>(copy.SharedReduction);
            Assert.Equal(12, shared.MaxColors);
            Assert.Equal(3, shared.TrainingLoops);
        }

        [Fact]
        public void With_OnBaseType_KeepsTheRuntimeType()
        {
            TransformOptions options = new ZxOptions { SeedLow = 100 };
            var copy = options with { DistanceMode = ColorDistanceEvaluationMode.HSV };

            var zx = Assert.IsType<ZxOptions>(copy);
            Assert.Equal(100, zx.SeedLow);
            Assert.Equal(ColorDistanceEvaluationMode.RGB, options.DistanceMode);   // the original is unchanged
        }

        [Fact]
        public void InheritCommon_CopiesOnlyTheCommonSettings()
        {
            var outer = new VgaOptions
            {
                DistanceMode = ColorDistanceEvaluationMode.LAB,
                Dithering = new DitherOptions { DitheringType = ColorDithering.Atkinson },
                UserPalette = new List<int> { 0xFF0000 },
            };
            var nested = new ClusteringOptions { MaxColors = 7, TrainingLoops = 2 };

            var result = outer.InheritCommon(nested);

            Assert.Equal(ColorDistanceEvaluationMode.LAB, result.DistanceMode);
            Assert.Equal(ColorDithering.Atkinson, result.Dithering.DitheringType);
            Assert.Equal(new[] { 0xFF0000 }, result.UserPalette);
            Assert.Equal(7, result.MaxColors);
            Assert.Equal(2, result.TrainingLoops);
            Assert.Equal(ColorDistanceEvaluationMode.RGB, nested.DistanceMode);   // the nested options are unchanged
        }

        [Fact]
        public void DeepClone_IsIndependentFromNestedRecords()
        {
            var options = new ScanlineOptions { ChunkReduction = new ClusteringOptions { MaxColors = 5 } };

            var copy = Assert.IsType<ScanlineOptions>(options.DeepClone());

            Assert.Equal(TransformOptionsSerializer.Serialize(options), TransformOptionsSerializer.Serialize(copy));
            Assert.NotSame(options.ChunkReduction, copy.ChunkReduction);
            Assert.Equal(5, copy.ChunkReduction.MaxColors);
        }

        [Fact]
        public void Validate_ReportsNestedErrors()
        {
            var options = new ScanlineOptions
            {
                ChunkHeight = 0,
                ChunkReduction = new ClusteringOptions { MaxColors = 0, TrainingLoops = 0 },
                Dithering = new DitherOptions { DitheringStrength = 3 },
            };

            var errors = options.Validate().ToList();

            Assert.Contains(errors, e => e.StartsWith("ChunkHeight"));
            Assert.Contains(errors, e => e.StartsWith("ChunkReduction : MaxColors"));
            Assert.Contains(errors, e => e.StartsWith("ChunkReduction : TrainingLoops"));
            Assert.Contains(errors, e => e.StartsWith("Dithering : DitheringStrength"));
        }

        [Fact]
        public void InvalidOptions_ProduceAnErrorResult_BeforeProcessing()
        {
            var image = new ImageData().Create(MatrixTools.CreateMatrix(4, 4, 0x808080));
            var transform = new ColorTransformClustering { Options = new ClusteringOptions { MaxColors = 0 } };

            var result = transform.Process(image);

            Assert.False(result.IsSuccess);
            Assert.Contains("invalid options", result.Message);
            Assert.Contains("MaxColors", result.Message);
        }

        [Fact]
        public void SetOptions_WithWrongType_Throws()
        {
            IColorTransform transform = new ColorTransformFast();
            Assert.Throws<ArgumentException>(() => transform.SetOptions(new ClusteringOptions()));
            Assert.Same(transform, transform.SetOptions(new FastOptions { MaxColors = 3 }));
            Assert.Equal(3, ((FastOptions)transform.Options).MaxColors);
        }

        [Fact]
        public void UserPalette_IsKeptByTheReduction()
        {
            // 16 distinct grays reduced to 4 colors: the user color must survive
            var matrix = new int[1, 16];
            for (int c = 0; c < 16; c++)
                matrix[0, c] = ColorIntExt.FromRGB(c * 16, c * 16, c * 16);
            var image = new ImageData().Create(matrix);
            int userColor = ColorIntExt.FromRGB(255, 0, 0);
            var transform = new ColorTransformFast { Options = new FastOptions { MaxColors = 4, UserPalette = new List<int> { userColor } } };

            var result = transform.Process(image);

            Assert.True(result.IsSuccess, result.Message);
            Assert.Contains(userColor, transform.TransformationMap.GetOutputPalette().ToList());
        }

        [Fact]
        public void PipelineOptions_DefaultsAreValid()
        {
            Assert.Empty(new PipelineOptions().Validate());
            var invalid = new PipelineOptions { Transform = new MedianCutOptions { MaxColors = 1000 } };
            Assert.Contains(invalid.Validate(), e => e.StartsWith("Transform : MaxColors"));
        }
    }
}
