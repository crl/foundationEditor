using foundation;
using gameSDK;

namespace foundationEditor
{
    [EventUI(typeof(EmptyEvent),-1)]
    [EventStack(typeof(EffectTrack),typeof(CameraTrack),typeof(EventTrack),typeof(AnimatorTrack),typeof(SoundTrack))]
    public class EmptyEventUI : IEventUI
    {
        private EmptyEvent ev;
        public override string OnGetLabel()
        {
            return "空帧";
        }

        public override void createUI(ISkillEvent value, EditorUI p)
        {
            base.createUI(value, p);

            this.ev=value as EmptyEvent;
            if (ev == null)
                return;

        }

       
    }
}