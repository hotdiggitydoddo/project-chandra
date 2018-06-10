using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ProjectChandra.Shared.MapGen.Generators
{
    public class TemplatedMap : Map
    {
        private List<Rectangle> _rooms;
        private List<Rectangle> _hallways;
      

        public TemplatedMap(int w, int h) : base(w, h)
        {
            _rooms = new List<Rectangle>();
            _hallways = new List<Rectangle>();
        }


    }
}
