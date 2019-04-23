using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using foundation;
using UnityEditor;
using UnityEngine;

class LYRagdollWindow : EditorWindow
{
    [MenuItem("GameObject/3D Object/LingYu Ragdoll")]
    [ExecuteInEditMode]
    static void Initialize()
    {
        LYRagdollWindow window =
            (LYRagdollWindow)EditorWindow.GetWindow(typeof(LYRagdollWindow), false, "LYRagdollWindow");
        window.minSize = new Vector2(200, 200);
        window.maxSize = new Vector2(1000, 2 * 1000);
        window.wantsMouseMove = true;
        window.Show(true);
    }

    void LoadConfig()
    {
        string path = Application.dataPath + "/Editor/ragdollConfig";
        LYRagdollManager.Instance.Initial(path);
    }

    private Vector2 uiScroll = new Vector2(0, 0);
    void OnGUI()
    {
        uiScroll = EditorGUILayout.BeginScrollView(uiScroll, false, false, GUILayout.MinWidth(200), GUILayout.MaxWidth(position.width));

        if (Selection.activeGameObject != null && EditorApplication.isPlaying == true)
        {
            GUI.color = Color.green;
            if (GUILayout.Button("重新加载配置文件"))
            {
                LoadConfig();
            }
            GUI.color = Color.white;
            EditorGUILayout.Separator();

            string curTemplateName = "";
            LYRagdoll r = Selection.activeGameObject.GetComponent<LYRagdoll>();
            if (r != null && r.RagdollItem != null)
                curTemplateName = r.RagdollItem.name;
            EditorGUILayout.LabelField("当前使用的模板："+curTemplateName);
            
            if (GUILayout.Button("自动匹配"))
            {//自动匹配模板
                LYRagdollManager.Instance.Clear(Selection.activeGameObject);
                LYRagdollManager.Instance.Enable(Selection.activeGameObject);
            }

            EditorGUILayout.Separator();

            //选择模板
            if (LYRagdollManager.Instance.NodeLst.Count > 0)
            {
                int curIdx = LYRagdollManager.Instance.GetIndexByName(curTemplateName);
                int idx = EditorGUILayout.Popup("选择模板：", curIdx, LYRagdollManager.Instance.NameLst.ToArray());
                if (idx != curIdx)
                {
                    LYRagdollManager.Instance.Clear(Selection.activeGameObject);
                    LYRagdollManager.Instance.EnableByNodeItem(Selection.activeGameObject, LYRagdollManager.Instance.NameLst[idx]);
                }
            }

            EditorGUILayout.Separator();
            
            if (GUILayout.Button("Disable Ragdoll"))
            {
                LYRagdollManager.Instance.Disable(Selection.activeGameObject);
            }
            
            if (r != null)
            {
                ShowNodeItem(0, r.RootItem);

                GUI.color = Color.green;
                if (GUILayout.Button("保存"))
                {
                    r.SaveXml(Application.dataPath + "/Editor/ragdollConfig/template/" + Selection.activeGameObject.name + ".xml", Selection.activeGameObject.name);
                    LoadConfig();
                }
            }

        }
        else
        {
            GUI.color = Color.red;
            if(EditorApplication.isPlaying == false)
                GUILayout.Label("请先处于运行状态");
            if(Selection.activeGameObject == null)
                GUILayout.Label("请选择角色");
        }

        EditorGUILayout.EndScrollView();
    }

    public void ShowNodeItem(int layer_ , LYRagdoll.BoneItemD node_)
    {
        if (node_ == null)
            return;
        string f = "";
        for (int i = 0; i < layer_; i ++)
            f += "  ";
        f += "|_";
        GUI.color = Color.white;
        Transform t = (Transform)EditorGUILayout.ObjectField(f + node_.boneName , node_.anchor, typeof (Transform), true);
        if (t != node_.anchor)
        {
            node_.anchor = t;
            LYRagdoll r = Selection.activeGameObject.GetComponent<LYRagdoll>();
            if (r != null)
            {
                r.OnDisable();
                r.OnEnable();
            }
        }

        for (int i = 0; i < node_.childLst.Count; i ++)
        {
            ShowNodeItem(layer_ + 1 , node_.childLst[i]);
        }
    }
}
