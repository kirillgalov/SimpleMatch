using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimpleMatch
{
    public class TilePoolController : MonoBehaviour
    {
        [SerializeField] 
        private Settings _settings;

        private readonly HashSet<TileController> _inPool = new();
        private IReadOnlyDictionary<TileDescriptionId, TileController> _prefabs;
        private IReadOnlyDictionary<TileDescriptionId, Stack<TileController>> _pool;

        private void Awake()
        {
            var prefabs = new Dictionary<TileDescriptionId, TileController>(_settings.TilePrefabs.Count);
            var pool = new Dictionary<TileDescriptionId, Stack<TileController>>(_settings.TilePrefabs.Count);
            foreach (Settings.TilePrefab tilePrefab in _settings.TilePrefabs)
            {
                if (tilePrefab.Prefab.TryGetComponent(out TileController tile))
                {
                    var tileDescriptionId = new TileDescriptionId(tilePrefab.Id);
                    prefabs.Add(tileDescriptionId, tile);
                    pool.Add(tileDescriptionId, new Stack<TileController>(16));
                }
            }
            _prefabs = prefabs;
            _pool = pool;
        }

        public TileController Create(TileDescriptionId id)
        {
            if (_pool.TryGetValue(id, out var tiles))
            {
                if (tiles.TryPop(out var tile))
                {
                    _inPool.Remove(tile);
                    tile.gameObject.SetActive(true);
                    return tile;
                }
                else if (_prefabs.TryGetValue(id, out var prefab))
                {
                    var tileController = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
                    tileController.DescriptionId = id;
                    return tileController;
                }
            }

            throw new ArgumentException($"Unknown id {id}", nameof(id));
        }

        public void Return(TileDescriptionId id, TileController tile)
        {
            if (!_inPool.Add(tile))
            {
                throw new ArgumentException("Already in pool", nameof(tile));
            }

            if (_pool.TryGetValue(id, out var tiles))
            {
                tile.gameObject.SetActive(false);
                tiles.Push(tile);
            }
        }
    }
}