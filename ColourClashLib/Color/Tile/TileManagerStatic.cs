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
    public static void AssertValid(TileManager tileMan)
    {
        if (tileMan == null)
            throw new ArgumentNullException($"{nameof(tileMan) is null}");
        ImageData.AssertValid(tileMan.ImageSource);
        if (tileMan.tileProcessingMatrix == null)
            throw new ArgumentNullException($"{nameof(tileMan.tileProcessingMatrix) is null}");
        if (tileMan.tileProcessingMatrix.Length == 0)
            throw new ArgumentOutOfRangeException($"{nameof(tileMan.tileProcessingMatrix)} size is zero");
    }

    static bool MergeToMatrix(int[,] oDataOut, List<TileManager?> lTileMan)
    {
        string sM = nameof(MergeToMatrix);
        if (oDataOut == null)
            throw new ArgumentNullException(nameof(oDataOut));

        var lTileManFiltered = lTileMan?.Where(X => X != null).ToList() ?? null;
        if (lTileManFiltered == null || lTileManFiltered.Count == 0)
        {
            LogMan.Error(sC, sM, "No valid TileManagers provided");
            return false;
        }

        var minRows = lTileManFiltered.Min(X => X.TileRows);
        var minColumns = lTileManFiltered.Min(X => X.TileColumns);
        var bRet = true;
        for (int r = 0; r < minRows; r++)
        {
            for (int c = 0; c < minColumns; c++)
            {
                var lTileProcessingRC = new List<TileProcessing>();
                lTileManFiltered.ForEach(X => lTileProcessingRC.Add(X.GetTileProcessing(r, c)));
                var lTileProcessingFilteredRC = lTileProcessingRC.Where(X => X != null).ToList();
                bRet &= TileProcessing.MergeData(oDataOut, lTileProcessingFilteredRC);
            }
        }
        return bRet;
    }
    bool MergeToMatrix(int[,] oDataRef, TileManager oTileA, TileManager oTileB)
        => MergeToMatrix(oDataRef, new List<TileManager?> { oTileA, oTileB });
    bool MergeToMatrix(int[,] oDataRef, TileManager oTileA, TileManager oTileB, TileManager oTileC, TileManager oTileD)
        => MergeToMatrix(oDataRef, new List<TileManager?> { oTileA, oTileB, oTileC, oTileD });

    public static ImageData MergeToImage(List<TileManager?> lTileMan)
    {
        string sM = nameof(MergeToImage);
        var lTileManFiltered = lTileMan?.Where(X => X != null).ToList() ?? null;
        if (lTileManFiltered == null || lTileManFiltered.Count == 0)
        {
            LogMan.Error(sC, sM, "No valid TileManagers provided");
            return null;
        }
        int R = (lTileManFiltered.Max(X => X.ImageSource.Rows));
        int C = (lTileManFiltered.Max(X => X.ImageSource.Columns));
        if( R== 0 || C == 0)
        {
            LogMan.Error(sC, sM, "Invalid dimensions calculated from TileManagers");
            return null;
        }
        var oDataMerge = new int[R, C];
        if (!MergeToMatrix(oDataMerge, lTileManFiltered))
        {
            LogMan.Error(sC, sM, "Merging data from TileManagers failed");
            return null;
        }
        return new ImageData().Create(oDataMerge);
    }


    public ImageData? MergeToImage( TileManager? oTileA, TileManager? oTileB)
       => MergeToImage(new List<TileManager?> { oTileA, oTileB });
    public ImageData? MergeToImage( TileManager? oTileA, TileManager? oTileB, TileManager? oTileC, TileManager? oTilD)
        => MergeToImage(new List<TileManager?> { oTileA, oTileB, oTileC, oTilD });

}
