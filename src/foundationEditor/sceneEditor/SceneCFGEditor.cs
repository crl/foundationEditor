using foundation;
using gameSDK;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Path = System.IO.Path;

namespace foundationEditor
{
    public class SceneConfigEditor : EditorWindow
    {
        private const string COPYTO = "SceneCFG_Copy2ServerPath";
        private Styles styles;

        [SerializeField]
        private Vector2 scrollPos;
        [SerializeField]
        private SceneCFGMode _selectedSceneCfgMode;
        [SerializeField]
        private bool showTransforms = false;
        private Vector3[] verts = new Vector3[4];
        private RenderTexture renderTexture;

        private SceneCFG sceneCFG;
        private GenericMenu scenesGenericMenu;
        private List<string> allReleaseScenes = new List<string>();

        [MenuItem("App/SceneEditor", false, 1)]
        public static void TSceneEditor()
        {
            GetWindow<SceneConfigEditor>();
        }

        public void OnEnable()
        {
            //autoFSMEvent init;
            //List<FsmEvent> EventList = FsmEvent.EventList;
            PrefabVODB.Reload();
            string path;
            IList o;
            string[] excelFileIDTypes = new string[]
                {ExcelFileIDType.Npc, ExcelFileIDType.Monster, ExcelFileIDType.Collection};
            foreach (string excelFileIDType in excelFileIDTypes)
            {
                path = EditorConfigUtils.GetPrifix(excelFileIDType);
                o = FileHelper.GetAMF(path, false) as IList;
                if (o == null)
                {
                    continue;
                }
                List<ExcelRefVO> list = new List<ExcelRefVO>();
                foreach (Dictionary<string, object> item in o)
                {
                    ExcelRefVO excelVO = new ExcelRefVO();
                    excelVO.id = item.Get<string>("id");
                    excelVO.name = item.Get<string>("name");
                    excelVO.uri = item.Get<string>("uri");
                    excelVO.prefabPath = PrefabVODB.GetPath(excelVO.uri);
                    list.Add(excelVO);
                }
                ExcelIDSelecterDrawer.Fill(excelFileIDType, list);
            }

            path = EditorConfigUtils.GetPrifix(ExcelFileIDType.Map);
            o = FileHelper.GetAMF(path, false) as IList;
            if (o != null)
            {
                foreach (Dictionary<string, object> item in o)
                {
                    ExcelMapVO excelVO = new ExcelMapVO();
                    excelVO.id = item.Get<string>("id");
                    excelVO.name = item.Get<string>("name");
                    excelVO.uri = item.Get<string>("uri");
                    ExcelIDSelecterDrawer.AddMapItem(excelVO);
                }
            }

            this.titleContent = new GUIContent("SceneEditor");


            allReleaseScenes.Clear();
            scenesGenericMenu = new GenericMenu();
            string[] fileList = new string[0];
            if (Directory.Exists("Assets/Prefabs/scene"))
            {
                fileList = Directory.GetFiles("Assets/Prefabs/scene", "*.unity", SearchOption.AllDirectories);
            }
            foreach (string file in fileList)
            {
                allReleaseScenes.Add(file);
                string fileName = Path.GetFileNameWithoutExtension(file);
                scenesGenericMenu.AddItem(new GUIContent(fileName), false, (object item) =>
                {
                    string scenePath = item.ToString();
                    if (File.Exists(scenePath))
                    {
                        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                        CheckSceneInfo(true);
                        _selectedSceneCfgMode = SceneCFGMode.SceneSettings;
                        Repaint();
                    }
                }, file);
            }

            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.playModeStateChanged -= playmodeStateChanged;
            EditorApplication.playModeStateChanged += playmodeStateChanged;
        }

        public void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
        protected void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= playmodeStateChanged;
        }

        private void playmodeStateChanged(PlayModeStateChange state)
        {
            this.CheckSceneInfo(false);
        }

        private Quaternion lastSceneViewRotation;
        private bool lastSceneViewOrtho;
        private bool is2DState = false;

