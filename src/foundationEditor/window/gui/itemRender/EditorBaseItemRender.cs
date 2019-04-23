using System;
using foundation;
using UnityEngine;

namespace foundationEditor
{
    public class EditorBaseItemRender : EditorUI, IListItemRender, IDataRenderer, IPoolable, IDisposable, IEventDispatcher, INotifier
    {
        protected int _index;
        private bool _initialized = false;
 
        private bool _isSelected;
        public virtual bool isSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; }
        }

        public virtual void poolAwake()
        {
        }

        public virtual void refresh()
        {
            
        }
        protected virtual void createChildren()
        {
        }

        protected virtual void doData()
        {
        }

        protected void selectedHandle(EventX e = null)
        {
            base.simpleDispatch(EventX.SELECT, this.data);
        }

        public virtual void poolRecycle()
        {
        }

        public override object data
        {
            get
            {
                return this._data;
            }
            set
            {
                if (!this._initialized)
                {
                    this._initialized = true;
                    this.createChildren();
                }
                this._data = value;
                this.doData();
            }
        }

        public int index
        {
            get
            {
                return this._index;
            }
            set
            {
                this._index = value;
            }
        }

        public Action<string, IListItemRender,object> itemEventHandle
        {
            get; set;
        }

        protected Color oldColor;
        public override void onRender()
        {
            if (_data != null)
            {

                if (_isSelected)
                {
                    oldColor = GUI.color;
                    GUI.color = new Color(0, 1f, 1f, 1f);
                }

                if (GUILayout.Button(_data.ToString()))
                {
                    this.simpleDispatch(EventX.SELECT);
                }

                if (_isSelected)
                {
                    GUI.color = oldColor;
                }
            }

        }
    }
}

