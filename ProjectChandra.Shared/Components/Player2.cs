using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using ProjectChandra.Shared.MapGen;
using ProjectChandra.Shared.Tiles;

namespace ProjectChandra.Shared.Components
{
    public class Player2 : Component, IUpdatable
    {
        private GameMap _map;
        private CustomTiledMapMover _mover;

        private VirtualIntegerAxis _xAxisInput;
        private VirtualIntegerAxis _yAxisInput;

        public int FOVRadius;
        public bool LightWalls;
        public bool AllowDiagonals;

        public Player2(GameMap map)
        {
            setupInput();
            _map = map;
            FOVRadius = 3;
            LightWalls = true;
        }

        public override void onAddedToEntity()
        {
            _mover = entity.getComponent<CustomTiledMapMover>();
        }

        public void update()
        {
            if (!_mover.IsMoving)
            {
                var moveDir = new Vector2(_xAxisInput.value, _yAxisInput.value);

                if (!AllowDiagonals)
                {
                    if (Math.Abs(moveDir.X) > Math.Abs(moveDir.Y))
                    {
                        moveDir.Y = 0;
                    }
                    else
                    {
                        moveDir.X = 0;
                    }
                }
                if (moveDir != Vector2.Zero)
                    Core.startCoroutine(_mover.Move(moveDir));
            }
            UpdateFOV();
        }

        private void UpdateFOV()
        {
            var cells = _map.ComputeFov((int)entity.position.X / 32, (int)entity.position.Y / 32, FOVRadius, LightWalls);
            foreach (var cell in cells)
                _map.SetCellProperties(cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable, true);
        }


        void setupInput()
        {
            // horizontal input from dpad, left stick or keyboard left/right
            _xAxisInput = new VirtualIntegerAxis();
            _xAxisInput.nodes.Add(new VirtualAxis.GamePadDpadLeftRight());
            _xAxisInput.nodes.Add(new VirtualAxis.GamePadLeftStickX());
            _xAxisInput.nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right));

            // vertical input from dpad, left stick or keyboard up/down
            _yAxisInput = new VirtualIntegerAxis();
            _yAxisInput.nodes.Add(new VirtualAxis.GamePadDpadUpDown());
            _yAxisInput.nodes.Add(new VirtualAxis.GamePadLeftStickY());
            _yAxisInput.nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Up, Keys.Down));

        }
    }
}
