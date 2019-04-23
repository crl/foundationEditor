using System.Collections.Generic;
using foundation;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace foundationEditor
{
    [CustomEditor(typeof (BoundariesCFG))]
    public class BoundariesCFGInspector : BaseInspector<BoundariesCFG>
    {
        private Dictionary<int, ReorderableList> reorderableListMapping = new Dictionary<int, ReorderableList>();
        private int preSize = 0;

        private static int selectedIndex = -1;

        public bool Header(SerializedProperty group, string title, int index)
        {
            Rect rect = GUILayoutUtility.GetRect(5, 22f, headStyle.header);
            GUI.Box(rect, title, headStyle.header);

            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                if (rect.Contains(e.mousePosition) && group != null)
                {
                    if (selectedIndex != index)
                    {
                        selectedIndex = index;
                    }
                    else
                    {
                        selectedIndex = -1;
                    }
                    e.Use();
                }
            }
            return selectedIndex == index;
        }

        protected override void OnEnable()
        {
            if (mTarget != target)
            {
                reorderableListMapping.Clear();
            }
            base.OnEnable();
        }


        protected override void drawInspectorGUI()
        {
            SerializedProperty pathListProperty = serializedObject.FindProperty("list");
            int newSize = pathListProperty.arraySize;
            if (newSize != preSize)
            {
                reorderableListMapping.Clear();
                preSize = newSize;
            }

            showElements(pathListProperty, boundCreateHandle, null, boundAddHandle);

        }

        private void boundAddHandle(SerializedProperty list)
        {
            list.FindPropertyRelative("color").colorValue = new Color(0, 0.5f, 0, 0.5f);
        }

        private void boundCreateHandle(SerializedProperty list, SerializedProperty item, int index)
        {
            if (selectedIndex == index)
            {
                GUILayout.BeginVertical(headStyle.box);
            }

            bool b = false;
            using (new EditorGUILayout.HorizontalScope())
            {
                b = Header(item, "Bound " + (index + 1), index);
                EditorGUILayout.PropertyField(item.FindPropertyRelative("color"), GUIContent.none, GUILayout.Width(80));
                b &= showButtons(list, index);
            }
            if (b)
            {
                EditorGUILayout.PropertyField(item.FindPropertyRelative("height"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("depth"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("isClosed"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("layer"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("depthAnchor"));

                SerializedProperty listProperty = item.FindPropertyRelative("segments");
                pointsCreateHandle(listProperty, item, index);

                Connect(mTarget.list[index]);
            }
            if (selectedIndex == index)
            {
                EditorGUILayout.EndVertical();
            }
        }

        public void Connect(BoundCFG cfg)
        {
            Undo.RecordObject(mTarget, "transform position");
            List<Segment> segments = cfg.segments;
            for (int i = 0; i < segments.Count - 1; i++)
            {
                segments[i].end = segments[i + 1].start;
            }

            if (cfg.isClosed && segments.Count > 2)
            {
                segments[segments.Count - 1].end = segments[0].start;
            }
        }

        private void pointsCreateHandle(SerializedProperty list, SerializedProperty item, int index)
        {
            ReorderableList _list = null;
            if (reorderableListMapping.TryGetValue(index, out _list) == false)
            {
                _list = new ReorderableList(item.serializedObject, list, true, true, true, true);
                _list.elementHeight = 42;
                reorderableListMapping.Add(index, _list);
                _list.drawHeaderCallback = delegate(Rect rect)
                {
                    EditorGUI.LabelField(rect, "路点");
                };

                _list.onAddCallback = (ReorderableList l) =>
                {
                    int i = l.serializedProperty.arraySize;
                    l.serializedProperty.arraySize++;
                    l.index = i;
                    SerializedProperty element = l.serializedProperty.GetArrayElementAtIndex(i);

                    Vector3 s = Vector3.zero;
                    if (i > 0)
                    {
                        s =
                            l.serializedProperty.GetArrayElementAtIndex(i - 1)
                                .FindPropertyRelative("end")
                                .vector3Value;
                    }
                    element.FindPropertyRelative("start").vector3Value = s;
                    element.FindPropertyRelative("end").vector3Value = s;
                };
                _list.drawElementCallback = delegate(Rect rect, int i, bool isActive, bool isFocused)
                {
                    rect.y += 2;
                    SerializedProperty pro = list.GetArrayElementAtIndex(i).FindPropertyRelative("start");
                    EditorGUI.PropertyField(rect, pro);

                    if (i == list.arraySize - 1)
                    {
                        pro = list.GetArrayElementAtIndex(i).FindPropertyRelative("end");
                        rect.y += 20;
                        EditorGUI.PropertyField(rect, pro);
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
            }
            _list.DoLayoutList();
        }


        protected override void drawSceneGUI()
        {
            BoundCFG[] list = mTarget.list;
            if (list.Length <= 0) return;

            int len = list.Length;
            bool isCurrent = false;
            for (int i = 0; i < len; i++)
            {
                BoundCFG cfg = list[i];
                isCurrent = i == selectedIndex;
                if (isCurrent == false)
                {
                    continue;
                }
                int j = 0;
                foreach (Segment segment in cfg.segments)
                {
                    Handles.Label(segment.start,"seg_"+j);
                    segment.start = Handles.PositionHandle(segment.start, Quaternion.identity);
                    j++;
                }
            }
        }
    }
}