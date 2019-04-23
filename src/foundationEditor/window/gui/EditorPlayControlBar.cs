using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class EditorPlayControlBar : EditorUI
    {
        public override void onRender()
        {

           /* Color contentColor = GUI.contentColor;
            GUI.contentColor = ((!FsmEditorStyles.UsingProSkin()) ? Color.black : EditorStyles.label.normal.textColor);

            EditorGUI.BeginChangeCheck();
            bool isPaused = GUILayout.Toggle(EditorApplication.isPaused, FsmEditorContent.Pause,
                EditorStyles.toolbarButton, new GUILayoutOption[]
                {
                    GUILayout.MaxWidth(40f)
                });
            if (EditorGUI.EndChangeCheck())
            {
                EditorApplication.isPaused = isPaused;
            }

            if (GUILayout.Button(FsmEditorContent.Step, EditorStyles.toolbarButton, new GUILayoutOption[]
            {
                GUILayout.MaxWidth(40f)
            }))
            {
                FsmDebugger.Instance.Step();
                GUIUtility.ExitGUI();
            }

            EditorGUI.BeginChangeCheck();
            bool isPlaying = GUILayout.Toggle(EditorApplication.isPlayingOrWillChangePlaymode, FsmEditorContent.Play,
                EditorStyles.toolbarButton, new GUILayoutOption[]
                {
                    GUILayout.MaxWidth(40f)
                });
            if (EditorGUI.EndChangeCheck())
            {
                EditorApplication.isPlaying = isPlaying;
            }

            GUI.contentColor = contentColor;*/

        }
    }
}