using Microsoft.Xna.Framework;
using MonoXEngine;
using MugHeadXEngine;
using StaticCoroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame.Scenes
{
    public class Menu : Scene
    {
        public override void Initialise()
        {
            GameGlobal.InitialiseAssets();
            GameGlobal.Fader.RunFunction("FadeIn");

            GameMethods.ShowOptionSelector(
                new Vector2(-94, -64),
                new List<Option>() {
                    new Option("newGame", "NEW GAME", new Vector2(0, 0)),
                    new Option("loadGame", "LOAD GAME", new Vector2(0, 16)),
                    new Option("options", "OPTIONS", new Vector2(0, 32)),
                    new Option("quit", "QUIT", new Vector2(0, 48)),
                },
                result => {
                    if (result == "newGame")
                    {
                        GameData.Reset();

                        GameGlobal.Fader.RunFunction("FadeOut");
                        CoroutineHelper.WaitRun(2f, () => {
                            Global.SceneManager.LoadScene("DebugScene");
                        });
                    }
                    else if (result == "loadGame")
                    {
                        GameData.Reset();
                        GameData.Load();

                        GameGlobal.Fader.RunFunction("FadeOut");
                        CoroutineHelper.WaitRun(2f, () => {
                            Global.SceneManager.LoadScene("DebugScene");
                        });
                    }
                    else if(result == "quit")
                    {
                        MonoXEngineGame.Instance.Exit();
                    }
                }
            );
        }

        public override void Update()
        {

        }
    }
}
