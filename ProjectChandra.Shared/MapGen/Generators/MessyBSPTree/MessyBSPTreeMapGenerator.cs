using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using ProjectChandra.Shared.Helpers;

namespace ProjectChandra.Shared.MapGen.Generators
{
    public class MessyBSPTreeMapGenerator
    {
        private Leaf _rootLeaf;
        private List<Leaf> _leafs;
        private MessyBSPTreeMap _map;

        public int Width { get; set; }
        public int Height { get; set; }
        public int MaxLeafSize { get; set; }
        public static int RoomMaxSize { get; set; }
        public static int RoomMinSize { get; set; }
        public bool SmoothEdges { get; set; }
        public int Smoothing { get; set; }
        public int Filling { get; set; }

        public MessyBSPTreeMapGenerator(int w, int h, bool smoothEdges = true, int smoothing = 1,
                                        int filling = 3, int maxLeafSize = 24, int roomMaxSize = 15, int roomMinSize = 6)
        {
            Width = w;
            Height = h;
            SmoothEdges = smoothEdges;
            Smoothing = smoothing;
            Filling = filling;
            MaxLeafSize = maxLeafSize;
            RoomMaxSize = roomMaxSize;
            RoomMinSize = roomMinSize;
        }

        public MessyBSPTreeMap CreateMap(int w = 0, int h = 0)
        {
            _map = new MessyBSPTreeMap(w > 0 ? w : Width, h > 0 ? h : Height);
            Clear();

            _rootLeaf = new Leaf(0, 0, Width, Height);
            _leafs.Add(_rootLeaf);

            SplitLeafs();

            _rootLeaf.CreateRooms(_map);

            if (SmoothEdges)
                CleanUpMap();
            PlaceDoors();
            return _map;
        }

        private void Clear()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _map.SetTile(x, y, TileType.Wall);
                }
            }
            _leafs = new List<Leaf>();
        }

        private void SplitLeafs()
        {
            var splitSuccesfully = true;

            // loop through all leaves until they can no longer split successfully
            while (splitSuccesfully)
            {
                splitSuccesfully = false;

                for (var i = 0; i < _leafs.Count; i++)
                {
                    var leaf = _leafs[i];

                    if (leaf.Child1 == null && leaf.Child2 == null)
                    {
                        if ((leaf.Width > MaxLeafSize)
                            || (leaf.Height > MaxLeafSize)
                            || Nez.Random.nextFloat() > 0.8f)
                        {
                            if (leaf.Split())
                            {
                                _leafs.Add(leaf.Child1);
                                _leafs.Add(leaf.Child2);
                                splitSuccesfully = true;
                            }
                        }
                    }
                }
            }
        }
        private void CleanUpMap()
        {
            for (var i = 0; i < 3; i++)
            {
                // Look at each cell individually and check for smoothness
                for (var y = 1; y < Width - 1; y++)
                {
                    for (var x = 1; x < Height - 1; x++)
                    {
                        var tile = _map.GetTile(x, y);

                        if (tile == TileType.Wall && GetAdjacentWalls(x, y) <= Smoothing)
                            _map.SetTile(x, y, TileType.Empty);

                        if (tile == TileType.Empty && GetAdjacentWalls(x, y) >= Filling)
                            _map.SetTile(x, y, TileType.Wall);
                    }
                }
            }
        }

        private void PlaceDoors()
        {
            foreach (var leaf in _leafs)
            {
                var room = leaf.GetRoom().Value;

                // The the boundries of the room
                int xMin = room.Left;
                int xMax = room.Right;
                int yMin = room.Top;
                int yMax = room.Bottom;

                // Put the rooms border cells into a list
                var borderPoints = _map.GetCellsAlongLine(xMin, yMin, xMax, yMin).ToList();
                borderPoints.AddRange(_map.GetCellsAlongLine(xMin, yMin, xMin, yMax));
                borderPoints.AddRange(_map.GetCellsAlongLine(xMin, yMax, xMax, yMax));
                borderPoints.AddRange(_map.GetCellsAlongLine(xMax, yMin, xMax, yMax));

                foreach (var point in borderPoints)
                {
                    if (IsPotentialDoor(point))
                    {
                        // A door must block field-of-view when it is closed.
                        _map.SetTile(point.X, point.Y, TileType.Door);
                        // _map.Doors.Add(new Door
                        // {
                        //     X = cell.X,
                        //     Y = cell.Y,
                        //     IsOpen = false
                        // });
                    }
                }
            }
        }

        private bool IsPotentialDoor(Point point)
        {
            var tile = _map.GetTile(point.X, point.Y);

            if (tile == TileType.Wall)
                return false;

            var adjacents = _map.GetAdjacentTilesSimple(point.X, point.Y);
            
            if (adjacents.Any(x => x.Tile == TileType.Door))
                return false;

            var xAdjacents = adjacents.Where(x => x.RelativeDirection == Direction.Left || x.RelativeDirection == Direction.Right);
            var yAdjacents = adjacents.Where(x => x.RelativeDirection == Direction.Up || x.RelativeDirection == Direction.Down);

            return ((xAdjacents.All(x => x.Tile == TileType.Wall) && yAdjacents.All(x => x.Tile == TileType.Empty)) 
                || (yAdjacents.All(x => x.Tile == TileType.Wall) && xAdjacents.All(x => x.Tile == TileType.Empty)));
        }

        private int GetAdjacentWalls(int x, int y)
        {
            var adjacents = _map.GetAdjacentTilesSimple(x, y);
            return adjacents.Count(o => o.Tile == TileType.Wall);
        }
    }
}
