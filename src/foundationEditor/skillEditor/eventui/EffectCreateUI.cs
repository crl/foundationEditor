using System;
using foundation;
using gameSDK;

namespace foundationEditor
{
    [EventUI(typeof(EffectCreateEvent))]
    [EventStack(typeof(EffectTrack))]
    public class EffectCreateUI:IEventUI
    {
        private EffectCreateEvent ev;

        private EditorFormItem effectFormItem;
        private EditorRadio skeletonToggle;
        private EditorRadio bindOnceToggle;
        private EditorSlider particlePlaybackSpeedSlider;

        private EditorRadio useTargetToggle;
        private EditorRadio isColliderToggle;
        private EditorRadio isUseTargetLayerToggle;
        private EditorFormItem skeletonFormItem;
        private EditorVector3 offsetFromItem;
        private EditorVector3 rotationFromItem;
        public override string OnGetLabel()
        {
            return "加载特效"; 
        }

        public override void createUI(ISkillEvent value, EditorUI p)
        {
            base.createUI(value, p);

            this.ev = value as EffectCreateEvent;
            effectFormItem = new EditorFormItem("特效:");

            effectFormItem.value = ev.effectPath;
            effectFormItem.addEventListener(EventX.CHANGE, effectHandle);
            effectFormItem.searckKey = DataSource.EFFECT;
            p.addChild(effectFormItem);

            skeletonToggle = new EditorRadio("绑定身体:");
            skeletonToggle.selected = ev.isBindSkeleton;
            skeletonToggle.addEventListener(EventX.CHANGE, skeletonToggleHandle);

            bindOnceToggle = new EditorRadio("一次性对位而已:");
            bindOnceToggle.selected = ev.isBindOnce;
            bindOnceToggle.addEventListener(EventX.CHANGE, bindOnceToggleHandle);
            bindOnceToggle.visible = ev.isBindSkeleton;

            skeletonFormItem = new EditorFormItem("骨骼:");
            skeletonFormItem.value = ev.skeletonName;
            skeletonFormItem.addEventListener(EventX.CHANGE, skeletonNameHandle);
            skeletonFormItem.searckKey = DataSource.BONE;
            skeletonFormItem.visible = ev.isBindSkeleton;
            skeletonFormItem.visible = skeletonToggle.selected;

            offsetFromItem = new EditorVector3("坐标偏移:");
            offsetFromItem.addEventListener(EventX.CHANGE, offsetHandle);
            offsetFromItem.value = ev.offset;

            rotationFromItem = new EditorVector3("坐标旋转:");
            rotationFromItem.addEventListener(EventX.CHANGE, rotationHandle);
            rotationFromItem.value = ev.offRotation;

            useTargetToggle = new EditorRadio("useTarget:");
            useTargetToggle.selected = ev.useTarget;
            useTargetToggle.addEventListener(EventX.CHANGE, useTargetToggleHandle);

            isColliderToggle = new EditorRadio("isCollider:");
            isColliderToggle.selected = ev.isCollider;
            isColliderToggle.addEventListener(EventX.CHANGE, isColliderToggleHandle);


            isUseTargetLayerToggle = new EditorRadio("useTargetLayer:");
            isUseTargetLayerToggle.selected = ev.useTargetLayer;
            isUseTargetLayerToggle.addEventListener(EventX.CHANGE, isUseTargetLayerHandle);

            particlePlaybackSpeedSlider = new EditorSlider("SpeedScale:");
            particlePlaybackSpeedSlider.min = 0.1f;
            particlePlaybackSpeedSlider.max = 5.0f;
            particlePlaybackSpeedSlider.value = ev.particlePlaybackSpeed;
            particlePlaybackSpeedSlider.addEventListener(EventX.CHANGE, particlePlaybackSpeedHandle);


            p.addChild(skeletonToggle);
            p.addChild(bindOnceToggle);
            p.addChild(skeletonFormItem);
            p.addChild(offsetFromItem);
            p.addChild(rotationFromItem);
            p.addChild(useTargetToggle);
            p.addChild(isColliderToggle);
            p.addChild(isUseTargetLayerToggle);
            p.addChild(particlePlaybackSpeedSlider);
        }

        private void particlePlaybackSpeedHandle(EventX e)
        {
            ev.particlePlaybackSpeed = particlePlaybackSpeedSlider.value;
        }

        private void offsetHandle(EventX e)
        {
            ev.offset = offsetFromItem.value;
        }

        private void rotationHandle(EventX e)
        {
            ev.offRotation = rotationFromItem.value;
        }

        private void skeletonToggleHandle(EventX e)
        {
            ev.isBindSkeleton = skeletonToggle.selected;

            if (ev.isBindSkeleton)
            {
                skeletonFormItem.visible = true;
                bindOnceToggle.visible = true;
            }
            else
            {
                skeletonFormItem.visible = false;
                bindOnceToggle.visible = false;
            }
        }

        private void bindOnceToggleHandle(EventX e)
        {
            ev.isBindOnce = bindOnceToggle.selected;
        }

        private void useTargetToggleHandle(EventX e)
        {
            ev.useTarget = useTargetToggle.selected;
        }

        private void isColliderToggleHandle(EventX e)
        {
            ev.isCollider = isColliderToggle.selected;
        }

        private void isUseTargetLayerHandle(EventX e)
        {
            ev.useTargetLayer = isUseTargetLayerToggle.selected;
        }

        

        private void effectHandle(EventX e)
        {
            EditorFormItem eui = e.target as EditorFormItem;
 
            if (e.type == EventX.CHANGE)
            {
                ev.effectPath = eui.value;
            }
        }

        private void skeletonNameHandle(EventX e)
        {
            if (e.type == EventX.CHANGE)
            {
                ev.skeletonName = skeletonFormItem.value;
            }
        }
    }
}