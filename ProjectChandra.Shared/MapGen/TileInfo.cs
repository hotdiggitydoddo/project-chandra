using Microsoft.Xna.Framework;
using ProjectChandra.Shared.Helpers;

namespace ProjectChandra.Shared.MapGen
{
    public struct TileInfo
    {
        public Point Location { get; set; }
        public Direction RelativeDirection { get; set; }
        public TileType Tile { get; set; }


        public override bool Equals(object other)
        {
            var otherInfo = (TileInfo)other;
            return Location == otherInfo.Location && RelativeDirection == otherInfo.RelativeDirection && Tile == otherInfo.Tile;
        }
    }
}
