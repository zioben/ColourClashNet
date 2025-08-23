using ColourClashLib;
using ColourClashLib.Color;
using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static ColourClashNet.Colors.Transformation.ColorTransformReductionZxSpectrum;

namespace ColourClashNet.Colors.Transformation
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
            OutputHistogram.SortColorsDescending();
            //FixedColorPalette = new ColorPalette();
            //FixedColorPalette.Add(0x00000000);
            //FixedColorPalette.Add(0x00ff0000);
            //FixedColorPalette.Add(0x0000ff00);
            //FixedColorPalette.Add(0x000000ff);
            //FixedColorPalette.Add(0x00ffffff);
            var oTempPalette = ColorPalette.MergeColorPalette(InputFixedColorPalette, OutputHistogram.ToColorPalette());
            if (oTempPalette.Count < ColorsMaxWanted)
            {
                foreach (var kvp in OutputHistogram.rgbHistogram)
                {
                    OutputPalette.Add(kvp.Key);
                    ColorTransformationMapper.rgbTransformationMap[kvp.Key] = kvp.Key;
                }
                return;
            }

            // Init Set
            // We need <List<ColorMeanOfTheCluster>,<Dictionary<ColorOfTheCluster,ColorOccurrences>>
            // ColorReference, with evolution -> Cluster of colors
            // ytryt
            List<Tuple<List<int>, Dictionary<int, int>>> lTupleColorCluster = new List<Tuple<List<int>, Dictionary<int, int>>>();

            // initial population of the cluster, with base max color occurrences 
            int i = 0;
            int iRGB = 0;           
            foreach (var rgb in oTempPalette.rgbPalette )//colorHistogram.rgbHistogram )
            {
                lTupleColorCluster.Add(Tuple.Create(new List<int> { rgb }, new Dictionary<int, int>()));
                if (++i == ColorsMaxWanted)
                    break;
            }

            // Clustering training
            for (int train = 0; train < TrainingLoop; train++)
            {
                // Reset Set
                lTupleColorCluster.ForEach(X => X.Item2.Clear()); ;
                if (ColorDefaults.Trace) 
                    Trace.TraceInformation($"{sClass}.{sMethod} ({Type}) : Train {train}");
                // Aggregate :  Assign every color to the cluster of appartenence 
                foreach (var kvp in OutputHistogram.rgbHistogram )
                {
                    // Evaluate minimum distance from every color to cluster
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
                            if (ColorDefaults.Trace)
                                Trace.TraceInformation($"{sClass}.{sMethod} ({Type}) : Color {iRgbMean} is fixed, skipping evolution");
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

            OutputPalette = new ColorPalette();
            foreach (var kvp in OutputHistogram.rgbHistogram )
            {
                var dMin = lTupleColorCluster.Min(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode));
                var oItem = lTupleColorCluster.FirstOrDefault(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode) == dMin);
                var iCol = -1;
                if (UseClusterColorMean)
                {
                    iCol = oItem?.Item1.Last() ?? -1;
                }
                else
                {
                    var Max = oItem?.Item2.Max(X => X.Value);
                    iCol = oItem?.Item2.FirstOrDefault(X => X.Value == Max).Key ?? -1;
                }
                OutputPalette.Add(iCol);
                ColorTransformationMapper.Add(kvp.Key,iCol); // Item1.Last()??-1;
            };
        }

    }
}
