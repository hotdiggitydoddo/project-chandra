using System;
using Nez;
using ProjectChandra.Shared.MapGen;

namespace ProjectChandra.Shared.Components
{
    public class FOVComponent : Component, IUpdatable
    {
        private MapGen.GameMap _map;

        public int Radius { get; set; }
        public bool LightWalls { get; set; }

        public FOVComponent(MapGen.GameMap map, int radius = 4, bool lightWalls = true)
        {
            Radius = radius;
            LightWalls = lightWalls;
        }

        public void update()
        {
            _map.ComputeFov((int)entity.position.X, (int)entity.position.Y, Radius, LightWalls);
        }
    }
}
