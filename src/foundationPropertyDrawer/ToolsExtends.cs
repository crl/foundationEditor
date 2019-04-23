using System;
using System.IO;
using System.Reflection;
using foundation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ObjectFactory = foundation.ObjectFactory;

namespace foundationEditor
{
    public class ToolsExtends
    {
        public static bool enabledAutoAMF = true;

        [MenuItem("App/ReporterCreate")]
        /*public static void ReporterEditor()
        {
            GameObject go=Selection.activeGameObject;

            if (go == null || go.activeInHierarchy==false)
            {
                return;
            }
            Type reporterType = ObjectFactory.DomainLocate("Reporter");
            if (reporterType == null)
            {
                reporterType = typeof (Reporter);
            }

            Component reporterInstance = go.GetComponent(reporterType);
            if (reporterInstance == null)
            {
                reporterInstance = go.AddComponent(reporterType);
            }
            string path = "Assets/Plugins/Reporter/Logs/";
            if (reporterInstance is Reporter)
            {
                Reporter reporter = reporterInstance as Reporter;
                //reporterObj.AddComponent<TestReporter>();
                Images images=new Images();
                reporter.images = images;
                images.clearImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "clear.png");
                images.collapseImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "collapse.png");
                images.clearOnNewSceneImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "clearOnSceneLoaded.png");
                images.showTimeImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "timer_1.png");
                images.showSceneImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "UnityIcon.png");
                images.userImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "user.png");
                images.showMemoryImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "memory.png");
                images.softwareImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "software.png");
                images.dateImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "date.png");
                images.showFpsImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "fps.png");
                images.showGraphImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + ".png");
                images.graphImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "chart.png");
                images.infoImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "info.png");
                images.searchImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "search.png");
                images.closeImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "close.png");
                images.buildFromImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "buildFrom.png");
                images.systemInfoImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "ComputerIcon.png");
                images.graphicsInfoImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "graphicCard.png");
                images.backImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "back.png");
                images.cameraImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + ".png");
                images.logImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "log_icon.png");
                images.warningImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "warning_icon.png");
                images.errorImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "error_icon.png");
                images.barImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "bar.png");
                images.button_activeImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "button_active.png");
                images.even_logImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "even_log.png");
                images.odd_logImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "odd_log.png");
                images.selectedImage =
                   AssetDatabase.LoadAssetAtPath<Texture2D>(path + "selected.png");

                images.reporterScrollerSkin =
                    (GUISkin) AssetDatabase.LoadAssetAtPath(path + "reporterScrollerSkin.guiskin", typeof (GUISkin));
            }
            else
            {
                MethodInfo methodInfo=reporterInstance.GetType().GetMethod("editorBindUI");
                if (methodInfo != null)
                {
                    methodInfo.Invoke(reporterInstance, new object[] {path});
                }
            }
        }

    */

        [MenuItem("Tools/GetProjectAllGUID")]
        public static void GetProjectAllGUID()
        {
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            
            string path = "Assets/GuidMapping/" + SystemInfo.deviceUniqueIdentifier + "_a.amf";
            SaveGUIDMap guidMap = SaveGUIDMap.Get(path);
            foreach (string assetPath in allAssetPaths)
            {
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                if(string.IsNullOrEmpty(guid))continue;

                guidMap.addGuid(guid, assetPath, false);
            }
            guidMap.save();
        }

