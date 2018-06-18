using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Tiled;
using ProjectChandra.Shared.MapGen;

namespace ProjectChandra.Shared.Tiles
{
    public class CustomTiledLayer : TiledTileLayer
    {
        private GameMap _map;

        public CustomTiledLayer(GameMap gameMap, TiledMap map, string name, int width, int height, TiledTile[] tiles) 
            : base(map, name, width, height, tiles)
        {
            _map = gameMap;
        }

        public override void draw(Batcher batcher, Vector2 position, float layerDepth, RectangleF cameraClipBounds)
        {
            // offset it by the entity position since the tilemap will always expect positions in its own coordinate space
            cameraClipBounds.location -= (position + offset);

            int minX, minY, maxX, maxY;
            if (tiledMap.requiresLargeTileCulling)
            {
                // we expand our cameraClipBounds by the excess tile width/height of the largest tiles to ensure we include tiles whose
                // origin might be outside of the cameraClipBounds
                minX = tiledMap.worldToTilePositionX(cameraClipBounds.left - (tiledMap.largestTileWidth - tiledMap.tileWidth));
                minY = tiledMap.worldToTilePositionY(cameraClipBounds.top - (tiledMap.largestTileHeight - tiledMap.tileHeight));
                maxX = tiledMap.worldToTilePositionX(cameraClipBounds.right + (tiledMap.largestTileWidth - tiledMap.tileWidth));
                maxY = tiledMap.worldToTilePositionY(cameraClipBounds.bottom + (tiledMap.largestTileHeight - tiledMap.tileHeight));
            }
            else
            {
                minX = tiledMap.worldToTilePositionX(cameraClipBounds.left);
                minY = tiledMap.worldToTilePositionY(cameraClipBounds.top);
                maxX = tiledMap.worldToTilePositionX(cameraClipBounds.right);
                maxY = tiledMap.worldToTilePositionY(cameraClipBounds.bottom);
            }

            // loop through and draw all the non-culled tiles
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var tile = getTile(x, y);
                    if (tile == null)
                        continue;
                    
                    var cell = _map.GetCell(x, y);
                    if (!cell.IsExplored)
                        continue;
                    color = !cell.IsInFov ? Color.DimGray : Color.White;
                    
                    var tileRegion = tile.textureRegion;

                    // culling for arbitrary size tiles if necessary
                    if (tiledMap.requiresLargeTileCulling)
                    {
                        // TODO: this only checks left and bottom. we should check top and right as well to deal with rotated, odd-sized tiles
                        var tileworldpos = tiledMap.tileToWorldPosition(new Point(x, y));
                        if (tileworldpos.X + tileRegion.sourceRect.Width < cameraClipBounds.left || tileworldpos.Y - tileRegion.sourceRect.Height > cameraClipBounds.bottom)
                            continue;
                    }

                    // for the y position, we need to take into account if the tile is larger than the tileHeight and shift. Tiled uses
                    // a bottom-left coordinate system and MonoGame a top-left
                    var tx = tile.x * tiledMap.tileWidth + (int)position.X;
                    var ty = tile.y * tiledMap.tileHeight + (int)position.Y;
                    var rotation = 0f;

                    var spriteEffects = SpriteEffects.None;
                    if (tile.flippedHorizonally)
                        spriteEffects |= SpriteEffects.FlipHorizontally;
                    if (tile.flippedVertically)
                        spriteEffects |= SpriteEffects.FlipVertically;
                    if (tile.flippedDiagonally)
                    {
                        if (tile.flippedHorizonally && tile.flippedVertically)
                        {
                            spriteEffects ^= SpriteEffects.FlipVertically;
                            rotation = MathHelper.PiOver2;
                            tx += tiledMap.tileHeight + (tileRegion.sourceRect.Height - tiledMap.tileHeight);
                            ty -= (tileRegion.sourceRect.Width - tiledMap.tileWidth);
                        }
                        else if (tile.flippedHorizonally)
                        {
                            spriteEffects ^= SpriteEffects.FlipVertically;
                            rotation = -MathHelper.PiOver2;
                            ty += tiledMap.tileHeight;
                        }
                        else if (tile.flippedVertically)
                        {
                            spriteEffects ^= SpriteEffects.FlipHorizontally;
                            rotation = MathHelper.PiOver2;
                            tx += tiledMap.tileWidth + (tileRegion.sourceRect.Height - tiledMap.tileHeight);
                            ty += (tiledMap.tileWidth - tileRegion.sourceRect.Width);
                        }
                        else
                        {
                            spriteEffects ^= SpriteEffects.FlipHorizontally;
                            rotation = -MathHelper.PiOver2;
                            ty += tiledMap.tileHeight;
                        }
                    }

                    // if we had no rotations (diagonal flipping) shift our y-coord to account for any non-tileSized tiles to account for
                    // Tiled being bottom-left origin
                    if (rotation == 0)
                        ty += (tiledMap.tileHeight - tileRegion.sourceRect.Height);

                    batcher.draw(tileRegion, new Vector2(tx, ty) + offset, color, rotation, Vector2.Zero, 1, spriteEffects, layerDepth);

                }
            }
        }
    }
}
