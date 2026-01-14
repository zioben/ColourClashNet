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
        public int[,]? Data { get; private set; }

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
                if (Data == null) return 0;
                return Data.GetLength(1);
            }
        }

        /// <summary>
        /// gets the number of rows in the data set.
        /// </summary>
        public int Height
        {
            get
            {
                if (Data == null) return 0;
                return Data.GetLength(0);
            }
        }

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
        public bool DataValid => Data != null;
        



        /// <summary>
        ///     
        /// </summary>
        /// <param name="oData"></param>
        /// <param name="oToken"></param>
        /// <returns></returns>
        public ImageData? Create(int[,]? oData, CancellationToken oToken = default)
        {
            string sM = nameof(Create);
            try
            {
                lock (locker)
                {
                    Destroy();
                    if (oData == null)
                    {
                        return null;
                    }
                    Data = oData.Clone() as int[,];                    
                    ColorPalette = Palette.CreateColorPalette(Data);
                }
                return this;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, "Error creating from data.", ex);
                return null;
            }
        }

       
        /// <summary>
        /// Asynchronously creates a new entity from the specified image.
        /// </summary>
        /// <param name="oImage">The image to use for creation. Must not be null.</param>
        /// <param name="oToken">An optional cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the creation
        /// succeeds; otherwise, <see langword="false"/>.</returns>
        public ImageData? Create(System.Drawing.Image oImage, CancellationToken oToken = default)
        {
            string sM = nameof(Create);
            try
            {
                var oImageRaw = ImageTools.GdiImageToMatrix(oImage as System.Drawing.Bitmap);
                return Create(oImageRaw, oToken);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, "Error creating from GDI.", ex);
                return null;
            }
        }

        /// <summary>
        /// Asynchronously creates an object from the specified image file.
        /// </summary>
        /// <remarks>If the specified file does not exist, the method returns <see langword="false"/>
        /// without performing any operation.</remarks>
        /// <param name="sFileName">The path to the image file to use for creation. The file must exist.</param>
        /// <param name="oToken">An optional cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the object
        /// was created successfully; otherwise, <see langword="false"/>.</returns>
        public async ImageData? Create(string sFileName, CancellationToken oToken = default)
        {
            string sM = nameof(Create);
            try
            {
                using (var oBitmap = ImageTools.GdiImageFromFile(sFileName))
                {
                    return Create(oBitmap, oToken);
                }
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, "Error creating from file.", ex);
                return false;
            }
        }

        /// <summary>
        /// Asynchronously releases resources and resets the object to its initial state.
        /// </summary>
        /// <returns>A completed task that represents the asynchronous destroy operation.</returns>
        public bool Destroy()
        {
            string sM = nameof(Destroy);
            try
            {
                lock (locker)
                {
                    Data = null;
                    ColorPalette = new Palette();
                    ColorHistogram = new Histogram();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC,sM, ex);
                return false;
            }
        }

        /// <summary>
        /// Saves the current image data to a file in the specified format.
        /// </summary>
        /// <param name="sFileName">The path and file name where the image will be saved. Cannot be null or empty.</param>
        /// <param name="eFormat">The format in which to save the image file.</param>
        /// <returns>true if the image is saved successfully; otherwise, false.</returns>
        public bool Save(string sFileName, ImageExportFormat eFormat)
        {
            string sM = nameof(Save);
            try
            {
                lock (locker)
                {
                    using (var oBmp = ImageTools.MatrixToGdiImage(Data))
                    {
                        return ImageTools.GdiImageToFile(oBmp, sFileName, eFormat);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, "Error saving to file.", ex);
                return false;
            }
        }

        /// <summary>
        /// Converts the current image data to a new System.Drawing.Image instance.
        /// </summary>
        /// <remarks>The caller is responsible for disposing the returned Bitmap when it is no longer
        /// needed.</remarks>
        /// <returns>A Bitmap object representing the current image data.</returns>
        public System.Drawing.Image? ToImage()
        {
            string sM = nameof(Save);
            try
            {
                lock (locker)
                {
                    return ImageTools.MatrixToGdiImage(Data);
                }
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, "Error Creating GDI Bitmap.", ex);
                return null;
            }
        }
    }
}
