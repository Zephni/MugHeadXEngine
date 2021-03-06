﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StaticCoroutines;

namespace MonoXEngine.EntityComponents
{
    public class Sprite : Drawable
    {
        protected Dictionary<string, Animation> Animations;

        public bool Paused = false;
        public bool Looping = false;
        public int CurrentAnimationFrame;

        public string CurrentAnimation;

        public Sprite()
        {
            this.Animations = new Dictionary<string, Animation>();
        }

        public void AddAnimation(Animation animation)
        {
            this.Animations.Add(animation.Name, animation);
        }

        public Animation GetAnimation(string name)
        {
            return this.Animations[name];
        }

        private Action OnFinish;

        /// <summary>
        /// RunAnimation
        /// </summary>
        /// <param name="name">Name of an animation that has already been added to this sprite</param>
        public void RunAnimation(string name, bool loop = true, Action onFinish = null)
        {
            if (CurrentAnimation != name)
            {
                Looping = loop;

                OnFinish = onFinish;

                CurrentAnimation = name;
                this.SourceRectangle = this.GetAnimation(CurrentAnimation).SourceRectangles[0];
                CurrentAnimationFrame = 0;
                AnimTimer = 0;
            }
        }

        private float AnimTimer = 0;

        public override void Update()
        {
            if(CurrentAnimation != null && !Paused)
            {
                AnimTimer += Global.DeltaTime;

                if(AnimTimer >= this.GetAnimation(CurrentAnimation).FrameInterval)
                {
                    AnimTimer = 0;

                    if(CurrentAnimationFrame < this.GetAnimation(CurrentAnimation).SourceRectangles.Count)
                        CurrentAnimationFrame++;

                    if (Looping && CurrentAnimationFrame == this.GetAnimation(CurrentAnimation).SourceRectangles.Count)
                        CurrentAnimationFrame = 0;

                    if (OnFinish != null && !Looping && CurrentAnimationFrame == this.GetAnimation(CurrentAnimation).SourceRectangles.Count)
                        OnFinish.Invoke();

                    if (CurrentAnimationFrame >= this.GetAnimation(CurrentAnimation).SourceRectangles.Count)
                    {
                        CurrentAnimationFrame = this.GetAnimation(CurrentAnimation).SourceRectangles.Count-1;
                    }


                    this.SourceRectangle = this.GetAnimation(CurrentAnimation).SourceRectangles[CurrentAnimationFrame];
                }
            }
        }
    }
}
