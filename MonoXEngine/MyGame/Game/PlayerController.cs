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

        List<Entity> PlayerCollidingTriggers = new List<Entity>();

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

            //
            Entity.SortingLayer = 4;
            Entity.AddComponent(new Sprite()).Run<Sprite>(component => {
                component.BuildRectangle(new Point(8, 20), Color.Blue);
                component.Visible = false;
            });

            GameGlobal.PlayerGraphicEntity = new Entity(e => {
                e.SortingLayer = Entity.SortingLayer;
                e.CheckPixels = false;
                e.AddComponent(new Sprite() { Texture2D = Global.Content.Load<Texture2D>("Entities/Pause") }).Run<Sprite>(s => {
                    s.AddAnimation(new Animation("Stand", 0.2f, new Point(32, 32), new Point(0, 0)));
                    s.AddAnimation(new Animation("Walk", 0.2f, new Point(32, 32), new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(3, 1)));
                    s.AddAnimation(new Animation("Jump", 0.2f, new Point(32, 32), new Point(0, 2), new Point(1, 2), new Point(2, 2), new Point(3, 2)));
                    s.AddAnimation(new Animation("Crawl", 0.15f, new Point(32, 32), new Point(0, 3), new Point(1, 3), new Point(2, 3), new Point(3, 3)));
                    s.AddAnimation(new Animation("Lay", 0.2f, new Point(32, 32), new Point(0, 3)));
                });
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

            Entity.CollidedWithTrigger = obj => {
                if (!MovementEnabled)
                    return;

                PlayerCollidingTriggers.Add(obj);

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
                    this.Gravity = 0.1f;
                    this.Acceleration = 1f;
                    this.JumpStrength = 0.2f;
                    MaxX = 1f;
                    MaxDown = 0.5f;
                }
            }
            LastMovementMode = MovementMode;

            // Uncolliding
            if (MovementEnabled && PlayerCollidingTriggers.Find(t => t.Name == "CameraLock") == null)
            {
                CameraController.Instance.Target = Entity;
                CameraController.Instance.ResetMinMax();
            }

            if (MovementEnabled && PlayerCollidingTriggers.Find(t => t.Name == "Water") == null)
            {
                MovementMode = PlayerController.MovementModes.Normal;
            }

            if (MovementEnabled && PlayerCollidingTriggers.Find(t => t.Name == "Door") == null)
            {
                ObstructCrouching = false;
            }

            PlayerCollidingTriggers = new List<Entity>();

            // Dir crouch temp
            if (Direction == -1)
                CameraController.Instance.Offset = new Vector2(-16, 0);
            if (Direction == 1)
                CameraController.Instance.Offset = new Vector2(16, 0);

            if (!Crouching)
                GameGlobal.PlayerGraphicEntity.Position = Entity.Position + new Vector2(0, -6);
            else
                GameGlobal.PlayerGraphicEntity.Position = Entity.Position + new Vector2(0, -11);

            // Direction
            if (MovementMode != MovementModes.None)
            {
                if (Global.InputManager.Held(InputManager.Input.Left)) Direction = -1;
                else if (Global.InputManager.Held(InputManager.Input.Right)) Direction = 1;
            }

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

                if (MovementEnabled && ((!ObstructCrouching && Global.InputManager.Held(InputManager.Input.Down)) || Collider.Colliding(new Point(0, -10))))
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

            GameData.Set("Player/Direction", (Direction == -1) ? "-1" : "1");

            base.Update();
        }
    }
}
