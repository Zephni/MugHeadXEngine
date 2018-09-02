using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MonoXEngine
{
    public struct Animation
    {
        /// <summary>
        /// Name of this animation
        /// </summary>
        public string Name;

        /// <summary>
        /// List of source rectangles
        /// </summary>
        public List<Rectangle> SourceRectangles;

        /// <summary>
        /// Time in seconds between frames
        /// </summary>
        public float FrameInterval;

        /// <summary>
        /// Animation construct
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sourceRectangles"></param>
        /// <param name="frameInterval"></param>
        public Animation(string name, float frameInterval, List<Rectangle> sourceRectangles)
        {
            this.Name = name;
            this.SourceRectangles = sourceRectangles;
            this.FrameInterval = frameInterval;
        }

        public Animation(string name, float frameInterval, Point frameSize, params Point[] frames)
        {
            this.Name = name;
            this.FrameInterval = frameInterval;
            this.SourceRectangles = new List<Rectangle>();
            foreach (Point point in frames)
                this.SourceRectangles.Add(new Rectangle(
                    point.X * frameSize.X,
                    point.Y * frameSize.Y,
                    frameSize.X,
                    frameSize.Y
                )
            );
        }

        public Animation(string name, float frameInterval, Point frameSize, List<Point> frames)
        {
            this.Name = name;
            this.FrameInterval = frameInterval;
            this.SourceRectangles = new List<Rectangle>();
            foreach (Point point in frames)
                this.SourceRectangles.Add(new Rectangle(
                    point.X * frameSize.X,
                    point.Y * frameSize.Y,
                    frameSize.X,
                    frameSize.Y
                )
            );
        }
    }
}