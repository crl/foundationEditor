using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class FindMissHelper
    {
        private SaveGUIDMap map;

        public FindMissHelper()
        {
            map = SaveGUIDMap.GetALL();
            SaveGUIDMap.initPluginDlls();
        }

        public void doSingle(GameObject go,bool isSingle=true)
        {
            bool hasChange = false;
            bool hasError = false;

            string v = AssetDatabase.GetAssetPath(go);
            PrefabFileRefGet prefabFileRefGet = PrefabFileRefGet.get(v);

            Selection.activeObject = null;
            Selection.activeGameObject = null;

            foreach (FileRefVO vo in prefabFileRefGet.fileRefs)
            {
                string guidPath = vo.guidPath;

                if (vo.isMonoScript)
                {
                    if (string.IsNullOrEmpty(guidPath))
                    {
                        guidPath = map.getGUIDPath(vo.guid);
                    }

                    if (string.IsNullOrEmpty(guidPath))
                    {
                        continue;
                    }

                    //map.getGUIDPath(vo.guid);
                    string className;

                    if (guidPath.EndsWith(".dll"))
                    {
                        className = map.getFileIDName(vo.fileID);
                        if (string.IsNullOrEmpty(className))
                        {
                            className = FileIDUtil.getFileNameByFileID(int.Parse(vo.fileID));
                        }
                        string outDllPath;
                        Type classType = FileIDUtil.getTypeByName(className, out outDllPath);
                        if (classType != null && outDllPath != guidPath)
                        {
                            vo.fileIDName = null;
                            vo.fileID = null;
                        }
                    }
                    else
                    {
                        className = Path.GetFileNameWithoutExtension(guidPath);
                    }

                    if (className == null)
                    {
                        continue;
                    }

                    ///没有变化;
                    if (className == vo.fileIDName)
                    {
                        continue;
                    }

                    string firstChar = className[0].ToString().ToUpper();
                    if (firstChar != className[0].ToString())
                    {
                        className = firstChar + className.Substring(1);
                    }

                    string dllPath;
                    Type type = FileIDUtil.getTypeByName(className, out dllPath);
                    if (type == null)
                    {
                        className = FileIDUtil.getRouter(className);
                        type = FileIDUtil.getTypeByName(className, out dllPath);
                    }

                    if (type != null)
                    {
                        string guid = AssetDatabase.AssetPathToGUID(dllPath);
                        string fileID = FileIDUtil.Compute(type).ToString();
                        if (string.IsNullOrEmpty(guid) == false && string.IsNullOrEmpty(fileID) == false &&
                            fileID != vo.fileID)
                        {
                            vo.guid = guid;
                            vo.fileID = fileID;
                            vo.isChange = true;
                        }
                    }
                    else if (string.IsNullOrEmpty(vo.guidPath) == false)
                    {
                        if (vo.fileID != "11500000")
                        {
                            //不在dll里面的类
                            vo.fileID = "11500000";
                            vo.isChange = true;
                        }
                    }
                    else
                    {
                        Debug.LogError("找不到类: " + className + ", 请配置一个转换路径");
                        hasError = true;
                        continue;
                    }
                }
                else if (string.IsNullOrEmpty(vo.guidPath))
                {
                    string path = map.getGUIDPath(vo.guid);
                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }

                    string guid = AssetDatabase.AssetPathToGUID(path);
                    if (string.IsNullOrEmpty(guid))
                    {
                        path = FileIDUtil.getRouter(path);
                        guid = AssetDatabase.AssetPathToGUID(path);
                    }

                    if (string.IsNullOrEmpty(guid))
                    {
                        Debug.LogError(path + ":找不到路径,请配置一个转换路径");
                        hasError = true;
                        continue;
                    }

                    if (guid != vo.guid)
                    {
                        vo.guid = guid;
                        vo.isChange = true;
                    }
                }

                if (vo.isChange)
                {
                    hasChange = true;
                }
            }

            if (hasError)
            {
                return;
            }

            if (hasChange)
            {
                prefabFileRefGet.replace();
                //AssetDatabase.Refresh();
            }
            else if(isSingle)
            {
                EditorUtility.DisplayDialog("找回失败", "只能手动找回,\n\n如果复杂可找程序员看是否有其它方法找回!!!", "好的");
            }
        }
    }
}