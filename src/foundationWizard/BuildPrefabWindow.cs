using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using foundation;
using foundationExport;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace foundationEditor
{
    public class BuildPrefabWindow : ScriptableWizard
    {
        private static string EditorPrefs_Key = "prefabExport_Selected";
        public static string svnExe = "TortoiseProc.exe";
       
        public string exportPrefabToPrefix;
        private Dictionary<string,ExportRootVO> mapList=new Dictionary<string, ExportRootVO>();
        private bool hasSVN = true;
        public string rootFolderName;
        public bool lz4Compress = false;
        public bool isForceRebuild = false;

        public BuildTarget buildTarget = BuildTarget.Android;
        private int selectedIndex = 0;


        [MenuItem("Tools/打包Assetboundle")]
        public static void ExportSelectionToAssetboundle()
        {
            ScriptableWizard.DisplayWizard("选择发布资源的平台类型", typeof(BuildPrefabWindow), "确定");
        }

        public BuildPrefabWindow()
        {
        }

        protected void OnEnable()
        {
            string path = Application.dataPath + "/Editor/config.xml";
            if (File.Exists(path) == false)
            {
                ShowNotification(new GUIContent(path + "不存在"));
                return;
            }

            XmlDocument doc=EditorConfigUtils.doc;
            XmlNode node = doc.SelectSingleNode("config/prefabExport");
            XmlAttribute nodeAttribute = node.Attributes["to"];
            if (nodeAttribute == null)
            {
                FileInfo fileInfo = new FileInfo(Application.dataPath);
                exportPrefabToPrefix = Path.Combine(fileInfo.Directory.FullName, "ReleaseResource/");
            }
            else
            {
                bool has = false;
                string[] paths = nodeAttribute.InnerText.Split(',');
                foreach (string s in paths)
                {
                    if (Directory.Exists(s))
                    {
                        has = true;
                        exportPrefabToPrefix = s;
                        break;
                    }
                    else
                    {
                        string[] sections = s.Split('/');
                        if (sections.Length > 1 && sections[0] == "~")
                        {
                            string subfix=sections.As3Join("/",1,sections.Length);
                            FileInfo fileInfo = new FileInfo(Application.dataPath);
                            string fullPath=Path.Combine(fileInfo.Directory.Parent.FullName, subfix);
                            if (Directory.Exists(fullPath))
                            {
                                has = true;
                                exportPrefabToPrefix = fullPath;
                                break;
                            }
                        }
                    }
                }

                if (has == false)
                {
                    FileInfo fileInfo = new FileInfo(Application.dataPath);
                    exportPrefabToPrefix = Path.Combine(fileInfo.Directory.FullName, "ReleaseResource/");
                }
            }

            XmlAttribute attribute = node.Attributes["hasSVN"];
            hasSVN = true;
            if (attribute != null && attribute.InnerText == "0")
            {
                hasSVN = false;
            }

            rootFolderName = node.Attributes["zipName"].InnerText;
            if (node.Attributes["lz4"].InnerText == "1")
            {
                lz4Compress = true;
            }
            

            mapList.Clear();
            foreach (XmlNode itemNode in node.ChildNodes)
            {
                ExportRootVO itemVo = new ExportRootVO();
                string name = itemNode.Attributes["name"].InnerText;
                if (mapList.ContainsKey(name))
                {
                    continue;
                }
                itemVo.bindXML(itemNode);
                mapList.Add(name, itemVo);
            }

            selectedIndex = EditorPrefs.GetInt(EditorPrefs_Key);

            this.titleContent = new GUIContent("发布资源设置");

            if (SystemInfo.operatingSystem.IndexOf("Mac") != -1)
            {
                buildTarget = BuildTarget.iOS;
            }
            else
            {
                buildTarget = EditorUserBuildSettings.activeBuildTarget;
            }

            if (hasSVN)
            {
                startSVNUpdates(buildTarget);
            }
        }

        private void startSVNUpdates(BuildTarget buildTarget)
        {
            string cmd = string.Format("/command:update /path:{0}", getFullBuildPath());
            System.Diagnostics.Process.Start(svnExe, cmd);
        }

        private string getFullBuildPath()
        {
            string path = exportPrefabToPrefix;
            if (path.EndsWith("/") == false)
            {
                path += "/";
            }
            return Path.Combine(path, buildTarget.ToString() + "/" + rootFolderName);
        }

        private void startSVNCommit(BuildTarget buildTarget)
        {
            string cmd = string.Format("/command:commit /path:{0}", exportPrefabToPrefix);
            System.Diagnostics.Process.Start(svnExe, cmd);
        }

        protected override bool DrawWizardGUI()
        {
            foreach (string name in mapList.Keys)
            {
                ExportRootVO itemVo = mapList[name];
                EditorGUILayout.TextField(itemVo.name,itemVo.@from.As3Join(",")+" "+ itemVo.extentions.As3Join(","));
            }
            return base.DrawWizardGUI();
        }

        protected virtual void OnWizardCreate()
        {
            if (Directory.Exists(exportPrefabToPrefix) == false)
            {
                EditorUtility.DisplayDialog("导出", "请配置Prefab to目录", "确定");
                return;
            }

            if (switchToPlatform(buildTarget))
            {
                startProgress();
            }
        }

        protected void startProgress()
        {
            PrefabExport prefabExport;
            prefabExport = new PrefabExport(buildTarget, exportPrefabToPrefix, rootFolderName);
            prefabExport.isForceRebuild = isForceRebuild;
            prefabExport.lz4Compress = lz4Compress;

            prefabExport.exportAllPrefab(mapList.Values.ToList());

            if (hasSVN)
            {
               startSVNCommit(buildTarget);
            }
        }

        //private bool isProgress = false;
        protected bool switchToPlatform(BuildTarget targetPlatform)
        {
            BuildTargetGroup targetGroup = BuildTargetGroup.Android;
            if (EditorUserBuildSettings.activeBuildTarget != targetPlatform)
            {
                Debug.Log("自动切换至 " + targetPlatform + " 平台.");

                if (targetPlatform == BuildTarget.iOS)
                {
                    targetGroup = BuildTargetGroup.iOS;
                }
                ActiveBuildTargetChange.StartCallBack(startProgress);
                EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup,targetPlatform);
              
                return false;
            }
            return true;
        }

        public class ActiveBuildTargetChange : IActiveBuildTargetChanged
        {
            public int callbackOrder
            {
                get { return 0; }
            }


            private static Action callBack;

            public static void StartCallBack(Action value)
            {
                callBack = value;
            }

            private static void activeBuildTargetChanged()
            {
                Action fun = callBack;
                if (fun != null)
                {
                    callBack = null;
                    fun();
                }
            }

            public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
            {
                Action fun = callBack;
                if (fun!=null)
                {
                    callBack = null;
                    fun();
                }
            }
        }
    }
}
