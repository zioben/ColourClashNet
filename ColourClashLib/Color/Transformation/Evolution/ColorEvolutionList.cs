using ColourClashLib.Color.Transformation;
using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System.Collections;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace ColourClashLib.Color.Transformation.Evolution
{


    /// <summary>
    /// One training step of the clustering: the set of clusters with their colors and assigned histograms.
    /// </summary>
    /// <typeparam name="T">Cluster type</typeparam>
    internal class ColorEvolutionList<T> where T : IColorEvolution, new()
    {
        static string sC = nameof(ColorEvolutionList<T>);

        /// <summary>
        /// Clusters of this step.
        /// </summary>
        internal List<T> evolution = new();
        /// <summary>
        /// Number of clusters reseeded while computing the next step.
        /// </summary>
        internal int ReseedingCount { get; set; } = 0;



        /// <summary>
        /// Adds a new cluster with color <paramref name="rgb"/> and an empty histogram.
        /// </summary>
        internal void SetClusterColor(int rgb)
        {
            var evo = new T();
            evo.SetClusterColor(rgb);
            evolution.Add(evo);
        }

        /// <summary>
        /// Returns the cluster nearest to <paramref name="rgb"/>.
        /// </summary>
        private T FindBestCluster(int rgb, ColorDistanceEvaluationMode mode)
        {
            T bestEvolution = new();
            double bestDistance = double.MaxValue;
            foreach (var evo in evolution)
            {
                double d = evo.Distance(rgb, mode);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    bestEvolution = evo;
                }
            }
            return bestEvolution;
        }



        /// <summary>
        /// Maps every color of <paramref name="image"/> to the color of its nearest cluster.
        /// </summary>
        internal ColorTransformationMap CreateTransformationMap(ImageData image, ColorDistanceEvaluationMode mode)
        {
            ColorTransformationMap map = new ColorTransformationMap();
            foreach (var rgb in image.ColorPalette)
            {
                var bestCluster = FindBestCluster(rgb, mode);
                map.Add(rgb, bestCluster.GetClusterColor(rgb,mode));
            }
            return map;
        }

        /// <summary>
        /// Assigns every histogram color (with its pixel count) to the nearest cluster.
        /// </summary>
        internal void Clusterize(HistogramRGB histogram, ColorDistanceEvaluationMode mode)
        {
            foreach (var kvp in histogram.HistogramDictionary)
            {
                var bestEvolution = FindBestCluster(kvp.Key, mode);
                bestEvolution.HistDictionary.Add(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Computes the cluster colors of the next training step.
        /// <para/>
        /// Fixed (reference palette) clusters keep their color, the others move to their representative color;
        /// clusters that end up empty or duplicated are reseeded with the worst represented colors.
        /// </summary>
        /// <param name="referencePalette">Fixed colors</param>
        /// <param name="mode">How the representative color is selected</param>
        /// <param name="distanceMode">Distance metric</param>
        /// <returns>The cluster colors for the next step, in the same order as the current clusters.</returns>
        internal List<int> ComputeNextEvolution(Palette referencePalette, ColorSelectionMode mode, ColorDistanceEvaluationMode distanceMode)
        {
            string sMethod = nameof(ComputeNextEvolution);
            int n = evolution.Count;
            var next = new int?[n];
            var fixedEntries = new HashSet<int>(referencePalette.RgbPalette);

            // 1. Compute the next color of each cluster: reference palette colors stay fixed,
            //    the others move to the representative color of their histogram
            for (int i = 0; i < n; i++)
            {
                var evo = evolution[i];
                if (referencePalette.Contains(evo.ClusterRGB))
                {
                    next[i] = evo.ClusterRGB;
                }
                else if (evo.HistDictionary.Count > 0)
                {
                    int rgbMean = ColorIntExt.GetColorMean(evo.HistDictionary, mode);
                    // The new color may coincide with an already used color: the cluster collapses
                    // and is left without color, to be reseeded in step 3
                    if (fixedEntries.Add(rgbMean))
                        next[i] = rgbMean;
                }
            }

            // 2. Reseed candidates: colors of populated clusters, ranked by their weighted error
            //    (pixel count * distance from the cluster color). Badly represented colors come first.
            var evoItems = evolution.Where(evo => evo.HistDictionary.Count > 1).ToList();
            var evoCandidates = evoItems.SelectMany(e => e.HistDictionary
                 .Where(kvp => !fixedEntries.Contains(kvp.Key))
                 .Select(kvp => (Color: kvp.Key, Err: (double)kvp.Value * e.GetClusterColor(kvp.Key, distanceMode).Distance(kvp.Key, distanceMode)))).ToList();
            var colorCandidates = evoCandidates.Where(t => t.Err > 0).OrderByDescending(t => t.Err).ToList();

            // 3. Reseed the clusters left without color with the worst represented colors
            int ci = 0;
            for (int i = 0; i < n; i++)
            {
                if (next[i].HasValue)
                    continue;

                while (ci < colorCandidates.Count && fixedEntries.Contains(colorCandidates[ci].Color))
                    ci++;

                if (ci < colorCandidates.Count)
                {
                    //LogMan.Warning(sC, sMethod, $"Cluster 0x{evolution[i].ClusterColorRGB:X8} empty/duplicated, reseed with 0x{colorCandidates[ci].Color:X8}");
                    ReseedingCount++;
                    next[i] = colorCandidates[ci].Color;
                    fixedEntries.Add(colorCandidates[ci].Color);
                    ci++;
                }
                else
                {
                    // No candidate available: keep the previous center
                    next[i] = evolution[i].ClusterRGB;
                }
            }

            //if (ReseedingCount > 0)
            //{
            //    LogMan.Warning(sC, sMethod, $"Reseeded {ReseedingCount} items during process");
            //}


            return next.Select(x => x.Value).ToList();
        }


    }


}
