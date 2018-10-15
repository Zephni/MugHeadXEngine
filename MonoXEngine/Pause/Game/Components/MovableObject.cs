using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
    public class MoveableObject : EntityComponent
    {
        public float Heavyness = 1f;
        public Collider Collider;

        public bool IsGrounded
        {
            get
            {
                return Collider.CheckOffset(new Offset(0, (int)Entity.Size.Y +1, null, 2));
            }
        }

        public override void Start()
        {
            Entity.Name = "MovableObject";
            Entity.AddComponent(new Collider()).Run<Collider>(c => { Collider = c; c.TriggerType = Collider.TriggerTypes.Solid; c.ColliderType = Collider.ColliderTypes.Box; });
        }

        public override void Update()
        {
            if (!Collider.CheckOffset(new Offset(0, null, null, 1)))
                Entity.Position.Y += 2;

            while (Collider.CheckOffset(new Offset(0, (int)Entity.Size.Y+1, null, 1))) Entity.Position.Y--;

            if (Entity.Data.ContainsKey("PO_ID"))
                GameData.Set("PO_ID:" + Entity.Data["PO_ID"] + "/Position", Entity.Position.X.ToString() + "," + Entity.Position.Y.ToString());
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}