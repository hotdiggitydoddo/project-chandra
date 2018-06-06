using System;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectChandra.Shared.MapGen.Generators
{
    public class MessyBSPTreeMap : Map
    {
        
        public MessyBSPTreeMap(int w, int h) : base(w, h)
        {
        }

        public void CreateRoom(Rectangle room)
        {
            // Set all tiles within a rectangle to 0
            for (var y = room.Top + 1; y < room.Bottom; y++)
                for (var x = room.Left + 1; x < room.Right; x++)
                    SetTile(x, y, TileType.Empty);
        }


        public void CreateHall(Rectangle room1, Rectangle room2)
        {
            // run a heavily weighted random Walk 
            // from point2 to point1

            var drunkardPoint = room2.Center;
            var goalPoint = room1.Center;

            while (!(room1.Left <= drunkardPoint.X && drunkardPoint.X <= room1.Right)
                    || !(room1.Top < drunkardPoint.Y && drunkardPoint.Y < room1.Bottom))
            {
                var north = 1.0f;
                var south = 1.0f;
                var east = 1.0f;
                var west = 1.0f;
                var weight = 1;

                // weight the random walk against edges
                if (drunkardPoint.X < goalPoint.X) //drunkard is left of point1
                    east += weight;
                else if (drunkardPoint.X > goalPoint.X) // drunkard is right of point1
                    west += weight;
                if (drunkardPoint.Y < goalPoint.Y) //drunkard is above point1
                    south += weight;
                else if (drunkardPoint.Y > goalPoint.Y) // drunkard is below point1
                    north += weight;

                int dx, dy;

                // normalize probabilities so they form a range from 0 to 1
                var total = north + south + east + west;
                north /= total;
                south /= total;
                east /= total;
                west /= total;

                // choose the direction
                var choice = Nez.Random.nextFloat();

                if (0 <= choice && choice < north)
                {
                    dx = 0;
                    dy = -1;
                }
                else if (north <= choice && choice < (north + south))
                {
                    dx = 0;
                    dy = 1;
                }
                else if ((north + south) <= choice && choice < (north + south + east))
                {
                    dx = 1;
                    dy = 0;
                }
                else
                {
                    dx = -1;
                    dy = 0;
                }

                // ==== Walk ====
                // check colision at edges
                if ((0 < drunkardPoint.X + dx && drunkardPoint.X + dx < Width - 1)
                    && (0 < drunkardPoint.Y + dy && drunkardPoint.Y + dy < Height - 1))
                {
                    drunkardPoint.X += dx;
                    drunkardPoint.Y += dy;

                    var cell = GetTile(drunkardPoint.X, drunkardPoint.Y);
                    if (cell != TileType.Empty)
                        SetTile(drunkardPoint.X, drunkardPoint.Y, TileType.Empty);

                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var tile = GetTile(x, y);
                    sb.Append(tile == TileType.Empty ? "." : "#");
                }
                sb.Append("\n");
            }
            //for (var y = 0; y < Height; y++)
            //{
            //    for (var x = 0; x < Width; x++)
            //    {
            //        sb.Append(_cells[x + y * Width] == 0 ? "." : "#");
            //    }
            //    sb.Append('\n');
            //}
            return sb.ToString();
        }
    }
}
