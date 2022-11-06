using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorDitherIdentity : ColorDitherBase
    {
        static string sClass = nameof(ColorDitherIdentity);

        public ColorDitherIdentity()
        {
            Name = "Identity dithering";
            Description = "Passthrought";
        }

        public override  bool Create()
        {
            return true;
        }

        public override int[,]? Dither(int[,]? oDataOriginal, int[,]? oDataProcessed, List<int>? oDataProcessedPalette, ColorDistanceEvaluationMode eDistanceMode)
        {
            string sMethod = nameof(Dither);
            try
            {
                if (oDataOriginal == null)
                {
                    Trace.TraceError($"{sClass}.{sMethod} ({Name}) : Invalid input data");
                    return null;
                }
                Trace.TraceInformation($"{sClass}.{sMethod} ({Name}) : Dithering");
                var oRet = oDataProcessed.Clone() as int[,];
                Trace.TraceInformation($"{sClass}.{sMethod} ({Name}) : Dithering completed");
                return oRet;

            }
            catch (Exception ex)
            {
                Trace.TraceError($"{sClass}.{sMethod} ({Name}) : Exception raised : {ex.Message}");
                return null;
            }
        }
    }
}
