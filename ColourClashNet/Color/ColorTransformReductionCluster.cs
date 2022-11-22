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

namespace ColourClashNet.Colors
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
            SortColorsByHistogram();
            if (ColorHistogram.Count < ColorsMax)
            {
                foreach (var kvp in ColorHistogram)
                {
                    ColorTransformationMap[kvp.Key] = kvp.Key;
                    ColorTransformationPalette.Add(kvp.Key);
                }
                return;
            }

            // Init Set
            // We need <List<ColorMeanOfTheCluster>,<Dictionary<ColorOfTheCluster,ColorOccurrences>>
            List<Tuple<List<int>, Dictionary<int,int>>> lColorCluster = new List<Tuple<List<int>, Dictionary<int,int>>>();

            // initial population of the cluster, with base max color occurrences 
            int i = 0;            
            foreach (var kvp in ColorHistogram)
            {
                lColorCluster.Add(Tuple.Create(new List<int> { kvp.Key }, new Dictionary<int, int>()));
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
                foreach ( var kvp in  ColorHistogram )
                {
                    // Evaluate minimum distance from every color to cluster
                    var dMin = lColorCluster.Min(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode));
                    var oTupleCluster = lColorCluster.FirstOrDefault(X => X.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode) == dMin);
                    oTupleCluster?.Item2.Add(kvp.Key,kvp.Value);
                };
                // Update the Color Mean for each cluster
                {
                    lColorCluster.ForEach( oTuple =>
                    {
                        int iMean = -1;
                        if (oTuple.Item2.Count() > 0)
                        {
                            int Count = 0;
                            double R = 0;
                            double G = 0;
                            double B = 0;
                            foreach( var kvp in oTuple.Item2)
                            {
                                Count += kvp.Value;
                                R += kvp.Value * kvp.Key.ToR();
                                G += kvp.Value * kvp.Key.ToG();
                                B += kvp.Value * kvp.Key.ToB();
                            };
                            R /= Count;
                            G /= Count;
                            B /= Count;
                            iMean = ColorIntExt.FromRGB(R, G, B);
                        }
                        oTuple.Item1.Add(iMean);
                    });
                }
            }

            foreach( var kvp in ColorHistogram )
            {                
                var dMin = lColorCluster.Min(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode));
                var oItem = lColorCluster.FirstOrDefault(Y => Y.Item1.Last().Distance(kvp.Key, ColorDistanceEvaluationMode) == dMin);
                var iCol = -1;
                if (UseClusterColorMean)
                {
                    iCol = oItem?.Item1.Last()??-1;
                }
                else
                {
                    var Max = oItem?.Item2.Max(X => X.Value);
                    iCol = oItem?.Item2.FirstOrDefault(X=>X.Value==Max).Key ?? -1;
                }
                ColorTransformationPalette.Add(iCol);
                ColorTransformationMap[kvp.Key] = iCol; // Item1.Last()??-1;
            };
        }

    }
}
