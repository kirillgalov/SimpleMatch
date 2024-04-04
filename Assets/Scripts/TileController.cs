using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleMatch
{
    public class TileController : MonoBehaviour, 
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerMoveHandler,
        IPointerExitHandler
    {

        public static event Action<TileController, Vector2Int> TileSwipeDetected; 
        
        private bool _detectSwipe;
        private static readonly float _swipeTreshold = 0.15f;
        private Vector2 _movedDistance;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _detectSwipe = true;
            _movedDistance = Vector2.zero;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _detectSwipe = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _detectSwipe = false;
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!_detectSwipe)
            {
                return;
            }

            _movedDistance += eventData.delta;

            if (TryToDetectSwipe(_swipeTreshold, _movedDistance, out var swipeDirection))
            {
                _detectSwipe = false;
                OnTileSwipeDetected(this, swipeDirection);
            }
        }

        private static bool TryToDetectSwipe(float swipeThreshold, Vector2 pointerMoved, out Vector2Int swipeDirection)
        {
            swipeDirection = Vector2Int.zero;
            if (Mathf.Abs(pointerMoved.x) > swipeThreshold)
            {
                swipeDirection = pointerMoved.x > 0 ? Vector2Int.right : Vector2Int.left;
                return true;
            }
            
            if (Mathf.Abs(pointerMoved.y) > swipeThreshold)
            {
                swipeDirection = pointerMoved.y > 0 ? Vector2Int.up : Vector2Int.down;
                return true;
            }

            return false;
        }

        private static void OnTileSwipeDetected(TileController tile, Vector2Int direction)
        {
            try
            {
                TileSwipeDetected?.Invoke(tile, direction);
            }
            catch (Exception e)
            {
                Debug.LogException(e, tile);
            }
        }
    }
}