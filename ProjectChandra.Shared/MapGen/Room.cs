using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ProjectChandra.Shared.MapGen
{
    public class Room
    {
        private GameMap _map;

        public List<Point> Cells;
        public List<Point> EdgeCells;
        public List<Room> ConnectedRooms;
        public int RoomSize => Cells.Count;

        public Room(List<Point> roomCells, GameMap map)
        {
            Cells = roomCells;
            ConnectedRooms = new List<Room>();
            EdgeCells = new List<Point>();

            foreach (var cell in roomCells)
            {
                
            }
        }
    }
}