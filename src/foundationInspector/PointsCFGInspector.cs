using System.Collections.Generic;
using foundation;
using gameSDK;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [CustomEditor(typeof (PointsCFG))]
    public class PointsCFGInspector : BaseInspector<PointsCFG>
    {
        protected static bool ShowPreview = false;
        protected Dictionary<PointVO, GameObject> previewObjects;
        protected override void OnEnable()
        {
            previewObjects=new Dictionary<PointVO, GameObject>();
            mTarget = target as PointsCFG;
            controlSize = 0.5f;
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            foreach (PointVO pointVo in previewObjects.Keys)
            {
                GameObject go=previewObjects[pointVo];
                if (go != null)
                {
                    GameObject.DestroyImmediate(go);
                }
            }
            previewObjects.Clear();
        }

        private static int selectedIndex = -1;
        private ListControlStyle itemControlStyle;
        protected override void drawInspectorGUI()
        {
            if (itemControlStyle == null)
            {
                itemControlStyle = new ListControlStyle();
                itemControlStyle.addButtonContent.text = "添加一点";
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("key"));

            SerializedProperty list = serializedObject.FindProperty("list");
            showElements(list, itemCreateHandle, itemControlStyle);

            if (list.arraySize > 1)
            {
                if (GUILayout.Button("类型排序", EditorStyles.miniButton))
                {
                    mTarget.list.Sort((x, y) =>
                    {
                        return (int)y.type - (int)x.type;
                    });
                }
            }
            EditorGUILayout.Separator();
            ShowPreview = GUILayout.Toggle(ShowPreview, "showPreview");
            drawExportUI("NpcCFG");
        }

        
        private void itemCreateHandle(SerializedProperty list, SerializedProperty item, int index)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            Color oldColor = GUI.contentColor;

            bool isCurrent = selectedIndex == index;
            if (isCurrent)
            {
                GUI.contentColor = Color.yellow;
            }
            EditorGUI.indentLevel++;

            SerializedProperty idProperty = item.FindPropertyRelative("id");
            SerializedProperty pointProperty = item.FindPropertyRelative("position");
            string id = idProperty.stringValue;
            EditorGUI.BeginChangeCheck();
            bool b = EditorGUILayout.Foldout(isCurrent, "m" + (index + 1) + ":" + id);
            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck())
            {
                if (b)
                {
                    selectedIndex = index;
                    EditorUtils.GotoScenePoint(pointProperty.vector3Value);
                }
                else
                {
                    selectedIndex = -1;
                }
            }
            SerializedProperty typeProperty = item.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(typeProperty, GUIContent.none, GUILayout.Width(60));
            b &= showButtons(list, index, itemControlStyle);
            EditorGUILayout.EndHorizontal();
            if (b)
            {
                EditorGUILayout.PropertyField(idProperty);
                EditorGUILayout.BeginHorizontal();
               
                EditorGUILayout.PropertyField(pointProperty);
                if (GUILayout.Button("Raycast", EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    Vector3 v = pointProperty.vector3Value;
                    v.y += 50f;
                    Ray ray = new Ray(v, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100, Physics.AllLayers,QueryTriggerInteraction.Ignore))
                    {
                        Undo.RegisterCompleteObjectUndo(mTarget, "rayCast");
                        pointProperty.vector3Value = hit.point;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(item.FindPropertyRelative("euler"));

                if (string.IsNullOrEmpty(id) == false)
                {
                    ExcelRefVO vo = ExcelIDSelecterDrawer.Get(ExcelFileIDType.Npc, id);
                    if (vo != null)
                    {
                        if (GUILayout.Button("View",EditorStyles.miniButton)) ExcelIDSelecterDrawer.ShowView(vo);
                    }
                }
            }

            GUI.contentColor = oldColor;
            EditorGUILayout.EndVertical();
        }

        protected override void drawSceneGUI()
        {
            foreach (PointVO pointVo in previewObjects.Keys)
            {
                GameObject go = previewObjects[pointVo];
                if (go != null)
                {
                    go.SetRendererEnabledRecursive(false);
                }
            }


            int len = mTarget.list.Count;
            Color color=Handles.color;
            for (int i = 0; i < len; i++)
            {
                PointVO pointCFG = mTarget.list[i];

                if (pointCFG.type == PointType.Entrance)
                {
                    color = Color.green;
                }else if (pointCFG.type == PointType.Entrance)
                {
                    color = Color.blue;
                }
                else if (pointCFG.type == PointType.Exit)
                {
                    color = Color.red;
                }
                else if (pointCFG.type == PointType.Npc)
                {
                    color = Color.yellow;
                }
                else
                {
                    color = Color.white;
                }

                if (i == selectedIndex)
                {
                    color.a = 1.0f;
                }
                else
                {
                    color.a = 0.5f;
                }

                Handles.color=color;
                EditorGUI.BeginChangeCheck();
                float size = HandleUtility.GetHandleSize(pointCFG.position) * controlSize;
                Vector3 newPos = Handles.FreeMoveHandle(pointCFG.position, Quaternion.identity, size / 2, Vector3.zero,
                    Handles.SphereHandleCap);

                Vector3 hitPos = pointCFG.position;
                RaycastHit hit;
                if (Physics.Linecast(pointCFG.position, pointCFG.position - new Vector3(0, 100, 0), out hit))
                {
                    hitPos = hit.point;
                }
                Handles.color = Color.black;
                Handles.DrawLine(pointCFG.position, hitPos);
                Handles.color = color;

                Quaternion q = Quaternion.Euler(0, pointCFG.euler, 0);
                Handles.ArrowHandleCap(0, pointCFG.position, q, size, Event.current.type);

                if (EditorGUI.EndChangeCheck())
                {
                    selectedIndex = i;
                    Undo.RegisterCompleteObjectUndo(mTarget, "move Points");
                    pointCFG.position = newPos;
                }

                Color oldColor = GUI.color;
                GUI.color = Color.white;
                string id = pointCFG.id;
                if (string.IsNullOrEmpty(id))
                {
                    id = "#" + (i + 1) + "#";
                }

                Handles.Label(newPos, id);
                GUI.color = oldColor;

                if (ShowPreview && pointCFG.type == PointType.Npc && string.IsNullOrEmpty(id) == false)
                {
                    GameObject go=null;
                    if (previewObjects.TryGetValue(pointCFG, out go)==false)
                    {
                        ExcelRefVO vo = ExcelIDSelecterDrawer.Get(ExcelFileIDType.Npc, id);
                        if (vo != null)
                        {
                            go=vo.CreateByPrefab();
                            if (go)
                            {
                                previewObjects.Add(pointCFG, go);
                            }
                        }
                    }

                    if (go != null)
                    {
                        
                        go.transform.position = newPos;
                        go.transform.rotation = q;
                        go.SetRendererEnabledRecursive(true);
                    }
                }
            }
        }
    }
}