using ColourClashNet;
using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColourClashLib.Color.Transformation.Evolution
{
    /// <summary>
    /// A single cluster of the K-means color reduction.
    /// </summary>
    public interface IColorEvolution
    {

        /// <summary>
        /// Colors assigned to the cluster, with their pixel counts.
        /// </summary>
        Dictionary<int, int> HistDictionary { get; set; }

        /// <summary>
        /// Number of distinct colors assigned to the cluster.
        /// </summary>
        int ClusterItems { get; }

        /// <summary>
        /// Sets the cluster color (centroid).
        /// </summary>
        void SetClusterColor(int rgb);

        /// <summary>
        /// Returns the cluster output color for <paramref name="rgb"/>; a cluster may expose more than one color (e.g. Amiga EHB).
        /// </summary>
        int GetClusterColor(int rgb, ColorDistanceEvaluationMode mode);

        /// <summary>
        /// Returns the distance between <paramref name="rgb"/> and the nearest color of the cluster.
        /// </summary>
        double Distance(int rgb, ColorDistanceEvaluationMode mode);

    }
}
