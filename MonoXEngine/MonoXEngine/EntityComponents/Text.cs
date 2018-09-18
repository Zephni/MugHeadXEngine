using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoXEngine.EntityComponents
{
    ///
    /// To add a new font, open Content.mgcb and add new item "Font Description" in the "Fonts" directory, then build
    ///

    /// <summary>
    /// Text component, set the SpriteFont to a .xnb loaded content. And set the text.
    /// </summary>
    class Text : Drawable
    {
        public string _text = "";
        public string String
        {
            get { return _text; }
            set { _text = value; this.Entity.TextureSize = this.SpriteFont.MeasureString(_text); }
        }

        public float Opacity = 1;
        private SpriteFont SpriteFont = null;

        public Text SetSpriteFont(string FontAlias = "HeartbitXX")
        {
            this.SpriteFont = Global.Content.Load<SpriteFont>(Global.MainSettings.Get<string>(new string[] {"Directories", "Fonts"})+FontAlias);
            return this;
        }

        public Text()
        {
            Color = Color.GhostWhite;
            this.SetSpriteFont();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            this.Entity.Origin = Vector2.Zero;

            if (this.SpriteFont != null && Visible)
            {
                spriteBatch.DrawString(
                    this.SpriteFont,
                    this.String,
                    this.Entity.Position,
                    this.Color,
                    this.Entity.Rotation,
                    this.Entity.Origin * this.Entity.Size,
                    this.Entity.Scale,
                    SpriteEffects.None,
                    0
                );
            }
        }
    }
}
