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
     
        /// <summary>
        /// Creates a new ImageData instance containing a cropped region from the specified source image.
        /// </summary>
        /// <remarks>If the specified region extends beyond the bounds of the source image, the resulting
        /// image may be partially empty or padded. The method does not modify the original source image.</remarks>
        /// <param name="imageSrc">The source ImageData from which to extract the cropped region. Cannot be null.</param>
        /// <param name="xs">The x-coordinate, in pixels, of the upper-left corner of the region to crop from the source image.</param>
        /// <param name="ys">The y-coordinate, in pixels, of the upper-left corner of the region to crop from the source image.</param>
        /// <param name="width">The width, in pixels, of the cropped region.</param>
        /// <param name="height">The height, in pixels, of the cropped region.</param>
        /// <returns>A new ImageData instance containing the specified cropped region of the source image.</returns>
        public static ImageData CreateImageData(ImageData imageSrc, int xs, int ys, int width, int height)
        {
            string sM = nameof(CreateImageData);    
            var imageDst = new ImageData().Create(width,height);
            if(!MatrixTools.Blit(imageSrc.matrix, imageDst.matrix, xs, ys, 0, 0, width, height))
                LogMan.Warning(sC, sM, "Blit failed when creating cropped image data.");
            return imageDst;
        }

        public static ImageData CreateImageData( int width, int height)
            => new ImageData().Create(width, height);

        public static ImageData CreateImageData(ImageData image)
            => new ImageData().Create(image);
        public static ImageData CreateImageData(int[,] matrix)
            => new ImageData().Create(matrix);
        public static ImageData CreateImageData(int width, int height, ImageData imageSrc, int xSrc, int ySrc)
            => new ImageData().Create(width,height);

    }
}
