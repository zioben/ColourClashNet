using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile;

public partial class TileItem
{
    /// <summary>
    /// Extract a tile data respect source data
    /// </summary>
    /// <param name="oDataSource"></param>
    /// <param name="iRowSource"></param>
    /// <param name="iColumnSource"></param>
    /// <param name="iTileWidth"></param>
    /// <param name="iTileHeight"></param>
    /// <returns></returns>
    public static ImageData? CreateTileData(ImageData oDataSource, int iRowSource, int iColumnSource, int iTileWidth, int iTileHeight)
    {
        string sM = nameof(CreateTileData);
        if (!oDataSource?.Valid ?? true)
        {
            LogMan.Error(sC, sM, "Datasource null or invalid");
            return new();
        }
        if (iTileWidth <= 0 || iTileHeight <= 0)
        {
            LogMan.Error(sC, sM, "Invalid Tile Size");
            return new();
        }

        var oDataTile = new int[iTileHeight, iTileWidth];
        int CC = Math.Max(0, Math.Min(oDataSource.Columns, iColumnSource + iTileWidth));
        int RR = Math.Max(0, Math.Min(oDataSource.Rows, iRowSource + iTileHeight));
        // Get tile data
        for (int sr = iRowSource, r = 0; sr < RR; sr++, r++)
        {
            for (int sc = iColumnSource, c = 0; sc < CC; sc++, c++)
            {
                oDataTile[r, c] = oDataSource.DataX[sr, sc];
            }
        }
        return new ImageData().Create(oDataTile);
    }

    /// <summary>
    /// Copy the best tile (lower color error) on destination data
    /// </summary>
    /// <param name="oDestinationData"></param>
    /// <param name="oTileA"></param>
    /// <param name="oTileB"></param>
    /// <returns>true if data</returns>
    public static bool MergeData(int[,]? oDestinationData, TileItem oTileA, TileItem oTileB)
    {
        if (oDestinationData == null)
        {
            return false;
        }
        if (oTileA == null && oTileB == null)
        {
            return false;
        }
        if (oTileA != null && oTileB == null)
        {
            return oTileA.MergeData(oDestinationData);
        }
        if (oTileA == null && oTileB != null)
        {
            return oTileB.MergeData(oDestinationData);
        }
        //double dErrorA=0, dErrorB=0;
        double dErrorA = oTileA.TransformationError;
        double dErrorB = oTileB.TransformationError;
        if (dErrorA <= dErrorB)
        {
            return oTileA.MergeData(oDestinationData);
        }
        else
        {
            return oTileB.MergeData(oDestinationData);
        }
    }

    /// <summary>
    /// Copy the best tile (lower color error) on destination data
    /// </summary>
    /// <param name="oDestinationData"></param>
    /// <param name="lTiles">List to tiles to compare</param>
    /// <returns></returns>
    public static bool MergeData(int[,]? oDestinationData, List<TileItem> lTiles)
    {
        if (oDestinationData == null)
        {
            return false;
        }
        if (lTiles == null || lTiles.Count == 0)
        {
            return false;
        }
        var oTile = lTiles.Where(X => X.TransformationError == lTiles.Min(Y => Y.TransformationError)).FirstOrDefault();
        if (oTile == null)
        {
            return false;
        }
        return oTile.MergeData(oDestinationData);
    }




}