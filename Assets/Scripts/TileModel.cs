using UnityEngine;

namespace SimpleMatch
{
    public class TileModel
    {
        public TileModel(int instanceId, TileDescription description)
        {
            InstanceId = instanceId;
            Description = description;
            Position = Vector2Int.zero;
        }

        public int InstanceId { get; }

        public TileDescription Description { get; }

        public Vector2Int Position { get; set; }

        public override string ToString()
        {
            return $"InstanceId: {InstanceId}, Description: {Description}, Position: {Position}";
        }
    }
}