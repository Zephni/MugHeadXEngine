using Microsoft.Xna.Framework;
using MonoXEngine;
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

        public void Interpret(EntityInfo entity)
        {
            if (entity.Name == "StartPosition" && GameData.Get("Player/Position") == null)
            {
                LevelScene.Player.Position = new Vector2(entity.Position.X * 16, entity.Position.Y * 16);
            }
        }
    }
}
