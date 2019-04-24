using System;
using System.Collections;
using UnityEngine;

namespace foundationEditor
{
    public class GameObjectInspector
    {
        private static float GetRenderableCenterRecurse(ref Vector3 center, GameObject go, int depth, int minDepth, int maxDepth)
        {
            if (depth > maxDepth)
            {
                return 0f;
            }
            float num2 = 0f;
            if (depth > minDepth)
            {
                MeshRenderer component = go.GetComponent<MeshRenderer>();
                MeshFilter filter = go.GetComponent<MeshFilter>();
                SkinnedMeshRenderer renderer2 = go.GetComponent<SkinnedMeshRenderer>();
                SpriteRenderer renderer3 = go.GetComponent<SpriteRenderer>();
                BillboardRenderer renderer4 = go.GetComponent<BillboardRenderer>();
                if ((((component == null) && (filter == null)) && ((renderer2 == null) && (renderer3 == null))) && (renderer4 == null))
                {
                    num2 = 1f;
                    center += go.transform.position;
                }
                else if ((component != null) && (filter != null))
                {
                    if (Vector3.Distance(component.bounds.center, go.transform.position) < 0.01f)
                    {
                        num2 = 1f;
                        center += go.transform.position;
                    }
                }
                else if (renderer2 != null)
                {
                    if (Vector3.Distance(renderer2.bounds.center, go.transform.position) < 0.01f)
                    {
                        num2 = 1f;
                        center += go.transform.position;
                    }
                }
                else if (renderer3 != null)
                {
                    if (Vector3.Distance(renderer3.bounds.center, go.transform.position) < 0.01f)
                    {
                        num2 = 1f;
                        center += go.transform.position;
                    }
                }
                else if ((renderer4 != null) && (Vector3.Distance(renderer4.bounds.center, go.transform.position) < 0.01f))
                {
                    num2 = 1f;
                    center += go.transform.position;
                }
            }
            depth++;
            IEnumerator enumerator = go.transform.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Transform current = (Transform)enumerator.Current;
                    num2 += GetRenderableCenterRecurse(ref center, current.gameObject, depth, minDepth, maxDepth);
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
            return num2;
        }

        public static Vector3 GetRenderableCenterRecurse(GameObject go, int minDepth, int maxDepth)
        {
            Vector3 zero = Vector3.zero;
            float num = GetRenderableCenterRecurse(ref zero, go, 0, minDepth, maxDepth);
            if (num > 0f)
            {
                return (Vector3)(zero / num);
            }
            return go.transform.position;
        }

        public static void GetRenderableBoundsRecurse(ref Bounds bounds, GameObject go)
        {
            MeshRenderer component = go.GetComponent<MeshRenderer>();
            MeshFilter filter = go.GetComponent<MeshFilter>();
            if (((component != null) && (filter != null)) && (filter.sharedMesh != null))
            {
                if (bounds.extents == Vector3.zero)
                {
                    bounds = component.bounds;
                }
                else
                {
                    bounds.Encapsulate(component.bounds);
                }
            }
            SkinnedMeshRenderer renderer2 = go.GetComponent<SkinnedMeshRenderer>();
            if ((renderer2 != null) && (renderer2.sharedMesh != null))
            {
                if (bounds.extents == Vector3.zero)
                {
                    bounds = renderer2.bounds;
                }
                else
                {
                    bounds.Encapsulate(renderer2.bounds);
                }
            }
            SpriteRenderer renderer3 = go.GetComponent<SpriteRenderer>();
            if ((renderer3 != null) && (renderer3.sprite != null))
            {
                if (bounds.extents == Vector3.zero)
                {
                    bounds = renderer3.bounds;
                }
                else
                {
                    bounds.Encapsulate(renderer3.bounds);
                }
            }
            BillboardRenderer renderer4 = go.GetComponent<BillboardRenderer>();
            if (((renderer4 != null) && (renderer4.billboard != null)) && (renderer4.sharedMaterial != null))
            {
                if (bounds.extents == Vector3.zero)
                {
                    bounds = renderer4.bounds;
                }
                else
                {
                    bounds.Encapsulate(renderer4.bounds);
                }
            }
            IEnumerator enumerator = go.transform.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Transform current = (Transform)enumerator.Current;
                    GetRenderableBoundsRecurse(ref bounds, current.gameObject);
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


        public static bool HasRenderableParts(GameObject go)
        {
            MeshRenderer[] componentsInChildren = go.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in componentsInChildren)
            {
                MeshFilter component = renderer.gameObject.GetComponent<MeshFilter>();
                if ((component != null) && (component.sharedMesh != null))
                {
                    return true;
                }
            }
            SkinnedMeshRenderer[] rendererArray3 = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer2 in rendererArray3)
            {
                if (renderer2.sharedMesh != null)
                {
                    return true;
                }
            }
            SpriteRenderer[] rendererArray5 = go.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer renderer3 in rendererArray5)
            {
                if (renderer3.sprite != null)
                {
                    return true;
                }
            }
            BillboardRenderer[] rendererArray7 = go.GetComponentsInChildren<BillboardRenderer>();
            foreach (BillboardRenderer renderer4 in rendererArray7)
            {
                if ((renderer4.billboard != null) && (renderer4.sharedMaterial != null))
                {
                    return true;
                }
            }
            return false;
        }

    }
}