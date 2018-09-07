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
        #region Constructor
        public WallCrab(EntityInfo entityInfo) : base(entityInfo)
        {
            // Starting HP
            HP = 5;

            // Get data
            ZInterpreter data = new ZInterpreter(entityInfo.Data);

            if (data.HasKey("direction"))
                Direction = data.GetString("direction");

            if (data.HasKey("speed"))
                Speed = data.GetFloat("speed");

            RotateSpeed = RotateSpeed * Speed * 12;

            if (data.HasKey("damage"))
                Damage = data.GetInt("damage");

            // Build entity
            Entity = new Entity(entity => {
                entity.LayerName = "Main";
                entity.Position = (entityInfo.Position * 16) + new Vector2(8, 12);
                entity.Origin = new Vector2(0.5f, 0.5f);

                entity.AddComponent(new Sprite()).Run<Sprite>(sprite => {
                    Sprite = sprite;
                    //sprite.BuildRectangle(new Point(16, 16), Color.White);
                    sprite.LoadTexture("Entities/Enemies/WallCrab");
                    sprite.AddAnimation(new Animation("walking", 0.2f, "16,16".ToPoint(), "0,0".ToPointList()));
                    sprite.RunAnimation("walking");
                });

                entity.AddComponent(new PixelCollider()).Run<PixelCollider>(collider => { Collider = collider; });
            });

            // Collides
            Entity.CollidedWithTrigger = obj => {
                if (obj.Name == "Player")
                {
                    GameGlobal.Player.GetComponent<PlayerController>().Hurt(Damage);
                }
            };
        }
        #endregion

        #region Properties
        PixelCollider Collider;
        Sprite Sprite;
        string Direction = "right";
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
                if (Planted == "top" && !Collider.Colliding(new Point(-4, 1))) ChangePlanted("left");
                else if (Planted == "bottom" && !Collider.Colliding(new Point(4, -1))) ChangePlanted("right");
                else if (Planted == "left" && !Collider.Colliding(new Point(1, 4))) ChangePlanted("bottom");
                else if (Planted == "right" && !Collider.Colliding(new Point(-1, -4)) || (Planted == "right" && !Collider.Colliding(new Point(0, 1)))) ChangePlanted("top");
            }
            else if (Direction == "right") // Needs copying from above in places
            {
                if (Planted == "top" && !Collider.Colliding(new Point(4, 1))) ChangePlanted("right");
                else if (Planted == "bottom" && !Collider.Colliding(new Point(-4, -1))) ChangePlanted("left");
                else if (Planted == "right" && !Collider.Colliding(new Point(1, 4))) ChangePlanted("bottom");
                else if (Planted == "left" && !Collider.Colliding(new Point(-1, -4))) ChangePlanted("top");
            }

            // Move
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

            base.Update();
        }

        public void ChangePlanted(string newPlanting)
        {
            string rotateDir = "none";

            if (Direction == "left")
            {
                if ((Planted == "top" && newPlanting == "right") || (Planted == "left" && newPlanting == "top")) rotateDir = "right";
                else rotateDir = "left";
            }
            else if (Direction == "right")
            {
                if ((Planted == "top" && newPlanting == "left") || (Planted == "right" && newPlanting == "bottom")) rotateDir = "left";
                else rotateDir = "right";
            }


            if (rotateDir != "none")
            {
                float finish = (rotateDir == "left") ? Entity.Rotation - (float)(90 * Math.PI / 180) : Entity.Rotation + (float)(90 * Math.PI / 180);
                StaticCoroutines.CoroutineHelper.RunUntil(() => { return (rotateDir == "left") ? Entity.Rotation <= finish : Entity.Rotation >= finish; }, () => {
                    Entity.Rotation += (rotateDir == "left") ? -(Global.DeltaTime * RotateSpeed) : (Global.DeltaTime * RotateSpeed);
                }, () => {
                    Entity.Rotation = finish;
                });
            }

            Planted = newPlanting;
        }
        #endregion
    }
}
