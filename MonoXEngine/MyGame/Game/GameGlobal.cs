using Microsoft.Xna.Framework;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using StaticCoroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
    public static class GameGlobal
    {
        /// <summary>
        /// Fader entity
        /// </summary>
        public static Entity Fader = new Entity(entity => {
            entity.LayerName = "Fade";
            entity.AddComponent(new Drawable()).Run<Drawable>(component => {
                component.BuildRectangle(new Point(Global.ScreenBounds.Width, Global.ScreenBounds.Height), Color.Black);
            });

            entity.AddFunction("FadeIn", e => {
                CoroutineHelper.RunFor(2, pcnt => { e.Opacity = 1 - pcnt; }, () => {

                });
            });

            entity.AddFunction("FadeOut", e => {
                CoroutineHelper.RunFor(2, pcnt => { e.Opacity = pcnt; }, () => {
                    
                });
            });
        });
    }
}
