using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using foundationEditor;
using foundationExport;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 不能添加命名空间,被其它自动发布程序调用了
/// </summary>
public class ProjectBuildSettings
{
    private static Dictionary<string, BuildTarget> platfromDictionary = new Dictionary<string, BuildTarget>();
    private static Dictionary<string, BuildTargetGroup> platfromGroupDictionary = new Dictionary<string, BuildTargetGroup>();

    private static void autoInit()
    {
        platfromDictionary.Add("android", BuildTarget.Android);
        platfromDictionary.Add("ios", BuildTarget.iOS);

        platfromGroupDictionary.Add("android", BuildTargetGroup.Android);
        platfromGroupDictionary.Add("ios", BuildTargetGroup.iOS);
    }

    /// <summary>
    /// 不能改名,被其它自动发布程序调用了
    /// </summary>
    public static void Build()
    {
        autoInit();

        string sceneName = "";
        string releasePath = "";

        BuildTarget buildTarget = BuildTarget.Android;
        BuildTargetGroup targetGroup = BuildTargetGroup.Android;
        string[] args = Environment.GetCommandLineArgs();
        //int len = args.Length;
        if (args.Length < 0)
        {
            return;
        }

        int index;

        index = Array.IndexOf(args, "-scene");
        if (index != -1)
        {
            sceneName = args[index + 1];
            Debug.Log(sceneName);
        }

        index = Array.IndexOf(args, "-buildTarget");
        if (index != -1)
        {
            string buildTargetKey = args[index + 1].ToLower();
            Debug.Log("buildTarget:" + buildTargetKey);
            buildTarget = platfromDictionary[buildTargetKey];
            targetGroup= platfromGroupDictionary[buildTargetKey];
        }

        index = Array.IndexOf(args, "-releasePath");
        if (index != -1)
        {
            releasePath = args[index + 1];
            Debug.Log("releasePath:" + releasePath);
        }

        string displayName = "test";
        index = Array.IndexOf(args, "-displayName");
        if (index != -1)
        {
            displayName = args[index + 1];
            Debug.Log("displayName:" + displayName);
        }

        string major = "0";
        index = Array.IndexOf(args, "-major");
        if (index != -1)
        {
            major = args[index + 1];
            Debug.Log("major:" + major);
        }

        int minor = 0;
        index = Array.IndexOf(args, "-minor");
        if (index != -1)
        {
            int.TryParse(args[index + 1], out minor);
            Debug.Log("minor:" + major);
        }

        string bundleIdentifier = "local";
        index = Array.IndexOf(args, "-bundleIdentifier");
        if (index != -1)
        {
            bundleIdentifier = args[index + 1];
            Debug.Log("bundleIdentifier:" + bundleIdentifier);
        }

        index = Array.IndexOf(args, "-bundleIdentifierIsFull");
        if (index == -1)
        {
            bundleIdentifier = "com.lingyu." + bundleIdentifier;
        }

        string signFile = "";
        index = Array.IndexOf(args, "-signFile");
        if (index != -1)
        {
            signFile = args[index + 1];
            Debug.Log("signFile:" + signFile);
        }

        bool isDebug = true;
        index = Array.IndexOf(args, "-isDebug");
        if (index != -1)
        {
            isDebug = args[index + 1] == "1";
            Debug.Log("isDebug:" + isDebug);
        }

        switchToPlatform(targetGroup,buildTarget);

        SetPlayerSettings(buildTarget, displayName, major, minor, bundleIdentifier, signFile, isDebug);

        BuildOptions buildOption = BuildOptions.None;
        if (isDebug)
        {
            buildOption |= BuildOptions.Development;
            buildOption |= BuildOptions.AllowDebugging;
            buildOption |= BuildOptions.ConnectWithProfiler;
        }

        BuildPipeline.BuildPlayer(new string[] {sceneName}, releasePath, buildTarget, buildOption);
    }

