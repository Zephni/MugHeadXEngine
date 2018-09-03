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
        public static string DebugString = "";

        public static Entity Fader;
        public static Entity Player;
        public static Entity PlayerGraphicEntity;
        public static Sprite PlayerGraphic { get { return GameGlobal.PlayerGraphicEntity.GetComponent<Sprite>(); } }

        public static bool DisableInteraction = false;

        public static void InitialiseAssets()
        {
            // Fader
            Fader = new Entity(entity => {
                entity.LayerName = "Fade";

                entity.Data.Add("Time", "");
                entity.Data.Add("Cancel", "");

                entity.AddComponent(new Drawable()).Run<Drawable>(component => {
                    component.BuildRectangle(new Point(Global.ScreenBounds.Width, Global.ScreenBounds.Height), Color.Black);
                });

                entity.AddFunction("SetDefault", (e) => {
                    entity.Data["Time"] = "0.5";
                    entity.Data["Cancel"] = "false";
                });

                entity.RunFunction("SetDefault");

                entity.AddFunction("FadeIn", (e, c) => {
                    entity.RunFunction("BlackOut");
                    CoroutineHelper.RunFor((float)Convert.ToDecimal(entity.Data["Time"]), pcnt => { if(entity.Data["Cancel"] != "true") e.Opacity = 1 - pcnt; }, () => {
                        c?.Invoke(e);
                    });
                });

                entity.AddFunction("FadeOut", (e, c) => {
                    entity.RunFunction("InstantIn");
                    CoroutineHelper.RunFor((float)Convert.ToDecimal(entity.Data["Time"]), pcnt => { if (entity.Data["Cancel"] != "true") e.Opacity = pcnt; }, () => {
                        c?.Invoke(e);
                    });
                });

                entity.AddFunction("InstantIn", (e) => {
                    e.Opacity = 0;
                });

                entity.AddFunction("BlackOut", (e) => {
                    e.Opacity = 1;
                });

                entity.AddFunction("Cancel", (e) => {
                    entity.Data["Cancel"] = "true";
                });

                entity.AddFunction("Resume", (e) => {
                    entity.Data["Cancel"] = "false";
                });
            });
        }
    }
}
