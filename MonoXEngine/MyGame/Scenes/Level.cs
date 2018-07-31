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

namespace MyGame.Scenes
{
    public class Level : Scene
    {
        // Static entities
        public Entity Player;

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
            new Entity(entity => {
                entity.LayerName = "Background";
                entity.Position += new Vector2(0, -80);
                entity.AddComponent(new Drawable(){
                    Texture2D = Global.Content.Load<Texture2D>("SkyTest"),
                });
            });
            new Entity(entity => {
                entity.LayerName = "Background";
                entity.Position += new Vector2(0, -50);
                entity.AddComponent(new CameraOffsetTexture()
                {
                    Texture2D = Global.Content.Load<Texture2D>("MountainsTest"),
                    Coefficient = new Vector2(0, 0)
                });
            });
            new Entity(entity => {
                entity.LayerName = "Background";
                entity.Position += new Vector2(0, 0);
                entity.AddComponent(new CameraOffsetTexture()
                {
                    Texture2D = Global.Content.Load<Texture2D>("BGTest"),
                    Coefficient = new Vector2(0, 0)
                });
            });
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
            });

            // Player entity
            Player = new Entity(entity => {
                entity.SortingLayer = 1;

                entity.AddComponent(new Sprite()).Run<Sprite>(component => {
                    component.BuildRectangle(new Point(16, 16), Color.White);
                });

                entity.AddComponent(new PlayerController(new PixelCollider()));

                if(GameData.Get("Player/Position") != null)
                {
                    string[] pPosData = GameData.Get("Player/Position").Split(',');
                    entity.Position = new Vector2(Convert.ToInt16(pPosData[0]), Convert.ToInt16(pPosData[1]));
                }
            });

            // Load level
            LevelLoader levelLoader = new LevelLoader();
            levelLoader.Load(GameData.Get("Level"), (tiles, entities) => {

                // Create tilemap
                TileMap tileMap = new TileMap(new Point(16, 16), "Tileset", tiles);
                tileMap.Build(new Point(32, 32));

                // Entities
                new EntityInfoInterpreter(entities);
            });         
        }
        
        public override void Update()
        {
            Vector2 camPos = (Player != null) ? new Vector2(Player.Position.X, Player.Position.Y) : Vector2.Zero;
            Global.Camera.Position = camPos;

            

            if (Global.RunOnce("Restart", Keyboard.GetState().IsKeyDown(Keys.Space)))
            {
                GameGlobal.Fader.RunFunction("FadeOut", c => {
                    Global.SceneManager.LoadScene("Menu");
                });
            }

            if (Global.InputManager.Pressed(InputManager.Input.L1))
            {
                GameData.Set("Player/Position", Player.Position.X.ToString() + "," + Player.Position.Y.ToString());
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