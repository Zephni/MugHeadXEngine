using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StaticCoroutines;
using System;

namespace MonoXEngine.EntityComponents
{
    public class Drawable : EntityComponent
    {
        public bool Visible = true;

        public Color Color = Color.White;

        public SpriteEffects SpriteEffect = SpriteEffects.None;

        private Texture2D texture2D;
        public Texture2D Texture2D
        {
            get { return texture2D; }
            set {
                texture2D = value;

                CoroutineHelper.RunWhen(() => this.Entity != null, () => {
                    this.Entity.TextureSize = new Vector2(this.Texture2D.Width, this.Texture2D.Height);
                });

                this.SourceRectangle = this.Texture2D.Bounds;
            }
        }

        private Rectangle sourceRectangle;
        public Rectangle SourceRectangle
        {
            get { return this.sourceRectangle; }
            set
            {
                this.sourceRectangle = value;

                CoroutineHelper.RunWhen(() => this.Entity != null, () => {
                    this.Entity.TextureSize = this.sourceRectangle.Size.ToVector2();
                });
            }
        }

        public override void Start()
        {

        }

        public void MoveSourceRectangle(Rectangle moveRect)
        {
            this.SourceRectangle = new Rectangle(
                this.sourceRectangle.X + moveRect.X,
                this.sourceRectangle.Y + moveRect.Y,
                this.sourceRectangle.Width + moveRect.Width,
                this.sourceRectangle.Height + moveRect.Height
            );
        }

        public void LoadTexture(string file)
        {
            this.Texture2D = Global.Content.Load<Texture2D>(file);
        }

        public void BuildRectangle(Point size, Color color)
        {
            this.Texture2D = new Texture2D(MonoXEngineGame.Instance.GraphicsDevice, size.X, size.Y, false, SurfaceFormat.Color);

            Color[] colors = new Color[size.X * size.Y];
            for (int I = 0; I < size.X * size.Y; I++)
                colors[I] = color;

            this.Texture2D.SetData<Color>(colors);

            this.Entity.TextureSize = new Vector2(this.Texture2D.Width, this.Texture2D.Height);
            this.SourceRectangle = this.Texture2D.Bounds;
        }

        public override void Update()
        {
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(this.Texture2D != null && Visible)
            {
                spriteBatch.Draw(
                    this.Texture2D,
                    new Rectangle(new Point((int)Math.Floor(Entity.Position.X), (int)Math.Floor(Entity.Position.Y)), this.Entity.Size.ToPoint()),
                    this.SourceRectangle,
                    Color * this.Entity.Opacity,
                    this.Entity.Rotation,
                    (this.Entity.Origin * this.Entity.Size).ToPoint().ToVector2(), // Hmm
                    SpriteEffect,
                    0
                );
            }
        }
    }
}
