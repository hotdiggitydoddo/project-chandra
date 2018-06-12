using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez.AI.GOAP;
using Nez.AI.Pathfinding;
using ProjectChandra.Shared.Helpers;

namespace ProjectChandra.Shared.MapGen.Generators
{
    public class TemplatedMapGenerator
    {
        private TemplatedMap _map;
        private List<RoomTemplate> _templates;
        private List<Rectangle> _rooms;
        private List<KeyValuePair<Point, Direction>> _doors;

        public int MinRoomSize { get; set; }
        public int MaxRoomSize { get; set; }
        public float TemplatedAreaChance { get; set; }
        public float HallwayChance { get; set; }
        public int MaxTries { get; set; }
        public int DesiredRoomCount { get; set; }

        public TemplatedMapGenerator(int minRoomSize = 6, int maxRoomSize = 20, int maxTries = 1000, 
            int desiredRoomCount = 30, float templatedAreaChance = .5f, float hallwayChance = .25f)
        {
            _templates = new List<RoomTemplate>();
            MinRoomSize = minRoomSize;
            MaxRoomSize = maxRoomSize;
            MaxTries = maxTries;
            DesiredRoomCount = desiredRoomCount;
            TemplatedAreaChance = templatedAreaChance;
            HallwayChance = hallwayChance;
        }

        public TemplatedMap CreateMap(int w, int h)
        {
            var mapCreated = false;

            while (!mapCreated)
            {
                _map = new TemplatedMap(w, h);
                _rooms = new List<Rectangle>();
                _doors = new List<KeyValuePair<Point, Direction>>();

                Clear();

                bool startingRoomPlaced = false;
                while (!startingRoomPlaced)
                {
                    startingRoomPlaced = TryPlaceStartingRoom();
                }

                var numTries = 0;

                while (_rooms.Count < DesiredRoomCount && numTries < MaxTries)
                {
                    numTries++;
                    KeyValuePair<Point, TileInfo> candidate;

                    try
                    {

                        // First, check if we created a hallway last iteration.  If so, we must try to connect it
                        //if (lastHallwayEndpoint.HasValue)
                        //{
                        //    // choose a direction, step in that direction (intersecting the wall now,) 
                        //    // and create a door in the wall.  if the door connects to something, great,
                        //    // otherwise, we need to build something from this door point.
                        //    var graph = new AStarPath(_map);
                        //    var otherRoom = _rooms[Nez.Random.nextInt(_rooms.Count - 1)];

                        //    var path = graph.search(lastHallwayEndpoint.Value, otherRoom.Center);
                        //    foreach(var p in path)
                        //    {
                        //        _map.SetTile(p.X, p.Y, TileType.Empty);
                        //    }
                        //}
                        //else
                        {
                            // get a candidate and choose whether or not to insert something procedural 
                            // (room or hallway) or a templated unit

                            // var candidateWallTiles = Nez.Random.choose(true, false) 
                            // ? GetCandidateWallTiles( _rooms.Last())
                            // : GetCandidateWallTiles();

                             var candidateWallTiles = GetCandidateWallTiles();

                            var randomCandidateIndex = Nez.Random.range(0, candidateWallTiles.Count);

                            // Choose a candidate at random
                            candidate = candidateWallTiles.ElementAt(randomCandidateIndex);

                           

                            //if (Nez.Random.chance(templatedChance))

                            //if (Nez.Random.chance(hallwayChance))
                            //    TryMakeHallway(candidate);
                            //else

                            // Create a door at the candidate location
                            _map.SetTile(candidate.Key.X, candidate.Key.Y, TileType.Door);

                            if (!TryMakeRoom(candidate))
                                _map.SetTile(candidate.Key.X, candidate.Key.Y, TileType.Wall);


                        }

                    }
                    catch
                    {
                        _map.SetTile(candidate.Key.X, candidate.Key.Y, TileType.Wall);
                        continue;
                    }
                }
                if (!AreDoorsOk())
                    continue;
                mapCreated = true;
            }

            return _map;
        }



        public void AddTemplates(params RoomTemplate[] templates)
        {
            _templates.AddRange(templates);
        }

        private void TryMakeHallway(KeyValuePair<Point, TileInfo> candidate)
        {
            var buildDirection = candidate.Value.RelativeDirection;
            int tx, ty;
        }

