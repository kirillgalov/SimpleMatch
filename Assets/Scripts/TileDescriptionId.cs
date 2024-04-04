using System;

namespace SimpleMatch
{
    public class TileDescriptionId : IEquatable<TileDescriptionId>
    {
        private readonly string _id;

        public TileDescriptionId(string id)
        {
            _id = id;
        }

        public bool Equals(TileDescriptionId other)
        {
            return other != null && _id == other._id;
        }

        public override bool Equals(object obj)
        {
            return obj is TileDescriptionId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id != null ? _id.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return _id;
        }

        public static bool operator ==(TileDescriptionId a, TileDescriptionId b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            if (a is null)
            {
                return false;
            }

            if (b is null)
            {
                return false;
            }
            
            return a._id == b._id;
        }

        public static bool operator !=(TileDescriptionId a, TileDescriptionId b)
        {
            return !(a == b);
        }
    }
}