using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace foundationEditor
{
    public class FindReference : EditorWindow
    {
        static public FindReference instance;
        Vector2 mScroll = Vector2.zero;
        public Dictionary<string, List<string>> dict;
        public List<MissGameObjectRef> missGameObjectRefs;

        void OnEnable() { instance = this; }
        void OnDisable() { instance = null; }

        void OnGUI()
        {

            if (dict == null)
            {
                return;
            }

            if (selectObject != null)
            {
                EditorGUILayout.ObjectField("Select", selectObject, typeof(UnityEngine.Object), false);
            }

            mScroll = GUILayout.BeginScrollView(mScroll);

            List<string> list = dict["prefab"];
            if (list != null && list.Count > 0)
            {
                if (DrawHeader("Prefab"))
                {
                    int i = 0;
                    foreach (string item in list)
                    {
                        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(item);
                        EditorGUILayout.ObjectField("Prefab", go, typeof(GameObject), false);

                        if (missGameObjectRefs != null)
                        {
                            EditorGUI.indentLevel++;
                            MissGameObjectRef missGameObjectRef = missGameObjectRefs[i];
                            foreach (MissComponentDes componentDes in missGameObjectRef.componentRefs)
                            {
                                EditorGUILayout.ObjectField(componentDes.des, componentDes.go,
                                    typeof(GameObject), false);
                            }
                            foreach (MissGameObjectDes missGameObjectDese in missGameObjectRef.missGameobjectDes)
                            {
                                EditorGUILayout.ObjectField(missGameObjectDese.des, missGameObjectDese.go,
                                    typeof(GameObject), false);
                            }
                            foreach (MissComponentDes missComponentDes in missGameObjectRef.missComponentRefs)
                            {
                                EditorGUILayout.ObjectField(missComponentDes.des, missComponentDes.go,
                                    typeof(Component),
                                    false);
                            }
                            EditorGUI.indentLevel--;
                        }

                        i++;
                    }
                }
                list = null;
            }

            list = dict["fbx"];
            if (list != null && list.Count > 0)
            {
                if (DrawHeader("FBX"))
                {
                    foreach (string item in list)
                    {
                        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(item);
                        EditorGUILayout.ObjectField("FBX", go, typeof(GameObject), false);

                    }
                }
                list = null;
            }

            list = dict["cs"];
            if (list != null && list.Count > 0)
            {
                if (DrawHeader("Script"))
                {
                    foreach (string item in list)
                    {
                        MonoScript go = AssetDatabase.LoadAssetAtPath<MonoScript>(item);
                        EditorGUILayout.ObjectField("Script", go, typeof(MonoScript), false);

                    }
                }
                list = null;
            }

            list = dict["texture"];
            if (list != null && list.Count > 0)
            {
                if (DrawHeader("Texture"))
                {
                    foreach (string item in list)
                    {
                        Texture2D go = AssetDatabase.LoadAssetAtPath<Texture2D>(item);
                        if (go != null)
                        {
                            EditorGUILayout.ObjectField("Texture:" + go.name, go, typeof (Texture2D), false);
                        }
                    }
                }
                list = null;
            }

            list = dict["mat"];
            if (list != null && list.Count > 0)
            {
                if (DrawHeader("Material"))
                {
                    foreach (string item in list)
                    {
                        Material go = AssetDatabase.LoadAssetAtPath<Material>(item);
                        EditorGUILayout.ObjectField("Material", go, typeof(Material), false);

                    }
                }
                list = null;
            }

            list = dict["shader"];
            if (list != null && list.Count > 0)
            {
                if (DrawHeader("Shader"))
                {
                    foreach (string item in list)
                    {
                        Shader go = AssetDatabase.LoadAssetAtPath<Shader>(item);
                        EditorGUILayout.ObjectField("Shader", go, typeof(Shader), false);
                    }
                }
                list = null;
            }

            list = dict["font"];
            if (list != null && list.Count > 0)
            {
                if (DrawHeader("Font"))
                {
                    foreach (string item in list)
                    {
                        Font go = AssetDatabase.LoadAssetAtPath<Font>(item);
                        EditorGUILayout.ObjectField("Font", go, typeof(Font), false);
                    }
                }
                list = null;
            }

            list = dict["anim"];
            if (list != null && list.Count > 0)
            {
                if (DrawHeader("Animation"))
                {
                    foreach (string item in list)
                    {
                        AnimationClip go = AssetDatabase.LoadAssetAtPath<AnimationClip>(item);
                        EditorGUILayout.ObjectField("Animation:", go, typeof(AnimationClip), false);
                    }
                }
                list = null;
            }

            list = dict["animTor"];
            if (list != null && list.Count > 0)
            {
                if (DrawHeader("AnimatorController"))
                {
                    foreach (string item in list)
                    {
                        AnimatorController go = AssetDatabase.LoadAssetAtPath<AnimatorController>(item);
                        EditorGUILayout.ObjectField("AnimatorController:", go, typeof(AnimatorController), true);
                    }
                }
                list = null;
            }

            list = dict["level"];
            if (list != null && list.Count > 0)
            {
                if (DrawHeader("Level"))
                {
                    foreach (string item in list)
                    {
                        if (GUILayout.Button(item))
                        {
                            EditorSceneManager.OpenScene(item);
                        }
                    }
                }
                list = null;
            }

            GUILayout.EndScrollView();
            //NGUIEditorTools.DrawList("Objects", list.ToArray(), "");
        }

        private bool DrawHeader(string text, string key="aa", bool forceOn=false, bool minimalistic=false)
        {
            bool state = EditorPrefs.GetBool(key, true);

            if (!minimalistic) GUILayout.Space(3f);
            if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal();
            GUI.changed = false;

            if (minimalistic)
            {
                if (state) text = "\u25BC" + (char)0x200a + text;
                else text = "\u25BA" + (char)0x200a + text;

                GUILayout.BeginHorizontal();
                GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
                if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
                GUI.contentColor = Color.white;
                GUILayout.EndHorizontal();
            }
            else
            {
                text = "<b><size=11>" + text + "</size></b>";
                if (state) text = "\u25BC " + text;
                else text = "\u25BA " + text;
                if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
            }

            if (GUI.changed) EditorPrefs.SetBool(key, state);

            if (!minimalistic) GUILayout.Space(2f);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            if (!forceOn && !state) GUILayout.Space(3f);
            return state;
        }

       
        private static UnityEngine.Object selectObject = null;

        /// <summary>
        /// 根据脚本查找引用的对象
        /// </summary>
        [MenuItem("Assets/Wiker/FindReference", false, 0)]
        static public void _FindReference()
        {
            if (Selection.activeObject == null)
            {
                return;
            }
            selectObject = Selection.activeObject;
            ShowProgress(0, 0, 0);
            string curPathName = AssetDatabase.GetAssetPath(selectObject.GetInstanceID());

            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            List<string> prefabList = new List<string>();
            List<MissGameObjectRef> missGameObjectRefs = new List<MissGameObjectRef>();
            List<string> fbxList = new List<string>();
            List<string> scriptList = new List<string>();
            List<string> textureList = new List<string>();
            List<string> matList = new List<string>();
            List<string> shaderList = new List<string>();
            List<string> fontList = new List<string>();
            List<string> levelList = new List<string>();

            string[] allGuids = AssetDatabase.FindAssets("t:Prefab t:Scene", new string[] { "Assets" });
            int i = 0;
            foreach (string guid in allGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string extension = Path.GetExtension(assetPath).ToLower();
                
                string[] names = EditorUtils.GetDependencies(assetPath); //依赖的东东
                foreach (string name in names)
                {
                    if (name == curPathName)
                    {

                        //Debug.Log("Refer:" + assetPath);
                        switch (extension)
                        {
                            case ".prefab":
                                prefabList.Add(assetPath);

                                GameObject go=AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                                MissGameObjectRef missGameObjectRef=new MissGameObjectRef(go);
                                CollectionFindRef(go, selectObject,curPathName, missGameObjectRef);
                                CollectionFindMiss(go, missGameObjectRef);

                                missGameObjectRefs.Add(missGameObjectRef);
                                break;

                            case ".fbx":

                                fbxList.Add(assetPath);
                                break;

                            case ".unity":

                                levelList.Add(assetPath);
                                break;

                            case ".cs":
                                scriptList.Add(assetPath);
                                break;
                            case ".png":

                                textureList.Add(assetPath);
                                break;

                            case ".mat":
                                matList.Add(assetPath);
                                break;
                            case ".shader":
                                shaderList.Add(assetPath);
                                break;
                            case ".ttf":
                                fontList.Add(assetPath);
                                break;
                        }
                    }
                }
                ShowProgress((float) i/(float) allGuids.Length, allGuids.Length, i);
                i++;
            }

            dic.Add("prefab", prefabList);
            dic.Add("fbx", fbxList);
            dic.Add("cs", scriptList);
            dic.Add("texture", textureList);
            dic.Add("mat", matList);
            dic.Add("shader", shaderList);
            dic.Add("font", fontList);
            dic.Add("level", levelList);
            dic.Add("anim", null);
            dic.Add("animTor", null);
            EditorUtility.ClearProgressBar();
            EditorWindow.GetWindow<FindReference>(false, "ObjectReference", true).Show();


            if (instance.dict != null)
            {
                instance.dict.Clear();
            }
            instance.dict = dic;
            instance.missGameObjectRefs = missGameObjectRefs;
        }



        private static void CollectionFindRef(GameObject go,UnityEngine.Object refValue,string fullPath, MissGameObjectRef missGameObjectRef)
        {
            Component[] components = go.GetComponents<Component>();
            foreach (var c in components)
            {
                if (c != null)
                {

                    string path=AssetDatabase.GetAssetPath(c);
                    if (path == fullPath)
                    {
                        missGameObjectRef.AddRefPropertys(c, "ref Component");
                    }

                    SerializedObject so = new SerializedObject(c);
                    SerializedProperty sp = so.GetIterator();

                    while (sp.NextVisible(true))
                    {
                        if (sp.propertyType != SerializedPropertyType.ObjectReference) continue;

                        if (sp.objectReferenceValue == refValue)
                        {
                            missGameObjectRef.AddRefPropertys(c, "ref Property:" + ObjectNames.NicifyVariableName(sp.name) +
                                                                 " in component: " + c.GetType().Name);
                        }
                    }
                }
            }
            int len = go.transform.childCount;
            for (int i = 0; i < len; i++)
            {
                CollectionFindRef(go.transform.GetChild(i).gameObject,refValue,fullPath,missGameObjectRef);
            }
        }
        private static void CollectionFindMiss(GameObject go, MissGameObjectRef missGameObjectRef)
        {
            Component[] components = go.GetComponents<Component>();
            foreach (var c in components)
            {
                if (c == null)
                {
                    missGameObjectRef.AddComponent(go,"compnent is null");
                }
                else
                {
                    SerializedObject so = new SerializedObject(c);
                    SerializedProperty sp = so.GetIterator();

                    while (sp.NextVisible(true))
                    {
                        if (sp.propertyType != SerializedPropertyType.ObjectReference) continue;

                        if (sp.objectReferenceValue == null
                            && sp.objectReferenceInstanceIDValue != 0)
                        {
                            missGameObjectRef.AddPropertys(c,
                                "Missing Property:" + ObjectNames.NicifyVariableName(sp.name) +
                                " in component: " + c.GetType().Name);

                        }
                    }
                }
            }
            int len = go.transform.childCount;
            for (int i = 0; i < len; i++)
            {
                CollectionFindMiss(go.transform.GetChild(i).gameObject, missGameObjectRef);
            }
        }


        public static void ShowProgress(float val, int total, int cur)
        {
            EditorUtility.DisplayProgressBar("Searching", string.Format("Finding ({0}/{1}), please wait...", cur, total), val);
        }



        /// <summary>
        /// 查找对象引用的类型
        /// </summary>
        [MenuItem("Assets/Wiker/FindDependencies", false, 10)]
        public static void FindObjectDependencies()
        {
            if (Selection.activeObject == null)
            {
                return;
            }
            selectObject = Selection.activeObject;

            ShowProgress(0, 0, 0);
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            List<string> prefabList = new List<string>();
            List<string> fbxList = new List<string>();
            List<string> scriptList = new List<string>();
            List<string> textureList = new List<string>();
            List<string> matList = new List<string>();
            List<string> shaderList = new List<string>();
            List<string> fontList = new List<string>();
            List<string> animList = new List<string>();
            List<string> animTorList = new List<string>();
            string curPathName = AssetDatabase.GetAssetPath(selectObject.GetInstanceID());
            string[] names = AssetDatabase.GetDependencies(new string[] {curPathName}); //依赖的东东
            int i = 0;
            foreach (string name in names)
            {
                if (name.EndsWith(".prefab"))
                {
                    prefabList.Add(name);
                }
                else if (name.ToLower().EndsWith(".fbx"))
                {
                    fbxList.Add(name);
                }
                else if (name.EndsWith(".cs"))
                {
                    scriptList.Add(name);
                }
                else if (name.EndsWith(".png"))
                {
                    textureList.Add(name);
                }
                else if (name.EndsWith(".mat"))
                {
                    matList.Add(name);
                }
                else if (name.EndsWith(".shader"))
                {
                    shaderList.Add(name);
                }
                else if (name.EndsWith(".ttf"))
                {
                    fontList.Add(name);
                }
                else if (name.EndsWith(".anim"))
                {
                    animList.Add(name);
                }
                else if (name.EndsWith(".controller"))
                {
                    animTorList.Add(name);
                }
                Debug.Log("Dependence:" + name);
                ShowProgress((float) i / (float) names.Length, names.Length, i);
                i++;
            }

            dic.Add("prefab", prefabList);
            dic.Add("fbx", fbxList);
            dic.Add("cs", scriptList);
            dic.Add("texture", textureList);
            dic.Add("mat", matList);
            dic.Add("shader", shaderList);
            dic.Add("font", fontList);
            dic.Add("level", null);
            dic.Add("animTor", animTorList);
            dic.Add("anim", animList);
            //deps.Sort(Compare);
            EditorWindow.GetWindow<FindReference>(false, "ObjectDependencies", true).Show();
            if (instance.dict != null)
            {
                instance.dict.Clear();
            }
            instance.dict = dic;
            instance.missGameObjectRefs = null;
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("Assets/Wiker/ClearDependenciesCache", false, 20)]
        public static void ClearDependenciesCache()
        {
            EditorUtils.ClearDependenciesCache();
        }
       
    }
}