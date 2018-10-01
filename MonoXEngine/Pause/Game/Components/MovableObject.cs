﻿using Microsoft.Xna.Framework;
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
        public float Heavyness = 0.5f;
        public MainCollider MainCollider;

        public override void Start()
        {
            Entity.Name = "MovableObject";
            Entity.Trigger = Entity.TriggerTypes.Solid;
            Entity.Collider = Entity.CollisionType.Pixel;
            Entity.BoxColliderRect = new Rectangle(new Point(0, 0), Entity.BoundingBox.Size);
            Entity.AddComponent(new MainCollider()).Run<MainCollider>(mc => { MainCollider = mc; });
        }

        public override void Update()
        {
            if (!MainCollider.Colliding(new Rectangle((int)Entity.Size.Y, 0, 0, 1)))
                Entity.Position.Y += 1;

            while (MainCollider.Colliding(new Rectangle((int)Entity.Size.X + 1, 0, 1, (int)Entity.Size.Y - 1))) Entity.Position.X -= (1 - Heavyness);
            while (MainCollider.Colliding(new Rectangle(-1, 0, 1, (int)Entity.Size.Y - 1))) Entity.Position.X += (1 - Heavyness);
            while (MainCollider.Colliding(new Rectangle(0, (int)Entity.Size.Y-1, 0, 1))) Entity.Position.Y--;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}