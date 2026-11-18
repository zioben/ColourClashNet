using ColourClashLib.Color.Transformation;
using ColourClashNet;
using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System.Collections;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Color.Transformation.Evolution
{

    /// <summary>
    /// Represents a color evolution cluster.
    /// </summary>
    public class ColorEvolution : IColorEvolution
    {


        public string sC = nameof(ColorEvolution);

        /// <summary>
        /// The Cluster RGB color representing the centroid of the cluster.
        /// </summary>
        public int ClusterRGB { get; private set; } = ColorDefaults.DefaultInvalidColorInt;
      
        /// <summary>
        /// The Histogram dictionary that maps colors to their respective counts within the cluster.
        /// </summary>
        public Dictionary<int, int> HistDictionary { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// The number of items (colors) in the cluster, derived from the histogram dictionary.
        /// </summary>
        public int ClusterItems => HistDictionary.Count();
        public double Distance(int rgb, ColorDistanceEvaluationMode mode) => ClusterRGB.Distance(rgb, mode);
        public int GetClusterColor(int rgb, ColorDistanceEvaluationMode mode) => ClusterRGB;
        public void SetClusterColor(int rgb) => ClusterRGB = rgb;

        public override string ToString()
        {
            return $"0x{ClusterRGB:X8} : Count {ClusterItems}";
        }
    }
}
