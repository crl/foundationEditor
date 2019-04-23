using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using foundation;
using UnityEditor.Sprites;

namespace foundationExport
{
    /// <summary>
    /// Assetbundle项目打具,有相应的配置来配置组合打包哪些文件夹,用什么压缩方式
    /// 对于场景会特别处理 (场景比较特别，*用于的mesh也直接打进场景)
    /// 两个打包项的同名同路径在unity里面认为是同一个文件
    /// </summary>
    public class PrefabExport
    {
        protected string exportPrefabToPath = "";
        public bool lz4Compress = false;
        public bool isForceRebuild = false;

        protected const string BITMAPS = "bi";
        protected const string SOUNDS = "so";
        protected const string ASSETS = "a";
        protected const string OBJS = "obj";
        protected const string SHADERS = "sh";
        protected const string MATERIALS = "ma";
        protected const string FBXS = "fbx";
        protected const string ANIMS = "ani";
        protected const string PREFABS = "pr";
        protected const string PLAYABLES = "pl";
        protected const string CONTROLLERS = "con";
        protected const string OTHERS = "ot";

        protected Dictionary<string, int> assetsReferenceCounter = new Dictionary<string, int>();

        protected Dictionary<string, AssetBundleBuild> assetBundleReferences =
            new Dictionary<string, AssetBundleBuild>();

        protected Dictionary<string, HashSet<string>> toUnityPathMapping = new Dictionary<string, HashSet<string>>();
        protected Dictionary<string, int> prefabDirMaping = new Dictionary<string, int>();
        protected BuildTarget buildTarget;
        protected string rootFolderName;

        private HashSet<string> toRemovesSet = new HashSet<string>();

        private Dictionary<string, ExportSceneAssetsRefVO> allScenesAssetsDic =
            new Dictionary<string, ExportSceneAssetsRefVO>();

