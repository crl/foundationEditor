namespace foundationEditor
{
    /*public class SpringManagerResolve : AbstractCopyResolve<SpringManager>
    {
        public override void doResolve(SpringManager nc, ComponentRecord fbxComponent)
        {
            PathComponentRecord t;
            SpringManager newBone = (SpringManager)nc;
            SpringManager oldBone = (SpringManager)(fbxComponent.component);
            List<SpringCollider> result = new List<SpringCollider>();
            foreach (SpringCollider oldSpringCollider in oldBone.commonColliders)
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
                newBone.commonColliders = result.ToArray();
            }
        }
    }*/
}