using ColourClashNet.Color;
using ColourClashNet.Defaults;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Imaging
{
    [Serializable]
       
    public partial class ImageData
    {
        public string Name { get; set; } = "";
        public int[,]? Data { get; set; }
        public Palette ColorPalette { get; set; } = new Palette();
        public Histogram ColorHistogram { get; set; } = new Histogram();

        public int Width
        {
            get
            {
                if (Data == null) return 0;
                return Data.GetLength(0);
            }
        }

        public int Height
        {
            get
            {
                if (Data == null) return 0;
                return Data.GetLength(1);
            }
        }

        public int PixelCount
        {
            get
            {
                return Width * Height;
            }
        }

        public int Colors
        {
            get
            {
                return ColorPalette.Count;
            }
        }

        
        public bool Reset()
        {
            Data = null;
            ColorPalette = new Palette();
            ColorHistogram = new Histogram();
            return true;
        }


        public async Task<bool> SetDataAsync(int[,] oData, CancellationToken? oToken)
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

        public async Task<bool> SetDataAsync(System.Drawing.Image oImage, CancellationToken? oToken)
        {
            string sM = nameof(SetDataAsync);
            if (oImage == null)
            {
                return false;
            }
            var oImageRaw = ImageTools.BitmapToMatrix(oImage as System.Drawing.Bitmap);
            return await SetDataAsync(oImageRaw, oToken);
        }

        public async Task<bool> SetDataAsync(string sFileName, CancellationToken? oToken)
        {
            string sM = nameof(SetDataAsync);
            if (!File.Exists(sFileName))
            {
                return false;
            }
            var oBitmap = ImageTools.BitmapFromFile(sFileName);
            return await SetDataAsync(oBitmap, oToken);
        }

        public bool Save(string sFileName, ImageExportFormat eFormat)
        {
            string sM = nameof(Save);
            var oBmp = ImageTools.MatrixToBitmap(Data);
            return ImageTools.BitmapToFile(oBmp, sFileName, eFormat);
        }
    }
}
