using System;
using System.Collections.Generic;
using System.Reflection;
using foundation;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace foundationEditor
{
    public class AvatarBoneDrawHelper
    {
        private static MethodInfo GetModelBonesMethod;
        private static MethodInfo DrawSkeletonMethod;
        private static MethodInfo GetHumanBonesMethod;

        private static Type BoneRendererType;
        static AvatarBoneDrawHelper()
        {
            Type type = foundation.ObjectFactory.Locate("UnityEditor.AvatarSetupTool");
            GetHumanBonesMethod = type.GetMethod("GetHumanBones", new Type[] { typeof(SerializedObject), typeof(Dictionary<Transform, bool>) });
            GetModelBonesMethod = type.GetMethod("GetModelBones", BindingFlags.Static | BindingFlags.Public);

            BoneRendererType = foundation.ObjectFactory.Locate("UnityEditor.Handles.BoneRenderer");
            type = foundation.ObjectFactory.Locate("UnityEditor.AvatarSkeletonDrawer");
            DrawSkeletonMethod = type.GetMethod("DrawSkeleton", new Type[]
            {
                typeof(Transform),typeof(Dictionary<Transform, bool>),BoneRendererType
            });
        }

        public static Object NewBoneRenderer()
        {
            return Activator.CreateInstance(BoneRendererType);

        }

        public static Dictionary<Transform, bool> GetModelBones(Transform transform, bool includeAll = true, object o = null)
        {
            try
            {
                object a = GetModelBonesMethod.Invoke(null, new object[] { transform, includeAll, o });
                return (Dictionary<Transform, bool>)a;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("GetModelBones:" + ex.Message);
            }


            return null;
        }

        public static object GetHumanBones(SerializedObject serializedObject, Dictionary<Transform, bool> modelBones)
        {
            try
            {
                object a = GetHumanBonesMethod.Invoke(null, new object[] { serializedObject, modelBones });
                return a;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("GetHumanBones:" + ex.Message);
            }

            return null;
        }

        public static void DrawSkeleton(Transform reference, Dictionary<Transform, bool> actualBones, object renderer)
        {
            if (actualBones != null)
            {
                DrawSkeletonMethod.Invoke(null, new object[] { reference, actualBones, renderer });
            }
        }
    }
}