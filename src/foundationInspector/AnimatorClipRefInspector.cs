using foundation;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace foundationEditor
{
    [CustomEditor(typeof(AnimatorClipRef))]
    public class AnimatorClipRefInspector:BaseInspector<AnimatorClipRef>
    {
        private ReorderableList animationClipUIList;
        private ReorderableList placeholderClipUIList;

        protected override void OnEnable()
        {
            base.OnEnable();
            animationClipUIList = new ReorderableList(serializedObject,
                serializedObject.FindProperty("animationClips"),
                true, true, true, true);
            animationClipUIList.drawHeaderCallback = (Rect rect) =>
            {
                GUI.Label(rect, "AnimationClips");
            };

            animationClipUIList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = animationClipUIList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                if (element.objectReferenceValue == null)
                {
                    GUI.color = Color.red;
                }
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    element,GUIContent.none);
                GUI.color = Color.white;
            };


            placeholderClipUIList = new ReorderableList(serializedObject,
                serializedObject.FindProperty("placeholderClips"),
                true, true, true, true);
            placeholderClipUIList.drawHeaderCallback = (Rect rect) =>
            {
                GUI.Label(rect, "placeholders");
            };

            placeholderClipUIList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = placeholderClipUIList.serializedProperty.GetArrayElementAtIndex(index);
                if (element.objectReferenceValue == null)
                {
                    GUI.color = Color.red;
                }
                rect.y += 2;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    element, GUIContent.none);
                GUI.color = Color.white;
            };
        }

        protected override void drawInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            animationClipUIList.DoLayoutList();
            placeholderClipUIList.DoLayoutList();

            SerializedProperty p = serializedObject.FindProperty("controller");
            EditorGUILayout.PropertyField(p);
            
           
            serializedObject.ApplyModifiedProperties();
        }
    }
}