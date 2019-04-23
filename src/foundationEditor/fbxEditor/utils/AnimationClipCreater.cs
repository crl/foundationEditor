using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace foundationEditor.utils
{
    public class AnimationClipCreater
    {
        private List<ModelImporterClipAnimation> _clipList;
        public AnimationClipCreater()
        {
            _clipList = new List<ModelImporterClipAnimation>();
        }

        /// <summary>
        /// 通过配置分帧
        /// </summary>
        /// <param name="modelImporter"></param>
        /// <param name="xml"></param>
        public static void CreateByConfig(ModelImporter modelImporter, XmlDocument xml)
        {
            AnimationClipCreater creater=new AnimationClipCreater();
            foreach (XmlNode childNode in xml.ChildNodes)
            {
                string name = childNode.Attributes["name"].InnerText;
                int firstFrame = int.Parse(childNode.Attributes["firstFrame"].InnerText);
                int lastFrame = int.Parse(childNode.Attributes["lastFrame"].InnerText);
                bool loop = childNode.Attributes["loop"].InnerText=="1";
                WrapMode wrapMode = (WrapMode)(int.Parse(childNode.Attributes["wrapMode"].InnerText));
                creater.addClip(name, firstFrame, lastFrame, loop, wrapMode);
            }
            modelImporter.clipAnimations = creater.clipList.ToArray();
        }

        public void addClip(string name, int firstFrame, int lastFrame, bool loop, WrapMode wrapMode)
        {
            ModelImporterClipAnimation tempClip = new ModelImporterClipAnimation();

            tempClip.name = name;
            tempClip.firstFrame = firstFrame;
            tempClip.lastFrame = lastFrame;
            tempClip.loop = loop;
            tempClip.wrapMode = wrapMode;
            _clipList.Add(tempClip);
        }

        public List<ModelImporterClipAnimation> clipList
        {
            get { return _clipList; }
        }
    }
}