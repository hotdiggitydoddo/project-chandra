using System;
using System.Collections.Generic;
using System.Linq;

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

            return _map;
        }

        private void Clear()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
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
                for (var x = 1; x < Height - 1; x++)
                {
                    for (var y = 1; y < Width - 1; y++)
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
        private int GetAdjacentWalls(int x, int y)
        {
            var adjacents = _map.GetAdjacentTilesSimple(x, y);
            return adjacents.Count(o => o.Tile == TileType.Wall);
        }
    }
}
