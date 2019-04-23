using UnityEngine;
using clayui;
using UnityEditor;

namespace foundationEditor
{
    [CustomEditor(typeof (UITweenColor))]
    public class UITweenColorInspector : UITweenerInspector<UITweenColor>
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorGUIUtility.labelWidth = 120f;

            EditorGUI.BeginChangeCheck();

            Color from = EditorGUILayout.ColorField("From", mTarget.from);
            Color to = EditorGUILayout.ColorField("To", mTarget.to);
            string nameColor = EditorGUILayout.TextField("nameColor", mTarget.nameColor);
            bool isIncludeAll =  EditorGUILayout.Toggle("isIncludeAll", mTarget.isIncludeAll);
          
            if (EditorGUI.EndChangeCheck())
            {
                InspectorToolExtends.RegisterUndo("Tween Change", mTarget);
                mTarget.from = from;
                mTarget.to = to;
                mTarget.isIncludeAll = isIncludeAll;
                mTarget.nameColor = nameColor;
                InspectorToolExtends.SetDirty(mTarget);
            }

            DrawCommonProperties();

        }
    }
}