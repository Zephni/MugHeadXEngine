using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using MyGame;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MugHeadXEngine.EntityComponents
{
    public class PlayerController : Physics
    {
        public enum MovementModes
        {
            None,
            Normal,
            Swimming
        }

        public float Acceleration;
        public float Deceleration;
        public float JumpStrength;
        public int CurrentJump = 0;
        public int MaxJumps = 2;
        public bool MovementEnabled = true;
        public List<Entity> CollidingEntities = new List<Entity>();
        public int Direction = 1;
        public bool Crouching = false;
        public bool ObstructCrouching = true;
        
        private MovementModes LastMovementMode = MovementModes.None;
        public MovementModes MovementMode = MovementModes.Normal;

        public Entity GraphicEntity;

        BaseCollider passThru;

        public PlayerController(BaseCollider collider)
        {
            passThru = collider;
        }

        public void Animate(string Alias)
        {
            GraphicEntity.GetComponent<Sprite>().RunAnimation(Alias);
        }

        public override void Start()
        {
            this.Entity.AddComponent(passThru);
            this.Collider = passThru;
            passThru = null;
            ObstructCrouching = false;
        }



        public override void Update()
        {
            // Movement mode change
            if(MovementMode != LastMovementMode)
            {
                if(MovementMode == MovementModes.Normal)
                {
                    Acceleration = 4f;
                    Deceleration = 8f;
                    Gravity = 7f;
                    JumpStrength = 3.5f;
                    MaxX = 1.7f;
                    MaxDown = 4f;
                }
                else if (MovementMode == MovementModes.Swimming)
                {
                    this.MoveX = 0;
                    this.MoveY = 0;
                    this.Gravity = 0.5f;
                    this.Acceleration = 1f;
                    this.JumpStrength = 2f;
                    MaxX = 1f;
                    MaxDown = 0.5f;
                }
            }
            LastMovementMode = MovementMode;

            // Direction
            if (Global.InputManager.Held(InputManager.Input.Left)) Direction = -1;
            else if (Global.InputManager.Held(InputManager.Input.Right)) Direction = 1;
            GameGlobal.PlayerGraphic.SpriteEffect = (Direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // Normal movement
            if (MovementMode == MovementModes.Normal)
            {
                if (MovementEnabled && CurrentJump < MaxJumps && IsGrounded && Global.InputManager.Pressed(InputManager.Input.Action1))
                {
                    MoveY = -JumpStrength;
                    CurrentJump++;
                    Global.AudioController.Play("SFX/Jump");
                }

                if (!Crouching && MovementEnabled && Global.InputManager.Held(InputManager.Input.Left))
                {
                    MoveX -= Acceleration * Global.DeltaTime;
                    if (!Kinetic) GameGlobal.PlayerGraphic.RunAnimation("Walk");
                }
                else if (!Crouching && MoveX < 0)
                {
                    MoveX += Deceleration * Global.DeltaTime;
                    if (MoveX >= -Deceleration * Global.DeltaTime)
                    {
                        if (!Kinetic) GameGlobal.PlayerGraphic.RunAnimation("Stand");
                        MoveX = 0;
                    }
                }
                else if (!Crouching && MovementEnabled && Global.InputManager.Held(InputManager.Input.Right))
                {
                    MoveX += Acceleration * Global.DeltaTime;
                    if (!Kinetic) GameGlobal.PlayerGraphic.RunAnimation("Walk");
                }
                else if (!Crouching && MoveX > 0)
                {
                    MoveX -= Deceleration * Global.DeltaTime;
                    if (MoveX <= Deceleration * Global.DeltaTime)
                    {
                        if (!Kinetic) GameGlobal.PlayerGraphic.RunAnimation("Stand");
                        MoveX = 0;
                    }
                }

                if (MovementEnabled && !ObstructCrouching && Global.InputManager.Held(InputManager.Input.Down))
                {
                    if (!Crouching)
                    {
                        Entity.GetComponent<Sprite>().BuildRectangle(new Point(8, 10), Color.Blue);
                        Entity.Position += new Vector2(0, 6);
                    }

                    Crouching = true;
                }

                if (!Kinetic && Crouching)
                {
                    if (Global.InputManager.Held(InputManager.Input.Left) || Global.InputManager.Held(InputManager.Input.Right))
                    {
                        GameGlobal.PlayerGraphic.RunAnimation("Crawl");
                        MoveX = (Direction == 1) ? 1 : -1;
                    }
                    else
                    {
                        MoveX = 0;
                        GameGlobal.PlayerGraphic.RunAnimation("Lay");
                    }
                }

                if (IsGrounded)
                {
                    CurrentJump = 0;

                    if (MoveX == 0 && !Kinetic && !Crouching)
                    {
                        GameGlobal.PlayerGraphic.RunAnimation("Stand");
                    }

                    if (Crouching && !Global.InputManager.Held(InputManager.Input.Down) && !Collider.Colliding(new Point(0, -10)))
                    {
                        Crouching = false;
                        GameGlobal.PlayerGraphic.RunAnimation("Stand");
                        Entity.GetComponent<Sprite>().BuildRectangle(new Point(8, 20), Color.Blue);
                        Entity.Position += new Vector2(0, -6);
                    }
                }
                else
                {
                    if (!Kinetic) GameGlobal.PlayerGraphic.RunAnimation("Jump");
                }
            }

            // Swimming movement
            if(MovementMode == MovementModes.Swimming)
            {
                if (MovementEnabled && Global.InputManager.Pressed(InputManager.Input.Action1))
                {
                    MoveY = -JumpStrength;
                    Global.AudioController.Play("SFX/Jump");
                }

                if (!Crouching && MovementEnabled && Global.InputManager.Held(InputManager.Input.Left))
                {
                    MoveX -= Acceleration * Global.DeltaTime;
                    if (!Kinetic) GameGlobal.PlayerGraphic.RunAnimation("Walk");
                }
                else if (!Crouching && MoveX < 0)
                {
                    MoveX += Deceleration * Global.DeltaTime;
                    if (MoveX >= -Deceleration * Global.DeltaTime)
                    {
                        if (!Kinetic) GameGlobal.PlayerGraphic.RunAnimation("Stand");
                        MoveX = 0;
                    }
                }
                else if (!Crouching && MovementEnabled && Global.InputManager.Held(InputManager.Input.Right))
                {
                    MoveX += Acceleration * Global.DeltaTime;
                    if (!Kinetic) GameGlobal.PlayerGraphic.RunAnimation("Walk");
                }
                else if (!Crouching && MoveX > 0)
                {
                    MoveX -= Deceleration * Global.DeltaTime;
                    if (MoveX <= Deceleration * Global.DeltaTime)
                    {
                        if (!Kinetic) GameGlobal.PlayerGraphic.RunAnimation("Stand");
                        MoveX = 0;
                    }
                }
            }

            base.Update();
        }
    }
}
