using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Direction = ProjectChandra.Shared.Helpers.Direction;

namespace ProjectChandra.Shared.MapGen
{
    public abstract class Map
    {
        protected TileType[] _cells;
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public Rectangle Bounds { get; private set; }

        public Map(int w, int h)
        {
            Width = w;
            Height = h;
            _cells = new TileType[Width * Height];
            Bounds = new Rectangle(1, 1, Width - 1, Height - 1);
        }

        public TileType GetTile(int x, int y)
        {
            return _cells[x + y * Width];
        }

        public void SetTile(int x, int y, TileType type)
        {
            _cells[x + y * Width] = type;
        }

        public TileType[] GetMap()
        {
            return _cells;
        }

        public Point GetTileLocation(int i)
        {
            return new Point(i % Width, i / Width);
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
                    RelativeDirection = Direction.Right,
                    Location = new Point(x + 1, y),
                    Tile = GetTile(x + 1, y)
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
                }
            };
        }

        public List<TileInfo> GetAdjacentTiles8Ways(int x, int y)
        {
            var cardinalDirs = GetAdjacentTilesSimple(x, y);
            cardinalDirs.AddRange(new []
            {
                new TileInfo
                {
                    RelativeDirection = Direction.UpperRight,
                    Location = new Point(x + 1, y - 1),
                    Tile = GetTile(x + 1, y - 1)
                },
                new TileInfo
                {
                    RelativeDirection = Direction.LowerRight,
                    Location = new Point(x + 1, y + 1),
                    Tile = GetTile(x + 1, y + 1)
                },
                new TileInfo
                {
                    RelativeDirection = Direction.LowerLeft,
                    Location = new Point(x - 1, y + 1),
                    Tile = GetTile(x - 1, y + 1)
                },
                new TileInfo
                {
                    RelativeDirection = Direction.UpperLeft,
                    Location = new Point(x - 1, y - 1),
                    Tile = GetTile(x - 1, y - 1)
                }
            });
            return cardinalDirs;
        }
    }
}
