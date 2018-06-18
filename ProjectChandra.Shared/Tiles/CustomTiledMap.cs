using System;
using System.Linq;
using Nez.Tiled;
using ProjectChandra.Shared.MapGen;

namespace ProjectChandra.Shared.Tiles
{
    public class CustomTiledMap : TiledMap
    {
        private GameMap _map;

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
            int tileHeight, GameMap map)
        {
            _map = map;
            var tiles = tileData.Select(x => new TiledTile(x) { tileset = tileset }).ToArray();
            var tileLayer = createTileLayer(name, width, height, tiles);

            return this;
        }

        public new TiledLayer createTileLayer(string name, int width, int height, TiledTile[] tiles)
        {
            if (orientation == TiledMapOrientation.Orthogonal)
            {
                var layer = new CustomTiledLayer(_map, this, name, width, height, tiles);
                layers.Add(layer);
                return layer;
            }

            throw new NotImplementedException();
        }
    }
}
