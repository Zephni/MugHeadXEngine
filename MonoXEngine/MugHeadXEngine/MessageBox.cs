﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MugHeadXEngine
{
    public class MessageBox
    {
        public static int defaultSortingLayer = 5;

        private Point Padding = new Point(6, 2);
        private Entity Container;
        private Entity TextObject;
        private Entity ArrowFlash;
        private Text TextString;
        private string TestString;

        private string PassedText;
        private Vector2 PassedPosition;

        public MessageBox(string text, Vector2 position)
        {
            PassedText = text;
            PassedPosition = new Vector2().Copy(position);
        }

        public void Build(Action action = null)
        {
            // Build and position text
            TextObject = new Entity(entity => {
                entity.LayerName = "Main";
                entity.SortingLayer = MessageBox.defaultSortingLayer + 1;
                entity.Position = PassedPosition;

                entity.AddComponent(new Text()).Run<Text>(component => {
                    component.String = PassedText.Replace("|", "");
                    TextString = component;

                    // Final positioning
                    entity.Position = entity.BoundingBox.Location.ToVector2();
                });
            });

            // Build container based on text
            Container = new Entity(entity => {
                entity.LayerName = "Main";
                entity.SortingLayer = MessageBox.defaultSortingLayer;
                entity.Origin = Vector2.Zero;
                entity.Position = new Vector2(TextObject.Position.X - Padding.X, (TextObject.Position.Y - Padding.Y) + 1);
                entity.AddComponent(new Drawable() {
                    Texture2D = MugHeadXEngine.Engine.RoundedRect(Global.Content.Load<Texture2D>("Defaults/9Patch_8"),
                    new Point(TextObject.BoundingBox.Width + (Padding.X * 2) + 8, TextObject.BoundingBox.Height + Padding.Y * 2))
                });
            });

            // Build arrow flasher
            ArrowFlash = new Entity(entity => {
                entity.LayerName = "Main";
                entity.SortingLayer = MessageBox.defaultSortingLayer + 1;
                entity.Position = new Vector2(TextObject.Position.X + TextObject.BoundingBox.Width + 6, TextObject.BoundingBox.Bottom +2);
                entity.AddComponent(new Sprite() {Texture2D = Global.Content.Load<Texture2D>("Defaults/ArrowFlash")}).Run<Sprite>(sprite => {
                    sprite.AddAnimation(new Animation("Flashing", 0.5f, new Point(8, 8), new Point(0, 0), new Point(1, 0)));
                    sprite.RunAnimation("Flashing");
                    sprite.Visible = false;
                });
            });

            // Write text from scratch
            TextString.String = "";
            TestString = "";
            ProgressText(action);
        }

        private void ProgressText(Action action = null)
        {
            TestString = PassedText.Substring(0, TestString.Length + 1);
            TextString.String = TestString.Replace("|", "");

            if (TestString != PassedText)
            {
                float stepTime = 0.02f;
                if (TestString.Length > 0 && TestString[TestString.Length-1] == '|')
                    stepTime = 0.3f;

                StaticCoroutines.CoroutineHelper.WaitRun(stepTime, () => {
                    ProgressText(action);
                });
            }
            else
            {
                ArrowFlash.GetComponent<Sprite>().Visible = true;

                StaticCoroutines.CoroutineHelper.RunWhen(() => /*Close MSG*/ Keyboard.GetState().IsKeyDown(Keys.Z), () => {
                    Destroy();
                    action?.Invoke();
                });
            }
        }

        public void Destroy()
        {
            TextObject.Destroy();
            Container.Destroy();
            ArrowFlash.Destroy();
        }
    }
}
