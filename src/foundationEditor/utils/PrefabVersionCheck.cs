using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;


namespace foundationEditor
{
    public class PrefabVersionCheck
    {
        private static string hashName = "hash.dat";

        private string hashFile;

        private Dictionary<string, string> hashDictionary;
        private Dictionary<string,List<string>> prefabDependenciesMapping=new Dictionary<string, List<string>>();

        public PrefabVersionCheck(string folderPath)
        {
            hashFile = folderPath + "/" + hashName;
            if (File.Exists(hashFile) == false)
            {
                hashDictionary = new Dictionary<string, string>();
            }
            else
            {
                try
                {
                    using (FileStream fileStream = File.OpenRead(hashFile))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        hashDictionary = (Dictionary<string, string>) formatter.Deserialize(fileStream);
                    }
                }
                catch (Exception)
                {
                }

            }
        }

        public void save(List<string> successList)
        {
            List<string> allSuccessList=new List<string>();

            List<string> temp;
            for (int i = 0; i < successList.Count; i++)
            {
                if (prefabDependenciesMapping.TryGetValue(successList[i], out temp))
                {
                    allSuccessList.AddRange(temp);
                }
            }
            foreach (string fileKey in allSuccessList)
            {
                FileInfo fileInfo = new FileInfo(fileKey);
                string lastWriteTime = fileInfo.LastWriteTime.ToString();
                string oldHashValue;

                if (hashDictionary.TryGetValue(fileKey, out oldHashValue))
                {
                    if (lastWriteTime != oldHashValue)
                    {
                        hashDictionary[fileKey] = lastWriteTime;
                    }
                }
                else
                {
                    hashDictionary.Add(fileKey, lastWriteTime);
                }
            }

            try
            {
                using (FileStream fileStream = File.OpenWrite(hashFile))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fileStream, hashDictionary);
                }
            }
            catch (Exception)
            {
            }
        }

        private void collection(string fullPath, Dictionary<string, List<string>> allDependencies)
        {
            string[] dependencies = null;
            if (fullPath.IndexOf(".prefab") == -1)
            {
                dependencies = new[] {fullPath};
            }
            else
            {
                dependencies = AssetDatabase.GetDependencies(new string[] {fullPath});
            }

            if (dependencies == null)
            {
                return;
            }

            List<string> prefabDependencies=new List<string>();

            List<string> hashSet;
            foreach (string item in dependencies)
            {
                if (item.IndexOf(".cs") != -1 && File.Exists(item) == false)
                {
                    continue;
                }
                if (item.IndexOf(".dll") != -1)
                {
                    continue;
                }

                if (item.IndexOf(".png") != -1)
                {
                    string pngMetaItem = item.Replace(".png", ".png.meta");
                    if (allDependencies.TryGetValue(pngMetaItem, out hashSet) == false)
                    {
                        hashSet = new List<string>();
                        allDependencies.Add(pngMetaItem, hashSet);
                    }

                    if (hashSet.Contains(fullPath) == false)
                    {
                        hashSet.Add(fullPath);
                    }

                    prefabDependencies.Add(pngMetaItem);
                }

                if (allDependencies.TryGetValue(item, out hashSet) == false)
                {
                    hashSet = new List<string>();
                    allDependencies.Add(item, hashSet);
                }

                if (hashSet.Contains(fullPath) == false)
                {
                    hashSet.Add(fullPath);
                }

                prefabDependencies.Add(item);
            }

            prefabDependenciesMapping.Add(fullPath,prefabDependencies);
        }

        private void checkHash(string fileKey, Dictionary<string, List<string>> allDependencies,
            List<string> changePrefab)
        {
            string oldHashValue;
            List<string> prefabsValue;

            FileInfo fileInfo = new FileInfo(fileKey);
            string lastWriteTime = fileInfo.LastWriteTime.ToString();

            prefabsValue = allDependencies[fileKey];
            hashDictionary.TryGetValue(fileKey, out oldHashValue);

            if (lastWriteTime != oldHashValue)
            {
                foreach (string item in prefabsValue)
                {
                    if (changePrefab.Contains(item) == false)
                    {
                        changePrefab.Add(item);
                    }
                }
            }

        }

        public List<string> getChangePrefab(List<string> prefabRelativePathList)
        {
            Dictionary<string, List<string>> allDependencies=new Dictionary<string, List<string>>();
            for (int i = 0; i < prefabRelativePathList.Count; i++)
            {
                collection(prefabRelativePathList[i], allDependencies);
            }

            List<string> changePrefab = new List<string>();

            foreach (string fileKey in allDependencies.Keys)
            {
                checkHash(fileKey, allDependencies, changePrefab);
            }
            return changePrefab;
        }
    }
}
