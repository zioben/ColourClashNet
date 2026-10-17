using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
                    res &= ImageTools.SaveImage( image, $"{sPath}\\{sPrefix}Blob_{Convert.ToInt32(image.Name):D3}", format, imageQuality);
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
    }
}