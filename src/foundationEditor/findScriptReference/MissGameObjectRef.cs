using System.Collections.Generic;
using UnityEngine;

namespace foundationEditor
{
    public class MissGameObjectRef
    {
        public GameObject refGo
        {
            get;
            private set;
        }
        public List<MissGameObjectDes> missGameobjectDes = new List<MissGameObjectDes>();
        public List<MissComponentDes> missComponentRefs = new List<MissComponentDes>();
        public List<MissComponentDes> componentRefs = new List<MissComponentDes>();

        public MissGameObjectRef(GameObject go)
        {
            this.refGo = go;
        }

        public void AddComponent(GameObject go, string v)
        {
            MissGameObjectDes i = new MissGameObjectDes();
            i.go = go;
            i.des = v;
            missGameobjectDes.Add(i);
        }

        public void AddPropertys(Component go, string v)
        {
            MissComponentDes i = new MissComponentDes();
            i.go = go;
            i.des = v;
            missComponentRefs.Add(i);
        }

        public void AddRefPropertys(Component go, string v)
        {
            MissComponentDes i = new MissComponentDes();
            i.go = go;
            i.des = v;
            componentRefs.Add(i);
        }
    }

    public class MissGameObjectDes
    {
        public GameObject go;
        public string des;
    }

    public class MissComponentDes
    {
        public Component go;
        public string des;
    }
}