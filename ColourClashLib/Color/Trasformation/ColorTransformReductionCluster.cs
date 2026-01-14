using ColourClashNet.Defaults;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionCluster : ColorTransformBase
    {
        static string sClass = nameof(ColorTransformReductionCluster);
        public int MaxColorsWanted { get; set; } = -1;
        public bool UseClusterColorMean { get; set; } = true;
        public int TrainingLoop { get; set; } = -1;

      
        public ColorTransformReductionCluster()
        {
            Type = ColorTransformType.ColorReductionClustering;
            Description = "Reduces color bit spectrum";
        }

        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            base.SetProperty(eProperty, oValue);
            switch (eProperty)
            {
                case ColorTransformProperties.MaxColorsWanted:
                    if (int.TryParse(oValue.ToString(), out var l))
                    {
                        MaxColorsWanted = l;
                        return this;
                    }
                    break;
                case ColorTransformProperties.ClusterTrainingLoop:
                    if (int.TryParse(oValue?.ToString(), out var d))
                    {
                        TrainingLoop = d;
                        return this;
                    }
                    break;
                case ColorTransformProperties.UseColorMean:
                    if (bool.TryParse(oValue.ToString(), out var h))
                    {
                        UseClusterColorMean = h;
                        return this;
                    }
                    break;
                default:
                    break;
            }
            return this;
        }


        ColorTransformationMap CreateTransformationMap( Histogram oTempHistogram, List<Tuple<List<int>, Dictionary<int, int>>> lTupleColorCluster)
        {
            ColorTransformationMap map = new ColorTransformationMap();
            foreach (var kvp in oTempHistogram.rgbHistogram)
            {
                var dMin = lTupleColorCluster.Min(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode));
                var oItem = lTupleColorCluster.FirstOrDefault(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode) == dMin);
                var iCol = ColorDefaults.DefaultInvalidColorInt; ;
                if (UseClusterColorMean)
                {
                    iCol = oItem?.Item1.Last() ?? ColorDefaults.DefaultInvalidColorInt; ;
                }
                else
                {
                    var Max = oItem?.Item2.Max(X => X.Value);
                    iCol = oItem?.Item2.FirstOrDefault(X => X.Value == Max).Key ?? ColorDefaults.DefaultInvalidColorInt; ;
                }
                map.Add(kvp.Key, iCol);
            }
            return map;
        }

        protected async override Task<ColorTransformResults> CreateTrasformationMapAsync(CancellationToken? oToken)
        {
            string sMethod = nameof(CreateTrasformationMapAsync);
            // Sort by most used colors
            var oTempHistogram = SourceHistogram.SortColorsDescending();
            // Creating a temporary palette with fixed colors and histogram colors
            var oTempPalette = Palette.MergeColorPalette(FixedPalette, oTempHistogram.ToColorPalette());
            // If we have less colors than wanted, just map them directly
            if (oTempPalette.Count <= MaxColorsWanted)
            {                
                foreach (var rgb in oTempPalette.rgbPalette)
                {
                    TransformationMap.Add(rgb, rgb);
                }
                return ColorTransformResults.CreateValidResult();
            }

            // Init Clustering Algorithm 
            // We need the Tuple <List<ColorMeanOfTheCluster>,<Dictionary<ColorOfTheCluster,ColorOccurrences>>
            // List<ColorMeanOfTheCluster> is the evolution of the cluster color, starting from the initial color
            // Dictionary<ColorOfTheCluster,ColorOccurrences> is the set of colors assigned to the cluster
            List<Tuple<List<int>, Dictionary<int, int>>> lTupleColorCluster = new List<Tuple<List<int>, Dictionary<int, int>>>();

            // initial population of the cluster, with base max color occurrences 
            // Starting with the fixed palette, then with the most used colors
            int i = 0;
            int iRGB = 0;
            foreach (var rgb in oTempPalette.rgbPalette)
            {
                lTupleColorCluster.Add(Tuple.Create(new List<int> { rgb }, new Dictionary<int, int>()));
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
                lTupleColorCluster.ForEach(X => X.Item2.Clear());
                // Aggregate part : Assign every color to the cluster of appartenence 
                foreach (var kvp in oTempHistogram.rgbHistogram)
                {
                    // For each color int the cluster
                    var dMin = lTupleColorCluster.Min(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode));
                    var oTupleCluster = lTupleColorCluster.FirstOrDefault(X => X.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode) == dMin);
                    oTupleCluster?.Item2.Add(kvp.Key, kvp.Value);
                };
                // Update the Color Mean for each cluster
                lTupleColorCluster.ForEach(oTuple =>
                {
                    // If color is in FixedColorPalette, block evolution evolution
                    var iRgbMean = oTuple.Item1.Last();
                    if (FixedPalette?.rgbPalette.Any(X => X == iRgbMean) ?? false)
                    {
                        LogMan.Trace(sClass, sMethod, $"{Type} : Color {iRgbMean} is fixed, skipping evolution");
                    }
                    // else evaluate color mean
                    else
                    {
                        iRgbMean = ColorIntExt.GetColorMean(oTuple.Item2, ColorMeanMode.UseMean);
                    }
                    oTuple.Item1.Add(iRgbMean);
                });

                // Show partials
                if (ProcessPartialEventRegistered)
                {
                    if (FastPreview)
                    {
                        RaiseProcessPartialEvent(new ColorTransformEventArgs()
                        {
                            ColorTransformInterface = this,
                            Message = $"Loop {train}/{TrainingLoop}",
                            ProcessingResults = ColorTransformResults.CreateValidResult(SourceData, null),
                            CompletedPercent = 100 * (train + 1) / TrainingLoop
                        });
                    }
                    else
                    {
                        var map = CreateTransformationMap(oTempHistogram, lTupleColorCluster);
                        var oTempData = await map.TransformAsync(SourceData, oToken);
                        RaiseProcessPartialEvent(new ColorTransformEventArgs()
                        {
                            ColorTransformInterface = this,
                            Message = $"Loop {train}/{TrainingLoop}",
                            ProcessingResults = ColorTransformResults.CreateValidResult(SourceData, oTempData),
                            TempImage = oTempData,
                            TransformationMap = map,
                            CompletedPercent = 100.0 * (train + 1) / TrainingLoop
                        });
                    }
                }
            }


            TransformationMap = CreateTransformationMap(oTempHistogram, lTupleColorCluster);
            return ColorTransformResults.CreateValidResult();


        }


        protected override async Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken? oToken)
        {
            var ret = await TransformationMap.TransformAsync(SourceData, oToken);
            if (ret != null)
            {
                return ColorTransformResults.CreateValidResult(SourceData, ret);    
            }
            return new();
        }
    }
}