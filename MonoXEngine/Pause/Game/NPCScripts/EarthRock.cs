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
                    new MessageBox("", "!", obj.Position + new Vector2(0, -64)),
                    new MessageBox("Ziggy", "Hi.|.|.", obj.Position + new Vector2(0, -64)),
                    new MessageBox("Ziggy", "I didn't think you'd survived.|.|.\nYou fell into that tree almost 3 days ago...", obj.Position + new Vector2(0, -64)),
                    new MessageBox("Ziggy", "I'm not sure what this means but I feel implelled\nto tell you that if you want to interact with\nsomething you can press UP!", obj.Position + new Vector2(0, -64)),
                    new MessageBox("Ziggy", "Why don't you try it on me!", obj.Position + new Vector2(16, -32))
                }, true, () =>
                {

                }
             );
        }

        public static void Talk(Entity obj)
        {
            GameMethods.ShowMessages(new List<MessageBox>() {
                    new MessageBox("Ziggy", "I can't believe you figured it out!", obj.Position + new Vector2(0, -84)),
                    new MessageBox("Ziggy", ".|.|. Well done!", obj.Position + new Vector2(0, -84))
                }, true, () =>
                {
                    GameGlobal.Player.GetComponent<PlayerController>().MovementEnabled = false;
                    Entity temp = Entity.Find("EarthRock");

                    CameraController.Instance.Shake(3);
                    Global.AudioController.Play("SFX/EarthRockRumble");

                    StaticCoroutines.CoroutineHelper.RunOverX(3, 64, t => {
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
