using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoXEngine.EntityComponents;
using System.Collections.Generic;
using MonoXEngine.Structs;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using StaticCoroutines;

namespace MyGame.Scenes
{
    public class DebugScene : Scene
    {
        Entity player;

        public override void Initialise()
        {
            Entity fader = new Entity(entity => {
                entity.LayerName = "Fade";
                entity.AddComponent(new Drawable()).Run<Drawable>(component => {
                    component.BuildRectangle(new Point(Global.ScreenBounds.Width, Global.ScreenBounds.Height), Color.Black);
                });

                entity.AddFunction("FadeIn", e => {
                    CoroutineHelper.RunFor(2, pcnt => { e.Opacity = 1 - pcnt; });
                });

                entity.AddFunction("FadeOut", e => {
                    CoroutineHelper.RunFor(2, pcnt => { e.Opacity = pcnt; }, () => {
                        Global.SceneManager.LoadScene("DebugScene");
                    });
                });
            });

            fader.RunFunction("FadeIn");

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
                entity.AddComponent(new CameraOffsetTexture(){
                    Texture2D = Global.Content.Load<Texture2D>("MountainsTest"),
                    Coefficient = new Vector2(0, 0)
                });
            });

            new Entity(entity => {
                entity.LayerName = "Background";
                entity.Position += new Vector2(0, 0);
                entity.AddComponent(new CameraOffsetTexture(){
                    Texture2D = Global.Content.Load<Texture2D>("BGTest"),
                    Coefficient = new Vector2(0, 0)
                });
            });

            new Entity(entity =>{
                entity.LayerName = "Background";
                entity.Position += new Vector2(0, 80);
                entity.AddComponent(new Drawable() { Texture2D = Global.Content.Load<Texture2D>("WaterTest") }).Run<Drawable>(component => {
                    Color[] colorArr = component.Texture2D.To1DArray();
                    CoroutineHelper.Always(() => {
                        component.Texture2D.ManipulateColors1D(colors => {
                            colors = (Color[])colorArr.Clone();
                            for (int y = 0; y < 100; y++)
                                colors.Shift(new Point(256, 100), new Rectangle(0, 1 * y, 256, 1), new Point(-(int)(Global.Camera.Position.X * (y+1) * 0.005f), 1 * y));
                            return colors;
                        });
                    });
                });
            });
            
            // Player
            player = new Entity(entity => {
                entity.Position = new Vector2(128, 32 * 4);

                entity.AddComponent(new Sprite()).Run<Sprite>(component => {
                    component.BuildRectangle(new Point(16, 16), Color.Green);
                });

                entity.AddComponent(new PlatformerController(new PixelCollider()));

                entity.CollidedWithTrigger += other => {
                    if (other.Name == "Collectable")
                    {
                        Global.AudioController.Play("SFX/Collect");
                        other.Destroy();

                        if(Global.Entities.FindAll(e => e.Name == "Collectable").Count == 0)
                            fader.RunFunction("FadeOut");
                    }
                };
            });

            // MessageBox test
            CoroutineHelper.WaitRun(2, () => {
                MugHeadXEngine.MessageBox mb1 = new MugHeadXEngine.MessageBox("Hello there.|.|.|| Madison!", player.Position + new Vector2(50, -100));

                CoroutineHelper.WaitRun(4, () => {
                    mb1.Destroy();

                    CoroutineHelper.WaitRun(1, () => {
                        MugHeadXEngine.MessageBox mb2 = new MugHeadXEngine.MessageBox("Who said that!?", player.Position + new Vector2(0, -42));

                        CoroutineHelper.WaitRun(2, () =>
                        {
                            mb2.Destroy();
                        });
                    });
                });
            });

            // Collectable prefab
            Entity collectable = new Entity(true, entity => {
                entity.Trigger = true;
                entity.Name = "Collectable";
                entity.AddComponent(new Drawable()).Run<Drawable>(component => {
                    component.BuildRectangle(new Point(8, 8), Color.Yellow);
                });
            });

            for(int X = 0; X < 5; X++)
            {
                Entity newCollectable = collectable.BuildPrefab();
                newCollectable.Position = new Vector2(25 + (X * 25), 32*4.72f);
            }

            // Build TileMap
            List<Tile> tempTiles = new List<Tile>();
            for (int X = 0; X < 12; X++) tempTiles.Add(new Tile(new Point(0, 0), new Point3D(X, 5, 0)));
            for (int X = 0; X < 12; X++) for (int Y = 1; Y < 4; Y++) tempTiles.Add(new Tile(new Point(0, 1), new Point3D(X, Y+5, 0)));
            
            TileMap tileMap = new TileMap(new Point(32, 32), "Tileset", tempTiles);
            tileMap.Build(new Point(30, 30));

            
            
            // Debug
            new Entity(entity => {
                entity.SortingLayer = 1;
                entity.LayerName = "Fade";
                entity.Position = -(Global.Resolution.ToVector2() / 2);
                entity.AddComponent(new Text()).Run<Text>(component => {
                    component.SetSpriteFont("HeartbitXX");
                    component.Color = Color.Yellow;

                    entity.UpdateAction = e => {
                        component.String = Global.FPS.ToString();
                    };
                });
            });
        }
        
        public override void Update()
        {
            Vector2 camPos = new Vector2(player.Position.X, 100);
            Global.Camera.Position = camPos;

            if(Global.RunOnce("Restart", Keyboard.GetState().IsKeyDown(Keys.Space)))
            {
                Global.SceneManager.LoadScene("DebugScene");
            }

            if (Global.RunWhenEventLoops("Shift", Keyboard.GetState().IsKeyDown(Keys.Z)))
            {
                player.Position.X += 32;
            }
        }
    }
}