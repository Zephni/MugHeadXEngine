using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
    public class RenderBlender
    {
        public static BlendState Multiply = new BlendState
        {
            AlphaBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
        };

        public static BlendState Subtract = new BlendState
        {
            ColorSourceBlend = Blend.Zero,
            ColorDestinationBlend = Blend.InverseSourceColor,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.InverseSourceColor
        };

        public class DrawableTexture
        {
            public Texture2D Texture;
            public Vector2 Position = Vector2.Zero;
            public float Scale = 1;
            public Vector2 Size { get { return Texture.Size() * Scale; } }
            public Color Color = Color.White;
            public BlendState Blend = Multiply;
            public Action<DrawableTexture> Update = null;
        }

        private RenderTarget2D RenderTarget2D;
        private SpriteBatch SpriteBatch;

        public Entity Entity;
        public Color ClearColor;
        public List<DrawableTexture> DrawableTextures;

        public Vector2 Position
        {
            get { return Entity.Position; }
            set { Entity.Position = value; }
        }

        public RenderBlender(Color clearColor, List<DrawableTexture> drawableTextures)
        {
            RenderTarget2D = new RenderTarget2D(Global.GraphicsDevice, Global.Resolution.X+4, Global.Resolution.Y+4);
            SpriteBatch = new SpriteBatch(Global.GraphicsDevice);

            ClearColor = clearColor;
            DrawableTextures = drawableTextures;

            Entity = new Entity(entity => {
                entity.LayerName = "Main";
                entity.SortingLayer = 10;
                entity.AddComponent(new Sprite()).Run<Sprite>(sprite => {
                    StaticCoroutines.CoroutineHelper.Always(() => {
                        Position = Global.Camera.Position;
                        UpdateRenderTarget();
                        sprite.Texture2D = RenderTarget2D;
                    });
                });
            });
        }

        private void UpdateRenderTarget()
        {
            Global.GraphicsDevice.SetRenderTarget(RenderTarget2D);
            Global.GraphicsDevice.Clear(ClearColor);

            Global.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

            // Draw textures
            foreach(DrawableTexture item in DrawableTextures)
            {
                item.Update?.Invoke(item);

                SpriteBatch.Begin(blendState: item.Blend);
                SpriteBatch.Draw(
                    item.Texture,
                    (item.Position - Position) + (item.Texture.Size() / 2 * (1 - item.Scale)),
                    null,
                    item.Color,
                    0,
                    Vector2.Zero,
                    item.Scale,
                    SpriteEffects.None,
                    0
                );
                SpriteBatch.End();
            }

            Global.GraphicsDevice.SetRenderTarget(null);
        }
    }
}
