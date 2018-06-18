using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.Tiled;
using ProjectChandra.Shared.Components;
using ProjectChandra.Shared.Helpers;
using ProjectChandra.Shared.MapGen;
using ProjectChandra.Shared.MapGen.Generators;
using ProjectChandra.Shared.Tiles;

namespace ProjectChandra.Shared.Scenes
{
    public class TestScene : Scene
    {
        private Texture2D _texture;
        private Entity _player;
        private MapGen.GameMap _map;

        public TestScene()
        {
        }

        public override void initialize()
        {
            base.initialize();
            setDesignResolution(1280, 720, SceneResolutionPolicy.None);
            Screen.setSize(1280, 720);
            clearColor = Color.Black;
            addRenderer(new DefaultRenderer());
            CreateTexture();

           

            //SetupBspMap();
            SetupTemplatedMap();

            _player = createEntity("hero");
            _player.position = new Vector2(320 + 16);
            _player.addComponent(new Sprite(new Nez.Textures.Subtexture(_texture, new Rectangle(32, 0, 32, 32), new Vector2(16))) { color = Color.LimeGreen, renderLayer = -1 });
            _player.addComponent(new Player2(_map));
            _player.addComponent(new CustomTiledMapMover());

            var pCol = _player.addComponent(new BoxCollider(32, 32));
            //Flags.setFlagExclusive(ref pCol.physicsLayer, (int)LayerMask.Player);
            //Flags.setFlagExclusive(ref pCol.collidesWithLayers, (int)LayerMask.Obstacles);
            camera.entity.addComponent(new FollowCamera(_player));
            //camera.zoom = -1f;
            //camera.position = new Vector2(640, 1100);
        }

        private void SetupBspMap()
        {
            var w = 70;
            var h = 70;
            var ts = 32;

            var gen = new MessyBSPTreeMapGenerator(w, h);
            _map = gen.CreateMap();
            Debug.log(_map.ToString());

            var custMap = new CustomTiledMap(0, _map.Width, _map.Height, ts, ts);
            var tileset = new TiledTileset(_texture, 0, ts, ts, 0, 0, 4, 4);
            custMap.loadFromArray("basic", _map.GetMap().Select(x => (int) x).ToArray(), _map.Width, _map.Height, tileset, ts, ts, _map);
            var mapEntity = createEntity("tiled-map");
            mapEntity.addComponent(new TiledMapComponent(custMap, shouldCreateColliders: false));

        }

        private List<RoomTemplate> LoadRoomTemplatesFromFile(string path)
        {
            var lines = File.ReadAllLines(path);

            List<RoomTemplate> templates = new List<RoomTemplate>();
            var parsingTemplate = false;

           
            int w = 0;
            int h = 0;
            string tData = "";

            foreach(var line in lines)
            {
                if (parsingTemplate && string.IsNullOrWhiteSpace(line))
                {
                    parsingTemplate = false;
                    templates.Add(new RoomTemplate(w, h, tData));
                    tData = string.Empty;
                    continue;
                }
                if (!parsingTemplate && line.Contains(','))
                {
                    var coordsStr = line.Split(',');
                    w = int.Parse(coordsStr[0]);
                    h = int.Parse(coordsStr[1]);
                    parsingTemplate = true;
                    continue;
                }
                if (parsingTemplate)
                {
                    tData += line;
                }
            }
            if (!string.IsNullOrWhiteSpace(tData))
                templates.Add(new RoomTemplate(w, h, tData));
            return templates;
        }

        private void SetupTemplatedMap()
        {
            var w = 70;
            var h = 70;
            var ts = 32;
            var templates = LoadRoomTemplatesFromFile(Path.Combine(content.RootDirectory, "Templates.txt"));

            var gen = new TemplatedMapGenerator() { DesiredRoomCount = 35 };
            gen.AddTemplates(templates.ToArray());

            _map = gen.CreateMap(w, h);

            var custMap = new CustomTiledMap(0, _map.Width, _map.Height, ts, ts);
            var tileset = new TiledTileset(_texture, -1, ts, ts, 0, 0, 4, 4);
            custMap.loadFromArray("basic", _map.GetMap().Select(x => (int)x).ToArray(), _map.Width, _map.Height, tileset, ts, ts, _map);
            var colArr = new TiledTile[_map.Width * _map.Height];
            var mapTiles = _map.GetMap();
            for (int i = 0; i < mapTiles.Length; i++)
            {
                if (mapTiles[i] == TileType.Empty)
                    continue;
                colArr[i] = new TiledTile((int)mapTiles[i]) { tileset = tileset };
            }
            custMap.createTileLayer("col", _map.Width, _map.Height, colArr).visible = false;


            var mapEntity = createEntity("tiled-map");
            mapEntity.addComponent(new TiledMapComponent(custMap, "col"));
        }

        private void CreateTexture()
        {
            _texture = new Texture2D(Core.graphicsDevice, 128, 32);
            var data = new Color[128 * 32];

            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (i < 32)
                        data[j * 128 + i] = Color.Black;
                    else if (i >= 32 && i < 64)
                        data[j * 128 + i] = Color.LightGray;
                    else if (i >= 64 && i < 96)
                        data[j * 128 + i] = Color.Gray;
                    else
                        data[j * 128 + i] = Color.Brown;
                }
            }

            _texture.SetData(data);
        }

        public override void update()
        {
            base.update();
            if (Input.isKeyDown(Keys.R))
            {
                //var tiledMapEntity = entities.findEntity("tiled-map");
                //tiledMapEntity.destroy();   
                //SetupTemplatedMap();
                //SetupBspMap();
            }
        }
    }
}
