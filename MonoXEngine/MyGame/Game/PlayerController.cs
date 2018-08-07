using Microsoft.Xna.Framework;
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
        public float Acceleration;
        public float Deceleration;
        public float JumpStrength;
        public int CurrentJump = 0;
        public int MaxJumps = 2;
        public bool MovementEnabled = true;
        public List<Entity> CollidingEntities = new List<Entity>();
        public int Direction = 1;

        public Entity GraphicEntity;

        BaseCollider passThru;

        public PlayerController(BaseCollider collider)
        {
            Acceleration = 4f;
            Deceleration = 9f;
            JumpStrength = 4f;
            passThru = collider;
            MaxX = 2;
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
        }

        public override void Update()
        {
            /*foreach(var entity in Global.Entities)
            {
                if(entity.Trigger && this.Collider.Colliding())
                {

                }
            }*/

            if (MovementEnabled && CurrentJump < MaxJumps && IsGrounded && Global.InputManager.Pressed(InputManager.Input.Action1))
            {
                MoveY = -JumpStrength;
                CurrentJump++;
                Global.AudioController.Play("SFX/Jump");
            }

            if (IsGrounded)
            {
                CurrentJump = 0;
            }

            if (MovementEnabled && Global.InputManager.Held(InputManager.Input.Down))
            {
                if(Direction == 1)
                    GameGlobal.PlayerGraphic.RunAnimation("LayRight");
                else
                    GameGlobal.PlayerGraphic.RunAnimation("LayLeft");
            }
            else
            {
                if (Direction == 1)
                    GameGlobal.PlayerGraphic.RunAnimation("StandRight");
                else
                    GameGlobal.PlayerGraphic.RunAnimation("StandLeft");
            }

            if (MovementEnabled && Global.InputManager.Held(InputManager.Input.Left))
            {
                MoveX -= Acceleration * Global.DeltaTime;
                GameGlobal.PlayerGraphic.RunAnimation("WalkLeft");
            }
            else if(MoveX < 0)
            {
                MoveX += Deceleration * Global.DeltaTime;
                if (MoveX >= -Deceleration * Global.DeltaTime)
                {
                    GameGlobal.PlayerGraphic.RunAnimation("StandLeft");
                    MoveX = 0;
                }
            }

            if (MovementEnabled && Global.InputManager.Held(InputManager.Input.Right))
            {
                MoveX += Acceleration * Global.DeltaTime;
                GameGlobal.PlayerGraphic.RunAnimation("WalkRight");
            }
            else if (MoveX > 0)
            {
                MoveX -= Deceleration * Global.DeltaTime;
                if (MoveX <= Deceleration * Global.DeltaTime)
                {
                    GameGlobal.PlayerGraphic.RunAnimation("StandRight");
                    MoveX = 0;
                }
            }

            if(!IsGrounded)
            {
                if (Direction == 1)
                    GameGlobal.PlayerGraphic.RunAnimation("JumpRight");
                else
                    GameGlobal.PlayerGraphic.RunAnimation("JumpLeft");
            }

            if (MoveX != 0)
            {
                Direction = (MoveX > 0) ? 1 : -1;
            }

            base.Update();
        }
    }
}
