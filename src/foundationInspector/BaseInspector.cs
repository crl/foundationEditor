using System;
using System.Collections.Generic;
using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class ListControlStyle
    {
        public GUIContent moveDownButtonContent = new GUIContent("∨", "move down");
        public GUIContent moveUpButtonContent = new GUIContent("∧", "move up");
        
        public GUIContent duplicateButtonContent = new GUIContent("+", "duplicate");
        public GUIContent deleteButtonContent = new GUIContent("-", "delete");

        public GUIContent addButtonContent = new GUIContent("+", "add element");

        public GUILayoutOption miniButtonWidth = GUILayout.Width(20f);
        public GUILayoutOption addButtonWidth = GUILayout.Width(100);
    }

    public class HeadStyles
    {
        public GUIStyle thumb2D = "ColorPicker2DThumb";
        public GUIStyle pickerBox = "ColorPickerBox";
        public GUIStyle thumbHoriz = "ColorPickerHorizThumb";
        public GUIStyle header = "ShurikenModuleTitle";
        public GUIStyle headerCheckbox = "ShurikenCheckMark";
        public Vector2 thumb2DSize;
        public GUIStyle box = new GUIStyle("box");
        public GUIStyle box2 = new GUIStyle("box");
        public GUIStyle text = new GUIStyle("sv_label_6");
        public GUIStyle zoomText = new GUIStyle("sv_label_4");

        public HeadStyles()
        {
            thumb2DSize = new Vector2(
                    !Mathf.Approximately(thumb2D.fixedWidth, 0f) ? thumb2D.fixedWidth : thumb2D.padding.horizontal,
                    !Mathf.Approximately(thumb2D.fixedHeight, 0f) ? thumb2D.fixedHeight : thumb2D.padding.vertical
                    );

            header.font = (new GUIStyle("Label")).font;
            header.border = new RectOffset(4, 4, 4, 4);
            header.fixedHeight = 22;
            header.contentOffset = new Vector2(10f, -2f);

            box.normal.background = EditorUtils.CreateTex(1, 1, new Color(0, 0, 0, 0.1f));
            box.border = new RectOffset();
            box.margin = new RectOffset();
            box.padding = new RectOffset();

            box2.normal.background = EditorUtils.CreateTex(1, 1, new Color(0, 0, 0, 0.2f));
        }

        
    }


    public class BaseInspector<T> : Editor where T : MonoBehaviour
    {
        protected HeadStyles headStyle;
        protected Color rectSolideColor = Color.yellow;
        protected static ListControlStyle DefaultControlStyle=new ListControlStyle();

        protected T mTarget;
        protected GameObject go;
        protected Transform transform;
        protected float controlSize = 0.3f;

        protected virtual bool canEditor
        {
            get { return true; }
        }

        protected virtual void OnEnable()
        {
            if (canEditor == false)
            {
                return;
            }
            mTarget = target as T;
            if (mTarget != null)
            {
                go = mTarget.gameObject;
                transform = go.transform;
                doEnable();
            }
            if (mTarget.gameObject.activeInHierarchy == false)
            {
                return;
            }
        }

        protected virtual void OnDisable()
        {
            
        }

        protected virtual void doEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            if (canEditor == false)
            {
                return;
            }
            if (mTarget == null)
            {
                return;
            }
            if (headStyle == null)
            {
                headStyle = new HeadStyles();
                rectSolideColor.a = 0.2f;
            }
            EditorGUI.BeginChangeCheck();
            drawInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this.target);
                serializedObject.ApplyModifiedProperties();
            }
        }

        public virtual void OnSceneGUI()
        {
            if (canEditor == false)
            {
                return;
            }
            if (headStyle == null)
            {
                headStyle = new HeadStyles();
                rectSolideColor.a = 0.2f;
            }

            GUI.changed = false;

            drawSceneGUI();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(this.target);
            }
        }

        public virtual void drawExportUI(string prefix,string savePrefixPath="")
        {
            if (string.IsNullOrEmpty(savePrefixPath))
            {
                savePrefixPath = Application.dataPath;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("导入"))
                {
                    string path = EditorUtility.OpenFilePanel("选择文件", savePrefixPath, "json");
                    if (string.IsNullOrEmpty(path) == false)
                    {
                        string json = FileHelper.GetUTF8Text(path);
                        JsonUtility.FromJsonOverwrite(json, mTarget);
                    }
                }
                if (GUILayout.Button("导出"))
                {
                    string path = EditorUtility.SaveFilePanel("选择文件", savePrefixPath, prefix, "json");
                    if (string.IsNullOrEmpty(path) == false)
                    {
                        string json = JsonUtility.ToJson(mTarget,false);
                        FileHelper.SaveUTF8(json, path);
                    }
                }
            }
        }

        public virtual void showElements(SerializedProperty list, Action<SerializedProperty,SerializedProperty, int> itemGuiCreateHandle=null,ListControlStyle style=null,Action<SerializedProperty> itemAddHandle=null)
        {
            if (style == null)
            {
                style = DefaultControlStyle;
            }
            if (list.arraySize > 0)
            {
                EditorGUILayout.Space();
            }
            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty item = list.GetArrayElementAtIndex(i);
                if (itemGuiCreateHandle != null)
                {
                    itemGuiCreateHandle(list,item, i);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(item);
                    showButtons(list, i,style);
                    EditorGUILayout.EndHorizontal();
                }
            }
            if (list.arraySize == 0 && GUILayout.Button(style.addButtonContent, style.addButtonWidth))
            {
                list.arraySize += 1;
                if (itemAddHandle != null)
                {
                    SerializedProperty item = list.GetArrayElementAtIndex(list.arraySize-1);
                    itemAddHandle(item);
                }
            }
        }
        protected bool showButtons(SerializedProperty list, int index, ListControlStyle style = null,Action<SerializedProperty> resetCreateItemAction=null)
        {
            if (style == null)
            {
                style = DefaultControlStyle;
            }
            if (GUILayout.Button(style.moveDownButtonContent, EditorStyles.miniButtonLeft, style.miniButtonWidth))
            {
                if (index < list.arraySize - 1)
                {
                    list.MoveArrayElement(index, index + 1);

                    return false;
                }
            }
            if (GUILayout.Button(style.moveUpButtonContent, EditorStyles.miniButtonMid, style.miniButtonWidth))
            {
                if (index >0)
                {
                    list.MoveArrayElement(index, index -1);

                    return false;
                }
            }

            if (GUILayout.Button(style.duplicateButtonContent, EditorStyles.miniButtonMid, style.miniButtonWidth))
            {
                list.InsertArrayElementAtIndex(index);
                if (resetCreateItemAction!=null)
                {
                    resetCreateItemAction(list.GetArrayElementAtIndex(index));
                }
                return false;
            }
            if (GUILayout.Button(style.deleteButtonContent, EditorStyles.miniButtonRight, style.miniButtonWidth))
            {
                int oldSize = list.arraySize;
                list.DeleteArrayElementAtIndex(index);
                if (list.arraySize == oldSize)
                {
                    list.DeleteArrayElementAtIndex(index);
                }
                return false;
            }

            return true;
        }

        protected virtual void drawInspectorGUI()
        {
            base.DrawDefaultInspector();
        }

        protected virtual void drawSceneGUI()
        {

        }

        public void DrawWireCube(Vector3 position, Vector3 size,bool isWorld=false)
        {
            var half = size/2;

            List<Vector3> list = new List<Vector3>();
            // draw front
            list.Add(position + new Vector3(-half.x, -half.y, half.z));
            list.Add(position + new Vector3(half.x, -half.y, half.z));
            list.Add(position + new Vector3(-half.x, -half.y, half.z));
            list.Add(position + new Vector3(-half.x, half.y, half.z));
            list.Add(position + new Vector3(half.x, half.y, half.z));
            list.Add(position + new Vector3(half.x, -half.y, half.z));
            list.Add(position + new Vector3(half.x, half.y, half.z));
            list.Add(position + new Vector3(-half.x, half.y, half.z));
            // draw back
            list.Add(position + new Vector3(-half.x, -half.y, -half.z));
            list.Add(position + new Vector3(half.x, -half.y, -half.z));
            list.Add(position + new Vector3(-half.x, -half.y, -half.z));
            list.Add(position + new Vector3(-half.x, half.y, -half.z));
            list.Add(position + new Vector3(half.x, half.y, -half.z));
            list.Add(position + new Vector3(half.x, -half.y, -half.z));
            list.Add(position + new Vector3(half.x, half.y, -half.z));
            list.Add(position + new Vector3(-half.x, half.y, -half.z));
            // draw corners
            list.Add(position + new Vector3(-half.x, -half.y, -half.z));
            list.Add(position + new Vector3(-half.x, -half.y, half.z));
            list.Add(position + new Vector3(half.x, -half.y, -half.z));
            list.Add(position + new Vector3(half.x, -half.y, half.z));
            list.Add(position + new Vector3(-half.x, half.y, -half.z));
            list.Add(position + new Vector3(-half.x, half.y, half.z));
            list.Add(position + new Vector3(half.x, half.y, -half.z));
            list.Add(position + new Vector3(half.x, half.y, half.z));
            int len = list.Count;
            if (isWorld == false)
            {
                for (int i = 0; i < len; i++)
                {
                    list[i] = getWorldByLocal(list[i]);
                }
            }

            for (int i = 0; i < len; i+=2)
            {
                Handles.DrawLine(list[i],list[i+1]);
            }

        }

        private Bounds getMouseWorldPosition()
        {
            Event e = Event.current;
            Vector3 position = new Vector3(e.mousePosition.x, Screen.height - e.mousePosition.y, 0);
            Ray ray = SceneView.currentDrawingSceneView.camera.ScreenPointToRay(position);

            if (ray.direction.y != 0.0f)
            {
                return new Bounds();
            }

            float t = -ray.origin.y;
            Vector3 center = ray.origin + t*ray.direction;
            center.z = 0;

            float size = 0.5f;

            Handles.RectangleHandleCap(100, center, new Quaternion(0, 0, 0, 1), size,Event.current.type);

            return new Bounds(center, new Vector3(size, size, 5));
        }

        /// <summary>
        /// 从本地坐标转世界坐标
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        protected Vector3 getWorldByLocal(Vector3 v)
        {
            return go.transform.localToWorldMatrix.MultiplyPoint(v);
            //  return go.transform.TransformPoint(v);
        }
        protected Vector3 getWorldDirectByLocal(Vector3 v)
        {
            return go.transform.localToWorldMatrix.MultiplyVector(v);
            //  return go.transform.TransformPoint(v);
        }

        /// <summary>
        /// 从世界坐标转本地坐标
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        protected Vector3 getLocalByWorld(Vector3 v)
        {
            return go.transform.worldToLocalMatrix.MultiplyPoint(v);
            //  return go.transform.InverseTransformPoint(v);
        }

    }
}