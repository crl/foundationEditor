using System;
using System.Collections.Generic;
using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [Serializable]
    public class EditorUI:EventDispatcher
    {
        public static readonly object[] EMPTY = new object[0];
        private static int Counter = 0;
        public Color contentColor = Color.clear;
        protected Color oldContentColor;

        internal List<EditorUI> mChildren = new List<EditorUI>();
        public bool visible=true;
        private EditorUI mParent;

        protected List<GUILayoutOption> layoutOptionList = new List<GUILayoutOption>();
        public GUILayoutOption widthOption;
        public GUILayoutOption heightOption;
        public bool expandWidth = true;
        public bool expandHeight = true;
        public int minWidth = -1;
        public int minHeight = -1;
        public GUIStyle style=GUIStyle.none;
        public string styleString = "";

        protected object _data;

        private int _guid;
        public EditorUI()
        {
            _guid = Counter++;
        }
        public virtual object data
        {
            get { return _data; }
            set { _data = value; } }
        public void addChild(EditorUI item)
        {
            this.addChildAt(item, this.mChildren.Count);
        }
        public EditorStage stage
        {
            get
            {
                EditorUI e = this;
                bool isStage = this is EditorStage;
                while (isStage==false)
                {
                    e = e.parent;
                    if (e == null)
                    {
                        break;
                    }
                    isStage = e is EditorStage;
                }

                if (isStage == true)
                {
                    return e as EditorStage;
                }

                return null;
            }
        }

       

        public BaseEditorWindow window
        {
            get
            {
                EditorStage _stage = stage;
                if (_stage != null)
                {
                    return _stage.__window;
                }
                return null;
            }
        }

        public void windowRepaint()
        {
            if (window != null)
            {
                window.Repaint();
            }
        }

        public string controlID
        {
            get { return this.GetType().Name + "_" + _guid; }
        }

        public EditorUI addChildAt(EditorUI child, int index)
        {
            int count = this.mChildren.Count;
            if ((index < 0) || (index > count))
            {
                throw new RankException();
            }
            if (child.parent == this)
            {
                this.setChildIndex(child, index);
                return child;
            }
            child.removeFromParent();
            if (index == count)
            {
                this.mChildren.Add(child);
            }
            else
            {
                this.mChildren.Insert(index, child);
            }
            child.setParent(this);
            child.simpleDispatch(EventX.ADDED, null);
            return child;
        }

        public EditorUI parent
        {
            get
            {
                return this.mParent;
            }
        }
        public bool contains(EditorUI child)
        {
            while (child != null)
            {
                if (child == this)
                {
                    return true;
                }
                child = child.parent;
            }
            return false;
        }

        public EditorUI getChildAt(int index)
        {
            if ((index < 0) || (index >= this.numChildren))
            {
                throw new RankException("Invalid child index");
            }
            return this.mChildren[index];
        }

        public int getChildIndex(EditorUI child)
        {
            return this.mChildren.IndexOf(child);
        }

        public virtual void onRender()
        {
            int num2;
            for (int i = 0; i < this.numChildren; i = num2 + 1)
            {
                EditorUI eui = this.getChildAt(i);
                if (eui.visible)
                {
                    eui.onRender();
                }
                num2 = i;
            }
        }

        protected virtual GUILayoutOption[] getGuiLayoutOptions()
        {
            layoutOptionList.Clear();
            if (widthOption != null)
            {
                layoutOptionList.Add(widthOption);
            }

            if (heightOption != null)
            {
                layoutOptionList.Add(heightOption);
            }

            if (expandWidth == false)
            {
                layoutOptionList.Add(GUILayout.ExpandWidth(false));
            }

            if (expandHeight == false)
            {
                layoutOptionList.Add(GUILayout.ExpandHeight(false));
            }
            if (minWidth >0)
            {
                layoutOptionList.Add(GUILayout.MinWidth(minWidth));
            }
            if (minHeight > 0)
            {
                layoutOptionList.Add(GUILayout.MinHeight(minHeight));
            }

            return layoutOptionList.ToArray();
        }

        virtual public void removeAllChildren()
        {
            while (this.numChildren > 0)
            {
                this.removeChildAt(0);
            }
        }

        public EditorUI removeChild(EditorUI child)
        {
            int index = this.getChildIndex(child);
            if (index != -1)
            {
                this.removeChildAt(index);
            }
            return child;
        }

        public EditorUI removeChildAt(int index)
        {
            if ((index < 0) || (index >= this.numChildren))
            {
                throw new RankException("Invalid child index");
            }
            EditorUI item = this.mChildren[index];
            item.simpleDispatch(EventX.REMOVED, null);
            item.setParent(null);
            index = this.mChildren.IndexOf(item);
            if (index >= 0)
            {
                this.mChildren.RemoveAt(index);
            }

            return item;
        }

        public void removeFromParent()
        {
            if (this.mParent != null)
            {
                this.mParent.removeChild(this);
            }
        }

        internal void setParent(EditorUI value)
        {
            EditorUI mParent = value;
            while ((mParent != this) && (mParent != null))
            {
                mParent = mParent.mParent;
            }
            if (mParent == this)
            {
                throw new ArgumentException("An object cannot be added as a child to itself or one of its children (or children's children, etc.)");
            }
            this.mParent = value;
        }

        public void setChildIndex(EditorUI child, int index)
        {
            int num = this.getChildIndex(child);
            if (num != index)
            {
                if (num == -1)
                {
                    throw new ArgumentException("Not a child of this container");
                }
                this.mChildren.RemoveAt(num);
                index = Math.Min(this.numChildren, index);
                this.mChildren.Insert(index, child);
            }
        }

        public int numChildren
        {
            get
            {
                return this.mChildren.Count;
            }
        }


        public static EditorUI ___FocusUI;

        public static void CheckFocus(EditorUI value)
        {
            string controlID = "";
            if (value != null)
            {
                controlID = value.controlID;
            }
            string keyControlID = GUI.GetNameOfFocusedControl();
            if (keyControlID == controlID)
            {
                if (___FocusUI != value)
                {
                    if (___FocusUI != null)
                    {
                        //Debug.Log("out"+ EStage.___FocusUI.controlID);
                        ___FocusUI.simpleDispatch(EventX.FOCUS_OUT);
                    }
                    ___FocusUI = value;
                    if (value != null)
                    {
                        //Debug.Log("in" + value.controlID);
                        value.simpleDispatch(EventX.FOCUS_IN);
                    }
                }
            }
        }


        public static void CheckLostFocus()
        {
            if (___FocusUI != null)
            {
                if (___FocusUI.stage == null)
                {
                    ___FocusUI = null;
                    GUI.FocusControl(null);
                }
            }
        }

        public static void ClearFocus()
        {
            ___FocusUI = null;
            GUI.FocusControl(null);
        }
    }


    public class EditorStage : EditorUI
    {
        public BaseEditorWindow __window;
        public float x;
        public float y;
        public float stageWidth;
        public float stageHeight;

        public float screenWidth
        {
            get { return Screen.currentResolution.width; }
        }

        public float screenHeight
        {
            get { return Screen.currentResolution.height; }
        }
    }

    public class EditorPopUp : EditorUI
    {
        public int selectedIndex = 0;
        public string[] items;
        public override void onRender()
        {
            GUI.changed = false;
            selectedIndex = EditorGUILayout.Popup(selectedIndex, items);
            if (GUI.changed)
            {
                this.simpleDispatch(EventX.CHANGE, selectedIndex);
            }
        }
    }

    public class EditorEnumPopUp : EditorUI
    {
        public Enum selectedIndex;
        public override void onRender()
        {
            GUI.changed = false;
            selectedIndex = EditorGUILayout.EnumPopup(selectedIndex);
            if (GUI.changed)
            {
                this.simpleDispatch(EventX.CHANGE, selectedIndex);
            }
        }
    }

    public class EditorRadioGroup : EditorBox
    {
        private string[] _items;
        public string[] items
        {
            set
            {
                _items = value;
                buildView();
            }
        }

        public EditorRadioGroup(bool isV = true) : base(isV)
        {

        }

        private void buildView()
        {
            removeAllChildren();

            foreach (string item in _items)
            {
                EditorRadio radio = new EditorRadio(item);
                radio.addEventListener(EventX.ITEM_CLICK, selectedHandle);

                this.addChild(radio);
            }
        }

        private void selectedHandle(EventX e)
        {
            EditorRadio radio = e.target as EditorRadio;
            if (radio.selected)
            {
                selectedIndex = getChildIndex(radio);
            }
        }

        private int _selectedIndex = -1;
        public int selectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex != -1)
                {
                    EditorRadio radio = getChildAt(_selectedIndex) as EditorRadio;
                    radio.selected = false;
                }

                _selectedIndex = value;

                if (_selectedIndex != -1)
                {
                    EditorRadio radio = getChildAt(_selectedIndex) as EditorRadio;
                    radio.selected = true;
                }

                this.simpleDispatch(EventX.CHANGE);
            }
        }
    }
}
