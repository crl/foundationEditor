using UnityEngine;
using UnityEditor;
using clayui;

namespace foundationEditor
{
    [CustomEditor(typeof (UITweenPosition))]
    public class UITweenPositionInspector : UITweenerInspector<UITweenPosition>
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorGUIUtility.labelWidth = 120f;

            EditorGUI.BeginChangeCheck();

            Vector3 from = EditorGUILayout.Vector3Field("From", mTarget.from);
            Vector3 to = EditorGUILayout.Vector3Field("To", mTarget.to);

            bool worldSpace = EditorGUILayout.Toggle("WorldSpace", mTarget.worldSpace);

            if (EditorGUI.EndChangeCheck())
            {
                InspectorToolExtends.RegisterUndo("Tween Change", mTarget);
                mTarget.from = from;
                mTarget.to = to;
                mTarget.worldSpace = worldSpace;
                InspectorToolExtends.SetDirty(mTarget);
            }

            DrawCommonProperties();

        }
    }
}
