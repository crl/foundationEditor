using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace foundationEditor
{
    public static class ExtensionsX
    {
        public static AnimationClip[] GetAnimationClipsFlattenedX(this BlendTree self)
        {
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var propInfo = typeof(BlendTree).GetMethod("GetAnimationClipsFlattened", flags);
            return (AnimationClip[])propInfo.Invoke(self, new object[0]);
        }

        public static void pushUndoX(this AnimatorController self, bool b)
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var propInfo = typeof(AnimatorController).GetProperty("pushUndo", flags);
            propInfo.SetValue(self, b, null);
        }
        public static void pushUndoX(this AnimatorState self, bool b)
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var propInfo = typeof(AnimatorState).GetProperty("pushUndo", flags);
            propInfo.SetValue(self, b, null);
        }

        public static AnimatorController GetEffectiveAnimatorControllerX(this Animator self)
        {
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var propInfo = typeof(AnimatorController).GetMethod("GetEffectiveAnimatorController", flags);
            return (AnimatorController)propInfo.Invoke(self, new object[1] { self });
        }

        public static void pushUndoX(this AnimatorStateMachine self, bool b)
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var propInfo = typeof(AnimatorStateMachine).GetProperty("pushUndo", flags);
            propInfo.SetValue(self, b, null);
        }

        public static Vector3 bodyPositionInternalX(this Animator self)
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var propInfo = typeof(Animator).GetProperty("bodyPositionInternal", flags);
            return (Vector3)propInfo.GetValue(self, new object[0]);
        }

        public static string CalculateBestFittingPreviewGameObjectX(this ModelImporter self)
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var propInfo = typeof(ModelImporter).GetMethod("CalculateBestFittingPreviewGameObject", flags);
            return (string)propInfo.Invoke(self, new object[0]); ;
        }
    }
}