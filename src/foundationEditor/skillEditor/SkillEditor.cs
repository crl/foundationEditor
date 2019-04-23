using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using foundation;
using gameSDK;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace foundationEditor
{
    public class SkillEditor : BaseEditorWindow
    {
        static string scenePath = "Assets/Scenes/SkillEditor.unity";

        [SerializeField]
        private int tabSelectedIndex = 0;
        [SerializeField]
        private int modelSelectedIndex;

        private string resourcePath;
        [SerializeField]
        private ModelWindow modelWindow;
        [SerializeField]
        private ModelWindow effectWindow;
        [SerializeField]
        private ModelWindow soundWindow;
        private TimeWindow timeWindow;

        private PropertyWindow propertyWindow;
        [SerializeField]
        private EditorTabNav tabNav;
        protected Vector3 bornPosition;

        private float rotationY = 0;
        public Type baseAppType;
        public EditorFormItem autoMononType;

        public string autoMononTypeName = "";

        [MenuItem("App/SkillEditor",false,1)]
    
        public static void TSkillEditor()
        {
            GetWindow<SkillEditor>();
        }

        public SkillEditor() : base(false)
        {
            bornPosition=Vector3.zero;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            AssetsManager.routerResourceDelegate = routerResourceDelegate;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            AssetsManager.routerResourceDelegate = null;
        }

        protected AssetResource routerResourceDelegate(string url, string uri, string prefix)
        {
            string fileName=Path.GetFileNameWithoutExtension(uri);

            ResourceVO resourceVO=DataSource.GetResourceVO(prefix, fileName);
            AssetResource resource=null;
            if (resourceVO != null)
            {
                resource=AssetsManager.getResource(resourceVO.itemPath, LoaderXDataType.EDITORRESOURCE);
            }
            return resource;
        }

        protected override void initialization()
        {
            base.initialization();

            BaseRigsterUtils.init();

            this.titleContent = new GUIContent("技能");

            EditorConfigUtils.load();
            resourcePath= EditorConfigUtils.ProjectResource;
            resourcePath = resourcePath.Replace("\\", "/");

            string basePrefabPath = "";
            PathDefine.effectPath = "file:///" + basePrefabPath;
            PathDefine.avatarPath = "file:///" + basePrefabPath;
            PathDefine.scenePath = "file:///" + basePrefabPath;
            PathDefine.soundPath = "file:///" + basePrefabPath;

            EditorBox vbox = new EditorBox();
            vbox.widthOption = GUILayout.Width(300);
            vbox.styleString = "box";
        
            tabNav = new EditorTabNav();
            tabNav.addEventListener(EventX.CHANGE, tabNavHandle);
            modelWindow = new ModelWindow();
            modelWindow.addEventListener(EventX.SELECT, modelSelectHandle);
            tabNav.addItem("avatar", modelWindow);

            effectWindow = new ModelWindow();
            effectWindow.addEventListener(EventX.SELECT, effectSelectHandle);
            tabNav.addItem("effect", effectWindow);

            soundWindow = new ModelWindow();
            soundWindow.exNameArr = new[] {"*.mp3", "*.ogg", "*.wav"};
            soundWindow.addEventListener(EventX.SELECT, soundSelectHandle);
            tabNav.addItem("sound", soundWindow);


            EditorBox box = new EditorBox(false);
            EditorButton btn;
            btn = new EditorButton("reload");
            btn.addEventListener(EventX.ITEM_CLICK, reload);
            box.addChild(btn);


            btn = new EditorButton("updateSVN");
            btn.addEventListener(EventX.ITEM_CLICK, updateSVN);
            box.addChild(btn);

            btn = new EditorButton("editor");
            btn.addEventListener(EventX.ITEM_CLICK, editor);
            box.addChild(btn);
            autoMononType = new EditorFormItem("自动代码");
            autoMononType.addEventListener(EventX.CHANGE, autoMononTypeHandle);
            autoMononType.value = autoMononTypeName;

            vbox.addChild(tabNav);
            vbox.addChild(new EditorFlexibleSpace());
            vbox.addChild(autoMononType);
            vbox.addChild(box);

            btn = new EditorButton("打包Assetbundle");
            btn.addEventListener(EventX.ITEM_CLICK, assetbundleClickHandle);
            vbox.addChild(btn);

            addChild(vbox);

            propertyWindow = new PropertyWindow();
            propertyWindow.widthOption = GUILayout.Width(300);
            timeWindow = new TimeWindow();
            timeWindow.addEventListener(EventX.SELECT, timeLineSelectHandle);
            timeWindow.init(resourcePath + "All/skill/", propertyWindow);

            addChild(timeWindow);

            addChild(propertyWindow);

            reload(null);

            tabNav.selectedIndex = tabSelectedIndex;
            modelWindow.selectedIndex = modelSelectedIndex;
        }
        private void assetbundleClickHandle(EventX e)
        {
            ScriptableWizard.DisplayWizard("选择发布资源的平台类型", typeof(BuildPrefabWindow), "确定");
        }
        private void tabNavHandle(EventX e)
        {
            tabSelectedIndex = tabNav.selectedIndex;
        }

        private string skillProjectDirectory;
        protected void reload(EventX e)
        {
            skillProjectDirectory = EditorConfigUtils.GetProjectResource("All/skill/");
            string editorPrefabPath = "Assets/Prefabs";
            modelWindow.init(editorPrefabPath, "avatar");
            effectWindow.init(editorPrefabPath, "effect");
            soundWindow.init(editorPrefabPath, "sound");
        }

        private void updateSVN(EventX e)
        {
            string cmd = string.Format("/command:update /path:{0}", skillProjectDirectory);
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(EditorConfigUtils.svnExe, cmd);
            process.EnableRaisingEvents = true;
            process.Start();
        }

        private void editor(EventX e)
        {
            GetWindow<FBXWindow>();
        }

        private void autoMononTypeHandle(EventX e)
        {
            autoMononTypeName = (string)e.data;
        }

        private ResourceVO _selectResourceVo;
        private BaseObject modelBaseObject;

        private void modelSelectHandle(EventX e)
        {
            _selectResourceVo = e.data as ResourceVO;
            if (_selectResourceVo == null)
            {
                return;
            }
            modelSelectedIndex = modelWindow.selectedIndex;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_selectResourceVo.itemPath);
            EditorGUIUtility.PingObject(prefab);

            if (EditorApplication.isPlayingOrWillChangePlaymode == false)
            {
                EditorApplication.isPlaying = true;
                return;
            }
            if (modelBaseObject != null && modelBaseObject.gameObject != null)
            {
                GameObject.DestroyImmediate(modelBaseObject.gameObject);
                modelBaseObject = null;
            }
            GameObject container = BaseApp.ActorContainer;
            if (container == null)
            {
                return;
            }
            int len = container.transform.childCount;
            for (int i = len - 1; i > -1; i--)
            {
                Transform t = container.transform.GetChild(i);
                DestroyImmediate(t.gameObject);
            }

            GameObject go = new GameObject(_selectResourceVo.fileName);
            modelBaseObject = go.AddComponent<BaseObject>();
            modelBaseObject.transform.SetParent(BaseApp.ActorContainer.transform);
            modelBaseObject.rotationY = rotationY;
            modelBaseObject.position = bornPosition;

            if (prefab == null)
            {
                return;
            }

            GameObject skin= GameObject.Instantiate(prefab);
            skin.transform.localPosition=Vector3.zero;
            skin.transform.localRotation=Quaternion.identity;

            modelBaseObject.skin = skin;
            modelBaseObject.fireReadyEvent();

            Animator _animator = modelBaseObject.getAnimator();
            HashSet<string> animationName = new HashSet<string>();
            List<string> animationParmsName = new List<string>();
            if (_animator != null)
            {
                AnimatorOverrideController runtimeAnimatorController = _animator.runtimeAnimatorController as AnimatorOverrideController;
                if (runtimeAnimatorController)
                {
                    List<KeyValuePair<AnimationClip , AnimationClip>> lst = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                    runtimeAnimatorController.GetOverrides(lst);
                    foreach (KeyValuePair<AnimationClip , AnimationClip> item in lst)
                    {
                        if (item.Key == null)
                        {
                            continue;
                        }
                        animationName.Add(item.Key.name);
                    }
                }

                foreach (AnimatorControllerParameter animatorParameter in _animator.parameters)
                {
                    animationParmsName.Add(animatorParameter.name);
                }
            }
           
            DataSource.Add(DataSource.ANIMATION, animationName.ToList(), true);
            DataSource.Add(DataSource.ANIMATION_PARMS, animationParmsName, true);

            List<string> boneNames = new List<string>();
            SkinnedMeshRenderer[] skinnedMeshRenderers = skin.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                foreach (Transform bone in skinnedMeshRenderer.bones)
                {
                    boneNames.Add(bone.name);
                }
            }
            DataSource.Add(DataSource.BONE, boneNames, true);

            Selection.activeGameObject = go;
            if (BaseApp.cameraController != null)
            {
                BaseApp.cameraController.setFollow(modelBaseObject);
            }

            if (string.IsNullOrEmpty(autoMononTypeName) == false)
            {
                Type type = foundation.ObjectFactory.Locate(autoMononTypeName);
                if (type == null)
                {
                    type = typeof(AnimatorControlerApp);
                }

                Component b = go.GetComponentInChildren(type);
                if (b == null)
                {
                    b = go.AddComponent(type);
                }
            }

            timeWindow.selectedbaseObject = modelBaseObject;
            timeWindow.searchSkillListBy(_selectResourceVo.fileName);
        }

        private BaseObject effectBaseObject;
        private void effectSelectHandle(EventX e)
        {
            ResourceVO resourceVo = e.data as ResourceVO;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(resourceVo.itemPath);
            EditorGUIUtility.PingObject(prefab);
            if (EditorApplication.isPlaying == false)
            {
                return;
            }

            if (effectBaseObject != null)
            {
                GameObject.DestroyImmediate(effectBaseObject.gameObject);
            }
        
            GameObject go = new GameObject(resourceVo.fileName);
            effectBaseObject = go.AddComponent<BaseObject>();
            effectBaseObject.transform.SetParent(BaseApp.EffectContainer.transform);
            effectBaseObject.rotationY = rotationY;

            GameObject _skin = GameObject.Instantiate(prefab);
            _skin.transform.localPosition = Vector3.zero;

            effectBaseObject.skin = _skin;
            effectBaseObject.fireReadyEvent();
            Selection.activeGameObject = go;
        }

        private void soundSelectHandle(EventX e)
        {
            ResourceVO resourceVo = e.data as ResourceVO;
            SoundClip prefab = AssetDatabase.LoadAssetAtPath<SoundClip>(resourceVo.itemPath);
            EditorGUIUtility.PingObject(prefab);
        }

        private void timeLineSelectHandle(EventX e)
        {
            SkillPointVO data=e.data as SkillPointVO;
            if (data != null)
            {
                propertyWindow.show(data);
            }
        }
       
        protected override void OnGUI()
        {
            if (laterGUICounter-- > 0)
            {
                return;
            }
            Scene scene = EditorSceneManager.GetActiveScene();
            bool isCurrent = scene.path == scenePath;
            if (isCurrent == false)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    laterGUICounter = 4;
                    EditorApplication.isPlaying = false;
                    return;
                }

                if (File.Exists(scenePath))
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
                else
                {
                    string label ="not exists:" + scenePath;
                    Vector2 size = new GUIStyle("label").CalcSize(new GUIContent(label));
                    Rect titleRect = new Rect(0, 0, size.x, size.y);
                    titleRect.center = new Vector2(Screen.width / 2, (Screen.height / 2) - size.y);
                    GUI.Label(titleRect, label);
                }
                return;
            }

            base.OnGUI();
        }
    }
}