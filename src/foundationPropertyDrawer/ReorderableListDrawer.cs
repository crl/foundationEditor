using foundation;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace foundationEditor
{

    [CustomPropertyDrawer(typeof (ReorderableListBase), true)]
    public class ReorderableListDrawer : PropertyDrawer
    {
        private ReorderableList _list;
        protected virtual ReorderableList GetReorderableList(SerializedProperty property)
        {
            if (_list == null)
            {
                SerializedProperty listProperty = property.FindPropertyRelative("_list");

                _list = new ReorderableList(property.serializedObject, listProperty, true, true, true, true);

                _list.drawHeaderCallback += delegate(Rect rect)
                {
                    EditorGUI.LabelField(rect, property.displayName);
                };

                _list.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused)
                {
                    EditorGUI.PropertyField(rect, listProperty.GetArrayElementAtIndex(index), true);
                };
            }

            return _list;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetReorderableList(property).GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ReorderableList list = GetReorderableList(property);

            var listProperty = property.FindPropertyRelative("_list");
            var height = 0f;
            for (var i = 0; i < listProperty.arraySize; i++)
            {
                height = Mathf.Max(height, EditorGUI.GetPropertyHeight(listProperty.GetArrayElementAtIndex(i)));
            }

            list.elementHeight = height;
            list.DoList(position);
        }
    }
}