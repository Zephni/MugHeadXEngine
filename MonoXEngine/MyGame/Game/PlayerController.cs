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

        BaseCollider passThru;

        List<Entity> PlayerCollidingTriggers = new List<Entity>();

        public PlayerController(BaseCollider collider)
        {
            passThru = collider;

            GameGlobal.PlayerGraphicEntity = new Entity(e => {
                e.SortingLayer = 4;
                e.CheckPixels = false;
                e.AddComponent(new Sprite() { Texture2D = Global.Content.Load<Texture2D>("Entities/Pause") }).Run<Sprite>(s => {
                    s.Visible = true;
                    s.AddAnimation(new Animation("Stand", 0.2f, new Point(32, 32), new Point(0, 0)));
                    s.AddAnimation(new Animation("Walk", 0.2f, new Point(32, 32), new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(3, 1)));
                    s.AddAnimation(new Animation("Jump", 0.2f, new Point(32, 32), new Point(0, 2), new Point(1, 2), new Point(2, 2), new Point(3, 2)));
                    s.AddAnimation(new Animation("Crawl", 0.15f, new Point(32, 32), new Point(0, 3), new Point(1, 3), new Point(2, 3), new Point(3, 3)));
                    s.AddAnimation(new Animation("Lay", 0.2f, new Point(32, 32), new Point(0, 3)));
                });
            });
        }

        public void Animate(string Alias)
        {
            GameGlobal.PlayerGraphicEntity.GetComponent<Sprite>().RunAnimation(Alias);
        }

        public override void Start()
        {
            this.Entity.AddComponent(passThru);
            this.Collider = passThru;
            passThru = null;
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
                Entity.Position = new Vector2(Convert.ToInt16(pPosData[0]) + 8, Convert.ToInt16(pPosData[1]));
            }

            if (GameData.Get("Player/Direction") != null)
            {
                Direction = (GameData.Get("Player/Direction") == "1") ? 1 : -1;
            }

            // New collides
            Entity.CollidedWithTrigger = obj => {
                if (obj.Name == "CameraLock")
                {
                    foreach (var item in obj.Data["Type"].Split(','))
                    {
                        if (item == "LockXY")
                        {
                            MyGame.Scenes.Level.CameraController.MinX = obj.BoundingBox.Left;
                            MyGame.Scenes.Level.CameraController.MaxX = obj.BoundingBox.Right;
                            MyGame.Scenes.Level.CameraController.MinY = obj.BoundingBox.Top;
                            MyGame.Scenes.Level.CameraController.MaxY = obj.BoundingBox.Bottom;
                        }
                        else if (item == "LockX")
                        {
                            MyGame.Scenes.Level.CameraController.MinX = obj.BoundingBox.Left;
                            MyGame.Scenes.Level.CameraController.MaxX = obj.BoundingBox.Right;
                        }
                        else if (item == "LockY")
                        {
                            MyGame.Scenes.Level.CameraController.MinY = obj.BoundingBox.Top;
                            MyGame.Scenes.Level.CameraController.MaxY = obj.BoundingBox.Bottom;
                        }
                        else if (item == "LockTop")
                            MyGame.Scenes.Level.CameraController.MinY = obj.BoundingBox.Top;
                        else if (item == "LockBottom")
                            MyGame.Scenes.Level.CameraController.MaxY = obj.BoundingBox.Bottom;
                        else if (item == "LockRight")
                            MyGame.Scenes.Level.CameraController.MaxX = obj.BoundingBox.Right;
                        else if (item == "LockLeft")
                            MyGame.Scenes.Level.CameraController.MinX = obj.BoundingBox.Left;
                    }
                }
            };

            // Current collides
            Entity.CollidingWithTrigger = obj => {
                if (!MovementEnabled)
                    return;

                if (obj.Name == "Chest")
                {
                    if (obj.GetComponent<Sprite>().CurrentAnimation == "Closed")
                        ObstructCrouching = true;

                    if (Global.InputManager.Pressed(InputManager.Input.Down))
                    {
                        if(obj.GetComponent<Sprite>().CurrentAnimation == "Closed")
                        {
                            obj.GetComponent<Sprite>().RunAnimation("Opening", false);
                        }
                    }
                }

                if (obj.Name == "Door")
                {
                    ObstructCrouching = true;
                    if (Global.InputManager.Pressed(InputManager.Input.Down))
                    {
                        MovementEnabled = false;
                        GameGlobal.Fader.RunFunction("FadeOut", e => {
                            GameData.Set("Level", obj.Data["Level"]);

                            string[] pos = obj.Data["Position"].Split(',');
                            Point pointPos = new Point(Convert.ToInt16(pos[0]) * 16, Convert.ToInt16(pos[1]) * 16);

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
                        Point pointPos = new Point(Convert.ToInt16(pos[0]) * 16, Convert.ToInt16(pos[1]) * 16);

                        GameData.Set("Player/Position", pointPos.X.ToString() + "," + pointPos.Y.ToString());
                        Global.SceneManager.LoadScene("Level");
                    });
                }

                if (obj.Name == "Water" && MovementMode == PlayerController.MovementModes.Normal && Entity.Position.Y - 4 > obj.Position.Y - obj.Size.Y / 2)
                {
                    MovementMode = PlayerController.MovementModes.Swimming;
                }
            };

            // New uncollides
            Entity.UnCollidedWithTrigger = obj => {
                if (!MovementEnabled)
                    return;

                if (obj.Name == "CameraLock")
                {
                    MyGame.Scenes.Level.CameraController.Target = GameGlobal.PlayerGraphicEntity;
                    MyGame.Scenes.Level.CameraController.ResetMinMax();
                }

                if (obj.Name == "Water")
                    MovementMode = PlayerController.MovementModes.Normal;

                if(obj.Name == "Door" || obj.Name == "Chest")
                    ObstructCrouching = false;
            };
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
                    Gravity = 8f;
                    JumpStrength = 3.3f;
                    MaxX = 1.7f;
                    MaxDown = 4f;
                }
                else if (MovementMode == MovementModes.Swimming)
                {
                    this.MoveX = 0;
                    this.MoveY = 0;
                    this.Gravity = 1f;
                    this.Acceleration = 1f;
                    this.JumpStrength = 1f;
                    MaxX = 1f;
                    MaxDown = 0.5f;
                }
            }
            LastMovementMode = MovementMode;

            // Direction
            MyGame.Scenes.Level.CameraController.Offset = (Direction == -1) ? new Vector2(-16, 0) : new Vector2(16, 0);

            if (MovementMode != MovementModes.None)
            {
                if (Global.InputManager.Held(InputManager.Input.Left)) Direction = -1;
                else if (Global.InputManager.Held(InputManager.Input.Right)) Direction = 1;
            }

            GameGlobal.PlayerGraphic.SpriteEffect = (Direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // Graphic position
            GameGlobal.PlayerGraphicEntity.Position = Entity.Position + new Vector2(0, -6);

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

                if (IsGrounded && MovementEnabled && ((!ObstructCrouching && Global.InputManager.Held(InputManager.Input.Down)) || (IsGrounded && Collider.Colliding(new Point(0, -10)))))
                {
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

            Entity.GetComponent<PixelCollider>().AddHeight = (Crouching) ? -12 : 0;

            GameData.Set("Player/Direction", (Direction == -1) ? "-1" : "1");

            base.Update();
        }
    }
}
