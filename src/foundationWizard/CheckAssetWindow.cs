using foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class CheckAssetWindow : ScriptableWizard
    {

        [HideInInspector] public string assetPath;

        public bool IsAutoDele = false;

        public CheckAssetWindow()
        {
            if (EditorPrefs.HasKey("assetPath"))
            {
                assetPath = EditorPrefs.GetString("assetPath");
            }
            else
            {
                assetPath = "/Prefabs/prefabResources/UI";
            }
        }

        protected override bool DrawWizardGUI()
        {
            EditorGUILayout.BeginHorizontal();

            assetPath = EditorGUILayout.TextField(assetPath);
            if (GUILayout.Button("select", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                string path = EditorUtility.OpenFolderPanel("", Application.dataPath, "");
                if (string.IsNullOrEmpty(path) == false)
                {
                    assetPath = path.Split(new string[] {"Assets"}, StringSplitOptions.None)[1];
                }
            }

            EditorGUILayout.EndHorizontal();
            IsAutoDele = EditorGUILayout.ToggleLeft("IsAutoDele", IsAutoDele);
            return true;
        }

        public void OnWizardCreate()
        {
            EditorPrefs.SetString("assetPath", assetPath);
            string url = Application.dataPath;
            List<string> prefabsUrl = FileHelper.FindFile(url, new string[] {"*.prefab", "*.asset" });
            string texUrl = Application.dataPath + assetPath;
            List<string> pngUrl = FileHelper.FindFile(texUrl, new string[] {"*.png", "*.jpg"});

            string[] useAssetPaths = AssetDatabase.GetDependencies(getLocalUrls(prefabsUrl).ToArray());
            List<string> totalAssetPath = getLocalUrls(pngUrl);

            List<string> uselessPng = new List<string>();

            for (int i = 0; i < totalAssetPath.Count; i++)
            {
                if (useAssetPaths.Contains(totalAssetPath[i]) == false)
                {
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(totalAssetPath[i]);
                    Debug.Log("未使用资源：" + totalAssetPath[i], obj);
                    uselessPng.Add(totalAssetPath[i]);
                    if (IsAutoDele)
                    {
                        AssetDatabase.DeleteAsset(totalAssetPath[i]);
                    }
                }
            }

        }

        private List<string> getLocalUrls(List<string> temps)
        {
            List<string> results = new List<string>();
            for (int i = 0; i < temps.Count; i++)
            {
                results.Add(GetLocalUrl(temps[i]));
            }
            return results;
        }

        private string GetLocalUrl(string url)
        {
            return "Asset" + url.Split(new string[] {"Asset"}, StringSplitOptions.None)[1];
        }
    }
}
