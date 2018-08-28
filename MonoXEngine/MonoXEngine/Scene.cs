using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MonoXEngine
{
    public class Scene
    {
        public bool Active = false;
        public Action OnExit = null;

        public Scene()
        {
            //this.Initialise();
        }

        public virtual void Initialise() { }

        public virtual void Update() { }

        public void Destroy()
        {
            if (OnExit != null)
                OnExit.Invoke();

            Global.Entities.Clear();
        }
    }
}