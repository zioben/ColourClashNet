using ColourClashLib;
using ColourClashNet.Color.Conversion;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
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
    public class ColorTransformClustering : ColorTransformBase
    {
        static string sC = nameof(ColorTransformClustering);
        public int MaxColorsWanted 
        { 
            get => config.MaxColorsWanted;
            set => config.MaxColorsWanted = value;  
        }
        public ColorSelectionMode ClusterColorSelectionMode
        {
            get => config.ClusterColorSelectionMode;
            set => config.ClusterColorSelectionMode = value;
        }
      
        public int TrainingLoop 
        {
            get => config.ClusterTrainingLoop;
            set => config.ClusterTrainingLoop = value;
        }


        public ColorTransformClustering()
        {
            Type = ColorTransformType.ColorReductionClustering;
            Description = "K-Means color reduction";
        }

        public ColorTransformClustering WithProcessingParams(int maxColorsWanted, int trainingLoop, ColorSelectionMode clusterColorSelectionMode)
        {
            MaxColorsWanted = maxColorsWanted;
            TrainingLoop = trainingLoop;
            ClusterColorSelectionMode = clusterColorSelectionMode;
            return this;
        }

        public ColorTransformClustering WithClustering(ColorTransformConfig cfg) =>
         WithProcessingParams(cfg.MaxColorsWanted, cfg.ClusterTrainingLoop, cfg.ClusterColorSelectionMode);





        protected override ColorTransformResult CreateTransformationMap(CancellationToken token = default)
        {
            string sMethod = nameof(CreateTransformationMap);
            Random random = new Random(1);

            // Creating initial color seeds for the clustering algorithm
            var tempImageHistogram = new HistogramRGB().Create(ImageSource).SortColorsDescending();
            var tempImagePalette = ImageSource.ColorPalette;// tempImageHistogram.ToPalette();
            var tempPalette = Palette.MergePalette(ReferencePalette, tempImagePalette);
            // If got less colors than wanted, just map them directly
            if (tempPalette.Count <= MaxColorsWanted)
            {
                foreach (var rgb in tempPalette)
                {
                    TransformationMap.Add(rgb, rgb);
                }
                return ColorTransformResult.CreateValidResult();
            }
            // Create a startup palette for color evolution - Random colors
            else
            {
                tempPalette = new Palette().Create(ReferencePalette);
                while (tempPalette.Count < MaxColorsWanted)
                {
                    tempPalette.Add(ColorIntExt.FromRGB(255.0 * random.NextDouble(), 255.0 * random.NextDouble(), 255.0 * random.NextDouble()));
                }
            }
            // Init Clustering Algorithm
            // Got a Tuple of List<ColorMeanOfTheCluster>, Dictionary<ColorOfTheCluster,ColorOccurrences> for each cluster
            // where
            // -List<ColorMeanOfTheCluster> is RGB, starting from the initial palette color, represents the color evolution.
            // -Dictionary<ColorOfTheCluster,ColorOccurrences> is the set of colors assigned to this cluster
            // Evolution behaviour
            // 1: for each histogram color, assign the histogram color to the best cluster color to a dictionary<color, occurrences>.
            // 2: avaluate the next cluster color:
            //    2.1 use color mean -> next cluster color is cluster center mass.
            //    2.3 use histogram  -> next cluster color is the best rapresentative color in the palette.
            // After every cycle the Tuple<List<int>,...> contains the evolution sequence of the cluster while the dictionary is resetted to recompute the cluster  
            //
            // Initial population of the cluster, with base max color occurrences.
            // Using tuple instead of dedicated class because tuple is enough
            // List<Tuple<List<RGB>,Dictionary<RGB,Occurrences>>>
            //List<Tuple<List<int>, Dictionary<int, int>>> colorClusterList = new List<Tuple<List<int>, Dictionary<int, int>>>();
            // Need to consider the fixed color list, since this colors can't evolve.
            var rgbFixedList = ReferencePalette.ToList();

            // Prepare the evolutions
            List<ColorEvolutionList> lEvolutions = new();
            var rgbFixedSet = new HashSet<int>(ReferencePalette);

            for (int train = 0; train < TrainingLoop; train++)
            {
                LogMan.Trace(sC, sMethod, $"{Type} : Training loop {train}");

                var lNextEvolution = new ColorEvolutionList();
                if (lEvolutions.Count == 0)
                {
                    foreach (var rgb in tempPalette)
                        lNextEvolution.SetClusterColor(rgb);
                }
                else
                {
                    var lLastEvolution = lEvolutions.Last();
                    var lNextClusterColors = lLastEvolution.ComputeNextEvolution(ReferencePalette, ClusterColorSelectionMode, ColorDistanceEvaluationMode);
                    foreach (var rgb in lNextClusterColors )
                    {
                        lNextEvolution.SetClusterColor(rgb);
                    }
                }

                lNextEvolution.Clusterize(tempImageHistogram, ColorDistanceEvaluationMode);
                lEvolutions.Add(lNextEvolution);

                var oArgs = new ColorProcessingEventArgs()
                {
                    ColorTransformInterface = this,
                    ProcessingResults = ColorTransformResult.CreateValidResult(ImageSource, null, $"Loop {train}/{TrainingLoop}"),
                    CompletedPercent = 100 * (train + 1) / TrainingLoop
                };
                if (EnablePreviewEvents)
                {
                    var map = lNextEvolution.CreateTransformationMap(ImageSource, ColorDistanceEvaluationMode);
                    oArgs.ProcessingResults.DataOut = map.Transform(ImageSource, token);
                }
                RaiseProcessPartialEvent(oArgs);
            }           
            TransformationMap = lEvolutions.Last().CreateTransformationMap(ImageSource, ColorDistanceEvaluationMode);
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