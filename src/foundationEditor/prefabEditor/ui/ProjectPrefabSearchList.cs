using System;
using System.Collections.Generic;
using foundation;
using gameSDK;

namespace foundationEditor
{
    public class ProjectPrefabSearchList:EditorUI
    {
        private EditorSearch _searchUI;
        private EditorPageList _pageList;
        private List<PrefabVO> _dataProvider;
        public ProjectPrefabSearchList()
        {
            _pageList = new EditorPageList(new ClassFactory<PrefabItemRender>());

            _searchUI = new EditorSearch();
            _searchUI.addEventListener(EventX.CHANGE, filterHandle);

            addChild(_searchUI);
            addChild(_pageList);
        }

        public void search(string v)
        {
            _searchUI.value = v;
            _searchUI.simpleDispatch(EventX.CHANGE, v);
        }

        protected virtual void filterHandle(EventX e)
        {
            List<PrefabVO> resultList = null;
            string v = (e.data as string).ToLower();
            if (string.IsNullOrEmpty(v))
            {
                resultList = _dataProvider;
            }
            else
            {
                resultList = new List<PrefabVO>();
                foreach (PrefabVO resourceVo in _dataProvider)
                {
                    string displayName = resourceVo.fileName.ToLower();
                    if (displayName.IndexOf(v) != -1)
                    {
                        resultList.Add(resourceVo);
                    }
                    else
                    {
                        displayName = resourceVo.keys.ToLower();
                        if (displayName.IndexOf(v) != -1)
                        {
                            resultList.Add(resourceVo);
                        }
                    }
                }
            }

            _pageList.dataProvider = resultList;
        }

        public Action<string, IListItemRender,object> itemEventHandle
        {
            set
            {
                _pageList.itemEventHandle = value;
            }
        }
        public List<PrefabVO> dataProvider
        {
            set
            {
                this._dataProvider = value;
                _pageList.dataProvider = value;
            }
            get
            {
                return (List<PrefabVO>) _pageList.dataProvider;
            }
        }

        public int selectedIndex
        {
            get { return _pageList.selectedIndex; }
            set { _pageList.selectedIndex = value; }
        }
    }
}