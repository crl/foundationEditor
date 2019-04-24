namespace foundationEditor
{
    using UnityEngine;

    internal abstract class ObjectSelectorReceiver : ScriptableObject
    {
        protected ObjectSelectorReceiver()
        {
        }

        public abstract void OnSelectionChanged(UnityEngine.Object selection);
        public abstract void OnSelectionClosed(UnityEngine.Object selection);
    }
}

