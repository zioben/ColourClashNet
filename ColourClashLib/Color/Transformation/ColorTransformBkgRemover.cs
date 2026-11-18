using ColourClashNet.Color.Transformation;
using ColourClashNet.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Replaces every color of <see cref="BackgroundPalette"/> with <see cref="ColorBackgroundReplacement"/>; other colors are kept.
    /// </summary>
    public class ColorTransformBkgRemover : ColorTransformBase
    {
        static string sC = nameof(ColorTransformBkgRemover);
        /// <summary>
        /// Creates the background remover.
        /// </summary>
        public ColorTransformBkgRemover()
        {
            Type = ColorTransformType.ColorRemover;
            Description = "Basic Background Color Replacement";
        }

        /// <summary>
        /// Colors treated as background.
        /// </summary>
        public Palette BackgroundPalette
        {
            get => config.ColorBackgroundPalette;
            set => config.ColorBackgroundPalette = value ?? new Palette();
        }

        /// <summary>
        /// Color that replaces the background colors.
        /// </summary>
        public int ColorBackgroundReplacement 
        { 
            get => config.ColorBackgroundReplacementValue;
            set => config.ColorBackgroundReplacementValue = value;
        } 

        /// <summary>
        /// Sets the background colors and their replacement (fluent).
        /// </summary>
        public ColorTransformInterface WithColorReplacement( Palette colorPalette, int replacementColor)
        {
            BackgroundPalette = new Palette().Create(colorPalette);
            ColorBackgroundReplacement = replacementColor;
            return this;
        }

        /// <summary>
        /// Sets the background colors and their replacement from a configuration (fluent).
        /// </summary>
        public ColorTransformInterface WithColorReplacement(ColorTransformConfig cfg) => WithColorReplacement(cfg.ColorBackgroundPalette, cfg.ColorBackgroundReplacementValue);


        /// <summary>
        /// Builds an identity map with the background colors redirected, then applies it.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
           
                string sM = nameof(ExecuteTransform);
                LogMan.Trace(sC, sM, $"{Type} : Creating transformation map");

                TransformationMap.Reset();
                var bkgList = BackgroundPalette.ToList();
                var sourceList = ImageSource.ColorPalette.ToList();
                foreach (var rgb in sourceList)
                {
                    TransformationMap.Add(rgb, rgb);
                }
                foreach (var rgb in bkgList)
                {
                    TransformationMap.Remove(rgb);
                    TransformationMap.Add(rgb, ColorBackgroundReplacement);
                }
            return base.ExecuteTransform(token);
        }
    }
}
