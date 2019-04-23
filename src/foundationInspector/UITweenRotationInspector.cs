using UnityEngine;
using UnityEditor;
using clayui;

namespace foundationEditor
{
    [CustomEditor(typeof (UITweenRotation))]
    public class UITweenRotationInspector : UITweenerInspector<UITweenRotation>
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorGUIUtility.labelWidth = 120f;

            EditorGUI.BeginChangeCheck();

            Vector3 from = EditorGUILayout.Vector3Field("From", mTarget.from);
            Vector3 to = EditorGUILayout.Vector3Field("To", mTarget.to);

            bool quaternionLerp = EditorGUILayout.Toggle("quaternionLerp", mTarget.quaternionLerp);

            if (EditorGUI.EndChangeCheck())
            {
                InspectorToolExtends.RegisterUndo("Tween Change", mTarget);
                mTarget.from = from;
                mTarget.to = to;
                mTarget.quaternionLerp = quaternionLerp;
                InspectorToolExtends.SetDirty(mTarget);
            }
            DrawCommonProperties();
        }
    }
}

