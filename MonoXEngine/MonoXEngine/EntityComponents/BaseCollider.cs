using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoXEngine.EntityComponents
{
    public abstract class BaseCollider : EntityComponent
    {
        public abstract bool Colliding(Point offset, int layerOffset = 0);

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
