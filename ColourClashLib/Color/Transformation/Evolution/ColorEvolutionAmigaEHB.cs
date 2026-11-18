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
    /// Cluster for the Amiga Extra Half-Brite mode: each palette color also provides a half-bright copy.
    /// </summary>
    public class ColorEvolutionAmigaEHB : ColorEvolution
    {
        static string sC = nameof(ColorEvolutionAmigaEHB);
     
        /// <summary>
        /// Half-bright version of the cluster color (each channel divided by 2).
        /// </summary>
        public int ClusterRGB_EHB => toEHB();
        /// <summary>
        /// Computes the half-bright color, or an invalid color when the cluster has no color.
        /// </summary>
        int toEHB()
        {
            if (ClusterRGB == ColorDefaults.DefaultInvalidColorInt)
                return ColorDefaults.DefaultInvalidColorInt;
            return ColorIntExt.FromRGB(ClusterRGB.ToR() / 2, ClusterRGB.ToG() / 2, ClusterRGB.ToB() / 2);
        }
      
        /// <summary>
        /// Distance to the nearest of the cluster color and its half-bright copy.
        /// </summary>
        public override double Distance(int rgb, ColorDistanceEvaluationMode mode)
        {
            return Math.Min(ClusterRGB.Distance(rgb, mode), ClusterRGB_EHB.Distance(rgb, mode));
        }

        /// <summary>
        /// Returns the nearest of the cluster color and its half-bright copy.
        /// </summary>
        public override int GetClusterColor(int rgb, ColorDistanceEvaluationMode mode)
        {
            if(ClusterRGB == ColorDefaults.DefaultInvalidColorInt)
                return ColorDefaults.DefaultInvalidColorInt;
            if( ClusterRGB.Distance(rgb, mode) <= ClusterRGB_EHB.Distance(rgb, mode))
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
