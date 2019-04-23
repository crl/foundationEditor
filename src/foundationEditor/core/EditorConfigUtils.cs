using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using foundation;
using UnityEngine;

namespace foundationEditor
{
    public class EditorConfigUtils
    {
        public static string svnExe = "TortoiseProc.exe";

        private static XmlDocument _doc;

        private static ASDictionary<string,string> prefixs=new ASDictionary<string, string>(); 
        private static ASDictionary<string,string[]> prefabExports=new ASDictionary<string[]>();
        private static ASDictionary<string, string> autoCopys = new ASDictionary<string, string>();
        private static ASDictionary<string, XmlNode> prefabExportsRaw = new ASDictionary<string, XmlNode>();
        public static XmlDocument doc
        {
            get
            {
                if (_doc == null)
                {
                    _doc=load();
                }
                return _doc;
            }
        }
        public static XmlDocument load()
        {
            string path = Application.dataPath + "/Editor/config.xml";
            if (File.Exists(path) == false)
            {
                return null;
            }
            _doc = new XmlDocument();
            _doc.Load(path);

            XmlNode node=_doc.SelectSingleNode("config/prefixes");

            foreach (XmlNode childNode in node.ChildNodes)
            {
                string key=childNode.Attributes["name"].InnerText;
                string value=childNode.Attributes["value"].InnerText;

                prefixs.Add(key,value);
            }


            node = doc.SelectSingleNode("config/prefabExport");
            foreach (XmlNode itemNode in node.ChildNodes)
            {
                string name = itemNode.Attributes["name"].InnerText;
                if (prefabExports.ContainsKey(name))
                {
                    continue;
                }
                prefabExportsRaw.Add(name, itemNode);
                try
                {
                    string[] tempList = itemNode.Attributes["from"].InnerText.As3Split(",");
                    prefabExports.Add(name, tempList);
                }
                catch (Exception)
                {
                }
            }

            autoCopys.Clear();
            node = doc.SelectSingleNode("config/autoCopy");
            if (node != null)
            {
                foreach (XmlNode itemNode in node.ChildNodes)
                {
                    string from = itemNode.Attributes["from"].InnerText;
                    string to = itemNode.Attributes["to"].InnerText;
                    if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
                    {
                        continue;
                    }
                    if (autoCopys.ContainsKey(from) == false)
                    {
                        autoCopys.Add(from, to);
                    }
                }
            }
            return _doc;
        }

        public static string GetPrifix(string name)
        {
            if (_doc == null)
            {
                _doc = load();
            }
            string value = null;
            prefixs.TryGetValue(name, out value);
            return value;
        }

        public static ASDictionary<string, string> GetAutoCopys()
        {
            if (_doc == null)
            {
                _doc = load();
            }
            return autoCopys;
        }

        public static string ProjectResource
        {
            get
            {
                if (_doc == null)
                {
                    _doc = load();
                }
                string value = "";
                prefixs.TryGetValue("projectResource", out value);
                if (string.IsNullOrEmpty(value))
                {
                    FileInfo fileInfo = new FileInfo(Application.dataPath);
                    value = Path.Combine(fileInfo.Directory.FullName, "ReleaseResource/");
                    prefixs.Add("projectResource", value);
                }
                return value;
            }
        }

        public static string AnimationType
        {
            get
            {
                string value = "";
                prefixs.TryGetValue("animationType", out value);
                return value;
            }
        }

        public static string[] HumanoidFolders
        {
            get
            {
                string value = "";
                prefixs.TryGetValue("humanoidFolder", out value);
                if (string.IsNullOrEmpty(value))
                {
                    return new string[0];
                }

                return value.As3Split(",");
            }
        }

        public static string[] AnimationClipEvents
        {
            get
            {
                if (_doc == null)
                {
                    _doc = load();
                }
                string value = "";
                prefixs.TryGetValue("animationClipEvents", out value);
                if (string.IsNullOrEmpty(value))
                {
                    return new string[0];
                }

                return value.As3Split(",");
            }
        }

        public static string[] LoopAnimationClipNames
        {
            get
            {
                string value = "";
                prefixs.TryGetValue("animationLoopNames", out value);

                if (string.IsNullOrEmpty(value))
                {
                    return new string[0];
                }

                return value.As3Split(",");
            }
        }


        public static string SpriteSlice
        {
            get
            {
                XmlDocument _doc=doc;
                string value = "";
                prefixs.TryGetValue("spriteSlice", out value);
                return value;
            }
        }

        public static string[] GetPrefabExports(string key)
        {
            if (_doc == null)
            {
                _doc = load();
            }

            string[] result=null;
            prefabExports.TryGetValue(key, out result);
            return result;
        }
        public static XmlNode GetPrefabExportsRaw(string key)
        {
            if (_doc == null)
            {
                _doc = load();
            }

            XmlNode result = null;
            prefabExportsRaw.TryGetValue(key, out result);
            return result;
        }

        public static string GetProjectResource(string value)
        {
            return ProjectResource  + value;
        }
    }
}