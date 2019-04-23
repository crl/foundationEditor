using UnityEngine;
using UnityEditor;
using clayui;

namespace foundationEditor
{
    [CustomEditor(typeof (UITweenScale))]
    public class UITweenScaleInspector : UITweenerInspector<UITweenScale>
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorGUIUtility.labelWidth = 120f;

            EditorGUI.BeginChangeCheck();

            Vector3 from = EditorGUILayout.Vector3Field("From", mTarget.from);
            Vector3 to = EditorGUILayout.Vector3Field("To", mTarget.to);

            if (EditorGUI.EndChangeCheck())
            {
                InspectorToolExtends.RegisterUndo("Tween Change", mTarget);
                mTarget.from = from;
                mTarget.to = to;
                InspectorToolExtends.SetDirty(mTarget);
            }

            DrawCommonProperties();

        }
    }
}
