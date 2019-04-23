using System;
using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{

    [Serializable]
    public class KeyFrameInfo
    {
        public float time;
        public string func;
        internal string stringParameter;
        public int intParameter;
        public float floatParameter;
        public int objectReferenceParameterInstanceID = -1;
    }
    /*
    [InitializeOnLoad]
    public static class EditorCore
    {
        static EditorCore()
        {
            foundation.ObjectFactory.registerClassAlias<KeyFrameInfo>("vo.KeyFrameInfo");

            EditorApplication.hierarchyWindowItemOnGUI += hierarchyWindowItemOnGUI;
        }

        static void hierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go != null)
            {
                if (Event.current.keyCode == KeyCode.S && (Event.current.control|| Event.current.shift) && go == Selection.activeGameObject)
                {

                    PrefabType prefabType=PrefabUtility.GetPrefabType(go);
                    if (prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance)
                    {
                        GameObject source = PrefabUtility.FindValidUploadPrefabInstanceRoot(go);
                        if (source != null)
                        {
                            var prefabSrc = PrefabUtility.GetPrefabParent(source);
                            if (prefabSrc != null)
                            {
                                if (Event.current.control)
                                {
                                    PrefabUtility.ReplacePrefab(source, prefabSrc,
                                        ReplacePrefabOptions.ConnectToPrefab);
                                }else if (Event.current.shift)
                                {
                                    Selection.activeObject = prefabSrc;
                                    EditorGUIUtility.PingObject(prefabSrc);
                                }
                            }
                        }
                    }
                }
            }
        }
    }*/
}