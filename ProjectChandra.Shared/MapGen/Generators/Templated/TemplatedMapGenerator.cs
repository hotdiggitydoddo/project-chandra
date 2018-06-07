using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using ProjectChandra.Shared.Helpers;

namespace ProjectChandra.Shared.MapGen.Generators
{
    public class TemplatedMapGenerator
    {
        private TemplatedMap _map;
        private List<RoomTemplate> _templates;
        private List<Rectangle> _rooms;

        public int MinRoomSize { get; set; }
        public int MaxRoomSize { get; set; }
        public int MaxTries { get; set; }
        public int DesiredRoomCount { get; set; }

        public TemplatedMapGenerator(int minRoomSize = 6, int maxRoomSize = 20, int maxTries = 1000, int desiredRoomCount = 30)
        {
            _templates = new List<RoomTemplate>();
            MinRoomSize = minRoomSize;
            MaxRoomSize = maxRoomSize;
            MaxTries = maxTries;
            DesiredRoomCount = desiredRoomCount;
        }

        public TemplatedMap CreateMap(int w, int h)
        {
            _map = new TemplatedMap(w, h);
            _rooms = new List<Rectangle>();

            Clear();

            bool startingRoomPlaced = false;
            while (!startingRoomPlaced)
            {
                startingRoomPlaced = TryPlaceStartingRoom(startingRoomPlaced);
            }

            var numTries = 0;

            while (_rooms.Count < DesiredRoomCount && numTries < MaxTries)
            {
                numTries++;
                KeyValuePair<Point, TileInfo> candidate;

                try
                {
                    var candidateWallTiles = GetCandidateWallTiles();
                    var randomCandidateIndex = Nez.Random.range(0, candidateWallTiles.Count);

                    // Choose a candidate at random
                    candidate = candidateWallTiles.ElementAt(randomCandidateIndex);

                    // Create a door at the candidate location
                    _map.SetTile(candidate.Key.X, candidate.Key.Y, TileType.Door);

                    //TODO: choose to make hallway or room?  Right now, just make rooms.
                    TryMakeRoom(candidate);
                }
                catch
                {
                    _map.SetTile(candidate.Key.X, candidate.Key.Y, TileType.Wall);
                    continue;
                }
            }

            //Ensure no doors open up to walls.
            CheckAndMoveDoors();

            return _map;
        }



        public void AddTemplates(params RoomTemplate[] templates)
        {
            _templates.AddRange(templates);
        }

