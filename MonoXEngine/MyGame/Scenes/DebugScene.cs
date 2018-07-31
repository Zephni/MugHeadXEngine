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

namespace MyGame.Scenes
{
    public class DebugScene : Scene
    {
        Entity player;

        public override void Initialise()
        {
            GameGlobal.InitialiseAssets();
            GameGlobal.Fader.RunFunction("FadeIn");

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
            player = new Entity(entity =>
            {
                entity.Position = new Vector2(128, 32 * 4);

                entity.AddComponent(new Sprite()).Run<Sprite>(component =>
                {
                    component.BuildRectangle(new Point(16, 16), Color.Green);
                });

                entity.AddComponent(new PlatformerController(new PixelCollider()));
            });

            // OptionSelector test
            if(GameData.Get("Flags/Init") == null)
            {
                CoroutineHelper.WaitRun(2, () => {
                    MessageBox mbQ = new MessageBox("Would you like to party?", player.Position + new Vector2(-50, -50), MessageBox.Type.ManualDestroy);
                    mbQ.Build(() => {
                        GameMethods.ShowOptionSelector(
                            player.Position,
                            new List<Option>() {
                        new Option("opt1", "YES", new Vector2(0, 0)),
                        new Option("opt2", "NO", new Vector2(0, 16)),
                        new Option("opt3", "ABSOLUTELY", new Vector2(32, 0)),
                        new Option("opt4", "MAYBE", new Vector2(32, 16)),
                            },
                            result => {
                                GameData.Set("Flags/Init", result);
                                mbQ.Destroy();
                                CoroutineHelper.WaitRun(2, () => {
                                    player.GetComponent<PlatformerController>().MovementEnabled = false;
                                    GameMethods.ShowMessages(
                                        new List<MugHeadXEngine.MessageBox>() {
                                new MugHeadXEngine.MessageBox("Your answer was.|.|.|| "+result+"!", player.Position + new Vector2(50, -100)),
                                new MugHeadXEngine.MessageBox("Who said that!?", player.Position + new Vector2(0, -42))
                                        },
                                        player
                                    );
                                });
                            },
                            player
                        );
                    });
                });
            }
            else
            {
                string[] pPosData = GameData.Get("Player/Position").Split(',');
                player.Position = new Vector2(Convert.ToInt16(pPosData[0]), Convert.ToInt16(pPosData[1]));
                player.GetComponent<PlatformerController>().MovementEnabled = false;
                new MessageBox("You do like to party! "+ GameData.Get("Flags/Init"), player.Position + new Vector2(-50, -50)).Build(() => {
                    player.GetComponent<PlatformerController>().MovementEnabled = true;
                });
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
                        component.String = "Entities: "+Global.CountEntities().ToString()+", Coroutines: "+Coroutines.Count.ToString()+", FPS: "+Global.FPS.ToString();
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
                GameGlobal.Fader.RunFunction("FadeOut");
                CoroutineHelper.WaitRun(2, () => {
                    Global.SceneManager.LoadScene("Menu");
                });
            }

            if (Global.InputManager.Pressed(InputManager.Input.L1))
            {
                GameData.Set("Player/Position", player.Position.X.ToString() + "," + player.Position.Y.ToString());
                GameData.Save();
            }

            if (Global.InputManager.Pressed(InputManager.Input.L2))
            {
                GameData.Load();
                Global.SceneManager.LoadScene("DebugScene");
            }
        }
    }
}