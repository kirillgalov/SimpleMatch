using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

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
                TileController tileController = _poolController.Create(tileModel.Description.Id);
                tileController.transform.position = _mapController.GetTileWorldPosition(tileModel.Position);
                _tileToModel[tileController] = tileModel;
                _modelToTile[tileModel] = tileController;
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
                if (TryGetSecondTile(tile, direction, out var secondTileModel)
                    && _modelToTile.TryGetValue(secondTileModel, out var secondTile)
                    && _tileToModel.TryGetValue(tile, out var tileModel))
                {
                    await Animation.AnimateSwapAsync(tile.transform, secondTile.transform);
                    _gameModel.Swap(tileModel, secondTileModel);
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

        private bool TryGetSecondTile(TileController tile, Vector2Int direction, out TileModel secondTile)
        {
            Transform tileTransform = tile.transform;
            Vector3 tilePosition = tileTransform.position;
            Vector2Int secondTilePos = _mapController.GetNextTilePos(tilePosition, direction);
            return _gameModel.PositionToTile.TryGetValue(secondTilePos, out secondTile);
        }

        
    }
}