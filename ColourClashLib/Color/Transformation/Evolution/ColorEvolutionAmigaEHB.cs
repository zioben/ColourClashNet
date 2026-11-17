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
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Color.Transformation.Evolution
{


    /// <summary>
    /// Represents a color evolution cluster.
    /// </summary>
    public class ColorEvolutionAmigaEHB : ColorEvolution
    {
        static string sC = nameof(ColorEvolutionAmigaEHB);
     
        /// <summary>
        /// The Cluster RGB color representing the centroid of the cluster.
        /// </summary>
        public int ClusterRGB_EHB => toEHB();
        int toEHB()
        {
            if (ClusterRGB == ColorDefaults.DefaultInvalidColorInt)
                return ColorDefaults.DefaultInvalidColorInt;
            return ColorIntExt.FromRGB(ClusterRGB.ToR() / 2, ClusterRGB.ToG() / 2, ClusterRGB.ToB() / 2);
        }
      
        public new double Distance(int rgb, ColorDistanceEvaluationMode eMode)
        {
            return Math.Min(ClusterRGB.Distance(rgb, eMode), ClusterRGB_EHB.Distance(rgb, eMode));
        }

        public new int GetClusterColor(int rgb, ColorDistanceEvaluationMode eMode)
        {
            if(ClusterRGB == ColorDefaults.DefaultInvalidColorInt)
                return ColorDefaults.DefaultInvalidColorInt;
            if( ClusterRGB.Distance(rgb, eMode) <= ClusterRGB_EHB.Distance(rgb, eMode))
                return ClusterRGB;
            else
                return ClusterRGB_EHB;
        }

        public override string ToString()
        {
           return $"0xMAIN:{ClusterRGB:X8} EHB:{ClusterRGB_EHB:X8} : Count {ClusterItems}";
        }
    }
 }
