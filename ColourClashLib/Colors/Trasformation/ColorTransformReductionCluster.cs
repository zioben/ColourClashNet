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
            Type = ColorTransform.ColorReductionClustering;
            Description = "Reduces color bit spectrum";
        }

        protected override void CreateTrasformationMap()
        {
            string sMethod = nameof(CreateTrasformationMap);
            ColorHistogram.SortColorsDescending();
            ColorPalette = ColorHistogram.ToColorPalette();
            if (ColorHistogram.rgbHistogram.Count < ColorsMax)
            {
                foreach (var kvp in ColorHistogram.rgbHistogram )
                {
                    ColorTransformationMap.Add(kvp.Key,kvp.Key);
                }
                ColorPalette = ColorTransformationMap.ToColorPalette();   
                return;
            }

            // Init Set
            // We need <List<ColorMeanOfTheCluster>,<Dictionary<ColorOfTheCluster,ColorOccurrences>>
            List<Tuple<List<int>, Dictionary<int, int>>> lColorCluster = new List<Tuple<List<int>, Dictionary<int, int>>>();

            // initial population of the cluster, with base max color occurrences 
            int i = 0;
            int iRGB = 0;
            foreach (var kvp in ColorHistogram.rgbHistogram )
            {
                lColorCluster.Add(Tuple.Create(new List<int> { kvp.Key }, new Dictionary<int, int>()));
                //lColorCluster.Add(Tuple.Create(new List<int> { iRGB }, new Dictionary<int, int>()));
                if (++i == ColorsMax)
                    break;
            }

            // Clustering training
            for (int train = 0; train < TrainingLoop; train++)
            {
                // Reset Set
                lColorCluster.ForEach(X => X.Item2.Clear()); ;
                Trace.TraceInformation($"{sClass}.{sMethod} ({Type}) : Train {train}");
                // Aggregate :  Assign every color to the cluster of appartenence 
                foreach (var kvp in ColorHistogram.rgbHistogram )
                {
                    // Evaluate minimum distance from every color to cluster
                    var dMin = lColorCluster.Min(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode));
                    var oTupleCluster = lColorCluster.FirstOrDefault(X => X.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode) == dMin);
                    oTupleCluster?.Item2.Add(kvp.Key, kvp.Value);
                };
                // Update the Color Mean for each cluster
                {
                    lColorCluster.ForEach(oTuple =>
                    {
                        var iRgbMean = ColorIntExt.GetColorMean(oTuple.Item2, ColorMeanMode.UseMean);
                        oTuple.Item1.Add(iRgbMean);
                    });
                }
            }

            ColorPalette = new ColorPalette();
            foreach (var kvp in ColorHistogram.rgbHistogram )
            {
                var dMin = lColorCluster.Min(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode));
                var oItem = lColorCluster.FirstOrDefault(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode) == dMin);
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
                ColorPalette.Add(iCol);
                ColorTransformationMap.Add(kvp.Key,iCol); // Item1.Last()??-1;
            };
        }

    }
}
