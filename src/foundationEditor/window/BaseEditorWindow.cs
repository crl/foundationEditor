using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class BaseEditorWindow : EditorWindow
    {
        protected int laterGUICounter = 0;
        private BaseEditorWindow singleUpdate;
        protected EditorStage stage;
        public bool isV = true;
        public GUIStyle style = GUIStyle.none;
        public string styleString = "";

        [NonSerialized]
        protected bool isinitialized = false;
        public BaseEditorWindow(bool isV = true)
        {
            this.isV = isV;
            stage = new EditorStage();
            stage.__window = this;
        }

        protected bool _canRepaint = true;
        public void DelayCallRepaint()
        {
            if (_canRepaint)
            {
                _canRepaint = false;
                this.Repaint();
            }
        }

        protected virtual void initialization()
        {
            if (singleUpdate == null)
            {
                singleUpdate = this;
                preTime = Time.time;
            }
        }

        public EditorUI Root
        {
            get { return stage; }
        }

        protected virtual void Awaken()
        {
            
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnEnable()
        {
            if (isinitialized == false)
            {
                initialization();
                isinitialized = true;
            }
        }

        protected virtual void OnDestroy()
        {
            
        }

        public void addChild(EditorUI value)
        {
            stage.addChild(value);
        }

        public void RemoveAllChildren()
        {
            stage.removeAllChildren();
        }

        private static float preTime;
        protected virtual void Update()
        {
            if (singleUpdate == this)
            {
                float deltaTime = Time.time - preTime;
                Action<float>[] list= tickList.ToArray();
                foreach (Action<float> action in list)
                {
                    action(deltaTime);
                }
                preTime = Time.time;
            }
         
        }

        protected virtual void OnGUI()
        {
            if (EditorApplication.isCompiling)
            {
                ShowNotification(new GUIContent("Compiling\n...Please wait..."));
                return;
            }

            _canRepaint = true;
            EditorUI.CheckLostFocus();
            stage.x = (int) this.position.x;
            stage.y = (int) this.position.y;
            stage.stageWidth = (int) this.position.width;
            stage.stageHeight = (int) this.position.height;
            if (stage.numChildren == 0)
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
                    GUILayout.BeginVertical(style);
                }
                else
                {
                    GUILayout.BeginVertical();
                }
                stage.onRender();
                EditorGUILayout.EndVertical();
            }
            else
            {
                if (style != GUIStyle.none)
                {
                    GUILayout.BeginHorizontal(style);
                }
                else
                {
                    GUILayout.BeginHorizontal();
                }
                stage.onRender();
                EditorGUILayout.EndHorizontal();
            }            
        }

        private static List<Action<float>> tickList=new List<Action<float>>(); 
        public void AddTick(Action<float> tickHandle)
        {
           int index= tickList.IndexOf(tickHandle);
            if (index == -1)
            {
                tickList.Add(tickHandle);
            }
        }

        public void RemoveTick(Action<float> tickHandle)
        {
            int index = tickList.IndexOf(tickHandle);
            if (index != -1)
            {
                tickList.RemoveAt(index);
            }
        }
    }
}