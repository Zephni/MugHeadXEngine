using Microsoft.Xna.Framework;
using MonoXEngine;
using MugHeadXEngine;
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
            GameGlobal.Fader.RunFunction("FadeIn");

            GameMethods.ShowOptionSelector(
                new Vector2(-94, -64),
                new List<Option>() {
                    new Option("newGame", "NEW GAME", new Vector2(0, 0)),
                    new Option("loadGame", "LOAD GAME", new Vector2(0, 16)),
                    new Option("loadGame", "OPTIONS", new Vector2(0, 32)),
                    new Option("quit", "QUIT", new Vector2(0, 48)),
                    new Option("newGame", "TEST 1", new Vector2(74, 0)),
                    new Option("loadGame", "TEST 2", new Vector2(74, 16)),
                    new Option("loadGame", "TEST 3", new Vector2(74, 32)),
                    new Option("quit", "TEST 4", new Vector2(74, 48)),
                    new Option("newGame", "TEST 1", new Vector2(74*2, 0)),
                    new Option("loadGame", "TEST 2", new Vector2(74*2, 16)),
                    new Option("loadGame", "TEST 3", new Vector2(74 * 2, 32)),
                    new Option("quit", "TEST 4", new Vector2(74 *2, 48)),
                },
                result => {
                    if(result == "newGame")
                    {
                        GameData.Reset();
                        Global.SceneManager.LoadScene("DebugScene");
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
