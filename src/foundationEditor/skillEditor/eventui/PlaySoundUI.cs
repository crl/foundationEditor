using System;
using foundation;
using gameSDK;

namespace foundationEditor
{
    [EventUI(typeof(PlaySoundEvent))]
    [EventStack(typeof(SoundTrack))]
    public class PlaySoundUI : IEventUI
    {
        private PlaySoundEvent ev;
        private EditorRadio heroRadio;
        private EditorRadio onceRadio;
        private EditorFormItem formItem1;
        private EditorFormItem formItem2;
        private EditorFormItem formItem3;
        private EditorFormItem formItem4;

        public override string OnGetLabel()
        {
            return "播放声音"; 
        }

        public override void createUI(ISkillEvent value, EditorUI p)
        {
            base.createUI(value, p);

            this.ev = value as PlaySoundEvent;

            heroRadio=new EditorRadio("仅主角");
            heroRadio.addEventListener(EventX.CHANGE, radioHandle);
            heroRadio.selected = ev.isOnlyHero;

            onceRadio = new EditorRadio("once");
            onceRadio.addEventListener(EventX.CHANGE, onceRadioHandle);
            onceRadio.selected = ev.isOnce;

            formItem1 = new EditorFormItem("声音1:");
            formItem1.addEventListener(EventX.CHANGE, soundHandle1);
            formItem1.searckKey = DataSource.SOUND;
            formItem1.value = ev.m_sound1;
           

            formItem2 = new EditorFormItem("声音2:");
            formItem2.addEventListener(EventX.CHANGE, soundHandle1);
            formItem2.searckKey = DataSource.SOUND;
            formItem2.value = ev.m_sound2;
           

            formItem3 = new EditorFormItem("声音3:");
            formItem3.addEventListener(EventX.CHANGE, soundHandle1);
            formItem3.searckKey = DataSource.SOUND;
            formItem3.value = ev.m_sound3;
           

            formItem4 = new EditorFormItem("声音4:");
            formItem4.addEventListener(EventX.CHANGE, soundHandle1);
            formItem4.searckKey = DataSource.SOUND;
            formItem4.value = ev.m_sound4;

            p.addChild(heroRadio);
            p.addChild(formItem1);
            p.addChild(formItem2);
            p.addChild(formItem3);
            p.addChild(formItem4);
            p.addChild(onceRadio);

            p.windowRepaint();
        }

        private void onceRadioHandle(EventX e)
        {
            ev.isOnce = onceRadio.selected;
        }
        private void radioHandle(EventX e)
        {
            ev.isOnlyHero = heroRadio.selected;
        }

        private void soundHandle1(EventX e)
        {
            if (e.target == formItem1)
            {
                this.ev.m_sound1 = formItem1.value;
            }
            else if (e.target == formItem1)
            {
                this.ev.m_sound2 = formItem2.value;
            }
            else if (e.target == formItem1)
            {
                this.ev.m_sound3 = formItem3.value;
            }
            else if (e.target == formItem1)
            {
                this.ev.m_sound4 = formItem3.value;
            }
        }
    }
}