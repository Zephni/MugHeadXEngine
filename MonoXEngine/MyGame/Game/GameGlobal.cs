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
        public static Entity Fader;

        public static void InitialiseAssets()
        {
            // Fader
            Fader = new Entity(entity => {
                entity.LayerName = "Fade";
                entity.AddComponent(new Drawable()).Run<Drawable>(component => {
                    component.BuildRectangle(new Point(Global.ScreenBounds.Width, Global.ScreenBounds.Height), Color.Black);
                });

                entity.AddFunction("FadeIn", (e, c) => {
                    CoroutineHelper.RunFor(0.5f, pcnt => { e.Opacity = 1 - pcnt; }, () => {
                        c?.Invoke(e);
                    });
                });

                entity.AddFunction("FadeOut", (e, c) => {
                    CoroutineHelper.RunFor(0.5f, pcnt => { e.Opacity = pcnt; }, () => {
                        c?.Invoke(e);
                    });
                });
            });
        }
    }
}