    public static void BuildAssetbundle()
    {
        autoInit();
        BuildTarget buildTarget = BuildTarget.Android;
        BuildTargetGroup targetGroup = BuildTargetGroup.Android;
        string[] args = Environment.GetCommandLineArgs();
        //int len = args.Length;
        if (args.Length < 0)
        {
            return;
        }
        int index = Array.IndexOf(args, "-buildTarget");
        if (index != -1)
        {
            string buildTargetKey = args[index + 1].ToLower();
            Debug.Log("buildTarget:" + buildTargetKey);
            buildTarget = platfromDictionary[buildTargetKey];
            targetGroup = platfromGroupDictionary[buildTargetKey];
        }

        bool isForceRebuild = false;
        index = Array.IndexOf(args, "-isForceRebuild");
        if (index != -1)
        {
            isForceRebuild = true;
        }

        bool lz4Compress = false;
        index = Array.IndexOf(args, "-lz4");
        if (index != -1)
        {
            lz4Compress = true;
        }

        string exportPrefabToPrefix = "";
        switchToPlatform(targetGroup, buildTarget);
        
        XmlDocument doc = EditorConfigUtils.doc;
        XmlNode node = doc.SelectSingleNode("config/prefabExport");
        XmlAttribute nodeAttribute = node.Attributes["to"];
        if (nodeAttribute == null)
        {
            FileInfo fileInfo = new FileInfo(Application.dataPath);
            exportPrefabToPrefix = Path.Combine(fileInfo.Directory.FullName, "ReleaseResource/");
        }
        else
        {
            string[] paths = nodeAttribute.InnerText.Split(',');
            foreach (string s in paths)
            {
                if (Directory.Exists(s))
                {
                    exportPrefabToPrefix = s;
                    break;
                }
            }
        }

        index = Array.IndexOf(args, "-exportTo");
        if (index != -1)
        {
            string value = args[index + 1];
            if (string.IsNullOrEmpty(value)==false)
            {
                exportPrefabToPrefix = value;
            }
        }

        Dictionary<string, ExportRootVO> mapList = new Dictionary<string, ExportRootVO>();
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
        string rootFolderName = node.Attributes["zipName"].InnerText;
        PrefabExport prefabExport = new PrefabExport(buildTarget, exportPrefabToPrefix, rootFolderName);
        prefabExport.isForceRebuild = isForceRebuild;
       
        if (node.Attributes["lz4"].InnerText == "1" || lz4Compress)
        {
            prefabExport.lz4Compress = true;
        }

        prefabExport.exportAllPrefab(mapList.Values.ToList());
    }

    protected static void switchToPlatform(BuildTargetGroup targetGroup, BuildTarget targetPlatform)
    {
        if (EditorUserBuildSettings.activeBuildTarget != targetPlatform)
        {
            Debug.Log("自动切换至 " + targetPlatform + " 平台.");
            EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup,targetPlatform);
        }
    }

    public static void SetPlayerSettings(BuildTarget buildTarget, string displayName, string bundleVersion,
        int buildNumber,
        string bundleIdentifier, string signFile, bool isDebug = false)
    {

        int currentLevel = QualitySettings.GetQualityLevel();
        if (currentLevel == 0)
        {
            QualitySettings.SetQualityLevel(3);
        }

        BuildTargetGroup targetGroup = BuildTargetGroup.Android;
        if (buildTarget == BuildTarget.Android)
        {
            targetGroup = BuildTargetGroup.Android;
            PlayerSettings.productName = displayName;

            if (string.IsNullOrEmpty(signFile))
            {
                signFile = "D:/web/Builder/cert/mxwy.keystore";
            }
            PlayerSettings.bundleVersion = bundleVersion + "." + buildNumber;
            PlayerSettings.applicationIdentifier = bundleIdentifier;

            PlayerSettings.Android.bundleVersionCode = buildNumber;
            PlayerSettings.Android.keystoreName = signFile;
            PlayerSettings.Android.keyaliasName = "mxwy";
            PlayerSettings.Android.keyaliasPass = "123456";
            PlayerSettings.Android.keystorePass = "123456";
        }
        else if (buildTarget == BuildTarget.iOS)
        {
            targetGroup = BuildTargetGroup.iOS;
            PlayerSettings.bundleVersion = bundleVersion;
            PlayerSettings.iOS.buildNumber = buildNumber.ToString();
            PlayerSettings.iOS.applicationDisplayName = displayName; // iOS中应用程序的显示名称。
        }
        
        PlayerSettings.SetApplicationIdentifier(targetGroup, bundleIdentifier);

        if (isDebug)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "DEBUG");
        }
        else
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "RELEASE");
        }
    }
}