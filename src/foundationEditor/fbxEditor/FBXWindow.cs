using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using foundation;
using foundationEditor;
using UnityEngine;
using System.Diagnostics;
using UnityEditor.Animations;

namespace UnityEditor
{
    public class FBXWindow : BaseEditorWindow
    {
        public const string EditorPrefs_Key = "fbxEditor_";
        private string SaveTabKey = EditorPrefs_Key + "tab";

        private PreviewSystem previewSystem;
        public static string svnExe = "TortoiseProc.exe";
        private string[] loopAnimationClipNames;
        private EditorTabNav tabNav;
        public FBXWindow()
        {
            this.titleContent = new GUIContent("FBXAutoTool");
            tabNav = new EditorTabNav();
            tabNav.addEventListener(EventX.CHANGE, tabNavHandle);
        }
        private void tabNavHandle(EventX obj)
        {
            EditorPrefs.SetInt(SaveTabKey, tabNav.selectedIndex);
        }

        protected override void initialization()
        {
            base.initialization();

            previewSystem = new PreviewSystem();
            previewSystem.parentEditorWindow = this;

            reload();
        }

        [MenuItem("App/FBXAutoTool %f", false,4)]
        public static void FBXEditor()
        {
            GetWindow<FBXWindow>();
        }

        private void itemEventHandle(string eventType, IListItemRender itemRender,object data)
        {
            FBXInfo info = data as FBXInfo;
            if (info == null)
            {
                return;
            }
            if (eventType == EventX.SELECT)
            {
                if (File.Exists(info.prefabPath))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(info.prefabPath);
                    if (prefab != null)
                    {
                        viewPrefab(prefab);
                        return;
                    }
                }
                else
                {
                    fbxCreatePrefab(info);
                }
            }
            else if (eventType == EventX.ADDED)
            {
                fbxCreatePrefab(info);
            }
        }

        private void fbxCreatePrefab(FBXInfo fbxInfo)
        {
            DirectoryInfo info = new DirectoryInfo(fbxInfo.rawFolder);
            List<string> fbxsList = new List<string>();

            if (Directory.Exists(fbxInfo.fbxFolder) == false)
            {
                Directory.CreateDirectory(fbxInfo.fbxFolder);
            }
            if (Directory.Exists(fbxInfo.prefabFolder) == false)
            {
                Directory.CreateDirectory(fbxInfo.prefabFolder);
            }
            bool hasModel = false;
            foreach (FileInfo fileInfo in info.GetFiles())
            {
                if (fileInfo.Extension.ToUpper() == ".FBX")
                {
                    string fbxName = fileInfo.Name;
                    string fbxPath;
                    if (fbxName == fbxInfo.fileName + fileInfo.Extension)
                    {
                        hasModel = true;
                        fbxPath = fbxInfo.fbxFolder + fbxName;
                        fileInfo.CopyTo(fbxPath, true);
                        continue;
                    }

                    if (fbxInfo.type == FBXType.MODEL)
                    {
                        continue;
                    }

                    if (fileInfo.Name.IndexOf(fbxInfo.fileName + "@") == -1)
                    {
                        fbxName = fbxInfo.fileName + "@" + fbxName;
                    }
                    fbxPath = fbxInfo.fbxFolder + fbxName;
                    fileInfo.CopyTo(fbxPath, true);
                    fbxsList.Add(fbxName);
                }
                else
                {
                    fileInfo.CopyTo(fbxInfo.fbxFolder + fileInfo.Name, true);
                }
            }

            string prefabFbxName = fbxInfo.fileName + ".fbx";
            if (hasModel == false)
            {
                if (fbxsList.Count == 0)
                {
                    ShowNotification(new GUIContent("不存在文件"));
                    return;
                }

                string prefabCopyFBX = fbxInfo.fbxFolder + fbxsList[0];
                string prefabFBX = fbxInfo.fbxFolder + prefabFbxName;
                File.Copy(prefabCopyFBX, prefabFBX, true);
                fbxsList.Add(prefabFbxName);
            }
            else
            {
                fbxsList.Add(prefabFbxName);
            }

            loopAnimationClipNames = EditorConfigUtils.LoopAnimationClipNames;

            AssetDatabase.Refresh();
            List<AnimationClip> animationClipList = new List<AnimationClip>();
            foreach (string fbxName in fbxsList)
            {
                string fbxPath = fbxInfo.fbxFolder + fbxName;
                ModelImporter assetImporter = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
                if (assetImporter == null)
                {
                    continue;
                }

                bool isHuman = fbxInfo.animationType == ModelImporterAnimationType.Human;
                assetImporter.animationType = fbxInfo.animationType;
                if (prefabFbxName == fbxName)
                {
                    assetImporter.importAnimation = false;
                }
                else
                {
                    assetImporter.importAnimation = true;

                    ModelImporterClipAnimation[] clipAnimations = assetImporter.defaultClipAnimations;
                    foreach (ModelImporterClipAnimation modelImporterClipAnimation in clipAnimations)
                    {
                        if (loopAnimationClipNames.Contains(modelImporterClipAnimation.name))
                        {
                            modelImporterClipAnimation.loop = true;
                            modelImporterClipAnimation.wrapMode = WrapMode.Loop;
                        }
                    }
                }
                assetImporter.SaveAndReimport();
                if (assetImporter.importAnimation)
                {
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fbxPath);
                    animationClipList.Add(clip);
                }
            }

