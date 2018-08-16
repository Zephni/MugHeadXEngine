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
                if (row.Length == 0) continue;
                string[] column = row.Split(new string[] { delim2 }, StringSplitOptions.None);
                Data.Add(column[0].Trim(), column[1].Trim());
            }
        }

        public string GetString(string key)
        {
            return Data[key];
        }

        public int GetInt(string key)
        {
            return Convert.ToInt16(Data[key]);
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
            var items = Data[key].Split(',');
            float[] intArr = new float[items.Length];

            for (int I = 0; I < intArr.Length; I++)
                intArr[I] = (float)Convert.ToDouble(items[I]);

            return intArr;
        }
    }
}
