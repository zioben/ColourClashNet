using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionCGA : ColorTransformReductionPalette
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
            Composite_1,
            Composite_2,
            DebugPalette,
        }

        List<int> paletteFull = new List<int>
            {
                    0x00_00_00_00,
                    0x00_00_00_AA,
                    0x00_00_AA_00,
                    0x00_00_AA_AA,
                    //
                    0x00_AA_00_00,
                    0x00_AA_00_AA,
                    0x00_AA_55_00,
                    0x00_AA_AA_AA,
                    //
                    0x00_55_55_55,
                    0x00_55_55_FF,
                    0x00_55_FF_55,
                    0x00_55_FF_FF,
                    //
                    0x00_FF_55_55,
                    0x00_FF_55_FF,
                    0x00_FF_FF_55,
                    0x00_FF_FF_FF,
            };

        List<int> paletteRGBI = new List<int>()
        {
            0x00_00_00_00,
            0x00_00_6E_31,
            0x00_31_09_FF,
            0x00_00_8A_FF,
            //
            0x00_A7_00_31,
            0x00_76_76_76,
            0x00_EC_11_FF,
            0x00_BB_92_FF,
            //
            0x00_31_5A_00,
            0x00_00_DB_00,
            0x00_76_76_76,
            0x00_45_F7_BB,
            //
            0x00_EC_63_00,
            0x00_BB_E4_00,
            0x00_FF_7F_BB,
            0x00_FF_FF_FF,
        };
        List<int> paletteComposite_1 = new List<int>()
        {
            0x00_00_00_00,
            0x00_00_71_D1,
            0x00_00_19_AC,
            0x00_00_71_F1,
            //
            0x00_95_4F_00,
            0x00_6D_D4_41,
            0x00_A2_7B_1C,
            0x00_74_D4_61,
            //     
            0x00_B8_21_00,
            0x00_90_A6_9C,
            0x00_C5_4E_76,
            0x00_97_A6_BB,
            //     
            0x00_F3_68_00,
            0x00_CB_ED_26,
            0x00_FF_95_01,
            0x00_D2_ED_46,
        };

        List<int> paletteComposite_2 = new List<int>()
        {
            0x00_00_00_00,
            0x00_009AFF,
            0x00_0042FF,
            0x00_0090FF,
            //
            0x00_AA_4C_00,
            0x00_84_FA_D2,
            0x00_B9_A2_AD,
            0x00_96_F0_FF,
            //     
            0x00_CD_1F_00,
            0x00_A7_CD_FF,
            0x00_DC_75_FF,
            0x00_B9_C3_FF,
            //     
            0x00_FF_5C_00,
            0x00_ED_FF_CC,
            0x00_FF_B2_A6,
            0x00_FF_FF_FF,
        };

        List<int> paletteMode4_0L = new List<int>();
        List<int> paletteMode4_0H = new List<int>();
        List<int> paletteMode4_1L = new List<int>();
        List<int> paletteMode4_1H = new List<int>();
        List<int> paletteMode5_1L = new List<int>();
        List<int> paletteMode5_1H = new List<int>();

        List<int> paletteMode6 = new List<int>()
        {
            0x00_00_00_00,
            0x00_FF_FF_FF
        };


        public ColorTransformReductionCGA()
        {
            Type = ColorTransformType.ColorReductionCGA;
            Description = "Reduce color to CGA palette";
            CreatePalette();
        }

        public CGAVideoMode VideoMode
        { 
            get => config.CGAVideoMode;
            set => config.CGAVideoMode = value;
        }

        public ColorTransformReductionCGA WithCgaVideoMode(CGAVideoMode value)
        {
            VideoMode = value;
            return this;
        }

        void CreatePalette()
        {
            paletteMode4_0L = new List<int>()
            {
                paletteFull[0],
                paletteFull[2],
                paletteFull[4],
                paletteFull[6],
            };

            paletteMode4_0H = new List<int>()
            {
                paletteFull[0],
                paletteFull[10],
                paletteFull[12],
                paletteFull[14],
            };

            paletteMode4_1L = new List<int>()
            {
                paletteFull[0],
                paletteFull[3],
                paletteFull[5],
                paletteFull[7],
            };

            paletteMode4_1H = new List<int>()
            {
                paletteFull[0],
                paletteFull[11],
                paletteFull[13],
                paletteFull[15],
            };

            paletteMode5_1L = new List<int>()
            {
                paletteFull[0],
                paletteFull[3],
                paletteFull[4],
                paletteFull[7],
            };

            paletteMode5_1H = new List<int>()
            {
                paletteFull[0],
                paletteFull[11],
                paletteFull[12],
                paletteFull[15],
            };


        }

        protected override void RebuildReferencePalette()
        {
            var refPalette = paletteFull;
            switch (VideoMode)
            {
                case CGAVideoMode.Mode4_0L:
                    refPalette = paletteMode4_0L;
                    break;
                case CGAVideoMode.Mode4_0H:
                    refPalette = paletteMode4_0H;
                    break;
                case CGAVideoMode.Mode4_1L:
                    refPalette = paletteMode4_1L;
                    break;
                case CGAVideoMode.Mode4_1H:
                    refPalette = paletteMode4_1H;
                    break;
                case CGAVideoMode.Mode5_1L:
                    refPalette = paletteMode5_1L;
                    break;
                case CGAVideoMode.Mode5_1H:
                    refPalette = paletteMode5_1H;
                    break;
                case CGAVideoMode.Mode6:
                    refPalette = paletteMode6;
                    break;
                case CGAVideoMode.RGBI:
                    refPalette = paletteRGBI;
                    break;
                case CGAVideoMode.Composite_1:
                    refPalette = paletteComposite_1;
                    break;
                case CGAVideoMode.Composite_2:
                    refPalette = paletteComposite_2;
                    break;
                case CGAVideoMode.DebugPalette:
                    refPalette = paletteFull;
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
                case CGAVideoMode.Composite_1:
                    return base.ExecuteTransform(token);
                case CGAVideoMode.Composite_2:
                    return base.ExecuteTransform(token);
                default:
                    return ColorTransformResult.CreateErrorResult($"Unsupported video mode {VideoMode}");
            }
        }
    }
}