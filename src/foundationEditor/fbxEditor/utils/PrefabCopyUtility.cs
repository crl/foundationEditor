using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 暂时没有用处
/// </summary>
namespace foundationEditor
{
    public class PrefabCopyUtility
    {
        public bool isDirty
        {
            get;
            private set;
        }

        private RecordMaping fbxDictionary = new RecordMaping();
        private RecordMaping modelDictionary = new RecordMaping();
        public GameObject replace(GameObject fbxPrefab, GameObject fbxRawModel)
        {
            PathComponentRecord prefabHierarchyRecord = recordListFromHierarchy(fbxPrefab, fbxDictionary);
            PathComponentRecord modelHierarchyRecord = recordListFromHierarchy(fbxRawModel, modelDictionary);
            bindComponentHierarchy(prefabHierarchyRecord, modelHierarchyRecord);

            if (isDirty)
            {
                SkinnedMeshRenderer[] prefabSkinnedMeshRenderers =
                    fbxPrefab.GetComponentsInChildren<SkinnedMeshRenderer>();
                SkinnedMeshRenderer[] modelSkinnedMeshRenderers =
                    fbxRawModel.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer modelSkinnedMeshRenderer in modelSkinnedMeshRenderers)
                {
                    foreach (SkinnedMeshRenderer prefabSkinnedMeshRenderer in prefabSkinnedMeshRenderers)
                    {
                        if (prefabSkinnedMeshRenderer.name == modelSkinnedMeshRenderer.name)
                        {
                            bindBone(prefabSkinnedMeshRenderer, modelSkinnedMeshRenderer);
                        }
                    }
                }
            }

            return fbxPrefab;
        }

        private void bindBone(SkinnedMeshRenderer prefab, SkinnedMeshRenderer model)
        {
            PathComponentRecord fromRecord;
            PathComponentRecord toRecord;
            List<Transform> bones=new List<Transform>();
            foreach (Transform modelChild in model.bones)
            {
                fromRecord = modelDictionary.get(modelChild);
                toRecord = fbxDictionary.get(fromRecord.path);
                bones.Add(toRecord.go.transform);
            }


            if (bones.Count > 0)
            {
                prefab.bones = bones.ToArray();
            }

            fromRecord = modelDictionary.get(model.rootBone);
            toRecord = fbxDictionary.get(fromRecord.path);
            prefab.rootBone = toRecord.go.transform;

        }

        private void bindComponentHierarchy(PathComponentRecord prefab, PathComponentRecord model)
        {
            bool has = false;
            foreach (PathComponentRecord modelChild in model.children)
            {
                has = false;
                foreach (PathComponentRecord prefabChild in prefab.children)
                {
                    if (prefabChild.name == modelChild.name)
                    {
                        has = true;
                        bindComponentHierarchy(prefabChild, modelChild);
                        break;
                    }
                }

                if (has == false)
                {
                    isDirty = true;
                    prefab.createChild(modelChild);
                }
            }
        }


        private PathComponentRecord recordListFromHierarchy(GameObject gameObject, RecordMaping recordMaping, string parentPath = "")
        {
            PathComponentRecord result = new PathComponentRecord();

            List<ComponentRecord> hierarchy = new List<ComponentRecord>();
            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component is Transform || component is Renderer)
                {
                    continue;
                }

                ComponentRecord record = new ComponentRecord();
                record.type = component.GetType();
                record.component = component;
                record.json = EditorJsonUtility.ToJson(component);
                hierarchy.Add(record);
            }

            result.rootMap = recordMaping;
            result.name = gameObject.name;
            result.path = parentPath + "/" + result.name;
            result.go = gameObject;
            result.components = hierarchy;

            result.localPosition = gameObject.transform.localPosition;
            result.localRotation = gameObject.transform.localRotation;
            result.localScale = gameObject.transform.localScale;

            recordMaping.add(result);

            int childCount = gameObject.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                PathComponentRecord item =
                    this.recordListFromHierarchy(gameObject.transform.GetChild(i).gameObject, recordMaping, result.path);

                result.children.Add(item);
            }

            return result;
        }

    }
}