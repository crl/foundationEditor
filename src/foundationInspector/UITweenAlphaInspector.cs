using UnityEngine;
using clayui;
using UnityEditor;

namespace foundationEditor
{
    [CustomEditor(typeof(UITweenAlpha))]
    public class UITweenAlphaInspector : UITweenerInspector<UITweenAlpha>
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorGUIUtility.labelWidth = 120f;

            EditorGUI.BeginChangeCheck();

            float from = EditorGUILayout.FloatField("From", mTarget.from);
            float to = EditorGUILayout.FloatField("To", mTarget.to);
            //bool isIncludeAll = EditorGUILayout.Toggle("isIncludeAll", mTarget.isIncludeAll);

            if (EditorGUI.EndChangeCheck())
            {
                InspectorToolExtends.RegisterUndo("Tween Change", mTarget);
                mTarget.from = from;
                mTarget.to = to;
                //mTarget.isIncludeAll = isIncludeAll;
                InspectorToolExtends.SetDirty(mTarget);
            }

            DrawCommonProperties();

        }
    }
}