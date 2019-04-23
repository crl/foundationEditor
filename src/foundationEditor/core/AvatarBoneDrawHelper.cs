using System;
using System.Collections.Generic;
using System.Reflection;
using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class AvatarBoneDrawHelper
    {
        private static MethodInfo GetModelBonesMethod;
        private static MethodInfo DrawSkeletonMethod;
        private static MethodInfo GetHumanBonesMethod;
        static AvatarBoneDrawHelper()
        {
            Type AvatarSetupTool = foundation.ObjectFactory.Locate("UnityEditor.AvatarSetupTool");
            GetHumanBonesMethod = AvatarSetupTool.GetMethod("GetHumanBones",
                BindingFlags.Static | BindingFlags.Public);

            GetModelBonesMethod = AvatarSetupTool.GetMethod("GetModelBones",
            BindingFlags.Static | BindingFlags.Public);


            Type AvatarSkeletonDrawer = foundation.ObjectFactory.Locate("UnityEditor.AvatarSkeletonDrawer");
            DrawSkeletonMethod = AvatarSkeletonDrawer.GetMethod("DrawSkeleton",new Type[]
            {
                typeof(Transform),typeof(Dictionary<Transform, bool>)
            });
        }

        public static Dictionary<Transform,bool> GetModelBones(Transform transform, bool includeAll=true,object o=null)
        {
            try
            {
                object a = GetModelBonesMethod.Invoke(null, new object[] { transform, includeAll, o });
                return (Dictionary<Transform, bool>)a;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("GetModelBones:"+ex.Message);
            }


            return null;
        }

        public static object GetHumanBones(SerializedObject serializedObject, Dictionary<Transform, bool> modelBones)
        {
            try
            {
                object a = GetHumanBonesMethod.Invoke(null, new object[] {serializedObject, modelBones});
                return a;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("GetHumanBones:" + ex.Message);
            }

            return null;
        }

        public static void DrawSkeleton(Transform reference, Dictionary<Transform, bool> actualBones)
        {
            if (actualBones != null)
            {
                DrawSkeletonMethod.Invoke(null, new object[] {reference, actualBones});
            }
        }
    }
}