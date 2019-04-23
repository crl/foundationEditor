using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class BoundsContrl
    {
        public bool toggle = false;
        public string label;
        public Color color = Color.red;

        public BoundsContrl(string label)
        {
            this.label = label;
        }

        public Bounds drawInspector(Bounds bound)
        {
            toggle = EditorGUILayout.Foldout(toggle, label);
            if (toggle)
            {
                Vector3 min = EditorGUILayout.Vector3Field("左下角", bound.min);
                Vector3 size = EditorGUILayout.Vector3Field("大小", bound.size);

                Vector3 max = new Vector3(min.x + size.x, min.y + size.y, min.z + size.z);
                bound = new Bounds();
                bound.SetMinMax(min, max);
            }

            return bound;
        }
    }
}