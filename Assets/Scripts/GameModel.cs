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

        public void CreateMap(int width, int height)
        {
            Vector2Int centerShift = new Vector2Int(width / 2, height / 2);
            Max = centerShift - Vector2Int.one;
            Min = -centerShift;
            Center = centerShift;
            var descriptions = TileDescription.Descriptions.ToList();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Shuffle(descriptions);
                    bool found = false;
                    Vector2Int newTilePos = new Vector2Int(x, y) - centerShift;
                    foreach (var description in descriptions)
                    {
                        
                        CreateTile(newTilePos, description);
                        if (!FindMatch(newTilePos, out _))
                        {
                            found = true;
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

        public SwapResultModel Swap(TileModel a, TileModel b)
        {
            SwapModels(a, b);
            TilesFrame tiles = default;
            if (FindMatch(a.Position, out tiles) || FindMatch(b.Position, out tiles))
            {
                _positionToTile.Remove(tiles.Pos1, out TileModel t1);
                _positionToTile.Remove(tiles.Pos2, out TileModel t2);
                _positionToTile.Remove(tiles.Pos3, out TileModel t3);

                if (tiles.IsVertical())
                {
                    MoveColumnDown(tiles.GetMinY(), true, out var finalHole);
                    
                }
            
                return SwapResultModel.Match(new[] { t1, t2, t3 });
            }
            else
            {
                SwapModels(a,b);
                return SwapResultModel.Fail();
            }
        }

        private void MoveColumnDown(Vector2Int hole, bool isVerticalStep, out Vector2Int finalHole)
        {
            const int horizontalStep = 1, verticalStep = 3;
            int step = isVerticalStep ? verticalStep : horizontalStep;
            while (hole.y < Max.y)
            {
                var nextTilePos = new Vector2Int(hole.x, hole.y + step);
                if (_positionToTile.Remove(nextTilePos, out var nextTile))
                {
                    nextTile.Position = hole;
                    _positionToTile[nextTile.Position] = nextTile;
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

        private bool FindMatch(Vector2Int pos, out TilesFrame match)
        {
            AddCheckTile(pos, _tilesToCheck);
            bool found = false;
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
            match = default;
            return found;
        }

        private static void AddCheckTile(Vector2Int position, List<TilesFrame> tilesToCheck)
        {
            tilesToCheck.Add(new TilesFrame(position + 2 * Vector2Int.left, Vector2Int.right));
            tilesToCheck.Add(new TilesFrame(position + Vector2Int.left, Vector2Int.right));
            tilesToCheck.Add(new TilesFrame(position, Vector2Int.right));
            
            tilesToCheck.Add(new TilesFrame(position + 2 * Vector2Int.up, Vector2Int.down));
            tilesToCheck.Add(new TilesFrame(position + Vector2Int.up, Vector2Int.down));
            tilesToCheck.Add(new TilesFrame(position, Vector2Int.down));
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
            public readonly Vector2Int Pos1;
            public readonly Vector2Int Pos2;
            public readonly Vector2Int Pos3;

            public TilesFrame(Vector2Int origin, Vector2Int direction)
            {
                Pos1 = origin;
                Pos2 = Pos1 + direction;
                Pos3 = Pos2 + direction;
            }

            public bool IsVertical() => Pos1.x == Pos2.x && Pos2.x == Pos3.x;

            public Vector2Int GetMinY() => new Vector2Int(Pos1.x, Mathf.Min(Pos1.y, Mathf.Min(Pos2.y, Pos3.y)));
        }
    }

    public class SwapResultModel
    {
        public static SwapResultModel Fail() => FailModel;
        public static SwapResultModel Match(IEnumerable<TileModel> matchedTiles) => new SwapResultModel(true, matchedTiles);
        
        private static readonly SwapResultModel FailModel = new(false, Enumerable.Empty<TileModel>());
        
        public bool HasMatch { get; }
        public IEnumerable<TileModel> MatchedTiles { get; }
        
        private SwapResultModel(bool hasMatch, IEnumerable<TileModel> matchedTiles)
        {
            HasMatch = hasMatch;
            MatchedTiles = matchedTiles;
        }
    }
}