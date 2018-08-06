using Microsoft.Xna.Framework;
using MonoXEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
    public class CameraController
    {
        public enum Modes
        {
            SnapToTarget,
            LerpToTarget
        }

        public Modes Mode;

        public Entity Target{set{TargetX = value;TargetY = value;}}
        public Entity TargetX;
        public Entity TargetY;

        public float? MaxX = null;
        public float? MinX = null;
        public float? MaxY = null;
        public float? MinY = null;

        public float Easing;
        public float MaxStep = 1000;
        public float MaxDistance;

        public static CameraController Instance = null;

        public CameraController()
        {
            Mode = Modes.SnapToTarget;
            Easing = 0.03f;
            MaxDistance = 64;

            if (Instance == null)
                Instance = this;
        }

        public void ResetMinMax()
        {
            MinY = null;
            MaxY = null;
            MinX = null;
            MaxX = null;
        }

        public void SnapOnce(Entity _target = null)
        {
            if (_target == null)
            {
                Global.Camera.Position = new Vector2(TargetX.Position.X, TargetY.Position.Y);
            }
            else
            {
                Global.Camera.Position = _target.Position;
            }
        }

        public void Update()
        {
            if (TargetX == null || TargetY == null)
                return;

            Vector2 camPos = Global.Camera.Position;

            if (Mode == Modes.LerpToTarget)
            {
                Vector2 targetXY = new Vector2(TargetX.Position.X, TargetY.Position.Y);

                // Min, Max X, Y
                if (MinY != null && (targetXY.Y - Global.ScreenBounds.Height / 2) < MinY)
                    targetXY.Y = (float)MinY + (Global.ScreenBounds.Height / 2);
                if (MaxY != null && (targetXY.Y + Global.ScreenBounds.Height / 2) > MaxY)
                    targetXY.Y = (float)MaxY - (Global.ScreenBounds.Height / 2);
                if (MinX != null && (targetXY.X - Global.ScreenBounds.Width / 2) < MinX)
                    targetXY.X = (float)MinX + (Global.ScreenBounds.Width / 2);
                if (MaxX != null && (targetXY.X + Global.ScreenBounds.Width / 2) > MaxX)
                    targetXY.X = (float)MaxX - (Global.ScreenBounds.Width / 2);

                Vector2 xyDist = new Vector2(targetXY.X - camPos.X, targetXY.Y - camPos.Y);
                double distance = Math.Sqrt(xyDist.X * xyDist.X + xyDist.Y * xyDist.Y);

                if (distance > 1)
                    camPos += new Vector2(
                        Math.Min(xyDist.X * Easing, MaxStep) * MonoXEngineGame.Instance.DeltaTimeMultiplier,
                        Math.Min(xyDist.Y * Easing, MaxStep) * MonoXEngineGame.Instance.DeltaTimeMultiplier
                        );
            }
            else if(Mode == Modes.SnapToTarget)
            {
                camPos = new Vector2(TargetX.Position.X, TargetY.Position.Y);
            }

            Global.Camera.Position = camPos;
        }
    }
}
