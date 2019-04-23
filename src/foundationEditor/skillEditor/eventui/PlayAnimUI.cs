using System;
using foundation;
using gameSDK;

namespace foundationEditor
{


    [EventUI(typeof(PlayAnimEvent))]
    [EventStack(typeof(AnimatorTrack))]
    public class PlayAnimUI : IEventUI
    {
        private PlayAnimEvent ev;
        private EditorFormItem formItem;
        private EditorRadio isForceRidio;
        private EditorSlider offsetSlider;
        public override string OnGetLabel()
        {
            return "播放动画"; 
        }
        public override void createUI(ISkillEvent value, EditorUI p)
        {
            base.createUI(value, p);

            this.ev=value as PlayAnimEvent;
            formItem = new EditorFormItem("动画:");
            formItem.searckKey = DataSource.ANIMATION;
            formItem.value = ev.aniName;
            formItem.addEventListener(EventX.CHANGE, aniHandle);

            isForceRidio = new EditorRadio("是否强制切换:");
            isForceRidio.selected = ev.isForce;
            isForceRidio.addEventListener(EventX.CHANGE, isForceRidioHandle);

            offsetSlider = new EditorSlider("偏移:");
            offsetSlider.setRank(0f, 1.0f, ev.offsetAvg);
            offsetSlider.value = ev.offsetAvg;
            offsetSlider.addEventListener(EventX.CHANGE, sliderHandle);

            p.addChild(formItem);
            p.addChild(isForceRidio);
            p.addChild(offsetSlider);
        }

        private void sliderHandle(EventX e)
        {
            ev.offsetAvg = offsetSlider.value;
        }

        private void isForceRidioHandle(EventX e)
        {
            ev.isForce = isForceRidio.selected;
        }

        private void aniHandle(EventX e)
        {
            EditorFormItem eui = e.target as EditorFormItem;
            ev.aniName = eui.value;
        }
    }
}