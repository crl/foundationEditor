using System;
using System.IO;
using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [CustomPropertyDrawer(typeof(ObjectSelecterAttribute))]
    public class ObjectSelecterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float w = position.width;
            position.width = w - 16;
            EditorGUI.PropertyField(position, property);

            position.x += position.width;
            position.width = 16;
            if (GUI.Button(position, "s", EditorStyles.miniButton))
            {
                ObjectSelecterAttribute selecterAttribute = attribute as ObjectSelecterAttribute;
                //Type extention = selecterAttribute.type;
                ObjectSelectorWindow.ShowObjectPicker(selecterAttribute.type, property.objectReferenceValue,property, null,
                    selecterAttribute.path);
            }
        }
    }
}