using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleMatch
{
    public class Startup : MonoBehaviour
    {
        [SerializeField] 
        private Settings _settings;

        [SerializeField] 
        private Grid _fieldGrid;

        [SerializeField] 
        private Transform _tileParent;

        [SerializeField] 
        private Physics2DRaycaster _raycaster;
        
        
        private void Start()
        {
            
            Vector3Int centerShift = new Vector3Int(_settings.Width / 2, _settings.Hight / 2, 0);
            for (int x = 0; x < _settings.Width; x++)
            {
                for (int y = 0; y < _settings.Hight; y++)
                {
                    Vector3Int cell = new Vector3Int(x, y, 0) - centerShift;
                    var tilePrefab = _settings.TilePrefabs[Random.Range(0, _settings.TilePrefabs.Count)];
                    GameObject tile = Instantiate(tilePrefab.Prefab, _fieldGrid.GetCellCenterLocal(cell), Quaternion.identity, _tileParent);
                }
            }
            
            TileController.TileSwipeDetected += TileControllerOnTileSwipeDetected;
        }

        private void TileControllerOnTileSwipeDetected(TileController tile, Vector2Int direction)
        {
                
        }
    }   
}
