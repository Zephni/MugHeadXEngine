﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using MyGame;
using MyGame.Scenes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MugHeadXEngine.EntityComponents
{
    public class PlayerController : Physics
    {
        public int MaxHP = 10;

        public int HP { get => hp; set { hp = value.Between(0, MaxHP); } }
        private int hp = 10;

        public enum MovementModes
        {
            None,
            Normal,
            Pushing,
            Paddleing,
            HangingEdge
        }
        public float Acceleration;
        public float Deceleration;
        public float JumpStrength;
        public int CurrentJump = 0;
        public int MaxJumps = 2;
        public bool MovementEnabled = true;
        public int Direction = 1;
        public bool Crouching = false;
        public bool ObstructCrouching = true;
        public int Pushing = 0;
        public Entity PushingObject;

        private MovementModes LastMovementMode = MovementModes.None;
        public MovementModes MovementMode = MovementModes.Normal;

        List<Entity> PlayerCollidingTriggers = new List<Entity>();

        public PlayerController()
        {
            GameGlobal.PlayerController = this;

            GameGlobal.PlayerGraphicEntity = new Entity(e => {
                e.SortingLayer = 4;
                e.AddComponent(new Sprite() { Texture2D = Global.Content.Load<Texture2D>("Entities/Pause") }).Run<Sprite>(s => {
                    s.Visible = true;
                    s.AddAnimation(new Animation("Stand", 0.2f, new Point(32, 32), "0,0".ToPointList()));
                    s.AddAnimation(new Animation("Walk", 0.2f, new Point(32, 32), "0,1 1,1 2,1 3,1".ToPointList()));
                    s.AddAnimation(new Animation("Jump", 0.2f, new Point(32, 32), "0,2 1,2 2,2 3,2".ToPointList()));
                    s.AddAnimation(new Animation("Crawl", 0.15f, new Point(32, 32), "0,3 1,3 2,3 3,3".ToPointList()));
                    s.AddAnimation(new Animation("Lay", 0.2f, new Point(32, 32), "0,3".ToPointList()));
                    s.AddAnimation(new Animation("Paddleing", 0.3f, new Point(32, 32), "0,4 1,4 2,4 3,4".ToPointList()));
                    s.AddAnimation(new Animation("Falling", 0.15f, new Point(32, 32), "1,5 2,5 3,5".ToPointList()));
                    s.AddAnimation(new Animation("Pulling", 0.15f, new Point(32, 32), "0,6 1,6 2,6 3,6".ToPointList()));
                    s.AddAnimation(new Animation("Hanging", 0.2f, new Point(32, 32), "3,4".ToPointList()));
                });
            });
        }

        public void Animate(string Alias)
        {
            GameGlobal.PlayerGraphicEntity.GetComponent<Sprite>().RunAnimation(Alias);
        }

        float HurtRespite = 0;
        public void Hurt(int amt)
        {
            if (HP > 0 && HurtRespite == 0)
            {
                GameMethods.ShowDamage(amt, Entity);
                Global.AudioController.Play("SFX/Hurt");
                HP -= amt;

                // HURT
                if(HP > 0)
                {
                    HurtRespite = 2;
                    MoveX = 0;
                    MoveY = -1;

                    StaticCoroutines.CoroutineHelper.RunFor(HurtRespite, t => {
                        GameGlobal.PlayerGraphicEntity.Opacity = 0.5f + (t.Wrap(0, 0.2f)) * 4;
                    }, () => {
                        GameGlobal.PlayerGraphicEntity.Opacity = 1f;
                        HurtRespite = 0;
                    });
                }
                // DIE
                else
                {
                    GameMethods.ShowDamage(amt, Entity);
                    Global.AudioController.Play("SFX/Died");

                    // Cam shake
                    Level.CameraController.Shake(1f, () => {
                        Level.CameraController.Target = null;
                    });
                    
                    // Dead
                    GameGlobal.Player.Destroy();
                    GameGlobal.PlayerGraphicEntity.Destroy();

                    StaticCoroutines.CoroutineHelper.WaitRun(2.2f, () => {
                        GameData.Load();
                        GameGlobal.Fader.RunFunction("FadeOut", e => {
                            Global.SceneManager.LoadScene("Level");
                        });
                    });
                }
            }
        }

        public override void Start()
        {
            Entity.AddComponent(new Collider()).Run<Collider>(c => { Collider = c; });

            Entity.Name = "Player";
            Entity.LayerName = "Main";
            Collider.TriggerType = Collider.TriggerTypes.NonSolid;
            Collider.ColliderType = Collider.ColliderTypes.Box;

            ObstructCrouching = false;

            //
            Entity.SortingLayer = 4;
            Entity.AddComponent(new Sprite()).Run<Sprite>(component => {
                component.BuildRectangle(new Point(8, 20), Color.White);
                component.Visible = false;
            });

            if (GameData.Get("Player/Position") != null)
            {
                string[] pPosData = GameData.Get("Player/Position").Split(',');

                Entity.Position = new Vector2(pPosData[0].ToFloat() + 0, pPosData[1].ToFloat());

                StaticCoroutines.CoroutineHelper.WaitRun(0.1f, () => {
                    for (int i = 0; i < 17; i++)
                    {
                        if (Collider.CheckOffset(new Offset(0, null, null, 1)))
                            Entity.Position.Y -= 1;
                        else
                            break;
                    }
                });
            }

            if (GameData.Get("Player/Direction") != null)
            {
                Direction = (GameData.Get("Player/Direction") == "1") ? 1 : -1;
            }
            
            // New collides
            Collider.CollidedWithTrigger = obj => {
                if (obj.Name == "MovablePlatform")
                {
                    /*
                    if(Collider.CollidingWith(new Rectangle(1, (int)Entity.Size.Y+1, (int)Entity.Size.X-2, 1), entity => entity.Name == "MovablePlatform").Count > 0)
                    {
                        float prev = obj.Position.X;
                        float cur = obj.Position.X;
                        StaticCoroutines.CoroutineHelper.RunUntil(() => { return !IsGrounded; }, () => {
                            cur = obj.Position.X;
                            if(prev != cur)
                            {
                                Entity.Position.X += (cur - prev);
                                prev = obj.Position.X;
                            }
                        });
                    }*/
                }
                if (obj.Name == "CameraLock")
                {
                    foreach (var item in obj.Data["Type"].Split(','))
                    {
                        if (item == "LockXY")
                        {
                            Level.CameraController.MinX = obj.BoundingBox.Left;
                            Level.CameraController.MaxX = obj.BoundingBox.Right;
                            Level.CameraController.MinY = obj.BoundingBox.Top;
                            Level.CameraController.MaxY = obj.BoundingBox.Bottom;
                        }
                        else if (item == "LockX")
                        {
                            Level.CameraController.MinX = obj.BoundingBox.Left;
                            Level.CameraController.MaxX = obj.BoundingBox.Right;
                        }
                        else if (item == "LockY")
                        {
                            Level.CameraController.MinY = obj.BoundingBox.Top;
                            Level.CameraController.MaxY = obj.BoundingBox.Bottom;
                        }
                        else if (item == "LockTop")
                            Level.CameraController.MinY = obj.BoundingBox.Top;
                        else if (item == "LockBottom")
                            Level.CameraController.MaxY = obj.BoundingBox.Bottom;
                        else if (item == "LockRight")
                            Level.CameraController.MaxX = obj.BoundingBox.Right;
                        else if (item == "LockLeft")
                            Level.CameraController.MinX = obj.BoundingBox.Left;
                    }
                }

                if (obj.Name == "TouchScript")
                {
                    string[] script = obj.Data["Script"].Split('.');
                    Type type = Type.GetType("MyGame."+script[0]);
                    MethodInfo mi = type.GetMethod(script[1], BindingFlags.Static | BindingFlags.Public);
                    mi.Invoke(null, new object[] { obj });
                }
            };

            // Current collides
            Collider.CollidingWithTrigger = obj => {
                if (!MovementEnabled || MovementMode == MovementModes.None)
                    return;

                if (obj.Name == "Enemy")
                {
                    Hurt(Convert.ToInt16(obj.Data["damage"]));
                }

                if (obj.Name == "Switch")
                {
                    if (Global.InputManager.Pressed(InputManager.Input.Up))
                    {
                        if(obj.Data["enabled"] == "true")
                        {
                            MyGame.Switch _switch = obj.GetComponent<MyGame.Switch>();
                            if (_switch.Entity.Data["value"] == "0")
                                _switch.UpdateValue("1");
                            else if (_switch.Entity.Data["value"] == "1")
                                _switch.UpdateValue("0");

                            Type type = Type.GetType("MyGame.SwitchScripts");
                            string id = obj.Data["id"];
                            MethodInfo mi = type.GetMethod(id, BindingFlags.Static | BindingFlags.Public);
                            mi.Invoke(null, new object[] { obj });
                        }
                    }
                }

                if (obj.Name == "InteractScript")
                {
                    if (Global.InputManager.Pressed(InputManager.Input.Up))
                    {
                        string[] script = obj.Data["Script"].Split('.');
                        Type type = Type.GetType("MyGame." + script[0]);
                        MethodInfo mi = type.GetMethod(script[1], BindingFlags.Static | BindingFlags.Public);
                        mi.Invoke(null, new object[] { obj });
                    }
                }

                if (obj.Name == "NPCChest")
                {
                    if (Global.InputManager.Pressed(InputManager.Input.Up))
                    {
                        Type type = typeof(NPCChest);
                        MethodInfo mi = type.GetMethod("ID_" + obj.Data["id"]);
                        mi.Invoke(null, new object[] { obj });
                    }
                }

                if (obj.Name == "Chest")
                {
                    if (Global.InputManager.Pressed(InputManager.Input.Up))
                    {
                        if(obj.GetComponent<Sprite>().CurrentAnimation == "Closed")
                        {
                            obj.GetComponent<Sprite>().RunAnimation("Opening", false);
                        }
                    }
                }

                if (obj.Name == "Door")
                {
                    if (Global.InputManager.Pressed(InputManager.Input.Up))
                    {
                        MovementEnabled = false;
                        GameGlobal.Fader.RunFunction("FadeOut", e => {
                            GameData.Set("Level", obj.Data["Level"]);

                            string[] pos = obj.Data["Position"].Split(',');
                            Point pointPos = new Point((pos[0].ToFloat() * 16).ToInt(), (pos[1].ToFloat() * 16).ToInt());

                            GameData.Set("Player/Position", pointPos.X.ToString() + "," + pointPos.Y.ToString());
                            Global.SceneManager.LoadScene("Level");
                        });
                    }
                }

                if (obj.Name == "AutoDoor")
                {
                    MovementEnabled = false;
                    MovementMode = PlayerController.MovementModes.None;
                    GameGlobal.Fader.RunFunction("FadeOut", e => {
                        GameData.Set("Level", obj.Data["Level"]);

                        string[] pos = obj.Data["Position"].Split(',');
                        Point pointPos = new Point((pos[0].ToFloat() * 16).ToInt(), (pos[1].ToFloat() * 16).ToInt());

                        GameData.Set("Player/Position", pointPos.X.ToString() + "," + pointPos.Y.ToString());
                        Global.SceneManager.LoadScene("Level");
                    });
                }

                // Entering water
                if (obj.Name == "Water" && MovementMode == PlayerController.MovementModes.Normal && Entity.Position.Y + 2 > obj.Position.Y - obj.Size.Y / 2)
                {
                    WaterLevel = obj.BoundingBox.Top;
                    MovementMode = PlayerController.MovementModes.Paddleing;
                }

                // Leaving water
                if (obj.Name == "Water" && MovementMode == PlayerController.MovementModes.Paddleing && Entity.Position.Y + 2 < obj.Position.Y - obj.Size.Y / 2)
                {
                    MovementMode = PlayerController.MovementModes.Normal;
                }
            };

            // New uncollides
            Collider.UnCollidedWithTrigger = obj => {
                if (!MovementEnabled)
                    return;

                if (obj.Name == "CameraLock")
                {
                    if(!Crouching && Pushing == 0) // This is because of bug
                    {
                        MyGame.Scenes.Level.CameraController.Target = GameGlobal.PlayerGraphicEntity;
                        MyGame.Scenes.Level.CameraController.ResetMinMax();
                    }
                }

                if (obj.Name == "Water" && MovementMode == PlayerController.MovementModes.Paddleing)
                    MovementMode = PlayerController.MovementModes.Normal;

                if(obj.Name == "Door" || obj.Name == "Chest" || obj.Name == "NPCChest")
                    ObstructCrouching = false;
            };
        }

        private float WaterLevel = 0;
        public override void Update()
        {
            // Override direction if pushing
            if (Pushing != 0)
                Direction = Pushing;

            // Movement mode change
            if (MovementMode != LastMovementMode)
            {
                if(MovementMode == MovementModes.Normal)
                {
                    GameGlobal.PlayerGraphic.Paused = false;

                    if (LastMovementMode == MovementModes.Paddleing)
                    {
                        Global.AudioController.Play("SFX/Splash").Volume = 0.5f;

                        // Splash graphic
                        new Entity(waterSplash => {
                            waterSplash.LayerName = "Main";
                            waterSplash.SortingLayer = GameGlobal.PlayerGraphicEntity.SortingLayer + 1;
                            waterSplash.Position = new Vector2(GameGlobal.PlayerGraphicEntity.Position.X, WaterLevel - 16);
                            waterSplash.Opacity = 0.4f;
                            waterSplash.AddComponent(new Sprite()).Run<Sprite>(c => {
                                c.Color = Color.AliceBlue;
                                c.LoadTexture("Graphics/Splash");
                                c.AddAnimation(new Animation("Splash", 0.1f, new Point(32, 32), "0,0 1,0 2,0 3,0 4,0 5,0".ToPointList()));
                                c.RunAnimation("Splash", false, () => {
                                    waterSplash.Destroy();
                                });
                            });
                        });
                    }

                    Acceleration = 4f;
                    Deceleration = 8f;
                    Gravity = 8f;
                    JumpStrength = 3.3f;
                    MaxX = 2f;
                    MaxDown = 4f;
                }
                else if (MovementMode == MovementModes.Paddleing)
                {
                    GameGlobal.PlayerGraphic.Paused = false;

                    Global.AudioController.Play("SFX/Splash").Volume = 0.5f;
                    this.MoveX = 0;
                    Gravity = 1;
                    this.Acceleration = 1f;
                    this.JumpStrength = 2f;
                    MaxX = 1f;

                    // Splash graphic
                    new Entity(waterSplash => {
                        waterSplash.LayerName = "Main";
                        waterSplash.SortingLayer = GameGlobal.PlayerGraphicEntity.SortingLayer + 1;
                        waterSplash.Position = new Vector2(GameGlobal.PlayerGraphicEntity.Position.X, WaterLevel-16);
                        waterSplash.Opacity = 0.4f;
                        waterSplash.AddComponent(new Sprite()).Run<Sprite>(c => {
                            c.LoadTexture("Graphics/Splash");
                            c.Color = Color.AliceBlue;
                            c.AddAnimation(new Animation("Splash", 0.1f, new Point(32, 32), "0,0 1,0 2,0 3,0 4,0 5,0".ToPointList()));
                            c.RunAnimation("Splash", false, () => {
                                waterSplash.Destroy();
                            });
                        });
                    });

                    if (this.MoveY >  3)
                        this.MoveY = 3;

                    StaticCoroutines.CoroutineHelper.RunUntil(() => { return this.MoveY < 0; }, () => {
                        this.MoveY -= 0.4f;
                    }, () => {
                        Gravity = 0;
                        MoveY = 0;
                        GameGlobal.PlayerGraphic.RunAnimation("Jump");

                        StaticCoroutines.CoroutineHelper.WaitRun(0.1f, () => {
                            StaticCoroutines.CoroutineHelper.RunUntil(() => { return Entity.Position.Y <= WaterLevel; }, () => {
                                Entity.Position.Y -= 0.8f;
                            }, () => {
                                Entity.Position.Y = WaterLevel + 1;
                                Gravity = 0;
                                MoveY = 0;
                                GameGlobal.PlayerGraphic.RunAnimation("Paddleing");
                            });
                        });
                    });
                }
            }
            LastMovementMode = MovementMode;

            // Temp
            if (Global.InputManager.Held(InputManager.Input.Up))
            {
                MoveY = 0;
                Entity.Position.Y -= 4;
            }
                

            if (MovementMode != MovementModes.None && MovementEnabled)
            {
                // Direction
                // Override direction if pushing
                if (MovementMode == MovementModes.Pushing)
                {
                    Direction = Pushing;
                }
                else
                {
                    if (Global.InputManager.Held(InputManager.Input.Left) && !Global.InputManager.Held(InputManager.Input.Right)) Direction = -1;
                    else if (Global.InputManager.Held(InputManager.Input.Right) && !Global.InputManager.Held(InputManager.Input.Left)) Direction = 1;
                    MyGame.Scenes.Level.CameraController.Offset = (Direction == -1) ? new Vector2(-16, 0) : new Vector2(16, 0);
                }
                    
            }
            else
            {
                //MyGame.Scenes.Level.CameraController.Offset = new Vector2(0, 0);
            }

            GameGlobal.PlayerGraphic.SpriteEffect = (Direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // Graphic position
            GameGlobal.PlayerGraphicEntity.Position = Entity.Position + new Vector2(0, -5);

            // No movement
            if (MovementMode == MovementModes.None)
            {
                MoveX = 0;
                MoveY = 0;
                GameGlobal.PlayerGraphic.Paused = true;
            }
            // HangingEdge movement
            else if (MovementMode == MovementModes.HangingEdge)
            {
                MoveX = 0;
                MoveY = 0;
                //Kinetic = true;
                Gravity = 0;

                GameGlobal.PlayerGraphic.RunAnimation("Hanging");

                // Abort
                if (Global.InputManager.Pressed(InputManager.Input.Down)) MovementMode = MovementModes.Normal;
                if (Direction > 0 && Global.InputManager.Pressed(InputManager.Input.Left)) MovementMode = MovementModes.Normal;
                if (Direction < 0 && Global.InputManager.Pressed(InputManager.Input.Right)) MovementMode = MovementModes.Normal;
                if(Global.InputManager.Pressed(InputManager.Input.Action1)) { MovementMode = MovementModes.Normal; MoveY = -2; }
            }
            // Normal movement
            else if (MovementMode == MovementModes.Normal)
            {
                Kinetic = false;

                if (MovementEnabled && CurrentJump < MaxJumps && IsGrounded && Global.InputManager.Pressed(InputManager.Input.Action1))
                {
                    MoveY = -JumpStrength;
                    CurrentJump++;
                    Global.AudioController.Play("SFX/Jump");
                }

                // Hanging
                int HangingOffset = 3;
                if (!IsGrounded && MoveY > 0 && Collider.CheckOffset(new Offset((Direction > 0) ? (int)Entity.Size.X + 1 : -1, HangingOffset, 1, 1)) && !Collider.CheckOffset(new Offset((Direction > 0) ? (int)Entity.Size.X + 1 : -1, HangingOffset-(int)MoveY-1, 1, 1)))
                {
                    MovementMode = MovementModes.HangingEdge;
                    MoveX = 0;
                    MoveY = 0;
                    Gravity = 0;
                }

                if (!Crouching && MovementEnabled && Global.InputManager.Held(InputManager.Input.Left) && !Global.InputManager.Held(InputManager.Input.Right))
                {
                    MoveX -= Acceleration * Global.DeltaTime;
                    if (!Kinetic && IsGrounded) GameGlobal.PlayerGraphic.RunAnimation("Walk");
                }
                else if (!Crouching && MoveX < 0)
                {
                    MoveX += Deceleration * Global.DeltaTime;
                    if (MoveX >= -Deceleration * Global.DeltaTime)
                    {
                        if (!Kinetic && IsGrounded) GameGlobal.PlayerGraphic.RunAnimation("Stand");
                        MoveX = 0;
                    }
                }
                else if (!Crouching && MovementEnabled && Global.InputManager.Held(InputManager.Input.Right) && !Global.InputManager.Held(InputManager.Input.Left))
                {
                    MoveX += Acceleration * Global.DeltaTime;
                    if (!Kinetic && IsGrounded) GameGlobal.PlayerGraphic.RunAnimation("Walk");
                }
                else if (!Crouching && MoveX > 0)
                {
                    MoveX -= Deceleration * Global.DeltaTime;
                    if (MoveX <= Deceleration * Global.DeltaTime)
                    {
                        if (!Kinetic && IsGrounded) GameGlobal.PlayerGraphic.RunAnimation("Stand");
                        MoveX = 0;
                    }
                }

                if (IsGrounded && MovementEnabled && ((!ObstructCrouching && Global.InputManager.Held(InputManager.Input.Down)) || (IsGrounded && Collider.CheckOffset(new Offset(2, -10, -4, 1)))))
                {
                    //Crouching = true;
                }

                if (!Kinetic && Crouching)
                {
                    if (Global.InputManager.Held(InputManager.Input.Left) && !Global.InputManager.Held(InputManager.Input.Right))
                    {
                        MoveX = -1;
                        GameGlobal.PlayerGraphic.RunAnimation("Crawl");
                    }
                    else if (Global.InputManager.Held(InputManager.Input.Right) && !Global.InputManager.Held(InputManager.Input.Left))
                    {
                        MoveX = 1;
                        GameGlobal.PlayerGraphic.RunAnimation("Crawl");
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

                    if (Crouching && !Global.InputManager.Held(InputManager.Input.Down) && !Collider.CheckOffset(new Offset(0, -10)))
                    {
                        Crouching = false;
                        GameGlobal.PlayerGraphic.RunAnimation("Stand");
                    }

                    // Pushing
                    if (Pushing == 0)
                    {
                        PushingObject = null;
                        if (IsGrounded && Collider.OverlappingEntity(p => p.Name == "MovableObject"))
                        {
                            Entity movableObject = Collider.OverlappingEntities.Find(p => p.Name == "MovableObject");

                            if (Global.InputManager.Pressed(InputManager.Input.Action2))
                            {
                                PushingObject = movableObject;
                                Pushing = Direction;
                                MovementMode = MovementModes.Pushing;
                            }
                        }
                    }
                }
                else
                {
                    if (!Kinetic)
                    {
                        if (MovementEnabled && MoveY > -1)
                        {
                            GameGlobal.PlayerGraphic.RunAnimation("Falling", false);
                        }
                        else
                        {
                            GameGlobal.PlayerGraphic.RunAnimation("Jump");
                        }
                    }
                }
            }
            // Pushing movement
            else if (MovementMode == MovementModes.Pushing)
            {
                int originalX = (int)Entity.Position.X;
                Direction = Pushing;

                Collider pushingObjectCollider = PushingObject.GetComponent<Collider>();

                if (Pushing == 1)
                {
                    if (Global.InputManager.Held(InputManager.Input.Left) && !Collider.CheckOffset(new Offset(-1, -2, 1, (int)Entity.Size.Y - 2))) { Entity.Position.X -= 0.5f; PushingObject.Position.X = Entity.Position.X + ((Pushing == 1) ? 12 : -12); }
                    if (Global.InputManager.Held(InputManager.Input.Right) && !pushingObjectCollider.CheckOffset(new Offset((int)PushingObject.Size.X + 2, -2, 1, (int)PushingObject.Size.Y - 2))) { Entity.Position.X += 0.5f; PushingObject.Position.X = Entity.Position.X + ((Pushing == 1) ? 12 : -12); }
                }
                else if (Pushing == -1)
                {
                    if (Global.InputManager.Held(InputManager.Input.Left) && !pushingObjectCollider.CheckOffset(new Offset(-2, -2, 1, (int)PushingObject.Size.Y - 2))) { Entity.Position.X -= 0.5f; PushingObject.Position.X = Entity.Position.X + ((Pushing == 1) ? 12 : -12); }
                    if (Global.InputManager.Held(InputManager.Input.Right) && !Collider.CheckOffset(new Offset((int)Entity.Size.X + 1, -2, 1, (int)Entity.Size.Y - 2))) { Entity.Position.X += 0.5f; PushingObject.Position.X = Entity.Position.X + ((Pushing == 1) ? 12 : -12); }
                }

                if (Collider.CheckOffset(new Offset(1, Collider.CheckArea.Height - 1, Collider.CheckArea.Width - 2, 1), p => p.Name != "MovableObject")) // Needs sorting
                {
                    Entity.Position.Y -= 1;
                }

                if (GameGlobal.PlayerGraphic.CurrentAnimation != "Pulling")
                    GameGlobal.PlayerGraphic.RunAnimation("Pulling");

                GameGlobal.PlayerGraphic.Paused = (originalX == (int)Entity.Position.X);

                if (Global.InputManager.Pressed(InputManager.Input.Action2) || !PushingObject.GetComponent<MoveableObject>().IsGrounded)
                {
                    Pushing = 0;
                    MovementMode = MovementModes.Normal;
                }
            }
            // Paddleing movement
            else if (MovementMode == MovementModes.Paddleing)
            {
                if (MovementEnabled && GameGlobal.Player.Position.Y == WaterLevel + 1 && Global.InputManager.Pressed(InputManager.Input.Action1))
                {
                    MoveY = -JumpStrength;
                }

                if (!Crouching && MovementEnabled && Global.InputManager.Held(InputManager.Input.Left) && !Global.InputManager.Held(InputManager.Input.Right))
                {
                    MoveX -= Acceleration * Global.DeltaTime;
                }
                else if (!Crouching && MoveX < 0)
                {
                    MoveX += Deceleration * Global.DeltaTime;
                    if (MoveX >= -Deceleration * Global.DeltaTime)
                        MoveX = 0;
                }
                else if (!Crouching && MovementEnabled && Global.InputManager.Held(InputManager.Input.Right) && !Global.InputManager.Held(InputManager.Input.Left))
                {
                    MoveX += Acceleration * Global.DeltaTime;
                }
                else if (!Crouching && MoveX > 0)
                {
                    MoveX -= Deceleration * Global.DeltaTime;
                    if (MoveX <= Deceleration * Global.DeltaTime)
                        MoveX = 0;
                }
            }

            //MainCollider.AddHeight = (Crouching) ? -12 : 0;

            GameData.Set("Player/Direction", (Direction == -1) ? "-1" : "1");

            //CameraController.Instance.Mode = CameraController.Modes.SnapToTarget;

            base.Update();
        }
    }
}
