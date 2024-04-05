using System;
using System.Collections.Generic;

namespace SimpleMatch
{
    public class SwapResultModel : IDisposable
    {
        private static readonly Stack<SwapResultModel> Pool = new Stack<SwapResultModel>(2);
        private static readonly SwapResultModel FailModel = new();
        public static SwapResultModel Fail() => FailModel;
        public static SwapResultModel Match(TileModel t1, TileModel t2, TileModel t3)
        {
            if (!Pool.TryPop(out SwapResultModel result))
            {
                result = new SwapResultModel();
            }
            result.MatchedTiles.Add(t1);
            result.MatchedTiles.Add(t2);
            result.MatchedTiles.Add(t3);
            
            return result;
        }

        private SwapResultModel()
        {
        }

        public bool HasMatch => MatchedTiles.Count != 0;
        public List<TileModel> MatchedTiles { get; } = new(3);
        public List<TileModel> CreatedTiles { get; } = new(16);
        public List<TileModel> MovedTiles { get; } = new(16);

        public void Dispose()
        {
            MatchedTiles.Clear();
            CreatedTiles.Clear();
            MovedTiles.Clear();
            Pool.Push(this);
        }
    }
}