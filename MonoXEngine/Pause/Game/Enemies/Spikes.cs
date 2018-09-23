using Microsoft.Xna.Framework;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using MugHeadXEngine.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XEditor;

namespace MyGame.Enemies
{
    public class Spikes : Enemy
    {
        #region Properties
        Sprite Sprite;
        string Direction = "none";
        int Damage = 4;
        #endregion

        #region Constructor
        public Spikes(EntityInfo entityInfo) : base(entityInfo)
        {
            // Starting HP
            HP = 1000;

            // Get data
            ZInterpreter data = new ZInterpreter(entityInfo.Data);

            if (data.HasKey("direction"))
                Direction = data.GetString("direction");

            if (data.HasKey("damage"))
                Damage = data.GetInt("damage");

            // Build entity
            Entity = new Entity(entity => {
                entity.Name = "Enemy";
                entity.LayerName = "Main";
                entity.SortingLayer = 6;
                entity.Position = (entityInfo.Position * 16);
                entity.Origin = new Vector2(0f, 0f);
                entity.Collider = Entity.CollisionType.Box;
                entity.BoxColliderRect = new Rectangle(0, 2, 10, 2);
                entity.Trigger = Entity.TriggerTypes.NonSolid;
                entity.Data.Add("damage", Damage.ToString());

                entity.AddComponent(new Sprite()).Run<Sprite>(sprite => {
                    Sprite = sprite;
                    sprite.LoadTexture("Entities/Enemies/Spikes");
                });
            });
        }
        #endregion

        #region Methods
        public override void Update()
        {
            base.Update();
        }
        #endregion
    }
}
