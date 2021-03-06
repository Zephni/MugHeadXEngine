﻿using Microsoft.Xna.Framework;
using MonoXEngine;
using StaticCoroutines;
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

        public Vector2 Offset = Vector2.Zero;

        public float? MaxX = null;
        public float? MinX = null;
        public float? MaxY = null;
        public float? MinY = null;

        public float Easing;
        public float MaxStep = 1000;
        public float MaxDistance;
        public float SpeedMultiplier = 1;
        public bool InstantExact = false;

        public static CameraController Instance = null;

        public CameraController()
        {
            Mode = Modes.LerpToTarget;
            SetDefault();

            Instance = this;
        }

        //private float PrevEasing;
        //private float PrevMaxStep;

        public void SetDefault()
        {
            Easing = 0.08f;
            MaxDistance = 1000;
        }

        public void ResetMinMax()
        {
            MinY = null;
            MaxY = null;
            MinX = null;
            MaxX = null;
        }

        public void SnapOnce()
        {
            CoroutineHelper.RunUntil(0.8f, () => {
                InstantExact = true;
            }, () => {
                InstantExact = false;
            });
        }

        public void Update()
        {
            if (Global.InputManager.Held(InputManager.Input.L1))
                return;
            if (TargetX == null || TargetY == null)
                return;

            Vector2 camPos = Global.Camera.Position;

            if (Mode == Modes.LerpToTarget)
            {
                Vector2 targetXY = new Vector2(TargetX.Position.X + Offset.X, TargetY.Position.Y + Offset.Y);

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

                if(!InstantExact)
                {
                    camPos += new Vector2(
                    Math.Min(xyDist.X * Easing, MaxStep) * MonoXEngineGame.Instance.DeltaTimeMultiplier * SpeedMultiplier,
                    Math.Min(xyDist.Y * Easing, MaxStep) * MonoXEngineGame.Instance.DeltaTimeMultiplier * SpeedMultiplier
                    );
                }
                else
                {
                    camPos += xyDist;
                }
            }
            else if(Mode == Modes.SnapToTarget)
            {
                camPos = new Vector2(TargetX.Position.X, TargetY.Position.Y);
            }

            Global.Camera.Position = camPos;
        }

        public void Shake(float Time, Action action = null)
        {
            Vector2 sPos = Global.Camera.Position;
            CoroutineHelper.RunFor(Time, p => {
                Random r = new Random();
                Global.Camera.Rotation = (float)MathHelper.Lerp(-0.04f, 0.04f, (float)r.NextDouble()) * (1 - p);
                Global.Camera.Position = new Vector2(sPos.X + (r.Next(0, 2) - 2) * (1 - p), sPos.Y + (r.Next(0, 2) - 1) * (1 - p));
            }, () => {
                Global.Camera.Rotation = 0;
                Global.Camera.Position = sPos;

                action?.Invoke();
            });
        }
    }
}
