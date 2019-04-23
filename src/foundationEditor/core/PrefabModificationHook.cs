using System;
using foundationEditor;
using UnityEngine;

namespace foundationEditor
{
   
    public class PrefabModificationHook : UnityEditor.AssetModificationProcessor
    {
        private static string[] OnWillSaveAssets(string[] paths)
        {
            if (ToolsExtends.enabledAutoAMF==false)
            {
                return paths;
            }
            SaveGUIDMap map = SaveGUIDMap.Get();
            bool has = false;

            foreach (string path in paths)
            {
                if (path.EndsWith(".prefab"))
                {
                    try
                    {
                        PrefabFileRefGet prefabFileRefGet = PrefabFileRefGet.get(path);
                        map.add(prefabFileRefGet);
                        has = true;
                    }
                    catch (Exception)
                    {
                        Debug.LogError("保存Prefab guid出错 :" + path);
                    }
                }
            }


            if (has)
            {
                map.save();
            }
            return paths;
        }
    }
}
