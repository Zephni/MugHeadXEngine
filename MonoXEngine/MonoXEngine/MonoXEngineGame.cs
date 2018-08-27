using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using System.Linq;
using System.Xml.XPath;
using StaticCoroutines;

namespace MonoXEngine
{
    public class MonoXEngineGame : Game
    {
        /// <summary>
        /// Static instance
        /// </summary>
        public static MonoXEngineGame Instance;

        /// <summary>
        /// GraphicsDeviceManager must be defined in construct
        /// </summary>
        private GraphicsDeviceManager Graphics;

        /// <summary>
        /// ViewportTexture
        /// </summary>
        public ViewportTexture ViewportTexture;

        /// <summary>
        /// MonoXEngine constructor
        /// </summary>
        /// <param name="MainSettingsFile"></param>
        public MonoXEngineGame(string MainSettingsFile)
        {
            // Static instance
            MonoXEngineGame.Instance = this;

            // Pass MainSettings
            Global.MainSettings = new DataSet();
            Global.MainSettings.FromXML(XDocument.Load(@MainSettingsFile));

            // Set Global.Game
            Global.Game = this;

            // GraphicsDeviceManager
            this.Graphics = new GraphicsDeviceManager(this);

            // Content RootDirectory
            Content.RootDirectory = Global.MainSettings.Get<string>(new string[] { "Directories", "Content" });

            // Window resizing
            if (Global.MainSettings.Get<string>("Viewport", "AllowResizing").ToLower() == "true")
            {
                Window.AllowUserResizing = true;
                Window.ClientSizeChanged += delegate {
                    this.ViewportTexture.WindowSizeUpdate();
                };
            }

            // Full screen
            if (Global.MainSettings.Get<string>("Viewport", "FullScreen").ToLower() == "true")
                Graphics.IsFullScreen = true;
        }

        protected override void Initialize()
        {
            Global.Cameras = new List<Camera>(){new Camera()};
            Global.Camera = Global.Cameras[0];
            Global.SpriteBatchLayers = new Dictionary<string, SpriteBatchLayer>();
            Global.SceneManager = new SceneManager(Global.MainSettings.Get<string>(new string[] { "Namespaces", "Scenes" }));
            Global.AudioController = new AudioController(Global.MainSettings.Get<string>(new string[] { "Directories", "Audio" }), Content);
            Global.Resolution = new Point(
                Global.MainSettings.Get<int>(new string[] { "Viewport", "ResolutionX" }),
                Global.MainSettings.Get<int>(new string[] { "Viewport", "ResolutionY" })
            );

            this.ViewportTexture = new ViewportTexture(Global.Resolution, Global.MainSettings.Get<string>(new string[] { "Viewport", "ViewportArea" }));

            Global.InputManager = new InputManager(InputManager.InputType.Keyboard);

            //this.IsFixedTimeStep = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            foreach (KeyValuePair<string, object> x in Global.MainSettings.NewNarrowedDataSet("Layers").Data)
            {
                string[] parts = x.Key.Split('/');
                SpriteBatchLayer spriteBatchLayer = new SpriteBatchLayer(Global.MainSettings.GetChildren("Layers/" + parts[0]));
                Global.SpriteBatchLayers.Add(parts[0], spriteBatchLayer);
            }

            Global.SceneManager.LoadScene(Global.MainSettings.Get<string>(new string[] { "Initiation", "StartupScene" }));

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        public float DeltaTimeMultiplier = 1;
        protected override void Update(GameTime gameTime)
        {
            Global.GameTime = gameTime;
            Global.DeltaTime = (float)Global.GameTime.ElapsedGameTime.TotalSeconds * DeltaTimeMultiplier;

            Global.InputManager.Update();

            Global.AudioController.Update();
            Global.SceneManager.CurrentScene.Update();

            for (int I = 0; I < Global.Entities.Count; I++)
                Global.Entities[I].Update();

            Coroutines.Update(Global.DeltaTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Global.FPS = ((int)(1 / Global.GameTime.ElapsedGameTime.TotalSeconds));
            GraphicsDevice.Clear(Color.Black);

            this.ViewportTexture.CaptureAndRender(this, () => {
                GraphicsDevice.Clear(Color.White);
                foreach (KeyValuePair<string, SpriteBatchLayer> SpriteBatchLayer in Global.SpriteBatchLayers)
                {
                    List<Entity> Entities = Global.Entities.FindAll(e => e.LayerName == SpriteBatchLayer.Key);
                    Entities.Sort((v1, v2) => { return v1.SortingLayer - v2.SortingLayer; });
                    SpriteBatchLayer.Value.Draw(Entities);
                }
            });

            base.Draw(gameTime);
        }
    }
}
