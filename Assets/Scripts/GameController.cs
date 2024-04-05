using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace SimpleMatch
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] 
        private Transform _tileParent;
        
        [SerializeField] 
        private Settings _settings;

        [SerializeField] 
        private MapController _mapController;

        [SerializeField] 
        private TilePoolController _poolController;
        
        
        private readonly GameModel _gameModel = new();
        private readonly Dictionary<TileController, TileModel> _tileToModel = new();
        private readonly Dictionary<TileModel, TileController> _modelToTile = new();
        
        private bool _handleSwipe = true;
        
        private void Start()
        {
            Profiler.BeginSample("_gameModel.CreateMap");
            _gameModel.CreateMap(_settings.Width, _settings.Hight);
            Profiler.EndSample();
            
            foreach (var tileModel in _gameModel.Tiles)
            {
                CreateTile(tileModel);
            }
        }

        private void OnEnable()
        {
            TileController.TileSwipeDetected += TileControllerOnTileSwipeDetected;
        }

        private void OnDisable()
        {
            TileController.TileSwipeDetected -= TileControllerOnTileSwipeDetected;
        }

        private async void TileControllerOnTileSwipeDetected(TileController tile, Vector2Int direction)
        {
            if (!_handleSwipe)
            {
                return;
            }

            _handleSwipe = false;
            
            try
            {
                if (!TryGetSecondTile(tile, direction, out var secondTileModel)
                    || !_modelToTile.TryGetValue(secondTileModel, out var secondTile)
                    || !_tileToModel.TryGetValue(tile, out var tileModel))
                {
                    return;
                }

                await Animation.AnimateSwapAsync(tile.transform, secondTile.transform);

                List<TileModel> movedTiles = new List<TileModel>();
                List<TileModel> createdTiles = new List<TileModel>();
                var swapResult = _gameModel.Swap(tileModel, secondTileModel, movedTiles, createdTiles);
                if (swapResult.HasMatch)
                {
                    foreach (var matchedTile in swapResult.MatchedTiles)
                    {
                        RemoveTile(matchedTile); // Todo пофиксить ошибки при запуске
                    }

                    Task[] movesTasks = new Task[movedTiles.Count];
                    for (var i = 0; i < movedTiles.Count; i++)
                    {
                        var movedTile = movedTiles[i];
                        movesTasks[i] = Animation.AnimateMoveAsync(_modelToTile[movedTile].transform, _mapController.GetTileWorldPosition(movedTile.Position));
                    }

                    await Task.WhenAll(movesTasks);
                        
                    foreach (var createdTile in createdTiles)
                    {
                        CreateTile(createdTile);
                    }

                }
                else
                {
                    await Animation.AnimateSwapAsync(tile.transform, secondTile.transform);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                _handleSwipe = true;
            }
        }


        private void CreateTile(TileModel tile)
        {
            TileController tileController = _poolController.Create(tile.Description.Id);
            tileController.transform.position = _mapController.GetTileWorldPosition(tile.Position);
            _tileToModel[tileController] = tile;
            _modelToTile[tile] = tileController;
        }

        private void RemoveTile(TileModel tile)
        {
            _modelToTile.Remove(tile, out var tileController);
            _tileToModel.Remove(tileController);
            _poolController.Return(tile.Description.Id, tileController);
        }

        private bool TryGetSecondTile(TileController tile, Vector2Int direction, out TileModel secondTile)
        {
            Transform tileTransform = tile.transform;
            Vector3 tilePosition = tileTransform.position;
            Vector2Int secondTilePos = _mapController.GetNextTilePos(tilePosition, direction);
            return _gameModel.PositionToTile.TryGetValue(secondTilePos, out secondTile);
        }

        
    }
}