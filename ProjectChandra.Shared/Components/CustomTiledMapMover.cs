using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Nez;

namespace ProjectChandra.Shared.Components
{
    public class CustomTiledMapMover : Component, IUpdatable
    {
        private Vector2 startPos, targetPos;
        private TiledMapComponent _tiledMapComp;
        float timer;
        float speed = 150;
        public bool IsMoving;

        public override void onAddedToEntity()
        {
            _tiledMapComp = entity.scene.findEntity("tiled-map").getComponent<TiledMapComponent>();
        }

        public IEnumerator Move(Vector2 moveDir)
        {
            startPos = entity.position;
            targetPos = new Vector2(startPos.X + Math.Sign(moveDir.X) * _tiledMapComp.tiledMap.tileWidth,
                startPos.Y + Math.Sign(moveDir.Y) * _tiledMapComp.tiledMap.tileWidth);
            timer = 0;
            IsMoving = true;

            var factor = 1f;

            while (timer < 1f)
            {
                timer += Time.deltaTime * (speed / _tiledMapComp.tiledMap.tileWidth) * factor;
                entity.position = Vector2.Lerp(startPos, targetPos, timer);
                yield return null;
            }

            IsMoving = false;
            entity.position = targetPos;
            yield return Coroutine.waitForSeconds(0f);
        }

        public void update()
        {
            //if (timer <= 0)
            //{
            //    timer += Time.deltaTime * speed;
            //    timer = MathHelper.Min(timer, 1);
            //    entity.position = Vector2.Lerp(entity.position, targetPos, timer);
            //}
        }
    }
}