        private void CheckAndMoveDoors()
        {
            var wholeMap = _map.GetMap();

            for (int i = 0; i < wholeMap.Length; i++)
            {
                if (wholeMap[i] != TileType.Door)
                    continue;
                var doorLoc = _map.GetTileLocation(i);
                var adjacents = _map.GetAdjacentTiles8Ways(doorLoc.X, doorLoc.Y);

                var cardinalDirs = new[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
                var cardinalTiles = adjacents.Where(x => cardinalDirs.Contains(x.RelativeDirection));

                if (cardinalTiles.Count(x => x.Tile == TileType.Wall) != 3)
                    continue;

                if (cardinalTiles.Any(x => x.Tile == TileType.Door))
                {
                    foreach (var doorTile in cardinalTiles.Where(x => x.Tile == TileType.Door))
                        _map.SetTile(doorTile.Location.X, doorTile.Location.Y, TileType.Wall);
                }

                var floorTile = cardinalTiles.Single(x => x.Tile == TileType.Empty);

                // Figure out what axis on which this door is supposed to swing open
                var isXAxis = floorTile.RelativeDirection == Direction.Left || floorTile.RelativeDirection == Direction.Right;

                if (isXAxis)
                {
                    // Can we move the door up?
                    var upperLeft = adjacents.Single(x => x.RelativeDirection == Direction.UpperLeft);
                    var upperRight = adjacents.Single(x => x.RelativeDirection == Direction.UpperRight);
                    if (upperLeft.Tile == TileType.Empty && upperRight.Tile == TileType.Empty)
                    {
                        // We can move the door up
                        var up = cardinalTiles.Single(x => x.RelativeDirection == Direction.Up);
                        _map.SetTile(up.Location.X, up.Location.Y, TileType.Door);
                        _map.SetTile(doorLoc.X, doorLoc.Y, TileType.Wall);
                        continue;
                    }

                    // Can we move the door down?
                    var lowerLeft = adjacents.Single(x => x.RelativeDirection == Direction.LowerLeft);
                    var lowerRight = adjacents.Single(x => x.RelativeDirection == Direction.LowerRight);
                    if (lowerLeft.Tile == TileType.Empty && lowerRight.Tile == TileType.Empty)
                    {
                        // We can move the door down
                        var down = cardinalTiles.Single(x => x.RelativeDirection == Direction.Down);
                        _map.SetTile(down.Location.X, down.Location.Y, TileType.Door);
                        _map.SetTile(doorLoc.X, doorLoc.Y, TileType.Wall);
                        continue;
                    }
                }
                else // y axis
                {
                    // Can we move the door left?
                    var upperLeft = adjacents.Single(x => x.RelativeDirection == Direction.UpperLeft);
                    var lowerLeft = adjacents.Single(x => x.RelativeDirection == Direction.LowerLeft);

                    if (upperLeft.Tile == TileType.Empty && lowerLeft.Tile == TileType.Empty)
                    {
                        // We can move the door left
                        var left = cardinalTiles.Single(x => x.RelativeDirection == Direction.Left);
                        _map.SetTile(left.Location.X, left.Location.Y, TileType.Door);
                        _map.SetTile(doorLoc.X, doorLoc.Y, TileType.Wall);
                        continue;
                    }

                    // Can we move the door right?
                    var upperRight = adjacents.Single(x => x.RelativeDirection == Direction.UpperRight);
                    var lowerRight = adjacents.Single(x => x.RelativeDirection == Direction.LowerRight);
                    if (upperRight.Tile == TileType.Empty && lowerRight.Tile == TileType.Empty)
                    {
                        // We can move the door right
                        var right = cardinalTiles.Single(x => x.RelativeDirection == Direction.Right);
                        _map.SetTile(right.Location.X, right.Location.Y, TileType.Door);
                        _map.SetTile(doorLoc.X, doorLoc.Y, TileType.Wall);
                        continue;
                    }
                }
            }
        }

        private void TryMakeRoom(KeyValuePair<Point, TileInfo> candidate)
        {
            var buildDirection = candidate.Value.RelativeDirection;
            var template = _templates[Nez.Random.range(0, _templates.Count)];

            int tx, ty;

            switch (buildDirection)
            {
                case Direction.Left:
                    tx = (candidate.Key.X) - template.Bounds.Width;
                    ty = candidate.Key.Y - template.Bounds.Center.Y;
                    break;
                case Direction.Right:
                    tx = candidate.Key.X + 1;
                    ty = candidate.Key.Y - template.Bounds.Center.Y;
                    break;
                case Direction.Up:
                    tx = candidate.Key.X - template.Bounds.Center.X;
                    ty = (candidate.Key.Y) - template.Bounds.Height;
                    break;
                default:
                    tx = candidate.Key.X - template.Bounds.Center.X;
                    ty = candidate.Key.Y + 1;
                    break;
            }


            var templateArea = new Rectangle(tx, ty, template.Bounds.Width, template.Bounds.Height);

            // If the template contains unused space, use it to help position it against the candidate
            if (buildDirection == Direction.Down)
            {
                var ready = false;
                while (!ready)
                {
                    if (template.GetCell(candidate.Key.X - templateArea.X, (candidate.Key.Y + 1) - templateArea.Y) == TileType.Unused)
                        templateArea.Offset(0, -1);
                    else
                        ready = true;
                }
            }
            else if (buildDirection == Direction.Up)
            {
                var ready = false;
                while (!ready)
                {
                    if (template.GetCell(candidate.Key.X - templateArea.X, (candidate.Key.Y - 1) - templateArea.Y) == TileType.Unused)
                        templateArea.Offset(0, +1);
                    else
                        ready = true;
                }
            }
            else if (buildDirection == Direction.Right)
            {
                var ready = false;
                while (!ready)
                {
                    if (template.GetCell((candidate.Key.X + 1) - templateArea.X, candidate.Key.Y - templateArea.Y) == TileType.Unused)
                        templateArea.Offset(-1, 0);
                    else
                        ready = true;
                }
            }
            else // Left
            {
                var ready = false;
                while (!ready)
                {
                    if (template.GetCell((candidate.Key.X - 1) - templateArea.X, candidate.Key.Y - templateArea.Y) == TileType.Unused)
                        templateArea.Offset(+1, 0);
                    else
                        ready = true;
                }
            }

            // Project the template onto the map and bail out if there are any intersections
            var intersects = false;

            for (var y = templateArea.Top; y < templateArea.Bottom; y++)
            {
                for (var x = templateArea.Left; x < templateArea.Right; x++)
                {
                    var templateCell = template.GetCell(x - templateArea.X, y - templateArea.Y);
                    var mapCell = _map.GetTile(x, y);

                    if (mapCell == TileType.Unused)
                        continue;
                    if (templateCell == TileType.Unused)
                        continue;
                    if (mapCell == TileType.Wall && templateCell == TileType.Wall)
                        continue;

                    intersects = true;
                    break;
                }
                if (intersects)
                {
                    // Put the wall back
                    _map.SetTile(candidate.Key.X, candidate.Key.Y, TileType.Wall);
                    break;
                }
            }

            if (intersects)
                return;

            // Set the new template in place on the actual map
            for (var y = templateArea.Top; y < templateArea.Bottom; y++)
            {
                for (var x = templateArea.Left; x < templateArea.Right; x++)
                {
                    var tCell = template.GetCell(x - templateArea.X, y - templateArea.Y);
                    if (tCell == TileType.Unused)
                        continue;
                    
                    _map.SetTile(x, y, template.GetCell(x - templateArea.X, y - templateArea.Y));
                }
            }

            for (var y = 1; y < _map.Height - 1; y++)
            {
                for (var x = 1; x < _map.Width - 1; x++)
                {
                    var tile = _map.GetTile(x, y);
                    if (tile != TileType.Empty)
                        continue;

                    var adjacents = _map.GetAdjacentTiles8Ways(x, y);
                    foreach (var adjacent in adjacents)
                        if (adjacent.Tile == TileType.Unused)
                            _map.SetTile(adjacent.Location.X, adjacent.Location.Y, TileType.Wall);
                }
            }

            _rooms.Add(templateArea);
        }

        private Dictionary<Point, TileInfo> GetCandidateWallTiles()
        {
            var retVal = new Dictionary<Point, TileInfo>();
            var allWalls = new Dictionary<int, int>();

            var tiles = _map.GetMap();

            for (var i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != TileType.Wall)
                    continue;

                // Possible candidate identified
                var cellLocation = _map.GetTileLocation(i);

                if (cellLocation.X == 0 || cellLocation.X == _map.Width - 1 || cellLocation.Y == 0 || cellLocation.Y == _map.Height - 1)
                    continue;

                var neighbors = _map.GetAdjacentTilesSimple(cellLocation.X, cellLocation.Y);

                var rNeighbor = neighbors.Single(x => x.RelativeDirection == Direction.Right);
                var lNeighbor = neighbors.Single(x => x.RelativeDirection == Direction.Left);
                var uNeighbor = neighbors.Single(x => x.RelativeDirection == Direction.Up);
                var dNeighbor = neighbors.Single(x => x.RelativeDirection == Direction.Down);

                var neighborsAlongX = rNeighbor.Tile == TileType.Wall && lNeighbor.Tile == TileType.Wall;
                var neighborsAlongY = uNeighbor.Tile == TileType.Wall && dNeighbor.Tile == TileType.Wall;

                if (!neighborsAlongX && !neighborsAlongY)
                    continue;

                // Candidate has neighboring walls along one axis

                // Is the candidate exposed to unused space?
                TileInfo? unusedTile = null;

                if (neighborsAlongX)
                {
                    if (uNeighbor.Tile == TileType.Unused)
                        unusedTile = uNeighbor;
                    else if (dNeighbor.Tile == TileType.Unused)
                        unusedTile = dNeighbor;
                }
                else if (neighborsAlongY)
                {
                    if (rNeighbor.Tile == TileType.Unused)
                        unusedTile = rNeighbor;
                    else if (lNeighbor.Tile == TileType.Unused)
                        unusedTile = lNeighbor;
                }

                if (!unusedTile.HasValue)
                    continue;

                // Winner winner, chicken dinner!
                retVal.Add(cellLocation, unusedTile.Value);
            }
            return retVal;
        }

