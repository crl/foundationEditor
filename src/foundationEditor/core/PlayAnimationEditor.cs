using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class PlayAnimationEditor
    {
        private float _duration;
        public AnimationClip animationClip;
        public GameObject go;
        private float _runningTime=0;
        public bool isLooping = false;

        public BaseEditorWindow parentEditorWindow;
        private Animator animator;

        private bool hasState = false;
        public void Play(AnimationClip clip, GameObject go,bool isLooping = false)
        {
            this.animationClip = clip;
            this.go = go;
            this.isLooping = isLooping;

            if (this.animationClip.legacy == false)
            {
                animator = go.GetComponentInChildren<Animator>();
                animator.fireEvents = false;
                animator.logWarnings = false;
                animator.enabled = false;
                animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;

                RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;
                hasState = false;
                if (runtimeAnimatorController != null)
                {
                    
                    foreach (AnimationClip item in runtimeAnimatorController.animationClips)
                    {
                        if (item.name == clip.name)
                        {
                            hasState = true;
                        }
                    }
                }
                if (hasState == false)
                {
                    parentEditorWindow.ShowNotification(new GUIContent("animator not exist state:" + clip.name));
                }
            }
            else
            {
                hasState = false;
                animator = null;
            }

            _runningTime = 0;
            _duration = clip.length;
            EditorTickManager.Add(update);
        }

        public void Stop()
        {
            EditorTickManager.Remove(update);
        }

        private void update(float deltaTime)
        {
            if (parentEditorWindow != null)
            {
                parentEditorWindow.DelayCallRepaint();
            }
            _runningTime += deltaTime;

            if (animationClip == null || go==null)
            {
                Stop();
                return;
            }

            if (animator != null)
            {
                if (_runningTime > _duration)
                {
                    if (isLooping == false && animationClip.isLooping == false)
                    {
                        animator.Play(animationClip.name, 0, 1.0f);
                        animator.Update(deltaTime);
                        EditorTickManager.Remove(update);
                        return;
                    }
                    _runningTime = 0;
                }
                if (hasState)
                {
                    animator.Play(animationClip.name, 0, _runningTime / _duration);
                    animator.Update(deltaTime);
                }
                return;
            }

            animationClip.SampleAnimation(go, _runningTime);
            if (_runningTime > _duration)
            {
                _runningTime = 0;
                if (animationClip.isLooping == false && isLooping==false)
                {
                    animationClip.SampleAnimation(go, _runningTime);
                    EditorTickManager.Remove(update);
                }
                
            }
        }
    }
}