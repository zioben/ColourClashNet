using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile;

public partial class TileManager
{

    static bool MergeDataFromTileManagers(int[,] oDataOut, List<TileManager?> lTileMan)
    {
        string sM = nameof(MergeDataFromTileManagers);
        if (oDataOut == null)
        {
            LogMan.Error(sC, sM, "output data matrix null");
            return false;
        }

        var lTileManFiltered = lTileMan?.Where(X => X != null).ToList() ?? null;
        if (lTileManFiltered == null || lTileManFiltered.Count == 0)
        {
            LogMan.Error(sC, sM, "No valid TileManagers provided");
            return false;
        }

        var RT = lTileManFiltered.Min(X => X.TileData?.GetLength(0) ?? 0);
        var CT = lTileManFiltered.Min(X => X.TileData?.GetLength(1) ?? 0);
        var bRet = true;
        for (int r = 0; r < RT; r++)
        {
            for (int c = 0; c < CT; c++)
            {
                int rr = r;
                int cc = c;
                var lTiles = new List<TileItem>();
                lTileManFiltered.ForEach(X => lTiles.Add(X.GetTileData(rr, cc)));
                bRet &= TileItem.MergeData(oDataOut, lTiles);
            }
        }
        return bRet;
    }
    public bool MergeDataFromTileManagers(int[,] oDataRef, TileManager oTileA, TileManager oTileB)
        => MergeDataFromTileManagers(oDataRef, new List<TileManager?> { oTileA, oTileB });
    public bool MergeDataFromTileManagers(int[,] oDataRef, TileManager oTileA, TileManager oTileB, TileManager oTileC, TileManager oTileD)
        => MergeDataFromTileManagers(oDataRef, new List<TileManager?> { oTileA, oTileB, oTileC, oTileD });

    static ImageData CreateImageFromTileManagers(List<TileManager?> lTileMan)
    {
        string sM = nameof(CreateImageFromTileManagers);
        var lTileManFiltered = lTileMan?.Where(X => X != null).ToList() ?? null;
        if (lTileManFiltered == null || lTileManFiltered.Count == 0)
        {
            LogMan.Error(sC, sM, "No valid TileManagers provided");
            return null;
        }
        int R = (lTileManFiltered.Max(X => X.SourceData.Rows));
        int C = (lTileManFiltered.Max(X => X.SourceData.Columns));
        if( R== 0 || C == 0)
        {
            LogMan.Error(sC, sM, "Invalid dimensions calculated from TileManagers");
            return null;
        }
        var oDataMerge = new int[R, C];
        if (!MergeDataFromTileManagers(oDataMerge, lTileManFiltered))
        {
            LogMan.Error(sC, sM, "Merging data from TileManagers failed");
            return null;
        }
        return new ImageData().Create(oDataMerge);
    }


    public ImageData? CreateImageFromTileManagers( TileManager? oTileA, TileManager? oTileB)
       => CreateImageFromTileManagers(new List<TileManager?> { oTileA, oTileB });
    public ImageData? CreateImageFromTileManagers( TileManager? oTileA, TileManager? oTileB, TileManager? oTileC, TileManager? oTilD)
        => CreateImageFromTileManagers(new List<TileManager?> { oTileA, oTileB, oTileC, oTilD });


    public static TileManager Create(int iTileW, int iTileH, int iMaxColorWanted)
    {
        return new TileManager()
        {
            TileW = iTileW,
            TileH = iTileH,
            MaxColorsWanted = iMaxColorWanted
        };
    }
}
