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
    /// 
    /// </summary>
    internal class ColorEvolutionList<T> where T : IColorEvolution, new()
    {
        static string sC = nameof(ColorEvolutionList<T>);

        /// <summary>
        /// The list represents color evolution workflow, step by step, for each cluster.
        /// </summary>
        internal List<T> evolution = new();
        internal int ReseedingCount { get; set; } = 0;



        /// <summary>
        /// Add a new cluster, set it with initial color and empty histogram.
        /// </summary>
        /// <param name="rgb"></param>
        internal void SetClusterColor(int rgb)
        {
            var evo = new T();
            evo.SetClusterColor(rgb);
            evolution.Add(evo);
        }

        /// <summary>
        /// Get the cluster based on the specified distance evaluation mode.
        /// </summary>
        /// <param name="rgb"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
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
        /// Create the color transformation map based on the current state of the color evolution clusters.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
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
        /// Populate cluster data using a color histogram
        /// </summary>
        /// <param name="histogram"></param>
        /// <param name="mode"></param>
        internal void Clusterize(HistogramRGB histogram, ColorDistanceEvaluationMode mode)
        {
            foreach (var kvp in histogram.HistogramDictionary)
            {
                var bestEvolution = FindBestCluster(kvp.Key, mode);
                bestEvolution.HistDictionary.Add(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Execute a learning step to compute the next evolution of clusters based on the reference palette and selection mode.
        /// </summary>
        /// <param name="referencePalette"></param>
        /// <param name="mode"></param>
        /// <param name="distanceMode"></param>
        /// <returns></returns>
        internal List<int> ComputeNextEvolution(Palette referencePalette, ColorSelectionMode mode, ColorDistanceEvaluationMode distanceMode)
        {
            string sMethod = nameof(ComputeNextEvolution);
            int n = evolution.Count;
            var next = new int?[n];
            var fixedEntries = new HashSet<int>(referencePalette.RgbPalette);

            // Evaluate the fixed cluster evolution
            for (int i = 0; i < n; i++)
            {
                var evo = evolution[i];
                if (referencePalette.Contains(evo.GetClusterColor(i, distanceMode)))
                {
                    next[i] = evo.GetClusterColor(i, distanceMode);
                }
                else if (evo.HistDictionary.Count > 0)
                {
                    int rgbMean = ColorIntExt.GetColorMean(evo.HistDictionary, mode);
                    // evaluated RGB mean might coincide with a fixed entry.
                    // the cluster collapses
                    if (fixedEntries.Add(rgbMean))
                        next[i] = rgbMean;
                }
            }

            // Select populate dclusters
            var evoItems = evolution.Where(evo => evo.HistDictionary.Count > 1).ToList();
            // flatten all histograms, peek actual color and "error" as (histogram.count * distance (cluster color, hitstogram color))
            var evoCandidates = evoItems.SelectMany(e => e.HistDictionary
                 .Where(kvp => !fixedEntries.Contains(kvp.Key))
                 .Select(kvp => (Color: kvp.Key, Err: (double)kvp.Value * e.GetClusterColor(kvp.Key, distanceMode).Distance(kvp.Key, distanceMode)))).ToList();
            //     .Select(kvp => (Color: kvp.Key, Err: (double)kvp.Value * e.ClusterRGB.Distance(kvp.Key, mode)))).ToList();
            // creating a list of selectable candidates ordered by error
            var colorCandidates = evoCandidates.Where(t => t.Err > 0).OrderByDescending(t => t.Err).ToList();

            // 3. Reseed clusters without centroid
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
                    // Nessun candidato disponibile: tieni il centro precedente
                    next[i] = evolution[i].GetClusterColor(i, distanceMode);
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
