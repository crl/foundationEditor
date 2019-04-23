using foundation;
using gameSDK;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class TimeLine : EditorUI
    {
        public static string RESOURCE_BASEPATH = "Assets/TimeFlowShiki/Editor/GUI/Res/";
        public const float TACK_5FRAME_WIDTH = 80f;
        public const float TACK_FRAME_WIDTH = TACK_5FRAME_WIDTH/5f;
        public const float TACK_FRAME_HEIGHT = 25f;
        public const float TACK_FRAME_TEXT_HEIGHT = 20f;
        public PropertyWindow propertyWindow;
        private List<SkillLineVO> _dataProvider;
        private Vector2 willDraggerDownPosition = Vector2.zero;

        private Vector2 scrollPosition;
      
        private Rect fullRect;
        private Rect viewRect;

        private Rect timeLineBound;
        private int linePadX = 300;
        public static Texture2D bgTex;
        private Texture2D tickTex;
        private Texture2D frameTex;
        private Texture2D whitePointTex;

        public Action<Vector2, string> genericMenuEditorCallBack;
        public Action<string> addMenuEditorCallBack;
        public TimeLine()
        {
            string projectPath = EditorConfigUtils.ProjectResource;
            load(projectPath);
        }

        public void load(string resourcePath)
        {
            if (resourcePath != null)
            {
                RESOURCE_BASEPATH = "SkillEditor.";
            }

            bgTex = EditorResourceUtils.LoadTextureFromDll(RESOURCE_BASEPATH+"bg", 16, 16);
            tickTex = EditorResourceUtils.LoadTextureFromDll(RESOURCE_BASEPATH + "tick", 16, 16);
            frameTex = EditorResourceUtils.LoadTextureFromDll(RESOURCE_BASEPATH + "5frame", 16, 16);
            whitePointTex = EditorResourceUtils.LoadTextureFromDll(RESOURCE_BASEPATH + "blankPoint", 16, 16);
        }

        public List<SkillLineVO> dataProvider
        {
            get { return this._dataProvider; }
            set { this._dataProvider = value; }
        }

        private void deleteHandle(EventX e)
        {
            EditorUI radio = e.target as EditorUI;
            this.simpleDispatch(EventX.CLOSE, radio.data);
        }

        public int Count
        {
            get
            {
                if (_dataProvider == null)
                {
                    return 0;
                }
                return _dataProvider.Count;
            }
        }

        public void Clear()
        {
            _dataProvider = null;
        }

        private Dictionary<string,ITrack> stackCaches=new Dictionary<string, ITrack>();
        public override void onRender()
        {
            if (_dataProvider == null)
            {
                return;
            }
            int timeLineCount = _dataProvider.Count;

            if (timeLineCount < 1)
            {
                return;
            }

            if (bgTex == null)
            {
                load(null);
            }

            float w = 0;
            if (propertyWindow.numChildren > 0)
            {
                w = 300;
            }

            Rect rect = GUILayoutUtility.GetRect(0, this.stage.stageWidth-w, 0, this.stage.stageHeight-60);
            GUI.BeginGroup(rect);

            viewRect = new Rect(0, 0, rect.width, rect.height);
           
            fullRect = new Rect(0, 0, TACK_5FRAME_WIDTH*70, timeLineCount*TACK_FRAME_HEIGHT + TACK_FRAME_TEXT_HEIGHT);
            scrollPosition = GUI.BeginScrollView(viewRect, scrollPosition, fullRect);
            
            int startIndex = (int) (scrollPosition.x/TACK_5FRAME_WIDTH);
            int repeatCount = (int) ((rect.width - linePadX)/TACK_5FRAME_WIDTH);
            int drawOffset = 3;//1;
            int endIndex = startIndex + repeatCount + drawOffset;

            float yPos = 0;
            for (int i = 0; i < timeLineCount; i++)
            {
                yPos = i*TACK_FRAME_HEIGHT + TACK_FRAME_TEXT_HEIGHT;
                for (int j = startIndex; j < endIndex; j++)
                {
                    Rect r = new Rect(linePadX + j*TACK_5FRAME_WIDTH, yPos, TACK_5FRAME_WIDTH, TACK_FRAME_HEIGHT-2);
                    if (frameTex != null)
                    {
                        GUI.DrawTexture(r, frameTex, ScaleMode.ScaleAndCrop, false);
                    }
                }
                SkillLineVO lineVo = _dataProvider[i];
                List<SkillPointVO> skillPointVos = lineVo.points;
                ITrack track = getSackByLine(lineVo);
                int plen= Mathf.Min(skillPointVos.Count,endIndex*5);
                for (int k= startIndex*5; k < plen; k++)
                {
                    SkillPointVO skillPointVo = skillPointVos[k];
                    if (skillPointVo.isEmpty == false)
                    {
                        Rect r=new Rect(linePadX+k*TACK_FRAME_WIDTH+3,yPos+8, 10,10);
                        if (whitePointTex != null)
                        {
                            track.drawPoint(skillPointVo, lineVo, r, whitePointTex);
                        }
                    }
                }
            }

            //时间轴范围;
            timeLineBound.x = rect.x;
            timeLineBound.y = rect.y + TACK_FRAME_TEXT_HEIGHT;
            timeLineBound.width = rect.width;
            timeLineBound.height = rect.height - TACK_FRAME_TEXT_HEIGHT;

            GUI.DrawTexture(new Rect(linePadX, scrollPosition.y, fullRect.width, TACK_FRAME_TEXT_HEIGHT), bgTex);
            GUI.Label(new Rect(linePadX, scrollPosition.y, 10, TACK_FRAME_TEXT_HEIGHT), (startIndex + 1).ToString());
            for (int i = startIndex + 1; i < endIndex; i++)
            {
                string frameCountStr = (i*5).ToString();
                Rect r = new Rect(linePadX + i*TACK_5FRAME_WIDTH - TACK_FRAME_WIDTH, scrollPosition.y+2,
                    frameCountStr.Length*10,
                    TACK_FRAME_TEXT_HEIGHT);
                GUI.Label(r, frameCountStr);
            }
            //绘制红色的当前帧位置
            Rect rrr = new Rect(linePadX+(cursorPos+0.5f) * TACK_FRAME_WIDTH, scrollPosition.y, 1, timeLineBound.height);
            if (tickTex != null)
            {
                GUI.DrawTexture(rrr, tickTex);
            }

            for (int i = 0; i < timeLineCount; i++)
            {
                Rect rr = new Rect(scrollPosition.x, (i * TACK_FRAME_HEIGHT) + TACK_FRAME_TEXT_HEIGHT, linePadX, TACK_FRAME_HEIGHT-2);
                SkillLineVO lineVo=_dataProvider[i];
                ITrack track = getSackByLine(lineVo);
                track.OnGUI(rr, lineVo, i);
            }

            GUI.EndScrollView();

            if (GUI.Button(new Rect(0, 0, 20, 20), "+", EditorStyles.miniButton))
            {
                GenericMenu menu = new GenericMenu();

                Type[] types = ReflectionTools.GetDerivedTypesOf(typeof(BaseTrack));
                foreach (Type type in types)
                {
                    string name = type.FullName;
                    menu.AddItem(new GUIContent(type.Name), false, (object userData) =>
                    {
                        addMenuEditorCallBack((string)userData);
                    }, name);
                }

                menu.ShowAsContext();
            }
            GUI.EndGroup();

            Event currentEvent = Event.current;
            Vector2 mousePosition = currentEvent.mousePosition;
            if (timeLineBound.Contains(mousePosition) == false)
            {
                return;
            }

            SkillPointVO vo;
            Vector2 v=Vector2.zero;
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    v = getTimeLineFrame(mousePosition);
                    vo = getSkillPointVoByPosition(v);
                    willDraggerDownPosition = mousePosition;

                    currentEvent.Use();
                    simpleDispatch(EventX.SELECT, vo);
                    break;
                case EventType.MouseUp:
                    v = getTimeLineFrame(mousePosition);
                    vo = getSkillPointVoByPosition(v);

                    if (willDraggerDownPosition != Vector2.zero && vo != null)
                    {
                        Vector2 distance = mousePosition - willDraggerDownPosition;
                        if (distance.y * distance.y < TACK_FRAME_HEIGHT * TACK_FRAME_HEIGHT &&
                            distance.x * distance.x > TACK_FRAME_WIDTH * TACK_FRAME_WIDTH)
                        {
                            v = getTimeLineFrame(willDraggerDownPosition, false);
                            SkillPointVO moveVO = getSkillPointVoByPosition(v);

                            if (moveVO != null && moveVO.isEmpty == false && vo.isEmpty==true)
                            {
                                SkillEvent moveEvent = moveVO.evt;
                                moveVO.removeEvent(moveEvent);
                                vo.addEvent(moveEvent);
                                currentEvent.Use();
                            }
                        }
                    }
                    willDraggerDownPosition = Vector2.zero;

                    break;

                case EventType.ContextClick:

                    v = getTimeLineFrame(mousePosition);
                    vo = getSkillPointVoByPosition(v);

                    currentEvent.Use();
                    if (vo != null)
                    {
                        SkillLineVO lineVo = getSkillLineVOByPosition(v);
                        GenericMenu menu = new GenericMenu();

                        if (vo.isEmpty)
                        {
                            menu = propertyWindow.getGenericMenu(lineVo, vo, v);
                            menu.AddItem(new GUIContent("加点+"), false, () =>
                            {
                                genericMenuEditorCallBack(v, "AddPointer");
                            });

                            menu.AddItem(new GUIContent("减点-"), false, () =>
                            {
                                genericMenuEditorCallBack(v, "RemovePointer");
                            });

                            menu.AddItem(new GUIContent("粘贴"), false, () =>
                            {
                                genericMenuEditorCallBack(v, "Parse");
                            });
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("复制"), false, () =>
                            {
                                genericMenuEditorCallBack(v, "Copy");
                            });
                        }

                        menu.ShowAsContext();
                    }
                    else if (v.y < _dataProvider.Count && v.x<0)
                    {
                        GenericMenu menu = GenericMenuEditor(v);
                        menu.ShowAsContext();
                    }
                    break;

                case EventType.MouseMove:
                    getTimeLineFrame(currentEvent.mousePosition);
                    break;
                case EventType.MouseDrag:
                    getTimeLineFrame(currentEvent.mousePosition);
                    this.window.Repaint();
                    break;

                case EventType.KeyDown:

                    v = getTimeLineFrame(mousePosition);
                    vo = getSkillPointVoByPosition(v);
                    if (vo != null && vo.isEmpty)
                    {
                        if (currentEvent.keyCode == KeyCode.Equals || currentEvent.keyCode==KeyCode.Plus)
                        {
                            currentEvent.Use();
                            genericMenuEditorCallBack(v, "AddPointer");
                            this.window.Repaint();
                        }
                        else if (currentEvent.keyCode == KeyCode.Minus)
                        {
                            currentEvent.Use();
                            genericMenuEditorCallBack(v, "RemovePointer");
                            this.window.Repaint();
                        }
                    }
                    break;
            }

        }

        private ITrack getSackByLine(SkillLineVO lineVo)
        {
            ITrack track = null;
            string typeFullName = lineVo.typeFullName;
            if (stackCaches.TryGetValue(typeFullName, out track) == false)
            {
                Type type = foundation.ObjectFactory.Locate(typeFullName);
                if (type == null)
                {
                    track = new BaseTrack();
                }
                else
                {
                    track = Activator.CreateInstance(type) as ITrack;
                    if (track == null)
                    {
                        track = new BaseTrack();
                    }
                }
                stackCaches.Add(typeFullName, track);
            }
            return track;
        }

        protected GenericMenu GenericMenuEditor(Vector2 v)
        {
            GenericMenu menu = new GenericMenu();
            List<string> list = new List<string>();
            list.Add("Up");
            list.Add("Down");
            list.Add("Remove");
            foreach (string label in list)
            {
                menu.AddItem(new GUIContent(label), false, (object userData) =>
                {
                    genericMenuEditorCallBack(v, (string)userData);
                }, label);
            }

            return menu;
        }

        private int cursorPos;


        public int keyFramePosition
        {
            set { cursorPos = value; }
            get { return cursorPos; }
        }

        public SkillLineVO getSkillLineVOByPosition(Vector2 v)
        {
            if (v.y >= _dataProvider.Count || v.x < 0)
            {
                return null;
            }
            SkillLineVO skillLineVo = _dataProvider[(int) v.y];
            if (skillLineVo == null)
            {
                return null;
            }
            return skillLineVo;
        }

        public SkillPointVO getSkillPointVoByPosition(Vector2 v)
        {
            SkillLineVO skillLineVo = getSkillLineVOByPosition(v);
            if (skillLineVo == null)
            {
                return null;
            }

            if (v.x >= skillLineVo.points.Count)
            {
                return null;
            }
            return skillLineVo.points[(int)v.x];
        }

        public Vector2 getTimeLineFrame(Vector2 mousePosition, bool autoCur = true)
        {
            float x = (mousePosition.x - timeLineBound.x - linePadX + scrollPosition.x) / TACK_FRAME_WIDTH;
            float y = (mousePosition.y - timeLineBound.y + scrollPosition.y) / TACK_FRAME_HEIGHT;
            if (x > 0 && autoCur)
            {
                cursorPos = (int)x;
            }
            return new Vector2((int)x, (int)y);
        }
    }
}