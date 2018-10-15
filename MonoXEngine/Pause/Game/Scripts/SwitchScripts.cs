using Microsoft.Xna.Framework;
using MonoXEngine;
using StaticCoroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
    public static class SwitchScripts
    {
        public static void MysticCave_HiddenPassage(Entity obj)
        {
            float value = obj.Data["value"].ToFloat();

            obj.Data["enabled"] = "false";

            CameraController.Instance.Shake(1.5f);
            Entity mcw = Entity.Find("MysticCaveWall");
            Vector2 origPosition = mcw.Position;
            CoroutineHelper.RunFor(1.5f, t => {
                mcw.Position = origPosition + new Vector2(0, (value == 1) ? -36 * t : 36 * t);
            }, () => {
                obj.Data["enabled"] = "true";
            });
        }
    }
}
