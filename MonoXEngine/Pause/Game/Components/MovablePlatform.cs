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
    public class MovablePlatform : EntityComponent
    {
        public Vector2 BasePosition;
        public float Speed = 1;
        public float XDistance;
        public float YDistance;

        public Vector2 Position
        {
            get {return Entity.Position;}
            set { Entity.Position = value; }
        }

        public MovablePlatform(Vector2 basePosition) : base()
        {
            BasePosition = basePosition;
        }

        public override void Start()
        {
            Entity.Name = "MovablePlatform";
            Entity.Trigger = Entity.TriggerTypes.Solid;
            Entity.Collider = Entity.CollisionType.Box;
            Entity.BoxColliderRect = new Rectangle(new Point(0, 0), Entity.BoundingBox.Size);
        }

        public override void Update()
        {
            Position = new Vector2(
                BasePosition.X + (int)(Math.Sin(GameGlobal.TimeLoop * Speed) * XDistance),
                BasePosition.Y + (int)(Math.Cos(GameGlobal.TimeLoop * Speed) * YDistance)
            );
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //base.Draw(spriteBatch);
        }
    }
}
