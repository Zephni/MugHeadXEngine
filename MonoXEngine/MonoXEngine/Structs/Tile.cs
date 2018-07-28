using Microsoft.Xna.Framework;
using MonoXEngine.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoXEngine.Structs
{
    public struct Tile
    {
        public Point SourcePosition;
        public Point3D Position3D;
        public Point Position;

        public Tile(Point sourcePosition, Point3D position)
        {
            this.SourcePosition = sourcePosition;
            this.Position3D = position;
            this.Position = new Point(position.X, position.Y);
        }
    }
}
