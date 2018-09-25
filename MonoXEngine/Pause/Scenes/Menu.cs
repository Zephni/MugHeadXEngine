using Microsoft.Xna.Framework;
using MonoXEngine;
using MonoXEngine.EntityComponents;
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
        public Dictionary<string, string> DebugData = new Dictionary<string, string>() {
            { "Level",                  "MysticCave" },
            { "Levels/TheTree/Intro",   "True"},
            { "Player/Position",    (15*16).ToString()+","+(38*16).ToString() }
        };

        Entity DebugText;

        public override void Initialise()
        {
            // Initialise and fade in
            GameGlobal.InitialiseAssets();
            GameGlobal.Fader.RunFunction("FadeIn");

            // DebugText
            DebugText = new Entity(entity => {
                entity.Position = new Vector2(-55, 20);

                entity.AddComponent(new Text()).Run<Text>(text => {
                    string temp = "";
                    foreach (var item in DebugData)
                        temp += item.Key + ": " + item.Value+"\n";

                    text.String = temp;

                    CoroutineHelper.Always(() => {
                        //text.Visible = (OptionSelector.SelectedOption.ID == "debug");
                    });
                });
            });

            MainMenu();
        }

        public void MainMenu()
        {
            // Menu options
            GameMethods.ShowOptionSelector(
                new Vector2(-66, -70),
                new List<Option>() {
                    new Option("newGame", "NEW GAME", new Vector2(0, 0)),
                    new Option("loadGame", "LOAD GAME", new Vector2(0, 16)),
                    new Option("options", "OPTIONS", new Vector2(0, 32)),
                    new Option("quit", "QUIT", new Vector2(0, 48)),

                    new Option("debug", "DEBUG", new Vector2(80, 0)),
                },
                result =>
                {
                    if (result == "newGame")
                    {
                        GameData.Reset();

                        // Initiate game data here
                        GameData.Set("Level", "TheTree");

                        GameGlobal.Fader.RunFunction("FadeOut", e =>
                        {
                            Global.SceneManager.LoadScene("Level");
                        });
                    }
                    else if (result == "loadGame")
                    {
                        GameData.Load();

                        GameGlobal.Fader.RunFunction("FadeOut", e =>
                        {
                            Global.SceneManager.LoadScene("Level");
                        });
                    }
                    else if (result == "quit")
                    {
                        MonoXEngineGame.Instance.Exit();
                    }
                    else if (result == "debug")
                    {
                        GameData.Reset();
                        foreach (var item in DebugData)
                            GameData.Set(item.Key, item.Value);

                        GameGlobal.Fader.RunFunction("FadeOut", e =>
                        {
                            Global.SceneManager.LoadScene("Level");
                        });
                    }
                    else if (result == "options")
                    {
                        Option fullscreenOption = new Option("fullscreen", "FULL SCREEN: " + ((MonoXEngineGame.Instance.Graphics.IsFullScreen) ? "YES" : "NO"), new Vector2(0, 0));
                        Option muteOption = new Option("mute", "MUTE: " + ((Global.AudioController.MasterVolume == 0) ? "YES" : "NO"), new Vector2(0, 16));

                        GameMethods.ShowOptionSelector(
                            new Vector2(-66, -70),
                            new List<Option>() {
                                fullscreenOption,
                                muteOption,
                                new Option("back", "BACK", new Vector2(0, 32))
                            },
                            optResult =>
                            {
                                if (optResult == "fullscreen")
                                {
                                    MonoXEngineGame.Instance.Graphics.ToggleFullScreen();
                                    fullscreenOption.Text = "FULL SCREEN: " + ((MonoXEngineGame.Instance.Graphics.IsFullScreen) ? "YES" : "NO");
                                }
                                if (optResult == "mute")
                                {
                                    Global.AudioController.MasterVolume = ((Global.AudioController.MasterVolume == 0) ? 1 : 0);
                                    muteOption.Text = "MUTE: " + ((Global.AudioController.MasterVolume == 0) ? "YES" : "NO");
                                }
                                else if (optResult == "back")
                                {
                                    GameMethods.CurrentOptionSelector.Close();
                                    MainMenu();
                                }
                            }
                            , false
                        );
                    }
                });
        }

        public override void Update()
        {

        }
    }
}
