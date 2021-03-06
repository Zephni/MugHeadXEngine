﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
            SoundEffectInstance forestAmbienceSFX = Global.AudioController.Play("SFX/Forest_Ambience");
            forestAmbienceSFX.Volume = 0.3f;
            CoroutineHelper.Always(() => {
                if (forestAmbienceSFX.State == SoundState.Stopped)
                {
                    forestAmbienceSFX = Global.AudioController.Play("SFX/Forest_Ambience");
                    forestAmbienceSFX.Volume = 0.3f;
                }
            });

            Global.SceneManager.CurrentScene.OnExit += () => {
                Global.AudioController.Stop("SFX/Forest_Ambience");
            };
            

            if (GameData.Get("Levels/TheTree/Intro") == "True")
            {
                // Music
                Global.AudioController.PlayMusic("Overworld Happy");
                return;
            }

            MyGame.Scenes.Level.CameraController.Offset = new Vector2(16, 0);

            // Initial fade
            GameGlobal.Fader.RunFunction("BlackOut");
            GameGlobal.Fader.RunFunction("Cancel");
            GameGlobal.Fader.Data["Time"] = "5";
            CoroutineHelper.WaitRun(2, () => {
                GameGlobal.Fader.RunFunction("Resume");
                GameGlobal.Fader.RunFunction("FadeIn");
            });

            GameGlobal.PlayerController.MovementEnabled = false;
            GameGlobal.PlayerController.Kinetic = true;
            GameGlobal.PlayerGraphic.Visible = false;
            GameGlobal.PlayerController.MovementMode = PlayerController.MovementModes.None;

            // Camera pan down
            Entity camePos = new Entity(e => { e.Position = GameGlobal.Player.Position + new Vector2(0, -800); });
            CameraController.Instance.Easing = 0.03f;
            CameraController.Instance.MaxStep = 1f;
            CameraController.Instance.Target = camePos;
            CameraController.Instance.SnapOnce();

            // Seagulls
            CoroutineHelper.WaitRun(11, () => {
                CoroutineHelper.WaitRun(1, () => {
                    Global.AudioController.Play("SFX/BushRustle");
                });
                new Entity(entity => {
                    entity.LayerName = "Main";
                    entity.SortingLayer = 8;
                    entity.AddComponent(new Sprite() { Texture2D = Global.Content.Load<Texture2D>("Entities/Seagull") }).Run<Sprite>(s => {
                        s.AddAnimation(new Animation("Default", 0.1f, new Point(16, 16), new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(3, 0), new Point(3, 0), new Point(3, 0)));
                        s.RunAnimation("Default");
                    });

                    entity.Position = GameGlobal.Player.Position + new Vector2(0, -100);
                    entity.Scale = new Vector2(2.1f, 2.1f);

                    CoroutineHelper.RunUntil(() => { return entity.Position.Y < -500; }, () => {
                        entity.Position += new Vector2(0.3f, -0.2f * entity.Scale.Y);
                        entity.Scale += new Vector2(-0.003f, -0.003f);
                        if (entity.Scale.X < 0.5f)
                            entity.Scale = new Vector2(0.5f, 0.5f);
                    });
                });
                new Entity(entity => {
                    entity.LayerName = "Main";
                    entity.SortingLayer = 8;
                    entity.AddComponent(new Sprite() { Texture2D = Global.Content.Load<Texture2D>("Entities/Seagull") }).Run<Sprite>(s => {
                        s.AddAnimation(new Animation("Default", 0.1f, new Point(16, 16), new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(3, 0), new Point(3, 0), new Point(3, 0)));
                        s.RunAnimation("Default");
                    });

                    entity.Position = GameGlobal.Player.Position + new Vector2(-40, -100);
                    entity.Scale = new Vector2(2, 2);

                    CoroutineHelper.RunUntil(() => { return entity.Position.Y < -500; }, () => {
                        entity.Position += new Vector2(0.3f, -0.2f * entity.Scale.Y);
                        entity.Scale += new Vector2(-0.003f, -0.003f);
                        if (entity.Scale.X < 0.5f)
                            entity.Scale = new Vector2(0.5f, 0.5f);
                    });
                });
            });

            CoroutineHelper.WaitRun(7, () => {
                camePos.Position = GameGlobal.Player.Position + new Vector2(0, -40);

                CoroutineHelper.WaitRun(12, () => {

                    GameGlobal.Player.Position = camePos.Position + new Vector2(0, -20);
                    GameGlobal.PlayerGraphic.RunAnimation("Jump");

                    GameMethods.ShowMessages(new List<MessageBox>() {
                        new MessageBox("Pause", ".|.|.|| !", camePos.Position),
                        new MessageBox("Pause", "Huh|.|.|.||| I don't remember\nsleeping there!?", camePos.Position)
                    }, null, () => {
                        CoroutineHelper.WaitRun(3, () => {
                            GameGlobal.PlayerGraphic.Visible = true;

                            MessageBox WoahMSG = new MessageBox("Pause", "Woah!", camePos.Position, MessageBox.Type.ManualDestroy);
                            WoahMSG.Build();

                            float temp = GameGlobal.Player.Position.Y;
                            CoroutineHelper.RunFor(0.7f, p => {
                                if (GameGlobal.Player.Position.Y < camePos.Position.Y + 45)
                                    GameGlobal.Player.Position.Y += 1f * (p * 4.15f);

                                if (GameGlobal.Player.Position.Y > WoahMSG.Position.Y + 32)
                                    WoahMSG.Position = GameGlobal.Player.Position + new Vector2(-WoahMSG.Container.Size.X / 2, -32);
                            }, () => {
                                GameGlobal.Player.Position.Y = camePos.Position.Y + 45;
                                GameGlobal.PlayerGraphic.RunAnimation("Lay");
                                Global.AudioController.Play("SFX/Thump");
                                WoahMSG.Destroy();

                                GameMethods.SmokePuffs(8, GameGlobal.PlayerGraphicEntity.Position, new Point(8, 8));

                                CameraController.Instance.Shake(0.7f);

                                CoroutineHelper.WaitRun(2, () => {
                                    // Music
                                    Global.AudioController.PlayMusic("Overworld Happy", 1);

                                    GameMethods.ShowMessages(new List<MessageBox>() {
                                        new MessageBox("Pause", "OUCH!", GameGlobal.Player.Position + new Vector2(0, -32)),
                                    }, null, () => {

                                        GameGlobal.PlayerController.MovementEnabled = true;
                                        GameGlobal.PlayerController.MovementMode = PlayerController.MovementModes.Normal;
                                        GameGlobal.PlayerController.Kinetic = false;
                                        CameraController.Instance.Easing = 0.1f;
                                        CameraController.Instance.MaxStep = 1000;
                                        CameraController.Instance.Target = GameGlobal.Player;
                                        camePos.Destroy();

                                        GameGlobal.PlayerGraphic.RunAnimation("Stand");
                                        GameGlobal.PlayerController.Direction = 1;
                                        GameGlobal.PlayerController.IsGrounded = true;

                                        GameData.Set("Levels/TheTree/Intro", "True");

                                        GameGlobal.Fader.RunFunction("SetDefault");
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
