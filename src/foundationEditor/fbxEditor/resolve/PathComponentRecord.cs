using System;
using System.Collections.Generic;
using UnityEngine;

namespace foundationEditor
{
    public class ComponentRecord
    {
        public Type type;
        public Component component;
        public string json;
    }
    public class PathComponentRecord
    {
        public string name;

        public GameObject go;

        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public List<ComponentRecord> components = new List<ComponentRecord>();
        public List<PathComponentRecord> children = new List<PathComponentRecord>();
        internal string path;
        internal RecordMaping rootMap;

        public void createChild(PathComponentRecord modelChild)
        {
            GameObject gameObject = GameObject.Instantiate(modelChild.go, go.transform, false);
            gameObject.name = modelChild.go.name;
            Transform newTransform = gameObject.transform;
            newTransform.localPosition = modelChild.localPosition;
            newTransform.localRotation = modelChild.localRotation;
            newTransform.localScale = modelChild.localScale;
        }
    }

    public class RecordMaping
    {
        private Dictionary<Transform, PathComponentRecord> transMap = new Dictionary<Transform, PathComponentRecord>();
        private Dictionary<string, PathComponentRecord> pathMap = new Dictionary<string, PathComponentRecord>();

        public void add(PathComponentRecord record)
        {
            transMap.Add(record.go.transform, record);
            pathMap.Add(record.path, record);
        }

        public PathComponentRecord get(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            PathComponentRecord result;
            pathMap.TryGetValue(path, out result);
            return result;
        }

        public PathComponentRecord get(Transform transform)
        {
            if (transform == null)
            {
                return null;
            }
            PathComponentRecord result;
            transMap.TryGetValue(transform, out result);
            return result;
        }
    }
}