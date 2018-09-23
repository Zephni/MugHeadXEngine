using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MonoXEngine.EntityComponents
{
    public abstract class BaseCollider : EntityComponent
    {
        public abstract bool Colliding(Rectangle offset, Entity.CollisionType CollisionType = Entity.CollisionType.Pixel);
        public virtual bool Colliding(Point offset, Entity.CollisionType CollisionType = Entity.CollisionType.Pixel)
        {
            return Colliding(new Rectangle(offset, Point.Zero));
        }

        public override void Start()
        {

        }

        public override void Update()
        {
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            
        }
    }
}
