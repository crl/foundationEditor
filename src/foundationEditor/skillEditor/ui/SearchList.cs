using System;
using System.Collections.Generic;
using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class SearchList :BaseEditorWindow
    {
        private EditorSearch formItem;
        private EditorPageList pageList;
        public List<string> dataList;
        public Action<string> callBack;
        private SearchList() : base(true)
        {
            pageList = new EditorPageList(new ClassFactory<EditorBaseItemRender>());
            pageList.addEventListener(EventX.SELECT, onListHandle);

            formItem = new EditorSearch();
            formItem.addEventListener(EventX.CHANGE, filterHandle);

            this.addChild(formItem);
            this.addChild(pageList);
        }

        private static SearchList _instance;
        public static SearchList getInstance()
        {
            if (_instance == null)
            {
                _instance=new SearchList();
            }
            return _instance;
        }

        private static SearchList w;

        public static void Show(string key = "effect",Action<string> callBack=null)
        {
            if (w == null)
            {
                w = EditorWindow.CreateInstance<SearchList>();
            }
            w.show(key,callBack);
        }

        private void show(string key, Action<string> callBack)
        {
            pageList.dataProvider = dataList = DataSource.Get(key);

            Rect rect = this.position;

            w.ShowAuxWindow();
            Vector2 v = EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            rect.x = v.x;

            if (v.x + rect.width >stage.screenWidth)
            {
                rect.x = v.x - rect.width;
            }
            rect.y = v.y+50;
            if (rect.y + rect.height> stage.screenHeight)
            {
                rect.y -= rect.height;
            }

            this.callBack = callBack;
            this.position = rect;
        }

        protected virtual void filterHandle(EventX e)
        {
            List<string> resultList = null;
            string v = (e.data as string).ToLower();
            if (string.IsNullOrEmpty(v))
            {
                resultList = dataList;
            }
            else if(dataList!=null)
            {
                resultList = new List<string>();
                foreach (string fileName in dataList)
                {
                    if (fileName.ToLower().IndexOf(v) != -1)
                    {
                        resultList.Add(fileName);
                    }
                }
            }

            pageList.dataProvider = resultList;
        }

        private void onListHandle(EventX e)
        {
            IListItemRender render = e.data as IListItemRender;
            if (callBack != null && render!=null)
            {
                callBack(render.data as string);
            }
            this.Close();
        }
    }
}