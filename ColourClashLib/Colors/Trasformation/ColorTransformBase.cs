using ColourClashLib.Color;
using ColourClashNet.Colors.Dithering;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public Dictionary<int, int> TransformationColorMap { get; protected set; } = new Dictionary<int, int>();
        public ColorPalette TransformationColorPalette { get; protected set; } = new ColorPalette();
        public int TransformationColors => TransformationColorPalette?.Colors ?? 0;

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
            SourceColorPalette.Reset();
            SourceColorHistogram.Reset();
            TransformationColorMap.Clear();
            TransformationColorPalette.Reset();
        }


        public bool Create(int[,]? oDataSource )
        {
            Reset();
            if (oDataSource == null)
            {
                return false;
            }
            SourceColorHistogram.Create(oDataSource);
            SourceColorPalette = SourceColorHistogram.ToColorPalette();
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
                SourceColorHistogram.rgbHistogram.Add(kvp.Key, kvp.Value);
            }
            SourceColorPalette = SourceColorHistogram.ToColorPalette();
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
                SourceColorHistogram.rgbHistogram.Add(rgb, 0);
            }
            SourceColorPalette = SourceColorHistogram.ToColorPalette();
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
            var oProcDither = Dithering.Dither(oDataSource, oDataTrasf, SourceColorPalette, ColorDistanceEvaluationMode);
            return oProcDither;
        }

    }
}
