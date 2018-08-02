using Microsoft.Xna.Framework;
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
            if(GameData.Get("Levels/TheTree/Intro") == "True")
            {
                return;
            }

            GameGlobal.Player.GetComponent<PlayerController>().MovementEnabled = false;
            GameGlobal.Player.GetComponent<PlayerController>().Kinetic = true;
            GameGlobal.Player.GetComponent<Sprite>().Visible = false;

            // Camera pan down
            Entity camePos = new Entity(e => { e.Position = GameGlobal.Player.Position + new Vector2(0, -200); });
            CameraController.Instance.Easing = 0.01f;
            CameraController.Instance.MaxStep = 1f;
            CameraController.Instance.Target = camePos;
            CameraController.Instance.SnapOnce();

            CoroutineHelper.WaitRun(2, () => {
                camePos.Position = GameGlobal.Player.Position + new Vector2(0, -40);

                CoroutineHelper.WaitRun(6, () => {
                    GameMethods.ShowMessages(new List<MessageBox>() {
                        new MessageBox(".|.|.|| !", camePos.Position),
                        new MessageBox("Huh|.|.|.||| I don't remember\nsleeping there!?", camePos.Position)
                    }, null, () => {
                        CoroutineHelper.WaitRun(3, () => {
                            GameGlobal.Player.GetComponent<Sprite>().Visible = true;
                            GameGlobal.Player.Position = camePos.Position + new Vector2(0, -20);

                            MessageBox WoahMSG = new MessageBox("woah!", camePos.Position, MessageBox.Type.ManualDestroy);
                            WoahMSG.Build();

                            CoroutineHelper.RunUntil(() => { return GameGlobal.Player.Position.Y > camePos.Position.Y + 50; }, () => {
                                GameGlobal.Player.Position.Y += 2.5f;
                                if (GameGlobal.Player.Position.Y > WoahMSG.Position.Y + 32)
                                    WoahMSG.Position = GameGlobal.Player.Position + new Vector2(-WoahMSG.Container.Size.X / 2, -32);
                            }, () => {
                                GameGlobal.Player.Position.Y = camePos.Position.Y + 48;
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
                                        CameraController.Instance.Easing = 0.2f;
                                        CameraController.Instance.MaxStep = 1000;
                                        CameraController.Instance.Target = GameGlobal.Player;
                                        camePos.Destroy();

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
