﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using MugHeadXEngine;
using MugHeadXEngine.EntityComponents;
using MyGame.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XEditor;
using static MyGame.RenderBlender;

namespace MyGame
{
    public class SavePoint
    {
        public static bool Saving = false;

        Entity Entity;
        PixelCollider Collider;

        public SavePoint(EntityInfo entityInfo)
        {
            Entity = new Entity(entity => {
                entity.Name = "SavePoint";
                entity.Position = entityInfo.Position * 16;
                entity.LayerName = "Main";
                entity.Trigger = true;
                //entity.SortingLayer = GameGlobal.PlayerGraphicEntity.SortingLayer-1;
                entity.Origin = Vector2.Zero;
                entity.AddComponent(new Sprite()).Run<Sprite>(sprite => {
                    sprite.LoadTexture("Entities/SavePoint");
                });

                entity.AddComponent(new PixelCollider()).Run<PixelCollider>(collider => { Collider = collider;});

                StaticCoroutines.CoroutineHelper.Always(() => {
                    Collider.Colliding(new Point(0, 0));
                });
            });

            Entity.CollidingWithTrigger = obj => {
                if (Saving)
                    return;

                if (obj.Name == "Player" && Global.InputManager.Pressed(InputManager.Input.Up))
                    Save(Entity.Position + new Vector2(0, -32));
            };

            Level.RenderBlender.DrawableTextures.AddRange(new List<DrawableTexture>(){
                new RenderBlender.DrawableTexture()
                {
                    Blend = RenderBlender.Lighting,
                    Texture = Global.Content.Load<Texture2D>("Graphics/Effects/alphamask"),
                    Position = Entity.Position + new Vector2(16, -16),
                    Color = Color.DeepSkyBlue * 0.6f,
                    Update = item => {
                        item.Scale = 0.6f + 0.04f * (float)Math.Sin(Global.GameTime.TotalGameTime.TotalMilliseconds / 300);
                    }
                },
                new RenderBlender.DrawableTexture()
                {
                    Blend = RenderBlender.Subtract,
                    Texture = Global.Content.Load<Texture2D>("Graphics/Effects/alphamask"),
                    Position = Entity.Position + new Vector2(16, -16),
                    Color = Color.White * 0.8f,
                    Update = item =>
                    {
                        item.Scale = 0.8f + 0.05f * (float)Math.Sin(Global.GameTime.TotalGameTime.TotalMilliseconds / 400);
                    }
                }
            });
        }

        public void Save(Vector2 pos)
        {
            Saving = true;
            GameGlobal.Player.GetComponent<PlayerController>().MovementEnabled = false;

            if(GameData.Get("SavePoint/First") == null)
            {
                GameData.Set("SavePoint/First", "1");

                GameMethods.ShowMessages(new List<MessageBox> {
                    new MessageBox("", "Something is written here.|.|.", pos),
                    new MessageBox("It say's", "'If you touch this grave stone your entire life up to\nthis point will be recorded.\n\nThe Holy Gravestone'|", pos),
                    new MessageBox("Holy Gravestone", "Why don't they just call it a save point?", pos)
                }, null, () => {
                    Save(pos);
                });
            }
            else
            {
                MessageBox messageBox = new MessageBox(null, "Would you like to save?", pos);
                messageBox.Build();

                GameMethods.ShowOptionSelector(pos + new Vector2(-16, 16), new List<Option>() {
                    new Option("yes", "Yes", new Vector2(0, 0)),
                    new Option("no", "No", new Vector2(0, 16))
                }, option => {
                    if(option == "yes")
                    {
                        GameData.Save();
                    }

                    messageBox.Destroy();
                    GameGlobal.Player.GetComponent<PlayerController>().MovementEnabled = true;
                    Saving = false;
                });
            }
        }
    }
}
