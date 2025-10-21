using ColourClashNet.Defaults;
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
using static ColourClashNet.Color.Transformation.ColorTransformReductionZxSpectrum;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionCluster : ColorTransformBase
    {
        static string sClass = nameof(ColorTransformReductionCluster);
        public int ColorsMaxWanted { get; set; } = -1;
        public bool UseClusterColorMean { get; set; } = true;
        public int TrainingLoop { get; set; } = -1;

        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            if (base.SetProperty(eProperty, oValue) != null)
                return this;
            switch (eProperty)
            {
                case ColorTransformProperties.MaxColorsWanted:
                    if (int.TryParse(oValue.ToString(), out var l))
                    {
                        ColorsMaxWanted = l;
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
            return null;
        }




        public ColorTransformReductionCluster()
        {
            Type = ColorTransformType.ColorReductionClustering;
            Description = "Reduces color bit spectrum";
        }

        protected override void CreateTrasformationMap()
        {
            string sMethod = nameof(CreateTrasformationMap);
            // Sort by most used colors
            var oTempHistogram = OutputHistogram.SortColorsDescending();
            // Creating a temporary palette with fixed colors and histogram colors
            var oTempPalette = Palette.MergeColorPalette(InputFixedColorPalette, oTempHistogram.ToColorPalette());
            // If we have less colors than wanted, just map them directly
            if (oTempPalette.Count < ColorsMaxWanted)
            {
                foreach (var kvp in OutputHistogram.rgbHistogram)
                {
                    OutputPalette.Add(kvp.Key);
                    ColorTransformationMapper.rgbTransformationMap[kvp.Key] = kvp.Key;
                }
                return;
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
            foreach (var rgb in oTempPalette.rgbPalette )
            {
                lTupleColorCluster.Add(Tuple.Create(new List<int> { rgb }, new Dictionary<int, int>()));
                if (++i == ColorsMaxWanted)
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
                foreach (var kvp in OutputHistogram.rgbHistogram )
                {
                    // For each color int the 
                    var dMin = lTupleColorCluster.Min(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode));
                    var oTupleCluster = lTupleColorCluster.FirstOrDefault(X => X.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode) == dMin);
                    oTupleCluster?.Item2.Add(kvp.Key, kvp.Value);
                };
                // Update the Color Mean for each cluster
                {
                    lTupleColorCluster.ForEach(oTuple =>
                    {
                        // If color is in FixedColorPalette, block evolution evolution
                        var iRgbMean = oTuple.Item1.Last();
                        if (InputFixedColorPalette?.rgbPalette.Any(X => X == iRgbMean) ?? false)
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
                }
            }

            OutputPalette = new Palette();
            foreach (var kvp in OutputHistogram.rgbHistogram )
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
                OutputPalette.Add(iCol);
                ColorTransformationMapper.Add(kvp.Key,iCol); 
            };
        }

    }
}
