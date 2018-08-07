using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoXEngine.EntityComponents;
using System.Collections.Generic;
using MonoXEngine.Structs;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using StaticCoroutines;
using MugHeadXEngine.EntityComponents;
using MugHeadXEngine;
using System;
using XEditor;
using System.Reflection;

namespace MyGame.Scenes
{
    public class Level : Scene
    {
        // Objects
        public CameraController CameraController;

        // Debugging
        private Entity hotspotTest;

        public override void Initialise()
        {
            // Initialise and initial fade
            GameGlobal.InitialiseAssets();
            GameGlobal.Fader.RunFunction("FadeIn");

            // DEBUGGING
            #region Debugging
            // Debug text
            new Entity(entity => {
                entity.SortingLayer = 1;
                entity.LayerName = "Fade";
                entity.Position = -(Global.Resolution.ToVector2() / 2);
                entity.AddComponent(new Text()).Run<Text>(component => {
                    component.SetSpriteFont("HeartbitXX");
                    component.Color = Color.Yellow;

                    entity.UpdateAction = e => {
                        component.String = "Entities: " + Global.CountEntities().ToString() + ", Coroutines: " + Coroutines.Count.ToString() + ", FPS: " + Global.FPS.ToString();
                    };
                });
            });

            // Hotspot tester
            hotspotTest = new Entity(entity => {
                entity.AddComponent(new Sprite()).Run<Sprite>(d => {
                    d.BuildRectangle(new Point(4, 4), Color.Orange);
                });
                entity.SortingLayer = 10;
            });
            CoroutineHelper.Always(() => {
                //hotspotTest.Position = new Vector2(textTest.BoundingBox.Left + textTest.Size.X / 2, textTest.BoundingBox.Bottom);
            });
            #endregion

            // Backgrounds
            #region Backgrounds
            /*
            new Entity(entity => {
                entity.LayerName = "Background";
                entity.Position += new Vector2(0, 80);
                entity.AddComponent(new Drawable() { Texture2D = Global.Content.Load<Texture2D>("WaterTest") }).Run<Drawable>(component => {
                    Color[] colorArr = component.Texture2D.To1DArray();
                    CoroutineHelper.Always(() => {
                        component.Texture2D.ManipulateColors1D(colors => {
                            colors = (Color[])colorArr.Clone();
                            for (int y = 0; y < 100; y++)
                                colors.Shift(new Point(256, 100), new Rectangle(0, 1 * y, 256, 1), new Point(-(int)(Global.Camera.Position.X * (y + 1) * 0.005f), 1 * y));
                            return colors;
                        });
                    });
                });
            });*/
            #endregion

            // Player entity
            List<Entity> PlayerCollidingTriggers = new List<Entity>();
            GameGlobal.Player = new Entity(entity => {
                entity.SortingLayer = 1;

                entity.AddComponent(new Sprite()).Run<Sprite>(component => {
                    component.BuildRectangle(new Point(8, 8), Color.Blue);
                    component.Visible = false;
                });

                entity.AddComponent(new PlayerController(new PixelCollider()));

                GameGlobal.PlayerGraphicEntity = new Entity(e => {
                    e.SortingLayer = entity.SortingLayer;
                    e.CheckPixels = false;
                    e.AddComponent(new Sprite() { Texture2D = Global.Content.Load<Texture2D>("Entities/Pause") }).Run<Sprite>(s => {
                        s.AddAnimation(new Animation("StandLeft", 0.2f, new Point(32, 32), new Point(0, 0)));
                        s.AddAnimation(new Animation("StandRight", 0.2f, new Point(32, 32), new Point(0, 1)));
                        s.AddAnimation(new Animation("WalkLeft", 0.2f, new Point(32, 32), new Point(0, 2), new Point(1, 2), new Point(2, 2), new Point(3, 2)));
                        s.AddAnimation(new Animation("WalkRight", 0.2f, new Point(32, 32), new Point(0, 3), new Point(1, 3), new Point(2, 3), new Point(3, 3)));
                        s.AddAnimation(new Animation("JumpLeft", 0.2f, new Point(32, 32), new Point(0, 4), new Point(1, 4), new Point(2, 4), new Point(3, 4)));
                        s.AddAnimation(new Animation("JumpRight", 0.2f, new Point(32, 32), new Point(0, 5), new Point(1, 5), new Point(2, 5), new Point(3, 5)));
                        s.AddAnimation(new Animation("CrawlLeft", 0.2f, new Point(32, 32), new Point(0, 6), new Point(1, 6), new Point(2, 6), new Point(3, 6)));
                        s.AddAnimation(new Animation("CrawlRight", 0.2f, new Point(32, 32), new Point(0, 7), new Point(1, 7), new Point(2, 7), new Point(3, 7)));
                        s.AddAnimation(new Animation("LayLeft", 0.2f, new Point(32, 32), new Point(0, 6), new Point(1, 6)));
                        s.AddAnimation(new Animation("LayRight", 0.2f, new Point(32, 32), new Point(0, 7), new Point(1, 7)));
                    });
                });

                if (GameData.Get("Player/Position") != null)
                {
                    string[] pPosData = GameData.Get("Player/Position").Split(',');
                    entity.Position = new Vector2(Convert.ToInt16(pPosData[0]), Convert.ToInt16(pPosData[1]));
                }                

                entity.UpdateAction = e => {
                    if(entity.GetComponent<PlayerController>().MovementEnabled && PlayerCollidingTriggers.Find(t => t.Name == "CameraLock") == null)
                    {
                        CameraController.Target = e;
                        CameraController.ResetMinMax();
                    }

                    PlayerCollidingTriggers = new List<Entity>();

                    GameGlobal.PlayerGraphicEntity.Position = entity.Position + new Vector2(0, -10);
                };

                entity.CollidedWithTrigger = obj => {
                    if (!entity.GetComponent<PlayerController>().MovementEnabled)
                        return;

                    PlayerCollidingTriggers.Add(obj);

                    if (obj.Name == "CameraLock")
                    {
                        foreach(var item in obj.Data["Type"].Split(','))
                        {
                            if (item  == "LockXY")
                                CameraController.Target = obj;
                            else if (item == "LockX")
                                CameraController.TargetX = obj;
                            else if (item == "LockY")
                                CameraController.TargetY = obj;
                            else if (item == "LockTop")
                                CameraController.MinY = obj.BoundingBox.Top;
                            else if (item == "LockBottom")
                                CameraController.MaxY = obj.BoundingBox.Bottom;
                            else if (item == "LockRight")
                                CameraController.MaxX = obj.BoundingBox.Right;
                            else if (item == "LockLeft")
                                CameraController.MinX = obj.BoundingBox.Left;
                        }
                    }
                };
            });            

            // Load level
            LevelLoader levelLoader = new LevelLoader();
            levelLoader.Load(GameData.Get("Level"), (tiles, entities) => {

                // Create tilemap
                TileMap tileMap = new TileMap(new Point(16, 16), "Tilesets/Forest", tiles);
                tileMap.Build(new Point(32, 32));

                // Entities
                new EntityInfoInterpreter(entities);
            });

            // Camera
            CameraController = new CameraController();
            CameraController.Target = GameGlobal.Player;
            CameraController.Easing = 0.2f;
            CameraController.Mode = CameraController.Modes.LerpToTarget;
            CameraController.SnapOnce(GameGlobal.Player);

            // Level script
            LevelScripts levelScripts = new LevelScripts();
            Type type = levelScripts.GetType();
            MethodInfo method = type.GetMethod(GameData.Get("Level"));
            method.Invoke(levelScripts, new object[0]);
        }
        
        public override void Update()
        {
            CameraController.Update();

            // Temp speed up game
            if (Keyboard.GetState().IsKeyDown(Keys.W))
                MonoXEngineGame.Instance.DeltaTimeMultiplier = 30;
            else if (Keyboard.GetState().IsKeyDown(Keys.Q))
                MonoXEngineGame.Instance.DeltaTimeMultiplier = 0.2f;
            else
                MonoXEngineGame.Instance.DeltaTimeMultiplier = 1;

            if (Global.RunOnce("Restart", Keyboard.GetState().IsKeyDown(Keys.F2)))
            {
                Global.SceneManager.LoadScene("Menu");
            }


            if (Global.InputManager.Pressed(InputManager.Input.L1))
            {
                GameData.Set("Player/Position", GameGlobal.Player.Position.X.ToString() + "," + GameGlobal.Player.Position.Y.ToString());
                GameData.Save();
            }

            if (Global.InputManager.Pressed(InputManager.Input.L2))
            {
                GameData.Load();
                GameGlobal.Fader.RunFunction("FadeOut", e => {
                    Global.SceneManager.LoadScene("Level");
                });
            }
        }
    }
}