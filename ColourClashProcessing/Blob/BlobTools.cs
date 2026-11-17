using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace ColourClashProcessing.Blob
{
    public static class BlobTools
    {
        static string sC = nameof(BlobTools);
        public static bool ExportImages(Blob blob, string sPath, string sPrefix, ImageSaveFormat format, int imageQuality)
        {
            string sM = nameof(ExportImages);
            try
            {
                Directory.CreateDirectory(sPath);
                var imageList = blob.ToImageList();
                var res = true;
                foreach (var image in imageList)
                {
                    res &= ImageTools.SaveImage(image, $"{sPath}\\{sPrefix}Blob_{Convert.ToInt32(image.Name):D3}", format, imageQuality);
                }
                return res;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return false;
            }
        }

        public static bool ExportImages(Blob blob, string sPath, string sPrefix)
            => ExportImages(blob, sPath, sPrefix, ImageSaveFormat.Png, 80);


        public static bool RepackImages(Blob blob, string sPath, string sPrefix, ImageSaveFormat format, int imageQuality)
        {
            string sM = nameof(ExportImages);
            try
            {
                throw new NotImplementedException(sM);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return false;
            }
        }

        public static ImageData ReshapeVertical(Blob blob, int maxWidth, int xPaddingL, int xPaddingR, int yPaddingT,int yPaddingB)            
        {
            string sM = nameof(ReshapeVertical);
            try
            {
                if(blob == null) throw new ArgumentNullException(nameof(blob));
                // Get all blobs
                var list = blob.ToImageList();
                if( maxWidth > 0)
                {
                    list = list.Where(x => x != null && x.Width <= maxWidth).ToList();
                }
                var count = list.Count();
                // evaluates the maximum width of the images and the total height of the images
                var maxw = list.Max(x => x.Width);
                var imageW = maxw + xPaddingL + xPaddingR;
                var imageH = list.Sum(x => x.Height) + (yPaddingT + yPaddingB) * (count);
                var matrix= MatrixTools.CreateMatrix(imageW, imageH, blob.BlobBkgColor);
                // starting point for the first image
                int xStart = xPaddingL;
                int yStart = yPaddingT;
                foreach (var image in list)
                {
                    if (!MatrixTools.Blit(image, matrix, 0, 0, xStart, yStart, image.Width, image.Height))
                    {
                        LogMan.Error(sC, sM, $"Blit failed for image {image.Name} at position ({xStart},{yStart})");
                        return null;
                    }
                    yStart += image.Height + yPaddingT + yPaddingB;
                }
                var outImage = new ImageData().Create(matrix);
                return outImage;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return null;
            }
        }
    }
}