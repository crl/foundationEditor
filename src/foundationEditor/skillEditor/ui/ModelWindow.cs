using System;
using System.Collections.Generic;
using System.IO;
using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [Serializable]
    public class ModelWindow : EditorBox
    {
        protected List<ResourceVO> dataList = new List<ResourceVO>();
        [SerializeField]
        private EditorSearch formItem;
        [SerializeField]
        private EditorPageList pageList;
        public string key;
        public string[] exNameArr = new string[] {"*.prefab"};
        public SearchOption searchOption = SearchOption.AllDirectories;

        public ModelWindow() : base(true)
        {
            pageList = new EditorPageList(new ClassFactory<EditorBaseItemRender>());
            pageList.allowRepeat = true;
            pageList.addEventListener(EventX.SELECT, onListHandle);

            formItem = new EditorSearch();
            formItem.addEventListener(EventX.CHANGE, filterHandle);

            this.addChild(formItem);
            this.addChild(pageList);
        }

        protected virtual void onListHandle(EventX e)
        {
            object o = pageList.selectedData;
            this.simpleDispatch(EventX.SELECT, o);
        }

        public int selectedIndex
        {
            set
            {
                pageList.selectedIndex = value;
            }
            get { return pageList.selectedIndex; }
        }

        protected virtual void filterHandle(EventX e)
        {
            List<ResourceVO> resultList = null;
            string v = (e.data as string).ToLower();
            if (string.IsNullOrEmpty(v))
            {
                resultList = dataList;
            }
            else
            {
                resultList = new List<ResourceVO>();
                foreach (ResourceVO resourceVo in dataList)
                {
                    if (resourceVo.fileName.ToLower().IndexOf(v) != -1)
                    {
                        resultList.Add(resourceVo);
                    }
                }
            }

            pageList.dataProvider = resultList;
        }

        protected virtual void saveHandle(EventX e)
        {
        }

        public void init(string editorPrefabPath, params string[] types)
        {
            dataList.Clear();
            key = "";
            foreach (string type in types)
            {
                key += type + "_";
                string prefabPath = editorPrefabPath + "/" + type + "/";

                string[] exportKeys = EditorConfigUtils.GetPrefabExports(type);
                if (exportKeys == null || exportKeys.Length<1)
                {
                    exportKeys = new[] {prefabPath};
                }
                List<string> nameList = new List<string>();
                foreach (string exportPrefabPath in exportKeys)
                {
                    List<string> list = FileHelper.FindFile(exportPrefabPath, exNameArr, searchOption);
                    for (int i = 0; i < list.Count; i++)
                    {
                        string itemPath = list[i];
                        itemPath = itemPath.Replace(Application.dataPath, "Assets");
                        ResourceVO item = new ResourceVO();
                        item.itemPath = itemPath;

                        item.fileFullName = Path.GetFileName(itemPath);
                        string fileName = Path.GetFileNameWithoutExtension(item.fileFullName);
                        item.fileName = fileName;

                        item.typeKey = type;
                        nameList.Add(fileName);
                        dataList.Add(item);
                    }
                }
                DataSource.Add(type, dataList);
                DataSource.Add(type, nameList);
            }

            pageList.dataProvider = dataList;

        }
    }

}