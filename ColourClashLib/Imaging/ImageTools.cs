using ColourClashNet.Color;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace ColourClashNet.Imaging;

public static partial class ImageTools
{
    static string sC = nameof(ImageTools);

    #region Indexed Data Creation

    /// <summary>
    /// Creates a two-dimensional byte array representing the indexed pixel data of the specified image, using the
    /// given pixel width alignment mode.
    /// </summary>
    /// <param name="oImage">The image data to convert to indexed format. Must not be null.</param>
    /// <param name="ePixelWidthAlign">The pixel width alignment mode to apply when generating the indexed data.</param>
    /// <returns>A two-dimensional byte array containing the indexed pixel data of the image, aligned according to the
    /// specified mode.</returns>
    static public byte[,]? CreateIndexedMatrix(ImageData oImage, WidthAlignMode ePixelWidthAlign)
        => MatrixTools.CreateIndexedMatrix(oImage?.matrix, oImage?.ColorPalette, ePixelWidthAlign);

    /// <summary>
    /// Creates a two-dimensional byte array representing the indexed pixel data of the specified image.
    /// </summary>
    /// <param name="oImage">The image data containing the pixel buffer and palette to convert to indexed format. Cannot be null.</param>
    /// <returns>A two-dimensional byte array containing the indexed pixel data of the image.</returns>
    static public byte[,]? CreateIndexedMatrix(ImageData oImage)
        => MatrixTools.CreateIndexedMatrix(oImage?.matrix, oImage?.ColorPalette);


    /// <summary>
    /// 
    /// </summary>
    /// <param name="oImage"></param>
    /// <returns></returns>
    public static ImageData? DoubleXResolution(ImageData oImage)
    {
        string sM = nameof(DoubleXResolution);  
        if (oImage == null)
        {
            LogMan.Error(sC,sM, "Input image is null.");
            return new ImageData();
        }
        var matrix = MatrixTools.DoubleColumnResolution(oImage.matrix);
        return new ImageData().Create(matrix);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="oImage"></param>
    /// <returns></returns>
    public static ImageData? HalveXResolution(ImageData oImage)
    {
        string sM = nameof(DoubleXResolution);
        if (oImage == null)
        {
            LogMan.Error(sC, sM, "Input image is null.");
            return new ImageData();
        }
        var matrix = MatrixTools.HalveColumnResolution(oImage.matrix);
        return new ImageData().Create(matrix);
    }

    #endregion
}