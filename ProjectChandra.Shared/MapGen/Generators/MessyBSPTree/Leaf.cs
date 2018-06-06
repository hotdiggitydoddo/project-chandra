using System;
using Microsoft.Xna.Framework;

namespace ProjectChandra.Shared.MapGen.Generators
{
    public class Leaf
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public static int MinLeafSize = 10;
        public Leaf Child1;
        public Leaf Child2;
        public Rectangle? Room;
        public Rectangle Hall;

        public Leaf(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        public bool Split()
        {
            if (Child1 != null || Child2 != null)
                return false;

            /*
                ==== Determine the direction of the split ====
                If the width of the leaf is >25% larger than the height,
                split the leaf vertically.
                If the height of the leaf is >25 larger than the width,
                split the leaf horizontally.
                Otherwise, choose the direction at random.
             */
            var splitHorizontally = Nez.Random.choose(true, false);

            if (Width / Height >= 1.25f)
                splitHorizontally = false;
            else if (Height / Width >= 1.25f)
                splitHorizontally = true;

            var max = splitHorizontally ? Height - MinLeafSize : Width - MinLeafSize;

            if (max <= MinLeafSize)
                return false; //the leaf is too small to split further

            var split = Nez.Random.range(MinLeafSize, max);

            if (splitHorizontally)
            {
                Child1 = new Leaf(X, Y, Width, split);
                Child2 = new Leaf(X, Y + split, Width, Height - split);
            }
            else
            {
                Child1 = new Leaf(X, Y, split, Height);
                Child2 = new Leaf(X + split, Y, Width - split, Height);
            }

            return true;
        }

        public void CreateRooms(MessyBSPTreeMap map)
        {
            if (Child1 != null || Child2 != null)
            {
                // recursively search for children until you hit the end of the branch
                if (Child1 != null)
                    Child1.CreateRooms(map);
                if (Child2 != null)
                    Child2.CreateRooms(map);

                if (Child1 != null && Child2 != null)
                    map.CreateHall(Child1.GetRoom().Value, Child2.GetRoom().Value);
            }
            else
            {
                // Create rooms in the end branches of the bsp tree
                var w = Nez.Random.range(MessyBSPTreeMapGenerator.RoomMinSize, Math.Min(MessyBSPTreeMapGenerator.RoomMaxSize, Width - 1));
                var h = Nez.Random.range(MessyBSPTreeMapGenerator.RoomMinSize, Math.Min(MessyBSPTreeMapGenerator.RoomMaxSize, Height - 1));
                var x = Nez.Random.range(X, X + (Width - 1) - w);
                var y = Nez.Random.range(Y, Y + (Height - 1) - h);
                Room = new Rectangle(x, y, w, h);
                map.CreateRoom(Room.Value);
            }
        }

        public Rectangle? GetRoom()
        {
            Rectangle? room1 = null;
            Rectangle? room2 = null;

            if (Room.HasValue)
                return Room;

            if (Child1 != null)
                room1 = Child1.GetRoom();
            if (Child2 != null)
                room2 = Child2.GetRoom();

            if (Child1 == null && Child2 == null)
                return null;
            else if (!room2.HasValue)
                return room1;
            else if (!room1.HasValue)
                return room2;

            else if (Nez.Random.choose(true, false) == true)
                return room1;
            else
                return room2;

        }

    }
}
