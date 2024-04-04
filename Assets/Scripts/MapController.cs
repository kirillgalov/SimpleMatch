using System;
using UnityEngine;

namespace SimpleMatch
{
    [RequireComponent(typeof(Grid))]
    public class MapController : MonoBehaviour
    {
        [SerializeField] 
        private Grid _grid;

        private void OnValidate()
        {
            _grid = GetComponent<Grid>();
        }

        public Vector2Int GetNextTilePos(Vector3 tilePosition, Vector2Int direction)
        {
            Vector3Int tileCellPosition = _grid.WorldToCell(tilePosition);
            return new Vector2Int(tileCellPosition.x + direction.x, tileCellPosition.y + direction.y);
        }

        public Vector3 GetTileWorldPosition(Vector2Int cellPosition)
        {
            return _grid.GetCellCenterLocal((Vector3Int)cellPosition);
        }
        
    }
}