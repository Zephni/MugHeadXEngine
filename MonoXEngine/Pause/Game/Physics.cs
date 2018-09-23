using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using MonoXEngine.EntityComponents;

namespace MyGame
{
    public class Physics : EntityComponent
    {
        public bool Kinetic;
        public bool Disabled;
        public float Gravity;
        public float MaxX;
        public float MaxUp;
        public float MaxDown;
        public float MoveX;
        public float MoveY;
        public bool IsGrounded;
        public BaseCollider Collider;
        public bool UsePlatforms = true;

        public Physics()
        {
            this.Kinetic = false;
            this.Disabled = false;
            this.Gravity = 10f;
            this.MaxX = 3;
            this.MaxUp = 4;
            this.MaxDown = 4;
            this.MoveX = 0;
            this.MoveY = 0;
            this.IsGrounded = false;
        }

        public override void Start()
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            
        }

        float floatingX;
        public override void Update()
        {
            if (Collider == null)
                return;
            
            if(!this.Kinetic && !this.Disabled)
            {
                // Apply gravity if not grounded
                if (!this.IsGrounded)
                    this.MoveY += (this.Gravity * Global.DeltaTime);

                // Clamp values
                this.MoveX = Math.Min(Math.Max(this.MoveX, -this.MaxX), this.MaxX);
                this.MoveY = Math.Min(Math.Max(this.MoveY, -this.MaxUp), this.MaxDown);

                int addSubOne = 0;
                if (Math.Abs(floatingX) >= 1)
                {
                    addSubOne = (floatingX >= 1) ? 1 : -1;
                    floatingX += (floatingX >= 1) ? -1 : 1;
                }

                floatingX += MoveX % 1;
                
                int finalX = (int)this.MoveX + addSubOne;

                // X move
                for (int X = 0; X < Math.Abs(finalX); X++)
                {
                    if (Collider.Colliding(new Point((finalX > 0) ? 1 : -1, 0)))
                    {
                        // Upwards slope check
                        if (IsGrounded && !Collider.Colliding(new Point((finalX > 0) ? 1 : -1, -1)))
                            this.Entity.Position.Y -= 1;
                        else
                            this.MoveX = 0;
                    }

                    // Downwards slope check
                    if (IsGrounded && !Collider.Colliding(new Point((finalX > 0) ? 1 : -1, 1)) && !Collider.Colliding(new Point((finalX > 0) ? 1 : -1, 1), Entity.CollisionType.Platform))
                        this.Entity.Position.Y += 1;

                    if (this.MoveX != 0)
                        this.Entity.Position.X += (finalX > 0) ? 1 : -1;
                }

                // Y move
                for (int Y = 0; Y < Math.Abs(this.MoveY); Y++)
                {
                    if (Collider.Colliding(new Point(0, (MoveY > 0) ? 1 : -1)))
                        this.MoveY = 0;

                    if(UsePlatforms && MoveY > 0) // Uses layer offset for platforms, can be turned off
                        if (Collider.Colliding(new Point(0, 1), Entity.CollisionType.Platform) && !Collider.Colliding(new Point(0, 0), Entity.CollisionType.Platform))
                            this.MoveY = 0;
                            

                    if (this.MoveY != 0)
                        this.Entity.Position.Y += (this.MoveY > 0) ? 1 : -1;
                }

                // Check if grounded or too deep in ground
                this.IsGrounded = false;
                if (Collider.Colliding(new Point(0, 1)) || (UsePlatforms && Collider.Colliding(new Point(0, 1), Entity.CollisionType.Platform) && !Collider.Colliding(new Point(0, 0), Entity.CollisionType.Platform)))
                    this.IsGrounded = true;
            }
        }
    }
}
