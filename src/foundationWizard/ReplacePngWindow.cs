using foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace foundationEditor
{
    public class ReplacePngWindow : ScriptableWizard
    {

        public Texture2D selectTex;

        public Texture2D replaceTex;

        private Sprite replaceSprite = null;

        public ReplacePngWindow()
        {

        }


        public void findTexture(object instance)
        {
            if (instance == null)
            {
                return;
            }
            Type monoType = instance.GetType();
            FieldInfo[] fieldInfos = monoType.GetFields();
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                if (fieldInfo.FieldType == typeof(Texture))
                {
                    Texture texture = fieldInfo.GetValue(instance) as Texture;
                    doRepleaceTexture(fieldInfo, instance, texture);
                }
                if (fieldInfo.FieldType == typeof(Texture2D))
                {
                    Texture2D texture = fieldInfo.GetValue(instance) as Texture2D;
                    doRepleaceTexture(fieldInfo, instance, texture);
                }

                if (fieldInfo.FieldType == typeof(Sprite))
                {
                    Sprite sprite = fieldInfo.GetValue(instance) as Sprite;
                    doRepleaceSprite(fieldInfo, instance, sprite);
                }

                //object obj = fieldInfo.GetValue(instance);
                //findTexture(obj);
            }

            PropertyInfo[] propertyInfos = monoType.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (propertyInfo.PropertyType == typeof(Texture))
                {
                    Texture texture = propertyInfo.GetValue(instance, null) as Texture;
                    if (texture == selectTex)
                    {
                        propertyInfo.SetValue(instance, replaceTex, null);
                    }
                }
                if (propertyInfo.PropertyType == typeof(Texture2D))
                {
                    Texture2D texture = propertyInfo.GetValue(instance, null) as Texture2D;
                    if (texture == selectTex)
                    {
                        propertyInfo.SetValue(instance, replaceTex, null);
                    }
                }
                if (propertyInfo.PropertyType == typeof(Sprite))
                {
                    Sprite sprite = propertyInfo.GetValue(instance, null) as Sprite;
                    if (sprite != null && sprite.texture == selectTex && replaceSprite != null)
                    {
                        propertyInfo.SetValue(instance, replaceSprite, null);
                    }
                }
            }
        }

        private void doRepleaceSprite(FieldInfo fieldInfo, object instance, Sprite sprite)
        {
            if (sprite != null && sprite.texture == selectTex && replaceSprite != null)
            {
                fieldInfo.SetValue(instance, replaceTex);
            }
        }

        private void doRepleaceTexture(FieldInfo fieldInfo, object instance, Texture texture)
        {
            if (texture == selectTex)
            {
                fieldInfo.SetValue(instance, replaceTex);
            }
        }

        public void OnWizardCreate()
        {
            if (selectTex == null || replaceTex == null)
            {
                ShowNotification(new GUIContent("请确保替selectTex和replaceTex不为null"));
                return;
            }
            string texPath = AssetDatabase.GetAssetPath(selectTex);
            List<string> prefabsUrl = FileHelper.FindFile(Application.dataPath, new string[] {"*.prefab"});
            string replacePath = AssetDatabase.GetAssetPath(replaceTex);
            UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(replacePath);

            foreach (UnityEngine.Object item in sprites)
            {
                if (item is Sprite == false)
                {
                    continue;
                }
                if (item.name == replaceTex.name)
                {
                    replaceSprite = (Sprite) item;
                    break;
                }
            }
            for (int i = 0; i < prefabsUrl.Count; i++)
            {
                string url = prefabsUrl[i];
                url = "Asset" + url.Split(new string[] {"Asset"}, StringSplitOptions.None)[1];
                List<string> depends = AssetDatabase.GetDependencies(new string[] {url}).ToList();
                if (depends.Contains(texPath) == false)
                {
                    continue;
                }
                GameObject dependObj = AssetDatabase.LoadAssetAtPath<GameObject>(url);
                if (dependObj == null) continue;

                MonoBehaviour[] monoBehaviours = dependObj.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (MonoBehaviour behaviour in monoBehaviours)
                {
                    findTexture(behaviour);
                }
                EditorUtility.SetDirty(dependObj);
            }
        }
    }
}
