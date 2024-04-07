using System.Collections.Generic;

namespace SimpleMatch
{
    public class MatchModel
    {
        public bool HasMatch => MatchedTiles.Count != 0;
        public List<TileModel> MatchedTiles { get; } = new(3);
        public List<TileModel> CreatedTiles { get; } = new(16);
        public List<TileModel> MovedTiles { get; } = new(16);

        public void Clear()
        {
            MatchedTiles.ForEach(t => t.Dispose());
            MatchedTiles.Clear();
            CreatedTiles.Clear();
            MovedTiles.Clear();
        }
    }
}