using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoXEngine
{
    public static class Extensions
    {
        #region String
        public static float ToFloat(this string value)
        {
            return (float)Convert.ToDouble(value);
        }

        public static float ToInt(this string value)
        {
            return Convert.ToInt16(value);
        }

        public static Point ToPoint(this string value)
        {
            var temp = value.Split(',');
            return new Point(Convert.ToInt16(temp[0]), Convert.ToInt16(temp[1]));
        }

        public static List<Point> ToPointList(this string value)
        {
            List<Point> pointList = new List<Point>();
            foreach(var item in value.Split(' '))
            {
                var temp = item.Split(',');
                pointList.Add(new Point(Convert.ToInt16(temp[0]), Convert.ToInt16(temp[1])));
            }

            return pointList;
        }
        #endregion

        #region Int
        public static int Wrap(this int value, int min, int max)
        {
            return (((value - min) % (max - min)) + (max - min)) % (max - min) + min;
        }

        public static int Between(this int value, int min, int max)
        {
            if (value < min) return min;
            else if (value > max) return max;
            else return value;
        }
        #endregion

        #region float
        public static int ToInt(this float value)
        {
            return Convert.ToInt16(value);
        }

        public static float Wrap(this float value, float min, float max)
        {
            return (((value - min) % (max - min)) + (max - min)) % (max - min) + min;
        }

        public static float Between(this float value, float min, float max)
        {
            if (value < min) return min;
            else if (value > max) return max;
            else return value;
        }
        #endregion

        #region Point
        public static float GetDistance(this Point value, Point other)
        {
            float distance = (float)Math.Sqrt(Math.Pow((other.X - value.X), 2) + Math.Pow((other.Y - value.Y), 2));
            return distance;
        }
        #endregion

        #region Vector2
        public static float GetDistance(this Vector2 value, Vector2 other)
        {
            return (float)Math.Sqrt(Math.Pow((other.X - value.X), 2) + Math.Pow((other.Y - value.Y), 2));
        }

        public static Vector2 Copy(this Vector2 value, Vector2 source)
        {
            return new Vector2(source.X, source.Y);
        }
        #endregion

        #region Color[]
        public static Color[] Copy1D(this Color[] origional, Rectangle selectArea)
        {
            Color[] copied = new Color[selectArea.Width * selectArea.Height];
            for (int x = 0; x < selectArea.Width; x++)
                for (int y = 0; y < selectArea.Height; y++)
                    copied[x + y * selectArea.Width] = origional[(x + selectArea.X) + (y + selectArea.Y) * selectArea.Width];

            return copied;
        }

        public static void Shift(this Color[] origional, Point bounds2D, Rectangle selectArea, Point newPosition)
        {
            Rectangle bounds = new Rectangle(0, 0, bounds2D.X, bounds2D.Y);
            Color[] copied = origional.Copy1D(selectArea);

            for (int x = 0; x < selectArea.Width; x++)
                for (int y = 0; y < selectArea.Height; y++)
                    origional[(x + newPosition.X).Wrap(bounds.X, bounds.Width) + (y + newPosition.Y).Wrap(bounds.Y, bounds.Height) * bounds.Width] = copied[x + y * bounds.Width];
        }
        #endregion

        #region Color[,]
        public static Color[,] Copy2D(this Color[,] origional, Rectangle selectArea)
        {
            Color[,] copied = new Color[selectArea.Width, selectArea.Height];
            for (int x = 0; x < selectArea.Width; x++)
                for (int y = 0; y < selectArea.Height; y++)
                    copied[x, y] = origional[x + selectArea.X, y + selectArea.Y];

            return copied;
        }

        public static void Shift(this Color[,] origional, Rectangle selectArea, Point newPosition)
        {
            Rectangle bounds = new Rectangle(0, 0, origional.GetLength(0), origional.GetLength(1));
            Color[,] copied = origional.Copy2D(selectArea);

            for (int x = 0; x < selectArea.Width; x++)
                for (int y = 0; y < selectArea.Height; y++)
                    origional[(x + newPosition.X).Wrap(bounds.X, bounds.Width), (y + newPosition.Y).Wrap(bounds.Y, bounds.Height)] = copied[x, y];
        }
        #endregion

        #region Texture2D
        public static void Clear(this Texture2D texture)
        {
            texture = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);
        }

        public static Vector2 Size(this Texture2D texture)
        {
            return new Vector2(texture.Width, texture.Height);
        }

        public static Color[] To1DArray(this Texture2D texture)
        {
            Color[] colorsOne = new Color[texture.Width * texture.Height]; //The hard to read,1D array
            texture.GetData(colorsOne); //Get the colors and add them to the array
            return colorsOne;
        }

        public static void From1DArray(this Texture2D texture, Color[] colors1D)
        {
            texture.SetData(colors1D);
        }

        public static void ManipulateColors1D(this Texture2D texture, Func<Color[], Color[]> action)
        {
            texture.From1DArray(action(texture.To1DArray()));
        }

        public static Color[,] To2DArray(this Texture2D texture)
        {
            Color[] colorsOne = texture.To1DArray();

            Color[,] colorsTwo = new Color[texture.Width, texture.Height]; //The new, easy to read 2D array
            for (int x = 0; x < texture.Width; x++) //Convert!
                for (int y = 0; y < texture.Height; y++)
                    colorsTwo[x, y] = colorsOne[x + y * texture.Width];

            return colorsTwo; //Done!
        }

        public static void From2DArray(this Texture2D texture, Color[,] colors2D)
        {
            Color[] colors1D = new Color[colors2D.Length];
            for (int x = 0; x < colors2D.GetLength(0); x++)
                for (int y = 0; y < colors2D.GetLength(1); y++)
                    colors1D[x + y * colors2D.GetLength(0)] = colors2D[x, y];

            texture.SetData(colors1D);
        }

        public static void ManipulateColors2D(this Texture2D texture, Func<Color[,], Color[,]> action)
        {
            texture.From2DArray(action(texture.To2DArray()));
        }
        #endregion
    }
}
