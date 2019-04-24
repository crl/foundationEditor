using UnityEngine;

namespace foundationEditor
{
    public class PreviewRenderUtilityX
    {
        internal static void SetEnabledRecursive(GameObject go, bool enabled)
        {
            foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = enabled;
            }
        }

    }
}