using System;
using StaticCoroutines;
using Microsoft.Xna.Framework;

namespace MonoXEngine
{
    public class SceneManager
    {
        private string ScenesNamespace;
        public Scene CurrentScene { get; set; }

        public SceneManager(string scenesNamespace)
        {
            ScenesNamespace = scenesNamespace;
        }

        public void LoadScene(Scene scene)
        {
            if(this.CurrentScene != null)
                this.CurrentScene.Destroy();

            Global.XIfTracker.Clear();
            Coroutines.routines.Clear();
            Global.Camera.Position = new Vector2(0, 0);

            CurrentScene = scene;
            CurrentScene.Initialise();
        }

        public void LoadScene(string sceneName)
        {
            LoadScene(Activator.CreateInstance(Type.GetType(ScenesNamespace + "." + sceneName)) as Scene);
        }
    }
}
