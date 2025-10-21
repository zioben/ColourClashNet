using ColourClashNet.Color;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Color.Trasformation
{
    public class DataContainer
    {
        public int[,]? Data { get; internal set; }

        public Palette ColorPalette { get; internal set; } = new();

        public Histogram ColorHistogram { get; internal set; } = new();

        public int Colors => ColorPalette.Count;

        public void Reset()
        {
            Data = null;
            ColorHistogram.Reset();
            ColorPalette.Reset();
        }

        public async Task<bool> SetDataAsync( int[,] oData, CancellationToken? oToken)
        {
            string sM = nameof(SetDataAsync);
            if (oData == null)
            {
                return false;
            }
            Data = oData.Clone() as int[,];
            ColorHistogram = await Histogram.CreateColorHistogramAsync(oData, oToken);
            ColorPalette = ColorHistogram.ToColorPalette();
            return true;
        }

        public async Task<bool> SetColorHistogramAsync(int[,]? oData, CancellationToken? oToken)
        {
            Reset();
            if (oData == null)
            {
                return false;
            }
            ColorHistogram = await Histogram.CreateColorHistogramAsync(oData, oToken);
            ColorPalette = ColorHistogram.ToColorPalette();
            return true;
        }

        public async Task<bool> SetColorHistogramAsync(Histogram? oHistogram, CancellationToken? oToken)
        {
            Reset();
            if (oHistogram == null)
            {
                return false;
            }
            await ColorHistogram.CreateAsync(oHistogram, oToken);
            return true;
        }

        //public bool SetColorHistogram(Histogram? oHist)
        //{
        //    var cts = new CancellationTokenSource();
        //    return  SetColorHistogramAsync(oHist, cts.Token).GetAwaiter().GetResult();
        //}

        //public bool SetColorHistogram(int[,]? oData)
        //{
        //    var cts = new CancellationTokenSource();
        //    return SetColorHistogramAsync(oData, cts.Token).GetAwaiter().GetResult();
        //}

        public async Task<bool> SetColorPaletteAsync(Palette? oColorPalette)
        {
            Reset();
            if (oColorPalette == null)
            {
                return false;
            }
            ColorPalette = Palette.CreateColorPalette(oColorPalette);
            ColorHistogram.Reset();
            return await Task.FromResult( true);
        }
    }
}
