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





        ImageData ToHam(ImageData dataSource, ImageData dataPreProcessed, ColorQuantizationMode quantization, CancellationToken token = default)
        {

            var quantizer = new ColorTransformQuantization();
            quantizer.QuantizationMode = quantization;
            quantizer.Create(dataPreProcessed);

            var hamData = new int[dataSource.Rows, dataSource.Columns];
            for (int r = 0; r < dataSource.Rows; r++)
            {
                int rgbPrev = ColorDefaults.DefaultInvalidColorInt; ;
                var palette = new Palette();
                token.ThrowIfCancellationRequested();
                for (int c = 0; c < dataSource.Columns; c++)
                {
                    if (rgbPrev < 0)
                    {
                        rgbPrev = dataPreProcessed.matrix[r, c];
                        hamData[r, c] = rgbPrev;
                        continue;
                    }
                    else
                    {
                        int rgbSrc = quantizer.QuantizeColor( dataSource.matrix[r, c] );
                        if (rgbSrc < 0)
                        {
                            rgbPrev = ColorDefaults.DefaultInvalidColorInt;
                            hamData[r, c] = rgbSrc;
                            continue;
                        }
                        int prevR = rgbPrev.ToR();
                        int prevG = rgbPrev.ToG();
                        int prevB = rgbPrev.ToB();
                        int srcR = rgbSrc.ToR();
                        int srcG = rgbSrc.ToG();
                        int srcB = rgbSrc.ToB();
                        int hamOriginal = quantizer.QuantizeColor( dataPreProcessed.matrix[r,c] );
                        int hamR = ColorIntExt.FromRGB(srcR, prevG, prevB);
                        int hamG = ColorIntExt.FromRGB(prevR, srcG, prevB);
                        int hamB = ColorIntExt.FromRGB(prevR, prevG, srcB);
                        palette.Reset();
                        palette.Add(hamOriginal);
                        palette.Add(hamR);
                        palette.Add(hamG);
                        palette.Add(hamB);
                        var col = quantizer.QuantizeColor( ColorIntExt.GetNearestColor(rgbSrc, palette, ColorDistanceEvaluationMode) );
                        rgbPrev = col;
                        hamData[r,c] = col;   
                    }
                }
            }
            return new ImageData().Create( hamData );
        }



        ImageData ToEHB(ImageData dataSource, ImageData dataPreProcessed, CancellationToken token = default)
        {

            var trans = new ColorTransformClustering()
                .WithClustering(config)
                .WithColorDistanceEvaluationMode(config)
                .WithDithering(DitheringConfig) as ColorTransformClustering;
            trans.MaxColorsWanted = 32;
            trans.ProcessAdvance += (s,e)=> RaiseProcessPartialEvent(e); 
            var res = trans.CreateAndProcessColors(dataPreProcessed, token);            
            return new ImageData().Create(res.DataOut);
        }


        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);

            ImageData? amigaData;

            var quantization = new ColorTransformQuantization();
            quantization.WithQuantization(ColorQuantizationMode.RGB444)
                         .WithReferencePalette(ReferencePalette)
                         .WithDithering(DitheringConfig);

            if (AmigaVideoMode == EnumAmigaVideoMode.ExtraHalfBright)
            {
                var resultQuantized = quantization.CreateAndProcessColors(ImageSource, token);
                amigaData = ToEHB(ImageSource, resultQuantized.DataOut, token);
            }
            else
            {

                int maxColors = 0;

                switch (AmigaVideoMode)
                {
                    case EnumAmigaVideoMode.Ham6:
                        maxColors = 16;
                        quantization.WithQuantization(ColorQuantizationMode.RGB444);
                        break;
                    case EnumAmigaVideoMode.Ham8:
                        maxColors = 64;
                        quantization.WithQuantization(ColorQuantizationMode.RGB666);
                        break;
                    default:
                        return null;
                }

                var quantizedResult = quantization.CreateAndProcessColors(ImageSource, token);

                var cfg = config.Clone()
                    .WithMaxColorWanted(maxColors);
                var colorReduction = ColorTransformInternal.Alloc(cfg, cfg.AmigaInternalTransformationModel) as ColorTransformBase;
                colorReduction.SetProperties(cfg);
                var dataPreprocessedResult = colorReduction.CreateAndProcessColors(quantizedResult.DataOut, token);
                BypassDithering = true;

                switch (AmigaVideoMode)
                {
                    case EnumAmigaVideoMode.Ham6:
                        amigaData = ToHam(ImageSource, dataPreprocessedResult.DataOut, ColorQuantizationMode.RGB444, token);
                        break;
                    case EnumAmigaVideoMode.Ham8:
                        amigaData = ToHam(ImageSource, dataPreprocessedResult.DataOut, ColorQuantizationMode.RGB666, token);
                        break;
                    default:
                        amigaData = null;
                        break;
                }
            }
            if (amigaData == null)
            {
                LogMan.Error(sC, sM, $"Failed to generate Amiga transformed data");
                return ColorTransformResult.CreateErrorResult(ImageSource,null, $"{sC}.{sM} : Amiga transformed data is null");
            }
            return ColorTransformResult.CreateValidResult(ImageSource, amigaData);
        }

    }
}
