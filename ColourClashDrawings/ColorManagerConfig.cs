using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace ColourClashNet.Drawing
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ColorManagerConfig
    {

        int iColorsMax = 16;
        public int ColorsMax
        {
            get { return iColorsMax; }
            set
            {
                iColorsMax = Math.Max(2, Math.Min(256, value));
            }
        }

        public double SaturationEnhancement { get; set; } = 1;
        public double BrightnessEnhancement { get; set; } = 1;
        public double HsvHueOffset { get; set; } = 0;

        int iTrainLoop = 30;
        public int ClusteringTrainingLoop
        {
            get
            {
                return iTrainLoop;
            }
            set
            {
                iTrainLoop = Math.Max(1, value);
            }
        }

        public bool ClusteringUseMeanColor { get; set; } = true;
        public bool ScanlineClustering { get; set; } = true;
        public int ScanlineColorsMax { get; set; } = 7;
        public bool ScanlineSharedPalette { get; set; } = true;

        public ColorTransformType ColorTransformAlgorithm { get; set; } = ColorTransformType.None;
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.RGB;
        public ColorQuantizationMode ColorQuantizationMode { get; set; } = ColorQuantizationMode.RGB888;
        public ColorDithering DitheringAlgorithm { get; set; } = ColorDithering.FloydSteinberg;
        public double DitheringStrenght { get; set; } = 1.0;
        public List<int> BackgroundColorList { get; set; } = new List<int>();
        public int BackgroundColorReplacement { get; set; } = 0;
        public bool ZxEqAutotune { get; set; } = false;
        public int ZxEqColorLO { get; set; } = 0x80;
        public int ZxEqColorHI { get; set; } = 0xFF;
        public bool ZxEqBlackHI { get; set; } = true;
        public bool ZxEqDitherHI { get; set; } = true;
        public ColorTransformReductionAmiga.EnumAMigaVideoMode AmigaScreenMode { get; set; } = ColorTransformReductionAmiga.EnumAMigaVideoMode.Ham6;
        public ColorTransformReductionC64.C64VideoMode C64ScreenMode { get; set; } = ColorTransformReductionC64.C64VideoMode.Multicolor;
        public ColorTransformReductionCPC.CPCVideoMode CPCScreenMode { get; set; } = ColorTransformReductionCPC.CPCVideoMode.Mode0;

        public void SetProperties(ColorTransformInterface oTrasf)
        {
            oTrasf
                .SetProperty(ColorTransformProperties.ColorBackgroundList, BackgroundColorList)
                .SetProperty(ColorTransformProperties.Amiga_HamColorReductionMode, ColorTransformReductionAmiga.EnumHamFirstColorReductionMode.Fast)
                .SetProperty(ColorTransformProperties.Amiga_VideoMode, AmigaScreenMode)
                .SetProperty(ColorTransformProperties.C64_VideoMode, C64ScreenMode)
                .SetProperty(ColorTransformProperties.ClusterTrainingLoop, ClusteringTrainingLoop)

                .SetProperty(ColorTransformProperties.ColorBackgroundList, BackgroundColorList)
                .SetProperty(ColorTransformProperties.ColorBackgroundReplacement, BackgroundColorReplacement)
                .SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, ColorDistanceEvaluationMode)
                .SetProperty(ColorTransformProperties.CPC_VideoMode, CPCScreenMode)
                .SetProperty(ColorTransformProperties.Dithering_Type, DitheringAlgorithm)
                .SetProperty(ColorTransformProperties.Dithering_Strength, DitheringStrenght)

                //.SetProperty(ColorTransformProperties.Fixed_Palette, )
                .SetProperty(ColorTransformProperties.HsvBrightnessMultFactor, BrightnessEnhancement)
                .SetProperty(ColorTransformProperties.HsvHueShift, HsvHueOffset)
                .SetProperty(ColorTransformProperties.HsvSaturationMultFactor, SaturationEnhancement)
                .SetProperty(ColorTransformProperties.MaxColorChangePerLine, ScanlineColorsMax)
                .SetProperty(ColorTransformProperties.MaxColorsWanted, ColorsMax)
                .SetProperty(ColorTransformProperties.QuantizationMode, ColorQuantizationMode)
                .SetProperty(ColorTransformProperties.UseClustering, ScanlineClustering)
                //.SetProperty(ColorTransformProperties.UseColorMean, )
                //.SetProperty(ColorTransformProperties.UseFixedPalette, )
                .SetProperty(ColorTransformProperties.Zx_ColH, ZxEqColorHI)
                .SetProperty(ColorTransformProperties.Zx_ColL, ZxEqColorLO)
                .SetProperty(ColorTransformProperties.Zx_DitherHighColor, ZxEqDitherHI)
                .SetProperty(ColorTransformProperties.Zx_IncludeBlackInHighColor, ZxEqBlackHI)
                .SetProperty(ColorTransformProperties.Zx_Autotune, ZxEqAutotune);

                //.SetProperty(ColorTransformProperties.Zx_PaletteMode, ZxEqBlackHI)
        }
    }
}