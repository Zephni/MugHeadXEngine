using Microsoft.Xna.Framework;
using MonoXEngine;
using MugHeadXEngine.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
    public class Debug
    {
        public static void End(Entity obj)
        {
            GameMethods.ShowMessages(new List<MessageBox>() {
                    new MessageBox("", "Oh, this is as far as you go...|", obj.Position + new Vector2(0, -32)),
                    new MessageBox("", "An adventure awaits you on the\nother side of this cave!|", obj.Position + new Vector2(0, -32))
                }, true, () =>
                {
                    GameGlobal.Player.GetComponent<PlayerController>().MovementEnabled = false;
                    StaticCoroutines.CoroutineHelper.RunUntil(() => { return Global.AudioController.MasterVolume <= 0; }, () => {
                        Global.AudioController.MasterVolume -= 0.15f * Global.DeltaTime;
                    }, () => {
                        MonoXEngineGame.Instance.Exit();
                    });
                    GameGlobal.Fader.Data["Time"] = "4";
                    GameGlobal.Fader.RunFunction("FadeOut", entity => {
                        entity.RunFunction("SetDefault");
                    });
                }
             );
        }
    }
}
