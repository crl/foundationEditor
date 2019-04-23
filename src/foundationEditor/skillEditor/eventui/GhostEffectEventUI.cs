using System;
using foundation;
using gameSDK;

namespace foundationEditor
{
    [EventUI(typeof(GhostEffectEvent))]
    [EventStack(typeof(EffectTrack))]
    public class GhostEffectEventUI : IEventUI
    {
        public GhostEffectEvent ev;
        private EditorSlider durationSlider;
        private EditorSlider intervalSlider;
        private EditorRadio radio;

        public override string OnGetLabel()
        {
            return "Ghost";
        }

        public override void createUI(ISkillEvent value, EditorUI p)
        {
            base.createUI(value, p);

            this.ev = value as GhostEffectEvent;

            durationSlider = new EditorSlider("duration:");
            durationSlider.min = 0.1f;
            durationSlider.max = 5.0f;
            durationSlider.value = ev.duration;
            durationSlider.addEventListener(EventX.CHANGE, durationSliderHandle);

            intervalSlider = new EditorSlider("interval");
            intervalSlider.min = 0.1f;
            intervalSlider.max = 5.0f;
            intervalSlider.value = ev.interval;
            intervalSlider.addEventListener(EventX.CHANGE, intervalSliderHandle);

            radio=new EditorRadio("onPositionChange:");
            radio.selected = ev.onPositionChange;
            radio.addEventListener(EventX.CHANGE, radioHandle);

            p.addChild(durationSlider);
            p.addChild(intervalSlider);
            p.addChild(radio);
        }

        private void radioHandle(EventX e)
        {
            ev.onPositionChange = radio.selected;
        }

        private void intervalSliderHandle(EventX e)
        {
            ev.interval = intervalSlider.value;
        }

        private void durationSliderHandle(EventX obj)
        {
            ev.duration = durationSlider.value;
        }
    }
}