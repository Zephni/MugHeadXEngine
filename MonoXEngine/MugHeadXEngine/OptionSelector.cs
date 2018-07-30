using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using MugHeadXEngine.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MugHeadXEngine
{
    public class Option
    {
        public string ID;

        private string text = "";
        public string Text { get { return text; } set { text = value; TextEntity.GetComponent<Text>().String = text; } }

        public Vector2 Position { get { return TextEntity.Position; } set { TextEntity.Position = value; } }
        public Vector2 Size { get { return TextEntity.Size; } }

        private Entity TextEntity;

        /// <summary>
        /// An option that can only be used with the OptionSelector class
        /// </summary>
        /// <param name="ID">A short ID for the option, this will be returned when an option is selected</param>
        /// <param name="pText">The text to display for this option</param>
        /// <param name="pPosition">The position of the option relative to the OptionSelector container position</param>
        public Option(string id, string pText, Vector2 pPosition)
        {
            ID = id;
            text = pText;

            TextEntity = new Entity(entity => {
                entity.LayerName = "Main";
                entity.SortingLayer = MugHeadXEngine.Engine.OptionsSortingLayer + 1;
                entity.Position = new Vector2(pPosition.X, pPosition.Y);
                entity.AddComponent(new Text()).Run<Text>(_text => {
                    _text.String = text;
                });
            });
        }

        public void Destroy()
        {
            TextEntity.Destroy();
        }
    }

    public static class OptionSelector
    {
        public static Vector2 Position { get { return Container.Position; } set { Container.Position = value; } }
        public static Point Padding = new Point(8, 8);
        public static Point SelectorSize = new Point(12, 12);

        public static Entity Selector;
        public static bool Selecting = true;
        private static Entity Container;

        private static Option selectedOption;
        public static Option SelectedOption
        {
            get { return selectedOption; }
            set
            {
                selectedOption = value;
                Selector.Position = selectedOption.Position + new Vector2(-(SelectorSize.X / 2), (SelectorSize.Y / 2) + 1);
            }
        }

        public static void Build(Vector2 position, List<Option> optionList, Action<string> action = null, string texture9Patch = "Defaults/9Patch_8")
        {
            Selecting = true;

            Selector = new Entity(entity => {
                entity.LayerName = "Main";
                entity.SortingLayer = Engine.OptionsSortingLayer + 1;
                entity.AddComponent(new Sprite() { Texture2D = Global.Content.Load<Texture2D>("Defaults/OptionSelector") });
            });

            Container = new Entity(entity => {
                entity.Position = position;
                entity.LayerName = "Main";
                entity.Origin = Vector2.Zero;
                entity.SortingLayer = Engine.OptionsSortingLayer;

                float w = 0, h = 0;

                foreach (var option in optionList)
                {
                    float optionBoundsX = option.Position.X + option.Size.X;
                    if (optionBoundsX > w) w = optionBoundsX;

                    float optionBoundsY = option.Position.Y + option.Size.Y;
                    if (optionBoundsY > h) h = optionBoundsY;

                    option.Position = entity.Position + option.Position + (Padding.ToVector2() / 2) + new Vector2(SelectorSize.X, 0);
                }

                if (texture9Patch != null)
                {
                    entity.AddComponent(new Drawable() {
                        Texture2D = Engine.RoundedRect(Global.Content.Load<Texture2D>(texture9Patch),
                        new Point((int)w + (int)(Padding.X *1.5f) + (int)SelectorSize.X, (int)h + Padding.Y))
                    });
                }

                SelectedOption = optionList.First<Option>();

                StaticCoroutines.CoroutineHelper.RunUntil(() => { return !Selecting; }, () => {
                    if (Global.InputManager.Pressed(InputManager.Input.Up))
                        FindOption(optionList, "Up");
                    else if (Global.InputManager.Pressed(InputManager.Input.Down))
                        FindOption(optionList, "Down");
                    else if (Global.InputManager.Pressed(InputManager.Input.Left))
                        FindOption(optionList, "Left");
                    else if (Global.InputManager.Pressed(InputManager.Input.Right))
                        FindOption(optionList, "Right");
                    else if (Global.InputManager.Pressed(InputManager.Input.Action1))
                        Finished(optionList, action);
                });
            });
        }

        private static void Finished(List<Option> optionList, Action<string> action)
        {
            action?.Invoke(SelectedOption.ID);

            for (int I = 0; I < optionList.Count; I++)
                optionList[I].Destroy();

            Container.Destroy();
            Selector.Destroy();

            Selecting = false;
        }

        private static void FindOption(List<Option>optionList, string Direction)
        {
            Option newOption = SelectedOption;
            
            if (Direction == "Up")
            {
                List<Option> possibleOptions = optionList.Where<Option>(e => e.Position.Y < SelectedOption.Position.Y).OrderByDescending(e => e.Position.Y).ToList<Option>();
                List<Option> possibleOposites = optionList.Where<Option>(e => e.Position.Y >= SelectedOption.Position.Y).OrderBy(e => e.Position.Y).ToList<Option>();

                newOption = (possibleOptions.Count > 0)
                    ? possibleOptions.Aggregate((x, y) => Math.Abs(x.Position.X - SelectedOption.Position.X) < Math.Abs(y.Position.X - SelectedOption.Position.X) ? x : y)
                    : possibleOposites.Aggregate((x, y) => Math.Abs(x.Position.X - SelectedOption.Position.X) < Math.Abs(y.Position.X - SelectedOption.Position.X) ? x : y);
            }
            else if (Direction == "Down")
            {
                List<Option> possibleOptions = optionList.Where(e => e.Position.Y > SelectedOption.Position.Y).OrderBy(e => e.Position.Y).ToList<Option>();
                List<Option> possibleOposites = optionList.Where<Option>(e => e.Position.Y <= SelectedOption.Position.Y).OrderByDescending(e => e.Position.Y).ToList<Option>();

                newOption = (possibleOptions.Count > 0)
                    ? possibleOptions.Aggregate((x, y) => Math.Abs(x.Position.X - SelectedOption.Position.X) < Math.Abs(y.Position.X - SelectedOption.Position.X) ? x : y)
                    : possibleOposites.Aggregate((x, y) => Math.Abs(x.Position.X - SelectedOption.Position.X) < Math.Abs(y.Position.X - SelectedOption.Position.X) ? x : y);
            }
            else if (Direction == "Left")
            {
                List<Option> possibleOptions = optionList.Where<Option>(e => e.Position.X < SelectedOption.Position.X).OrderByDescending(e => e.Position.X).ToList<Option>();
                List<Option> possibleOposites = optionList.Where<Option>(e => e.Position.X >= SelectedOption.Position.X).OrderBy(e => e.Position.X).ToList<Option>();

                newOption = (possibleOptions.Count > 0)
                    ? possibleOptions.Aggregate((x, y) => Math.Abs(x.Position.Y - SelectedOption.Position.Y) < Math.Abs(y.Position.Y - SelectedOption.Position.Y) ? x : y)
                    : possibleOposites.Aggregate((x, y) => Math.Abs(x.Position.Y - SelectedOption.Position.Y) < Math.Abs(y.Position.Y - SelectedOption.Position.Y) ? x : y);
            }
            else if (Direction == "Right")
            {
                List<Option> possibleOptions = optionList.Where<Option>(e => e.Position.X > SelectedOption.Position.X).OrderBy(e => e.Position.X).ToList<Option>();
                List<Option> possibleOposites = optionList.Where<Option>(e => e.Position.X <= SelectedOption.Position.X).OrderByDescending(e => e.Position.X).ToList<Option>();

                newOption = (possibleOptions.Count > 0)
                    ? possibleOptions.Aggregate((x, y) => Math.Abs(x.Position.Y - SelectedOption.Position.Y) < Math.Abs(y.Position.Y - SelectedOption.Position.Y) ? x : y)
                    : possibleOposites.Aggregate((x, y) => Math.Abs(x.Position.Y - SelectedOption.Position.Y) < Math.Abs(y.Position.Y - SelectedOption.Position.Y) ? x : y);
            }

            SelectedOption = newOption;
        }
    }
}
