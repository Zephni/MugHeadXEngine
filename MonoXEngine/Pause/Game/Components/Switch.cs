using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XEditor;

namespace MyGame
{
    public class Switch : EntityComponent
    {
        Sprite Sprite;
        ZInterpreter Data;
        EntityInfo EntityInfo;

        public Switch(EntityInfo entityInfo)
        {
            Data = new ZInterpreter(entityInfo.Data);
            EntityInfo = entityInfo;
        }

        public override void Start()
        {
            Entity.Name = "Switch";
            Entity.LayerName = "Main";
            Entity.SortingLayer = GameGlobal.PlayerGraphicEntity.SortingLayer - 1;
            Entity.Position = (EntityInfo.Position * 16) + (EntityInfo.Size.ToVector2() / 2) * 16;
            Entity.Trigger = Entity.TriggerTypes.NonSolid;
            Entity.Collider = Entity.CollisionType.Pixel;

            if (Data.HasKey("id"))
                Entity.Data.Add("id", Data.GetString("id"));

            Entity.Data.Add("value", (Data.HasKey("value") ? Data.GetString("value") : "0"));

            Entity.AddComponent(new Sprite()).Run<Sprite>(sprite => {
                Sprite = sprite;
                sprite.LoadTexture("Entities/Switch1");
                sprite.AddAnimation(new Animation("Off", 0.3f, new Point(32, 32), "0,0".ToPointList()));
                sprite.AddAnimation(new Animation("On", 0.3f, new Point(32, 32), "0,1".ToPointList()));
            });

            string value = GameData.Get("Switch_ID:" + Entity.Data["id"] + "/Value");
            if (Entity.Data.ContainsKey("id") && value != null)
            {
                UpdateValue(value);
            }
            else
            {
                UpdateValue(Entity.Data["value"]);
            }
        }

        public void UpdateValue(string value)
        {
            Entity.Data["value"] = value;

            if(Entity.Data.ContainsKey("id"))
                GameData.Set("Switch_ID:" + Entity.Data["id"] + "/Value", value);

            if (Entity.Data["value"] == "0")
                Sprite.RunAnimation("Off");
            else if (Entity.Data["value"] == "1")
                Sprite.RunAnimation("On");
        }

        public override void Update()
        {
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
