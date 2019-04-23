using foundation;
using gameSDK;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class PrefabItemRender : EditorBaseItemRender
    {
        public override void onRender()
        {
            PrefabVO vo = _data as PrefabVO;
            if (vo != null)
            {

                GUILayout.BeginHorizontal();

                if (isSelected)
                {
                    oldColor = GUI.color;
                    GUI.color = new Color(0, 1f, 1f, 1f);
                }

                string displayName = vo.getDisplayName();
                if (GUILayout.Button(displayName))
                {
                    itemEventHandle(EventX.SELECT, this, data);
                    selectedHandle();
                }
                if (vo.hasFBX)
                {
                    if (GUILayout.Button("fbx", GUILayout.Width(30)))
                    {
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(vo.fbxPath);
                        Selection.activeGameObject = prefab;
                        EditorGUIUtility.PingObject(prefab);
                    }
                }
                if (isSelected)
                {
                    GUI.color = oldColor;
                }
                GUILayout.EndHorizontal();

            }
        }
    }
}