using System.Collections.Generic;
using foundation;
using gameSDK;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [CustomEditor(typeof (TeamSpawersCFG))]
    public class TeamSpawersCFGInspector : BaseInspector<TeamSpawersCFG>
    {
        private Vector3[] verts = new Vector3[4];
        public static int zoomIndex = 0;
        public static int monsterIndex = 0;
        protected Dictionary<TeamMonsterCFG, GameObject> previewObjects;
        protected static bool ShowPreview = false;
        protected override void OnEnable()
        {
            previewObjects = new Dictionary<TeamMonsterCFG, GameObject>();
            base.OnEnable();
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            foreach (TeamMonsterCFG cfg in previewObjects.Keys)
            {
                GameObject go = previewObjects[cfg];
                if (go != null)
                {
                    GameObject.DestroyImmediate(go);
                }
            }
            previewObjects.Clear();
        }

        public bool Header(SerializedProperty group, string title,int index,Rect zoomRect)
        {
            Rect rect = GUILayoutUtility.GetRect(5, 22f, headStyle.header);
            GUI.Box(rect, title, headStyle.header);

            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                if (rect.Contains(e.mousePosition) && group != null)
                {
                    if (zoomIndex != index)
                    {
                        zoomIndex = index;
                        EditorUtils.GotoScenePoint(new Vector3(zoomRect.x,mTarget.transform.position.y, zoomRect.y));
                    }
                    else
                    {
                        zoomIndex = -1;
                    }
                    GUIUtility.hotControl = 0;
                    e.Use();
                }
            }
            return zoomIndex==index;
        }
        private ListControlStyle zoneControlStyle;
        protected override void drawInspectorGUI()
        {
            if (zoneControlStyle == null)
            {
                zoneControlStyle = new ListControlStyle();
                zoneControlStyle.addButtonContent.text = "添加一组";
            }

            SerializedProperty list = serializedObject.FindProperty("list");
       
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fileName"));
            showElements(list, zoneCreateHandle, zoneControlStyle);

            EditorGUILayout.Separator();
            ShowPreview = GUILayout.Toggle(ShowPreview, "showPreview");
            drawExportUI("TeamSpawersCFG");
        }

        protected override void drawSceneGUI()
        {
            foreach (TeamMonsterCFG cfg in previewObjects.Keys)
            {
                GameObject go = previewObjects[cfg];
                if (go != null)
                {
                    go.SetRendererEnabledRecursive(false);
                }
            }

            List<TeamCFG> list = mTarget.list;
            if (list == null || list.Count <= 0) return;

            Event e = Event.current;

            Vector3 position;
            int len = list.Count;

            bool isCurrentZoom = false;

            Color color = Color.red;
            float centerY = 0;

            int[] zoomHosControl = new int[len];
            for (int i = 0; i < len; i++)
            {
                isCurrentZoom = zoomIndex == i;
                TeamCFG teamCfg = list[i];
                Rect rect = teamCfg.rect;
                position = new Vector3(rect.center.x, 0, rect.center.y);

                if (isCurrentZoom == false)
                {
                    color.a = 0.3f;
                }
                else
                {
                    color.a = 1.0f;
                }
                Handles.color = color;
                GUI.color = color;

                Vector3 rectCenter = new Vector3(rect.x, 0, rect.y);

                verts[0] = new Vector3(rect.x - rect.width / 2f, centerY, rect.y - rect.height / 2f);
                verts[1] = new Vector3(rect.x + rect.width / 2f, centerY, rect.y - rect.height / 2f);
                verts[2] = new Vector3(rect.x + rect.width / 2f, centerY, rect.y + rect.height / 2f);
                verts[3] = new Vector3(rect.x - rect.width / 2f, centerY, rect.y + rect.height / 2f);

                Handles.DrawSolidRectangleWithOutline(verts, rectSolideColor, Color.gray);

                float size = HandleUtility.GetHandleSize(rectCenter) * controlSize;
                Vector3 newPos;

                newPos = Handles.FreeMoveHandle(rectCenter, Quaternion.identity, size / 2, Vector3.zero,
                    (controlID, p, rotation, s, evenType) =>
                    {
                        zoomHosControl[i] = controlID;
                        Handles.DotHandleCap(controlID, p, rotation, s, evenType);
                    });

                if (GUIUtility.hotControl == zoomHosControl[i])
                {
                    if (GUIUtility.hotControl != 0 && e.type == EventType.Repaint)
                    {
                        zoomIndex = i;
                        Repaint();
                    }
                }

                if (rectCenter != newPos)
                {
                    Undo.RecordObject(mTarget, "TeamMove");
                    rect.x = newPos.x;
                    rect.y = newPos.z;
                    teamCfg.rect = rect;
                }

                int jLen = teamCfg.list.Count;
                int[] monsterHosControl = new int[jLen];
                for (int j = 0; j < jLen; j++)
                {
                    TeamMonsterCFG monsterCfg = teamCfg.list[j];
                    position = rectCenter + monsterCfg.position;

                    size = HandleUtility.GetHandleSize(position) * controlSize;
                    newPos = Handles.FreeMoveHandle(position, Quaternion.identity, size, Vector3.zero,
                        (int controlID, Vector3 pos, Quaternion rotation, float s, EventType et) =>
                        {
                            monsterHosControl[j] = controlID;
                            if (j == monsterIndex && isCurrentZoom)
                            {
                                Handles.color = Color.green;
                            }
                            Handles.SphereHandleCap(controlID, pos, rotation, s, et);
                            Handles.color = color;
                        });

                    if (GUIUtility.hotControl == monsterHosControl[j] && GUIUtility.hotControl != 0 && isCurrentZoom)
                    {
                        if (e.type == EventType.Repaint)
                        {
                            monsterIndex = j;
                            Repaint();
                        }
                    }

                    Quaternion q = Quaternion.Euler(0, monsterCfg.euler, 0);
                    Handles.ArrowHandleCap(0, position, q, size, Event.current.type);

                    Vector3 hitPos = new Vector3(monsterCfg.position.x, 0, monsterCfg.position.z);
                    RaycastHit hit;
                    if (Physics.Linecast(monsterCfg.position, monsterCfg.position - new Vector3(0, 100, 0), out hit))
                    {
                        hitPos = hit.point;
                    }
                    Handles.color = Color.black;
                    Handles.DrawLine(monsterCfg.position, hitPos);
                    Handles.color = color;

                    if (position != newPos && isCurrentZoom)
                    {
                        Undo.RegisterCompleteObjectUndo(mTarget, "MonsterMove");
                        monsterCfg.position = newPos - rectCenter;
                    }

                    Color oldColor = GUI.color;
                    GUI.color = Color.white;
                    string monsterId = monsterCfg.id;
                    if (string.IsNullOrEmpty(monsterId))
                    {
                        monsterId = "#" + (j + 1) + "#";
                    }
                    else if(ShowPreview)
                    {
                        GameObject go = null;
                        if (previewObjects.TryGetValue(monsterCfg, out go) == false)
                        {
                            ExcelRefVO vo = ExcelIDSelecterDrawer.Get(ExcelFileIDType.Monster, monsterId);
                            if (vo != null)
                            {
                                go = vo.CreateByPrefab();
                                if (go)
                                {
                                    previewObjects.Add(monsterCfg, go);
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
                    Handles.Label(position, monsterId);
                    GUI.color = oldColor;
                }

                GUI.color = Color.white;
                Handles.Label(rectCenter, "Team" + (i + 1), headStyle.zoomText);
            }
            GUI.color = Color.white;
        }

        private void OnDestory()
        {

        }
        private ListControlStyle monsterControlStyle;
        private SerializedProperty rectProperty;
        private void zoneCreateHandle(SerializedProperty list, SerializedProperty item, int index)
        {
           bool b = false;

            if (zoomIndex == index)
            {
                GUILayout.BeginVertical(headStyle.box);
            }

            if (monsterControlStyle == null)
            {
                monsterControlStyle = new ListControlStyle();
                monsterControlStyle.addButtonContent.text = "添加一只怪";
            }

         
            rectProperty = item.FindPropertyRelative("rect");
            if (rectProperty.rectValue.size == Vector2.zero)
            {
                rectProperty.rectValue = new Rect(Vector2.zero, Vector2.one * 3);
                serializedObject.ApplyModifiedProperties();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                b = Header(item, "Team " + (index + 1), index, rectProperty.rectValue);
       
                b &= showButtons(list, index, zoneControlStyle);
            }
            if (b)
            {

                EditorGUILayout.PropertyField(item.FindPropertyRelative("key"));
                EditorGUILayout.PropertyField(rectProperty, GUIContent.none);
                EditorGUILayout.PropertyField(item.FindPropertyRelative("resetTime"));

                SerializedProperty slotsProperty = item.FindPropertyRelative("list");
                showElements(slotsProperty, monsterCreateHandle, monsterControlStyle);
            }

            if (zoomIndex == index)
            {
                GUILayout.EndVertical();
            }
        }
     
        private void monsterCreateHandle(SerializedProperty list,SerializedProperty item, int index)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            
            Color oldColor=GUI.contentColor;

            bool isCurrent = monsterIndex == index;
            if (isCurrent)
            {
                GUI.contentColor = Color.yellow;
            }
            EditorGUI.indentLevel++;

            string id=item.FindPropertyRelative("id").stringValue;

            EditorGUI.BeginChangeCheck();
            bool b = EditorGUILayout.Foldout(isCurrent, "m" + (index + 1) + ":" + id);
            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck())
            {
                if (b)
                {
                    monsterIndex = index;
                }
                else
                {
                    monsterIndex = -1;
                }
            }
            b &= showButtons(list, index, monsterControlStyle);
            EditorGUILayout.EndHorizontal();

            if (b)
            {
                SerializedProperty idProperty=item.FindPropertyRelative("id");
                EditorGUILayout.PropertyField(idProperty);

                idProperty = item.FindPropertyRelative("type");
                EditorGUILayout.PropertyField(idProperty);

                EditorGUILayout.BeginHorizontal();

                SerializedProperty pointProperty = item.FindPropertyRelative("position");
                EditorGUILayout.PropertyField(pointProperty);
                if (GUILayout.Button("Raycast", EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    Rect rect = rectProperty.rectValue;
                    Vector3 rectCenter = new Vector3(rect.x, 0, rect.y);
                    Vector3 v = pointProperty.vector3Value+ rectCenter;
                    v.y += 50f;
                    Ray ray = new Ray(v, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit,100, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                    {
                        Undo.RegisterCompleteObjectUndo(mTarget, "rayCast");
                        pointProperty.vector3Value = hit.point-rectCenter;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(item.FindPropertyRelative("euler"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("enterAction"));

                if (string.IsNullOrEmpty(id) == false)
                {
                    ExcelRefVO vo = ExcelIDSelecterDrawer.Get(ExcelFileIDType.Monster, id);
                    if (vo != null)
                    {
                        if(GUILayout.Button("View", EditorStyles.miniButton))ExcelIDSelecterDrawer.ShowView(vo);
                    }
                }

            }

            GUI.contentColor = oldColor;
            EditorGUILayout.EndVertical();
        }
    }
}