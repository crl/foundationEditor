using System;
using UnityEngine;

namespace foundationEditor
{
    public interface IPrefabCopyResolve
    {
        Type resolveType { get; }
        void init(RecordMaping fbxMap, RecordMaping modelMap);
        void resolve(Component nc, ComponentRecord fbxComponent);
    }
    public abstract class AbstractCopyResolve<T>: IPrefabCopyResolve where T:Component
    {
        private RecordMaping fbxMap = new RecordMaping();
        private RecordMaping modelMap = new RecordMaping();

        public Type resolveType
        {
            get { return typeof(T); }
        }

        public virtual void init(RecordMaping fbxMap, RecordMaping modelMap)
        {
            this.fbxMap = fbxMap;
            this.modelMap = modelMap;
        }

        public void resolve(Component nc, ComponentRecord fbxComponent)
        {
            doResolve((T) nc, fbxComponent);
        }

        public abstract void doResolve(T nc, ComponentRecord fbxComponent);

        protected virtual PathComponentRecord getNewByOld(Transform oldTransform)
        {
            PathComponentRecord t = null;
            PathComponentRecord f = fbxMap.get(oldTransform);
            if (f != null)
            {
                t = modelMap.get(f.path);
            }
            return t;
        }
    }
}