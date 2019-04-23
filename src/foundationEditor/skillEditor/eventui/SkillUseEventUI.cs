using gameSDK;

namespace foundationEditor
{
    //[EventUI(typeof(SkillUseEvent))]
    //[EventStack(typeof(EffectTrack))]
    public class SkillUseEventUI : IEventUI
    {
        //private SkillUseEvent ev;
        public override void createUI(ISkillEvent value, EditorUI p)
        {
            base.createUI(value, p);
        }

        public override string OnGetLabel()
        {
            return "供技能使用";
        }
    }
}