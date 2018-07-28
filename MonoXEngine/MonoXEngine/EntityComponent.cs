using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;

namespace MonoXEngine
{
    public abstract class EntityComponent
    {
        public Entity Entity;

        public void Initialise(Entity entity)
        {
            this.Entity = entity;
            this.Start();
        }

        public abstract void Start();

        public abstract void Update();

        public abstract void Draw(SpriteBatch spriteBatch);

        public virtual void Run<T>(Action<T> action)
        {
            T component = (T)Convert.ChangeType(this, typeof(T));
            action(component);
        }
    }
}
