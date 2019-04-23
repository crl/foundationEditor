using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using foundation;
using UnityEngine;

namespace foundationEditor
{
    public class SaveGUIDMap : IAmfSetMember
    {
        public Dictionary<string, string> guidMapping = new Dictionary<string, string>();
        public Dictionary<string, string> fileIDMapping = new Dictionary<string, string>();

        public string deviceName;

        public void add(PrefabFileRefGet prefabFileRefGet)
        {
            foreach (FileRefVO scriptVo in prefabFileRefGet.fileRefs)
            {
                string guidPath = scriptVo.guidPath;
                if (string.IsNullOrEmpty(guidPath) == false)
                {
                    if (addGuid(scriptVo.guid, scriptVo.guidPath))
                    {
                        PrefabFileRefGet get = PrefabFileRefGet.get(scriptVo.guidPath);
                        add(get);
                    }
                }

                string fileName = scriptVo.fileIDName;
                if (string.IsNullOrEmpty(fileName) == false)
                {
                    addFileID(scriptVo.fileID, fileName);
                }
            }
        }

        public string getFileIDName(string fileID)
        {
            string value;
            guidMapping.TryGetValue(fileID, out value);

            return value;
        }

        public string getGUIDPath(string guid)
        {
            string value;
            guidMapping.TryGetValue(guid, out value);

            return value;
        }

        public static SaveGUIDMap Get(string path = null)
        {
            if (path == null)
            {
                path = "Assets/GuidMapping/" + SystemInfo.deviceUniqueIdentifier + "_c.amf";
            }
            SaveGUIDMap s = FileHelper.GetAMF(path) as SaveGUIDMap;
            if (s == null)
            {
                s = new SaveGUIDMap();
                s.deviceName = SystemInfo.deviceName;
            }
            s.path = path;
            return s;
        }

        public static SaveGUIDMap GetALL()
        {
            SaveGUIDMap s = new SaveGUIDMap();

            List<string> amfs = FileHelper.FindFile("Assets/GuidMapping/", new string[] {"*.amf"});
            foreach (string amf in amfs)
            {
                string amfPath = amf.Replace(Application.dataPath, "Assets");
                SaveGUIDMap item = FileHelper.GetAMF(amfPath) as SaveGUIDMap;

                foreach (string guid in item.guidMapping.Keys)
                {
                    if (s.guidMapping.ContainsKey(guid) == false)
                    {
                        s.guidMapping.Add(guid, item.guidMapping[guid]);
                    }
                }
                foreach (string fileID in item.fileIDMapping.Keys)
                {
                    if (s.fileIDMapping.ContainsKey(fileID) == false)
                    {
                        s.fileIDMapping.Add(fileID, item.fileIDMapping[fileID]);
                    }
                }
            }

            string path = "Assets/GuidMapping/replace.txt";
            if (File.Exists(path))
            {
                using (StreamReader reader = File.OpenText(path))
                {
                    string content = reader.ReadToEnd();
                    getMapping(content);
                }
            }

            return s;
        }

        private static void getMapping(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }
            string[] list = content.Split(new string[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);

            if (list.Length < 1)
            {
                return;
            }

            FileIDUtil.clearRouter();
            string[] keyValuePair;
            string key;
            string value;
            foreach (string item in list)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                keyValuePair = item.Split('=');

                key = keyValuePair[0].Trim();
                value = keyValuePair[1].Trim();

                /*int index = value.IndexOf('\\');
                if (index!=-1)
                {
                    value=value.Substring(0, index - 1);
                }*/

                FileIDUtil.registerRouter(key, value);
            }
        }

        public static void initPluginDlls()
        {
            List<string> dlls = FileHelper.FindFile("Assets/Plugins/", new string[] {"*.dll"});

            SaveGUIDMap saveGuidMap = Get();

            bool hasNew = false;
            foreach (string dllItem in dlls)
            {
                string dllPath = dllItem.Replace(Application.dataPath, "Assets");
                Dictionary<int, string> dic = FileIDUtil.getAllFileIDByDll(dllPath);
                if (dic == null) continue;
                foreach (int key in dic.Keys)
                {
                    if (saveGuidMap.addFileID(key.ToString(), dic[key], false))
                    {
                        hasNew = true;
                    }
                }
            }

            if (hasNew)
            {
                saveGuidMap.save();
            }
        }


        private string path;

        public void save()
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            FileHelper.SaveAMF(this, path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="path"></param>
        /// <param name="tipError"></param>
        /// <returns></returns>
        public bool addGuid(string guid, string path, bool tipError = false)
        {
            string oldValue;
            if (guidMapping.TryGetValue(guid, out oldValue))
            {
                if (oldValue != path && tipError)
                {
                    Debug.LogError("guid:" + guid + " path:" + oldValue + " newPath:" + path);
                }
                return false;
            }

            guidMapping.Add(guid, path);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileID"></param>
        /// <param name="path"></param>
        /// <param name="tipError"></param>
        /// <returns></returns>
        public bool addFileID(string fileID, string path, bool tipError = false)
        {
            string oldValue;
            if (fileIDMapping.TryGetValue(fileID, out oldValue))
            {
                if (oldValue != path && tipError)
                {
                    Debug.LogError("fileID:" + fileID + " fileID:" + oldValue + " newPath:" + path);
                }
                return false;
            }

            fileIDMapping.Add(fileID, path);
            return true;
        }

        public void __AmfSetMember(string key, object value)
        {
            IDictionary dic = value as IDictionary;
            if (key == "fileIDMapping")
            {
                foreach (object k in dic.Keys)
                {
                    fileIDMapping.Add((string) k, (string) dic[k]);
                }
            }
            else if (key == "guidMapping")
            {
                foreach (object k in dic.Keys)
                {
                    guidMapping.Add((string) k, (string) dic[k]);
                }


            }
        }
    }
}
