using Microsoft.Xna.Framework;
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
                new RenderBlender.DrawableTexture("alphamask2"){
                    Blend = RenderBlender.Subtract,
                    Position = Entity.Position + new Vector2(8),
                    Color = Color.White,
                    Update = item => {item.Scale = 2f + 0.06f * (float)Math.Sin(GameGlobal.TimeLoop * 2);}
                }
            });

            Level.RenderBlender.DrawableTextures.AddRange(new List<DrawableTexture>(){
                new RenderBlender.DrawableTexture("alphamask"){
                    Position = Entity.Position + new Vector2(8),
                    Color = Color.DeepSkyBlue * 0.3f,
                    Update = item => {item.Scale = 1.2f + 0.2f * (float)Math.Sin(GameGlobal.TimeLoop * 2);}
                }
            });

            // Circle bits
            Vector2 origin = (entityInfo.Position * 16) + new Vector2(8);

            for(int I = 0; I < 7; I++)
            {
                float rand = GameMethods.Random(1, 2);
                if (rand >= 1.5f) rand = -rand;
                Level.RenderBlender.DrawableTextures.Add(new RenderBlender.DrawableTexture("alphamask2") {
                    Color = Color.AliceBlue * 0.6f,
                    Update = item => {
                        item.Scale = 0.02f * rand + 0.02f * (float)Math.Sin(GameGlobal.TimeLoop * rand);
                        item.Position.X = origin.X + (float)Math.Cos(MathHelper.WrapAngle(GameGlobal.TimeLoop * 0.4f * rand)) * 12 * rand;
                        item.Position.Y = origin.Y + (float)Math.Sin(GameGlobal.TimeLoop * 0.7f * rand) * 12 * rand;
                    }
                });
            }
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
                messageBox.Build(null, () => {
                    GameMethods.ShowOptionSelector(pos + new Vector2(-16, 16), new List<Option>() {
                        new Option("yes", "Yes", new Vector2(0, 0)),
                        new Option("no", "No", new Vector2(0, 16))
                    }, option => {
                        if (option == "yes")
                        {
                            GameData.Save();
                        }

                        messageBox.Destroy();
                        GameGlobal.Player.GetComponent<PlayerController>().MovementEnabled = true;
                        Saving = false;
                    });
                });
            }
        }
    }
}
