using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile;

/// <summary>
/// Rectangular portion of a source image, with its origin coordinates.
/// </summary>
public partial class TileItem
{
    static readonly string sC =nameof(TileItem);

    object locker = new object();

    /// <summary>
    /// Copy of the source image area covered by the tile.
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
    /// Left coordinate of the tile in the source image.
    /// </summary>
    public int OriginX { get; private set; } = 0;

    /// <summary>
    /// Top coordinate of the tile in the source image.
    /// </summary>
    public int OriginY { get; private set; } = 0;

    /// <summary>
    /// Gets the number of colors in the tile image.
    /// </summary>
    public int SourceColors { get => TileImage?.Colors ?? 0; }

    /// <summary>
    /// Resets the origin to zero and the tile image to an empty image.
    /// </summary>
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
    /// Extracts the tile from <paramref name="sourceImage"/>.
    /// </summary>
    /// <param name="sourceImage">Source image</param>
    /// <param name="sourceX">Tile left coordinate</param>
    /// <param name="sourceY">Tile top coordinate</param>
    /// <param name="tileWidth">Tile width (must be positive)</param>
    /// <param name="tileHeight">Tile height (must be positive)</param>
    /// <returns>This instance.</returns>
    /// <exception cref="InvalidConstraintException">Invalid tile size.</exception>
    public TileItem Create(ImageData sourceImage, int sourceX, int sourceY, int tileWidth, int tileHeight)
    {
        string sM = nameof(Create);

        ImageData.AssertValid(sourceImage);
        if (tileWidth <= 0 || tileHeight <= 0)
            throw new InvalidConstraintException($"{sC}.{sM} : tile size {tileWidth}x{tileHeight} invalid");

        lock (locker)
        {
            Reset();
            OriginX = sourceX;
            OriginY = sourceY;
            TileImage = sourceImage.Extract(sourceX, sourceY, tileWidth, tileHeight);
            return this;
        }
    }

    /// <summary>
    /// Returns the tile position and size.
    /// </summary>
    public override string ToString()
    {
        return $"R={OriginY}:C={OriginX}:H={TileH}:W={TileW}";
    }  
}
