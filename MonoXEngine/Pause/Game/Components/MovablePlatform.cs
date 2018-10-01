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
        public string Type = "standard";
        public float XDistance;
        public float YDistance;

        private float XMove = 0;
        private float YMove = 0;

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
            if (Type == "standard")
            {
                XMove += XDistance * Global.DeltaTime;
                YMove += YDistance * Global.DeltaTime;

                if (Math.Abs(XMove) >= Math.Abs(XDistance)) { XMove = XDistance; XDistance = XDistance * -1; }
                if (Math.Abs(YMove) >= Math.Abs(YDistance)) { YMove = YDistance; YDistance = YDistance * -1; }

                Position = new Vector2(
                    BasePosition.X + XMove,
                    BasePosition.Y + YMove
                );
            }
            else if(Type == "lerp")
            {
                Position = new Vector2(
                    BasePosition.X + (int)(Math.Sin(GameGlobal.TimeLoop * Speed) * XDistance),
                    BasePosition.Y + (int)(Math.Cos(GameGlobal.TimeLoop * Speed) * YDistance)
                );
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //base.Draw(spriteBatch);
        }
    }
}
