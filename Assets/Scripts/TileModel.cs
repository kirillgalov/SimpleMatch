using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleMatch
{
    public class TileModel : IDisposable
    {
        private static readonly Stack<TileModel> Pool = new Stack<TileModel>(64);
        private static int _seed = 1;
        
        public static TileModel Create(TileDescription description)
        {
            if (!Pool.TryPop(out var model))
            {
                model = new TileModel(description);
            }

            model.Description = description;
            return model;
        } 
        
        public TileModel(TileDescription description)
        {
            Id = _seed++;
            Description = description;
            Position = Vector2Int.zero;
        }

        public int Id { get; }

        public TileDescription Description { get; private set; }

        public Vector2Int Position { get; set; }

        public override string ToString()
        {
            return $"InstanceId: {Id}, Description: {Description}, Position: {Position}";
        }

        public void Dispose()
        {
            Pool.Push(this);
        }
    }
}