using ColourClashLib;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Color.Transformation.ColorTransformReductionC64;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionCluster : ColorTransformBase
    {
        static string sClass = nameof(ColorTransformReductionCluster);
        public int MaxColorsWanted 
        { 
            get => config.MaxColorsWanted;
            set => config.MaxColorsWanted = value;  
        }
        public bool UseClusterColorMean 
        { 
            get => config.UseColorMean;
            set => config.UseColorMean = value;
        } 
        
        public int TrainingLoop 
        {
            get => config.ClusterTrainingLoop;
            set => config.ClusterTrainingLoop = value;
        }

      
        public ColorTransformReductionCluster()
        {
            Type = ColorTransformType.ColorReductionClustering;
            Description = "K-Means color reduction";
        }

        public ColorTransformReductionCluster WithProcessingParams(int maxColorsWanted, int trainingLoop, bool useClusterColorMean)
        {
            MaxColorsWanted = maxColorsWanted;
            TrainingLoop = trainingLoop;
            UseClusterColorMean = useClusterColorMean;
            return this;
        }

        public ColorTransformReductionCluster WithClustering(ColorTransformConfig cfg) =>
         WithProcessingParams(cfg.MaxColorsWanted, cfg.ClusterTrainingLoop, cfg.UseColorMean);


        ColorTransformationMap CreateTransformationMap( HistogramRGB histogram, List<Tuple<List<int>, Dictionary<int, int>>> colorClusterList)
        {
            ColorTransformationMap map = new ColorTransformationMap();
            foreach (var kvp in histogram.HistogramDictionary)
            {
                var distanceMin = colorClusterList.Min(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode));
                var item = colorClusterList.FirstOrDefault(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode) == distanceMin);
                var rgb = ColorDefaults.DefaultInvalidColorInt; ;
                if (UseClusterColorMean)
                {
                    rgb = item?.Item1.Last() ?? ColorDefaults.DefaultInvalidColorInt; ;
                }
                else
                {
                    if (item?.Item2.Count > 0)
                    {
                        var maxOccurrences = item?.Item2.Max(X => X.Value);
                        rgb = item?.Item2.FirstOrDefault(X => X.Value == maxOccurrences).Key ?? ColorDefaults.DefaultInvalidColorInt; ;
                    }
                    else
                    {
                        rgb = ColorDefaults.DefaultInvalidColorInt;
                    }
                }
                map.Add(kvp.Key, rgb);
            }
            return map;
        }

        protected override ColorTransformResult CreateTransformationMap(CancellationToken token = default)
        {
            string sMethod = nameof(CreateTransformationMap);
            // Creating initial color seeds for the clustering algorithm
            // Sort by most used colors from original image, to have the most used colors as initial cluster seeds
            // Creating a temporary palette with fixed colors and histogram colors
            var tempHistogram = new HistogramRGB().Create(ImageSource).SortColorsDescending();
            var tempPalette = Palette.MergePalette(ReferencePalette, tempHistogram.ToPalette());
            // If got less colors than wanted, just map them directly
            if (tempPalette.Count <= MaxColorsWanted)
            {
                foreach (var rgb in tempPalette)
                {
                    TransformationMap.Add(rgb, rgb);
                }
                return ColorTransformResult.CreateValidResult();
            }

            // Init Clustering Algorithm
            // Got a Tuple of List<ColorMeanOfTheCluster>, Dictionary<ColorOfTheCluster,ColorOccurrences> for each cluster
            // List<ColorMeanOfTheCluster> is the evolution of the cluster color, starting from the initial color
            // Dictionary<ColorOfTheCluster,ColorOccurrences> is the set of colors assigned to the cluster
            //
            // For each cluster, we need to keep track of the evolution of the cluster color mean, and the set of colors assigned to the cluster
            // For each mean color we add a dictionary representing the colors assigned to the cluster, and their occurrences in the original image
            // After obtaining this information we use the dictionary to calculate the next color mean on the next tuple, then we repeat the process for the number of training loops specified
            // At every cycle the Tuple<List<int>,...> contains the evolution sequence of the cluster while the dictionary is resetted to recompute the cluster  
            //
            // Initial population of the cluster, with base max color occurrences.
            // Using tuple instead of dedicated class because tuple is enough
            List<Tuple<List<int>, Dictionary<int, int>>> colorClusterList = new List<Tuple<List<int>, Dictionary<int, int>>>();
            // This list of color cannot evolve, they are fixed in the cluster and will be used as reference for the other colors to evolve
            // So, reference palette colors are first included and fixed. Remaininig colow can evolve the color mean.
            var rgbFixedList = ReferencePalette.ToList();
    
            int i = 0;
            foreach (var rgb in tempPalette)
            {
                colorClusterList.Add(Tuple.Create(new List<int> { rgb }, new Dictionary<int, int>()));
                if (++i == MaxColorsWanted)
                {
                    break;
                }
            }

            // Clustering training
            // For each loop, assign every color to the nearest cluster, then recalculate the cluster mean
            for (int train = 0; train < TrainingLoop; train++)
            {
                LogMan.Trace(sClass, sMethod, $"{Type} : Training loop {train}");
                // Clear previous cluster assignment
                colorClusterList.ForEach(X => X.Item2.Clear());
                // Aggregate part : Assign every color to the best cluster of appartenence 
                foreach (var kvp in tempHistogram.HistogramDictionary)
                {
                    // For each color int the cluster
                    var minDistance = colorClusterList.Min(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode));
                    //var oTupleCluster = lTupleColorCluster.FirstOrDefault(X => X.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode) == dMin);
                    //oTupleCluster?.Item2.Add(kvp.Key, kvp.Value);
                    var tupleClusterSelected = colorClusterList.Where(X => X.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode) == minDistance);
                    foreach (var tuple in tupleClusterSelected)
                    {
                        tuple.Item2.Add(kvp.Key, kvp.Value);
                    }
                }
                ;
                // Update the Color Mean for each cluster. Get evolving color mean for each cluster, and add it to the list of evolving colors for the cluster
                var colorClusterFixedList = colorClusterList.Where(X => !rgbFixedList.Any( Y => X.Item1.Last() == Y )).ToList();
                colorClusterFixedList.ForEach(tuple =>
                {
                    if (tuple.Item2.Count > 0)
                    {
                        var rgbMean = ColorIntExt.GetColorMean(tuple.Item2, ColorMeanMode.UseMean);
                        tuple.Item1.Add(rgbMean);
                    }
                    else
                    {
                        // Might that no cluster is assigned to a evolving color because the distance from this cluster is too high.
                        // the color can be re-engaged during next evolutions
                        var rgb = tuple.Item1.Last();
                        LogMan.Warning(sClass, sMethod, $"Color {rgb} did't evolved, replug it");
                        tuple.Item1.Add(rgb);
                    }
                });

                var oArgs = new ColorProcessingEventArgs()
                    {
                        ColorTransformInterface = this,
                        ProcessingResults = ColorTransformResult.CreateValidResult(ImageSource, null, $"Loop {train}/{TrainingLoop}"),
                        CompletedPercent = 100 * (train + 1) / TrainingLoop
                    };
                if (EnablePreviewEvents)
                {
                    var map = CreateTransformationMap(tempHistogram, colorClusterList);
                    oArgs.ProcessingResults.DataOut = map.Transform(ImageSource, token); 
                }
                RaiseProcessPartialEvent(oArgs);
            }

            TransformationMap = CreateTransformationMap(tempHistogram, colorClusterList);
            return ColorTransformResult.CreateValidResult();


        }


        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            
                var ret = TransformationMap.Transform(ImageSource, token);
                if (ret != null)
                {
                    return ColorTransformResult.CreateValidResult(ImageSource, ret);
                }
                return new();
        }
    }
}