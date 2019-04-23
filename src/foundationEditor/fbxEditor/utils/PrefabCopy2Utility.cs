using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class PrefabCopy2Utility
    {
        private RecordMaping fbxDictionary = new RecordMaping();
        private RecordMaping modelDictionary =new RecordMaping();

        private static Dictionary<Type, IPrefabCopyResolve> Resolves=new Dictionary<Type, IPrefabCopyResolve>();

        static PrefabCopy2Utility()
        {
            //register<SpringBoneResolve>();
            //register<SpringManagerResolve>();
        }

        public static void register<T>() where T: IPrefabCopyResolve,new()
        {
            IPrefabCopyResolve prefabCopyResolve = new T();
            Type t = prefabCopyResolve.resolveType;
            Resolves.Add(t, prefabCopyResolve);

        }
       
        public GameObject replace(GameObject fbxPrefab, GameObject fbxRawModel)
        {
            GameObject prefabInstance=GameObject.Instantiate(fbxPrefab);
            prefabInstance.name = fbxPrefab.name;
            prefabInstance.SetActive(false);
            prefabInstance.hideFlags = HideFlags.HideAndDontSave;

            PathComponentRecord fbxRecord= this.recordListFromHierarchy(prefabInstance, fbxDictionary);

           var path= AssetDatabase.GetAssetPath(fbxPrefab);
            fbxPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(fbxRawModel, path, InteractionMode.AutomatedAction);
            PathComponentRecord modelRecord = this.recordListFromHierarchy(fbxPrefab, modelDictionary);

            ///先添加组件及基本信息
            doReplace(fbxRecord, modelRecord, false);
           ///绑定引用类型
            doReplace(fbxRecord, modelRecord,true);

            GameObject.DestroyImmediate(prefabInstance);

            /*SpringManager sm= fbxPrefab.GetComponent<SpringManager>();
            if (sm != null)
            {
                sm.OnValidate();
            }*/

            return fbxPrefab;
        }

        private void doReplace(PathComponentRecord fbxRecord, PathComponentRecord modelRecord,bool isResolve)
        {
            foreach (ComponentRecord fbxComponent in fbxRecord.components)
            {
                Type type = fbxComponent.type;
                if (type == null)
                {
                    continue;
                }

                Component nc= modelRecord.go.GetComponent(type);
                if (nc == null)
                {
                    nc= modelRecord.go.AddComponent(type);
                }

                if (nc != null)
                {
                    if (isResolve)
                    {
                        IPrefabCopyResolve resolver;
                        if (Resolves.TryGetValue(nc.GetType(), out resolver))
                        {
                            resolver.init(fbxDictionary, modelDictionary);
                            resolver.resolve(nc, fbxComponent);
                        }
                    }
                    else
                    {
                        EditorJsonUtility.FromJsonOverwrite(fbxComponent.json, nc);
                    }
                }
            }

            foreach (PathComponentRecord fbxRecordChild in fbxRecord.children)
            {
                foreach (PathComponentRecord modelRecordChild in modelRecord.children)
                {
                    if (modelRecordChild.name == fbxRecordChild.name)
                    {
                        doReplace(fbxRecordChild, modelRecordChild, isResolve);
                        break;
                    }
                }
            }
        }

        private PathComponentRecord recordListFromHierarchy(GameObject gameObject, RecordMaping recordMaping,string parentPath="")
        {
            PathComponentRecord result = new PathComponentRecord();

            List<ComponentRecord> hierarchy = new List<ComponentRecord>();
            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component is Transform || component is Renderer)
                {
                    continue;
                }

                ComponentRecord record=new ComponentRecord();
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
                    this.recordListFromHierarchy(gameObject.transform.GetChild(i).gameObject,recordMaping,result.path);

                result.children.Add(item);
            }

            return result;
        }


    }
}