        private bool TryPlaceStartingRoom(bool startingRoomPlaced)
        {
            var startingWidth = Nez.Random.range(MinRoomSize / 2, MaxRoomSize / 2);
            var startingHeight = Nez.Random.range(MinRoomSize / 2, MaxRoomSize / 2);
            var sx = Nez.Random.range(2, _map.Width - 2 - startingWidth);
            var sy = Nez.Random.range(2, _map.Height - 2 - startingHeight);

            var roomRect = new Rectangle(sx, sy, startingWidth, startingHeight);
            roomRect.Inflate(1, 1);
            if (_map.Bounds.Contains(roomRect))
            {
                for (var y = roomRect.Top; y < roomRect.Bottom; y++)
                    for (var x = roomRect.Left; x < roomRect.Right; x++)
                        if (x == roomRect.Left || x == roomRect.Right - 1 || y == roomRect.Top || y == roomRect.Bottom - 1)
                            _map.SetTile(x, y, TileType.Wall);
                        else
                            _map.SetTile(x, y, TileType.Empty);

                _rooms.Add(roomRect);
                startingRoomPlaced = true;
            }

            return startingRoomPlaced;
        }

        private void Clear()
        {
            for (var y = 0; y < _map.Height; y++)
                for (var x = 0; x < _map.Width; x++)
                    if (x == 0 || x == _map.Width - 1 || y == 0 || y == _map.Height - 1)
                        _map.SetTile(x, y, TileType.Wall);
                    else
                        _map.SetTile(x, y, TileType.Unused);

        }
    }
}
