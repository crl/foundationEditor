using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace foundationEditor
{
    public class PictureEditor : ScriptableWizard
    {
        public string _copyPictureFromPath = "E:/language_fuck";
        public string _copyPictureToPath = "";

        private List<FileInfo> defaultList = new List<FileInfo>();
        private Dictionary<string, List<FileInfo>> otherDictionary = new Dictionary<string, List<FileInfo>>();

        private List<string> languages = new List<string>();

        [MenuItem("App/PictureEditor",false,201)]
        public static void TPictureEditor()
        {
            ScriptableWizard.DisplayWizard("选择发布类型", typeof (PictureEditor), "确定");
        }

        public PictureEditor()
        {
            languages.Add("china");
        }

        protected void OnEnable()
        {
            string path = Application.dataPath;
            FileInfo fileInfo = new FileInfo(path);
            path = Path.Combine(fileInfo.Directory.FullName, "language_fuck");
            DirectoryInfo dir = new DirectoryInfo(path);

            if (dir.Exists == false)
            {
                return;
            }

            FileInfo[] fileInfos = dir.GetFiles();
            foreach (FileInfo info in fileInfos)
            {
                string fileName=Path.GetFileNameWithoutExtension(info.Name);
                string[] names = fileName.Split('_');

                if (names.Length < 2)
                {
                    defaultList.Add(info);
                    continue;
                }
                string key = names[names.Length-1].ToLower();
                int index = languages.IndexOf(key);
                if (index == -1)
                {
                    defaultList.Add(info);
                    continue;
                }

                List<FileInfo> otherList = null;
                if (otherDictionary.TryGetValue(key, out otherList) == false)
                {
                    otherList = new List<FileInfo>();
                    otherDictionary.Add(key, otherList);
                }
                otherList.Add(info);
            }
        }



        protected override bool DrawWizardGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("默认"))
                {
                    copyFile(defaultList, "");
                }
                
                foreach (string key in otherDictionary.Keys)
                {
                    if (GUILayout.Button(key))
                    {
                        copyFile(otherDictionary[key], key);
                    }
                }
            }

            return true;//base.DrawWizardGUI();
        }

        private void copyFile(List<FileInfo> list,string key)
        {
            string path=Path.Combine(Application.dataPath, "language");
            bool isEmpty = string.IsNullOrEmpty(key);

            foreach (FileInfo fileInfo in list)
            {
                if (isEmpty)
                {
                    File.Copy(fileInfo.FullName, path + "/" + fileInfo.Name, true);

                    continue;
                    
                }
                string fileName=Path.GetFileNameWithoutExtension(fileInfo.Name);
                string[] names = fileName.Split('_');
                string name = names[0];
                int len = names.Length;
                for (int i = 1; i < len-1; i++)
                {
                    name +="_"+names[i];
                }
                File.Copy(fileInfo.FullName, path + "/" + name +fileInfo.Extension, true);
            }


            AssetDatabase.Refresh();

        }
    }
}
