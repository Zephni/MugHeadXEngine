using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using MugHeadXEngine.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
    public partial class LevelScripts
    {
        public void MysticCave()
        {
            // Input icons
            Entity pushableBox = Global.Entities.Find(p => p.Name == "MovableObject");

            StaticCoroutines.CoroutineHelper.RunWhen(() => { return GameGlobal.Player.Position.GetDistance(pushableBox.Position) < Global.Resolution.X /2; }, () => {
                if (GameData.Get("Tips/Action2ToPush") == null)
                {
                    // Action 2
                    Texture2D texture2D = GameMethods.GetInputIcon(InputManager.Input.Action2, Global.InputManager.CurrentInputType);
                    GameMethods.DisplayInputIcon(texture2D, pushableBox.Position + new Vector2(0, -18), () => { return GameGlobal.Player.GetComponent<PlayerController>().Pushing != 0; }, () => {
                        GameData.Set("Tips/Action2ToPush", "1");
                    });

                    // Right
                    texture2D = GameMethods.GetInputIcon(InputManager.Input.Right, Global.InputManager.CurrentInputType);
                    GameMethods.DisplayInputIcon(texture2D, pushableBox.Position + new Vector2(-18, 0), () => { return GameGlobal.Player.GetComponent<PlayerController>().Pushing != 0; });

                    // Left
                    texture2D = GameMethods.GetInputIcon(InputManager.Input.Left, Global.InputManager.CurrentInputType);
                    GameMethods.DisplayInputIcon(texture2D, pushableBox.Position + new Vector2(18, 0), () => { return GameGlobal.Player.GetComponent<PlayerController>().Pushing != 0; });
                }
            });
        }
    }
}
