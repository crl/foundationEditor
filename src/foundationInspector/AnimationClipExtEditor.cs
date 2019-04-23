
using System;
using System.Collections.Generic;
using System.IO;
using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    //[CustomEditor(typeof(AnimationClip))]
    public class AnimationClipExtEditor : DecoratorEditor
    {
        private GUIStyle style;
        private bool isDirty = false;
        private string savePath=null;
        private int selectKeyFrameIndex = -1;
        private AnimationEvent selectKeyFrameInfo = null;
        private int kScrubberIDHash = "kScrubberIDHash".GetHashCode();
        public AnimationClipExtEditor() : base("AnimationClipEditor")
        {
        }

        protected virtual void OnEnable()
        {
            isDirty = false;
            savePath = null;
            selectKeyFrameInfo = null;

            string path = AssetDatabase.GetAssetPath(target);
            string extension = Path.GetExtension(path).ToLower();
            if (extension == ".anim")
            {
                savePath = path.Replace(".anim", ".amf");
            }else if (extension == ".fbx")
            {
                string newPath=Path.GetDirectoryName(path);
                savePath = Path.Combine(newPath, target.name + ".amf");
            }
        }

        public override bool HasPreviewGUI()
        {
            bool b= base.HasPreviewGUI();
            return b;
        }

        public override void OnPreviewSettings()
        {
            CallInspectorMethod("OnPreviewSettings");
        }


        public override void DrawPreview(Rect previewArea)
        {
            if (style == null)
            {
                style = (GUIStyle)"MeTransPlayhead";
            }
            AnimationClip target = base.target as AnimationClip;
            //AnimationClipSettings animationClipSettings = AnimationUtility.GetAnimationClipSettings(target);

            Rect rect;
            int offsetY = 15;
            rect = previewArea;
            rect.height = offsetY;
            GUI.Box(rect, GUIContent.none);
            rect.x += 33;
            Event current = Event.current;
            int controlID = GUIUtility.GetControlID(kScrubberIDHash, FocusType.Keyboard, previewArea);
            EventType eventType = current.GetTypeForControl(controlID);
            switch (eventType)
            {
                case EventType.MouseDown:

                    if (rect.Contains(current.mousePosition) && current.button == 1)
                    {
                        GenericMenu menu = new GenericMenu();
                        foreach (string animationClipEvent in EditorConfigUtils.AnimationClipEvents)
                        {
                            AnimationEvent animationEvent = new AnimationEvent();
                            animationEvent.time = ((current.mousePosition.x - rect.x) / (rect.width - rect.x))*target.length;
                            animationEvent.stringParameter = animationClipEvent;
                            animationEvent.functionName = "receiptAnimationEvent";
                            menu.AddItem(new GUIContent(animationClipEvent), false, menuHandle, animationEvent);
                        }
                        menu.ShowAsContext();
                    }
                    break;
            }


            Handles.color = Color.green;
            int size = offsetY;

            float maxWidth = rect.width - rect.x;
            int i = 0;
            foreach (AnimationEvent animationEvent in target.events)
            {
                float percent=animationEvent.time / target.length;
                float x = rect.x + percent * maxWidth;
                GUIContent c = new GUIContent();
                c.tooltip = animationEvent.functionName;
                Rect tempRect = new Rect(x - size / 2f, rect.y, size, rect.height);
                GUI.Box(tempRect, c, style);

                if (eventType == EventType.MouseDown)
                {
                    if (tempRect.Contains(current.mousePosition))
                    {
                        if (current.button == 1)
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("clear"), false, clearHandle, animationEvent);
                            menu.ShowAsContext();
                            current.Use();
                        }
                        selectKeyFrameIndex = i;
                        selectKeyFrameInfo = animationEvent;
                        //Debug.Log(keyFrameInfo.percent);
                        
                    }
                }
                i++;
            }

            switch (eventType)
            {
                case EventType.MouseDrag:
                    if (selectKeyFrameInfo != null)
                    {
                        float v = Mathf.Min(Mathf.Max(current.mousePosition.x - rect.x, 0), maxWidth);
                        selectKeyFrameInfo.time = (v / maxWidth)*target.length;

                        if (selectKeyFrameIndex > -1)
                        {
                            isDirty = true;
                            AnimationEvent[] events = AnimationUtility.GetAnimationEvents(target);
                            if (selectKeyFrameIndex < events.Length)
                            {
                                events[selectKeyFrameIndex] = selectKeyFrameInfo;
                                AnimationUtility.SetAnimationEvents(target, events);
                            }
                        }
                        current.Use();
                     
                    }
                    break;

                case EventType.MouseUp:
                case EventType.Used:
                    selectKeyFrameIndex = -1;
                    selectKeyFrameInfo = null;
                    break;
            }


            rect = previewArea;
            rect.width = 40;
            rect.height = offsetY;
            

            if (isDirty && string.IsNullOrEmpty(savePath) == false)
            {
                if (GUI.Button(rect, "save", (GUIStyle) "sv_label_5"))
                {
                    List<KeyFrameInfo> list=new List<KeyFrameInfo>(); 
                    foreach (AnimationEvent animationEvent in target.events)
                    {
                        KeyFrameInfo item = new KeyFrameInfo();
                        item.time = animationEvent.time;
                        item.func = animationEvent.functionName;
                        item.stringParameter = animationEvent.stringParameter;
                        item.intParameter = animationEvent.intParameter;
                        item.floatParameter = animationEvent.floatParameter;

                        if (animationEvent.objectReferenceParameter)
                        {
                            item.objectReferenceParameterInstanceID =
                                animationEvent.objectReferenceParameter.GetInstanceID();
                        }
                        list.Add(item);
                    }
                    AmfHelper.save(list, savePath);
                    isDirty = false;
                }
            }

            previewArea.y += offsetY;
            previewArea.height -= offsetY;
            base.DrawPreview(previewArea);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            AnimationClip target = base.target as AnimationClip;
            if (target.isHumanMotion == false)
            {
                //todo;
            }
            if (GUILayout.Button("Editor"))
            {
                Type type = foundation.ObjectFactory.Locate("UnityEditor.AnimationWindow");

                EditorWindow.GetWindow(type);
            }
        }

        private void clearHandle(object s)
        {
            AnimationClip target = base.target as AnimationClip;
            AnimationEvent[] events=AnimationUtility.GetAnimationEvents(target);
            AnimationEvent e = (AnimationEvent) s;
            List<AnimationEvent> list = new List<AnimationEvent>();
            bool has = false;
            foreach (AnimationEvent animationEvent in events)
            {
                if (animationEvent.time == e.time)
                {
                    has = true;
                    continue;
                }
                list.Add(animationEvent);
            }

            if (has)
            {
                isDirty = true;
                AnimationUtility.SetAnimationEvents(target, list.ToArray());
            }
        }

        private void menuHandle(object s)
        {
            AnimationClip target = base.target as AnimationClip;
            AnimationEvent[] events = AnimationUtility.GetAnimationEvents(target);
            AnimationEvent e = (AnimationEvent) s;
            if (e!=null)
            {
                isDirty = true;
                ArrayUtil.AddItem(ref events, e);
                AnimationUtility.SetAnimationEvents(target, events);
            }
        }
    }
}