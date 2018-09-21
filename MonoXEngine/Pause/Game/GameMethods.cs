using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using MugHeadXEngine;
using MugHeadXEngine.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
    public static class GameMethods
    {
        /*public static void PhysicsActive(bool active)
        {
            Global.Entities.FindAll(entity => entity.HasComponent<Physics>()).ForEach(entity => {
                entity.GetComponent<Physics>().Disabled = !active;
            });

            Global.Entities.FindAll(entity => entity.HasComponent<PlayerController>()).ForEach(entity => {
                entity.GetComponent<PlayerController>().Disabled = !active;
            });
        }*/

        public static T GetProperty<T>(string propertyName)
        {
            if (propertyName == null)
                return default(T);

            var prop = typeof(T).GetProperty(propertyName);
            return (prop != null) ? (T)prop.GetValue(null, null) : default(T);
        }

        public static void ShowOptionSelector(Vector2 position, List<Option> optionList, Action<string> action = null, Entity player = null, string texture9Patch = "Defaults/9Patch_8")
        {
            if(player != null)
                player.GetComponent<PlayerController>().MovementEnabled = false;

            OptionSelector os = new OptionSelector();
            os.Build(position, optionList, result => {
                action?.Invoke(result);

                if (player != null)
                    player.GetComponent<PlayerController>().MovementEnabled = true;
            }, texture9Patch);
        }

        public static void ShowMessages(List<MessageBox> Messages, bool? DisablePlayerMovement = null, Action action = null, bool overrideInteraction = false)
        {
            if (!overrideInteraction && GameGlobal.DisableInteraction)
                return;

            GameGlobal.DisableInteraction = true;
            if (Messages.Count > 0)
            {
                MessageBox CurrentMessage = Messages.First();
                Messages.Remove(CurrentMessage);

                if(DisablePlayerMovement != null)
                {
                    GameGlobal.Player.GetComponent<PlayerController>().MovementMode = (DisablePlayerMovement == true) ? PlayerController.MovementModes.None : PlayerController.MovementModes.Normal;
                }

                CurrentMessage.Build(() => {
                    ShowMessages(Messages, DisablePlayerMovement, action, true);
                });
            }
            else
            {
                GameGlobal.DisableInteraction = false;

                action?.Invoke();

                if (DisablePlayerMovement == true)
                    GameGlobal.Player.GetComponent<PlayerController>().MovementMode = PlayerController.MovementModes.Normal;
            }
        }

        private static Random random = new Random();
        public static float Random(float min, float max)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }

        public static void SmokePuffs(int count, Vector2 areaPos, Point areaSize, float speed = 10)
        {
            for(int i = 0; i < count; i++)
            {
                new Entity(entity => {
                    entity.LayerName = "Main";
                    entity.SortingLayer = 8;
                    entity.Opacity = 0.9f;
                    entity.Position = new Vector2(Random(areaPos.X - areaSize.X / 2, areaPos.X + areaSize.X), Random(areaPos.Y - areaSize.Y / 2, areaPos.Y + areaSize.Y));
                    entity.AddComponent(new Sprite()).Run<Sprite>(s => {
                        s.LoadTexture("Entities/SmokePuff");
                        s.AddAnimation(new Animation("Default", 0.1f, "16,16".ToPoint(), "0,0".ToPointList()));

                        s.RunAnimation("Default");
                        
                        float angleDegrees = Random(0, 359).ToInt();
                        float initialScale = Random(0.6f, 1);
                        float scaleRate = Random(1f, 3f);
                        float speedRnd = Random(speed - (speed / 2), speed + (speed / 2));
                        entity.Scale = new Vector2(initialScale, initialScale);
                        speed = speedRnd;

                        StaticCoroutines.CoroutineHelper.RunUntil(() => { return entity == null || entity.Scale.X <= 0.05f; }, () => {
                            entity.Scale -= new Vector2(scaleRate, scaleRate) * Global.DeltaTime;
                            float new_x = entity.Position.X + (speed * (float)Math.Cos(angleDegrees * Math.PI / 180) * Global.DeltaTime);
                            float new_y = entity.Position.Y + (speed * (float)Math.Sin(angleDegrees * Math.PI / 180) * Global.DeltaTime);
                            entity.Position = new Vector2(new_x, new_y);
                        }, () => {
                            if (entity != null)
                                entity.Destroy();
                        });
                    });
                });

                
            }
        }

        public static Texture2D RoundedRect(Texture2D texture9Patch, Point size)
        {
            int border = texture9Patch.Bounds.Width / 3;
            Color[,] colors9Patch = texture9Patch.To2DArray();
            Color[,] colors2D = new Color[size.X, size.Y];

            // Top left
            for (int x = 0; x < border; x++)
                for (int y = 0; y < border; y++)
                    colors2D[x, y] = colors9Patch[x, y];

            // Top
            for (int x = border; x < size.X - border; x++)
                for (int y = 0; y < border; y++)
                    colors2D[x, y] = colors9Patch[x.Wrap(border, border * 2), y];

            // Top right
            for (int x = size.X - border; x < size.X; x++)
                for (int y = 0; y < border; y++)
                    colors2D[x, y] = colors9Patch[(x - (size.X - border)) + (border * 2), y];

            // Middle left
            for (int x = 0; x < border; x++)
                for (int y = border; y < size.Y - border; y++)
                    colors2D[x, y] = colors9Patch[x, y.Wrap(border, border * 2)];

            // Middle
            for (int x = border; x < size.X - border; x++)
                for (int y = border; y < size.Y - border; y++)
                    colors2D[x, y] = colors9Patch[x.Wrap(border, border * 2), y.Wrap(border, border * 2)];

            // Middle right
            for (int x = size.X - border; x < size.X; x++)
                for (int y = border; y < size.Y - border; y++)
                    colors2D[x, y] = colors9Patch[(x - (size.X - border)) + (border * 2), y.Wrap(border, border * 2)];

            // Bottom left
            for (int x = 0; x < border; x++)
                for (int y = size.Y - border; y < size.Y; y++)
                    colors2D[x, y] = colors9Patch[x, (y - (size.Y - border)) + (border * 2)];

            // Bottom
            for (int x = border; x < size.X - border; x++)
                for (int y = size.Y - border; y < size.Y; y++)
                    colors2D[x, y] = colors9Patch[x.Wrap(border, border * 2), (y - (size.Y - border)) + (border * 2)];

            // Bottom right
            for (int x = size.X - border; x < size.X; x++)
                for (int y = size.Y - border; y < size.Y; y++)
                    colors2D[x, y] = colors9Patch[(x - (size.X - border)) + (border * 2), (y - (size.Y - border)) + (border * 2)];

            Texture2D texture2D = new Texture2D(Global.GraphicsDevice, size.X, size.Y);
            texture2D.From2DArray(colors2D);

            return texture2D;
        }
    }
}
