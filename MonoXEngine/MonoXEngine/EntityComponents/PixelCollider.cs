using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoXEngine;

namespace MonoXEngine.EntityComponents
{
    public class PixelCollider : BaseCollider 
    {
        public static string TEST = "";
        public List<Entity> PrevCollidingTriggers;
        public List<Entity> CollidingTriggers;

        public PixelCollider()
        {
            PrevCollidingTriggers = new List<Entity>();
            CollidingTriggers = new List<Entity>();
        }

        public override void Update()
        {
            foreach(var item in CollidingTriggers)
            {
                // New colliding triggers
                if (!PrevCollidingTriggers.Contains(item))
                    Entity.CollidedWithTrigger?.Invoke(item);

                // Current colliding triggers
                Entity.CollidingWithTrigger?.Invoke(item);
            }
                    
            // New uncolliding triggers
            foreach (var item in PrevCollidingTriggers)
                if (!CollidingTriggers.Contains(item))
                {
                    Entity.UnCollidedWithTrigger?.Invoke(item);
                }

            PrevCollidingTriggers = new List<Entity>();
            foreach (var item in CollidingTriggers)
                PrevCollidingTriggers.Add(item);

            CollidingTriggers = new List<Entity>();
        }

        public override bool Colliding(Point offset, Entity.CollisionType CollisionType = Entity.CollisionType.Pixel)
        {
            Rectangle checkArea = new Rectangle(
                (this.Entity.Position.ToPoint() - (this.Entity.Size/2).ToPoint()) + offset,
                this.Entity.Size.ToPoint()
            );

            List<Entity> possibleCollidingEntities = new List<Entity>();
            foreach(Entity entity in Global.Entities.FindAll(e => e.LayerName == this.Entity.LayerName))
            {
                if (entity == Entity || entity.Collider != CollisionType || !entity.CheckPixels)
                    continue;

                if (checkArea.Intersects(entity.BoundingBox))
                {
                    possibleCollidingEntities.Add(entity);
                }
            }

            if (IsOverlappingPixel(checkArea, possibleCollidingEntities))
                return true;

            return false;
        }

        private bool IsOverlappingPixel(Rectangle checkRect, List<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                if (entity.GetComponent<Drawable>() == null)
                    return false;

                Rectangle entityBox = entity.BoundingBox;

                Rectangle.Intersect(ref checkRect, ref entityBox, out Rectangle intersectRect);

                if (intersectRect.Width > 0 && intersectRect.Height > 0)
                {
                    intersectRect.Location -= entityBox.Location;

                    int totalPixels = intersectRect.Width * intersectRect.Height;
                    Color[] colors = new Color[totalPixels];
                    entity.GetComponent<Drawable>().Texture2D.GetData(0, intersectRect, colors, 0, totalPixels);

                    for (int I = 0; I < colors.Length; I++)
                        if (colors[I].A != byte.MinValue)
                        {
                            if(entity.Trigger)
                            {
                                if(!CollidingTriggers.Contains(entity))
                                    CollidingTriggers.Add(entity);
                            }
                            else
                            {
                                return true;
                            }
                        }
                }
            }

            return false;
        }
    }
}