            GameObject fbxRawModel = AssetDatabase.LoadAssetAtPath<GameObject>(fbxInfo.fbxFolder + prefabFbxName);
            if (fbxRawModel == null)
            {
                ShowNotification(new GUIContent("模型文件不存在"));
                return;
            }

            string prefabPath = fbxInfo.prefabPath;

            GameObject fbxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (fbxPrefab == null)
            {
                fbxPrefab = PrefabUtility.SaveAsPrefabAsset( fbxRawModel, prefabPath);
            }
            else
            {
                ///已经存在了的
                PrefabCopy2Utility prefabCopyUtility = new PrefabCopy2Utility();
                //PrefabCopyUtility prefabCopyUtility = new PrefabCopyUtility();
                fbxPrefab = prefabCopyUtility.replace(fbxPrefab, fbxRawModel);
            }

            if (fbxInfo.animationType != ModelImporterAnimationType.Legacy)
            {
                Animation animation = fbxPrefab.GetComponent<Animation>();
                if (animation != null)
                {
                    GameObject.DestroyImmediate(animation, true);
                }
                Animator animator = fbxPrefab.GetComponent<Animator>();
                if (animationClipList.Count > 0)
                {
                    if (animator == null)
                    {
                        animator = fbxPrefab.AddComponent<Animator>();
                    }
                    string controllerFullPath = string.Format("{0}default.controller", fbxInfo.fbxFolder);
                    AnimatorController animatorController =AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerFullPath);
                    if (animatorController == null)
                    {
                        animatorController = AnimatorController.CreateAnimatorControllerAtPath(controllerFullPath);
                    }
                    if (animatorController.layers.Length == 0)
                    {
                        animatorController.AddLayer("Base Layer");
                    }
                    AnimatorControllerLayer animatorControllerLayer = animatorController.layers[0];
                    AnimatorState defaultState = animatorControllerLayer.stateMachine.defaultState;

                    foreach (AnimationClip clip in animationClipList)
                    {
                        //需要重新加载不然会崩溃
                        AnimatorState state = AnimatorControllerCreater.addNoExistState(animatorControllerLayer, clip);
                        if (state != null && defaultState == null)
                        {
                            if (state.name.IndexOf("idle") == 0)
                            {
                                defaultState = state;
                                animatorControllerLayer.stateMachine.defaultState = defaultState;
                            }
                        }
                    }

                    if (animator.runtimeAnimatorController == null)
                    {
                        animator.runtimeAnimatorController = animatorController;
                    }

                    AnimatorClipRef animatiorClipRef = fbxPrefab.GetComponent<AnimatorClipRef>();
                    if (animatiorClipRef == null)
                    {
                        animatiorClipRef = fbxPrefab.AddComponent<AnimatorClipRef>();
                    }
                    animatiorClipRef.animationClips = animationClipList.ToArray();
                }
                else if (animator != null)
                {
                    AnimatorClipRef animatiorClipRef = fbxPrefab.GetComponent<AnimatorClipRef>();
                    if (animatiorClipRef == null)
                    {
                        GameObject.DestroyImmediate(animator, true);
                    }
                }
            }
            else
            {
                Animation animation = fbxPrefab.GetComponent<Animation>();
                if (animationClipList.Count > 0)
                {
                    if (animation == null)
                    {
                        animation = fbxPrefab.AddComponent<Animation>();
                    }
                    AnimationUtility.SetAnimationClips(animation, animationClipList.ToArray());
                }
                else
                {
                    if (animation != null)
                    {
                        GameObject.DestroyImmediate(animation, true);
                    }
                }
            }

            UnitCFG cfg = fbxPrefab.GetComponent<UnitCFG>();
            if (cfg == null)
            {
                cfg = fbxPrefab.AddComponent<UnitCFG>();
            }
            EditorUtility.SetDirty(fbxPrefab);
            viewPrefab(fbxPrefab);
            AssetDatabase.Refresh();
        }

        protected override void OnDestroy()
        {
            if (previewSystem != null)
            {
                previewSystem.OnDestroy();
                previewSystem = null;
            }

            base.OnDestroy();
        }

        /// <summary>
        /// 查看模型
        /// </summary>
        /// <param name="prefab"></param>
        private void viewPrefab(GameObject prefab)
        {
            if (prefab != null)
            {
                Selection.activeGameObject = prefab;
                previewSystem.SetPreview(prefab);
                EditorGUIUtility.PingObject(prefab);
            }
        }

        private Dictionary<string, List<FBXInfo>> dataProvider=null;
        private string fbxProjectDirectory;
        public void reload(EventX e=null)
        {
            string v = EditorConfigUtils.GetProjectResource("fbx/");

            if (string.IsNullOrEmpty(v))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo("ReleaseResource/fbx/");
                if (directoryInfo.Exists==false)
                {
                    ShowNotification(new GUIContent("没本配置路径"));
                    return;
                }
                v = directoryInfo.FullName;
            }
            if (Directory.Exists(v) == false)
            {
                ShowNotification(new GUIContent("配置路径不存在:"+v));
                return;
            }

            fbxProjectDirectory = v;

            if (dataProvider == null)
            {
                dataProvider = new Dictionary<string, List<FBXInfo>>();
            }
            else
            {
                dataProvider.Clear();
            }

            ModelImporterAnimationType animationType= ModelImporterAnimationType.Legacy;
            ModelImporterAnimationType foldersAnimationType = ModelImporterAnimationType.Legacy;

            string type= EditorConfigUtils.AnimationType;
            if (type == "Generic")
            {
                animationType = ModelImporterAnimationType.Generic;
            }
            string[] humanoidFolders = EditorConfigUtils.HumanoidFolders;
            string[] parentDirectoryList = Directory.GetDirectories(v);

            foreach (string parentDirectory in parentDirectoryList)
            {
                DirectoryInfo parentDirectoryInfo = new DirectoryInfo(parentDirectory);
                string[] directoryList = Directory.GetDirectories(parentDirectory);
                string parentDirectoryName = parentDirectoryInfo.Name;

                if (humanoidFolders.Contains(parentDirectoryName))
                {
                    foldersAnimationType = ModelImporterAnimationType.Human;
                }
                else
                {
                    foldersAnimationType = animationType;
                }

                List<FBXInfo> list = new List<FBXInfo>();
                FBXInfo fbxInfo = null;
                bool isSingle = false;
                foreach (string directory in directoryList)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    List<FileInfo> fbxFileInfos=new List<FileInfo>();
                    isSingle = false;
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        if (fileInfo.Extension.ToUpper() != ".FBX")
                        {
                            continue;
                        }
                        fbxFileInfos.Add(fileInfo);
                        if (Path.GetFileNameWithoutExtension(fileInfo.Name) == directoryInfo.Name)
                        {
                            isSingle = true;
                            fbxFileInfos.Clear();
                            fbxFileInfos.Add(fileInfo);
                            break;
                        }
                    }

                    foreach (FileInfo fileInfo in fbxFileInfos)
                    {
                        if (fileInfo.Extension.ToUpper() != ".FBX")
                        {
                            continue;
                        }
                        fbxInfo = new FBXInfo();
                        fbxInfo.keys = fileInfo.Directory.Name;
                        fbxInfo.fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                        fbxInfo.rawFolder = directory + "/";
                        fbxInfo.fbxFolder = "Assets/Fbxs/" + parentDirectoryName + "/" + directoryInfo.Name + "/";
                        fbxInfo.prefabFolder = "Assets/Prefabs/" + parentDirectoryName + "/" +fileInfo.Directory.Name + "/";

                        if (isSingle)
                        {
                            fbxInfo.animationType = foldersAnimationType;
                            fbxInfo.type = FBXType.ANIM;
                        }
                        else
                        {
                            fbxInfo.animationType = ModelImporterAnimationType.None;
                            fbxInfo.type = FBXType.MODEL;
                        }
                        list.Add(fbxInfo);
                    }
                }
                dataProvider.Add(parentDirectoryName, list);
            }

            RemoveAllChildren();
            ModelSearchList item;

            tabNav.removeAllChildren();
            foreach (string key in dataProvider.Keys)
            {
                item=new ModelSearchList();
                item.itemEventHandle = itemEventHandle;
                item.dataProvider=dataProvider[key];
                tabNav.addItem(key,item);
            }
            this.addChild(tabNav);
            this.addChild(new EditorFlexibleSpace());

            tabNav.selectedIndex = EditorPrefs.GetInt(SaveTabKey);

            EditorButton btn;

            EditorBox box=new EditorBox(false);
            btn = new EditorButton("reload");
            btn.addEventListener(EventX.ITEM_CLICK, reload);
            box.addChild(btn);

            btn = new EditorButton("commitSVN");
            btn.addEventListener(EventX.ITEM_CLICK, commitSVN);
            box.addChild(btn);

            btn = new EditorButton("updateSVN");
            btn.addEventListener(EventX.ITEM_CLICK, updateSVN);
            box.addChild(btn);


            this.addChild(box);

            btn =new EditorButton("打包Assetbundle");
            btn.addEventListener(EventX.ITEM_CLICK, assetbundleClickHandle);
            this.addChild(btn);
        }

        private void assetbundleClickHandle(EventX e)
        {
            ScriptableWizard.DisplayWizard("选择发布资源的平台类型", typeof (BuildPrefabWindow), "确定");
        }

        private string svnPathList
        {
            get
            {
                List<string> pathList = new List<string>();
                pathList.Add(fbxProjectDirectory);

                //补两个路径
                DirectoryInfo d = new DirectoryInfo("Assets/Fbxs/");
                pathList.Add(d.FullName);
                d = new DirectoryInfo("Assets/Prefabs/");
                pathList.Add(d.FullName);

                //用*号连接文件夹
                return string.Join("*", pathList.ToArray());
            }
        }

        private void updateSVN(EventX e)
        {
            string cmd = string.Format("/command:update /path:{0}", svnPathList);
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(svnExe, cmd);
            process.EnableRaisingEvents = true;
            process.Exited += processExitedHandle;
            process.Start();
        }

        private void commitSVN(EventX e)
        {
            string cmd = string.Format("/command:commit /path:{0}", svnPathList);
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(svnExe, cmd);
            process.EnableRaisingEvents = true;
            process.Exited += processExitedHandle;
            process.Start();
        }

        private void processExitedHandle(object sender, EventArgs e)
        {
            EditorCallLater.Add(laterReload, 1.0f);
        }
        private void laterReload()
        {
            reload(null);
        }

        protected override void OnEnable()
        {
            if (previewSystem != null)
            {
                previewSystem.OnEnable();
            }
            base.OnEnable();
        }
        protected override void OnDisable()
        {
            if (previewSystem!=null)
            {
                previewSystem.OnDisable();
            }
            base.OnDisable();
        }


        protected override void OnGUI()
        {
            _canRepaint = true;
            EditorUI.CheckLostFocus();
            stage.x = (int)position.x;
            stage.y = (int)position.y;
            stage.stageWidth = (int)position.width;
            stage.stageHeight = (int)position.height;

            EditorGUILayout.BeginHorizontal();
             EditorGUILayout.BeginVertical(GUILayout.Width(300));
                stage.onRender();
            EditorGUILayout.EndVertical();

            Rect rect = GUILayoutUtility.GetRect(300, Screen.width, 300, Screen.height);
            previewSystem.DrawRect(rect);

            EditorGUILayout.EndHorizontal();
        }
    }
}