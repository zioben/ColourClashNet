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
    public static TileItem CreateTileItem(ImageData sourceImage, int sourceX, int sourceY, int tileWidth, int tileHeight)
        => new TileItem().Create(sourceImage, sourceX, sourceY, tileWidth, tileHeight);

}
