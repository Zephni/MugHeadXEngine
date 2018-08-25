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
using System.Linq;

namespace MyGame.Scenes
{
    public class Level : Scene
    {
        // Objects
        public static CameraController CameraController;

        public float LevelTime = 0;

        // Debugging
        private Entity hotspotTest;

        public override void Initialise()
        {
            // Initialise and
            GameGlobal.InitialiseAssets();

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
                        //component.String = "Entities: " + Global.CountEntities().ToString() + ", Coroutines: " + Coroutines.Count.ToString() + ", FPS: " + Global.FPS.ToString();
                        component.String = String.Join(", ", GameGlobal.Player.GetComponent<PixelCollider>().PrevCollidingTriggers.Select(x => x.Name).ToArray());
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
            GameGlobal.Player = new Entity();
            GameGlobal.Player.AddComponent(new PlayerController(new PixelCollider()));

            // Load level
            LevelLoader levelLoader = new LevelLoader();
            levelLoader.Load(GameData.Get("Level"), (tileset, tiles, entities) => {

                // Create tilemap
                TileMap tileMap = new TileMap(new Point(16, 16), "Tilesets/"+ tileset.Substring(0, tileset.LastIndexOf('.')), tiles);
                tileMap.Build(new Point(32, 32));

                // Map collision
                foreach (var tgroup in tileMap.TileGroupEntities)
                    if (tgroup.SortingLayer == GameGlobal.Player.SortingLayer)
                        tgroup.Collider = Entity.CollisionType.Pixel;
                    else if (tgroup.SortingLayer == GameGlobal.Player.SortingLayer-1)
                        tgroup.Collider = Entity.CollisionType.Platform;

                // Entities
                new EntityInfoInterpreter(entities);
            });

            // Camera
            CameraController = new CameraController
            {
                Target = GameGlobal.Player,
                Easing = 0.08f,
                MaxStep = 1000
            };
            CameraController.SnapOnce(GameGlobal.Player);
            CameraController.Mode = CameraController.Modes.LerpToTarget;

            // Level script
            LevelScripts levelScripts = new LevelScripts();
            Type type = levelScripts.GetType();
            MethodInfo method = type.GetMethod(GameData.Get("Level"));

            if(method != null)
                method.Invoke(levelScripts, new object[0]);

            CoroutineHelper.WaitRun(0.05f, () => {
                GameGlobal.Fader.RunFunction("FadeIn");
            });
        }
        
        public override void Update()
        {
            if (LevelTime < 0.5f)
                CameraController.SnapOnce();

            LevelTime += Global.DeltaTime;

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

            if (Global.InputManager.Pressed(InputManager.Input.Special1))
            {
                GameData.Set("Player/Position", GameGlobal.Player.Position.X.ToString() + "," + GameGlobal.Player.Position.Y.ToString());
                GameData.Save();
            }

            if (Global.InputManager.Pressed(InputManager.Input.Special2))
            {
                GameData.Load();
                GameGlobal.Fader.RunFunction("FadeOut", e => {
                    Global.SceneManager.LoadScene("Level");
                });
            }
        }
    }
}