using foundation;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace foundationEditor
{

    [CustomPropertyDrawer(typeof (ReorderableAttribute), true)]
    public class ReorderableDrawer : PropertyDrawer
    {
        private ReorderableList _list;
        protected virtual ReorderableList GetReorderableList(SerializedProperty listProperty)
        {
            if (_list == null)
            {
                _list = new ReorderableList(listProperty.serializedObject, listProperty, true, true, true, true);
                _list.drawHeaderCallback += delegate(Rect rect)
                {
                    ReorderableAttribute selecterAttribute = attribute as ReorderableAttribute;
                    string title= listProperty.displayName;
                    if (selecterAttribute != null)
                    {
                        title = selecterAttribute.title;
                    }
                    EditorGUI.LabelField(rect, title);
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

            var height = 0f;
            for (var i = 0; i < property.arraySize; i++)
            {
                height = Mathf.Max(height, EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(i)));
            }

            list.elementHeight = height;
            list.DoList(position);
        }
    }
}