        public void OnGUI()
        {
            if (EditorApplication.isCompiling)
            {
                ShowNotification(new GUIContent("Compiling\n...Please wait..."));
                return;
            }

            GUI.skin.label.richText = true;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            EditorStyles.label.richText = true;
            EditorStyles.textField.wordWrap = true;
            EditorStyles.foldout.richText = true;


            GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
            GUI.color = Color.white;
            GUILayout.BeginHorizontal(EditorStyles.toolbar);


            if (GUILayout.Button("File", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                scenesGenericMenu.ShowAsContext();
            }

            if (GUILayout.Button("Edit2D", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                SceneView sceneView = SceneView.lastActiveSceneView;
                if (is2DState == false)
                {
                    lastSceneViewRotation = sceneView.rotation;
                    lastSceneViewOrtho = sceneView.orthographic;
                }
                is2DState = true;
                sceneView.LookAt(sceneView.pivot, Quaternion.Euler(90, 0, 0), sceneView.size, true);
                sceneView.isRotationLocked = true;
                FieldInfo fieldInfo = sceneView.GetType()
                    .GetField("m_SceneViewState", BindingFlags.NonPublic | BindingFlags.Instance);

                SceneView.SceneViewState scneViewState = (SceneView.SceneViewState)fieldInfo.GetValue(sceneView);
                scneViewState.showFog = false;
                fieldInfo.SetValue(sceneView, scneViewState);

                sceneView.Repaint();
            }

            if (GUILayout.Button("Edit3D", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                is2DState = false;
                SceneView sceneView = SceneView.lastActiveSceneView;
                sceneView.isRotationLocked = false;
                sceneView.LookAt(sceneView.pivot, lastSceneViewRotation, sceneView.size, lastSceneViewOrtho);
                sceneView.Repaint();
            }

            if (sceneCFG)
            {
                if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    GameObject root = sceneCFG.gameObject;
                    var prefabSrc = PrefabUtility.GetCorrespondingObjectFromSource(root);
                    var path = AssetDatabase.GetAssetPath(prefabSrc);
                    PrefabUtility.SaveAsPrefabAssetAndConnect(root, path,
                        InteractionMode.AutomatedAction);

                    Scene scene = root.scene;
                    if (scene != null)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Check", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    CheckSceneInfo(false);
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;

            if (sceneCFG == null)
            {
                string label = string.Format("<size=30><b>{0}</b></size>", "Welcome to SceneEditor!");
                Vector2 size = new GUIStyle("label").CalcSize(new GUIContent(label));
                Rect titleRect = new Rect(0, 0, size.x, size.y);
                titleRect.center = new Vector2(Screen.width / 2, (Screen.height / 2) - size.y);
                GUI.Label(titleRect, label);

                titleRect.y = titleRect.y + titleRect.height;
                titleRect.height = 20;
                //空场景没有存在时才能创建;
                string path = getSceneAssetPrefix() + "/sceneInfo.prefab";
                if (File.Exists(path) == false)
                {
                    if (GUI.Button(titleRect, "create"))
                    {
                        CreateSceneInfo();
                    }
                }
                return;
            }

            EditorGUILayout.Space();
            this.ModeToggle();
            EditorGUILayout.Space();

            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, new GUILayoutOption[0]);

            switch (this._selectedSceneCfgMode)
            {
                case SceneCFGMode.SceneSettings:
                    this.SceneInfo();
                    break;
                case SceneCFGMode.RegionSettings:
                    this.RegionSettings();
                    break;

                case SceneCFGMode.PathsSettings:
                    this.PathsSettings();
                    break;

                case SceneCFGMode.SpawersSettings:
                    this.SpawersSettings();
                    break;
                case SceneCFGMode.NPCSettings:
                    this.NPCSettings();
                    break;
                case SceneCFGMode.CutSettings:
                    this.CutSettings();
                    break;
                case SceneCFGMode.COPYSettings:
                    this.CopySetting();
                    break;
            }
            EditorGUILayout.EndScrollView();
        }

        private void CheckSceneInfo(bool revert)
        {
            sceneCFG = FindObjectOfType<SceneCFG>();
            if (sceneCFG != null)
            {
                GameObject root = sceneCFG.gameObject;
                PrefabInstanceStatus prefabType = PrefabUtility.GetPrefabInstanceStatus(root);
                if (prefabType == PrefabInstanceStatus.NotAPrefab)
                {
                    GameObject.DestroyImmediate(root);
                    sceneCFG = null;
                }
                else
                {
                    //重置为已存储的数据
                    if (revert)
                    {
                        PrefabUtility.RevertPrefabInstance(root, InteractionMode.AutomatedAction);
                    }
                }
            }

            if (sceneCFG == null)
            {
                string path = getSceneAssetPrefix() + "/sceneInfo.prefab";
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null)
                {
                    GameObject root = (GameObject)PrefabUtility.InstantiatePrefab(go);
                    //root=PrefabUtility.ConnectGameObjectToPrefab(root, go);
                    sceneCFG = root.GetComponent<SceneCFG>();
                }
            }
        }

        private void CreateSceneInfo()
        {
            GameObject root = new GameObject("SceneInfo");
            //root.hideFlags = HideFlags.HideInHierarchy;
            sceneCFG = root.AddComponent<SceneCFG>();

            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid() == false)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            string path = getSceneAssetPrefix() + "/SceneInfo.prefab";

            if (Directory.Exists(getSceneAssetPrefix()) == false)
            {
                Directory.CreateDirectory(getSceneAssetPrefix());
            }
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }

        private void SceneInfo()
        {
            EditorGUI.BeginChangeCheck();
            GameObject root = sceneCFG.gameObject;
            showTransforms = EditorGUILayout.ToggleLeft("show", root.hideFlags == 0);
            if (GUI.changed)
            {
                if (showTransforms)
                {
                    root.hideFlags = 0;
                }
                else
                {
                    root.hideFlags = HideFlags.HideInHierarchy;
                }
                EditorApplication.DirtyHierarchyWindowSorting();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical("box");
            int size = 1000;
            EditorGUILayout.IntSlider("X", (int)sceneCFG.rect.x, -size, size);
            EditorGUILayout.IntSlider("Y", (int)sceneCFG.rect.y, -size, size);
            int x = EditorGUILayout.IntSlider("CenterX", (int)sceneCFG.rect.center.x, -size, size);
            int y = EditorGUILayout.IntSlider("CenterY", (int)sceneCFG.rect.center.y, -size, size);
            int width = EditorGUILayout.IntSlider("Width", (int)sceneCFG.rect.width, 0, 2000);
            int height = EditorGUILayout.IntSlider("Height", (int)sceneCFG.rect.height, 0, 2000);
            EditorGUILayout.EndVertical();
            Rect rect = new Rect(0, 0, width, height);
            rect.center = new Vector2(x, y);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(sceneCFG, "position");
                sceneCFG.rect = rect;
            }

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                sceneView = SceneView.currentDrawingSceneView;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(sceneCFG.cutview, typeof(Texture), false);
            sceneCFG.preview = (Texture)EditorGUILayout.ObjectField("preview", sceneCFG.preview, typeof(Texture), false);
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("GetPreview"))
            {
                int textureSize = (int)(rect.height * 2);
                float aspect = rect.width / rect.height;
                int textWidth = Mathf.RoundToInt(aspect * textureSize);
                if (renderTexture == null || renderTexture.height != textureSize || renderTexture.width != textWidth)
                {
                    renderTexture = new RenderTexture(textWidth, textureSize, 24);
                    renderTexture.hideFlags = HideFlags.HideAndDontSave;
                }
                else
                {
                    renderTexture.DiscardContents();
                }
                Camera camera = sceneView.camera;

                Vector3 v = new Vector3(rect.x + rect.width / 2f, 0, rect.y + rect.height / 2f);
                sceneView.orthographic = true;
                sceneView.isRotationLocked = true;
                sceneView.LookAt(v, Quaternion.Euler(90, 0, 0));
                sceneView.Repaint();
                EditorApplication.delayCall += () =>
                {
                    RenderTexture oldRenderTexture = camera.targetTexture;
                    float oldAspect = camera.aspect;
                    float oldOrthographicSize = camera.orthographicSize;

                    bool oldFog = RenderSettings.fog;
                    Unsupported.SetRenderSettingsUseFogNoDirty(false);

                    camera.orthographicSize = rect.height / 2f;
                    camera.aspect = aspect;
                    camera.targetTexture = renderTexture;
                    camera.Render();

                    camera.orthographicSize = oldOrthographicSize;
                    camera.aspect = oldAspect;
                    camera.targetTexture = null;
                    Unsupported.SetRenderSettingsUseFogNoDirty(oldFog);

                    string name = "preview";
                    SaveRenderTextureToPNG(renderTexture, aspect, name);
                    AssetDatabase.Refresh();

                    string path = getSceneAssetPrefix() + "/" + name + ".png";
                    sceneCFG.cutview = sceneCFG.preview = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    sceneView.Repaint();
                };
            }
            if (GUILayout.Button("GetHeightmap"))
            {
                int w = (int)(rect.width * 10);
                int h = (int)(rect.height * 10);

                Texture2D texture2D = new Texture2D(w, h, TextureFormat.ARGB32, false);
                texture2D.hideFlags = HideFlags.HideAndDontSave;
                Color[] colors = new Color[w * h];

                NavMeshTriangulation tmpNavMeshTriangulation = NavMesh.CalculateTriangulation();
                Vector3[] vertices = tmpNavMeshTriangulation.vertices;
                int[] indices = tmpNavMeshTriangulation.indices;
                int len = indices.Length / 3;
                HeightMap heightMap = new HeightMap(w, h, TextureFormat.ARGB32, false);

                for (int i = 0; i < len; i++)
                {
                    int index = indices[i * 3 + 0];
                    Vector3 v0 = Trans(vertices[index]);
                    index = indices[i * 3 + 1];
                    Vector3 v1 = Trans(vertices[index]);
                    index = indices[i * 3 + 2];
                    Vector3 v2 = Trans(vertices[index]);
                    heightMap.DrawTriangle(v0, v1, v2, Color.red);

                    if (i % 500 == 0)
                    {
                        ShowProgress(i, len);
                    }
                }
                EditorUtility.ClearProgressBar();
                texture2D = heightMap.EndDraw();

                string pngName = "heightMap";
                saveTexture(texture2D, pngName);

                Texture2D.DestroyImmediate(texture2D);
                AssetDatabase.Refresh();
            }
            if (GUILayout.Button("ExportNav"))
            {

                Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();
                List<MeshFilter> meshRenderers = new List<MeshFilter>();
                List<SkinnedMeshRenderer> skinMeshRenderers = new List<SkinnedMeshRenderer>();
                foreach (Renderer renderer in renderers)
                {
                    if (renderer is MeshRenderer)
                    {
                        GameObject go = renderer.gameObject;
                        SerializedObject so = new SerializedObject(go);
                        SerializedProperty property = so.FindProperty("m_StaticEditorFlags");
                        if ((property.intValue & 8) != 0)
                        {
                            MeshFilter f = renderer.gameObject.GetComponent<MeshFilter>();
                            if (f) meshRenderers.Add(f);
                        }
                    }
                }

                if (meshRenderers.Count == 0)
                {
                    List<GameObject> list = new List<GameObject>();
                    SceneManager.GetActiveScene().GetRootGameObjects(list);
                    List<Renderer> rList = new List<Renderer>();
                    foreach (GameObject go in list)
                    {
                        Renderer[] t = go.GetComponentsInChildren<Renderer>(true);
                        rList.AddRange(t);
                    }

                    foreach (Renderer renderer in rList)
                    {
                        if (renderer is MeshRenderer)
                        {
                            GameObject go = renderer.gameObject;
                            SerializedObject so = new SerializedObject(go);
                            SerializedProperty property = so.FindProperty("m_StaticEditorFlags");
                            if ((property.intValue & 8) != 0)
                            {
                                MeshFilter f = renderer.gameObject.GetComponent<MeshFilter>();
                                if (f) meshRenderers.Add(f);
                            }
                        }
                    }
                }


                NavObjectData v = new NavObjectData();
                v.export(meshRenderers.ToArray(), skinMeshRenderers.ToArray());

                FileInfo fileInfo = new FileInfo(Application.dataPath);
                string prefix = Path.Combine(fileInfo.Directory.FullName, "ReleaseResource/NAV/");
                if (Directory.Exists(prefix) == false)
                {
                    Directory.CreateDirectory(prefix);
                }
                v.save(prefix + "Meshes/RootScene.obj");

                System.Diagnostics.Process.Start(prefix + "Recast.bat");
                System.Diagnostics.Process.Start(prefix);
            }

            if (GUILayout.Button("CopyRect"))
            {
                IntRectange intRectange = new IntRectange();
                intRectange.CopyFrom(sceneCFG.rect);
                string v = intRectange.x + "," + intRectange.y + "," + intRectange.width + "," + intRectange.height;
                EditorGUIUtility.systemCopyBuffer = v;
                ShowNotification(new GUIContent("已复制"));
            }

            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                sceneView.Repaint();
            }
        }
        private Vector3 Trans(Vector3 v)
        {
            float rawY = v.y;
            v.x = ((v.x - sceneCFG.rect.x) * 10f);
            v.y = ((v.z - sceneCFG.rect.y) * 10f);
            v.z = rawY;
            return v;
        }

        private bool ShowProgress(int cur, int total)
        {
            float val = cur / (float)total;

            return EditorUtility.DisplayCancelableProgressBar("Searching",
                string.Format("Finding ({0}/{1}), please wait...", cur, total), val);
        }
        public bool SaveRenderTextureToPNG(RenderTexture renderTexture, float aspect, string pngName)
        {
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = renderTexture;
            int h = renderTexture.height;
            int w = renderTexture.width;
            Texture2D png = new Texture2D(w, h, TextureFormat.ARGB32, false);
            png.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            saveTexture(png, pngName);

            Texture2D.DestroyImmediate(png);
            RenderTexture.active = prev;
            return true;
        }

        private string getSceneAssetPrefix()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            string path = scene.path;

            string directoryPath;
            string fileName;
            if (File.Exists(path) == false)
            {
                directoryPath = Application.dataPath + "/Temp";
                fileName = "Temp";
            }
            else
            {
                directoryPath = Path.GetDirectoryName(path);
                fileName = Path.GetFileNameWithoutExtension(path);
            }
            return directoryPath + "/" + fileName;
        }

        private void saveTexture(Texture2D texture2D, string pngName)
        {

            string prefix = getSceneAssetPrefix();
            byte[] bytes = texture2D.EncodeToPNG();
            if (!Directory.Exists(prefix))
            {
                Directory.CreateDirectory(prefix);
            }
            FileStream file = File.Open(prefix + "/" + pngName + ".png", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);
            writer.Write(bytes);
            file.Close();
        }

        private void PathsSettings()
        {
            GameObject root = sceneCFG.gameObject;
            PathsCFG[] pathsCfgs = root.GetComponentsInChildren<PathsCFG>();

            foreach (PathsCFG item in pathsCfgs)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(item.name))
                {
                    Selection.activeObject = item;
                }
                if (GUILayout.Button("delete", GUILayout.Width(50)))
                {
                    GameObject.DestroyImmediate(item.gameObject);
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add"))
            {
                GameObject go = new GameObject("pathsCFG");
                go.AddComponent<PathsCFG>();
                go.transform.SetParent(root.transform, false);
            }
        }

        private void RegionSettings()
        {
            GameObject root = sceneCFG.gameObject;
            PlayMakerTrigger[] playMakerTriggers = root.GetComponentsInChildren<PlayMakerTrigger>();

            string name;
            foreach (PlayMakerTrigger item in playMakerTriggers)
            {
                EditorGUILayout.BeginHorizontal();
                name = item.name + "[" + item.areaType.ToString() + "]";
                if (GUILayout.Button(name))
                {
                    //                    PlayMakerFSM fsm = item.GetComponentInChildren<PlayMakerFSM>();
                    //                    if (fsm)
                    //                    {
                    //                        FsmComponentInspector.OpenInEditor(fsm);
                    //                    }
                    //                    else
                    //                    {
                    //                        Selection.activeObject = item;
                    //                    }
                }
                if (GUILayout.Button("delete", GUILayout.Width(50)))
                {
                    GameObject.DestroyImmediate(item.gameObject);
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add"))
            {
                //                GameObject go = new GameObject("playMakerTrigger");
                //                BoxCollider collider = go.AddComponent<BoxCollider>();
                //                collider.isTrigger = true;
                //                go.AddComponent<PlayMakerTrigger>();
                //                PlayMakerFSM playMakerFSM = go.AddComponent<PlayMakerFSM>();
                //                Fsm targetFsm = playMakerFSM.Fsm;
                //
                //                FsmState enterState = new FsmState(targetFsm);
                //                enterState.Name = "enter";
                //                enterState.Position = new Rect(enterState.Position)
                //                {
                //                    x = 250,
                //                    y = 102
                //                };
                //                enterState.ColorIndex = 1;
                //
                //
                //                FsmState exitState = new FsmState(targetFsm);
                //                exitState.Name = "exit";
                //                exitState.Position = new Rect(exitState.Position)
                //                {
                //                    x = 450,
                //                    y = 102
                //                };
                //                exitState.ColorIndex = 6;
                //                targetFsm.States = ArrayAdd(targetFsm.States, enterState, exitState);
                //                FsmTransition enterTransition = new FsmTransition
                //                {
                //                    ToState = enterState.Name,
                //                    FsmEvent = FsmEvent.TriggerEnter
                //                };
                //                FsmTransition exitTransition = new FsmTransition
                //                {
                //                    ToState = exitState.Name,
                //                    FsmEvent = FsmEvent.TriggerExit
                //                };
                //                targetFsm.Events = ArrayAdd(targetFsm.Events, enterTransition.FsmEvent, exitTransition.FsmEvent);
                //                targetFsm.GlobalTransitions = ArrayAdd(targetFsm.GlobalTransitions, enterTransition, exitTransition);
                //                AddVariable(targetFsm, VariableType.GameObject, "Collider");
                //                go.transform.SetParent(root.transform, false);
            }
        }

        //        public static NamedVariable AddVariable(Fsm fsm, VariableType type, string name)
        //        {
        //            List<FsmVariable> fsmVariableList = FsmVariable.GetFsmVariableList(fsm.OwnerObject);
        //            name = FsmVariable.GetUniqueVariableName(fsmVariableList, name);
        //            FsmVariable.AddVariable(fsm.Variables, type, name, null, VariableType.Float);
        //            return fsm.Variables.GetVariable(name);
        //        }

        public static T[] ArrayAdd<T>(T[] array, params T[] items)
        {
            List<T> list = new List<T>(array);
            foreach (var item in items)
            {
                list.Add(item);
            }
            return list.ToArray();
        }

        private void SpawersSettings()
        {
            GameObject root = sceneCFG.gameObject;
            TeamSpawersCFG[] regionsCfGs = root.GetComponentsInChildren<TeamSpawersCFG>();

            foreach (TeamSpawersCFG item in regionsCfGs)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(item.name))
                {
                    Selection.activeObject = item;

                }
                if (GUILayout.Button("delete", GUILayout.Width(50)))
                {
                    GameObject.DestroyImmediate(item.gameObject);
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add"))
            {
                GameObject go = new GameObject("spawersCFG");
                go.AddComponent<TeamSpawersCFG>();
                go.transform.SetParent(root.transform, false);
            }
        }

        private void NPCSettings()
        {
            GameObject root = sceneCFG.gameObject;
            PointsCFG[] pointsCfgs = root.GetComponentsInChildren<PointsCFG>();
            foreach (PointsCFG item in pointsCfgs)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(item.name))
                {
                    Selection.activeObject = item;
                }
                if (GUILayout.Button("delete", GUILayout.Width(50)))
                {
                    GameObject.DestroyImmediate(item.gameObject);
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add"))
            {
                GameObject go = new GameObject("npcCFG");
                go.AddComponent<PointsCFG>();
                go.transform.SetParent(root.transform, false);
            }
        }

        private void CutSettings()
        {
            /*
#if DEBUG
            string cutScenePrefix = getSceneAssetPrefix() + "/CutScene/";
            Cutscene[] cutscenes = FindObjectsOfType<Cutscene>();
            List<string> cutsceneNames=new List<string>();
            foreach (Cutscene item in cutscenes)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(item.name))
                {
                    CutsceneEditor.ShowWindow(item);
                }
                cutsceneNames.Add(item.name);
                if (GUILayout.Button("save", GUILayout.Width(50)))
                {
                    saveCutscene(cutScenePrefix,item);
                    //不删除,以免影响保存体验
                    //GameObject.DestroyImmediate(item.gameObject);
                }
                if (GUILayout.Button("delete", GUILayout.Width(50)))
                {
                    CutsceneEditor.ShowWindow(null);
                    if (EditorUtility.DisplayDialog("提示", "是否先保存再删除", "先保存", "不了"))
                    {
                        saveCutscene(cutScenePrefix, item);
                    }
                    DestroyImmediate(item.gameObject);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Separator();
            string[] fileList;

            if (Directory.Exists(cutScenePrefix))
            {
                fileList = Directory.GetFiles(cutScenePrefix, "*.prefab");
                foreach (string item in fileList)
                {
                    EditorGUILayout.BeginHorizontal("toolBar");
                    FileInfo fileInfo = new FileInfo(item);
                    string name=Path.GetFileNameWithoutExtension(fileInfo.Name);

                    GUI.color =Color.white;
                    foreach (Cutscene cutItem in cutscenes)
                    {
                        if (cutItem == null)
                        {
                            continue;
                        }
                        if (cutItem.name == name)
                        {
                            GUI.color=new Color(0,1,1,1);
                            break;
                        }
                    }

                    if (GUILayout.Button(name, EditorStyles.toolbarButton))
                    {
                        string path = fileInfo.FullName.Replace("\\", "/");
                        path = path.Replace(Application.dataPath, "Assets");
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        Selection.activeGameObject = prefab;
                        EditorGUIUtility.PingObject(prefab);
                    }
                    if (GUILayout.Button("edit", EditorStyles.toolbarButton, GUILayout.Width(50)))
                    {
                        string path = fileInfo.FullName.Replace("\\", "/");
                        path = path.Replace(Application.dataPath, "Assets");
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        int index = cutsceneNames.IndexOf(prefab.name);
                        if (index != -1)
                        {
                            CutsceneEditor.ShowWindow(cutscenes[index]);
                            ShowNotification(new GUIContent("已经在场景上"));
                            continue;
                        }

                        if (cutscenes.Length > 0)
                        {
                            CutsceneEditor.ShowWindow(null);
                            foreach (Cutscene cutItem in cutscenes)
                            {
                                DestroyImmediate(cutItem.gameObject);
                            }
                        }
                        CutsceneInject.ClearContainer();

                        GameObject go = (GameObject) PrefabUtility.InstantiatePrefab(prefab);
                        Cutscene cutscene = go.GetComponent<Cutscene>();
                        cutscene.HideTransforms();
                        CutsceneEditor.ShowWindow(cutscene);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                GUI.color = Color.white;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add"))
            {
                var cutscene = Cutscene.Create();
                Selection.activeObject = cutscene;
            }
#endif
*/
        }

        private void saveCutscene(string cutScenePrefix, MonoBehaviour item)
        {
            if (Directory.Exists(cutScenePrefix) == false)
            {
                Directory.CreateDirectory(cutScenePrefix);
            }

            string prefabPath = cutScenePrefix + item.name + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                prefab = PrefabUtility.SaveAsPrefabAsset(item.gameObject, prefabPath);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(item.gameObject, prefabPath, InteractionMode.AutomatedAction);
            }

        }
        private void CopySetting()
        {
            string copyToPath = EditorPrefs.GetString(COPYTO);

            EditorGUI.BeginChangeCheck();
            copyToPath = EditorGUILayout.TextField("服务端配置路径:", copyToPath);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(COPYTO, copyToPath);
            }

            if (GUILayout.Button("复制所有"))
            {
                if (File.Exists(copyToPath) == false)
                {
                    ShowNotification(new GUIContent("服务端配置路径不存在"));
                    return;
                }

                foreach (string releaseScenePath in allReleaseScenes)
                {
                    if (File.Exists(releaseScenePath) == false)
                    {
                        continue;
                    }
                    string directoryPath = Path.GetDirectoryName(releaseScenePath);
                    string fileName = Path.GetFileNameWithoutExtension(releaseScenePath);
                    string prefix = directoryPath + "/" + fileName;
                    //File();
                }
            }
        }


        private void ModeToggle()
        {
            if (styles == null)
            {
                styles = new Styles();
            }
            this._selectedSceneCfgMode = (SceneCFGMode)GUILayout.Toolbar((int)this._selectedSceneCfgMode, styles.modeToggles, "LargeButton");
        }


        public virtual void OnSceneGUI(SceneView sceneView)
        {
            if (sceneCFG == null)
            {
                return;
            }

            switch (_selectedSceneCfgMode)
            {
                case SceneCFGMode.SceneSettings:
                    this.SceneInfoGUI();
                    break;
                case SceneCFGMode.SpawersSettings:
                    this.SpawersSceneGUI();
                    break;
                case SceneCFGMode.NPCSettings:
                    this.NpcSceneGUI();
                    break;
            }
        }

        private void NpcSceneGUI()
        {
        }

        private void SpawersSceneGUI()
        {
        }

        private void SceneInfoGUI()
        {
            Rect rect = sceneCFG.rect;
            Vector3 pos = new Vector3(rect.center.x, 0, rect.center.y);
            Vector3 newPos = Handles.FreeMoveHandle(pos, Quaternion.identity, 5, Vector3.zero, Handles.CircleHandleCap);
            if (newPos != pos)
            {
                rect.center = new Vector2(newPos.x, newPos.z);
                Undo.RegisterCompleteObjectUndo(sceneCFG, "position");
                sceneCFG.rect = rect;
            }

            verts[0] = new Vector3(rect.xMin, 0, rect.yMin);
            verts[1] = new Vector3(rect.xMax, 0, rect.yMin);
            verts[2] = new Vector3(rect.xMax, 0, rect.yMax);
            verts[3] = new Vector3(rect.xMin, 0, rect.yMax);

            Color c = Color.green;
            c.a = 0.01f;
            Handles.DrawSolidRectangleWithOutline(verts, c, Color.gray);
        }

        public void init(SceneCFGInspector sceneCFGInspector)
        {
            CheckSceneInfo(false);
        }

        protected enum SceneCFGMode
        {
            SceneSettings,
            RegionSettings,
            PathsSettings,
            NPCSettings,
            SpawersSettings,
            CutSettings,
            COPYSettings
        }


        public class Styles
        {
            public GUIStyle header = "ShurikenModuleTitle";
            public readonly GUIContent[] modeToggles;
            public Styles()
            {
                this.modeToggles = new GUIContent[]
                {
                    EditorGUIUtilityExt.TextContent("场景信息|场景信息设置."),
                    EditorGUIUtilityExt.TextContent("区域|区域设置."),
                    EditorGUIUtilityExt.TextContent("路点|路点设置"),
                    EditorGUIUtilityExt.TextContent("NPC|NPC设置"),
                    EditorGUIUtilityExt.TextContent("刷怪|刷怪设置"),
                    EditorGUIUtilityExt.TextContent("CutScene|剧情片段"),
                    EditorGUIUtilityExt.TextContent("Copy|复制")
                };
            }
        }


    }

}