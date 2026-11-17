using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformCbmAmiga : ColorTransformBase
    {
        static string sC = nameof(ColorTransformCbmAmiga);


        public enum EnumAmigaVideoMode
        {
            //Bitplanes = 0,
            Ham6,
            Ham8,
            ExtraHalfBright
        }

        public EnumAmigaVideoMode AmigaVideoMode 
        { 
            get => config.AmigaVideoMode;
            set => config.AmigaVideoMode = value;
        }
            
        public  ColorTransformType HamColorTransformationModel
        { 
            get => config.AmigaInternalTransformationModel;
            set => config.AmigaInternalTransformationModel = value;
        }

        public ColorTransformCbmAmiga()
        {
            Type = ColorTransformType.ColorReductionClustering;
            Description = "Commododre Amiga specific color reduction";
        }


        public ColorTransformCbmAmiga WithAmigaProperties(EnumAmigaVideoMode videoMode, ColorTransformType hamColorTransformationModel )
        {
            AmigaVideoMode = videoMode;
            HamColorTransformationModel = hamColorTransformationModel;
            return this;
        }

        public ColorTransformCbmAmiga WithAmigaProperties(ColorTransformConfig cfg) => WithAmigaProperties(cfg.AmigaVideoMode, cfg.AmigaInternalTransformationModel);





        ImageData ToHam(ImageData oDataSource, ImageData oDataPreProcessed, ColorQuantizationMode eQuantization, CancellationToken token = default)
        {

            var oQuantization = new ColorTransformQuantization();
            oQuantization.QuantizationMode = eQuantization;
            oQuantization.Create(oDataPreProcessed);

            var oHamData = new int[oDataSource.Rows, oDataSource.Columns];
            for (int r = 0; r < oDataSource.Rows; r++)
            {
                int iRgbPrev = ColorDefaults.DefaultInvalidColorInt; ;
                var oPalette = new Palette();
                token.ThrowIfCancellationRequested();
                for (int c = 1; c < oDataSource.Columns; c++)
                {
                    if (iRgbPrev < 0)
                    {
                        iRgbPrev = oDataPreProcessed.matrix[r, c];
                        oHamData[r, c] = iRgbPrev;
                        continue;
                    }
                    else
                    {
                        int iRgbSrc = oQuantization.QuantizeColor( oDataSource.matrix[r, c] );
                        if (iRgbSrc < 0)
                        {
                            iRgbPrev = ColorDefaults.DefaultInvalidColorInt;
                            oHamData[r, c] = iRgbSrc;
                            continue;
                        }
                        int pR = iRgbPrev.ToR();
                        int pG = iRgbPrev.ToG();
                        int pB = iRgbPrev.ToB();
                        int sR = iRgbSrc.ToR();
                        int sG = iRgbSrc.ToG();
                        int sB = iRgbSrc.ToB();
                        int iHamO = oQuantization.QuantizeColor( oDataPreProcessed.matrix[r,c] );
                        int iHamR = ColorIntExt.FromRGB(sR, pG, pB);
                        int iHamG = ColorIntExt.FromRGB(pR, sG, pB);
                        int iHamB = ColorIntExt.FromRGB(pR, pG, sB);
                        oPalette.Reset();
                        oPalette.Add(iHamO);
                        oPalette.Add(iHamR);
                        oPalette.Add(iHamG);
                        oPalette.Add(iHamB);
                        var oCol = oQuantization.QuantizeColor( ColorIntExt.GetNearestColor(iRgbSrc, oPalette, ColorDistanceEvaluationMode) );
                        iRgbPrev = oCol;
                        oHamData[r,c] = oCol;   
                    }
                }
            }
            return new ImageData().Create( oHamData );
        }



        ImageData ToEHB(ImageData oDataSource, ImageData oDataPreProcessed, CancellationToken token = default)
        {

            var trans = new ColorTransformClustering()
                .WithClustering(config)
                .WithColorDistanceEvaluationMode(config)
                .WithDithering(DitheringConfig) as ColorTransformClustering;
            trans.MaxColorsWanted = 32;
            trans.ProcessAdvance += (s,e)=> RaiseProcessPartialEvent(e); 
            var res = trans.CreateAndProcessColors(oDataPreProcessed, token);            
            return new ImageData().Create(res.DataOut);
        }


        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);

            ImageData? oAmigaData;

            var oQuantization = new ColorTransformQuantization();
            oQuantization.WithQuantization(ColorQuantizationMode.RGB444)
                         .WithReferencePalette(ReferencePalette)
                         .WithDithering(DitheringConfig);

            if (AmigaVideoMode == EnumAmigaVideoMode.ExtraHalfBright)
            {
                var oResultQuantized = oQuantization.CreateAndProcessColors(ImageSource, token);
                oAmigaData = ToEHB(ImageSource, oResultQuantized.DataOut, token);
            }
            else
            {

                int iMaxColors = 0;

                switch (AmigaVideoMode)
                {
                    case EnumAmigaVideoMode.Ham6:
                        iMaxColors = 16;
                        oQuantization.WithQuantization(ColorQuantizationMode.RGB444);
                        break;
                    case EnumAmigaVideoMode.Ham8:
                        iMaxColors = 64;
                        oQuantization.WithQuantization(ColorQuantizationMode.RGB666);
                        break;
                    default:
                        return null;
                }

                var oResultQuantized = oQuantization.CreateAndProcessColors(ImageSource, token);

                var cfg = config.Clone()
                    .WithMaxColorWanted(iMaxColors);
                var oColorReduction = ColorTransformInternal.Alloc(cfg, cfg.AmigaInternalTransformationModel) as ColorTransformBase;
                oColorReduction.SetProperties(cfg);
                var oDataPreprocessedResult = oColorReduction.CreateAndProcessColors(oResultQuantized.DataOut, token);
                BypassDithering = true;

                switch (AmigaVideoMode)
                {
                    case EnumAmigaVideoMode.Ham6:
                        oAmigaData = ToHam(ImageSource, oDataPreprocessedResult.DataOut, ColorQuantizationMode.RGB444);
                        break;
                    case EnumAmigaVideoMode.Ham8:
                        oAmigaData = ToHam(ImageSource, oDataPreprocessedResult.DataOut, ColorQuantizationMode.RGB666);
                        break;
                    default:
                        oAmigaData = null;
                        break;
                }
            }
            if (oAmigaData == null)
            {
                LogMan.Error(sC, sM, $"Failed to generate Amiga transformed data");
                return ColorTransformResult.CreateErrorResult(ImageSource,null, $"{sC}.{sM} : Amiga transformed data is null");
            }
            return ColorTransformResult.CreateValidResult(ImageSource, oAmigaData);
        }

    }
}
