using System;
using foundation;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [Serializable]
    public class EditorPageList : EditorUI
    {
        private IList _dataProvider;
        private IListItemRender _selectedItem = null;
        private IFactory factory;
        public bool allowRepeat;
        private object _selectedData;

        public Action<string, IListItemRender, object> itemEventHandle;

        public bool isV = true;
        public bool hasScrollBar = true;

        public EditorPageList(IFactory factory, bool isV = true)
        {
            this.factory = factory;
            this.expandHeight = false;
            this.expandWidth = false;
            this.isV = isV;
        }

        public IList dataProvider
        {
            get { return this._dataProvider; }
            set
            {
                this._dataProvider = value;
                if (this._dataProvider == null)
                {
                    this._dataProvider = EditorUI.EMPTY;
                }
                if (this._dataProvider.Count == 0)
                {
                    this._dataProvider = EditorUI.EMPTY;
                }
                this._selectedItem = null;
                base.removeAllChildren();
                int count = this._dataProvider.Count;
                for (int i = 0; i < count; i++)
                {
                    IListItemRender render = (IListItemRender) this.factory.newInstance();
                    EditorUI item = render as EditorUI;
                    if (item != null)
                    {
                        base.addChild(item);
                    }
                    render.addEventListener(EventX.SELECT, this.selectedHandle, 0);
                    render.itemEventHandle = itemEventHandle;
                    render.index = i;
                    render.data = this._dataProvider[i];
                }

            }
        }

        private void selectedHandle(EventX e)
        {
            this.selectedItem = e.target as IListItemRender;
        }

        public IListItemRender selectedItem
        {
            get { return this._selectedItem; }
            set
            {
                if ((this._selectedItem != value) || this.allowRepeat == true)
                {
                    if (this._selectedItem != null)
                    {
                        this._selectedItem.isSelected = false;
                    }
                    this._selectedItem = value;
                    if (this._selectedItem != null)
                    {
                        this._selectedItem.isSelected = true;
                        this._selectedData = this._selectedItem.data;
                    }
                    else
                    {
                        this._selectedData = null;
                    }
                    if (base.hasEventListener(EventX.SELECT))
                    {
                        base.dispatchEvent(new EventX(EventX.SELECT, this._selectedItem, false));
                    }
                }
            }
        }

        public object selectedData
        {
            get { return this._selectedData; }
            set
            {
                bool flag = false;
                foreach (IListItemRender render in base.mChildren)
                {
                    if (render.data == value)
                    {
                        this.selectedItem = render;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    this.selectedIndex = 0;
                }
            }
        }

        public int selectedIndex
        {
            get
            {
                if (this._selectedItem == null)
                {
                    return -1;
                }
                return base.mChildren.IndexOf((EditorUI) this._selectedItem);
            }
            set
            {
                if (base.mChildren.Count > value)
                {
                    this.selectedItem = (IListItemRender) base.mChildren[value];
                }
                else
                {
                    this.selectedItem = null;
                }
            }
        }

        private Vector2 scrollPosition;
        private Rect lastRect;
        public override void onRender()
        {
            if (numChildren == 0)
            {
                return;
            }
            if (hasScrollBar)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition,getGuiLayoutOptions());
            }

            if (string.IsNullOrEmpty(styleString) == false)
            {
                style = styleString;
            }

            if (isV)
            {
                if (style != GUIStyle.none)
                {
                    GUILayout.BeginVertical(style, getGuiLayoutOptions());
                }
                else
                {
                    GUILayout.BeginVertical(getGuiLayoutOptions());
                }
                base.onRender();
                EditorGUILayout.EndVertical();
            }
            else
            {
                if (style != GUIStyle.none)
                {
                    GUILayout.BeginHorizontal(style, getGuiLayoutOptions());
                }
                else
                {
                    GUILayout.BeginHorizontal(getGuiLayoutOptions());
                }
                base.onRender();
                EditorGUILayout.EndHorizontal();
            }
            EventType type = Event.current.type;
            if (type == EventType.Repaint)
            {
                lastRect = GUILayoutUtility.GetLastRect();
            }else if (type == EventType.KeyUp)
            {
                if (lastRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.keyCode == KeyCode.UpArrow)
                    {
                        if (selectedIndex > 0)
                        {
                            selectedIndex--;
                        }
                    }
                    if (Event.current.keyCode == KeyCode.DownArrow)
                    {
                        if (selectedIndex < dataProvider.Count - 1)
                        {
                            selectedIndex++;
                        }
                    }
                }
            }

            if (hasScrollBar)
            {
                EditorGUILayout.EndScrollView();
            }
        }
    }
}
