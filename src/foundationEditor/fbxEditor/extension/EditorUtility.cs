using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace foundationEditor
{
    public class EditorUtilityX
    {
        internal static void InitInstantiatedPreviewRecursive(GameObject go)
        {
            go.hideFlags = HideFlags.HideAndDontSave;
            go.layer = EditorUtils.PreviewCullingLayer;
            IEnumerator enumerator = go.transform.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Transform current = (Transform)enumerator.Current;
                    InitInstantiatedPreviewRecursive(current.gameObject);
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        internal static GameObject InstantiateForAnimatorPreview(UnityEngine.Object original)
        {
            if (original == null)
            {
                throw new ArgumentException("The Prefab you want to instantiate is null.");
            }

            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var propInfo = typeof(UnityEditor.EditorUtility).GetMethod("InstantiateRemoveAllNonAnimationComponents", flags);
            var value=propInfo.Invoke(null, new object[3] {original, Vector3.zero, Quaternion.identity});

            GameObject go = value as GameObject;
            go.name = go.name + "AnimatorPreview";
            go.tag = "Untagged";
            InitInstantiatedPreviewRecursive(go);
            Animator[] componentsInChildren = go.GetComponentsInChildren<Animator>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                Animator animator = componentsInChildren[i];
                animator.enabled = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.logWarnings = false;
                animator.fireEvents = false;
            }
            if (componentsInChildren.Length == 0)
            {
                Animator animator2 = go.AddComponent<Animator>();
                animator2.enabled = false;
                animator2.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator2.logWarnings = false;
                animator2.fireEvents = false;
            }
            return go;
        }

    }
}