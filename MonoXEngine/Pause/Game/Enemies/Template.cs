using MonoXEngine;
using MonoXEngine.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XEditor;

namespace MyGame.Enemies
{
    public class Template : Enemy
    {
        #region Constructor
        public Template(EntityInfo entityInfo) : base(entityInfo)
        {
            // Starting HP
            HP = 5;

            // Get data
            ZInterpreter data = new ZInterpreter(entityInfo.Data);

            // Build entity
            Entity = new Entity(entity => {
                entity.Position = entityInfo.Position;

                entity.AddComponent(new Sprite()).Run<Sprite>(sprite => {
                    sprite.LoadTexture("Entities/Enemies/Template");
                });

                entity.AddComponent(new MainCollider()).Run<MainCollider>(collider => {

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
