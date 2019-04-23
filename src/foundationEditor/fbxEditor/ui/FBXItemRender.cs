using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class FBXItemRender : EditorBaseItemRender
    {
        public override void onRender()
        {
            FBXInfo info = _data as FBXInfo;
            if (info != null)
            {

                GUILayout.BeginHorizontal();

                if (isSelected)
                {
                    oldColor = GUI.color;
                    GUI.color = new Color(0, 1f, 1f, 1f);
                }

                string displayName = info.getDisplayName();
                if (GUILayout.Button(displayName))
                {
                    itemEventHandle(EventX.SELECT, this, data);
                    selectedHandle();
                }
                if (GUILayout.Button("fbx",GUILayout.Width(30)))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(info.fbxPath);
                    Selection.activeGameObject = prefab;
                    EditorGUIUtility.PingObject(prefab);
                }

                if (GUILayout.Button("打开", GUILayout.MaxWidth(30)))
                {
                    selectedHandle();
                    EditorUtility.OpenWithDefaultApp(info.rawFolder);
                }

                if (GUILayout.Button("重新生成", GUILayout.MaxWidth(60)))
                {
                    itemEventHandle(EventX.ADDED, this, data);
                    selectedHandle();
                }
                if (isSelected)
                {
                    GUI.color = oldColor;
                }
                GUILayout.EndHorizontal();

            }
            //base.onRender();
        }


        public override bool isSelected
        {
            get { return base.isSelected; }
            set
            {
                if (value && base.isSelected != value && data != null)
                {
                    itemEventHandle(EventX.SELECT, this, data);
                }

                base.isSelected = value;
            }
        }
    }
}