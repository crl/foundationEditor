using System.IO;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class CopyZip2StreamAsset : ScriptableWizard
    {
        //[MenuItem("Tools/CopyZip2StreamAsset")]
        public static void ExportSelectionToAssetboundle()
        {
            ScriptableWizard.DisplayWizard("复制zip到StreamAsset", typeof (CopyZip2StreamAsset), "确定");
        }

        private static string EditorPrefs_Key = "CopyZip2StreamAsset_Selected";
        public string zipFromPath = "";

        protected void OnEnable()
        {
            string path = EditorPrefs.GetString(EditorPrefs_Key);
            if (string.IsNullOrEmpty(path) == false)
            {
                this.zipFromPath = path;
            }
        }

        protected override bool DrawWizardGUI()
        {
            EditorGUILayout.BeginHorizontal();
            zipFromPath = EditorGUILayout.TextField("path:", zipFromPath);

            if (GUILayout.Button("选择"))
            {
                string path = EditorUtility.OpenFilePanel("zip", zipFromPath, "zip");
                if (string.IsNullOrEmpty(path) == false)
                {
                    zipFromPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            return true;
        }

        protected virtual void OnWizardCreate()
        {

            if (File.Exists(zipFromPath) == false)
            {
                ShowNotification(new GUIContent("not exist"));
                return;
            }

            EditorPrefs.SetString(EditorPrefs_Key,zipFromPath);

            FileInfo file=new FileInfo(zipFromPath);

            DirectoryInfo directoryInfo=file.Directory;

            string path=directoryInfo.FullName + "/z.txt";

            File.Copy(zipFromPath, Application.streamingAssetsPath+"/z.zip",true);

            if (File.Exists(path))
            {
                File.Copy(path, Application.streamingAssetsPath + "/z.txt",true);
            }
            AssetDatabase.Refresh();
        }
    
    }
}