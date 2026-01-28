using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile;

public partial class TileItem
{
    static readonly string sC =nameof(TileItem);

    object locker = new object();

    /// <summary>
    /// Original image reference
    /// </summary>
    public ImageData TileImage { get; internal set; } = new();

    /// <summary>
    /// Tile Width
    /// </summary>
    public int TileW => TileImage?.Width ?? 0;

    /// <summary>
    /// Tile Height
    /// </summary>
    public int TileH => TileImage?.Height ?? 0;

    /// <summary>
    /// Original position in source
    /// </summary>
    public int OriginX { get; private set; } = 0;

    /// <summary>
    /// Original position in source
    /// </summary>
    public int OriginY { get; private set; } = 0;


    /// <summary>
    /// Gets a value indicating whether the current image source is valid.
    /// </summary>
    public bool IsValid => TileImage?.IsValid ?? false;

    /// <summary>
    /// Resets the origin coordinates and image source to their default values.
    /// </summary>
    /// <remarks>This method sets the origin coordinates to zero and reinitializes the image source. Use this
    /// method to restore the object's state to its initial configuration.</remarks>
    public void Reset()
    {
        lock (locker)
        {
            OriginX = 0;
            OriginY = 0;
            TileImage = new();
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceImage"></param>
    /// <param name="sourceX"></param>
    /// <param name="sourceY"></param>
    /// <param name="tileWidth"></param>
    /// <param name="tileHeight"></param>
    /// <returns></returns>
    public TileItem Create(ImageData sourceImage, int sourceX, int sourceY, int tileWidth, int tileHeight)
    {
        string sM = nameof(Create);
        try
        {
            lock (locker)
            {
                Reset();
                if (sourceImage == null)
                {
                    LogMan.Error(sC, sM, "Source image null");
                    return this;
                }
                if (tileWidth <= 0 || tileHeight <= 0)
                {
                    LogMan.Error(sC, sM, "Invalid Tile Dimension");
                    return this;
                }
                OriginX = sourceX;
                OriginY = sourceY;
                TileImage = sourceImage.Extract( sourceX, sourceY, tileWidth, tileHeight);
                return this;
            }
        }
        catch (Exception ex)
        {
            LogMan.Exception(sC, sM, ex);
            return this;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"R={OriginY}:C={OriginX}:H={TileH}:W={TileW}";
    }  
}
