using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [CustomPropertyDrawer(typeof(DisableAttribute))]
    public class DisableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndDisabledGroup();
        }
    }
}