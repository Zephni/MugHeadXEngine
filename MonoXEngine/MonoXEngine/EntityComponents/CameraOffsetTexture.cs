using Microsoft.Xna.Framework;

namespace MonoXEngine.EntityComponents
{
    /// <summary>
    /// Should only be used on linear wrap sample state sprite batch
    /// </summary>
    public class CameraOffsetTexture : Sprite
    {
        public Vector2 Coefficient = new Vector2(1, 1);
        public Vector2 Offset = new Vector2(0, 0);

        public override void Update()
        {
            this.SourceRectangle = new Rectangle(Offset.ToPoint() + (Global.Camera.Position * this.Coefficient).ToPoint(), this.Entity.TextureSize.ToPoint());
        }
    }
}