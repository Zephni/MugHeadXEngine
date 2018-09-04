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

            RotateSpeed = RotateSpeed * Speed * 4;

            if (data.HasKey("damage"))
                Damage = data.GetInt("damage");

            // Build entity
            Entity = new Entity(entity => {
                entity.LayerName = "Main";
                entity.Position = (entityInfo.Position * 16) + new Vector2(8, 8);
                entity.Origin = new Vector2(0.5f, 0.5f);

                entity.AddComponent(new Sprite()).Run<Sprite>(sprite => {
                    Sprite = sprite;
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
        string Direction = "left";
        string Planted = "top";
        float Speed = 0.25f;
        float RotateSpeed = 0.05f;
        int Damage = 2;
        #endregion

        #region Methods
        public override void Update()
        {
            // Check positioning and direction
            if (Direction == "left")
            {
                if (Planted == "top" && !Collider.Colliding(new Point(-3, 1))) ChangePlanted("left");
                else if (Planted == "left" && !Collider.Colliding(new Point(1, 3))) ChangePlanted("bottom");
                else if (Planted == "bottom" && !Collider.Colliding(new Point(3, -1))) ChangePlanted("right");
                else if (Planted == "right" && !Collider.Colliding(new Point(-1, -3))) ChangePlanted("top");
            }
            else if (Direction == "right") // Needs copying from above in places
            {
                if (Planted == "top" && !Collider.Colliding(new Point(3, 1))) ChangePlanted("right");
                else if (Planted == "right" && !Collider.Colliding(new Point(-1, 3))) ChangePlanted("bottom");
                else if (Planted == "bottom" && !Collider.Colliding(new Point(-3, -1))) ChangePlanted("left");
                else if (Planted == "right" && !Collider.Colliding(new Point(1, -3))) ChangePlanted("top");
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
            float r = 0;
            StaticCoroutines.CoroutineHelper.RunUntil(() => { return r >= Math.PI / 2; }, () => {
                r += (Direction == "left") ? RotateSpeed : -RotateSpeed;
                Entity.Rotation += (Direction == "left") ? -RotateSpeed : RotateSpeed;
            });

            Planted = newPlanting;
        }
        #endregion
    }
}
