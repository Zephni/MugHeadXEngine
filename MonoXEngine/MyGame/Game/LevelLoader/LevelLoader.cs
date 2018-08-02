using Microsoft.Xna.Framework;
using MonoXEngine;
using MonoXEngine.Structs;
using MugHeadXEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using XCompressFile;

namespace XEditor
{
    public class LevelLoader
    {
        public void Load(string file, Action<List<Tile>, List<EntityInfo>> action)
        {
            string b64CompressedData = File.ReadAllText("MyGame/Game/Levels/" + file + ".lvl");
            XDocument loadedDoc = XDocument.Parse(Compressor.Unzip(Convert.FromBase64String(b64CompressedData)));
            DataFromDocument(loadedDoc, action);
            GameData.Set("Level", file);
        }

        private void DataFromDocument(XDocument xDoc, Action<List<Tile>, List<EntityInfo>> action)
        {
            string tilesetPath = xDoc.Root.Element("EditorConfig").Element("Tileset").Value;

            string[] mapSize = xDoc.Root.Element("Config").Element("MapSize").Value.Split(',');
            Point mapSizep = new Point(Convert.ToInt16(mapSize[0]), Convert.ToInt16(mapSize[1]));

            List<string> layers = new List<string>();
            foreach(XElement xEl in xDoc.Root.Element("Config").Elements("Layers").Descendants())
                layers.Add(xEl.Name.ToString());


            // READY MAP HERE
            //MainWindow.Instance.NewMap(mapSizep, tilesetPath, layers);

            List<Tile> tileList = new List<Tile>();
            foreach (XElement xEl in xDoc.Root.Element("Tiles").Elements("Tile"))
            {
                string[] source = xEl.Element("Source").Value.Split(',');
                string[] location = xEl.Element("Location").Value.Split(',');

                tileList.Add(new Tile(
                    new Point(Convert.ToInt16(source[0]), Convert.ToInt16(source[1])),
                    new Point3D(Convert.ToInt16(location[0]), Convert.ToInt16(location[1]), Convert.ToInt16(location[2]))
                ));
            }

            // Entities
            // Really this should just pass out the info, then can be dealt with
            List<EntityInfo> entityList = new List<EntityInfo>();
            foreach (XElement xEl in xDoc.Root.Element("Entities").Elements("Entity"))
            {
                string[] location = xEl.Element("Location").Value.Split(',');
                string[] size = xEl.Element("Size").Value.Split(',');

                entityList.Add(new EntityInfo {
                    Name = xEl.Element("Name").Value,
                    Position = new Vector2(Convert.ToInt16(location[0]), Convert.ToInt16(location[1])),
                    Data = xEl.Element("CustomData").Value,
                    Size = new Point(Convert.ToInt16(size[0]), Convert.ToInt16(size[1]))
                });
            }
            
            action.Invoke(tileList, entityList);
        }
    }
}