        [MenuItem("Tools/AutoTagLayer")]
        public static void AddLayerTag()
        {
            EditorUtils.AddLayer("Terrain");
            EditorUtils.AddLayer("Player");
            EditorUtils.AddLayer("UI3D");

            EditorUtils.AddTag("Terrain");
            EditorUtils.AddTag("Player");
            EditorUtils.AddTag("Avatar");
            EditorUtils.AddTag("Monster");
            EditorUtils.AddTag("Npc");
            EditorUtils.AddTag("Spawn");
            EditorUtils.AddTag("Drop");
            EditorUtils.AddTag("Stairs");


            EditorUtils.AddSortingLayer("Background");
            EditorUtils.AddSortingLayer("Middle");
            EditorUtils.AddSortingLayer("Platform");
            EditorUtils.AddSortingLayer("Npc");
            EditorUtils.AddSortingLayer("Player");
            EditorUtils.AddSortingLayer("Foreground");
        }


        [MenuItem("Assets/FindMiss", false, 0)]
        [MenuItem("Tools/FindMiss")]
        public static void FindMiss()
        {
            foreach (GameObject go  in Selection.gameObjects)
            {
                GameObjectUtils.hasMiss(go);
            }
        }

        [MenuItem("App/GameEditor", false, 31)]
        public static void GameEditor()
        {
           openOrCreateScene("Game",false);
        }

        [MenuItem("App/UIEditor", false, 32)]
        public static void UIEditor()
        {
            openOrCreateScene("UIEditor", false);

            GameObject uiCamera = GameObject.Find("UICamera");
            GameObject uiCanvas;
            if (uiCamera == null)
            {
                uiCamera = new GameObject("UICamera");
                Camera camera = uiCamera.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.Depth;
                camera.orthographic = true;
                camera.orthographicSize = 5;
                camera.nearClipPlane = -10;
                camera.farClipPlane = 100;

                uiCanvas = new GameObject("Canvas");
                uiCanvas.transform.SetParent(uiCamera.transform);

                uiCanvas.AddComponent<RectTransform>();
                Canvas canvas = uiCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.pixelPerfect = true;
                canvas.worldCamera = camera;
                canvas.planeDistance = 10;
                canvas.sortingLayerName = "UI";

                CanvasScaler canvasScaler = uiCanvas.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1080, 1920);
                canvasScaler.referencePixelsPerUnit = 100f;

                uiCanvas.AddComponent<GraphicRaycaster>();
            }
            else
            {
                uiCanvas = GameObject.Find("Canvas");
            }
            GameObject eventSystem = GameObject.Find("EventSystem");
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                sceneView = (SceneView)SceneView.sceneViews[0];
            }
            sceneView.in2DMode = true;
            Selection.activeGameObject = uiCanvas;

