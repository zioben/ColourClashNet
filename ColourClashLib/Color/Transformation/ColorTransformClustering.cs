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
    /// Clusters are seeded with the user palette plus random colors and refined for <see cref="ClusteringOptions.TrainingLoops"/>
    /// iterations; <typeparamref name="T"/> defines how a cluster computes its color (e.g. Amiga EHB half-bright pairs).
    /// </summary>
    /// <typeparam name="T">Cluster evolution strategy</typeparam>
    public class ColorTransformClusteringTemplate<T> : ColorTransformMapBase<ClusteringOptions> where T : IColorEvolution, new()
    {
        static string sC = nameof(ColorTransformClusteringTemplate<T>);



        /// <summary>
        /// Creates the clustering reduction.
        /// </summary>
        public ColorTransformClusteringTemplate()
        {
            Description = "K-Means color reduction";
        }







        /// <summary>
        /// Runs the clustering and builds the map from the last evolution; raises <see cref="ColorTransformBase.Progress"/> after each loop.
        /// </summary>
        protected override ColorTransformResult CreateTransformationMap(CancellationToken token = default)
        {
            string sMethod = nameof(CreateTransformationMap);
            var options = Options;
            var userPalette = UserPalette;
            Random random = new Random(1);

            // Creating initial color seeds for the clustering algorithm
            var sortedHistogram = new HistogramRGB().Create(ImageSource).SortColorsDescending();
            var imagePalette = ImageSource.ColorPalette;
            var seedPalette = Palette.MergePalette(userPalette, imagePalette);
            // If got less colors than wanted, just map them directly
            if (seedPalette.Count <= options.MaxColors)
            {
                LogDebug(sC, sMethod, $"{Type} : {seedPalette.Count} colors fit in {options.MaxColors} : identity map, no clustering");
                foreach (var rgb in seedPalette)
                {
                    TransformationMap.Add(rgb, rgb);
                }
                return ColorTransformResult.CreateValidResult();
            }
            // Otherwise seed the clusters with the reference palette plus random colors (fixed seed: reproducible)
            else
            {
                seedPalette = new Palette().Create(userPalette);
                while (seedPalette.Count < options.MaxColors)
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
            var rgbFixedList = userPalette.ToList();

            // Prepare the evolutions
            List<ColorEvolutionList<T>> evolutions = new();
            var rgbFixedSet = new HashSet<int>(userPalette);
            LogDebug(sC, sMethod, $"{Type} : clustering {imagePalette.Count} colors into {options.MaxColors} ({userPalette.Count} fixed user colors), {options.TrainingLoops} loops, selection {options.Selection}");

            for (int train = 0; train < options.TrainingLoops; train++)
            {
                token.ThrowIfCancellationRequested();
                LogTrace(sC, sMethod, $"{Type} : training loop {train + 1}/{options.TrainingLoops}");

                var nextEvolution = new ColorEvolutionList<T>();
                if (evolutions.Count == 0)
                {
                    foreach (var rgb in seedPalette)
                        nextEvolution.SetClusterColor(rgb);
                }
                else
                {
                    var lastEvolution = evolutions.Last();
                    var nextClusterColors = lastEvolution.ComputeNextEvolution(userPalette, options.Selection, ColorDistanceEvaluationMode);
                    foreach (var rgb in nextClusterColors)
                    {
                        nextEvolution.SetClusterColor(rgb);
                    }
                }

                nextEvolution.Clusterize(sortedHistogram, ColorDistanceEvaluationMode);
                evolutions.Add(nextEvolution);

                var args = new ColorProcessingEventArgs()
                {
                    Transformation = this,
                    ProcessingResults = ColorTransformResult.CreateValidResult(ImageSource, null, $"Loop {train}/{options.TrainingLoops}"),
                    CompletedPercent = 100 * (train + 1) / options.TrainingLoops
                };
                if (EnablePreviewEvents)
                {
                    var map = nextEvolution.CreateTransformationMap(ImageSource, ColorDistanceEvaluationMode);
                    args.ProcessingResults.DataOut = map.Transform(ImageSource, token);
                }

                RaiseProgressEvent(args);
            }
            TransformationMap = evolutions.Last().CreateTransformationMap(ImageSource, ColorDistanceEvaluationMode);
            LogDebug(sC, sMethod, $"{Type} : clustering completed : {TransformationMap.GetOutputPalette().Count} output colors");
            return ColorTransformResult.CreateValidResult();
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