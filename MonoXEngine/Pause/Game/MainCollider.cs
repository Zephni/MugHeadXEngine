using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoXEngine;
using MonoXEngine.EntityComponents;

namespace MyGame
{
    public class MainCollider : BaseCollider 
    {
        // Add base size change  option
        public int AddWidth = 0;
        public int AddHeight = 0;

        public List<Entity> PrevCollidingTriggers;
        public List<Entity> CollidingTriggers;

        public MainCollider()
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
                    Entity.UnCollidedWithTrigger?.Invoke(item);

            PrevCollidingTriggers = new List<Entity>();
            foreach (var item in CollidingTriggers)
                PrevCollidingTriggers.Add(item);

            CollidingTriggers = new List<Entity>();
        }

        public List<Entity> CollidingWith(Rectangle rect, Predicate<Entity> predicate)
        {
            List<Entity> collidingEntities = new List<Entity>();

            Rectangle checkArea = new Rectangle(
                (this.Entity.Position.ToPoint() + rect.Location),
                rect.Size
            );

            List<Entity> possibleCollidingEntities = new List<Entity>();
            foreach (Entity entity in Global.Entities.FindAll(e => e.LayerName == this.Entity.LayerName).FindAll(predicate))
            {
                if (entity == Entity || !entity.CheckPixels)
                    continue;

                if (checkArea.Intersects(entity.BoundingBox))
                    possibleCollidingEntities.Add(entity);
            }
            
            foreach(var item in possibleCollidingEntities)
            {
                if(item.Collider == Entity.CollisionType.Pixel)
                {
                    if (IsColliding(checkArea, new List<Entity>() { item }))
                        collidingEntities.Add(item);
                }
                else if (item.Collider == Entity.CollisionType.Box)
                {
                    //if (IsOverlappingBox(new List<Entity>() { item })
                    //    collidingEntities.Add(item);
                }
            }                

            return collidingEntities;
        }

        public override bool Colliding(Rectangle offset, Entity.CollisionType CollisionType = Entity.CollisionType.Pixel)
        {
            int sx = (offset.Size.X == 0) ? (int)Entity.Size.X : offset.Size.X;
            int sy = (offset.Size.Y == 0) ? (int)Entity.Size.Y : offset.Size.Y;
            offset.Size = new Point(sx, sy);

            Rectangle checkArea = new Rectangle(
                (Entity.Position - Entity.Size/2).ToPoint() + offset.Location,
                offset.Size
            );

            checkArea.Size += new Point(AddWidth, AddHeight);
            checkArea.Location -= new Point(AddWidth, AddHeight);

            List<Entity> possibleCollidingEntities = new List<Entity>();
            foreach(Entity entity in Global.Entities.FindAll(e => e.LayerName == this.Entity.LayerName))
            {
                if (entity == Entity || !entity.CheckPixels)
                    continue;

                if (checkArea.Intersects(entity.BoundingBox))
                {
                    possibleCollidingEntities.Add(entity);
                }
            }
            
            if (IsColliding(checkArea, possibleCollidingEntities))
                return true;

            return false;
        }

        private bool IsColliding(Rectangle checkRect, List<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                
                if (entity.Collider == Entity.CollisionType.Pixel || entity.Collider == Entity.CollisionType.Platform)
                {
                    var Drawable = (entity.GetComponent<Drawable>() != null) ? entity.GetComponent<Drawable>() : entity.GetComponent<Sprite>();

                    if (Drawable == null)
                        return false;

                    Rectangle entityBox = entity.BoundingBox;

                    Rectangle.Intersect(ref checkRect, ref entityBox, out Rectangle intersectRect);

                    if (intersectRect.Width > 0 && intersectRect.Height > 0)
                    {
                        intersectRect.Location -= entityBox.Location;

                        int totalPixels = intersectRect.Width * intersectRect.Height;
                        Color[] colors = new Color[totalPixels];
                        Drawable.Texture2D.GetData(0, intersectRect, colors, 0, totalPixels);

                        for (int I = 0; I < colors.Length; I++)
                            if (colors[I].A != byte.MinValue)
                            {
                                if (entity.Trigger != Entity.TriggerTypes.None)
                                {
                                    if (!CollidingTriggers.Contains(entity))
                                        CollidingTriggers.Add(entity);

                                    if (entity.Trigger == Entity.TriggerTypes.Solid)
                                        return true;
                                }
                                else
                                {
                                    return true;
                                }
                            }
                    }
                }
                else if(entity.Collider == Entity.CollisionType.Box)
                {
                    Rectangle rect = new Rectangle(
                        (entity.BoundingBox.Center + entity.BoxColliderRect.Location) - entity.BoxColliderRect.Size/new Point(2, 2),
                        entity.BoxColliderRect.Size
                    );
                    if (Entity.BoundingBox.Intersects(rect))
                    {
                        if (entity.Trigger != Entity.TriggerTypes.None)
                        {
                            if (!CollidingTriggers.Contains(entity))
                                CollidingTriggers.Add(entity);

                            if (entity.Trigger == Entity.TriggerTypes.Solid)
                                return true;
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