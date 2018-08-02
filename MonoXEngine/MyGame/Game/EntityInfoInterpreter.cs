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
                LevelScene.Player.Position = new Vector2(entityInfo.Position.X * 16, entityInfo.Position.Y * 16);
            }
            else if(entityInfo.Name == "CameraLock")
            {
                new Entity(entity => {
                    entity.Trigger = true;
                    entity.Name = "CameraLock";
                    entity.LayerName = "Main";
                    entity.Data.Add("Type", entityInfo.Data);
                    entity.SortingLayer = LevelScene.Player.SortingLayer;
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
