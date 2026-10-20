using ColourClashNet.Imaging;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformCGA : ColorTransformFixedPalette
    {

        public enum CGAVideoMode
        {
            Mode4_0L,
            Mode4_0H,
            Mode4_1L,
            Mode4_1H,
            Mode5_1L,
            Mode5_1H,
            Mode6,
            RGBI,
            Composite_0,
            Composite_1,
            DebugPalette,
        }

        public enum CGAOutputMode
        {
            ProcessedImage,
            RowImage,
        }


        EncodingPatternDictionary<List<int>>  patternFull = new();
        EncodingPatternDictionary<List<int>> patternRGBI = new();
        EncodingPatternDictionary<List<int>> patternComposite0 = new();
        EncodingPatternDictionary<List<int>> patternComposite1 = new();
        EncodingPatternDictionary<List<int>> patternMode4_0L = new();
        EncodingPatternDictionary<List<int>> patternMode4_0H = new();
        EncodingPatternDictionary<List<int>> patternMode4_1L = new();
        EncodingPatternDictionary<List<int>> patternMode4_1H = new();
        EncodingPatternDictionary<List<int>> patternMode5_1L = new();
        EncodingPatternDictionary<List<int>> patternMode5_1H = new();
        EncodingPatternDictionary<List<int>> patternMode6 = new();



        public ColorTransformCGA()
        {
            Type = ColorTransformType.ColorReductionCGA;
            Description = "Reduce color to CGA palette";
            Processed += ColorTransformReductionCGA_Processed;
            CreatePalette();
        }

        private void ColorTransformReductionCGA_Processed(object? sender, ColorProcessingEventArgs e)
        {
            if (e.ProcessingResults.IsSuccess)
            {
                if (OutputMode == CGAOutputMode.RowImage)
                {
                    e.ProcessingResults.DataOut = CreateRowImage(e.ProcessingResults.DataOut, VideoMode);
                }
            }
        }

        private ImageData? CreateRowImage(ImageData? dataOut, CGAVideoMode videoMode)
        {
            throw new NotImplementedException();
        }

        public CGAVideoMode VideoMode
        { 
            get => config.CGAVideoMode;
            set => config.CGAVideoMode = value;
        }

        public CGAOutputMode OutputMode
        {
            get => config.CGAOutputMode;
            set => config.CGAOutputMode = value;
        }

        public ColorTransformCGA WithProcessingParams(CGAVideoMode value, CGAOutputMode outputMode)
        {
            OutputMode = outputMode;
            VideoMode = value;
            return this;
        }

        void CreatePalette()
        {
            patternFull.Reset()
                .AddPattern(0x00_00_00_00)
                .AddPattern(0x00_00_00_AA)
                .AddPattern(0x00_00_AA_00)
                .AddPattern(0x00_00_AA_AA)
                //
                .AddPattern(0x00_AA_00_00)
                .AddPattern(0x00_AA_00_AA)
                .AddPattern(0x00_AA_55_00)
                .AddPattern(0x00_AA_AA_AA)
                //
                .AddPattern(0x00_55_55_55)
                .AddPattern(0x00_55_55_FF)
                .AddPattern(0x00_55_FF_55)
                .AddPattern(0x00_55_FF_FF)
                //
                .AddPattern(0x00_FF_55_55)
                .AddPattern(0x00_FF_55_FF)
                .AddPattern(0x00_FF_FF_55)
                .AddPattern(0x00_FF_FF_FF);

            var pal = patternFull.GetPalette().ToList();

            patternMode4_0L.Reset()
                .AddPattern(pal[0])
                .AddPattern(pal[2])
                .AddPattern(pal[4])
                .AddPattern(pal[6]);

            patternMode4_0H.Reset()
                .AddPattern(pal[0])
                .AddPattern(pal[10])
                .AddPattern(pal[12])
                .AddPattern(pal[14]);

            patternMode4_1L.Reset()
                .AddPattern(pal[0])
                .AddPattern(pal[3])
                .AddPattern(pal[5])
                .AddPattern(pal[7]);

            patternMode4_1H.Reset()
                .AddPattern(pal[0])
                .AddPattern(pal[11])
                .AddPattern(pal[13])
                .AddPattern(pal[15]);

            patternMode5_1L.Reset()
                .AddPattern(pal[0])
                .AddPattern(pal[3])
                .AddPattern(pal[4])
                .AddPattern(pal[7]);

            patternMode5_1H.Reset()
                .AddPattern(pal[0])
                .AddPattern(pal[11])
                .AddPattern(pal[12])
                .AddPattern(pal[15]);

            patternMode6.Reset()
                .AddPattern(pal[0])
                .AddPattern(pal[15]);

            patternRGBI.Reset()
                .AddPattern(0x00_00_00_00)
                .AddPattern(0x00_00_6E_31)
                .AddPattern(0x00_31_09_FF)
                .AddPattern(0x00_00_8A_FF)
                //
                .AddPattern(0x00_A7_00_31)
                .AddPattern(0x00_76_76_76)
                .AddPattern(0x00_EC_11_FF)
                .AddPattern(0x00_BB_92_FF)
                //
                .AddPattern(0x00_31_5A_00)
                .AddPattern(0x00_00_DB_00)
                .AddPattern(0x00_76_76_76)
                .AddPattern(0x00_45_F7_BB)
                //
                .AddPattern(0x00_EC_63_00)
                .AddPattern(0x00_BB_E4_00)
                .AddPattern(0x00_FF_7F_BB)
                .AddPattern(0x00_FF_FF_FF);

            patternComposite0.Reset()
                .AddPattern(0x00_00_00_00)
                .AddPattern(0x00_00_71_D1)
                .AddPattern(0x00_00_19_AC)
                .AddPattern(0x00_00_71_F1)
                //
                .AddPattern(0x00_95_4F_00)
                .AddPattern(0x00_6D_D4_41)
                .AddPattern(0x00_A2_7B_1C)
                .AddPattern(0x00_74_D4_61)
                //     
                .AddPattern(0x00_B8_21_00)
                .AddPattern(0x00_90_A6_9C)
                .AddPattern(0x00_C5_4E_76)
                .AddPattern(0x00_97_A6_BB)
                //     
                .AddPattern(0x00_F3_68_00)
                .AddPattern(0x00_CB_ED_26)
                .AddPattern(0x00_FF_95_01)
                .AddPattern(0x00_D2_ED_46);

            patternComposite1.Reset()
                .AddPattern(0x00_00_00_00)
                .AddPattern(0x00_00_9A_FF)
                .AddPattern(0x00_00_42_FF)
                .AddPattern(0x00_00_90_FF)
                //
                .AddPattern(0x00_AA_4C_00)
                .AddPattern(0x00_84_FA_D2)
                .AddPattern(0x00_B9_A2_AD)
                .AddPattern(0x00_96_F0_FF)
                //     
                .AddPattern(0x00_CD_1F_00)
                .AddPattern(0x00_A7_CD_FF)
                .AddPattern(0x00_DC_75_FF)
                .AddPattern(0x00_B9_C3_FF)
                //     
                .AddPattern(0x00_FF_5C_00)
                .AddPattern(0x00_ED_FF_CC)
                .AddPattern(0x00_FF_B2_A6)
                .AddPattern(0x00_FF_FF_FF);
        }
       


        protected override void RebuildReferencePalette()
        {
            var refPalette = patternFull.GetPalette();
            switch (VideoMode)
            {
                case CGAVideoMode.Mode4_0L:
                    refPalette = patternMode4_0L.GetPalette();
                    break;
                case CGAVideoMode.Mode4_0H:
                    refPalette = patternMode4_0H.GetPalette();
                    break;
                case CGAVideoMode.Mode4_1L:
                    refPalette = patternMode4_1L.GetPalette();
                    break;
                case CGAVideoMode.Mode4_1H:
                    refPalette = patternMode4_1H.GetPalette();
                    break;
                case CGAVideoMode.Mode5_1L:
                    refPalette = patternMode5_1L.GetPalette();
                    break;
                case CGAVideoMode.Mode5_1H:
                    refPalette = patternMode5_1H.GetPalette();
                    break;
                case CGAVideoMode.Mode6:
                    refPalette = patternMode6.GetPalette();
                    break;
                case CGAVideoMode.RGBI:
                    refPalette = patternRGBI.GetPalette();
                    break;
                case CGAVideoMode.Composite_0:
                    refPalette = patternComposite0.GetPalette();
                    break;
                case CGAVideoMode.Composite_1:
                    refPalette = patternComposite1.GetPalette();
                    break;
                case CGAVideoMode.DebugPalette:
                    refPalette = patternFull.GetPalette();
                    break;
                default:
                    throw new NotImplementedException($"{VideoMode} Unsupported video mode");

            }
            WithReferencePalette(refPalette);
        }

        protected override ColorTransformResult CreateTransformationMap(CancellationToken oToken = default)
        {
            TransformationMap.Reset();
            RebuildReferencePalette();
            var rgbList = ImageSource.ColorPalette.ToList();
            foreach (var rgb in rgbList)
            {
                TransformationMap.Add(rgb, ColorIntExt.GetNearestColor(rgb, ReferencePalette, this.ColorDistanceEvaluationMode));
            }
            return ColorTransformResult.CreateValidResult();
        }

        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            switch (VideoMode)
            {
                case CGAVideoMode.Mode4_0L:
                case CGAVideoMode.Mode4_0H:
                case CGAVideoMode.Mode4_1L:
                case CGAVideoMode.Mode4_1H:
                case CGAVideoMode.Mode5_1L:
                case CGAVideoMode.Mode5_1H:
                case CGAVideoMode.Mode6:
                case CGAVideoMode.RGBI:
                    return base.ExecuteTransform(token);
                case CGAVideoMode.Composite_0:
                    return base.ExecuteTransform(token);
                case CGAVideoMode.Composite_1:
                    return base.ExecuteTransform(token);
                default:
                    return ColorTransformResult.CreateErrorResult($"Unsupported video mode {VideoMode}");
            }
        }
    }
}