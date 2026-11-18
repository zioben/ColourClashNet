using ColourClashLib;
using ColourClashLib.Color.Transformation.Evolution;
using ColourClashNet.Color.Conversion;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using NLog.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Color.Transformation.ColorTransformCbm64;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// K-means color reduction.
    /// <para/>
    /// Clusters are seeded with the reference palette plus random colors and refined for <see cref="TrainingLoop"/>
    /// iterations; <typeparamref name="T"/> defines how a cluster computes its color (e.g. Amiga EHB half-bright pairs).
    /// </summary>
    /// <typeparam name="T">Cluster evolution strategy</typeparam>
    public class ColorTransformClusteringTemplate<T> : ColorTransformBase where T : IColorEvolution, new()
    {
        static string sC = nameof(ColorTransformClusteringTemplate<T>);
        /// <summary>
        /// Number of clusters (output colors).
        /// </summary>
        public int MaxColorsWanted
        {
            get => config.MaxColorsWanted;
            set => config.MaxColorsWanted = value;
        }
        /// <summary>
        /// How the color of each cluster is recomputed.
        /// </summary>
        public ColorSelectionMode ClusterColorSelectionMode
        {
            get => config.ClusterColorSelectionMode;
            set => config.ClusterColorSelectionMode = value;
        }

        /// <summary>
        /// Number of clustering iterations.
        /// </summary>
        public int TrainingLoop
        {
            get => config.ClusterTrainingLoop;
            set => config.ClusterTrainingLoop = value;
        }


        /// <summary>
        /// Creates the clustering reduction.
        /// </summary>
        public ColorTransformClusteringTemplate()
        {
            Type = ColorTransformType.ColorReductionClustering;
            Description = "K-Means color reduction";
        }

        /// <summary>
        /// Sets the processing parameters (fluent).
        /// </summary>
        public ColorTransformClusteringTemplate<T> WithProcessingParams(int maxColorsWanted, int trainingLoop, ColorSelectionMode clusterColorSelectionMode)
        {
            MaxColorsWanted = maxColorsWanted;
            TrainingLoop = trainingLoop;
            ClusterColorSelectionMode = clusterColorSelectionMode;
            return this;
        }

        /// <summary>
        /// Sets the processing parameters from a configuration (fluent).
        /// </summary>
        public ColorTransformClusteringTemplate<T> WithClustering(ColorTransformConfig cfg) =>
         WithProcessingParams(cfg.MaxColorsWanted, cfg.ClusterTrainingLoop, cfg.ClusterColorSelectionMode);





        /// <summary>
        /// Runs the clustering and builds the map from the last evolution; raises <see cref="ColorTransformBase.ProcessPartial"/> after each loop.
        /// </summary>
        protected override ColorTransformResult CreateTransformationMap(CancellationToken token = default)
        {
            string sMethod = nameof(CreateTransformationMap);
            Random random = new Random(1);

            // Creating initial color seeds for the clustering algorithm
            var sortedHistogram = new HistogramRGB().Create(ImageSource).SortColorsDescending();
            var imagePalette = ImageSource.ColorPalette;
            var seedPalette = Palette.MergePalette(ReferencePalette, imagePalette);
            // If got less colors than wanted, just map them directly
            if (seedPalette.Count <= MaxColorsWanted)
            {
                foreach (var rgb in seedPalette)
                {
                    TransformationMap.Add(rgb, rgb);
                }
                return ColorTransformResult.CreateValidResult();
            }
            // Otherwise seed the clusters with the reference palette plus random colors (fixed seed: reproducible)
            else
            {
                seedPalette = new Palette().Create(ReferencePalette);
                while (seedPalette.Count < MaxColorsWanted)
                {
                    seedPalette.Add(ColorIntExt.FromRGB(255.0 * random.NextDouble(), 255.0 * random.NextDouble(), 255.0 * random.NextDouble()));
                }
            }
            // K-means style clustering. Every training loop:
            // 1: each histogram color is assigned to the nearest cluster color (Clusterize);
            // 2: the next cluster colors are computed (ComputeNextEvolution):
            //    - EvaluateColorMean -> the cluster center of mass (pixel-count weighted mean);
            //    - UseColorPalette   -> the most representative color already in the cluster.
            // Reference palette colors are fixed and never move; empty or duplicated clusters are reseeded.
            // Each loop is stored as a new ColorEvolutionList, so the whole evolution can be inspected.
            var rgbFixedList = ReferencePalette.ToList();

            // Prepare the evolutions
            List<ColorEvolutionList<T>> evolutions = new();
            var rgbFixedSet = new HashSet<int>(ReferencePalette);

            for (int train = 0; train < TrainingLoop; train++)
            {
                LogMan.Trace(sC, sMethod, $"{Type} : Training loop {train}");

                var nextEvolution = new ColorEvolutionList<T>();
                if (evolutions.Count == 0)
                {
                    foreach (var rgb in seedPalette)
                        nextEvolution.SetClusterColor(rgb);
                }
                else
                {
                    var lastEvolution = evolutions.Last();
                    var nextClusterColors = lastEvolution.ComputeNextEvolution(ReferencePalette, ClusterColorSelectionMode, ColorDistanceEvaluationMode);
                    foreach (var rgb in nextClusterColors)
                    {
                        nextEvolution.SetClusterColor(rgb);
                    }
                }

                nextEvolution.Clusterize(sortedHistogram, ColorDistanceEvaluationMode);
                evolutions.Add(nextEvolution);

                var args = new ColorProcessingEventArgs()
                {
                    ColorTransformInterface = this,
                    ProcessingResults = ColorTransformResult.CreateValidResult(ImageSource, null, $"Loop {train}/{TrainingLoop}"),
                    CompletedPercent = 100 * (train + 1) / TrainingLoop
                };
                if (EnablePreviewEvents)
                {
                    var map = nextEvolution.CreateTransformationMap(ImageSource, ColorDistanceEvaluationMode);
                    args.ProcessingResults.DataOut = map.Transform(ImageSource, token);
                }

                RaiseProcessPartialEvent(args);
            }
            TransformationMap = evolutions.Last().CreateTransformationMap(ImageSource, ColorDistanceEvaluationMode);
            return ColorTransformResult.CreateValidResult();
        }


        /// <summary>
        /// Applies the transformation map.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {

            var outputImage = TransformationMap.Transform(ImageSource, token);
            if (outputImage != null)
            {
                return ColorTransformResult.CreateValidResult(ImageSource, outputImage);
            }
            return new();
        }
    }

    /// <summary>
    /// K-means color reduction with standard clusters.
    /// </summary>
    public class ColorTransformClustering : ColorTransformClusteringTemplate<ColorEvolution>;
    /// <summary>
    /// K-means color reduction for the Amiga Extra Half-Brite mode (each cluster also produces its half-bright color).
    /// </summary>
    public class ColorTransformClusteringEHB : ColorTransformClusteringTemplate<ColorEvolutionAmigaEHB>;
}