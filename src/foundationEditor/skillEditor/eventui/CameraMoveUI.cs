using foundation;
using gameSDK;

namespace foundationEditor
{
    [EventUI(typeof(CameraMoveEvent))]
    [EventStack(typeof(CameraTrack))]
    public class CameraMoveUI : IEventUI
    {
        public override string OnGetLabel()
        {
            return "摄像机偏移";
        }
        private EditorVector3 formItem;
        private CameraMoveEvent ev;
        private EditorRadio checkRadio;
        private EditorRadio forceRadio;
        public override void createUI(ISkillEvent value, EditorUI p)
        {
            base.createUI(value, p);

            ev =value as CameraMoveEvent;
   
            formItem = new EditorVector3("偏移:");
            formItem.value = ev.position;
            formItem.addEventListener(EventX.CHANGE, formHandle);

            checkRadio=new EditorRadio("获取焦点");
            checkRadio.selected = ev.focusGet;
            checkRadio.addEventListener(EventX.CHANGE, checkRadioHandle);

            if (checkRadio.selected)
            {
                forceRadio = new EditorRadio("强转");
                forceRadio.selected = ev.forceChange;
                forceRadio.addEventListener(EventX.CHANGE, checkRadioHandle);
            }

            p.addChild(formItem);
            p.addChild(checkRadio);
        }

        private void checkRadioHandle(EventX e)
        {
            bool b = (bool) e.data;
            if (e.target == checkRadio)
            {
                ev.focusGet = b;
                repaint();
            }
            else
            {
                ev.forceChange = b;
            }
        }

        private void formHandle(EventX obj)
        {
            ev.position = formItem.value;
        }
    }
}