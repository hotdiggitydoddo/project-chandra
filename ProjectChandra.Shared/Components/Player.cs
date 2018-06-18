using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Tiled;
using ProjectChandra.Shared.MapGen;
using static Nez.Tiled.TiledMapMover;

namespace ProjectChandra.Shared.Components
{
    public class Player : Component, IUpdatable
    {
        private GameMap _map;
        private TiledMapComponent _tiledMapComponent;

        VirtualIntegerAxis _xAxisInput;
        VirtualIntegerAxis _yAxisInput;
        public int FOVRadius;
        public bool LightWalls;

        public float moveSpeed = 150;

        private Mover _mover;
        private BoxCollider _collider;
        private CollisionState _collisionState = new CollisionState();

        public Player(GameMap map)
        {
            FOVRadius = 6;
            LightWalls = true;
            _map = map;
        }

        public override void onAddedToEntity()
        {
            _mover = this.getComponent<Mover>();
            _collider = this.getComponent<BoxCollider>();
            _tiledMapComponent = this.entity.scene.findEntity("tiled-map").getComponent<TiledMapComponent>();

            setupInput();
        }

        public override void onRemovedFromEntity()
        {
            // deregister virtual input
            _xAxisInput.deregister();
            _yAxisInput.deregister();
        }

        public void update()
        {
            // handle movement and animations
            var moveDir = new Vector2(_xAxisInput.value, _yAxisInput.value);

            if (moveDir != Vector2.Zero)
            {
                var movement = moveDir * moveSpeed * Time.deltaTime;

                UpdateFOV();
                _mover.move(movement, out var res);

                if (res.collider != null)
                {
                    var pos = entity.position + new Vector2(-32) * res.normal;
                    var tile = _tiledMapComponent.tiledMap.getLayer<TiledTileLayer>("basic").getTileAtWorldPosition(pos);
                    if (tile != null)
                    if ((TileType)tile.id == TileType.Door)
                    {
                        //open door
                        tile.setTileId(0);
                        //_tiledMapComponent.collisionLayer.re
                        _tiledMapComponent.collisionLayer.removeTile(tile.x, tile.y);
                        _tiledMapComponent.removeColliders();
                        _tiledMapComponent.addColliders();
                        _map.SetTile(tile.x, tile.y, TileType.Empty);

                    }
                }
            }
            //this.entity.scene.camera.transform.position = this.transform.position;
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
