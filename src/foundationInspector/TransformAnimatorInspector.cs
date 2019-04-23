using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [CustomEditor(typeof (TransformAnimator))]
    public class TransformAnimatorInspector : BaseInspector<TransformAnimator>
    {
        static Styles s_Styles;
        class Styles
        {
            public GUIStyle thumb2D = "ColorPicker2DThumb";
            public GUIStyle pickerBox = "ColorPickerBox";
            public GUIStyle thumbHoriz = "ColorPickerHorizThumb";
            public GUIStyle header = "ShurikenModuleTitle";
            public GUIStyle headerCheckbox = "ShurikenCheckMark";
            public Vector2 thumb2DSize;

            internal Styles()
            {
                thumb2DSize = new Vector2(
                        !Mathf.Approximately(thumb2D.fixedWidth, 0f) ? thumb2D.fixedWidth : thumb2D.padding.horizontal,
                        !Mathf.Approximately(thumb2D.fixedHeight, 0f) ? thumb2D.fixedHeight : thumb2D.padding.vertical
                        );

                header.font = (new GUIStyle("Label")).font;
                header.border = new RectOffset(15, 7, 4, 4);
                header.fixedHeight = 22;
                header.contentOffset = new Vector2(20f, -2f);
            }
        }

        public bool Header(SerializedProperty group, SerializedProperty enabledField)
        {
            var display = group == null || group.isExpanded;
            var enabled = enabledField != null && enabledField.boolValue;
            var title = group == null ? "Unknown Group" : ObjectNames.NicifyVariableName(group.displayName);

            Rect rect = GUILayoutUtility.GetRect(16f, 22f, s_Styles.header);
            GUI.Box(rect, title, s_Styles.header);

            Rect toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);
            if (Event.current.type == EventType.Repaint)
                s_Styles.headerCheckbox.Draw(toggleRect, false, false, enabled, false);

            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                if (toggleRect.Contains(e.mousePosition) && enabledField != null)
                {
                    enabledField.boolValue = !enabledField.boolValue;
                    e.Use();
                    serializedObject.ApplyModifiedProperties();
                }
                else if (rect.Contains(e.mousePosition) && group != null)
                {
                    display = !display;
                    group.isExpanded = !group.isExpanded;
                    e.Use();
                }
            }
            return display;
        }
        protected override void drawInspectorGUI()
        {
            if (s_Styles == null)
                s_Styles = new Styles();

            mTarget.style = (TransformStyle)EditorGUILayout.EnumPopup("Play Style", mTarget.style);
            mTarget.playOnAwake = EditorGUILayout.Toggle("Play On Awake", mTarget.playOnAwake);

            mTarget.duration = EditorGUILayout.Slider("Duration", mTarget.duration, 0f, 10f);
            mTarget.delay = EditorGUILayout.Slider("Delay", mTarget.delay, 0f, 10f);

            mTarget.progress = EditorGUILayout.Slider("Progress", mTarget.progress, 0f, 1f);

            SerializedProperty hasProperty = serializedObject.FindProperty("hasPosition");
            if (Header(hasProperty, hasProperty))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    mTarget.syncPosition = EditorGUILayout.ToggleLeft("syncXYZ", mTarget.syncPosition);
                    mTarget.isPositionOffset = EditorGUILayout.ToggleLeft("isOffset", mTarget.isPositionOffset);
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        if (mTarget.syncPosition == false)
                        {
                            mTarget.hasPositionX = EditorGUILayout.ToggleLeft("toggleX", mTarget.hasPositionX,
                                GUILayout.Width(80));
                        }
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("animationCurvePosX"),
                            GUIContent.none,
                            GUILayout.MinHeight(100));
                    }
                    if (mTarget.syncPosition == false)
                    {
                        using (new EditorGUILayout.VerticalScope())
                        {
                            mTarget.hasPositionY = EditorGUILayout.ToggleLeft("toggleY", mTarget.hasPositionY,
                                GUILayout.Width(80));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("animationCurvePosY"),
                                GUIContent.none,
                                GUILayout.MinHeight(100));
                        }
                        using (new EditorGUILayout.VerticalScope())
                        {
                            mTarget.hasPositionZ = EditorGUILayout.ToggleLeft("toggleZ", mTarget.hasPositionZ,
                                GUILayout.Width(80));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("animationCurvePosZ"),
                                GUIContent.none,
                                GUILayout.MinHeight(100));
                        }
                    }
                }
                mTarget.endPosition = EditorGUILayout.Vector3Field("position", mTarget.endPosition);
            }

            hasProperty = serializedObject.FindProperty("hasRotation");
            if(Header(hasProperty, hasProperty))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    mTarget.isQuaternionLerp = EditorGUILayout.ToggleLeft("isQuaternionLerp", mTarget.isQuaternionLerp);
                    mTarget.isRotationOffset = EditorGUILayout.ToggleLeft("isOffset", mTarget.isRotationOffset);
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animationCurveRotation"), GUIContent.none,
                    GUILayout.MinHeight(50));
                mTarget.endEuler = EditorGUILayout.Vector3Field("euler", mTarget.endEuler);
            }
            hasProperty = serializedObject.FindProperty("hasScale");
            if (Header(hasProperty, hasProperty))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animationCurveScale"), GUIContent.none,
                     GUILayout.MinHeight(50));
                mTarget.endScale = EditorGUILayout.Vector3Field("scale", mTarget.endScale);
            }

            hasProperty = serializedObject.FindProperty("hasColor");
            if (Header(hasProperty, hasProperty))
            {
                mTarget.isIncludeAll = EditorGUILayout.ToggleLeft("isIncludeAll", mTarget.isIncludeAll);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animationCurveColor"), GUIContent.none,
                     GUILayout.MinHeight(50));
                mTarget.endColor = EditorGUILayout.ColorField("color", mTarget.endColor);
            }
        }
    }
}