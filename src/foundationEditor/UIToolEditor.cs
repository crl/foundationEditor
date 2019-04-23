using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using foundation;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using foundation.monoExt;

namespace foundationEditor
{
    public static class UIToolEditor
    {
        [MenuItem("Tools/UITool/FindAddress &a")]
        private static void DebugAddress()
        {
            GameObject[] gamesObjects = Selection.gameObjects;
            for (int i = 0; i < gamesObjects.Length; i++)
            {
                string uri = "";
                Transform temp = gamesObjects[i].transform;
                while (temp != null)
                {
                    uri = temp.name + "/" + uri;
                    temp = temp.parent;
                }
                Debug.Log(uri);
            }
        }

        [MenuItem("Assets/SpritePackingAtlas")]
        public static void CreateUITextureAtlas()
        {
            string[] guids = Selection.assetGUIDs;
            for (int i = 0; i < guids.Length; i++)
            {
                string file = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (file.Contains("."))
                {
                    continue;
                }
                file = file.Replace("Assets", "");
                TexFileHandle(Application.dataPath + file);
            }
        }

        private static void TexFileHandle(string file)
        {
            List<string> texs = FileHelper.FindFile(file, new string[] {"*.png"}, SearchOption.TopDirectoryOnly);
            string[] temps = file.Split('/');
            string tag = temps[temps.Length - 1] + "Atlas";
            for (int i = 0; i < texs.Count; i++)
            {
                string url = texs[i].Split(new string[] {"Assets"}, StringSplitOptions.None)[1];
                TextureImporter textureImporter = AssetImporter.GetAtPath("Assets" + url) as TextureImporter;
                if (textureImporter != null)
                {
                    bool isChange = false;
                    if (textureImporter.spritePackingTag != tag)
                    {
                        textureImporter.spritePackingTag = tag;
                        isChange = true;
                    }
                    TextureImporterPlatformSettings textureImporterPlatformSettings= textureImporter.GetPlatformTextureSettings("Android");
                    
                    ///强制可读写的,不能合;
                    bool canOverride = (textureImporter.isReadable == false);
                    // 压缩的格式，android下修改为分离alpha通道的etc1  
                    if (canOverride && textureImporterPlatformSettings.format == TextureImporterFormat.Automatic)
                    {
                        if (textureImporterPlatformSettings.maxTextureSize != 2048)
                        {
                            isChange = true;
                            textureImporterPlatformSettings.maxTextureSize = 2048;
                        }

                        textureImporterPlatformSettings.format = TextureImporterFormat.ETC_RGB4;
                        if (textureImporterPlatformSettings.allowsAlphaSplitting == false)
                        {
                            isChange = true;
                            textureImporterPlatformSettings.allowsAlphaSplitting = true;
                        }

                        if (textureImporterPlatformSettings.overridden == false)
                        {
                            isChange = true;
                            textureImporterPlatformSettings.overridden = true;
                        }

                        if (isChange)
                        {
                            textureImporter.SetPlatformTextureSettings(textureImporterPlatformSettings);
                        }
                    }

                    if (isChange == true)
                    {
                        textureImporter.SaveAndReimport();
                    }
                }
            }
        }

        [MenuItem("Assets/FormatUITransform")]
        private static void FormatUITransform()
        {
            GameObject[] gos = Selection.gameObjects;

            for (int i = 0; i < gos.Length; i++)
            {
                if (gos[i] == null)
                {
                    break;
                }
                RectTransform[] trans = gos[i].GetComponentsInChildren<RectTransform>(true);
                for (int j = 0; j < trans.Length; j++)
                {
                    RectTransform t = trans[j];
                    RectFormartHandle(t);
                }
                EditorUtility.SetDirty(gos[i]);
            }
            
        }

        private static void RectFormartHandle(RectTransform t)
        {
            Undo.RecordObject(t, "chage RectTransform");
            
            Vector2 size = t.sizeDelta;
            size.x = Mathf.Round(size.x);
            size.y = Mathf.Round(size.y);
            t.sizeDelta = size;

            Vector3 pos = t.anchoredPosition;
            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);
            if (pos.x % 2 == 1 || pos.x % 2 == -1)
            {
                pos.x++;
            }
            if (pos.y % 2 == 1|| pos.y % 2 == -1)
            {
                pos.y++;
            }
            t.anchoredPosition = pos;
            
            Vector3 rota = t.localRotation.eulerAngles;
            rota.x = Mathf.Round(rota.x);
            rota.y = Mathf.Round(rota.y);
            rota.z = Mathf.Round(rota.z);
            t.localRotation = Quaternion.Euler(rota);

            Vector3 scale = t.localScale;

            for (int i = 0; i < 100; i++)
            {
                if (scale.x > i-0.02f && scale.x < i+0.02f)
                {
                    scale.x = i;
                    break;
                }
            }
            for (int i = 0; i < 100; i++)
            {
                if (scale.y > i - 0.02f && scale.y < i + 0.02f)
                {
                    scale.y = i;
                    break;
                }
            }
            for (int i = 0; i < 100; i++)
            {
                if (scale.z > i - 0.02f && scale.z < i + 0.02f)
                {
                    scale.z = i;
                    break;
                }
            }
            t.localScale = scale;
        }

        

