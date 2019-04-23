using clayui;
using UnityEngine;
using UnityEditor;

namespace foundationEditor
{

    [CustomEditor(typeof (UITweener))]
    public class UITweenerInspector<T> : BaseInspector<T> where T : UITweener
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorGUIUtility.labelWidth = 110f;
            //base.OnInspectorGUI();
            DrawCommonProperties();
        }

        protected void DrawCommonProperties()
        {
            if (InspectorToolExtends.DrawHeader("Tweener"))
            {
                InspectorToolExtends.BeginContents();
                EditorGUIUtility.labelWidth = 110f;

                EditorGUI.BeginChangeCheck();

                AnimationCurve curve = EditorGUILayout.CurveField("Animation Curve", mTarget.animationCurve,
                    GUILayout.Width(170f),
                    GUILayout.Height(62f));
                UITweener.Style style = (UITweener.Style) EditorGUILayout.EnumPopup("Play Style", mTarget.style);
                UITweener.Method moveType =
                    (UITweener.Method) EditorGUILayout.EnumPopup("Play MoveType", mTarget.method);

                GUILayout.BeginHorizontal();
                float dur = EditorGUILayout.FloatField("Duration", mTarget.duration, GUILayout.Width(170f));
                GUILayout.Label("seconds");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                float del = EditorGUILayout.FloatField("Start Delay", mTarget.delay, GUILayout.Width(170f));
                GUILayout.Label("seconds");
                GUILayout.EndHorizontal();

                bool ts = EditorGUILayout.Toggle("Ignore TimeScale", mTarget.ignoreTimeScale);

                if (EditorGUI.EndChangeCheck())
                {
                    InspectorToolExtends.RegisterUndo("Tween Change", mTarget);
                    mTarget.animationCurve = curve;
                    mTarget.method = moveType;
                    mTarget.style = style;
                    mTarget.ignoreTimeScale = ts;
                    mTarget.duration = dur;
                    mTarget.delay = del;
                    InspectorToolExtends.SetDirty(mTarget);
                }
                InspectorToolExtends.EndContents();
            }
        }

    }
}