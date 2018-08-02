using Microsoft.Xna.Framework.Input;
using MonoXEngine;
using MonoXEngine.EntityComponents;
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

        BaseCollider passThru;

        public PlayerController(BaseCollider collider)
        {
            Acceleration = 4f;
            Deceleration = 9f;
            JumpStrength = 4f;
            passThru = collider;
            MaxX = 2;
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
            }

            if (IsGrounded)
                CurrentJump = 0;                

            if (MovementEnabled && Global.InputManager.Held(InputManager.Input.Left))
                MoveX -= Acceleration * Global.DeltaTime;
            else if(MoveX < 0)
            {
                MoveX += Deceleration * Global.DeltaTime;
                if (MoveX >= -Deceleration * Global.DeltaTime) MoveX = 0;
            }
            if (MovementEnabled && Global.InputManager.Held(InputManager.Input.Right))
                MoveX += Acceleration * Global.DeltaTime;
            else if (MoveX > 0)
            {
                MoveX -= Deceleration * Global.DeltaTime;
                if (MoveX <= Deceleration * Global.DeltaTime) MoveX = 0;
            }

            base.Update();
        }
    }
}
