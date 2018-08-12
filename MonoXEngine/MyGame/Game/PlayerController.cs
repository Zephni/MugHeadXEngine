﻿using Microsoft.Xna.Framework;
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
        bool Crouching = false;
        public bool ObstructCrouching = true;

        public Entity GraphicEntity;

        BaseCollider passThru;

        public PlayerController(BaseCollider collider)
        {
            Acceleration = 8f;
            Deceleration = 9f;
            JumpStrength = 4f;
            passThru = collider;
            MaxX = 1.2f;
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
            /*foreach(var entity in Global.Entities)
            {
                if(entity.Trigger && this.Collider.Colliding())
                {

                }
            }*/

            // Direction
            if (Global.InputManager.Held(InputManager.Input.Left))
                Direction = -1;
            else if (Global.InputManager.Held(InputManager.Input.Right))
                Direction = 1;

            if (MovementEnabled && CurrentJump < MaxJumps && IsGrounded && Global.InputManager.Pressed(InputManager.Input.Action1))
            {
                MoveY = -JumpStrength;
                CurrentJump++;
                Global.AudioController.Play("SFX/Jump");
            }

            if (!Crouching && MovementEnabled && Global.InputManager.Held(InputManager.Input.Left))
            {
                MoveX -= Acceleration * Global.DeltaTime;
                if(!Kinetic) GameGlobal.PlayerGraphic.RunAnimation("WalkLeft");
            }
            else if(!Crouching && MoveX < 0)
            {
                MoveX += Deceleration * Global.DeltaTime;
                if (MoveX >= -Deceleration * Global.DeltaTime)
                {
                    if(!Kinetic) GameGlobal.PlayerGraphic.RunAnimation("StandLeft");
                    MoveX = 0;
                }
            }else if (!Crouching && MovementEnabled && Global.InputManager.Held(InputManager.Input.Right))
            {
                MoveX += Acceleration * Global.DeltaTime;
                if (!Kinetic) GameGlobal.PlayerGraphic.RunAnimation("WalkRight");
            }
            else if (!Crouching && MoveX > 0)
            {
                MoveX -= Deceleration * Global.DeltaTime;
                if (MoveX <= Deceleration * Global.DeltaTime)
                {
                    if (!Kinetic) GameGlobal.PlayerGraphic.RunAnimation("StandRight");
                    MoveX = 0;
                }
            }

            if (MovementEnabled && !ObstructCrouching && Global.InputManager.Held(InputManager.Input.Down))
            {
                if (!Kinetic)
                {
                    Crouching = true;
                    
                    if (Global.InputManager.Held(InputManager.Input.Left) || Global.InputManager.Held(InputManager.Input.Right))
                    {
                        GameGlobal.PlayerGraphic.RunAnimation((Direction == 1) ? "CrawlRight" : "CrawlLeft");
                        MoveX = (Direction == 1) ? 1 : -1;
                    }
                    else
                    {
                        MoveX = 0;
                        GameGlobal.PlayerGraphic.RunAnimation((Direction == 1) ? "LayRight" : "LayLeft");
                    }
                }
            }

            if (IsGrounded)
            {
                CurrentJump = 0;

                if(MoveX == 0 && !Kinetic && !Crouching)
                    GameGlobal.PlayerGraphic.RunAnimation((Direction == 1) ? "StandRight" : "StandLeft");

                if (Crouching && !Global.InputManager.Held(InputManager.Input.Down))
                {
                    Crouching = false;
                    GameGlobal.PlayerGraphic.RunAnimation((Direction == 1) ? "StandRight" : "StandLeft");
                }
            }
            else
            {
                if (!Kinetic) GameGlobal.PlayerGraphic.RunAnimation((Direction == 1) ? "JumpRight" : "JumpLeft");
            }

            base.Update();
        }
    }
}
