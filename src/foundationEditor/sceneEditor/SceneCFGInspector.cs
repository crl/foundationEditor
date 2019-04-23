using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [CustomEditor(typeof(SceneCFG))]
    public class SceneCFGInspector : BaseInspector<SceneCFG>
    {
        protected override void drawInspectorGUI()
        {
            base.drawInspectorGUI();

            if (GUILayout.Button("editor"))
            {
                SceneConfigEditor editor=EditorWindow.GetWindow<SceneConfigEditor>();
                editor.init(this);
            }
        }
    }
}