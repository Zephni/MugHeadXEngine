using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace MonoXEngine.EntityComponents
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

                // X move
                if (Collider.Colliding(new Point(Convert.ToInt16(Math.Ceiling(MoveX)), 0)))
                {
                    // Upwards slope check
                    if (IsGrounded && !Collider.Colliding(new Point(Convert.ToInt16(Math.Ceiling(MoveX)), -1)))
                        this.Entity.Position.Y -= 1;
                    else
                        this.MoveX = 0;
                }

                // Downwards slope check
                if (IsGrounded && !Collider.Colliding(new Point(Convert.ToInt16(Math.Ceiling(MoveX)), 1)) && !Collider.Colliding(new Point(Convert.ToInt16(Math.Ceiling(MoveX)), 1), -1))
                    this.Entity.Position.Y += 1;

                if (this.MoveX != 0)
                    this.Entity.Position.X += this.MoveX;

                // Y move
                if (Collider.Colliding(new Point(0, Convert.ToInt16(Math.Ceiling(MoveY)))))
                    this.MoveY = 0;

                if(UsePlatforms && MoveY > 0) // Uses layer offset for platforms, can be turned off
                    if (Collider.Colliding(new Point(0, 1), -1) && !Collider.Colliding(new Point(0, 0), -1))
                        this.MoveY = 0;
                            

                if (this.MoveY != 0)
                    this.Entity.Position.Y += this.MoveY;

                // Always check if colliding in center (This was added for trigger detection)
                Collider.Colliding(new Point(0, 0));

                // Check if grounded or too deep in ground
                this.IsGrounded = false;
                if (Collider.Colliding(new Point(0, 1)) || (UsePlatforms && Collider.Colliding(new Point(0, 1), -1) && !Collider.Colliding(new Point(0, 0), -1)))
                    this.IsGrounded = true;
            }
        }


    }
}
