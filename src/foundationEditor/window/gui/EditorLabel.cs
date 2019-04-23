using System;
using System.Collections.Generic;
using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [Serializable]
    public class EditorLabel : EditorUI
    {

        public string text = "";
        public bool isDynamic = false;

        public EditorLabel(string value = "")
        {
            this.text = value;
        }

        public override void onRender()
        {
            if (isDynamic)
            {
                GUI.SetNextControlName(controlID);

                if (contentColor != Color.clear)
                {
                    oldContentColor = GUI.contentColor;
                    GUI.contentColor = contentColor;
                }
                GUI.changed = false;
                text = EditorGUILayout.TextField(text, getGuiLayoutOptions());

                if (contentColor != Color.clear)
                {
                    GUI.contentColor = oldContentColor;
                }

                if (GUI.changed)
                {
                    this.simpleDispatch(EventX.CHANGE, data);
                }
                CheckFocus(this);
            }
            else
            {
                GUILayout.Label(text, getGuiLayoutOptions());
            }
        }
    }

    [Serializable]
    public class EditorButton : EditorUI
    {
        public string text = "";

        public EditorButton(string value = "")
        {
            this.text = value;
        }

        public override void onRender()
        {
            if (contentColor != Color.clear)
            {
                oldContentColor = GUI.contentColor;
                GUI.contentColor = contentColor;
            }
            bool b = false;
            if (string.IsNullOrEmpty(styleString) == false)
            {
                style = (GUIStyle) styleString;
            }
            if (style != GUIStyle.none)
            {
                b = GUILayout.Button(text, style, GUILayout.ExpandWidth(expandWidth));
            }
            else
            {
                b = GUILayout.Button(text, GUILayout.ExpandWidth(expandWidth));
            }
            if (b)
            {
                this.simpleDispatch(EventX.ITEM_CLICK, data);
            }

            if (contentColor != Color.clear)
            {
                GUI.contentColor = oldContentColor;
            }
            CheckFocus(this);
        }
    }
    [Serializable]
    public class EditorRadio : EditorUI
    {
        public string label;
        public bool selected;

        public EditorRadio(string label = "")
        {
            this.label = label;
        }

        public override void onRender()
        {
            GUI.changed = false;
            selected = GUILayout.Toggle(selected, label);
            if (GUI.changed)
            {
                this.simpleDispatch(EventX.CHANGE, selected);
            }
        }
    }
    [Serializable]
    public class EditorSpace : EditorUI
    {
        private bool isFlexible = false;
        public float pixels = 5;

        public EditorSpace(bool isFlexible = true)
        {
            this.isFlexible = isFlexible;
        }

        public override void onRender()
        {
            if (isFlexible)
            {
                GUILayout.FlexibleSpace();
            }
            else
            {
                GUILayout.Space(pixels);
            }
        }
    }

    [Serializable]
    public class EditorSlider : EditorUI
    {
        public bool isV = true;
        public float value = 0;
        public float min = 0f;
        public float max = 1f;
        public string label = "";
        public bool canEdit = false;

        public EditorSlider(string label = "", bool isV = false)
        {
            this.label = label;
            this.isV = isV;
        }

        public override void onRender()
        {
            GUILayout.BeginHorizontal();
            if (string.IsNullOrEmpty(label) == false)
            {
                GUILayout.Label(label, GUILayout.ExpandWidth(false));
            }

            GUI.changed = false;
            if (isV == false)
            {
                value = GUILayout.HorizontalSlider(value, min, max, getGuiLayoutOptions());
            }
            else
            {
                value = GUILayout.VerticalSlider(value, min, max, getGuiLayoutOptions());
            }
            GUILayout.Label(value.ToString(), GUILayout.ExpandWidth(false), GUILayout.Width(50));

            if (GUI.changed)
            {
                value = Mathf.Min(Mathf.Max(min, value), max);
                value = (float) Math.Round(value, 2);
                this.simpleDispatch(EventX.CHANGE, value);
            }
            GUILayout.EndHorizontal();
        }

        public void setRank(float min, float max, float value)
        {
            this.min = min;
            this.max = max;
            this.value = value;
        }
    }

    [Serializable]
    public class EditorTabNav : EditorUI
    {
        private List<string> tabs = new List<string>();
        private List<EditorUI> targets = new List<EditorUI>();
        [SerializeField]
        private int _selectedIndex = -1;
        private EditorUI _selectedItem;

        public EditorTabNav() : base()
        {
        }

        public void addItem(string label, EditorUI target)
        {
            tabs.Add(label);
            targets.Add(target);
        }

        public bool selectedTabLabel(string label)
        {
            int i = 0;
            foreach (string s in tabs)
            {
                if (s == label)
                {
                    selectedIndex = i;
                    return true;
                }
                i++;
            }
            return false;
        }

        public int selectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;

                if (_selectedItem != null)
                {
                    this.removeChild(_selectedItem);
                }

                _selectedItem = targets[_selectedIndex];
                if (_selectedItem != null)
                {
                    this.addChild(_selectedItem);
                }
                this.simpleDispatch(EventX.CHANGE, _selectedIndex);
            }
        }

        public EditorUI selectedItem
        {
            get { return _selectedItem; }
            set
            {
                int i = 0;
                foreach (EditorUI s in targets)
                {
                    if (s == value)
                    {
                        selectedIndex = i;
                        break;
                    }
                    i++;
                }
            }
        }

        public override void onRender()
        {
            if (string.IsNullOrEmpty(styleString) == false)
            {
                style = styleString;
            }
            if (style != GUIStyle.none)
            {
                GUILayout.BeginVertical(style, getGuiLayoutOptions());
            }
            else
            {
                GUILayout.BeginVertical(getGuiLayoutOptions());
            }
            GUI.changed = false;
            _selectedIndex = GUILayout.Toolbar(_selectedIndex, tabs.ToArray());
            if (GUI.changed)
            {
                selectedIndex = _selectedIndex;
            }
            base.onRender();
            GUILayout.EndVertical();
        }

        public override void removeAllChildren()
        {
            tabs.Clear();
            targets.Clear();
            base.removeAllChildren();
        }

        public void autoSelected()
        {
            if (_selectedIndex == -1)
            {
                selectedIndex = 0;
            }
            else
            {
                selectedIndex = _selectedIndex;
            }
        }
    }

    public class EditorBox : EditorUI
    {
        public bool isV = true;

        public EditorBox(bool isV = true)
        {
            this.isV = isV;
        }

        public override void onRender()
        {
            if (numChildren == 0)
            {
                return;
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
                GUILayout.EndVertical();
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
                GUILayout.EndHorizontal();
            }
        }
    }

    public class EditorToggleGroup : EditorUI
    {
        public string label = "";
        public bool toggle = true;

        public EditorToggleGroup(string label = "")
        {
            this.label = label;
        }

        public override void onRender()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUI.changed = false;

            Color oldColor = GUI.color;
            GUI.color = new Color(0, 1f, 1f, 1f);
            toggle = EditorGUILayout.BeginToggleGroup(label, toggle);

            GUI.color = oldColor;
            if (GUI.changed)
            {
                this.simpleDispatch(EventX.CHANGE, toggle);
            }

            base.onRender();

            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();
        }
    }

    public class EditorFoldoutGroup : EditorUI
    {
        public string label = "";
        public bool toggle = true;

        public EditorFoldoutGroup(string label = "")
        {
            this.label = label;
        }

        public override void onRender()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            toggle = EditorGUILayout.Foldout(toggle, label);
            if (toggle)
            {
                base.onRender();
            }
            EditorGUILayout.EndVertical();
        }
    }

    public class EditorFormItem : EditorUI
    {
        public string label;

        private string _value = "";

        public string value
        {
            get { return _value; }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if (_value != value)
                {
                    _value = value;
                    this.simpleDispatch(EventX.CHANGE, _value);
                }
            }
        }

        public EditorFormItem(string label = "")
        {
            this.label = label;
        }

        public int lineCount = 1;
        public string searckKey = "";
        private static GUIStyle wordWrap;

        public override void onRender()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(label, GUILayout.ExpandWidth(false));

            EventType typeBackup = Event.current.type;

            GUI.SetNextControlName(controlID);

            GUI.changed = false;

            if (lineCount == 1)
            {
                _value = EditorGUILayout.TextField(_value);
            }
            else
            {
                if (wordWrap == null)
                {
                    wordWrap = new GUIStyle("TextArea");
                    wordWrap.wordWrap = true;
                }
                _value = EditorGUILayout.TextArea(_value, wordWrap, GUILayout.Height(20 * lineCount));
            }
            if (string.IsNullOrEmpty(searckKey) == false)
            {
                if (GUILayout.Button("S", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    SearchList.Show(searckKey, selectBackHandle);
                }
            }

            if (GUI.changed)
            {
                this.simpleDispatch(EventX.CHANGE, _value);
            }
            CheckFocus(this);
            GUILayout.EndHorizontal();
        }

        private void selectBackHandle(string v)
        {
            if (this.window)
            {
                this.window.Focus();
            }

            _value = v;
            this.simpleDispatch(EventX.CHANGE, _value);
        }
    }



    [Serializable]
    public class EditorSearch : EditorUI
    {
        public string value = "";

        public override void onRender()
        {
            GUILayout.BeginHorizontal();
            //GUILayout.Space(84f);
            GUI.changed = false;
            value = EditorGUILayout.TextField("", value, "SearchTextField");
            if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
            {
                value = "";
                GUIUtility.keyboardControl = 0;
            }

            if (GUI.changed)
            {
                this.simpleDispatch(EventX.CHANGE, value);
            }

            GUILayout.EndHorizontal();
        }
    }

    public class EditorVector : EditorBox
    {
        private float _hax = 1;
        private float _value;
        private EditorSlider silder;
        private EditorLabel valueTF;

        public EditorVector(string label = "v:") : base(false)
        {
            this.valueTF = new EditorLabel();
            this.valueTF.expandWidth = false;
            this.valueTF.minWidth = 20;
            this.valueTF.addEventListener(EventX.CHANGE, this.textHandle, 0);
            this.silder = new EditorSlider(label);
            this.silder.addEventListener(EventX.CHANGE, this.silderHandle, 0);
            base.addChild(this.silder);
            base.addChild(this.valueTF);
            this.value = 0f;
        }

        public bool hasValueInput
        {
            get { return valueTF.isDynamic; }
            set { valueTF.isDynamic = value; }
        }

        private float formatValue(float v)
        {
            v = Mathf.Min(Mathf.Max(this.silder.min, v), this.silder.max);
            v = ((int) (v * this._hax)) / this._hax;
            return v;
        }

        public void setRank(float min, float max, float value, int hax = 1)
        {
            this.silder.setRank(min, max, value);
            while (hax-- > 0)
            {
                this._hax *= 10;
            }
        }

        private void silderHandle(EventX e)
        {
            value = this.silder.value;
        }

        private void textHandle(EventX e)
        {
            float result = 0;
            if (!float.TryParse(this.valueTF.text, out result))
            {
                this.valueTF.text = this._value.ToString();
            }
            else
            {
                this.value = result;
            }
        }

        public float value
        {
            get { return this._value; }
            set
            {
                value = this.formatValue(value);
                if (this._value != value)
                {
                    this._value = value;
                    this.silder.value = this.value;
                    this.valueTF.text = this.value.ToString();
                    base.simpleDispatch(EventX.CHANGE, value);
                }
            }
        }
    }


    public class EditorSeparator : EditorUI
    {
        public override void onRender()
        {
            EditorGUILayout.Separator();
        }
    }

    public class EditorFlexibleSpace : EditorUI
    {
        public override void onRender()
        {
            GUILayout.FlexibleSpace();
        }
    }

    public class EditorVector3 : EditorUI
    {
        public string label;

        public EditorVector3(string label = "")
        {
            this.label = label;
        }

        public override void onRender()
        {
            GUI.changed = false;
            value = EditorGUILayout.Vector3Field(label, value);
            if (GUI.changed)
            {
                base.simpleDispatch(EventX.CHANGE, value);
            }
        }

        public Vector3 value;
    }

    public class EditorFloat : EditorUI
    {
        public string label;
        public float value = 0;

        public EditorFloat(string label = "")
        {
            this.label = label;
        }

        public override void onRender()
        {
            GUI.changed = false;
            value = EditorGUILayout.FloatField(label, value);
            if (GUI.changed)
            {
                base.simpleDispatch(EventX.CHANGE, value);
            }
        }

    }

    public class EditorEnum : EditorUI
    {
        public string label;
        public Enum value;
        public EditorEnum(string label = "")
        {
            this.label = label;
        }
        public override void onRender()
        {
            GUI.changed = false;
            value = EditorGUILayout.EnumPopup(label, value);
            if (GUI.changed)
            {
                base.simpleDispatch(EventX.CHANGE, value);
            }
        }
    }

    public class EditorObjectSelect : EditorUI
    {
        public string label;
        public Type type;
        public UnityEngine.Object obj;
        public EditorObjectSelect(Type type,string label = "")
        {
            this.label = label;
            this.type = type;
        }
        public override void onRender()
        {
            GUI.changed = false;
            obj = EditorGUILayout.ObjectField(label, obj, type, false);
            if (GUI.changed)
            {
                base.simpleDispatch(EventX.CHANGE, obj);
            }
        }
    }

}