        public PrefabExport(BuildTarget buildTarget, string exportPrefabToPath, string rootFolderName)
        {
            if (exportPrefabToPath.EndsWith("/") == false)
            {
                exportPrefabToPath += "/";
            }

            this.exportPrefabToPath = exportPrefabToPath;
            this.buildTarget = buildTarget;
            this.rootFolderName = rootFolderName;
            toUnityPathMapping.Add(BITMAPS, new HashSet<string>());
            toUnityPathMapping.Add(SOUNDS, new HashSet<string>());
            toUnityPathMapping.Add(ASSETS, new HashSet<string>());
            toUnityPathMapping.Add(SHADERS, new HashSet<string>());
            toUnityPathMapping.Add(MATERIALS, new HashSet<string>());
            toUnityPathMapping.Add(OBJS, new HashSet<string>());
            toUnityPathMapping.Add(FBXS, new HashSet<string>());
            toUnityPathMapping.Add(ANIMS, new HashSet<string>());
            toUnityPathMapping.Add(CONTROLLERS, new HashSet<string>());
            toUnityPathMapping.Add(PLAYABLES, new HashSet<string>());
            toUnityPathMapping.Add(PREFABS, new HashSet<string>());
            toUnityPathMapping.Add(OTHERS, new HashSet<string>());

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        public virtual void exportAllPrefab(List<ExportRootVO> list)
        {
            clearCache();
            //Unity5Packer
            Packer.RebuildAtlasCacheIfNeeded(buildTarget, true, Packer.Execution.Normal);

            List<ExportPrefabRefVO> prefabPathList = new List<ExportPrefabRefVO>();
            foreach (ExportRootVO exportPrefabItemVo in list)
            {
                foreach (string exportPrefabFromPath in exportPrefabItemVo.from)
                {
                    List<string> tempList = FileHelper.FindFile(exportPrefabFromPath, exportPrefabItemVo.extentions);
                    foreach (string item in tempList)
                    {
                        ExportPrefabRefVO prefabRefVo = new ExportPrefabRefVO();
                        prefabRefVo.path = item.Replace(Application.dataPath, "Assets");
                        prefabRefVo.exportRootVo = exportPrefabItemVo;
                        prefabPathList.Add(prefabRefVo);
                    }
                }
            }

            exprotPrefabs(prefabPathList);
        }

        public static void ShowProgress(int cur, int total)
        {
            float val = cur / (float) total;
            EditorUtility.DisplayProgressBar("Searching",
                string.Format("Finding ({0}/{1}), please wait...", cur, total), val);
        }

        protected bool exprotPrefabs(List<ExportPrefabRefVO> prefabPathList)
        {
            int len = prefabPathList.Count;
            int i = 0;
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            foreach (ExportPrefabRefVO exportPrefabRefVo in prefabPathList)
            {
                List<AssetBundleBuild> tempList = collectionPrefab(exportPrefabRefVo);
                if (tempList.Count > 0)
                {
                    builds.AddRange(tempList);
                }

                ShowProgress(i++, len);
            }

            EditorUtility.ClearProgressBar();

            #region 内部包含其它的prefab

            /*int prefabCount = prefabs.Count;
            while (prefabCount > 0)
            {
                string[] tempPrefabs = prefabs.ToArray();
                prefabs.Clear();

                bool exist = false;
                foreach (string item in tempPrefabs)
                {
                    exist = false;
                    foreach (ExportPrefabRefVO exportPrefabRefVo in prefabPathList)
                    {
                        if (exportPrefabRefVo.path == item)
                        {
                            exist = true;
                            break;
                        }
                    }
                    if (exist)
                    {
                        continue;
                    }
                    ///额外的外部引用
                    ExportPrefabRefVO newExportPrefabRefVo = new ExportPrefabRefVO();
                    newExportPrefabRefVo.path = item;
                    newExportPrefabRefVo.exportRootVo = otherExportRootVO;
                    prefabPathList.Add(newExportPrefabRefVo);

                    List<AssetBundleBuild> tempList = collectionPrefab(newExportPrefabRefVo);
                    if (tempList.Count > 0)
                    {
                        builds.AddRange(tempList);
                    }
                }
                prefabCount = prefabs.Count;
            }*/

            #endregion

            HashSet<String> bitmaps = toUnityPathMapping[BITMAPS];
            //Dictionary<string, List<string>> spritePacking = new Dictionary<string, List<string>>();
            List<string> toRemoveBitmap = new List<string>();
            foreach (string bitmap in bitmaps)
            {
                TextureImporter importer = TextureImporter.GetAtPath(bitmap) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                string spritePackingTag = importer.spritePackingTag;
                if (string.IsNullOrEmpty(spritePackingTag))
                {
                    continue;
                }

                toRemoveBitmap.Add(bitmap);
                AssetBundleBuild build = new AssetBundleBuild();
                string assetBundleName = "sp/" + spritePackingTag + PathDefine.U3D;
                build.assetBundleName = assetBundleName;
                build.assetNames = new string[] {bitmap};

                builds.Add(build);
                assetBundleReferences.Add(bitmap, build);
            }

            foreach (string bitmap in toRemoveBitmap)
            {
                bitmaps.Remove(bitmap);
            }

            foreach (string key in toUnityPathMapping.Keys)
            {
                HashSet<string> itemList = toUnityPathMapping[key];
                foreach (string item in itemList)
                {
                    if (assetsReferenceCounter[item] == 1)
                    {
                        continue;
                    }

                    string keyFolder = rootFolderName + "_" + key;

                    string fileName = Path.GetFileNameWithoutExtension(item).Trim();
                    fileName = fileNameFormat(fileName);
                    string assetBundleName = keyFolder + "/" + fileName + PathDefine.U3D;

                    int hasIndex;
                    if (prefabDirMaping.TryGetValue(assetBundleName, out hasIndex))
                    {
                        prefabDirMaping[assetBundleName] = hasIndex + 1;
                        assetBundleName = keyFolder + "/" + fileName + "_A" + (hasIndex + 1) + PathDefine.U3D;
                    }
                    else
                    {
                        prefabDirMaping.Add(assetBundleName, 0);
                    }

                    AssetBundleBuild build;
                    if (assetBundleReferences.TryGetValue(item, out build) == false)
                    {
                        build = new AssetBundleBuild();
                        build.assetBundleName = assetBundleName;
                        build.assetNames = new string[] {item};
                        builds.Add(build);
                        assetBundleReferences.Add(item, build);
                    }
                }
            }

            foreach (string scenePathKey in allScenesAssetsDic.Keys)
            {
                ExportSceneAssetsRefVO exportSceneAssetsRefVO = allScenesAssetsDic[scenePathKey];
                foreach (string key in exportSceneAssetsRefVO.assetsMap.Keys)
                {
                    List<string> fileList = exportSceneAssetsRefVO.assetsMap[key];
                    int t = fileList.Count;
                    for (int j = t - 1; j > -1; j--)
                    {
                        string filePath = fileList[j];
                        if (assetsReferenceCounter[filePath] > 1)
                        {
                            fileList.RemoveAt(j);
                        }
                    }

                    if (fileList.Count > 0)
                    {
                        AssetBundleBuild build = new AssetBundleBuild();
                        string assetBundleName = exportSceneAssetsRefVO.saveScenePath + "/" + key + PathDefine.U3D;
                        build.assetBundleName = assetBundleName;
                        build.assetNames = fileList.ToArray();
                        builds.Add(build);
                    }
                }
            }


            try
            {
                string exportPlatformRootPath = Path.Combine(exportPrefabToPath,
                    buildTarget.ToString() + "/" + rootFolderName);
                if (Directory.Exists(exportPlatformRootPath) == false)
                {
                    Directory.CreateDirectory(exportPlatformRootPath);
                }

                BuildAssetBundleOptions options = BuildAssetBundleOptions.None |
                                                  BuildAssetBundleOptions.DeterministicAssetBundle |
                                                  BuildAssetBundleOptions.StrictMode;
                if (lz4Compress == true)
                {
                    options = options | BuildAssetBundleOptions.ChunkBasedCompression;
                }

                if (isForceRebuild == true)
                {
                    options = options | BuildAssetBundleOptions.ForceRebuildAssetBundle;
                }
                ///不行 有些类型就会找不到 unity操蛋的机制
                /*if (isDisableWriteTypeTree)
                {
                    options = options | BuildAssetBundleOptions.DisableWriteTypeTree;
                }*/

                BuildPipeline.BuildAssetBundles(exportPlatformRootPath, builds.ToArray(),
                    options, buildTarget);

            }
            catch (Exception e)
            {
                Debug.Log("失败:" + e.Message);
                return false;
            }
            finally
            {
                clearCache();
            }

            return true;
        }

        private string fileNameFormat(string fileName)
        {
            fileName = fileName.Replace(" ", "");
            fileName = fileName.Replace('#', '_');
            fileName = fileName.Replace('@', '_');
            fileName = fileName.Replace('(', '_');
            fileName = fileName.Replace(')', '_');
            return fileName;
        }

        private void clearCache()
        {
            foreach (HashSet<string> hashSet in toUnityPathMapping.Values)
            {
                hashSet.Clear();
            }

            assetsReferenceCounter.Clear();
            assetBundleReferences.Clear();
            cacheGetDependenciesMap.Clear();
            allScenesAssetsDic.Clear();
        }

        protected virtual string getBuildTargetName(BuildTarget buildTarget)
        {
            return buildTarget.ToString();
        }

        private void getInnerPrefabDepend(string outerPrefabPath, string[] outerDepends,
            HashSet<string> inInnerPrefabAssets)
        {
            foreach (string depend in outerDepends)
            {
                if (depend == outerPrefabPath)
                {
                    continue;
                }

                string extension = Path.GetExtension(depend).ToLower();
                if (extension == ".prefab")
                {
                    string[] innerDepends = GetDependencies(depend);
                    foreach (string innerDepend in innerDepends)
                    {
                        if (innerDepend != depend)
                        {
                            inInnerPrefabAssets.Add(innerDepend);
                        }
                    }
                }
            }
        }

        private Dictionary<string, string[]> cacheGetDependenciesMap = new Dictionary<string, string[]>();

        public string[] GetDependencies(string prefab)
        {
            string[] result = null;
            if (cacheGetDependenciesMap.TryGetValue(prefab, out result) == false)
            {
                result = AssetDatabase.GetDependencies(new string[] {prefab});
                cacheGetDependenciesMap.Add(prefab, result);
            }

            return result;
        }

        //private ExportSceneAssetsRefVO _currentProcessSceneAssets;
        protected List<AssetBundleBuild> collectionPrefab(ExportPrefabRefVO exportPrefabRefVO)
        {
            string prefabPath = exportPrefabRefVO.path;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(prefabPath).Trim();
            fileNameWithoutExtension = fileNameFormat(fileNameWithoutExtension);
            string fileExtension = Path.GetExtension(prefabPath).ToLower();

            //是否是个scene场景;
            bool isScene = (fileExtension == ".unity");
            /*_currentProcessSceneAssets = null;
            if (isScene)
            {
                if (allScenesAssetsDic.TryGetValue(prefabPath, out _currentProcessSceneAssets) == false)
                {
                    _currentProcessSceneAssets = new ExportSceneAssetsRefVO(fileNameWithoutExtension);
                    _currentProcessSceneAssets.exportPrefabRefVO = exportPrefabRefVO;
                    allScenesAssetsDic.Add(prefabPath, _currentProcessSceneAssets);
                }
            }*/
            string[] depends = GetDependencies(prefabPath);
            toRemovesSet.Clear();
            //收集嵌套的prefab;
            getInnerPrefabDepend(prefabPath, depends, toRemovesSet);

            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            foreach (string depend in depends)
            {
                if (toRemovesSet.Contains(depend))
                {
                    continue;
                }

                string extension = Path.GetExtension(depend).ToLower();
                switch (extension)
                {
                    case ".jpg":
                    case ".png":
                    case ".psd":
                    case ".tif":
                    case ".dds":
                    case ".tga":
                    case ".ttf":
                    case ".cubemap":
                        addDepend(BITMAPS, depend, prefabPath);
                        break;
                    case ".wav":
                    case ".mp3":
                    case ".mp4":
                    case ".ogg":
                        addDepend(SOUNDS, depend, prefabPath);
                        break;
                    case ".exr":
                        //独立掉这个会丢失lightmap
                        break;
                    case ".shader":
                        addDepend(SHADERS, depend, prefabPath);
                        break;
                    case ".mat":
                        ///材质只加入到通用列表里面(区分一个包名,两个打包项的同名同路径在unity里面认为是同一个文件)
                        addDepend(MATERIALS, depend, prefabPath);
                        break;
                    case ".dll":
                    case ".cs":
                        break;
                    case ".obj":
                        if (isScene == false)
                        {
                            addDepend(OBJS, depend, prefabPath);
                        }
                        break;
                    case ".fbx":
                        if (isScene == false)
                        {
                            addDepend(FBXS, depend, prefabPath);
                        }
                        break;
                    case ".anim":
                        addDepend(ANIMS, depend, prefabPath);
                        break;
                    case ".playable":
                        addDepend(PLAYABLES, depend, prefabPath);
                        break;
                    case ".controller":
                        addDepend(CONTROLLERS, depend, prefabPath);
                        break;
                    case ".asset":
                        break;
                    case ".prefab":
                    case ".unity":
                        //不用加进去;
                        //addDepend(PREFABS, depend, prefabPath);
                        break;
                    default:
                        break;
                }
            }

            string fileName = fileNameWithoutExtension + PathDefine.U3D;
            string folder = exportPrefabRefVO.exportRootVo.name;
            if (string.IsNullOrEmpty(folder) == false)
            {
                fileName = folder + "/" + fileName;
            }

            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = fileName;
            build.assetNames = new string[] {prefabPath};
            builds.Add(build);

            assetBundleReferences.Add(prefabPath, build);

            return builds;
        }

        private void addDepend(string key, string depend, string prefabPath)
        {
            int v;
            if (assetsReferenceCounter.TryGetValue(depend, out v))
            {
                assetsReferenceCounter[depend] = v + 1;
            }
            else
            {
                assetsReferenceCounter[depend] = 1;
            }

            /*if (_currentProcessSceneAssets!=null)
            {
                _currentProcessSceneAssets.add(key, depend);
                return;
            }*/

            HashSet<string> hashSet = null;
            if (toUnityPathMapping.TryGetValue(key, out hashSet) == false)
            {
                hashSet = toUnityPathMapping[OTHERS];
            }

            hashSet.Add(depend);
            //DebugX.Log(depend + ":" + prefabPath);
        }
    }
}