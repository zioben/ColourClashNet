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

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionCluster : ColorTransformBase
    {
        static string sClass = nameof(ColorTransformReductionCluster);
        public int ColorsMax { get; set; } = -1;
        public bool UseClusterColorMean { get; set; } = true;
        public int TrainingLoop { get; set; } = -1;

        public ColorTransformReductionCluster()
        {
            type = ColorTransform.ColorReductionClustering;
            description = "Reduces color bit spectrum";
        }

        protected override void CreateTrasformationMap()
        {
            string sMethod = nameof(CreateTrasformationMap);
            colorHistogram.SortColorsDescending();
            //FixedColorPalette = new ColorPalette();
            //FixedColorPalette.Add(0x00000000);
            //FixedColorPalette.Add(0x00ff0000);
            //FixedColorPalette.Add(0x0000ff00);
            //FixedColorPalette.Add(0x000000ff);
            //FixedColorPalette.Add(0x00ffffff);
            var oTempPalette = ColorPalette.MergeColorPalette(FixedColorPalette, colorHistogram.ToColorPalette());
            if (oTempPalette.Colors < ColorsMax)
            {
                foreach (var kvp in colorHistogram.rgbHistogram)
                {
                    colorPalette.Add(kvp.Key);
                    colorTransformationMap.rgbTransformationMap[kvp.Key] = kvp.Key;
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
                if (++i == ColorsMax)
                    break;
            }

            // Clustering training
            for (int train = 0; train < TrainingLoop; train++)
            {
                // Reset Set
                lTupleColorCluster.ForEach(X => X.Item2.Clear()); ;
                if (ColorDefaults.Trace) 
                    Trace.TraceInformation($"{sClass}.{sMethod} ({type}) : Train {train}");
                // Aggregate :  Assign every color to the cluster of appartenence 
                foreach (var kvp in colorHistogram.rgbHistogram )
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
                        if (FixedColorPalette?.rgbPalette.Any(X => X == iRgbMean) ?? false)
                        {
                            if (ColorDefaults.Trace)
                                Trace.TraceInformation($"{sClass}.{sMethod} ({type}) : Color {iRgbMean} is fixed, skipping evolution");
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

            colorPalette = new ColorPalette();
            foreach (var kvp in colorHistogram.rgbHistogram )
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
                colorPalette.Add(iCol);
                colorTransformationMap.Add(kvp.Key,iCol); // Item1.Last()??-1;
            };
        }

    }
}
