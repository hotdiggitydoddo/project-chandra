using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;

namespace ProjectChandra.Shared.MapGen
{
    public abstract class Map
    {
        protected TileType[] _cells;
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public Map(int w, int h)
        {
            Width = w;
            Height = h;
            _cells = new TileType[Width * Height];
        }

        public TileType GetTile(int x, int y)
        {
            return _cells[y * Width + x];
        }

        public void SetTile(int x, int y, TileType type)
        {
            _cells[y * Width + x] = type;
        }

        public List<TileInfo> GetAdjacentTilesSimple(int x, int y)
        {
            return new List<TileInfo>
            {
                new TileInfo
                {
                    RelativeDirection = Direction.Up,
                    Location = new Point(x, y - 1),
                    Tile = GetTile(x, y - 1)
                },
                new TileInfo
                {
                    RelativeDirection = Direction.Down,
                    Location = new Point(x, y + 1),
                    Tile = GetTile(x, y + 1)
                },
                new TileInfo
                {
                    RelativeDirection = Direction.Left,
                    Location = new Point(x - 1, y),
                    Tile = GetTile(x - 1, y)
                },
                new TileInfo
                {
                    RelativeDirection = Direction.Right,
                    Location = new Point(x + 1, y),
                    Tile = GetTile(x + 1, y)
                }
            };
        }
    }
}
