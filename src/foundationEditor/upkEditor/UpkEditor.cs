using foundation;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class UpkEditor:BaseEditorWindow
    {
        public static string UPK_PREFIX = "Assets/Prefabs/assetBundle/UI/Upk/";

        [MenuItem("App/UpkImport", false, 33)]
        public static void TSkillEditor()
        {
            GetWindow<UpkEditor>();
        }

        protected override void initialization()
        {
            base.initialization();

            this.titleContent = new GUIContent("Upk");

            this.maxSize = new Vector2(200, 300);
            EditorConfigUtils.load();
            EditorButton btn = new EditorButton("选择");
            btn.addEventListener(EventX.ITEM_CLICK, start);
            this.addChild(btn);


            btn = new EditorButton("选择2(深度遍历子文件夹)");
            btn.addEventListener(EventX.ITEM_CLICK, start2);
            this.addChild(btn);
        }


        private void start(EventX e)
        {
            string upkEditorWorkspace = EditorConfigUtils.GetProjectResource("upk/");
            if (Directory.Exists(upkEditorWorkspace) == false)
            {
                EditorUtility.DisplayDialog("error", "path not found:" + upkEditorWorkspace, "ok");
                return;
            }
            string selectedPath = EditorUtility.OpenFolderPanel("选择upk文件夹", upkEditorWorkspace, "");
            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }
            doSingle(selectedPath);
        }

        private void start2(EventX e)
        {
            string upkEditorWorkspace = EditorConfigUtils.GetProjectResource("upk/");
            if (Directory.Exists(upkEditorWorkspace) == false)
            {
                EditorUtility.DisplayDialog("error", "path not found:" + upkEditorWorkspace, "ok");
                return;
            }
            string selectedPath = EditorUtility.OpenFolderPanel("选择upk文件夹", upkEditorWorkspace, "");
            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(selectedPath);
            DirectoryInfo[] list = directoryInfo.GetDirectories();

            foreach (DirectoryInfo info in list)
            {
                doSingle(info.FullName, false);
            }

            AssetDatabase.Refresh();
        }

        private void doSingle(string selectedPath,bool updateIt=true)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(selectedPath);
            string directoryName = directoryInfo.Name;
            string[] files = Directory.GetFiles(selectedPath);

            if (files.Length < 1)
            {
                return;
            }
            byte[] bytes;
            List<string> nameList = new List<string>();
            List<Texture2D> texture2DList = new List<Texture2D>();
            foreach (string file in files)
            {
                bytes = FileHelper.GetBytes(file);
                Texture2D texture2D = new Texture2D(1, 1);
                string fileName = Path.GetFileNameWithoutExtension(file);
                texture2D.name = fileName;
                texture2D.LoadImage(bytes);
                texture2DList.Add(texture2D);

                nameList.Add(fileName);
            }
            Texture2D[] textures = texture2DList.ToArray();
            Texture2D temp = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            Rect[] rect = null;
            try
            {
                rect = temp.PackTextures(textures, 1, 2048);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                return;
            }

            List<SpriteMetaData> spriteMetaDatas = new List<SpriteMetaData>();
            int len = texture2DList.Count;
            for (int i = 0; i < len; i++)
            {
                Rect r = rect[i];
                r.x *= temp.width;
                r.y *= temp.height;
                r.width *= temp.width;
                r.height *= temp.height;
                SpriteMetaData metaData = creatSpriteMetaData(r, textures[i].name);
                spriteMetaDatas.Add(metaData);
            }
            byte[] pngBytes = temp.EncodeToPNG();
            string newPath = UPK_PREFIX + directoryName + ".png";
            FileHelper.SaveBytes(pngBytes, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            TextureImporter texImp = AssetImporter.GetAtPath(newPath) as TextureImporter;
            texImp.isReadable = true;
            if (texImp != null)
            {
                processToSprite(texImp, spriteMetaDatas);
            }
            texImp.SaveAndReimport();
            UpkAniVO unAniVo = ScriptableObject.CreateInstance<UpkAniVO>();

            List<SpriteInfoVO> list = new List<SpriteInfoVO>();
            UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetsAtPath(newPath);
            Sprite item;
            for (int i = 0; i < objects.Length; i++)
            {
                item = objects[i] as Sprite;
                if (item)
                {
                    int index = nameList.IndexOf(item.name);
                    if (index != -1)
                    {
                        addItemAt(item, index, list);
                    }
                }
            }
           
            newPath = UPK_PREFIX + directoryName + ".asset";

            if (File.Exists(newPath) == false)
            {
                unAniVo.keys = list;
                AssetDatabase.CreateAsset(unAniVo, newPath);
            }
            else
            {
                unAniVo = AssetDatabase.LoadAssetAtPath<UpkAniVO>(newPath);
                if (unAniVo != null)
                {
                    unAniVo.keys = list;
                    EditorUtility.SetDirty(unAniVo);
                }
            }
            AssetDatabase.SaveAssets();


            if (updateIt)
            {
                AssetDatabase.Refresh();

                unAniVo = AssetDatabase.LoadAssetAtPath<UpkAniVO>(newPath);
                if (unAniVo != null)
                {
                    Selection.activeObject = unAniVo;
                }
            }
        }

        public void addItemAt(Sprite sprite, int index,List<SpriteInfoVO> keys)
        {
            if (index > keys.Count - 1)
            {
                for (int i = keys.Count; i < index + 1; i++)
                {
                    keys.Add(null);
                }
            }
            SpriteInfoVO item=new SpriteInfoVO();
            item.sprite = sprite;
            
            keys[index] = item;
        }

        private void processToSprite(TextureImporter texImp, List<SpriteMetaData> sprites)
        {
            texImp.spritesheet = sprites.ToArray();
            texImp.textureType = TextureImporterType.Sprite;
            texImp.spriteImportMode = SpriteImportMode.Multiple;
            texImp.mipmapEnabled = false;
            texImp.filterMode = FilterMode.Bilinear;
            texImp.textureCompression = TextureImporterCompression.Compressed;
        }

        private SpriteMetaData creatSpriteMetaData(Rect rect, string name, SpriteAlignment alignment = SpriteAlignment.Center)
        {
            SpriteMetaData smd = new SpriteMetaData();
            smd.rect = rect;
            smd.alignment = (int)alignment;
            smd.name = name;
            smd.pivot = rect.center;
            return smd;
        }

    }

  
    
}