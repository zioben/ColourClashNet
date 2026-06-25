using ColourClashNet.Color.Transformation;
using ColourClashNet.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformBkgRemover : ColorTransformBase
    {
        static string sC = nameof(ColorTransformBkgRemover);
        public ColorTransformBkgRemover()
        {
            Type = ColorTransformType.ColorRemover;
            Description = "Basic Background Color Replacement";
        }

        public Palette BackgroundPalette { get; set; } = new Palette();
        public int ColorBackgroundReplacement { get; set; } = 0;

        public ColorTransformInterface WithColorReplacement( Palette colorPalette, int replacementColor)
        {
            BackgroundPalette = new Palette().Create(colorPalette);
            ColorBackgroundReplacement = replacementColor;
            return this;
        }

        public ColorTransformInterface WithColorReplacement(ColorTransformConfig cfg) => WithColorReplacement(cfg.ColorBackgroundList, cfg.ColorBackgroundReplacement);

        public override ColorTransformInterface SetProperties(ColorTransformConfig cfg)
        {
            base.SetProperties(cfg);
            return WithColorReplacement(cfg);
        }

        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
           
                string sM = nameof(CreateTransformationMap);
                LogMan.Trace(sC, sM, $"{Type} : Creating trasformation map");

                TransformationMap.Reset();
                var oBkgList = BackgroundPalette.ToList();
                var oPalList = ImageSource.ColorPalette.ToList();
                foreach (var rgb in oPalList)
                {
                    TransformationMap.Add(rgb, rgb);
                }
                foreach (var rgb in oBkgList)
                {
                    TransformationMap.Remove(rgb);
                    TransformationMap.Add(rgb, ColorBackgroundReplacement);
                }
            return base.ExecuteTransform(token);// ColorTransformResult.CreateValidResult();
        }
    }
}
