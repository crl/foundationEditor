using foundation;
using gameSDK;

namespace foundationEditor
{
    [EventUI(typeof(CameraShakeEvent))]
    [EventStack(typeof(CameraTrack))]
    public class CameraShakeUI : IEventUI
    {
        public override string OnGetLabel()
        {
            return "相机抖动"; 
        }

        private CameraShakeEvent ev;
        private EditorSlider slider;
        private EditorSlider slider1;
        private EditorVector3 formItem;
        public override void createUI(ISkillEvent value, EditorUI p)
        {
            base.createUI(value, p);

            this.ev=value as CameraShakeEvent;

            slider = new EditorSlider("幅度:");
            slider.setRank(0.1f,10.0f,0.1f);
            slider.value = ev.factor;

            slider1 = new EditorSlider("周期:");
            slider1.setRank(0.1f, 10.0f, 1);
            slider1.value = ev.period;

            formItem = new EditorVector3("偏移:");
            formItem.value = ev.shaderVector;
            formItem.addEventListener(EventX.CHANGE, formHandle);

            slider.addEventListener(EventX.CHANGE, changeHandle);
            slider1.addEventListener(EventX.CHANGE, changeHandle1);

            p.addChild(slider);
            p.addChild(slider1);
            p.addChild(formItem);
        }

        private void changeHandle(EventX obj)
        {
            ev.factor = slider.value;
        }

        private void changeHandle1(EventX obj)
        {
            ev.period = slider1.value;
        }

        private void formHandle(EventX obj)
        {
            ev.shaderVector = formItem.value;
        }
    }
}