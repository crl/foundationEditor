using System;
using System.Collections.Generic;
using System.IO;
using foundation;
using gameSDK;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace foundationEditor
{
    [CustomEditor(typeof (SpawersCFG))]
    public class SpawersCFGInspector : BaseInspector<SpawersCFG>
    {
        private Vector3[] verts = new Vector3[4];
        public static int zoomIndex = 0;
        public static int waveIndex = 0;
        public static int monsterIndex = 0;

        private SceneCameraPreview _sceneCameraPreview;
        private ReorderableList _list = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            HashSet<string> keyIn = new HashSet<string>();
            foreach (SpawersZoneVO pathCfg in mTarget.list)
            {
                string guid = pathCfg.guid;
                if (keyIn.Contains(guid))
                {
                    guid = Guid.NewGuid().ToString();
                    pathCfg.guid = guid;
                }
                keyIn.Add(guid);

            }


            SerializedProperty list = serializedObject.FindProperty("connList");
            if (_list == null)
            {
                _list = new ReorderableList(serializedObject, list, true, true, true, true);
                _list.drawHeaderCallback = delegate (Rect rect)
                {
                    EditorGUI.LabelField(rect, "连接点");
                };

                _list.drawElementCallback = delegate (Rect rect, int i, bool isActive, bool isFocused)
                {
                    rect.y += 2;

                    float w = rect.width / 2;
                    rect.height -= 4;
                    SerializedProperty s = list.GetArrayElementAtIndex(i);
                    rect.width = w;
                    EditorGUI.PropertyField(rect, s.FindPropertyRelative("x"), GUIContent.none);
                    rect.x += w + 10;
                    rect.width = w - 20;
                    EditorGUI.PropertyField(rect, s.FindPropertyRelative("y"), GUIContent.none);
                };
                _list.onCanRemoveCallback = (ReorderableList l) => {
                    return l.count > 0;
                };
                _list.onRemoveCallback = (ReorderableList l) =>
                {
                    if (EditorUtility.DisplayDialog("Warning!",
                        "Are you sure you want to delete the conn?", "Yes", "No"))
                    {
                        ReorderableList.defaultBehaviours.DoRemoveButton(l);
                    }
                };
            }
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
                        waveIndex = monsterIndex = 0;
                        EditorUtils.GotoScenePoint(new Vector3(zoomRect.x,0, zoomRect.y));
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
            if (_sceneCameraPreview == null)
            {
                _sceneCameraPreview = new SceneCameraPreview();
            }

            if (zoneControlStyle == null)
            {
                zoneControlStyle = new ListControlStyle();
                zoneControlStyle.addButtonContent.text = "添加一波刷怪";
            }

            SerializedProperty list = serializedObject.FindProperty("list");
       
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fileName"));
            showElements(list, zoneCreateHandle, zoneControlStyle,zoomItemAddBackHandle);

            _list.DoLayoutList();


            EditorGUILayout.Separator();

            drawExportUI();
        }

        private void zoomItemAddBackHandle(SerializedProperty zoom)
        {
            zoom.FindPropertyRelative("guid").stringValue = Guid.NewGuid().ToString();
        }

        private void drawExportUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("导入"))
                {
                    string sceneName = EditorSceneManager.GetActiveScene().name;
                    string mapPathPrefix = EditorConfigUtils.GetProjectResource("All/map/" + sceneName + "/");
                    FileHelper.AutoCreateDirectory(mapPathPrefix);

                    string path = EditorUtility.OpenFilePanel("选择文件", mapPathPrefix, "json");
                    if (string.IsNullOrEmpty(path) == false)
                    {
                        string json = FileHelper.GetUTF8Text(path);
                        JsonUtility.FromJsonOverwrite(json, mTarget);
                        mTarget.fileName = Path.GetFileNameWithoutExtension(path);
                    }
                }
                if (GUILayout.Button("导出"))
                {
                    string sceneName = EditorSceneManager.GetActiveScene().name;
                    string mapPathPrefix = EditorConfigUtils.GetProjectResource("All/map/" + sceneName + "/");
                    FileHelper.AutoCreateDirectory(mapPathPrefix);

                    string fileName = mTarget.fileName;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = "default";
                    }
                    
                    string path = EditorUtility.SaveFilePanel("选择文件", mapPathPrefix, fileName, "json");
                    if (string.IsNullOrEmpty(path) == false)
                    {
                        mTarget.fileName = Path.GetFileNameWithoutExtension(path);
                        string json = JsonUtility.ToJson(mTarget, false);
                        FileHelper.SaveUTF8(json, path);

                    }
                }
            }
        }

        protected override void drawSceneGUI()
        {
            List<SpawersZoneVO> list = mTarget.list;
            if (list==null || list.Count <= 0) return;

            Event e = Event.current;
            bool isMouseUp = false;
            if (e.type == EventType.MouseUp)
            {
                isMouseUp = true;
            }
            bool isShift = e.shift;

            Vector3 position;
            int len = list.Count;

            bool isCurrentZoom = false;
            bool isCurrentWave = false;

            Color color=Color.red;
            float centerY = 0;

            int[] zoomHosControl = new int[len];
            for (int i = 0; i < len; i++)
            {
                isCurrentZoom = zoomIndex == i;
                SpawersZoneVO spawersZoneVo = list[i];
                Rect rect = spawersZoneVo.rect;
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
                //Vector3 rectSize = new Vector3(rect.width, 0, rect.height);

                verts[0] = new Vector3(rect.x - rect.width/2f, centerY, rect.y - rect.height/2f);
                verts[1] = new Vector3(rect.x + rect.width/2f, centerY, rect.y - rect.height/2f);
                verts[2] = new Vector3(rect.x + rect.width/2f, centerY, rect.y + rect.height/2f);
                verts[3] = new Vector3(rect.x - rect.width/2f, centerY, rect.y + rect.height/2f);

                Handles.DrawSolidRectangleWithOutline(verts, rectSolideColor, Color.gray);

                float size = HandleUtility.GetHandleSize(rectCenter)*controlSize;
                Vector3 newPos;

                newPos = Handles.FreeMoveHandle(rectCenter, Quaternion.identity, size/2, Vector3.zero,
                    (controlID, p, rotation, s,evenType) =>
                    {
                        zoomHosControl[i] = controlID;
                        Handles.DotHandleCap(controlID, p, rotation, s,evenType);
                    });

                if (GUIUtility.hotControl == zoomHosControl[i])
                {
                    if (isShift)
                    {
                        Vector2 v = Event.current.mousePosition;
                        Ray ray = HandleUtility.GUIPointToWorldRay(v);
                        Vector3 rayPoint = ray.origin;
                        Handles.DrawLine(rectCenter, rayPoint);

                        if (isMouseUp)
                        {
                            SpawersZoneVO refVector2 =
                                mTarget.getNearSpawersZoneCfg(new Vector2(rayPoint.x, rayPoint.z));
                            if (refVector2 != null)
                            {
                                KeyVector2 intVector2 = new KeyVector2();
                                intVector2.x = spawersZoneVo.getGUID();
                                intVector2.y = refVector2.getGUID();
                                mTarget.addConn(intVector2);
                            }
                            isMouseUp = false;
                        }
                    }
                    else if (GUIUtility.hotControl!=0)
                    {
                        zoomIndex = i;
                        Repaint();
                    }
                }

                if (rectCenter != newPos && isCurrentZoom && isShift == false)
                {
                    Undo.RecordObject(mTarget, "ZoneMove");
                    rect.x = newPos.x;
                    rect.y = newPos.z;
                    spawersZoneVo.rect = rect;
                }

                int jLen = spawersZoneVo.list.Count;
                for (int j = 0; j < jLen; j++)
                {
                    WaveCFG waveCfg = spawersZoneVo.list[j];
                    isCurrentWave = isCurrentZoom && (j == waveIndex);
                    int kLen = waveCfg.list.Count;

                    int[] monsterHosControl = new int[kLen];
                    for (int k = 0; k < kLen; k++)
                    {
                        MonsterCFG monsterCfg = waveCfg.list[k];
                        position = rectCenter + new Vector3(monsterCfg.position.x, 0, monsterCfg.position.y);
                        if (isCurrentWave && isShift==false)
                        {
                            size = HandleUtility.GetHandleSize(position)*controlSize;
                            newPos = Handles.FreeMoveHandle(position, Quaternion.identity, size, Vector3.zero,
                                (int controlID, Vector3 pos, Quaternion rotation, float s,EventType et) =>
                                {
                                    monsterHosControl[k] = controlID;
                                    if (k == monsterIndex)
                                    {
                                        Handles.color = Color.green;
                                    }
                                    Handles.SphereHandleCap(controlID, pos, rotation, s,et);
                                    Handles.color = color;
                                });

                            if (GUIUtility.hotControl == monsterHosControl[k] && GUIUtility.hotControl != 0)
                            {
                                monsterIndex = k;
                                Repaint();
                            }

                            Quaternion q = Quaternion.Euler(0, monsterCfg.euler, 0);
                            Handles.ArrowHandleCap(0, position, q, size,Event.current.type);

                            if (position != newPos)
                            {
                                Undo.RecordObject(mTarget, "MonsterMove");
                                Vector3 p = newPos - rectCenter;
                                monsterCfg.position = new Vector2(p.x, p.z);
                            }
                        }

                        Color oldColor=GUI.color;
                        GUI.color = Color.white;
                        string monsterId = monsterCfg.id;
                        if (string.IsNullOrEmpty(monsterId))
                        {
                            monsterId = "#" + (j + 1) + "#";
                        }
                        Handles.Label(position, monsterId);
                        GUI.color = oldColor;
                    }
                }
      
                GUI.color = Color.white;
                Handles.Label(rectCenter, "Zone" + (i + 1), headStyle.zoomText);
            }
            GUI.color = Color.white;


            Handles.color = Color.green;
            List<KeyVector2> todoRemove = new List<KeyVector2>();
            foreach (KeyVector2 item in mTarget.connList)
            {
                SpawersZoneVO a = mTarget.getSpawersZoneCfgByGUID(item.x);
                SpawersZoneVO b = mTarget.getSpawersZoneCfgByGUID(item.y);
                if (a == null || b == null)
                {
                    todoRemove.Add(item);
                    continue;
                }
                Handles.DrawLine(new Vector3(a.rect.x, 0, a.rect.y), new Vector3(b.rect.x, 0, b.rect.y));
            }

            if (todoRemove.Count > 0)
            {
                foreach (KeyVector2 item in todoRemove)
                {
                    int tIndex = mTarget.connList.IndexOf(item);
                    if (tIndex != -1)
                    {
                        mTarget.connList.RemoveAt(tIndex);
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnDestory()
        {
            if (_sceneCameraPreview != null)
            {
                _sceneCameraPreview.Dispose();
                _sceneCameraPreview = null;
            }
        }
        private ListControlStyle waveControlStyle;
        private void zoneCreateHandle(SerializedProperty list, SerializedProperty item, int index)
        {
           bool b = false;

            if (waveControlStyle == null)
            {
                waveControlStyle = new ListControlStyle();
                waveControlStyle.addButtonContent.text = "添加一波刷怪";
            }

            if (zoomIndex == index)
            {
                GUILayout.BeginVertical(headStyle.box);
            }

            SerializedProperty rectProperty = item.FindPropertyRelative("rect");
            if (rectProperty.rectValue.size == Vector2.zero)
            {
                rectProperty.rectValue = new Rect(Vector2.zero,Vector2.one*3);
                serializedObject.ApplyModifiedProperties();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                b = Header(item, "Zone " + (index + 1), index, rectProperty.rectValue);
       
                b &= showButtons(list, index, zoneControlStyle, zoomItemAddBackHandle);
            }
            if (b)
            {
               
                EditorGUILayout.PropertyField(rectProperty, GUIContent.none);
                EditorGUILayout.PropertyField(item.FindPropertyRelative("enterType"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("enterPath"));

                EditorGUILayout.PropertyField(item.FindPropertyRelative("exitType"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("exitPath"));

                SerializedProperty slotsProperty = item.FindPropertyRelative("list");
                showElements(slotsProperty, waveCreateHandle, zoneControlStyle);
            }

            if (zoomIndex == index)
            {
                GUILayout.EndVertical();
            }
        }

        private ListControlStyle monsterControlStyle;
        private void waveCreateHandle(SerializedProperty list, SerializedProperty item, int index)
        {
            if (monsterControlStyle == null)
            {
                monsterControlStyle = new ListControlStyle();
                monsterControlStyle.addButtonContent.text = "添加一只怪";
            }
            if (waveIndex == index)
            {
                EditorGUILayout.BeginVertical(headStyle.box2);
            }
            else
            {
                EditorGUILayout.BeginVertical("box");
            }
            EditorGUILayout.BeginHorizontal();

            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            bool b = EditorGUILayout.Foldout(waveIndex==index, "Wave " + (index + 1));
            if (EditorGUI.EndChangeCheck())
            {
                if (b)
                {
                    waveIndex = index;
                    monsterIndex = 0;
                }
                else if (waveIndex == index)
                {
                    waveIndex = -1;
                }
            }

            b &= showButtons(list, index, waveControlStyle);
            EditorGUILayout.EndHorizontal();

            if (waveIndex==index)
            {
                EditorGUILayout.PropertyField(item.FindPropertyRelative("trigType"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("enterType"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("enterPath"));

                SerializedProperty slotsProperty = item.FindPropertyRelative("list");
                showElements(slotsProperty, monsterCreateHandle,monsterControlStyle);
                SerializedProperty repeatProperty = item.FindPropertyRelative("repeat");
                EditorGUILayout.PropertyField(repeatProperty);
                if (repeatProperty.intValue > 0)
                {
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("repeatDelayTime"));
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

     
        private void monsterCreateHandle(SerializedProperty list,SerializedProperty item, int index)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            Color oldColor=GUI.contentColor;

            bool isCurrent = monsterIndex == index;
            if (isCurrent)
            {
                GUI.contentColor = Color.yellow;
            }
            bool b = EditorGUILayout.Foldout(isCurrent,"monster" + (index + 1));
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
                EditorGUILayout.PropertyField(item.FindPropertyRelative("id"));

//               SerializedProperty prefabProperty = item.FindPropertyRelative("prefab");
//                prefabProperty.objectReferenceValue = EditorGUILayout.ObjectField("prefab",
//                    prefabProperty.objectReferenceValue, typeof (GameObject), false);

                EditorGUILayout.PropertyField(item.FindPropertyRelative("position"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("euler"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("enterAction"));
               // EditorGUILayout.PropertyField(item.FindPropertyRelative("pathName"));
                SerializedProperty repeatProperty = item.FindPropertyRelative("repeat");
                EditorGUILayout.PropertyField(repeatProperty);
                if (repeatProperty.intValue > 0)
                {
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("repeatDelayTime"));
                }
            }

            GUI.contentColor = oldColor;
            EditorGUILayout.EndVertical();
        }
    }
}