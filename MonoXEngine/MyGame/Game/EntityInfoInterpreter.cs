using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using MugHeadXEngine;
using MyGame.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XEditor;

namespace MyGame
{
    public class EntityInfoInterpreter
    {
        Level LevelScene;

        public EntityInfoInterpreter(List<EntityInfo> entities)
        {
            LevelScene = Global.SceneManager.CurrentScene as Level;

            foreach (var entity in entities)
                Interpret(entity);
        }

        public void Interpret(EntityInfo entityInfo)
        {
            if (entityInfo.Name == "StartPosition" && GameData.Get("Player/Position") == null)
            {
                GameGlobal.Player.Position = new Vector2(entityInfo.Position.X * 16, entityInfo.Position.Y * 16);
            }
            else if (entityInfo.Name == "Water")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);

                new Entity(entity => {
                    entity.Name = "Water";
                    entity.LayerName = "Main";
                    entity.Trigger = true;
                    entity.Collider = Entity.CollisionType.Pixel;
                    entity.SortingLayer = GameGlobal.Player.SortingLayer+1;
                    entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    entity.Opacity = 0.75f;
                    entity.AddComponent(new Drawable()).Run<Drawable>(d => {
                        d.BuildRectangle(new Point(Convert.ToInt16(entityInfo.Size.X) * 16, Convert.ToInt16(entityInfo.Size.Y) * 16), Color.CornflowerBlue);
                    });
                });
            }
            else if (entityInfo.Name == "Background")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);
                
                new Entity(entity => {
                    entity.LayerName = "Background";
                    entity.Position = new Vector2(0, 0);
                    float[] coefficient = data.GetFloatArr("coefficient");

                    entity.AddComponent(new CameraOffsetTexture() { Texture2D = Global.Content.Load<Texture2D>("Backgrounds/"+data.GetString("image")), Coefficient = new Vector2(coefficient[0], coefficient[1]), Offset = new Vector2(0, 0) });
                });
            }
            else if (entityInfo.Name == "AutoDoor")
            {
                new Entity(entity => {
                    entity.Name = "AutoDoor";
                    entity.LayerName = "Main";
                    entity.Collider = Entity.CollisionType.Pixel;
                    entity.Trigger = true;

                    ZInterpreter data = new ZInterpreter(entityInfo.Data);
                    entity.Data.Add("Level", data.GetString("level"));
                    entity.Data.Add("Position", data.GetString("position"));

                    entity.SortingLayer = GameGlobal.Player.SortingLayer;
                    entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    entity.AddComponent(new Drawable()).Run<Drawable>(d => {
                        d.BuildRectangle(new Point(16, 16), Color.Blue);
                        d.Visible = false;
                    });
                });
            }
            else if (entityInfo.Name == "Door")
            {
                new Entity(entity => {
                    entity.Name = "Door";
                    entity.LayerName = "Main";
                    entity.Collider = Entity.CollisionType.Pixel;
                    entity.Trigger = true;

                    ZInterpreter data = new ZInterpreter(entityInfo.Data);
                    entity.Data.Add("Level", data.GetString("level"));
                    entity.Data.Add("Position", data.GetString("position"));

                    entity.SortingLayer = GameGlobal.Player.SortingLayer;
                    entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    entity.AddComponent(new Drawable()).Run<Drawable>(d => {
                        d.BuildRectangle(new Point(16, 16), Color.Blue);
                        d.Visible = false;
                    });
                });
            }
            else if (entityInfo.Name == "Graphic")
            {
                new Entity(entity => {
                    entity.Name = "Graphic";
                    entity.LayerName = "Main";
                    entity.Origin = Vector2.Zero;
                    entity.SortingLayer = 3;

                    ZInterpreter data = new ZInterpreter(entityInfo.Data);

                    entity.AddComponent(new Drawable()).Run<Drawable>(d => {
                        d.LoadTexture("Graphics/" + data.GetString("image"));
                        entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    });
                });
            }
            else if (entityInfo.Name == "Animation")
            {
                new Entity(entity => {
                    entity.Name = "Animation";
                    entity.LayerName = "Main";
                    entity.Origin = Vector2.Zero;
                    entity.SortingLayer = 3;

                    ZInterpreter data = new ZInterpreter(entityInfo.Data);

                    entity.AddComponent(new Sprite()).Run<Sprite>(d => {
                        d.LoadTexture("Graphics/" + data.GetString("image"));
                        d.AddAnimation(new Animation("Default", data.GetFloat("delay"), new Point(32, 32), data.GetPointArr("frames")));
                        d.RunAnimation("Default");
                        entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    });
                });
            }
            else if(entityInfo.Name == "CameraLock")
            {
                new Entity(entity => {
                    entity.Trigger = true;
                    entity.Name = "CameraLock";
                    entity.LayerName = "Main";
                    entity.Collider = Entity.CollisionType.Pixel;
                    entity.Data.Add("Type", entityInfo.Data);
                    entity.SortingLayer = GameGlobal.Player.SortingLayer;
                    entity.Opacity = 0;
                    entity.AddComponent(new Drawable()).Run<Drawable>(d => {
                        d.BuildRectangle(new Point(entityInfo.Size.X * 16, entityInfo.Size.Y * 16), Color.Red);
                        entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    });
                });
            }
        }
    }
}
