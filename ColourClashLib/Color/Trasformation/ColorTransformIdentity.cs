using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformIdentity : ColorTransformBase
    {
        static string sC = nameof(ColorTransformIdentity);
        public ColorTransformIdentity()
        {
            Type = ColorTransformType.ColorIdentity;
            Description = "1:1 Color transformation";
        }

        // Not Needed
        // protected async override Task<ColorTransformResults> CreateTrasformationMapAsync(CancellationToken? oToken)

        protected override async Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken oToken=default)
        {
            string sM = nameof(ExecuteTransformAsync);
            try
            {
                var res = ColorTransformResults.CreateValidResult(SourceData, new ImageData().Create(SourceData));
                return await Task.FromResult(res);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"{Type}", ex);
                return new ColorTransformResults()
                {
                    Exception = ex,
                    Message = ex.Message
                };
            }
        }
    }
}
