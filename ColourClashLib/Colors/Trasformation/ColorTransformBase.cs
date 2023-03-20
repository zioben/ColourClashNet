using ColourClashLib.Color;
using ColourClashNet.Colors.Dithering;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace ColourClashNet.Colors.Transformation
{
    public abstract partial class ColorTransformBase : ColorTransformInterface
    {
        //---------------- Base description ---------------------------------
        public ColorTransform Type { get; protected init; }
        public string Description { get; protected set; } = "";

        //---------------- Source properties --------------------------------------
        public ColorHistogram SourceColorHistogram { get; protected set; } = new ColorHistogram();
        public ColorPalette SourceColorPalette { get; protected set; } = new ColorPalette();
        public int SourceColors => SourceColorPalette?.Colors ?? 0;

        //---------------- Transformation properties------------------------------
        public ColorHistogram ColorHistogram { get; protected set; } = new ColorHistogram();
        public ColorPalette ColorPalette { get; set; } = new ColorPalette();
        public int Colors => ColorPalette?.Colors ?? 0;
        public ColorTransformationMap ColorTransformationMap { get; protected set; } = new  ColorTransformationMap();

        //---------------- Useful objects ------------------------------
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.All;
        public DitherInterface? Dithering { get; set; } = null;
        protected bool BypassDithering { get; set; }
        protected abstract void CreateTrasformationMap();

        protected virtual int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            return ExecuteStdTransform(oDataSource,this);
        }

        void Reset()
        {
            ColorPalette.Reset();
            ColorHistogram.Reset();
            ColorTransformationMap.Reset();
        }


        public bool Create(int[,]? oDataSource )
        {
            Reset();
            if (oDataSource == null)
            {
                return false;
            }
            ColorHistogram.Create(oDataSource);
            ColorPalette = ColorHistogram.ToColorPalette();
            CreateTrasformationMap();
            return true;    
        }

        public bool Create(ColorHistogram oSourceColorHistogram)
        {
            Reset();
            if (oSourceColorHistogram == null )
            {
                return false;
            }
            foreach (var kvp in oSourceColorHistogram.rgbHistogram)
            {
                ColorHistogram.rgbHistogram.Add(kvp.Key, kvp.Value);
            }
            ColorPalette = ColorHistogram.ToColorPalette();
            CreateTrasformationMap();
            return true;    
        }

        public bool Create(ColorPalette oSourceColorPalette)
        {
            Reset();
            if (oSourceColorPalette == null)
            {
                return false;
            }
            foreach (var rgb in oSourceColorPalette.rgbPalette)
            {
                ColorHistogram.rgbHistogram.Add(rgb, 0);
            }
            ColorPalette = ColorHistogram.ToColorPalette();
            CreateTrasformationMap();
            return true;
        }

        public int[,]? TransformAndDither(int[,]? oDataSource)
        {
            if (oDataSource == null)
            {
                return null;
            }
            var oDataTrasf = ExecuteTransform(oDataSource);
            if (oDataTrasf == null || Dithering == null || BypassDithering)
            {
                return oDataTrasf;
            }
            var oh = new HashSet<int>();
            foreach (var rgb in oDataTrasf)
            {
                oh.Add(rgb);
            }
            if( oh.Count >= 256 ) 
            {
                return oDataTrasf;
            }
            var oProcDither = Dithering.Dither(oDataSource, oDataTrasf, ColorPalette, ColorDistanceEvaluationMode);
            return oProcDither;
        }

        static public double Error(int[,]? oDataA, int[,]? oDataB, ColorDistanceEvaluationMode eMode)
        {
            if (oDataA == null || oDataB == null)
            {
                return double.NaN;
            }
            int r1 = oDataA.GetLength(0);
            int c1 = oDataA.GetLength(1);
            int r2 = oDataB.GetLength(0);
            int c2 = oDataB.GetLength(1);
            if (r1 != r2 || c1 != c2)
            {
                return double.NaN;
            }
            double err = 0;
            for( int r=0; r<r1; r++ ) 
            {
                for (int c = 0; c < c1; c++)
                {
                    var rgb1 = oDataA[r, c];
                    var rgb2 = oDataB[r, c];
                    if( rgb1 >=0 && rgb2 >=0 ) 
                    {
                        err += ColorIntExt.Distance(rgb1, rgb2, eMode);
                    }
                }
            }
            return err; 
        }

    }
}
