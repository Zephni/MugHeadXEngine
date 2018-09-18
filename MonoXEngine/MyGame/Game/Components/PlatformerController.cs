using MonoXEngine;
using MonoXEngine.EntityComponents;

namespace MyGame
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
