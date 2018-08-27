using Microsoft.Xna.Framework;
using MyGame;

namespace MonoXEngine.EntityComponents
{
    /// <summary>
    /// Should only be used on linear wrap sample state sprite batch
    /// </summary>
    public class CameraOffsetTexture : Sprite
    {
        public Vector2 Coefficient = new Vector2(1, 1);
        public Vector2 Offset = new Vector2(0, 0);

        public Point Size = new Point();

        public bool Animate = false;
        public int AnimFrame = 0;
        public float AnimTimer = 0;
        public float AnimStepTime = 0.2f;

        public override void Update()
        {
            if (Size.X == 0 && Size.Y == 0)
                Size = this.Entity.TextureSize.ToPoint();

            if(Animate)
            {
                AnimTimer += Global.DeltaTime;
                if (AnimTimer > AnimStepTime)
                {
                    AnimTimer = 0;
                    AnimFrame++;
                    if (AnimFrame > 5)
                        AnimFrame = 0;
                }
            }
            
            this.SourceRectangle = new Rectangle(
                Offset.ToPoint() + (Global.Camera.Position * this.Coefficient).ToPoint() + new Point(AnimFrame * Size.X, 0), Size);
        }
    }
}