        private bool TryMakeRoom(KeyValuePair<Point, TileInfo> candidate)
        {
            var buildDirection = candidate.Value.RelativeDirection;
            var template = _templates[Nez.Random.range(0, _templates.Count)];

            int tx, ty;

            switch (buildDirection)
            {
                case Direction.Left:
                    tx = (candidate.Key.X) - template.Bounds.Width;
                    ty = candidate.Key.Y - Nez.Random.range(template.Bounds.Top, template.Bounds.Bottom);
                    break;
                case Direction.Right:
                    tx = candidate.Key.X + 1;
                    ty = candidate.Key.Y - Nez.Random.range(template.Bounds.Top, template.Bounds.Bottom);
                    break;
                case Direction.Up:
                    tx = candidate.Key.X - Nez.Random.range(template.Bounds.Left, template.Bounds.Right);
                    ty = (candidate.Key.Y) - template.Bounds.Height;
                    break;
                default:
                    tx = candidate.Key.X - Nez.Random.range(template.Bounds.Left, template.Bounds.Right);
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

                    var adj = _map.GetAdjacentTilesSimple(x, y).Select(p=>p.Location);
                    if (adj.Contains(candidate.Key) && templateCell == TileType.Wall)
                        return false;

                    if (mapCell == TileType.Unused)
                        continue;
                    if (templateCell == TileType.Unused)
                        continue;
                  
                    return false;
                }
            }


            // Set the new template in place on the actual map
            for (var y = templateArea.Top; y < templateArea.Bottom; y++)
            {
                for (var x = templateArea.Left; x < templateArea.Right; x++)
                {
                    var tCell = template.GetCell(x - templateArea.X, y - templateArea.Y);
                    if (tCell == TileType.Unused)
                        continue;

                    _map.SetTile(x, y, tCell);
                }
            }

            for (var y = templateArea.Top; y < templateArea.Bottom; y++)
            {
                for (var x = templateArea.Left; x < templateArea.Right; x++)
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


            _doors.Add(new KeyValuePair<Point, Direction>(candidate.Key, candidate.Value.RelativeDirection));

            _rooms.Add(templateArea);

            return true;
        }
        
        private bool AreDoorsOk()
        {
            return !_doors.Any(x => _map.GetAdjacentTilesSimple(x.Key.X, x.Key.Y).Count(o => o.Tile == TileType.Wall) == 3);
        }

        private Dictionary<Point, TileInfo> GetCandidateWallTiles(Rectangle? room = null)
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

                if (neighbors.Any(x => x.Tile == TileType.Door))
                    continue;

                if (neighborsAlongX)
                {
                    if (uNeighbor.Tile == TileType.Unused && dNeighbor.Tile != TileType.Wall)
                        unusedTile = uNeighbor;
                    else if (dNeighbor.Tile == TileType.Unused && uNeighbor.Tile != TileType.Wall)
                        unusedTile = dNeighbor;
                }
                else if (neighborsAlongY)
                {
                    if (rNeighbor.Tile == TileType.Unused && lNeighbor.Tile != TileType.Wall)
                        unusedTile = rNeighbor;
                    else if (lNeighbor.Tile == TileType.Unused && rNeighbor.Tile != TileType.Wall)
                        unusedTile = lNeighbor;
                }

                if (!unusedTile.HasValue)
                    continue;

                // Winner winner, chicken dinner!
                if (room.HasValue)
                {
                    var roomRect = new Rectangle(room.Value.X, room.Value.Y, room.Value.Width, room.Value.Height);
                    roomRect.Inflate(1, 1);

                    if (roomRect.Contains(cellLocation))
                        retVal.Add(cellLocation, unusedTile.Value);
                }
                else
                    retVal.Add(cellLocation, unusedTile.Value);

            }
            return retVal;
        }

        private bool TryPlaceStartingRoom()
        {
            var startingRoomPlaced = false;
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
            _rooms.Clear();
            _doors.Clear();
            for (var y = 0; y < _map.Height; y++)
                for (var x = 0; x < _map.Width; x++)
                    if (x == 0 || x == _map.Width - 1 || y == 0 || y == _map.Height - 1)
                        _map.SetTile(x, y, TileType.Wall);
                    else
                        _map.SetTile(x, y, TileType.Unused);
        }
    }
}