            sceneView.FrameSelected();
        }

        private static void openOrCreateScene(string sceneName,bool playIt=false,Type type=null)
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            FileHelper.AutoCreateDirectory("Assets/Scenes/");

            Scene currentScene = EditorSceneManager.GetActiveScene();
            string openSceneName = "Assets/Scenes/"+ sceneName + ".unity";
            if (currentScene.name != openSceneName)
            {
                if (File.Exists(currentScene.path))
                {
                    EditorSceneManager.SaveScene(currentScene);
                }

                if (File.Exists(openSceneName) == false)
                {
                    Scene newScene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
                    if (type != null)
                    {
                        Camera.main.gameObject.AddComponent(type);
                    }
                    EditorSceneManager.SaveScene(newScene,openSceneName);
                }
                else
                {
                    EditorSceneManager.OpenScene(openSceneName);
                }
            }
            if (playIt)
            {
                EditorApplication.isPlaying = true;
            }
        }



        [MenuItem("Assets/FindMiss", true)]
        [MenuItem("Assets/RetrieveMiss", true)]
        public static bool replaceGUIDMappingCheck()
        {
            GameObject gos = Selection.activeGameObject;
            if (gos == null)
            {
                return false;
            }

            bool has = false;
            foreach (GameObject go in Selection.gameObjects)
            {
                if (PrefabUtility.GetPrefabInstanceStatus(go) == PrefabInstanceStatus.NotAPrefab)
                {
                    continue;
                }
                if (GameObjectUtils.hasMiss(go, false) == false)
                {
                    continue;
                }

                has = true;
            }

            return has;
        }

        [MenuItem("Assets/RetrieveMiss", false, 1)]
        public static void replaceGUIDMapping()
        {
            bool isSingle = (Selection.gameObjects.Length==1);
            FindMissHelper findMissHelper = new FindMissHelper();
            foreach (GameObject go in Selection.gameObjects)
            {
                if (go == null)
                {
                    continue;
                }
                if (PrefabUtility.GetPrefabInstanceStatus(go) == PrefabInstanceStatus.NotAPrefab)
                {
                    continue;
                }
                if (GameObjectUtils.hasMiss(go, false) == false)
                {
                    continue;
                }

                findMissHelper.doSingle(go, isSingle);
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("CONTEXT/MonoBehaviour/SelectScript")]
        private static void SelectScript(MenuCommand cmd)
        {
            UnityEngine.Object context = cmd.context;

            MonoScript script=MonoScript.FromMonoBehaviour(context as MonoBehaviour);
            if (script != null)
            {
                EditorGUIUtility.PingObject(script);
            }
            else
            {
                EditorGUIUtility.PingObject(context);
            }
        }

        [MenuItem("CONTEXT/MonoBehaviour/CopyComponentValuesToJson")]
        private static void ScriptToJson(MenuCommand cmd)
        {
            MonoBehaviour monoBehaviour = cmd.context as MonoBehaviour;
            string json=JsonUtility.ToJson(monoBehaviour);
            EditorGUIUtility.systemCopyBuffer = json;
            
        }

        [MenuItem("CONTEXT/MonoBehaviour/ParseComponentValuesFromJson")]
        private static void ScriptFromJson(MenuCommand cmd)
        {
            MonoBehaviour monoBehaviour = cmd.context as MonoBehaviour;
            string json = EditorGUIUtility.systemCopyBuffer;
            if (json != null)
            {
                try
                {
                    JsonUtility.FromJsonOverwrite(json, monoBehaviour);
                }
                catch (Exception ex)
                {
                    Debug.Log("FromJson Error:" + ex);
                }
            }
        }

        [MenuItem("CONTEXT/MonoBehaviour/ToDllScript")]
        public static void ToDllScript(MenuCommand cmd)
        {
            MonoBehaviour monoBehaviour = cmd.context as MonoBehaviour;
            Type type = monoBehaviour.GetType();

            if (type.Assembly.FullName.IndexOf("foundation") > 0)
            {
                return;
            }

            Assembly[] assemblies = ObjectFactory.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];

                if (assembly.FullName.IndexOf("foundation") != 0)
                {
                    continue;
                }
                Type t = assembly.GetType(type.FullName, false);
                if (t == null)
                {
                    t = assembly.GetType("foundation." + type.Name, false);
                }
                if (t == null)
                {
                    t = assembly.GetType("clayui." + type.Name, false);
                }
                if (t == null)
                {
                    t = assembly.GetType("gameSDK." + type.Name, false);
                }
                if (t != null)
                {
                    MonoBehaviour newMonoBehaviour = (MonoBehaviour) monoBehaviour.gameObject.AddComponent(t);
                    if (newMonoBehaviour != null)
                    {
                        string json = JsonUtility.ToJson(monoBehaviour);
                        JsonUtility.FromJsonOverwrite(json, newMonoBehaviour);
                        GameObject.DestroyImmediate(monoBehaviour, true);
                        break;
                    }
                }
                else
                {
                    Debug.Log("foundation not found class:" + type.Name);
                }
            }
        }

        [MenuItem("GameObject/UI/Image(noRaycast)")]
        static void CreatImage()
        {
            if (Selection.activeTransform)
            {
                if (Selection.activeTransform.GetComponentInParent<Canvas>())
                {
                    Image image = UIUtils.CreateImage("Image", Selection.activeGameObject);
                    image.raycastTarget = false;
                    Selection.activeGameObject = image.gameObject;
                }
            }
        }
    }
}