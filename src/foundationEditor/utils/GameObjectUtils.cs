using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class GameObjectUtils
    {
        public static bool hasMiss(GameObject go, bool tip = true)
        {
            Component[] components = go.GetComponents<Component>();
            bool has = false;
            foreach (var c in components)
            {
                if (c == null)
                {
                    has = true;
                    if (tip)
                    {
                        Debug.LogError("Missing Component in GO: " + go.name, go);
                    }
                    else
                    {
                        Debug.LogError("had miss Component in GO: " + go.name, go);
                        return true;
                    }
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
                            has = true;
                            if (tip == false) return true;
                            Debug.LogError(
                                "Missing Property:" + ObjectNames.NicifyVariableName(sp.name) +
                                " in component: " + c.GetType().Name, c);
                        }
                    }
                }
            }
            int len = go.transform.childCount;
            for (int i = 0; i < len; i++)
            {
                has |= hasMiss(go.transform.GetChild(i).gameObject, tip);
                if (tip == false && has)
                {
                    Debug.LogError("had miss Component in GO: " + go.name+"  child:"+ go.transform.GetChild(i).gameObject, go.transform.GetChild(i).gameObject);
                    return true;
                }
            }

            return has;
        }



    }
}
