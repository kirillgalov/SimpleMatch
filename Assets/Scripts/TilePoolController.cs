using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleMatch
{
    public class TilePoolController : MonoBehaviour
    {
        [SerializeField] 
        private Settings _settings;

        private IReadOnlyDictionary<TileDescriptionId, TileController> _prefabs;
        private IReadOnlyDictionary<TileDescriptionId, Stack<TileController>> _pool;
        private HashSet<TileController> _inPool;

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
                    return tile;
                }
                else if (_prefabs.TryGetValue(id, out var prefab))
                {
                    return Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
                }
            }

            throw new ArgumentException($"Unknown id {id}", nameof(id));
        }

        public void Return(TileDescriptionId id, TileController tile)
        {
            if (!_inPool.Add(tile))
            {
                throw new Exception();
            }

            if (_pool.TryGetValue(id, out var tiles))
            {
                tiles.Push(tile);
            }
        }
    }
}