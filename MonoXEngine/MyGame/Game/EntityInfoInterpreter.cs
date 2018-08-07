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
            else if (entityInfo.Name == "Door")
            {
                new Entity(entity => {
                    entity.Name = "Door";
                    entity.LayerName = "Main";
                    entity.Trigger = true;
                    string[] data = entityInfo.Data.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    entity.Data.Add("Level", data[0]);
                    entity.Data.Add("Position", data[1]);
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
                    //entity.SortingLayer = entityInfo.; (Need Z pos)
                    entity.AddComponent(new Drawable()).Run<Drawable>(d => {
                        d.LoadTexture("Graphics/"+entityInfo.Data);
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
