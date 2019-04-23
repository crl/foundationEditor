using System.Collections.Generic;
using foundation;
using foundationEditor;
using gameSDK;
using UnityEngine;

namespace UnityEditor
{
    public class ProjectPrefabWindow : BaseEditorWindow
    {
     
        private PreviewSystem previewSystem;
        private EditorTabNav tabNav;
        private Dictionary<string, List<PrefabVO>> dataProvider;
        public ProjectPrefabWindow()
        {
            this.titleContent = new GUIContent("ProjectPrefab");
            tabNav = new EditorTabNav();
        }

        protected override void initialization()
        {
            base.initialization();

            previewSystem = new PreviewSystem();
            previewSystem.parentEditorWindow = this;

            reload();
        }

        [MenuItem("App/ProjectPrefab",false,4)]
        public static void ProjectPrefabWindowEditor()
        {
            GetWindow<ProjectPrefabWindow>();
        }

        private void itemEventHandle(string eventType, IListItemRender itemRender, object data)
        {
            if (eventType == EventX.SELECT)
            {
                viewPrefabInfo(data as PrefabVO);
            }
        }

        protected override void OnDestroy()
        {
            if (previewSystem != null)
            {
                previewSystem.OnDestroy();
                previewSystem = null;
            }

            base.OnDestroy();
        }

        /// <summary>
        /// 查看模型
        /// </summary>
        /// <param name="prefab"></param>
        private void viewPrefabInfo(PrefabVO vo,bool selectedIt=true)
        {
            if (vo == null)
            {
                return;
            }
            string path = vo.prefabPath;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                return;
            }
            previewSystem.SetPreview(prefab);
            if (selectedIt)
            {
                Selection.activeGameObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
        }


        public void reload(EventX e = null)
        {
            RemoveAllChildren();
            ProjectPrefabSearchList item;

            dataProvider = PrefabVODB.Reload();

            tabNav.removeAllChildren();
            foreach (string key in dataProvider.Keys)
            {
                item=new ProjectPrefabSearchList();;
                item.itemEventHandle = itemEventHandle;
                item.dataProvider=dataProvider[key];
                tabNav.addItem(key,item);
            }
            this.addChild(tabNav);
            this.addChild(new EditorFlexibleSpace());

            tabNav.autoSelected();

            EditorButton btn;

            EditorBox box=new EditorBox(false);
            btn = new EditorButton("reload");
            btn.addEventListener(EventX.ITEM_CLICK, reload);
            box.addChild(btn);

            this.addChild(box);
        }


        public void searchView(string uri)
        {
            if (dataProvider == null)
            {
                dataProvider = PrefabVODB.Reload();
            }
            PrefabVO prefabVo= PrefabVODB.Get(uri);
            if (prefabVo == null)
            {
                return;
            }
            tabNav.selectedTabLabel(prefabVo.rootKey);
            ProjectPrefabSearchList searchList= tabNav.selectedItem as ProjectPrefabSearchList;
            if (searchList != null)
            {
                EditorCallLater.Add(() =>
                {
                    searchList.search(prefabVo.fileName);
                    viewPrefabInfo(prefabVo, false);
                },0.5f);
            }
        }

        protected override void OnEnable()
        {
            if (previewSystem != null)
            {
                previewSystem.OnEnable();
            }
            base.OnEnable();
        }
        protected override void OnDisable()
        {
            if (previewSystem!=null)
            {
                previewSystem.OnDisable();
            }
            base.OnDisable();
        }


        protected override void OnGUI()
        {
            _canRepaint = true;
            EditorUI.CheckLostFocus();
            stage.x = (int)position.x;
            stage.y = (int)position.y;
            stage.stageWidth = (int)position.width;
            stage.stageHeight = (int)position.height;

            EditorGUILayout.BeginHorizontal();
             EditorGUILayout.BeginVertical(GUILayout.Width(300));
                stage.onRender();
            EditorGUILayout.EndVertical();

            Rect rect = GUILayoutUtility.GetRect(300, Screen.width, 300, Screen.height);
            previewSystem.DrawRect(rect);

            EditorGUILayout.EndHorizontal();
        }

     
    }
}