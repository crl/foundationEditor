using foundation;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(Hypertext), true)]
public class HypertextInspector : TextEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Hypertext m_Target = target as Hypertext;
        m_Target.spriteOffsetY =EditorGUILayout.FloatField("SpriteOffsetY", m_Target.spriteOffsetY);
    }
}

