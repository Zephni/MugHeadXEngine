using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
    public class ZInterpreter
    {
        Dictionary<string, string> Data;

        public ZInterpreter(string str, string delim1 = ";", string delim2 = "=")
        {                
            Data = new Dictionary<string, string>();

            foreach (var row in str.Split(new string[] { delim1 }, StringSplitOptions.None))
            {
                if (row.Length == 0 || row == "\n") continue;
                string[] column = row.Split(new string[] { delim2 }, StringSplitOptions.None);
                Data.Add(column[0].Trim(), column[1].Trim());
            }
        }

        public bool HasKey(string key)
        {
            return Data.ContainsKey(key);
        }

        public string GetString(string key)
        {
            if (!HasKey(key))
                return default(string);

            return Data[key];
        }

        public int GetInt(string key)
        {
            if (!HasKey(key))
                return default(int);

            return Convert.ToInt16(Data[key]);
        }

        public float GetFloat(string key)
        {
            if (!HasKey(key))
                return default(float);

            return (float)Convert.ToDouble(Data[key]);
        }

        public int[] GetIntArr(string key, char split = ',')
        {
            var items = Data[key].Split(',');
            int[] intArr = new int[items.Length];

            for (int I = 0; I < intArr.Length; I++)
                intArr[I] = Convert.ToInt16(items[I]);

            return intArr;
        }

        public float[] GetFloatArr(string key, char split = ',')
        {
            if (!Data.ContainsKey(key))
                return new float[2];

            var items = Data[key].Split(',');
            float[] intArr = new float[items.Length];

            for (int I = 0; I < intArr.Length; I++)
                intArr[I] = (float)Convert.ToDouble(items[I]);

            return intArr;
        }

        /// <summary>
        /// Points with comma delimiter, points seperated by space
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Point[] GetPointArr(string key)
        {
            var items = Data[key].Split(' ');
            Point[] pointArr = new Point[items.Length];

            for (int I = 0; I < pointArr.Length; I++)
            {
                string[] pointData = items[I].Split(',');
                pointArr[I] = new Point(Convert.ToInt16(pointData[0]), Convert.ToInt16(pointData[1]));
            }

            return pointArr;
        }
    }
}
