using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace ProjectChandra.Shared.Scenes
{
    public class TestScene : Scene
    {
        private Texture2D _texture;

        public TestScene()
        {
        }

        public override void initialize()
        {
            base.initialize();
            setDesignResolution(1280, 720, SceneResolutionPolicy.None);
            Screen.setSize(1280, 720);
            clearColor = Color.CornflowerBlue;
            addRenderer(new DefaultRenderer());

            CreateTexture();


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
    }
}
