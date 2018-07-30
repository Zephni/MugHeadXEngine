using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Content;
using System.IO;

namespace MugHeadXEngine
{
    public static class GameData
    {
        private static string Format_UnitSeperator = ";!;";
        private static string Format_KVSeperator = ":!:";
        private static Dictionary<string, string> Data = new Dictionary<string, string>();

        // Location needs sorting
        public static string SavePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        public static string SaveFile = "DebugData.txt";

        public static void Set(string key, string value)
        {
            if(!Data.ContainsKey(key))
                Data.Add(key, value);
            else
                Data[key] = value;
        }

        public static string Get(string key)
        {
            if (Data.ContainsKey(key))
                return Data[key];
            else
                return null;
        }

        public static void Reset()
        {
            Data = new Dictionary<string, string>();
        }

        public static string GetFilePath()
        {
            return new Uri(SavePath).LocalPath + "\\" + SaveFile;
        }

        public static void Save()
        {
            string SaveString = "";

            foreach (KeyValuePair<string, string> item in Data)
                SaveString += item.Key + Format_KVSeperator + item.Value + Format_UnitSeperator;

            SaveString = SaveString.TrimEnd(Format_UnitSeperator.ToCharArray());

            // Saving needs sorting depending on device and save slot
            System.IO.File.WriteAllText(GetFilePath(), SaveString);
        }

        public static void Load()
        {
            // Loading needs sorting depending on device and save slot
            string LoadString = System.IO.File.ReadAllText(GetFilePath());

            string[] LoadStringUnits = LoadString.Split(new string[] { Format_UnitSeperator }, StringSplitOptions.None);

            foreach(string Unit in LoadStringUnits)
            {
                string[] KVUnit = Unit.Split(new string[] { Format_KVSeperator }, StringSplitOptions.None);
                string key = KVUnit[0];
                string value = KVUnit[1];

                // Load data
                Set(key, value);
            }
        }
    }
}
