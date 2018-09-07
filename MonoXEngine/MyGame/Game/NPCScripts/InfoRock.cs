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
    public class EarthRock
    {
        public static void First(Entity obj)
        {
            if (GameData.Get("EarthRock/First") != null)
            {
                obj.Destroy();
                return;
            }

            GameData.Set("EarthRock/First", "1");
            GameMethods.ShowMessages(new List<MessageBox>() {
                    new MessageBox("", "!", obj.Position + new Vector2(16, -32)),
                    new MessageBox("Earth Rock", "Hi :) .|.|.", obj.Position + new Vector2(16, -32)),
                    new MessageBox("Earth Rock", "I didn't think you'd survived.|.|.\nYou fell into that tree almost 3 days ago..", obj.Position + new Vector2(16, -32)),
                    new MessageBox("Earth Rock", "I'm not sure what this means but I feel implelled to tell you that\nif you want to interact with something you can press UP!", obj.Position + new Vector2(8, -32)),
                    new MessageBox("Earth Rock", "Why don't you try it!", obj.Position + new Vector2(16, -32))
                }, true, () =>
                {

                }
             );
        }

        public static void Talk(Entity obj)
        {
            GameMethods.ShowMessages(new List<MessageBox>() {
                    new MessageBox("EarthRock", "I can't believe you figured it out!", obj.Position + new Vector2(32, -32)),
                    new MessageBox("EarthRock", ".|.|. Well done!", obj.Position + new Vector2(32, -32))
                }, true, () =>
                {
                    GameGlobal.Player.GetComponent<PlayerController>().MovementEnabled = false;
                    Entity temp = Entity.Find("EarthRock");

                    CameraController.Instance.Shake(2);

                    StaticCoroutines.CoroutineHelper.RunOverX(2, 32, t => {
                        temp.Position += new Vector2(0, 1f);
                    }, () => {
                        GameData.Set("EarthRock/First", "2");
                        temp.Destroy();
                        GameGlobal.Player.GetComponent<PlayerController>().MovementEnabled = true;
                    });
                }
             );
        }

        public static void CheckExistance(Entity obj)
        {
            if (GameData.Get("EarthRock/First") == "2")
            {
                Entity.Find("EarthRock").Destroy();
                obj.Destroy();
                return;
            }
        }
    }
}
