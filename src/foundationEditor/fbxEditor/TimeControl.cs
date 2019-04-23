using System;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class TimeControl
    {
        public float currentTime = float.NegativeInfinity;
        private const float kPlayButtonWidth = 33f;
        private const float kScrubberHeight = 21f;
        private static readonly int kScrubberIDHash = "ScrubberIDHash".GetHashCode();
        private const float kStepTime = 0.01f;
        public bool loop = true;
        private float m_DeltaTime = 0f;
        private bool m_DeltaTimeSet = false;
        private double m_LastFrameEditorTime = 0.0;
        private float m_MouseDrag = 0f;
        private bool m_NextCurrentTimeSet = false;
        private bool m_Playing = false;
        private bool m_ResetOnPlay = false;
        private bool m_WrapForwardDrag = false;
        public float playbackSpeed = 1f;
        public bool playSelection = false;
        private static Styles s_Styles;
        public float startTime = 0f;
        public float stopTime = 1f;

        // Methods
        public void DoTimeControl(Rect rect)
        {
            if (s_Styles == null)
            {
                s_Styles = new Styles();
            }
            Event current = Event.current;
            int controlID = GUIUtility.GetControlID(kScrubberIDHash, FocusType.Keyboard);
            Rect position = rect;
            position.height = 21f;
            Rect rect3 = position;
            rect3.xMin += 33f;
            switch (current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (rect.Contains(current.mousePosition))
                    {
                        GUIUtility.keyboardControl = controlID;
                    }
                    if (rect3.Contains(current.mousePosition))
                    {
                        EditorGUIUtility.SetWantsMouseJumping(1);
                        GUIUtility.hotControl = controlID;
                        this.m_MouseDrag = current.mousePosition.x - rect3.xMin;
                        this.nextCurrentTime = ((this.m_MouseDrag * (this.stopTime - this.startTime)) / rect3.width) + this.startTime;
                        this.m_WrapForwardDrag = false;
                        current.Use();
                    }
                    goto Label_02DC;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        EditorGUIUtility.SetWantsMouseJumping(0);
                        GUIUtility.hotControl = 0;
                        current.Use();
                    }
                    goto Label_02DC;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl != controlID)
                    {
                        goto Label_02DC;
                    }
                    this.m_MouseDrag += current.delta.x * this.playbackSpeed;
                    if (!this.loop || (((this.m_MouseDrag >= 0f) || !this.m_WrapForwardDrag) && (this.m_MouseDrag <= rect3.width)))
                    {
                        goto Label_01EC;
                    }
                    if (this.m_MouseDrag <= rect3.width)
                    {
                        if (this.m_MouseDrag < 0f)
                        {
                            this.currentTime += this.stopTime - this.startTime;
                        }
                        break;
                    }
                    this.currentTime -= this.stopTime - this.startTime;
                    break;

                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl == controlID)
                    {
                        if (current.keyCode == KeyCode.LeftArrow)
                        {
                            if ((this.currentTime - this.startTime) > 0.01f)
                            {
                                this.deltaTime = -0.01f;
                            }
                            current.Use();
                        }
                        if (current.keyCode == KeyCode.RightArrow)
                        {
                            if ((this.stopTime - this.currentTime) > 0.01f)
                            {
                                this.deltaTime = 0.01f;
                            }
                            current.Use();
                        }
                    }
                    goto Label_02DC;

                default:
                    goto Label_02DC;
            }
            this.m_WrapForwardDrag = true;
            this.m_MouseDrag = Mathf.Repeat(this.m_MouseDrag, rect3.width);
            Label_01EC:
            this.nextCurrentTime = ((Mathf.Clamp(this.m_MouseDrag, 0f, rect3.width) * (this.stopTime - this.startTime)) / rect3.width) + this.startTime;
            current.Use();
            Label_02DC:
            GUI.Box(position, GUIContent.none, s_Styles.timeScrubber);
            this.playing = GUI.Toggle(position, this.playing, !this.playing ? s_Styles.playIcon : s_Styles.pauseIcon, s_Styles.playButton);
            float x = Mathf.Lerp(rect3.x, rect3.xMax, this.normalizedTime);
            if (GUIUtility.keyboardControl == controlID)
            {
                Handles.color = new Color(1f, 0f, 0f, 1f);
            }
            else
            {
                Handles.color = new Color(1f, 0f, 0f, 0.5f);
            }
            Handles.DrawLine((Vector3)new Vector2(x, rect3.yMin), (Vector3)new Vector2(x, rect3.yMax));
            Handles.DrawLine((Vector3)new Vector2(x + 1f, rect3.yMin), (Vector3)new Vector2(x + 1f, rect3.yMax));
        }

        public void OnDisable()
        {
            this.playing = false;
        }

        public void Update()
        {
            if (!this.m_DeltaTimeSet)
            {
                if (this.playing)
                {
                    double timeSinceStartup = EditorApplication.timeSinceStartup;
                    this.deltaTime = ((float)(timeSinceStartup - this.m_LastFrameEditorTime)) * this.playbackSpeed;
                    this.m_LastFrameEditorTime = timeSinceStartup;
                }
                else
                {
                    this.deltaTime = 0f;
                }
            }
            this.currentTime += this.deltaTime;
            if ((this.loop && this.playing) && !this.m_NextCurrentTimeSet)
            {
                this.normalizedTime = Mathf.Repeat(this.normalizedTime, 1f);
            }
            else
            {
                if (this.normalizedTime > 1f)
                {
                    this.playing = false;
                    this.m_ResetOnPlay = true;
                }
                this.normalizedTime = Mathf.Clamp01(this.normalizedTime);
            }
            this.m_DeltaTimeSet = false;
            this.m_NextCurrentTimeSet = false;
        }

        // Properties
        public float deltaTime
        {
            get
            {
                return this.m_DeltaTime;
            }
            set
            {
                this.m_DeltaTime = value;
                this.m_DeltaTimeSet = true;
            }
        }

        public float nextCurrentTime
        {
            set
            {
                this.deltaTime = value - this.currentTime;
                this.m_NextCurrentTimeSet = true;
            }
        }

        public float normalizedTime
        {
            get
            {
                return ((this.stopTime != this.startTime) ? ((this.currentTime - this.startTime) / (this.stopTime - this.startTime)) : 0f);
            }
            set
            {
                this.currentTime = (this.startTime * (1f - value)) + (this.stopTime * value);
            }
        }

        public bool playing
        {
            get
            {
                return this.m_Playing;
            }
            set
            {
                if (this.m_Playing != value)
                {
                    if (value)
                    { 
                        this.m_LastFrameEditorTime = EditorApplication.timeSinceStartup;
                        if (this.m_ResetOnPlay)
                        {
                            this.nextCurrentTime = this.startTime;
                            this.m_ResetOnPlay = false;
                        }
                    }
                    else
                    {
                    }
                }
                this.m_Playing = value;
            }
        }

        // Nested Types
        private class Styles
        {
            // Fields
            public GUIContent pauseIcon = EditorGUIUtility.IconContent("PauseButton");
            public GUIStyle playButton = "TimeScrubberButton";
            public GUIContent playIcon = EditorGUIUtility.IconContent("PlayButton");
            public GUIStyle timeScrubber = "TimeScrubber";
        }
    }
}