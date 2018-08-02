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

            new Entity(entity => {
                entity.LayerName = "Background";
                entity.Position += new Vector2(0, 0);
                entity.AddComponent(new CameraOffsetTexture(){Texture2D = Global.Content.Load<Texture2D>("Backgrounds/night"), Coefficient = new Vector2(0, 0) });
            });
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
                    component.BuildRectangle(new Point(16, 16), Color.Blue);
                });

                entity.AddComponent(new PlayerController(new PixelCollider()));

                if(GameData.Get("Player/Position") != null)
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

            if (Global.RunOnce("Restart", Keyboard.GetState().IsKeyDown(Keys.Space)))
            {
                GameGlobal.Fader.RunFunction("FadeOut", c => {
                    Global.SceneManager.LoadScene("Menu");
                });
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