using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System.Collections;
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

    internal class ColorEvolution
    {
        static string sC = nameof(ColorEvolutionList);

        internal int ClusterColorRGB { get; set; } = ColorDefaults.DefaultInvalidColorInt;
        internal Dictionary<int, int> HistDictionary { get; set; } = new Dictionary<int, int>();
        internal int ClusterItems => HistDictionary.Count();
        internal int GetClusterWeight() => HistDictionary.Sum(X => X.Value);

        public override string ToString()
        {
            return $"0x{ClusterColorRGB:X8} : Count {ClusterItems}";
        }
    }

    internal class ColorEvolutionList : IEnumerable<ColorEvolution>
    {
        static string sC = nameof(ColorEvolutionList);

        internal List<ColorEvolution> lEvolution = new();

        internal void SetClusterColor(int rgb)
        {
            lEvolution.Add(new ColorEvolution()
            {
                ClusterColorRGB = rgb
            });
        }

        private ColorEvolution FindBestEvolution(int rgb, ColorDistanceEvaluationMode eMode)
        {
            ColorEvolution bestEvolution = null;
            double bestDistance = double.MaxValue;
            foreach (var evo in lEvolution)
            {
                double d = evo.ClusterColorRGB.Distance(rgb, eMode);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    bestEvolution = evo;
                }
            }
            return bestEvolution ?? new ColorEvolution();
        }

        internal ColorTransformationMap CreateTransformationMap(ImageData image, ColorDistanceEvaluationMode eMode)
        {
            ColorTransformationMap map = new ColorTransformationMap();
            foreach (var rgb in image.ColorPalette)
            {
                var bestEvolution = FindBestEvolution(rgb, eMode);
                map.Add(rgb, bestEvolution.ClusterColorRGB);
            }
            return map;
        }

        internal void Clusterize(HistogramRGB histogram, ColorDistanceEvaluationMode eMode)
        {
            foreach (var kvp in histogram.HistogramDictionary)
            {
                var bestEvolution = FindBestEvolution(kvp.Key, eMode);
                bestEvolution.HistDictionary.Add(kvp.Key, kvp.Value);
            }
        }

        internal List<int> ComputeNextEvolution(Palette referencePalette, ColorSelectionMode mode, ColorDistanceEvaluationMode eMode)
        {
            string sMethod = nameof(ComputeNextEvolution);
            int n = lEvolution.Count;
            var next = new int?[n];
            var fixedEntries = new HashSet<int>(referencePalette.rgbPalette);

            // Evaluate the fixed cluster evolution
            for (int i = 0; i < n; i++)
            {
                var evo = lEvolution[i];
                if (referencePalette.Contains(evo.ClusterColorRGB))
                {
                    next[i] = evo.ClusterColorRGB;
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
            var evoItems = lEvolution.Where(evo => evo.HistDictionary.Count > 1).ToList();
            // flatten all histograms, peek actual color and "error" as (histogram.count * distance (cluster color, hitstogram color))
            var evoCandidates = evoItems.SelectMany(e => e.HistDictionary
                 .Where(kvp => !fixedEntries.Contains(kvp.Key))
                 .Select(kvp => (Color: kvp.Key, Err: (double)kvp.Value * e.ClusterColorRGB.Distance(kvp.Key, eMode)))).ToList();
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
                    LogMan.Warning(sC, sMethod, $"Cluster 0x{lEvolution[i].ClusterColorRGB:X8} empty/duplicated, reseed with 0x{colorCandidates[ci].Color:X8}");
                    next[i] = colorCandidates[ci].Color;
                    fixedEntries.Add(colorCandidates[ci].Color);
                    ci++;
                }
                else
                {
                    // Nessun candidato disponibile: tieni il centro precedente
                    next[i] = lEvolution[i].ClusterColorRGB;
                }
            }

            return next.Select(x => x.Value).ToList();
        }

        public IEnumerator<ColorEvolution> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


}
