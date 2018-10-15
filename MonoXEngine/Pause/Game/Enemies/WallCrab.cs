using Microsoft.Xna.Framework;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using MugHeadXEngine.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XEditor;

namespace MyGame.Enemies
{
    public class WallCrab : Enemy
    {
        Entity TriggerCollider;

        #region Constructor
        public WallCrab(EntityInfo entityInfo) : base(entityInfo)
        {
            // Starting HP
            HP = 5;

            // Get data
            ZInterpreter data = new ZInterpreter(entityInfo.Data);

            if (data.HasKey("direction"))
                Direction = data.GetString("direction");

            Planted = (data.HasKey("planted")) ? data.GetString("planted") : "top";

            if (data.HasKey("speed"))
                Speed = data.GetFloat("speed");

            RotateSpeed = RotateSpeed * Speed * 12;

            if (data.HasKey("damage"))
                Damage = data.GetInt("damage");

            // Collider 
            TriggerCollider = new Entity(entity => {
                entity.Name = "Enemy";
                entity.LayerName = "Main";
                entity.SortingLayer = 100;
                entity.Data.Add("damage", Damage.ToString());

                entity.AddComponent(new Collider()).Run<Collider>(c => { c.ColliderType = Collider.ColliderTypes.Box; c.TriggerType = Collider.TriggerTypes.NonSolid; });

                entity.AddComponent(new Sprite()).Run<Sprite>(sprite => {
                    sprite.BuildRectangle(new Point(10, 10), Color.White);
                    sprite.Visible = false;
                });

                StaticCoroutines.CoroutineHelper.Always(() => {
                    if(Planted == "top")
                        entity.Position = Entity.Position + new Vector2(0, -8);
                    else if (Planted == "left")
                        entity.Position = Entity.Position + new Vector2(-8, 0);
                    else if (Planted == "bottom")
                        entity.Position = Entity.Position + new Vector2(0, 8);
                    else if (Planted == "right")
                        entity.Position = Entity.Position + new Vector2(8, 0);
                });
            });

            // Build entity
            Entity = new Entity(entity => {
                entity.LayerName = "Main";
                entity.SortingLayer = 6;
                entity.Position = (entityInfo.Position * 16);
                entity.Origin = new Vector2(0.5f, 1f);

                entity.AddComponent(new Collider()).Run<Collider>(c => { c.ColliderType = Collider.ColliderTypes.Box; c.TriggerType = Collider.TriggerTypes.NonSolid; });

                if (Planted == "top")
                {
                    entity.Position += new Vector2(12, 16);
                }
                else if (Planted == "right")
                {
                    entity.Position += new Vector2(0, 16);
                    entity.Rotation += (float)Math.PI / 2;
                }

                entity.AddComponent(new Sprite()).Run<Sprite>(sprite => {
                    Sprite = sprite;

                    sprite.LoadTexture("Entities/Enemies/WallCrab");
                    sprite.AddAnimation(new Animation("walking", 0.2f, "24,16".ToPoint(), "0,0 1,0 2,0 3,0 4,0 5,0".ToPointList()));
                    sprite.RunAnimation("walking");
                });

                entity.AddComponent(new Collider()).Run<Collider>(pc => { Collider = pc; });
            });
        }
        #endregion

        #region Properties
        Collider Collider;
        Sprite Sprite;
        string Direction = "none";
        string Planted = "top";
        float Speed = 0.25f;
        float RotateSpeed = 1;
        int Damage = 2;
        #endregion

        #region Methods
        public override void Update()
        {
            // Check positioning and direction
            if (Direction == "left")
            {
                if (Planted == "top" && !Collider.CheckOffset(new Offset(-12, 1))) ChangePlanted("left");
                else if (Planted == "bottom" && !Collider.CheckOffset(new Offset(-12, 1))) ChangePlanted("right");
                else if (Planted == "left" && !Collider.CheckOffset(new Offset(1, 8))) ChangePlanted("bottom");
                else if ((Planted == "right" && !Collider.CheckOffset(new Offset(-1, -8)))) ChangePlanted("top");
            }
            else if (Direction == "right") // Needs copying from above in places
            {
                if (Planted == "top" && !Collider.CheckOffset(new Offset(12, 1))) ChangePlanted("right");
                else if (Planted == "bottom" && !Collider.CheckOffset(new Offset(-12, -1))) ChangePlanted("left");
                else if (Planted == "right" && !Collider.CheckOffset(new Offset(-1, 8))) ChangePlanted("bottom");
                else if (Planted == "left" && !Collider.CheckOffset(new Offset(1, -8))) ChangePlanted("top");
            }

            // Move
            if (Planted != "none")
            {
                if (Direction == "left")
                {
                    if (Planted == "top") Entity.Position.X -= Speed;
                    else if (Planted == "left") Entity.Position.Y += Speed;
                    else if (Planted == "bottom") Entity.Position.X += Speed;
                    else if (Planted == "right") Entity.Position.Y -= Speed;
                }
                else if (Direction == "right")
                {
                    if (Planted == "top") Entity.Position.X += Speed;
                    else if (Planted == "right") Entity.Position.Y += Speed;
                    else if (Planted == "bottom") Entity.Position.X -= Speed;
                    else if (Planted == "left") Entity.Position.Y -= Speed;
                }
            }

            base.Update();
        }

        public void ChangePlanted(string newPlanting)
        {
            string rotateDir = "none";

            if (Direction == "left")
            {
                if (Planted == "left" && newPlanting == "top") rotateDir = "right";
                else rotateDir = "left";
            }
            else if (Direction == "right")
            {
                
                if (Planted == "right" && newPlanting == "top") rotateDir = "left";
                else rotateDir = "right";
            }

            Planted = "none";

            if (rotateDir != "none")
            {
                float start = Entity.Rotation;
                float finish = start + ((rotateDir== "left") ? -(float)(Math.PI / 2) : (float)(Math.PI / 2));

                StaticCoroutines.CoroutineHelper.RunOverX(0.6f, 10, t => {
                    Entity.Rotation = start - (start - finish) * (t+1) / 10;
                }, () => {
                    Entity.Rotation = finish;
                    Planted = newPlanting;
                });

                
            }
        }
        #endregion
    }
}
