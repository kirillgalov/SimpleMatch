﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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


        private void CreateTiles(Vector2Int start, Vector2Int end, ICollection<TileModel> createdTiles = null)
        {
            var descriptions = TileDescription.Descriptions.ToList();
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
                        createdTiles?.Add(model);
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
        
        public void CreateMap(int width, int height)
        {
            Vector2Int center = new Vector2Int(width / 2, height / 2);
            Max = center - Vector2Int.one;
            Min = -center;
            Center = center;
            CreateTiles(Min, Max);
        }

        public SwapResultModel Swap(TileModel a, TileModel b)
        {
            SwapModels(a, b);
            
            if (!FindMatch(a.Position, b.Position, out var tiles))
            {
                SwapModels(a, b);
                return SwapResultModel.Fail();
            }


            _positionToTile.Remove(tiles.Pos1, out TileModel t1);
            _positionToTile.Remove(tiles.Pos2, out TileModel t2);
            _positionToTile.Remove(tiles.Pos3, out TileModel t3);

            SwapResultModel swapResult = SwapResultModel.Match(t1, t2, t3);
            
            const int verticalMatchHeight = 3, horizontalMatchHeight = 1;
            if (tiles.IsVertical())
            {
                MoveColumnDown(tiles.GetMinY(), verticalMatchHeight, swapResult.MovedTiles, out var startHole);
                Vector2Int endHole = new Vector2Int(startHole.x, Max.y);
                CreateTiles(startHole, endHole, swapResult.CreatedTiles);
            }
            else
            {
                MoveColumnDown(tiles.Pos1, horizontalMatchHeight, swapResult.MovedTiles, out var hole1);
                MoveColumnDown(tiles.Pos2, horizontalMatchHeight, swapResult.MovedTiles, out var hole2);
                MoveColumnDown(tiles.Pos3, horizontalMatchHeight, swapResult.MovedTiles, out var hole3);
                CreateTiles(hole1, hole3, swapResult.CreatedTiles);
            }

            return swapResult;
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

            public override string ToString() => $"p1: {Pos1}, p2: {Pos2}, p3: {Pos3};";
        }
    }
}