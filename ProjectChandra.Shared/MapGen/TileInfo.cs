using Microsoft.Xna.Framework;
using ProjectChandra.Shared.Helpers;

namespace ProjectChandra.Shared.MapGen
{
    public struct TileInfo
    {
        public Point Location { get; set; }
        public Direction RelativeDirection { get; set; }
        public TileType Tile { get; set; }

    }
}
