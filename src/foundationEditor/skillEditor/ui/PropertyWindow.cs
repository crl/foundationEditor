using foundation;
using gameSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class RefEventUISort
    {
        public EventUIAttribute attribute;
        public IEventUI ui;

        public RefEventUISort(IEventUI ui, EventUIAttribute attribute)
        {
            this.ui = ui;
            this.attribute = attribute;
        }
    }

    [Serializable]
    public class PropertyWindow : EditorUI
    {
        private SkillPointVO vo;
        private List<RefEventUISort> refEventUIlist = new List<RefEventUISort>();
        public GenericMenu getGenericMenu(SkillLineVO lineVo, SkillPointVO pointVo, Vector2 v)
        {
            //List<Type> list = new List<Type>();
            GenericMenu menu = new GenericMenu();
            Type lineType = foundation.ObjectFactory.Locate(lineVo.typeFullName);
            Type[] types =ReflectionTools.GetDerivedTypesOf(typeof(IEventUI));

            refEventUIlist.Clear();
            foreach (Type type in types)
            {
                object[] stackAttributes = type.GetCustomAttributes(typeof(EventStackAttribute), false);
                if (stackAttributes == null || stackAttributes.Length==0)
                {
                    continue;
                }
                EventStackAttribute eventStackAttribute = (EventStackAttribute)stackAttributes[0];
                if (eventStackAttribute.types.Contains(lineType))
                {
                    object[] eventUIAttributes=type.GetCustomAttributes(typeof(EventUIAttribute), false);
                    if (stackAttributes == null || stackAttributes.Length == 0)
                    {
                        continue;
                    }
                    RefEventUISort sortItem = new RefEventUISort(Activator.CreateInstance(type) as IEventUI, (EventUIAttribute)eventUIAttributes[0]);
                    refEventUIlist.Add(sortItem);
                }
            }
            refEventUIlist.Sort((a, b) =>
            {
                return a.attribute.priority.CompareTo(b.attribute.priority);
            });

            foreach (RefEventUISort sortItem in refEventUIlist)
            {
                menu.AddItem(new GUIContent(sortItem.ui.label), false, (object userData) =>
                {
                    Type eType = userData as Type;
                    SkillEvent skillEvent = Activator.CreateInstance(eType) as SkillEvent;
                    pointVo.addEvent(skillEvent);
                    show(pointVo);
                }, sortItem.attribute.type);
            }


            return menu;
        }


        private Dictionary<Type,Type> eventMaping=new Dictionary<Type, Type>();
        public void updateEventConnectionUI()
        {
            eventMaping.Clear();
            Type[] types = ReflectionTools.GetDerivedTypesOf(typeof(IEventUI));
            foreach (Type typeUI in types)
            {
                object[] eventUIAttributes = typeUI.GetCustomAttributes(typeof(EventUIAttribute), false);
                if (eventUIAttributes == null || eventUIAttributes.Length == 0)
                {
                    continue;
                }
                EventUIAttribute eventUIAttribute = (EventUIAttribute)eventUIAttributes[0];

                eventMaping.Add(eventUIAttribute.type, typeUI);
            }
        }

        public void show(SkillPointVO vo)
        {
            this.vo = vo;
            updateView();
        }

        public void hide()
        {
            this.removeAllChildren();
        }

        protected void updateView()
        {
            this.removeAllChildren();
            if (vo.isEmpty)
            {
                return;
            }
            updateEventConnectionUI();
            ISkillEvent skillEvent = vo.evt;
            EditorToggleGroup box;

            box = new EditorToggleGroup();
            box.data = skillEvent;
            box.addEventListener(EventX.CHANGE, toggleHandle);
            box.addEventListener(EventX.REPAINT, itemRepaintHandle);
            Type type;

            if (eventMaping.TryGetValue(skillEvent.GetType(), out type))
            {
                IEventUI ui = Activator.CreateInstance(type) as IEventUI;
                box.label = ui.label;
                box.toggle = skillEvent.enabled;
                ui.createUI(skillEvent, box);
            }

            EditorButton btn = new EditorButton("delete");
            btn.style = EditorStyles.miniButton;
            btn.data = skillEvent;
            btn.addEventListener(EventX.ITEM_CLICK, deleteHandle);
            box.addChild(btn);

            this.addChild(box);

        }

        private void toggleHandle(EventX e)
        {
            EditorToggleGroup eui = e.target as EditorToggleGroup;
            ISkillEvent skilEvent = eui.data as ISkillEvent;
            bool b = EditorUtility.DisplayDialog("提示", "确定要更改状态", "是", "否");
            if (b)
            {
                skilEvent.enabled = eui.toggle;
            }
            else
            {
                eui.toggle = skilEvent.enabled;
            }
        }

        private void itemRepaintHandle(EventX e)
        {
            updateView();
        }

        private void deleteHandle(EventX e)
        {
            bool b=EditorUtility.DisplayDialog("提示","确定要删除","是","否");
            if (b)
            {
                EditorButton eui = e.target as EditorButton;
                SkillEvent skilEvent = eui.data as SkillEvent;

                if (vo != null)
                {
                    vo.removeEvent(skilEvent);
                }

                Undo.RecordObject(this.window, "delete SkillEvent");
                updateView();
            }
        }

      
    }
}