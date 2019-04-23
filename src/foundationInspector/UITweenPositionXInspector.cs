using clayui;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [CustomEditor(typeof(UITweenPositionX))]
    public class UITweenPositionXInspector : UITweenerInspector<UITweenPositionX>
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorGUIUtility.labelWidth = 120f;

            EditorGUI.BeginChangeCheck();

            float from = EditorGUILayout.FloatField("From", mTarget.from);
            float to = EditorGUILayout.FloatField("To", mTarget.to);

         
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

    [CustomEditor(typeof(UITweenPositionY))]
    public class UITweenPositionYInspector : UITweenerInspector<UITweenPositionY>
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorGUIUtility.labelWidth = 120f;

            EditorGUI.BeginChangeCheck();

            float from = EditorGUILayout.FloatField("From", mTarget.from);
            float to = EditorGUILayout.FloatField("To", mTarget.to);


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

    [CustomEditor(typeof(UITweenPositionZ))]
    public class UITweenPositionZInspector : UITweenerInspector<UITweenPositionZ>
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(6f);
            EditorGUIUtility.labelWidth = 120f;

            EditorGUI.BeginChangeCheck();

            float from = EditorGUILayout.FloatField("From", mTarget.from);
            float to = EditorGUILayout.FloatField("To", mTarget.to);


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