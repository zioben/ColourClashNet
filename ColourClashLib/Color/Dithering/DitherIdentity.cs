using ColourClashNet.Color;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Dithering
{
    public class DitherIdentity : DitherBase
    {
        static string sClass = nameof(DitherIdentity);

        public DitherIdentity()
        {
            Type = ColorDithering.None;
            Description = "Passthrought";
        }

        public override bool Create()
        {
            return true;
        }

        public override async Task<ImageData?> DitherAsync(ImageData oImageReference, ImageData oImageReduced, ColorDistanceEvaluationMode eDistanceMode, CancellationToken? oToken)
        {
            return await Task.Run(() =>
            {
                string sMethod = nameof(DitherAsync);
                try
                {
                    if (oImageReduced == null)
                    {
                        LogMan.Error(sClass, sMethod, "Invalid input data");
                        return null;
                    }
                    LogMan.Trace(sClass, sMethod, $"{Type} : Dithering is a simple clone");
                    return new ImageData().Create(oImageReduced);

                }
                catch (Exception ex)
                {
                    LogMan.Exception(sClass, sMethod, ex);
                    return null;
                }
            });
        }
    }
}
