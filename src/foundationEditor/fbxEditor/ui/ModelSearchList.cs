using System;
using System.Collections.Generic;
using foundation;

namespace foundationEditor
{
    public class ModelSearchList:EditorUI
    {
        private EditorSearch search;
        private EditorPageList pageList;
        private List<FBXInfo> dataList;
        public ModelSearchList()
        {
            pageList = new EditorPageList(new ClassFactory<FBXItemRender>());

            search=new EditorSearch();
            search.addEventListener(EventX.CHANGE, filterHandle);

            addChild(search);
            addChild(pageList);
        }

        protected virtual void filterHandle(EventX e)
        {
            List<FBXInfo> resultList = null;
            string v = (e.data as string).ToLower();
            if (string.IsNullOrEmpty(v))
            {
                resultList = dataList;
            }
            else
            {
                resultList = new List<FBXInfo>();
                foreach (FBXInfo resourceVo in dataList)
                {
                    string displayName = resourceVo.fileName.ToLower();
                    if (displayName.IndexOf(v) != -1)
                    {
                        if (resultList.Contains(resourceVo) == false)
                        {
                            resultList.Add(resourceVo);
                        }
                    }

                    displayName = resourceVo.keys.ToLower();
                    if (displayName.IndexOf(v) != -1)
                    {
                        if (resultList.Contains(resourceVo) == false)
                        {
                            resultList.Add(resourceVo);
                        }
                    }
                }
            }
            pageList.dataProvider = resultList;
        }

        public Action<string, IListItemRender,object> itemEventHandle
        {
            set
            {
                pageList.itemEventHandle = value;
            }
        }
        public List<FBXInfo> dataProvider
        {
            set
            {
                this.dataList = value;
                pageList.dataProvider = value;
            }
        }
    }
}