        [MenuItem("Tools/UITool/FindUselessPng")]
        public static void getObjectsInPath()
        {
            ScriptableWizard.DisplayWizard("请配置资源路径", typeof (CheckAssetWindow), "确定");
        }

        [MenuItem("Tools/UITool/替换贴图")]
        [MenuItem("Assets/替换贴图")]
        public static void OpenReplacePngWindow()
        {
            ReplacePngWindow windows = ScriptableWizard.DisplayWizard("请选择替换png", typeof(ReplacePngWindow), "确定") as ReplacePngWindow;
            object[] objs = Selection.objects;
            if (objs.Length != 1) return;
            Texture2D tex = objs[0] as Texture2D;
            windows.selectTex = tex;
        }


        [MenuItem("Assets/FindUITextureDependObj", true)]
        public static bool FindTextureDependObjCheck()
        {
            object[] objs = Selection.objects;
            if (objs.Length != 1) return false;
            Texture2D tex = objs[0] as Texture2D;
            if (tex == null) return false;
            return true;
        }

        [MenuItem("Assets/FindUITextureDependObj")]
        public static void FindTextureDependObj()
        {
            object[] objs = Selection.objects;
            if (objs.Length != 1) return;
            Texture2D tex = objs[0] as Texture2D;
            if (tex == null) return;
            string texPath = AssetDatabase.GetAssetPath(tex);
            List<string> prefabsUrl = FileHelper.FindFile(Application.dataPath, new string[] {"*.prefab"});
            int useCount = 0;
            for (int i = 0; i < prefabsUrl.Count; i++)
            {
                string url = prefabsUrl[i];
                url = "Asset" + url.Split(new string[] {"Asset"}, StringSplitOptions.None)[1];
                List<string> depends = AssetDatabase.GetDependencies(new string[] { url }).ToList();
                if (depends.Contains(texPath) == false)
                {
                    continue;
                }
                GameObject dependObj = AssetDatabase.LoadAssetAtPath<GameObject>(url);
                if (dependObj == null) continue;
                Image[] images = dependObj.GetComponentsInChildren<Image>(true);
                RawImage[] rawImages = dependObj.GetComponentsInChildren<RawImage>(true);

                for (int j = 0; j < images.Length; j++)
                {
                    if (images[j].sprite == null) continue;
                    if (images[j].sprite.texture == tex)
                    {
                        string uri = "";
                        Transform temp = images[j].transform;
                        while (temp != null)
                        {
                            uri = temp.name + "/" + uri;
                            temp = temp.parent;
                        }
                        Debug.Log("使用物体：" + uri, dependObj);
                        useCount++;
                    }
                }
                for (int j = 0; j < rawImages.Length; j++)
                {
                    if (rawImages[j].texture == null) continue;
                    if (rawImages[j].texture == tex)
                    {
                        string uri = "";
                        Transform temp = rawImages[j].transform;
                        while (temp != null)
                        {
                            uri = temp.name + "/" + uri;
                            temp = temp.parent;
                        }
                        Debug.Log("使用物体：" + uri, dependObj);
                        useCount++;
                    }
                }
            }
            if (useCount == 0)
            {
                Debug.Log("无使用 UI prefab");
            }
        }


        [MenuItem("GameObject/UI/Raw Image(noRaycast)")]
        static void CreatRawImage()
        {
            if (Selection.activeTransform)
            {
                if (Selection.activeTransform.GetComponentInParent<Canvas>())
                {
                    RawImage rawImage = UIUtils.CreateRawImage("RawImage", Selection.activeGameObject);
                    rawImage.raycastTarget = false;
                    Selection.activeGameObject = rawImage.gameObject;
                }
            }
        }

        [MenuItem("GameObject/UI/Text(noRaycast)")]
        static void CreatText()
        {
            if (Selection.activeTransform)
            {
                if (Selection.activeTransform.GetComponentInParent<Canvas>())
                {
                    Text text = UIUtils.CreateText("Text", 24, Selection.activeGameObject);
                    text.raycastTarget = false;
                    text.text = "New Text";
                    string url = Application.dataPath + "/Fonts/default.txt";
                    string fontName = FileHelper.GetUTF8Text(url);

                    text.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 30);
                   // GameObject fontText = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Fonts/fontModel.prefab");
                    if (fontName != "")
                    {
                        Font t = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/" + fontName + ".ttf");
                        if (t != null)
                        {
                            text.font = t;
                        }
                    }
                    Selection.activeGameObject = text.gameObject;
                }
            }
        }

//        [MenuItem("GameObject/UI/UIPrefabSlot")]
//        static void CreatUIPrefabSlot()
//        {
//            if (Selection.activeTransform)
//            {
//                if (Selection.activeTransform.GetComponentInParent<Canvas>())
//                {
//                    GameObject go = UIUtils.CreateEmpty("UIPrefabSlot",Selection.activeGameObject);
//                    Selection.activeGameObject = go;
//                    go.AddComponent<UIPrefabSlot>();
//                }
//            }
//        }

        [MenuItem("GameObject/UI/FocusGameObject")]
        static void FocusGameObject()
        {
            if (Selection.activeTransform)
            {
                if (Selection.activeTransform.GetComponentInParent<Canvas>())
                {
                    GameObject go = Selection.activeGameObject;
                    go.GetOrAddComponent<FocusGameObject>();
                }
            }
        }

    }
}