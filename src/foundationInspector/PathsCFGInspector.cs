using foundation;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace foundationEditor
{
    [CustomEditor(typeof (PathsCFG))]
    public class PathsCFGInspector : BaseInspector<PathsCFG>
    {
        private Dictionary<string, ReorderableList> reorderablePointListMapping=new Dictionary<string, ReorderableList>();

        protected override void OnEnable()
        {
            if (mTarget != target)
            {
                reorderablePointListMapping.Clear();
            }

            mTarget = target as PathsCFG;
            controlSize = 0.5f;

            HashSet<string> keyIn = new HashSet<string>();
            foreach (RefVector2 refVector2 in mTarget.list)
            {
                string guid = refVector2.guid;

                if (keyIn.Contains(guid))
                {
                    guid = Guid.NewGuid().ToString();
                    refVector2.guid = guid;
                }
                keyIn.Add(guid);
            }
        }

        protected override void drawInspectorGUI()
        {
            SerializedProperty listProperty = serializedObject.FindProperty("list");

            EditorGUILayout.Space();
            pointsCreateHandle(listProperty, "a");

            listProperty = serializedObject.FindProperty("connList");
            connsCreateHandle(listProperty, "b");

            drawExportUI("Paths");
        }
    

        private void pointsCreateHandle(SerializedProperty list, string key)
        {
            ReorderableList _list = null;
            if (reorderablePointListMapping.TryGetValue(key, out _list) == false)
            {
                _list = new ReorderableList(serializedObject, list, true, true, true, true);
                reorderablePointListMapping.Add(key, _list);
                _list.drawHeaderCallback= delegate(Rect rect)
                {
                    EditorGUI.LabelField(rect, "路点");
                };

                _list.drawElementCallback = delegate(Rect rect, int i, bool isActive, bool isFocused)
                {
                    rect.y += 2;
                    float w = rect.width;
                    rect.height -= 4;
                    rect.width = 20;
                    EditorGUI.LabelField(rect,i+"");
                    rect.x += 20;
                    SerializedProperty s = list.GetArrayElementAtIndex(i);
                    rect.width = w-20;

                    Vector2 v = new Vector2();
                    v.x= s.FindPropertyRelative("x").floatValue;
                    v.y = s.FindPropertyRelative("y").floatValue;

                    EditorGUI.BeginChangeCheck();
                    v = EditorGUI.Vector2Field(rect, GUIContent.none, v);
                    if (EditorGUI.EndChangeCheck())
                    {
                        s.FindPropertyRelative("x").floatValue = v.x;
                        s.FindPropertyRelative("y").floatValue = v.y;
                    }
                };
                _list.onCanRemoveCallback = (ReorderableList l) => {
                                                                       return l.count > 1;
                };
                _list.onRemoveCallback = (ReorderableList l) =>
                {
                    if (EditorUtility.DisplayDialog("Warning!",
                        "Are you sure you want to delete the point?", "Yes", "No"))
                    {
                        ReorderableList.defaultBehaviours.DoRemoveButton(l);
                    }
                };

                _list.onAddCallback = (ReorderableList l) =>
                {
                    if (l.serializedProperty != null)
                    {
                        l.serializedProperty.arraySize++;
                        l.index = l.serializedProperty.arraySize - 1;

                        SerializedProperty itemData = l.serializedProperty.GetArrayElementAtIndex(l.index);

                        itemData.FindPropertyRelative("guid").stringValue = Guid.NewGuid().ToString();
                    }
                    else
                    {
                        ReorderableList.defaultBehaviours.DoAddButton(l);
                    }
                };
            }
            
            _list.DoLayoutList();
        }

        private void connsCreateHandle(SerializedProperty list, string key)
        {
            ReorderableList _list = null;
            if (reorderablePointListMapping.TryGetValue(key, out _list) == false)
            {
                _list = new ReorderableList(serializedObject, list, true, true, true, true);
                reorderablePointListMapping.Add(key, _list);
                _list.drawHeaderCallback = delegate(Rect rect)
                {
                    EditorGUI.LabelField(rect, "连接点");
                };

                _list.drawElementCallback = delegate(Rect rect, int i, bool isActive, bool isFocused)
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
                _list.onCanRemoveCallback = (ReorderableList l) =>
                {
                    return l.count > 0;
                };
//                _list.onAddCallback = (ReorderableList l) =>
//                {
//                    return false;
//                };
                _list.onRemoveCallback = (ReorderableList l) =>
                {
                    if (EditorUtility.DisplayDialog("Warning!",
                        "Are you sure you want to delete the conn?", "Yes", "No"))
                    {
                        ReorderableList.defaultBehaviours.DoRemoveButton(l);
                    }
                };
            }

            _list.DoLayoutList();
        }


        protected override void drawSceneGUI()
        {
            int index = 0;

            Event e = Event.current;
            bool isMouseUp = false;
            if (e.type == EventType.MouseUp)
            {
                isMouseUp = true;
            }
            bool isShift = e.shift;

            Vector3 newPos;


            Handles.color = Color.white;
            Vector3 center = new Vector3(mTarget.center.x, 0, mTarget.center.y);
            float size = HandleUtility.GetHandleSize(center) * controlSize;

            int len = mTarget.list.Count;
            int[] pointerHosControl = new int[len];
            for (int j = 0; j < len; j++)
            {
                Vector2 point = mTarget.list[j].getVector2();
                Vector3 position = new Vector3(point.x, 0, point.y) + center;

                newPos = Handles.FreeMoveHandle(position, Quaternion.identity, size / 2, Vector3.zero,
                    (int controlID, Vector3 pos, Quaternion rotation, float s, EventType eventType) =>
                    {
                        pointerHosControl[j] = controlID;
                        Handles.SphereHandleCap(controlID, pos, rotation, s, eventType);
                    });

                if (GUIUtility.hotControl == pointerHosControl[j] && isShift)
                {
                    Vector2 v = Event.current.mousePosition;
                    Ray ray = HandleUtility.GUIPointToWorldRay(v);
                    Vector3 rayPoint = ray.origin;
                    Handles.DrawLine(position, rayPoint);

                    if (isMouseUp)
                    {
                        RefVector2 refVector2 = mTarget.getNearRefVector2(new Vector2(rayPoint.x, rayPoint.z));
                        if (refVector2 != null)
                        {
                            KeyVector2 intVector2 = new KeyVector2();
                            intVector2.x = mTarget.list[j].getGUID();
                            intVector2.y = refVector2.getGUID();
                            mTarget.addConn(intVector2);
                        }
                        isMouseUp = false;
                    }
                }


                Handles.Label(position, j + "", headStyle.text);
                if (position != newPos && isShift == false)
                {
                    Vector3 te = newPos - center;
                    mTarget.list[j].reset(te.x, te.z);

                    Undo.RecordObject(mTarget, "movePath");
                    //serializedObject.ApplyModifiedProperties();
                }
                if (j == 0)
                {
                    string name = mTarget.name;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = "";
                    }
                    Handles.Label(position, "path" + index + "#" + name, headStyle.box2);
                }
            }

            Handles.color = Color.green;
            List<KeyVector2> todoRemove = new List<KeyVector2>();
            foreach (KeyVector2 item in mTarget.connList)
            {
                RefVector2 a = mTarget.getRefVector2ByGUID(item.x);
                RefVector2 b = mTarget.getRefVector2ByGUID(item.y);
                if (a == null || b == null)
                {
                    todoRemove.Add(item);
                    continue;
                }
                Handles.DrawLine(new Vector3(a.x, 0, a.y) + center, new Vector3(b.x, 0, b.y) + center);
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

            index++;
        }
    }
}