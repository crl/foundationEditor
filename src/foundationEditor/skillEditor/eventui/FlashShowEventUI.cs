using foundation;
using gameSDK;

namespace foundationEditor
{

    [EventUI(typeof(FlashShowEvent))]
    [EventStack(typeof(EffectTrack))]
    public class FlashShowEventUI : IEventUI
    {
        private FlashShowEvent ev;
        private EditorRadio showToggle;
        private EditorVector3 offsetFromItem;
        private EditorRadio useTargetToggle;

        public override string OnGetLabel()
        {
            return "闪现";
        }
        public override void createUI(ISkillEvent value, EditorUI p)
        {
            base.createUI(value, p);
            this.ev=value as FlashShowEvent;
            showToggle = new EditorRadio("显示:");
            showToggle.selected = ev.isShow;
            showToggle.addEventListener(EventX.CHANGE, skeletonToggleHandle);

            offsetFromItem = new EditorVector3("坐标偏移:");
            offsetFromItem.addEventListener(EventX.CHANGE, offsetHandle);
            offsetFromItem.value = ev.offset;
            offsetFromItem.visible = showToggle.selected;
            useTargetToggle = new EditorRadio("useTarget:");
            useTargetToggle.selected = ev.useTarget;
            useTargetToggle.addEventListener(EventX.CHANGE, useTargetToggleHandle);

            p.addChild(showToggle);
            p.addChild(offsetFromItem);
            p.addChild(useTargetToggle);
        }
        private void skeletonToggleHandle(EventX e)
        {
            ev.isShow = showToggle.selected;

            if (ev.isShow)
            {
                offsetFromItem.visible = true;
            }
            else
            {
                offsetFromItem.visible = false;
            }
        }
        private void useTargetToggleHandle(EventX e)
        {
            ev.useTarget = useTargetToggle.selected;
        }

        private void offsetHandle(EventX e)
        {
            ev.offset = offsetFromItem.value;
        }


    }
}