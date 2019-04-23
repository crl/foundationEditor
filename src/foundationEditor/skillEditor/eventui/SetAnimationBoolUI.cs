using System;
using foundation;
using gameSDK;

namespace foundationEditor
{
    [EventUI(typeof(SetAnimationBoolEvent))]
    [EventStack(typeof(AnimatorTrack))]
    public class SetAnimationBoolUI : IEventUI
    {
        private SetAnimationBoolEvent ev;
        private EditorFormItem formItem;
        private EditorRadio isResetRidio;
        private EditorRadio valueRedio;
        public override string OnGetLabel()
        {
            return "设置动画bool参"; 
        }
        public override void createUI(ISkillEvent value, EditorUI p)
        {
            base.createUI(value, p);

            this.ev=value as SetAnimationBoolEvent;
            formItem = new EditorFormItem("参数:");
            formItem.searckKey = DataSource.ANIMATION_PARMS;
            formItem.value = ev.key;
            formItem.addEventListener(EventX.CHANGE, aniHandle);

         
            valueRedio = new EditorRadio("值:");
            valueRedio.selected = ev.value;
            valueRedio.addEventListener(EventX.CHANGE, valueHandle);

            isResetRidio = new EditorRadio("播完重置:");
            isResetRidio.selected = ev.resetDefault;
            isResetRidio.addEventListener(EventX.CHANGE, resetHandle);


            p.addChild(formItem);
            p.addChild(valueRedio);
            p.addChild(isResetRidio);
        }

        private void valueHandle(EventX e)
        {
            ev.value = valueRedio.selected;
        }

        private void resetHandle(EventX e)
        {
            ev.resetDefault = isResetRidio.selected;
        }

        private void aniHandle(EventX e)
        {
            EditorFormItem eui = e.target as EditorFormItem;
            ev.key = eui.value;
        }
    }
}