using ColourClashNet;
using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColourClashLib.Color.Transformation.Evolution
{
    public interface IColorEvolution
    {

        /// <summary>
        /// The Histogram dictionary that maps colors to their respective counts within the cluster.
        /// </summary>
        Dictionary<int, int> HistDictionary { get; set; }

        /// <summary>
        /// The number of items (colors) in the cluster, derived from the histogram dictionary.
        /// </summary>
        int ClusterItems { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rgb"></param>
        void SetClusterColor(int rgb);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rgb"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        int GetClusterColor(int rgb, ColorDistanceEvaluationMode mode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rgb"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        double Distance(int rgb, ColorDistanceEvaluationMode mode);

    }
}
