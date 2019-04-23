using foundation;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace foundationEditor
{
    [CustomEditor(typeof(SpriteRef))]
    [CanEditMultipleObjects]
    public class SpriteRefInspector : BaseInspector<SpriteRef>
    {
        private ReorderableList reorderableList;
        protected override void OnEnable()
        {
            base.OnEnable();

            SerializedProperty p = serializedObject.FindProperty("spriteSet");
            reorderableList = new ReorderableList(serializedObject, p, true, true, true, true);

            reorderableList.drawHeaderCallback = (Rect rect) => { GUI.Label(rect, "SpriteSet"); };
            reorderableList.onRemoveCallback = (ReorderableList list) =>
            {
                if (EditorUtility.DisplayDialog("警告", "是否真的要删除这个名称？", "是", "否"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };
            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                var textuRelative = element.FindPropertyRelative("sprite");
                if (textuRelative.objectReferenceValue == null)
                {
                    GUI.color = Color.red;
                }

                rect.y += 2;
                float width = rect.width - 80;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight),
                    textuRelative, GUIContent.none);

                var keyRelative = element.FindPropertyRelative("name");
                EditorGUI.PropertyField(new Rect(rect.x + width, rect.y, 80, EditorGUIUtility.singleLineHeight),
                    keyRelative, GUIContent.none);

                GUI.color = Color.white;
            };
        }


        protected override void drawInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("errorSprite"));
            reorderableList.DoLayoutList();
        }
    }
}