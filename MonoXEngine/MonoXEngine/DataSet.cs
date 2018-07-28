using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Xml.XPath;
using System.Diagnostics;
using System.Collections;

namespace MonoXEngine
{
    public class DataSet
    {
        public Dictionary<string, object> Data;

        public DataSet()
        {
            this.Data = new Dictionary<string, object>();
        }

        public string KeysToString(List<string> keys)
        {
            return string.Join("/", keys);
        }

        public List<string> StringToKeys(string keys)
        {
            return keys.Split("/".ToCharArray()).ToList<string>();
        }

        public void Set(List<string> keys, object value)
        {
            this.Data.Add(KeysToString(keys), value);
        }

        public void Set(string keys, object value)
        {
            this.Data.Add(keys, value);
        }

        public T Get<T>(List<string> keys)
        {
            return (T)Convert.ChangeType(Data[KeysToString(keys)], typeof(T));
        }

        public T Get<T>(params string[] keys)
        {
            return Get<T>(keys.ToList());
        }

        public Dictionary<string, object> GetChildren(string path)
        {
            List<string> pathKeys = path.Split('/').ToList();
            Dictionary<string, object> newDict = new Dictionary<string, object>();

            foreach(KeyValuePair<string, object> item in Data)
            {
                List<string> keys = item.Key.Split('/').ToList();
                if(keys.GetRange(0, pathKeys.Count).SequenceEqual(pathKeys))
                {
                    if(pathKeys.Count < keys.Count)
                        newDict.Add(keys[pathKeys.Count], item.Value);
                }
            }

            return newDict;
        }

        private void RenameKey(string fromKey, string toKey)
        {
            object value = Data[fromKey];
            Data.Remove(fromKey);
            Data[toKey] = value;
        }

        public DataSet NewNarrowedDataSet(string path)
        {
            DataSet ds = new DataSet();
            XDocument xDocument = ToXML();
            ds.FromXML(xDocument.Root.XPathSelectElement(path).Descendants());

            // Removes path
            for (int I = 0; I < ds.Data.Count; I++)
                ds.RenameKey(ds.Data.Keys.ElementAt<string>(I), ds.Data.Keys.ElementAt<string>(I).TrimStart((path + "/").ToCharArray()));

            return ds;
        }

        public void FromXML(IEnumerable<XElement> elements)
        {
            Data = new Dictionary<string, object>();
            foreach (XElement xEl in elements.Where(x => x.Descendants().Count() == 0))
            {
                List<string> keys = new List<string>();
                foreach (XElement a in xEl.AncestorsAndSelf().ToList())
                    keys.Add(a.Name.ToString());

                keys.Reverse();
                keys.RemoveAt(0);
                Set(string.Join("/", keys), xEl.Value);
            }
        }

        public void FromXML(XElement elements)
        {
            FromXML(elements.Descendants());
        }

        public void FromXML(XDocument xDocument)
        {
            FromXML(xDocument.Root);
        }

        public XDocument ToXML(string root = "Root")
        {
            XDocument xDocument = new XDocument();
            xDocument.Add(new XElement(root));

            foreach(KeyValuePair<string, object> kv in Data)
            {
                string xPath = "/" + root;
                string prevPath = "";

                string[] keys = kv.Key.Split('/');
                for(int I = 0; I < keys.Length; I++)
                {
                    prevPath = xPath;
                    xPath += "/" + keys[I];

                    if (xDocument.XPathSelectElement(xPath) == null)
                    {
                        if(I < keys.Length-1)
                            xDocument.XPathSelectElement(prevPath).Add(new XElement(keys[I]));
                        else
                            xDocument.XPathSelectElement(prevPath).Add(new XElement(keys[I], kv.Value));
                    }
                }
            }

            return xDocument;
        }
    }
}
