using System;
using StaticCoroutines;

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

            CurrentScene = scene;
            CurrentScene.Initialise();
        }

        public void LoadScene(string sceneName)
        {
            LoadScene(Activator.CreateInstance(Type.GetType(ScenesNamespace + "." + sceneName)) as Scene);
        }
    }
}
