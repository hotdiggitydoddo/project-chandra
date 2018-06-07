using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ProjectChandra.Shared.MapGen.Generators
{
    public class RoomTemplate
    {
        TileType[] _cells;
        int _width;
        int _height;
        string _name;

        public Rectangle Bounds => new Rectangle(0, 0, _width, _height);

        public RoomTemplate(int width, int height, string roomData = null, string name = null)
        {
            _width = width;
            _height = height;

            _cells = new TileType[width * height];
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    _cells[x + y * width] = TileType.Unused;

            if (roomData == null)
                return;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    switch (roomData[x + y * width])
                    {
                        case '.':
                            _cells[x + y * width] = TileType.Empty;
                            break;
                        case 'w':
                            _cells[x + y * width] = TileType.Wall;
                            break;
                        case 'x':
                            _cells[x + y * width] = TileType.Unused;
                            break;
                        case 'd':
                            _cells[x + y * width] = TileType.Door;
                            break;
                    }
                }
            }
            _name = name;
        }

        public TileType GetCell(int x, int y)
        {
            return _cells[x + y * _width];
        }

        public List<Point> GetEdgeCells()
        {
            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {

                }
            }

            return null;
        }
    }
}
