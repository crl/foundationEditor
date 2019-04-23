using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissingInSceneFinder: EditorWindow
{
    public class MissRef
    {
        public Object o;
        public string des;

        public MissRef(Object o, string des)
        {
            this.o = o;
            this.des = des;
        }
    }

    [MenuItem("Tools/Missing in scene", false, 50)]
    public static void FindMissingReferencesInCurrentScene()
    {
        Scene scene=EditorSceneManager.GetActiveScene();
        GameObject[] objects=scene.GetRootGameObjects();

        missComp.Clear();
        missComp.Clear();
        FindMissingReferences(scene.name, objects);


        GetWindow<MissingInSceneFinder>();
    }

    public static List<MissRef> missComp=new List<MissRef>();
    public static List<MissRef> missRef = new List<MissRef>();
    private static void FindMissingReferences(string context, GameObject[] objects)
    {
        foreach (GameObject go in objects)
        {
            Component[] components = go.GetComponents<Component>();

            foreach (Component c in components)
            {
                if (c==null)
                {
                    missComp.Add(new MissRef(go, "Missing Component in GO: " + FullPath(go)));
                }
                else
                {
                    SerializedObject so = new SerializedObject(c);
                    var sp = so.GetIterator();

                    while (sp.NextVisible(true))
                    {
                        if (sp.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (sp.objectReferenceValue == null
                                && sp.objectReferenceInstanceIDValue != 0)
                            {
                                missRef.Add(new MissRef(c,
                                    "Missing Property:" + ObjectNames.NicifyVariableName(sp.name) +
                                    " in component: " + c.GetType().Name));
                            }
                        }
                    }
                }
            }
            GameObject[] childrenObjects = new GameObject[go.transform.childCount];
            for (int i = 0; i < childrenObjects.Length; i++)
            {
                childrenObjects[i] = go.transform.GetChild(i).gameObject;
            }
            FindMissingReferences("", childrenObjects);
        }
    }

    private Vector2 classSroll;

    void OnGUI()
    {
        using (var scrollRectLayout = new GUILayout.ScrollViewScope(classSroll))
        {
            classSroll = scrollRectLayout.scrollPosition;
            if (DrawHeader("MissComponent"))
            {
                foreach (MissRef m in missComp)
                {
                    if (GUILayout.Button(m.o.name))
                    {
                        Selection.activeObject = m.o;
                    }

                }
            }

            if (DrawHeader("MissRef"))
            {
                foreach (MissRef m in missRef)
                {
                    if (m.o == null)
                    {
                        continue;
                    }
                    if (GUILayout.Button(m.o.name))
                    {
                        Selection.activeObject = m.o;
                    }
                }
            }
        }
    }

    private bool DrawHeader(string text, string key = "", bool forceOn = false, bool minimalistic = false)
    {
        if (string.IsNullOrEmpty(key))
        {
            key = text;
        }
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
    private static string FullPath(GameObject go)
    {
        return go.transform.parent == null
            ? go.name
            : FullPath(go.transform.parent.gameObject) + "/" + go.name;
    }
}