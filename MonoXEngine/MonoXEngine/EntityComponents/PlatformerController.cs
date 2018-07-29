using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoXEngine.EntityComponents
{
    public class PlatformerController : Physics
    {
        public float Acceleration;
        public float Deceleration;
        public float JumpStrength;
        public int CurrentJump = 0;
        public int MaxJumps = 2;
        public bool MovementEnabled = true;

        BaseCollider passThru;

        public PlatformerController(BaseCollider collider)
        {
            Acceleration = 4f;
            Deceleration = 4f;
            JumpStrength = 4f;
            passThru = collider;
        }

        public override void Start()
        {
            this.Entity.AddComponent(passThru);
            this.Collider = passThru;
            passThru = null;
        }

        public override void Update()
        {
            if (MovementEnabled && CurrentJump < MaxJumps && Keyboard.GetState().IsKeyDown(Keys.Z) && !MugHeadXEngine.Engine.DisableJump)
            {
                MoveY = -JumpStrength;
                CurrentJump++;
            }

            if (IsGrounded)
                CurrentJump = 0;                

            if (MovementEnabled && Keyboard.GetState().IsKeyDown(Keys.Left))
                MoveX -= Acceleration * Global.DeltaTime;
            else if(MoveX < 0)
            {
                MoveX += Deceleration * Global.DeltaTime;
                if (MoveX >= -Deceleration * Global.DeltaTime) MoveX = 0;
            }
            if (MovementEnabled && Keyboard.GetState().IsKeyDown(Keys.Right))
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
