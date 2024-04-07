using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SimpleMatch
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] 
        private Settings _settings;

        [SerializeField] 
        private MapController _mapController;

        [SerializeField] 
        private TilePoolController _poolController;

        [SerializeField] 
        private Button _simulate;
        
        private readonly Dictionary<TileController, TileModel> _tileToModel = new();
        private readonly Dictionary<TileModel, TileController> _modelToTile = new();
        private readonly GameModel _gameModel = new();
        private readonly MatchModel _matchModel = new();
        private bool _handleSwipe = true;


        private void Start()
        {
            Profiler.BeginSample("_gameModel.CreateMap");
            _gameModel.CreateMap(_settings.Width, _settings.Hight);
            Profiler.EndSample();
            CreateAll(_gameModel.Tiles);
        }

        private void OnEnable()
        {
            TileController.TileSwipeDetected += TileControllerOnTileSwipeDetected;
            _simulate.onClick.AddListener(SimulatePlayers);
        }

        private void OnDisable()
        {
            TileController.TileSwipeDetected -= TileControllerOnTileSwipeDetected;
            _simulate.onClick.RemoveListener(SimulatePlayers);
        }

        private void SimulatePlayers()
        {
            int reshuffleCount = 0;
            Profiler.BeginSample("SimulatePlayers");
            for(int i = 0; i < 100_0; i++)
            {
                var (a, b) = _gameModel.FindPossibleMatches();

                if (a == null || b == null)
                {
                    reshuffleCount++;
                    _gameModel.Reshuffle();
                    continue;
                }
                
                _gameModel.Swap(a, b, _matchModel);
                while (_matchModel.HasMatch)
                {
                    _gameModel.FindAndMatch(_matchModel);
                }
                _matchModel.Clear();
            }
            Profiler.EndSample();
            Debug.Log($"Reshuffle count: {reshuffleCount}");
            RemoveAll();
            CreateAll(_gameModel.Tiles);
            // Debug.Break();
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
                if (!TryGetSecondTile(tile, direction, out var secondTileModel) || 
                    !_modelToTile.TryGetValue(secondTileModel, out var secondTile) || 
                    !_tileToModel.TryGetValue(tile, out var tileModel))
                {
                    return;
                }

                await Animation.AnimateSwapAsync(tile.transform, secondTile.transform);

                _gameModel.Swap(tileModel, secondTileModel, _matchModel);
                if (!_matchModel.HasMatch)
                {
                    await Animation.AnimateSwapAsync(tile.transform, secondTile.transform);
                    return;
                }
                await HandleMatchAsync(_matchModel);
                await HandleCascadeMatches(_matchModel);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                _handleSwipe = true;
                _matchModel.Clear();
            }
        }

        private async Task HandleCascadeMatches(MatchModel matchModel)
        {
            _gameModel.FindAndMatch(matchModel);
            while (matchModel.HasMatch)
            {
                await HandleMatchAsync(matchModel);
                _gameModel.FindAndMatch(matchModel);
            }
        }

        private async Task HandleMatchAsync(MatchModel match)
        {
            foreach (var matchedTile in match.MatchedTiles)
            {
                RemoveTile(matchedTile); 
            }

            Task[] movesTasks = new Task[match.MovedTiles.Count];
            for (var i = 0; i < match.MovedTiles.Count; i++)
            {
                var movedTile = match.MovedTiles[i];
                movesTasks[i] = Animation.AnimateMoveAsync(_modelToTile[movedTile].transform, _mapController.GetTileWorldPosition(movedTile.Position));
            }

            await Task.WhenAll(movesTasks);
                        
            foreach (var createdTile in match.CreatedTiles)
            {
                CreateTile(createdTile);
            }
        }

        private void CreateAll(IEnumerable<TileModel> tiles)
        {
            foreach (var tile in tiles)
            {
                CreateTile(tile);
            }
        }

        private void RemoveAll()
        {
            foreach (var (model, controller) in _modelToTile)
            {
                _poolController.Return(controller.DescriptionId, controller);
            }
            _modelToTile.Clear();
            _tileToModel.Clear();
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
            _poolController.Return(tileController.DescriptionId, tileController);
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