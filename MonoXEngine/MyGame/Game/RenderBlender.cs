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
        public static BlendState Lighting = new BlendState
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.InverseSourceColor
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
            public DrawableTexture(string effect = null)
            {
                if (effect != null)
                    Texture = Global.Content.Load<Texture2D>("Graphics/Effects/"+effect);
            }

            public Texture2D Texture;
            public Vector2 Position = Vector2.Zero;
            public int Layer = 0;
            private Vector2 scale = Vector2.One;
            public float Scale { get => scale.X; set => scale = new Vector2(value, value); }
            public Vector2 Size { get { return Texture.Size() * Scale; } }
            public Color Color = Color.White;
            public BlendState Blend = Lighting;
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
            RenderTarget2D = new RenderTarget2D(Global.GraphicsDevice, Global.Resolution.X+8, Global.Resolution.Y+8);
            SpriteBatch = new SpriteBatch(Global.GraphicsDevice);

            ClearColor = clearColor;
            DrawableTextures = drawableTextures;

            Entity = new Entity(entity => {
                entity.LayerName = "Main";
                entity.SortingLayer = 8;
                entity.Origin = Vector2.Zero;
                entity.AddComponent(new Sprite()).Run<Sprite>(sprite => {
                    StaticCoroutines.CoroutineHelper.Always(() => {
                        Position = Global.Camera.Position - (RenderTarget2D.Size() / 2) - new Vector2(2, 2);
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

            // Draw textures
            foreach (DrawableTexture item in DrawableTextures)
            {
                item.Update?.Invoke(item);

                SpriteBatch.Begin(blendState: item.Blend);

                SpriteBatch.Draw(
                    item.Texture,
                    
                    new Rectangle((item.Position.ToPoint()) - Position.ToPoint() + new Point(8, 8), item.Size.ToPoint()),
                    null,
                    item.Color,
                    0,
                    item.Texture.Size() / 2,
                    SpriteEffects.None,
                    item.Layer
                );

                SpriteBatch.End();
            }

            Global.GraphicsDevice.SetRenderTarget(null);
        }
    }
}
