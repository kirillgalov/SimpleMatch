using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SimpleMatch
{
    public class GameModel
    {
        private readonly Dictionary<Vector2Int, TileModel> _positionToTile = new Dictionary<Vector2Int, TileModel>();
        private readonly List<TilesFrame> _tilesToCheck = new();
        private readonly List<TileDescription> _tileDescriptionsCache = TileDescription.Descriptions.ToList();
        private int _tileIds = 1;

        public Vector2Int Min { get; private set; }
        public Vector2Int Max { get; private set; }
        public Vector2Int Center { get; private set; }
        public IReadOnlyDictionary<Vector2Int, TileModel> PositionToTile => _positionToTile;
        public IEnumerable<TileModel> Tiles => _positionToTile.Values;

        public TileModel CreateTile(Vector2Int pos, TileDescription tileDescription = null)
        {
            tileDescription ??= TileDescription.Descriptions[Random.Range(0, TileDescription.Descriptions.Count)];
            var model = new TileModel(_tileIds++, tileDescription) { Position = pos };
            _positionToTile[pos] = model;
            return model;
        }


        private void CreateTiles(Vector2Int start, Vector2Int end, ICollection<TileModel> createdTiles = null)
        {
            var descriptions = _tileDescriptionsCache;
            for (int x = start.x; x <= end.x; x++)
            {
                for (int y = start.y; y <= end.y; y++)
                {
                    Shuffle(descriptions);
                    bool found = false;
                    Vector2Int newTilePos = new Vector2Int(x, y);
                    foreach (var description in descriptions)
                    {
                        TileModel model = CreateTile(newTilePos, description);
                        
                        if (!FindMatch(newTilePos, out _))
                        {
                            found = true;
                            createdTiles?.Add(model);
                            break;
                        }
                    }

                    if (!found)
                    {
                        Debug.LogError($"Tile not found pos:{newTilePos}");
                    }
                    
                }
            }
        }
        
        public void CreateMap(int width, int height)
        {
            Vector2Int center = new Vector2Int(width / 2, height / 2);
            Max = center - Vector2Int.one;
            Min = -center;
            Center = center;
            CreateTiles(Min, Max);
        }

        public void Swap(TileModel a, TileModel b, MatchModel matchModel)
        {
            matchModel.Clear();
            SwapModels(a, b);
            if (!FindMatch(a.Position, b.Position, out var tiles))
            {
                SwapModels(a, b);
                return;
            }
            
            HandleMatch(tiles, matchModel);
        }

        public (TileModel a, TileModel b) FindPossibleMatches()
        {
            foreach (var pos in _positionToTile.Keys)
            {
                _tilesToCheck.Clear();
                
                _tilesToCheck.Add(TilesFrame.WithBreak(pos, Vector2Int.down));
                _tilesToCheck.Add(TilesFrame.WithBreak(pos, Vector2Int.left));
                _tilesToCheck.Add(TilesFrame.WithBreak(pos, Vector2Int.up));
                _tilesToCheck.Add(TilesFrame.WithBreak(pos, Vector2Int.right));
                
                _tilesToCheck.Add(TilesFrame.WithCentralBreak(pos, Vector2Int.right));
                _tilesToCheck.Add(TilesFrame.WithCentralBreak(pos, Vector2Int.left));
                _tilesToCheck.Add(TilesFrame.WithCentralBreak(pos, Vector2Int.down));
                _tilesToCheck.Add(TilesFrame.WithCentralBreak(pos, Vector2Int.up));
                
                foreach (var tiles in _tilesToCheck)
                {
                    if (IsMatch(tiles.Pos1, tiles.Pos2, tiles.Pos3) && _positionToTile.TryGetValue(tiles.PossibleHole, out var hole))
                    {
                        return (_positionToTile[tiles.Pos1], hole);
                    }
                }
            }

            throw new Exception();
        }

        public void FindAndMatch(MatchModel matchModel)
        {
            matchModel.Clear();
            foreach (var position in _positionToTile.Keys)
            {
                if (FindMatch(position, out var tiles))
                {
                    HandleMatch(tiles, matchModel);
                    return;
                }
            }
        }

        private void HandleMatch(TilesFrame tiles, MatchModel matchModel)
        {
            _positionToTile.Remove(tiles.Pos1, out TileModel t1);
            _positionToTile.Remove(tiles.Pos2, out TileModel t2);
            _positionToTile.Remove(tiles.Pos3, out TileModel t3);

            matchModel.MatchedTiles.Add(t1);
            matchModel.MatchedTiles.Add(t2);
            matchModel.MatchedTiles.Add(t3);
            
            const int verticalMatchHeight = 3, horizontalMatchHeight = 1;
            if (tiles.IsVertical())
            {
                MoveColumnDown(tiles.GetMinY(), verticalMatchHeight, matchModel.MovedTiles, out var startHole);
                Vector2Int endHole = new Vector2Int(startHole.x, Max.y);
                CreateTiles(startHole, endHole, matchModel.CreatedTiles);
            }
            else
            {
                MoveColumnDown(tiles.Pos1, horizontalMatchHeight, matchModel.MovedTiles, out var hole1);
                MoveColumnDown(tiles.Pos2, horizontalMatchHeight, matchModel.MovedTiles, out var hole2);
                MoveColumnDown(tiles.Pos3, horizontalMatchHeight, matchModel.MovedTiles, out var hole3);
                CreateTiles(hole1, hole3, matchModel.CreatedTiles);
            }
        }

        private void MoveColumnDown(Vector2Int hole, int height, ICollection<TileModel> movedTiles, out Vector2Int finalHole)
        {
            while (hole.y < Max.y)
            {
                var nextTilePos = new Vector2Int(hole.x, hole.y + height);
                if (_positionToTile.Remove(nextTilePos, out var nextTile))
                {
                    nextTile.Position = hole;
                    _positionToTile[nextTile.Position] = nextTile;
                    movedTiles.Add(nextTile);
                    hole += Vector2Int.up;
                }
                else
                {
                    finalHole = hole;
                    break;
                }
            }

            finalHole = hole;
        }

        private void SwapModels(TileModel a, TileModel b)
        {
            _positionToTile[a.Position] = b;
            _positionToTile[b.Position] = a;
            (a.Position, b.Position) = (b.Position, a.Position);
        }

        private bool FindMatch(Vector2Int pos1, Vector2Int pos2, out TilesFrame tiles)
        {
            return FindMatch(pos1, out tiles) || FindMatch(pos2, out tiles);
        }
        
        private bool FindMatch(Vector2Int pos, out TilesFrame match)
        {
            AddCheckTile(pos, _tilesToCheck);
            bool found = false;
            match = default;
            foreach (var tilesFrame in _tilesToCheck)
            {
                if (IsMatch(tilesFrame.Pos1, tilesFrame.Pos2, tilesFrame.Pos3))
                {
                    match = tilesFrame;
                    found = true;
                    break;
                }
            }
            _tilesToCheck.Clear();
            return found;
        }

        private static void AddCheckTile(Vector2Int position, List<TilesFrame> tilesToCheck)
        {
            tilesToCheck.Add(TilesFrame.WithoutBreaks(position + 2 * Vector2Int.left, Vector2Int.right));
            tilesToCheck.Add(TilesFrame.WithoutBreaks(position + Vector2Int.left, Vector2Int.right));
            tilesToCheck.Add(TilesFrame.WithoutBreaks(position, Vector2Int.right));
            
            tilesToCheck.Add(TilesFrame.WithoutBreaks(position + 2 * Vector2Int.up, Vector2Int.down));
            tilesToCheck.Add(TilesFrame.WithoutBreaks(position + Vector2Int.up, Vector2Int.down));
            tilesToCheck.Add(TilesFrame.WithoutBreaks(position, Vector2Int.down));
        }

        private bool IsMatch(Vector2Int pos1, Vector2Int pos2, Vector2Int pos3)
        {
            if (_positionToTile.TryGetValue(pos1, out var tile1) &&
                _positionToTile.TryGetValue(pos2, out var tile2) &&
                _positionToTile.TryGetValue(pos3, out var tile3))
            {
                return tile1.Description.Id == tile2.Description.Id && tile1.Description.Id == tile3.Description.Id;
            }

            return false;
        }

        private static void Shuffle<T>(List<T> items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                int rand = Random.Range(0, items.Count);
                (items[i], items[rand]) = (items[rand], items[i]);
            }
        }
        
        private readonly struct TilesFrame
        {
            public static TilesFrame WithoutBreaks(Vector2Int origin, Vector2Int direction)
            {
                var pos1 = origin;
                var pos2 = pos1 + direction;
                var pos3 = pos2 + direction;
                return new TilesFrame(pos1, pos2, pos3);
            }

            public static TilesFrame WithBreak(Vector2Int origin, Vector2Int direction)
            {
                var pos1 = origin;
                var possibleHole = pos1 + direction;
                var pos2 = possibleHole + direction;
                var pos3 = pos2 + direction;
                return new TilesFrame(pos1, pos2, pos3, possibleHole);
            }

            public static TilesFrame WithCentralBreak(Vector2Int origin, Vector2Int direction)
            {
                var pos1 = origin;
                var possibleHole = pos1 + direction;
                var pos2 = origin + new Vector2Int(direction.x != 0 ? direction.x : -1, direction.y != 0 ? direction.y : -1);
                var pos3 = origin + new Vector2Int(direction.x != 0 ? direction.x : 1, direction.y != 0 ? direction.y : 1);
                return new TilesFrame(pos1, pos2, pos3, possibleHole);
            }
            
            public readonly Vector2Int Pos1;
            public readonly Vector2Int Pos2;
            public readonly Vector2Int Pos3;
            public readonly Vector2Int PossibleHole;
            
            private TilesFrame(Vector2Int pos1, Vector2Int pos2, Vector2Int pos3, Vector2Int possibleHole = default)
            {
                Pos1 = pos1;
                Pos2 = pos2;
                Pos3 = pos3;
                PossibleHole = possibleHole;
            }

            public bool IsVertical() => Pos1.x == Pos2.x && Pos2.x == Pos3.x;

            public Vector2Int GetMinY() => new Vector2Int(Pos1.x, Mathf.Min(Pos1.y, Mathf.Min(Pos2.y, Pos3.y)));

            public override string ToString() => $"p1: {Pos1}, p2: {Pos2}, p3: {Pos3};";
        }
    }
}