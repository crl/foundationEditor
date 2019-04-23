using foundation;
using gameSDK;

namespace foundationEditor
{
    [EventUI(typeof(TimeScaleEvent))]
    [EventStack(typeof(TimelineTrack))]
    public class TimeScaleUI : IEventUI
    {
        public TimeScaleEvent ev;
        private EditorSlider slider;

        public override string OnGetLabel()
        {
            return "时间缩放"; 
        }
        public override void createUI(ISkillEvent value, EditorUI p)
        {
            base.createUI(value, p);

            this.ev=value as TimeScaleEvent;

            slider = new EditorSlider();
            slider.min = 0.1f;
            slider.max = 1.0f;
            slider.value = ev.timeScale;

            slider.addEventListener(EventX.CHANGE, timeChangeHandle);

            p.addChild(slider);
        }

        private void timeChangeHandle(EventX obj)
        {
            ev.timeScale = slider.value;
        }
    }
}