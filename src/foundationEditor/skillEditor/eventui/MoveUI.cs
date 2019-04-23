using foundation;
using gameSDK;

namespace foundationEditor
{
    [EventUI(typeof(MoveEvent))]
    [EventStack(typeof(CameraTrack),typeof(EffectTrack))]
    public class MoveUI:EmptyEventUI
    {
        private MoveEvent ev;
        private EditorVector3 formItem;
        private EditorEnumPopUp ridioGroup;

        private EditorRadio resetRidio;
        private EditorRadio checkCollideRidio;
        private EditorRadio isInterpolationRidio;
        private EditorRadio isSpeedRidio;
        private EditorEnum easeType;
        public override string OnGetLabel()
        {
            return "移动";
        }

        public override void createUI(ISkillEvent value, EditorUI p)
        {
            base.createUI(value, p);

            this.ev = value as MoveEvent;

            formItem = new EditorVector3("偏移:");
            formItem.value = ev.position;

            formItem.addEventListener(EventX.CHANGE, aniHandle);

            ridioGroup = new EditorEnumPopUp();
            ridioGroup.addEventListener(EventX.CHANGE, ridioGroupHandle);
            ridioGroup.selectedIndex = ev.type;


            isInterpolationRidio = new EditorRadio("是否插值");
            isInterpolationRidio.selected = ev.isInterpolation;
            isInterpolationRidio.addEventListener(EventX.CHANGE, isInterpolationHandle);


            easeType=new EditorEnum("Ease");
            //easeType.value = ev.easeType;
            easeType.addEventListener(EventX.CHANGE, easeTypeHandle);

            isSpeedRidio = new EditorRadio("是否只按此速度");
            isSpeedRidio.visible = isInterpolationRidio.selected;
            isSpeedRidio.selected = ev.isSpeed;
            isSpeedRidio.addEventListener(EventX.CHANGE, isSpeedRidioHandle);

            checkCollideRidio = new EditorRadio("检查碰撞");
            checkCollideRidio.selected = ev.checkCollider;
            checkCollideRidio.addEventListener(EventX.CHANGE, checkCollideRidioHandle);

            resetRidio = new EditorRadio("播完回复到原位置");
            resetRidio.selected = ev.reback;
            resetRidio.addEventListener(EventX.CHANGE, ridioHandle);

            p.addChild(ridioGroup);
            p.addChild(formItem);
            p.addChild(isInterpolationRidio);
            p.addChild(easeType);
            p.addChild(isSpeedRidio);
            p.addChild(checkCollideRidio);
            if (isInterpolationRidio.selected==false)
            {
                easeType.visible = false;
                isSpeedRidio.visible = false;
            }

          
            p.addChild(resetRidio);
        }

        private void ridioGroupHandle(EventX e)
        {
            ev.type = (EventTargetType)ridioGroup.selectedIndex;
        }

        private void easeTypeHandle(EventX e)
        {
            //ev.easeType = (EaseType)e.data;
        }

        private void isInterpolationHandle(EventX e)
        {
            isSpeedRidio.visible = isInterpolationRidio.selected;
            easeType.visible = isInterpolationRidio.selected;
            ev.isInterpolation = isInterpolationRidio.selected;
        }

        private void checkCollideRidioHandle(EventX e)
        {
            ev.checkCollider = checkCollideRidio.selected;
        }
        private void isSpeedRidioHandle(EventX e)
        {
            ev.isSpeed = isSpeedRidio.selected;
        }
        private void ridioHandle(EventX e)
        {
            ev.reback = resetRidio.selected;
        }
        private void aniHandle(EventX e)
        {
            ev.position = formItem.value;
        }
    }
}