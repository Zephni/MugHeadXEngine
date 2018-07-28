using System;
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

        private int CurrentAnimationFrame;

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

        /// <summary>
        /// RunAnimation
        /// </summary>
        /// <param name="name">Name of an animation that has already been added to this sprite</param>
        public void RunAnimation(string name)
        {
            Animation thisAnimation = this.GetAnimation(name);

            this.SourceRectangle = thisAnimation.SourceRectangles[0];

            CoroutineHelper.Every(thisAnimation.FrameInterval, () => {
                CurrentAnimationFrame++;

                if (CurrentAnimationFrame >= thisAnimation.SourceRectangles.Count)
                    CurrentAnimationFrame = 0;
                
                this.SourceRectangle = thisAnimation.SourceRectangles[CurrentAnimationFrame];
            });
        }
    }
}
