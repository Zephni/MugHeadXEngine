using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MonoXEngine.EntityComponents;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoXEngine
{
    [Serializable]
    public class Entity
    {
        private List<EntityComponent> EntityComponents;

        public string Name;
        public Vector2 Position;
        public Vector2 Origin;
        public Vector2 Scale;
        public Vector2 TextureSize;
        public float Rotation;
        public Dictionary<string, string> Data = new Dictionary<string, string>();
        public bool CheckPixels = true;

        private Dictionary<string, Action<Entity, Action<Entity>>> Functions = new Dictionary<string, Action<Entity, Action<Entity>>>();

        public bool Trigger = false;
        public Action<Entity> CollidedWithTrigger;
        
        public int SortingLayer;

        public Rectangle BoundingBox
        {
            get { return new Rectangle(this.Position.ToPoint() - (this.Origin * this.Size).ToPoint(), this.Size.ToPoint()); }
        }

        private float opacity = 1;
        public float Opacity
        {
            get { return opacity; }
            set { if (value < 0) value = 0; if (value > 1) value = 1; this.opacity = value; }
        }

        public Vector2 Size { get { return this.TextureSize * this.Scale; } }

        public string LayerName;

        public Entity(Action<Entity> action = null)
        {
            this.Position = Vector2.Zero;
            this.Origin = new Vector2(0.5f, 0.5f);
            this.Scale = new Vector2(1, 1);
            this.EntityComponents = new List<EntityComponent>();

            if (this.LayerName == null)
                this.LayerName = Global.MainSettings.Get<string>(new string[] { "Defaults", "Layer" });

            Global.Entities.Add(this);

            if (action != null)
                action.Invoke(this);

            this.Start();
        }

        private Action<Entity> prefabAction = null;
        public Entity(bool prefab, Action<Entity> action = null)
        {
            this.Position = Vector2.Zero;
            this.Origin = new Vector2(0.5f, 0.5f);
            this.Scale = new Vector2(1, 1);
            this.EntityComponents = new List<EntityComponent>();

            if (!prefab)
            {
                this.LayerName = Global.MainSettings.Get<string>(new string[] { "Defaults", "Layer" });
                Global.Entities.Add(this);
                if(action != null)
                    action.Invoke(this);
                this.Start();
            }
            else
            {
                prefabAction = action;
            }
        }

        public Entity BuildPrefab(string layerName = null)
        {
            Entity newEntity = Cloner.Copy(this);
            
            if (layerName == null)
                newEntity.LayerName = Global.MainSettings.Get<string>(new string[] { "Defaults", "Layer" });
            else
                newEntity.LayerName = layerName;

            Global.Entities.Add(newEntity);

            if (this.prefabAction != null)
                this.prefabAction(newEntity);

            newEntity.Start();
            return newEntity;
        }

        public virtual void Start() { }

        public EntityComponent AddComponent(EntityComponent entityComponent)
        {
            this.EntityComponents.Add(entityComponent);
            entityComponent.Initialise(this);
            return entityComponent;
        }

        public T GetComponent<T>()
        {
            foreach (EntityComponent component in this.EntityComponents)
                if (component.GetType() == typeof(T))
                    return (T)Convert.ChangeType(component, typeof(T));

            return default(T);
        }

        public bool HasComponent<T>()
        {
            foreach (EntityComponent component in this.EntityComponents)
                if (component.GetType() == typeof(T))
                    return true;

            return false;
        }

        public void Destroy()
        {
            Global.Entities.Remove(this);
        }

        public Action<Entity> UpdateAction;
        public virtual void Update()
        {
            if(this.UpdateAction != null)
                this.UpdateAction(this);

            foreach (EntityComponent component in this.EntityComponents)
            {
                component.Update();
            }
        }

        public void AddFunction(string alias, Action<Entity, Action<Entity>> action)
        {
            this.Functions.Add(alias, action);
        }

        public void AddFunction(string alias, Action<Entity> action)
        {
            Action<Entity, Action<Entity>> actionBlackCallback = (e, c) => {
                action.Invoke(e);
            };

            this.Functions.Add(alias, actionBlackCallback);
        }

        public void RunFunction(string alias, Action<Entity> callback = null)
        {
            this.Functions[alias](this, callback);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            foreach(Drawable drawable in this.EntityComponents.FindAll(
                e => e.GetType().IsSubclassOf(typeof(Drawable)) || e.GetType() == typeof(Drawable)))
            {
                drawable.Draw(spriteBatch);
            }
        }
    }
}