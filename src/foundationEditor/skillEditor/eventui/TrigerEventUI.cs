using System;
using foundation;
using gameSDK;

namespace foundationEditor
{
    [EventUI(typeof(TrigerEvent))]
    [EventStack(typeof(EventTrack))]
    public class TrigerEventUI : IEventUI
    {
        private TrigerEvent ev;
        private EditorEnumPopUp grouGroup;
        private EditorFormItem formItem;
        public override string OnGetLabel()
        {
            return "触发事件"; 
        }

        public override void createUI(ISkillEvent value, EditorUI p)
        {
            base.createUI(value, p);
            this.ev = value as TrigerEvent;

            grouGroup = new EditorEnumPopUp();
            grouGroup.selectedIndex = ev.type;
            grouGroup.addEventListener(EventX.CHANGE, ridioGroupHandle);

            p.addChild(grouGroup);

            if (SkillEventType.DIY.CompareTo(grouGroup.selectedIndex)==0)
            {
                formItem = new EditorFormItem("事件:");
                formItem.value = ev.eventType;
                formItem.addEventListener(EventX.CHANGE, effectHandle);

                p.addChild(formItem);
            }
        }

        private void ridioGroupHandle(EventX obj)
        {
            ev.type = (SkillEventType)grouGroup.selectedIndex;

            this.repaint();
        }
        private void effectHandle(EventX e)
        {
            ev.eventType = formItem.value;

           
        }
    }
}