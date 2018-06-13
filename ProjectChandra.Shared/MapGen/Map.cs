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
            cardinalDirs.AddRange(new[]
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

        private int ClampX(int x)
        {
            return (x < 0) ? 0 : (x > Width - 1) ? Width - 1 : x;
        }

        private int ClampY(int y)
        {
            return (y < 0) ? 0 : (y > Height - 1) ? Height - 1 : y;
        }

        public IEnumerable<Point> GetCellsAlongLine(int xOrigin, int yOrigin, int xDestination, int yDestination)
        {
            xOrigin = ClampX(xOrigin);
            yOrigin = ClampY(yOrigin);
            xDestination = ClampX(xDestination);
            yDestination = ClampY(yDestination);

            int dx = Math.Abs(xDestination - xOrigin);
            int dy = Math.Abs(yDestination - yOrigin);

            int sx = xOrigin < xDestination ? 1 : -1;
            int sy = yOrigin < yDestination ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                yield return new Point(xOrigin, yOrigin);
                if (xOrigin == xDestination && yOrigin == yDestination)
                {
                    break;
                }
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    xOrigin = xOrigin + sx;
                }
                if (e2 < dx)
                {
                    err = err + dx;
                    yOrigin = yOrigin + sy;
                }
            }
        }
    }
}
