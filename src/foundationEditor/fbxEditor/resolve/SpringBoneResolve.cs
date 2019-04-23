using System.Collections.Generic;
using UnityEngine;
/*
namespace foundationEditor
{
    public class SpringBoneResolve: AbstractCopyResolve<SpringBone>
    {
        public override void doResolve(SpringBone nc, ComponentRecord fbxComponent)
        {
            PathComponentRecord t;
            SpringBone newBone = (SpringBone)nc;
            SpringBone oldBone = (SpringBone)(fbxComponent.component);
            Transform oldTransform = oldBone.child;
            if (oldTransform != null)
            {
                t = getNewByOld(oldTransform);
                if (t != null)
                {
                    newBone.child = t.go.transform;
                }
            }

            List<SpringCollider> result = new List<SpringCollider>();
            foreach (SpringCollider oldSpringCollider in oldBone.colliders)
            {
                t = getNewByOld(oldSpringCollider.transform);
                if (t == null)
                {
                    continue;
                }

                SpringCollider item = t.go.GetComponent<SpringCollider>();
                if (item != null)
                {
                    result.Add(item);
                }
            }

            if (result.Count > 0)
            {
                newBone.colliders = result.ToArray();
            }
        }
    }
}*/