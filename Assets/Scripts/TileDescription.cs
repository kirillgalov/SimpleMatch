using System;
using System.Collections.Generic;

namespace SimpleMatch
{
    public class TileDescription
    {
        private static readonly Dictionary<TileDescriptionId, TileDescription> _tileDescriptions = new();
        private static readonly List<TileDescription> _descriptions = new();
        public static IReadOnlyDictionary<TileDescriptionId, TileDescription> TileDescriptions => _tileDescriptions;
        public static IReadOnlyList<TileDescription> Descriptions => _descriptions;
        private static TileDescription Add(string tileId, bool isBonus)
        {
            var tileDescription = new TileDescription(new TileDescriptionId(tileId), isBonus);
            _tileDescriptions.Add(tileDescription.Id, tileDescription);
            _descriptions.Add(tileDescription);
            return tileDescription;
        }

        public static TileDescription Circle { get; } = Add("Tile.Circle", false);
        public static TileDescription Triangle { get; } = Add("Tile.Triangle", false);
        public static TileDescription Square { get; } = Add("Tile.Square", false);
        public static TileDescription Hexagon { get; } = Add("Tile.Hexagon", false);

        public TileDescription(TileDescriptionId id, bool isBonus)
        {
            Id = id;
            IsBonus = isBonus;
        }

        public TileDescriptionId Id { get; }

        public bool IsBonus { get; } // For feature purpose

        public override string ToString()
        {
            return $"Id: {Id}, IsBonus: {IsBonus}";
        }
    }
}