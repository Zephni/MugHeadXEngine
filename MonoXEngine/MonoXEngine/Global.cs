using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

namespace MonoXEngine
{
    public static class Global
    {
        /// <summary>
        /// Game
        /// </summary>
        public static Game Game;

        /// <summary>
        /// MainSettings
        /// </summary>
        public static DataSet MainSettings;

        /// <summary>
        /// Content
        /// </summary>
        public static ContentManager Content
        {
            get { return Global.Game.Content; }
        }

        public static List<Entity> Entities = new List<Entity>();

        public static int CountEntities()
        {
            return Global.Entities.Count;
        }

        /// <summary>
        /// SceneManager
        /// </summary>
        public static SceneManager SceneManager;

        /// <summary>
        /// GameTime
        /// </summary>
        public static GameTime GameTime;

        /// <summary>
        /// Updated in game updates
        /// </summary>
        public static float DeltaTime;

        /// <summary>
        /// Resolution (Set in initialise)
        /// </summary>
        public static Point Resolution;

        /// <summary>
        /// ScreenBounds
        /// </summary>
        public static Rectangle ScreenBounds
        {
            get { return new Rectangle(new Point(-Global.Resolution.X / 2, -Global.Resolution.Y / 2), Global.Resolution); }
        }

        public static AudioController AudioController;

        /// <summary>
        /// GraphicsDevice
        /// </summary>
        public static GraphicsDevice GraphicsDevice
        {
            get { return MonoXEngineGame.Instance.GraphicsDevice; }
        }

        /// <summary>
        /// Cameras
        /// </summary>
        public static List<Camera> Cameras;

        /// <summary>
        /// Camera
        /// </summary>
        public static Camera Camera;

        /// <summary>
        /// SpriteBatchLayers
        /// </summary>
        public static Dictionary<string, SpriteBatchLayer> SpriteBatchLayers;

        public static InputManager InputManager;

        /// <summary>
        /// FPS
        /// </summary>
        public static int FPS;

        /// <summary>
        /// String to point array. Use "1,0|2,0|.."
        /// </summary>
        /// <param name="pointListStr"></param>
        /// <returns></returns>
        public static Point[] Str2Points(string pointListStr)
        {
            string[] points = pointListStr.Split('|');

            List<Point> pointList = new List<Point>();
            foreach (string point in points)
            {
                string[] pointXY = point.Trim().Split(',');
                pointList.Add(new Point(Convert.ToInt16(pointXY[0]), Convert.ToInt16(pointXY[1])));
            }

            return pointList.ToArray();
        }

        /// <summary>
        /// String to point array. Use "0,1,2,3,4,5", "0"
        /// </summary>
        /// <param name="pointListStr"></param>
        /// <returns></returns>
        public static Point[] Str2Points(string xPointList = "0", string yPointList = "0")
        {
            string[] xPoints = xPointList.Split(',');
            string[] yPoints = yPointList.Split(',');

            List<Point> pointList = new List<Point>();
            int largestArr = Math.Max(xPoints.Length, yPoints.Length);
            int x = 0, y = 0;
            for (int I = 0; I < largestArr; I++ )
            {
                if(I < xPoints.Length-1) x = Convert.ToInt16(xPoints[I]);
                if(I < yPoints.Length-1) y = Convert.ToInt16(yPoints[I]);

                pointList.Add(new Point(x, y));
            }

            return pointList.ToArray();
        }

        public static Dictionary<string, int[]> XIfTracker = new Dictionary<string, int[]>();
        public static bool XIf(string alias, int x, bool condition)
        {
            if(!XIfTracker.ContainsKey(alias))
                XIfTracker.Add(alias, new int[2] { 0, x });

            if (XIfTracker[alias][0] >= XIfTracker[alias][1] || condition == false)
                return false;

            XIfTracker[alias][0] += 1;
            return true;
        }

        public static bool RunOnce(string alias, bool condition)
        {
            return XIf(alias, 1, condition);
        }

        public static bool RunWhenEventLoops(string alias, bool condition)
        {
            if(!condition)
                XIfTracker.Remove(alias);

            return (XIf(alias, 1, condition));
        }

        public static T[,] Make2DArray<T>(T[] input, int height, int width)
        {
            T[,] output = new T[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    output[i, j] = input[i * width + j];
                }
            }
            return output;
        }

        public static T[] Make1DArray<T>(T[,] input)
        {
            int width = input.GetLength(0);

            T[] output = new T[width * input.GetLength(1)];

            for (int x = 0; x < input.GetLength(0); x++)
                for (int y = 0; y < input.GetLength(1); y++)
                    output[y * width + x] = input[x, y];

            return output;
        }
    }
}
