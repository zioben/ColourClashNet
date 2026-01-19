using ColourClashNet.Color;
using ColourClashNet.Defaults;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Imaging
{
    
    public partial class ImageData
    {
        static string sC = nameof(ImageData);

        object locker = new object();

        /// <summary>
        /// Gets or sets the name associated with the object.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets the two-dimensional array of integer data associated with this instance.
        /// </summary>
        public int[,]? DataX { get; private set; }

        /// <summary>
        /// Gets the color palette used for rendering visual elements.
        /// </summary>
        public Palette ColorPalette { get; private set; } = new Palette();


        /// <summary>
        /// Gets the number of columns in the data set.
        /// </summary>
        public int Width
        {
            get
            {
                if (DataX == null) return 0;
                return DataX.GetLength(1);
            }
        }

        /// <summary>
        /// gets the number of rows in the data set.
        /// </summary>
        public int Height
        {
            get
            {
                if (DataX == null) return 0;
                return DataX.GetLength(0);
            }
        }

        /// <summary>
        /// Gets the number of columns in the data set.
        /// </summary>
        public int Columns => Width;

        /// <summary>
        /// gets the number of rows in the data set.
        /// </summary>
        public int Rows =>Height;

        /// <summary>
        /// Gets the total number of pixels in the image.
        /// </summary>
        public int PixelCount
        {
            get
            {
                return Width * Height;
            }
        }

        /// <summary>
        /// Gets the number of colors in the current color palette.
        /// </summary>
        public int Colors
        {
            get
            {
                return ColorPalette.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current object contains valid data.
        /// </summary>
        public bool DataValid => DataX != null;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Reset()
        {
            string sM = nameof(Reset);
            try
            {
                lock (locker)
                {
                    DataX = null;
                    ColorPalette = new Palette();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return false;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="oData"></param>
        /// <returns></returns>
        public ImageData Create(int[,]? oData)
        {
            string sM = nameof(Create);
            try
            {
                lock (locker)
                {
                    Reset();
                    if (oData == null)
                    {
                        return this;
                    }
                    DataX = oData.Clone() as int[,];                    
                    ColorPalette = Palette.CreatePalette(DataX);
                }
                return this;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, "Error creating from data.", ex);
                return this;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oImageData"></param>
        /// <returns></returns>
        public ImageData Create(ImageData oImageData) 
            => Create(oImageData?.DataX);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"ImageData: {Name}, {Width}x{Height}, Colors: {Colors}";
        }
       
    }
}
