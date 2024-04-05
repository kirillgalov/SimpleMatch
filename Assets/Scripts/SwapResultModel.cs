using System.Collections.Generic;
using System.Linq;

namespace SimpleMatch
{
    public class SwapResultModel
    {
        private static readonly SwapResultModel FailModel = new(false, Enumerable.Empty<TileModel>());
        public static SwapResultModel Fail() => FailModel;
        public static SwapResultModel Match(IEnumerable<TileModel> matchedTiles) => new SwapResultModel(true, matchedTiles);


        public bool HasMatch { get; }
        public IEnumerable<TileModel> MatchedTiles { get; }
        
        private SwapResultModel(bool hasMatch, IEnumerable<TileModel> matchedTiles)
        {
            HasMatch = hasMatch;
            MatchedTiles = matchedTiles;
        }
    }
}