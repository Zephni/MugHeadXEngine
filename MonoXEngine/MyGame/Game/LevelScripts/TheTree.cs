using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using MugHeadXEngine;
using MugHeadXEngine.EntityComponents;
using MyGame.Scenes;
using StaticCoroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
    public partial class LevelScripts
    {
        public void TheTree()
        {
            // Backgrounds
            new Entity(entity => {
                entity.LayerName = "Background";
                entity.Position = new Vector2(0, 0);
                entity.AddComponent(new CameraOffsetTexture() { Texture2D = Global.Content.Load<Texture2D>("Backgrounds/NeutralSky"), Coefficient = new Vector2(0f, 0.1f), Offset = new Vector2(0, -80) });
            });
            new Entity(entity => {
                entity.LayerName = "Background";
                entity.Position = new Vector2(0, 0);
                entity.AddComponent(new CameraOffsetTexture() { Texture2D = Global.Content.Load<Texture2D>("Backgrounds/ForestBG_Rocks"), Coefficient = new Vector2(0.04f, 0.4f), Offset = new Vector2(-80, -220) });
            });

            if (GameData.Get("Levels/TheTree/Intro") == "True")
            {
                return;
            }

            // Music
            Global.AudioController.PlayMusic("Music/Overworld1");

            // Initial fade
            GameGlobal.Fader.RunFunction("Cancel");
            GameGlobal.Fader.RunFunction("BlackOut");
            GameGlobal.Fader.Data["Time"] = "5";
            CoroutineHelper.WaitRun(2, () => {
                GameGlobal.Fader.RunFunction("Resume");
                GameGlobal.Fader.RunFunction("FadeIn", e => {
                    e.Data["Time"] = "0.5";
                });
            });

            GameGlobal.Player.GetComponent<PlayerController>().MovementEnabled = false;
            GameGlobal.Player.GetComponent<PlayerController>().Kinetic = true;
            GameGlobal.PlayerGraphic.Visible = false;

            // Camera pan down
            Entity camePos = new Entity(e => { e.Position = GameGlobal.Player.Position + new Vector2(0, -400); });
            CameraController.Instance.Easing = 0.03f;
            CameraController.Instance.MaxStep = 1f;
            CameraController.Instance.Target = camePos;
            CameraController.Instance.SnapOnce();

            CoroutineHelper.WaitRun(9, () => {
                camePos.Position = GameGlobal.Player.Position + new Vector2(0, -40);

                CoroutineHelper.WaitRun(5, () => {
                    GameMethods.ShowMessages(new List<MessageBox>() {
                        new MessageBox(".|.|.|| !", camePos.Position),
                        new MessageBox("Huh|.|.|.||| I don't remember\nsleeping there!?", camePos.Position)
                    }, null, () => {
                        CoroutineHelper.WaitRun(3, () => {
                            GameGlobal.PlayerGraphic.Visible = true;
                            GameGlobal.PlayerGraphic.RunAnimation("JumpLeft");
                            GameGlobal.Player.Position = camePos.Position + new Vector2(0, -20);

                            MessageBox WoahMSG = new MessageBox("woah!", camePos.Position, MessageBox.Type.ManualDestroy);
                            WoahMSG.Build();

                            float temp = GameGlobal.Player.Position.Y;
                            CoroutineHelper.RunFor(0.7f, p => {
                                if (GameGlobal.Player.Position.Y < camePos.Position.Y + 50)
                                    GameGlobal.Player.Position.Y += 1f * (p * 4.15f);

                                if (GameGlobal.Player.Position.Y > WoahMSG.Position.Y + 32)
                                    WoahMSG.Position = GameGlobal.Player.Position + new Vector2(-WoahMSG.Container.Size.X / 2, -32);
                            }, () => {
                                GameGlobal.Player.Position.Y = camePos.Position.Y + 50;
                                GameGlobal.PlayerGraphic.RunAnimation("LayLeft");
                                Global.AudioController.Play("SFX/Thump");
                                WoahMSG.Destroy();

                                Vector2 sPos = Global.Camera.Position;
                                CoroutineHelper.RunFor(0.7f, p => {
                                    Random r = new Random();
                                    Global.Camera.Rotation = (float)MathHelper.Lerp(-0.04f, 0.04f, (float)r.NextDouble()) * (1 - p);
                                    Global.Camera.Position = new Vector2(sPos.X + (r.Next(0, 2) - 2) * (1 - p), sPos.Y + (r.Next(0, 2) - 1) * (1 - p));
                                }, () => {
                                    Global.Camera.Rotation = 0;
                                    Global.Camera.Position = sPos;
                                });

                                CoroutineHelper.WaitRun(2, () => {
                                    GameMethods.ShowMessages(new List<MessageBox>() {
                                        new MessageBox("OUCH!", GameGlobal.Player.Position + new Vector2(0, -32)),
                                    }, null, () => {
                                        
                                        GameGlobal.Player.GetComponent<PlayerController>().MovementEnabled = true;
                                        GameGlobal.Player.GetComponent<PlayerController>().Kinetic = false;
                                        CameraController.Instance.Easing = 0.1f;
                                        CameraController.Instance.MaxStep = 1000;
                                        CameraController.Instance.Target = GameGlobal.Player;
                                        camePos.Destroy();

                                        GameGlobal.PlayerGraphic.RunAnimation("StandRight");

                                        GameData.Set("Levels/TheTree/Intro", "True");
                                    });
                                });
                            });
                        });
                    });
                });
            });
        }
    }
}
