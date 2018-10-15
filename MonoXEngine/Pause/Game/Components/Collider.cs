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
    public struct Offset
    {
        public int? X;        // Null == Width
        public int? Y;        // Null == Height
        public int? Width;    // Null == Width
        public int? Height;   // Null == Height

        public Offset(int? x, int? y, int? width, int? height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Offset(int? x, int? y)
        {
            X = x;
            Y = y;
            Width = null;
            Height = null;
        }
    }

    public class Collider : EntityComponent
    {
        // Properties
        public enum TriggerTypes
        {
            None,
            NonSolid,
            Solid
        }
        public TriggerTypes TriggerType = TriggerTypes.None;

        public enum ColliderTypes
        {
            Box,
            Pixel
        }
        public ColliderTypes ColliderType = ColliderTypes.Box;

        public Rectangle CheckArea
        {
            get
            {
                return new Rectangle(
                    Entity.Position.ToPoint() - (Entity.Size * Entity.Origin).ToPoint(),
                    Entity.Size.ToPoint()
                );
            }
        }

        public Offset Offset = new Offset();

        public List<Entity> OverlappingEntities = new List<Entity>();

        public List<Entity> PrevCollidingTriggers;
        public List<Entity> CollidingTriggers;

        public Action<Entity> CollidedWithTrigger = null;
        public Action<Entity> CollidingWithTrigger = null;
        public Action<Entity> UnCollidedWithTrigger = null;

        // Construct
        public Collider()
        {
            PrevCollidingTriggers = new List<Entity>();
            CollidingTriggers = new List<Entity>();
        }

        // Colliding method
        public bool OverlappingEntity(Predicate<Entity> predicate)
        {
            return (OverlappingEntities.Find(predicate) != null);
        }

        private Rectangle AreaPlusOffset(Rectangle rect, Offset offset)
        {
            Rectangle final = rect;

            final.X += offset.X ?? rect.Width;
            final.Y += offset.Y ?? rect.Height;
            final.Width = offset.Width ?? rect.Width;
            final.Height = offset.Height ?? rect.Height;

            return final;
        }
        
        // CheckOffset method
        public bool CheckOffset(Offset offset, Predicate<Entity> predicate = null)
        {
            Rectangle thisCheckRect = AreaPlusOffset(CheckArea, offset);

            foreach (Entity other in (predicate == null) ? OverlappingEntities : OverlappingEntities.FindAll(predicate))
            {
                Collider otherCollider = other.GetComponent<Collider>();

                if(otherCollider.ColliderType == ColliderTypes.Box)
                {
                    Rectangle otherCheckRect = otherCollider.CheckArea;

                    if (thisCheckRect.Intersects(otherCheckRect))
                    {
                        if (otherCollider.TriggerType != Collider.TriggerTypes.None && !CollidingTriggers.Contains(other))
                            CollidingTriggers.Add(other);

                        if (otherCollider.TriggerType != Collider.TriggerTypes.NonSolid)
                            return true;
                    }
                }
                else if(otherCollider.ColliderType == ColliderTypes.Pixel)
                {
                    Rectangle entityBox = otherCollider.CheckArea;
                    Rectangle.Intersect(ref thisCheckRect, ref entityBox, out Rectangle intersectRect);

                    if(intersectRect.Width > 0 && intersectRect.Height > 0)
                    {
                        intersectRect.Location -= entityBox.Location;

                        int totalPixels = intersectRect.Width * intersectRect.Height;
                        Color[] colors = new Color[totalPixels];
                        other.GetComponent<Drawable>().Texture2D.GetData(0, intersectRect, colors, 0, totalPixels);

                        for (int I = 0; I < colors.Length; I++)
                        {
                            if (colors[I].A != byte.MinValue)
                            {
                                if (otherCollider.TriggerType != Collider.TriggerTypes.None && !CollidingTriggers.Contains(other))
                                    CollidingTriggers.Add(other);

                                if (otherCollider.TriggerType != Collider.TriggerTypes.NonSolid)
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        // Update method
        public override void Update()
        {
            // Overlapping entities
            OverlappingEntities.Clear();

            foreach (Entity checkEntity in Global.Entities.FindAll(p => p.HasComponent<Collider>()))
            {
                if (checkEntity == Entity)
                    continue;

                Collider checkCollider = checkEntity.GetComponent<Collider>();

                if(CheckArea.Intersects(checkCollider.CheckArea))
                    OverlappingEntities.Add(checkEntity);
            }

            // Triggers
            foreach (var item in CollidingTriggers)
            {
                // New colliding triggers
                if (!PrevCollidingTriggers.Contains(item))
                    CollidedWithTrigger?.Invoke(item);

                // Current colliding triggers
                CollidingWithTrigger?.Invoke(item);
            }

            // New uncolliding triggers
            foreach (var item in PrevCollidingTriggers)
                if (!CollidingTriggers.Contains(item))
                    UnCollidedWithTrigger?.Invoke(item);

            PrevCollidingTriggers = new List<Entity>();
            foreach (var item in CollidingTriggers)
                PrevCollidingTriggers.Add(item);

            CollidingTriggers = new List<Entity>();
        }

        #region Unused
        public override void Draw(SpriteBatch spriteBatch)
        {
            //throw new NotImplementedException();
        }

        public override void Start()
        {
            //throw new NotImplementedException();
        }
        #endregion
    }
}
