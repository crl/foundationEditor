using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [CustomEditor(typeof(PlayMakerTrigger))]
    public class PlayMakerTriggerInspector: BaseInspector<PlayMakerTrigger>
    {
        protected override void drawInspectorGUI()
        {

            SerializedProperty property;
            
            property=serializedObject.FindProperty("areaType");
            EditorGUILayout.PropertyField(property);

            property = serializedObject.FindProperty("id");
            EditorGUILayout.PropertyField(property);

            property = serializedObject.FindProperty("filterTag");
            int selectedIndex = TagX.All.IndexOf(property.stringValue);
            if (selectedIndex == -1)
            {
                selectedIndex = 0;
            }
            selectedIndex=EditorGUILayout.Popup("filterTag", selectedIndex, TagX.All.ToArray());
            string selectedValue = TagX.All[selectedIndex];
            if (selectedValue != property.stringValue)
            {
                property.stringValue = selectedValue;
            }

            if (mTarget.areaType == TriggerAreaType.Jump)
            {
                property = serializedObject.FindProperty("reference");
                EditorGUILayout.PropertyField(property);
            }
        }

        [DrawGizmo(GizmoType.NotInSelectionHierarchy|GizmoType.Selected|GizmoType.Pickable)]
        static void DrawGizmo(PlayMakerTrigger trigger, GizmoType gizmoType)
        {
            GameObject go = trigger.reference;
            if (go)
            {
                Handles.color = Color.yellow;
                Vector3 startPoint = trigger.transform.position;
                Vector3 endPoint = go.transform.position;
                Vector3 startTangent = startPoint + Vector3.up * 5;
                Vector3 endTangent = endPoint + Vector3.up * 3;

                Handles.DrawBezier(startPoint, endPoint, startTangent, endTangent, Color.yellow, null, 2f);
                Handles.DrawLine(endPoint, endPoint + new Vector3(1, 1, 0));
                Handles.DrawLine(endPoint, endPoint + new Vector3(-1, 1, 0));
            }
        }

    }
}