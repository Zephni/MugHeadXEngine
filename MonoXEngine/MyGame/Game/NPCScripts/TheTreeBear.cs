using Microsoft.Xna.Framework;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using MugHeadXEngine;
using MugHeadXEngine.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
    public class TheTreeBear
    {
        public static void First(Entity obj)
        {
            if (GameData.Get("TheTreeBear/First") == "True")
            {
                obj.Destroy();
                return;
            }

            GameData.Set("TheTreeBear/First", "True");
            GameMethods.ShowMessages(new List<MessageBox>() {
                    new MessageBox("Bradley Bear", "!", obj.Position + new Vector2(16, -32)),
                    new MessageBox("Bradley Bear", "Hi :) .|.|.", obj.Position + new Vector2(16, -32)),
                    new MessageBox("Bradley Bear", "I didn't think you'd survived.|.|.\nYou fell into that tree almost 3 days ago..", obj.Position + new Vector2(16, -32)),
                    new MessageBox("Bradley Bear", "This probably won't mean much to you\nbut if you ever want to interact with something you can\nalways press UP!", obj.Position + new Vector2(8, -32)),
                    new MessageBox("Bradley Bear", "Why don't you try it!", obj.Position + new Vector2(16, -32))
                }, true, () =>
                {

                }
             );
        }

        public static void Talk(Entity obj)
        {
            GameMethods.ShowMessages(new List<MessageBox>() {
                    new MessageBox("Bradley Bear", "I can't believe you figured it out!", obj.Position + new Vector2(32, -32)),
                    new MessageBox("Bradley Bear", ".|.|. Well done!", obj.Position + new Vector2(32, -32))
                }, true, () =>
                {
                    GameGlobal.Player.GetComponent<PlayerController>().MovementEnabled = false;
                    Entity temp = Entity.Find("TheTreeBear");
                    temp.GetComponent<Sprite>().SpriteEffect = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
                    
                    StaticCoroutines.CoroutineHelper.RunUntil(() => { return temp.Position.X >= 119 *16 + 8; }, () => {
                        temp.Position += new Vector2(1f, 0);
                    }, () => {
                        StaticCoroutines.CoroutineHelper.WaitRun(0.5f, () => {
                            temp.GetComponent<Sprite>().SpriteEffect = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;

                            StaticCoroutines.CoroutineHelper.WaitRun(1f, () => {
                                StaticCoroutines.CoroutineHelper.RunUntil(() => { return temp.Opacity <= 0; }, () => {
                                    temp.Opacity -= 0.1f;
                                }, () => {
                                    temp.Destroy();
                                    GameGlobal.Player.GetComponent<PlayerController>().MovementEnabled = true;
                                });
                            });
                        });
                    });
                }
             );
        }

        public static void CheckExistance(Entity obj)
        {
            if (GameData.Get("TheTreeBear/First") == "True")
            {
                Entity.Find("TheTreeBear").Destroy();
                obj.Destroy();
                return;
            }
        }
    }
}
