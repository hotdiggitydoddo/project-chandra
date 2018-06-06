using System.Linq;
using Nez.Tiled;

namespace ProjectChandra.Shared.Components
{
    /// <summary>
    /// A highly simplified tilemap that can be loaded from an array.
    /// </summary>
    public class CustomTiledMap : TiledMap
    {
        public CustomTiledMap(
            int firstGid,
            int width,
            int height,
            int tileWidth,
            int tileHeight,
            TiledMapOrientation orientation = TiledMapOrientation.Orthogonal) : base(
            firstGid,
            width,
            height,
            tileWidth,
            tileHeight
            )
        { }


        public CustomTiledMap loadFromArray(string name, int[] tileData, int width, int height, TiledTileset tileset, int tileWidth,
            int tileHeight)
        {
            var tiles = tileData.Select(x => new TiledTile(x) { tileset = tileset }).ToArray();
            var tileLayer = createTileLayer(name, width, height, tiles);

            return this;
        }
    }
}
