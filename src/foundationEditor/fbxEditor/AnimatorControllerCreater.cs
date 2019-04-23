using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace foundationEditor
{
    public class AnimatorControllerCreater
    {
        [MenuItem("Tools/CreateAnimatorController")]
        static void CreateAnimationAssets(MenuCommand cmd)
        {
            UnityEngine.Object obj = Selection.activeObject;
            if (obj == null)
            {
                Debug.Log("选择的对像不存在");
                return;
            }
            string filePath = AssetDatabase.GetAssetPath(obj);

            string rootFolder = Path.GetDirectoryName(filePath);
            string[] files = Directory.GetFiles(rootFolder, "*.fbx");

            if (files.Length == 0)
            {
                Debug.Log("选择的对像文件夹下不存在fbx文件");
                return;
            }
            string controllerFullPath = string.Format("{0}/default.controller", rootFolder);
            AnimatorController animatorController =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerFullPath);
            if (animatorController == null)
            {
                animatorController = AnimatorController.CreateAnimatorControllerAtPath(controllerFullPath);
            }
            if (animatorController.layers.Length == 0)
            {
                animatorController.AddLayer("Base Layer");
            }
            AnimatorControllerLayer animatorControllerLayer = animatorController.layers[0];
            AnimatorState defaultState = animatorControllerLayer.stateMachine.defaultState;

            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (name.IndexOf("@") == -1)
                {
                    continue;
                }
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(file);
                if (clip == null)
                {
                    continue;
                }
                // 绑定动画文件
                addNoExistState(animatorControllerLayer, clip, true);
            }
        }

        public static AnimatorState addNoExistState(AnimatorControllerLayer layer, AnimationClip newClip, bool autoCreate = true)
        {
            if (newClip == null || layer == null)
            {
                return null;
            }

            UnityEditor.Animations.AnimatorStateMachine sm = layer.stateMachine;
            AnimatorState state = getExistState(sm, newClip.name);
            if (state == null)
            {
                state = sm.AddState(newClip.name);
            }
            state.motion = newClip;
            return state;
        }

        private static AnimatorState getExistState(UnityEditor.Animations.AnimatorStateMachine stateMachine, string name)
        {
            AnimatorState state = null;
            foreach (ChildAnimatorState animatorState in stateMachine.states)
            {
                if (animatorState.state.name == name)
                {
                    state = animatorState.state;
                    break;
                }
            }
            if (state == null)
            {
                foreach (ChildAnimatorStateMachine childAnimatorStateMachine in stateMachine.stateMachines)
                {
                    state = getExistState(childAnimatorStateMachine.stateMachine, name);
                    break;
                }
            }
            return state;
        }
    }
}