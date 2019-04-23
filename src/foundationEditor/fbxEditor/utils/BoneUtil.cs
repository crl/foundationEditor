using System;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace foundationEditor.utils
{
    public class BoneUtil
    {
        public static void BindBone(FBXInfo fbxInfo, GameObject fbxInstance)
        {
            string importFilePath = fbxInfo.fbxFolder + "import.xml";
            if (File.Exists(importFilePath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(importFilePath);

                XmlNodeList nodeList = doc.SelectSingleNode("data").SelectNodes("import");

                foreach (XmlNode node in nodeList)
                {
                    string importFbxName = node.Attributes["fbx"].InnerText;
                    string importPrefabPath = "Assets/Prefabs/" + importFbxName + ".prefab";
                    if (File.Exists(importPrefabPath) == false)
                    {
                        continue;
                    }
                    string importToBoneName = node.Attributes["bone"].InnerText;
                    Transform boneTransform = findBone(fbxInstance.transform, importToBoneName);
                    if (boneTransform == null)
                    {
                        continue;
                    }
                    GameObject importPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(importPrefabPath);
                    if (importPrefab == null)
                    {
                        continue;
                    }
                    GameObject importPrefabInstance = GameObject.Instantiate(importPrefab);
                    importPrefabInstance.transform.SetParent(boneTransform, false);
                }
            }
        }

        public static Transform findBone(Transform transform, string boneName)
        {
            Transform result = null;
            int len = transform.childCount;
            for (int i = 0; i < len; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name == boneName)
                {
                    return child;
                }

                result = findBone(child, boneName);
                if (result != null)
                {
                    return result;
                }
            }

            return result;
        }
    }
}