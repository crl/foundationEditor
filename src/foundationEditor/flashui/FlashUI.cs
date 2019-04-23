using System.IO;
using System.Xml;
using foundation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace foundationEditor
{
    public class FlashUI
    {
        public void import()
        {
            string path = EditorConfigUtils.GetPrifix("flashUI");

            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("ui导入", "请配置ui导入目录", "确定");
                return;
            }
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("ui导入", "场景内必须有canvas", "确定");
                return;
            }

            path = EditorUtility.OpenFilePanel("选择场景", path, ",xml");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(path);

            XmlDocument doc = new XmlDocument();

            FileStream fileStream = File.OpenRead(path);
            doc.Load(fileStream);

            XmlNode rootNode = doc.SelectSingleNode("component");

            GameObject go=new GameObject(fileName);
            go.AddComponent<RectTransform>();

            createChild(rootNode.ChildNodes,go);
        }


        private RectTransform createUI(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            RectTransform rect = go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();
            rect.anchorMax = new Vector2(0, 1);
            rect.anchorMin = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);

            go.transform.SetParent(parent.transform, false);
            return rect;
        }

        private void createChild(XmlNodeList nodeList, GameObject go)
        {
            foreach (XmlNode node in nodeList)
            {
                string matrixStrings = node.Attributes["matrix"].InnerText;
                //Transform2X transform2X = Transform2X.fromString(matrixStrings);

                XmlAttribute attribute = node.Attributes["name"];
                string name = "";
                if (attribute != null)
                {
                    name = attribute.InnerText;
                }
                if (string.IsNullOrEmpty(name))
                {
                    name = node.LocalName;
                }
                RectTransform rect = createUI(name, go);
                //rect.localPosition = new Vector3(transform2X.tx, -transform2X.ty);

                switch (node.LocalName)
                {
                    case "text":


                        Text text = rect.gameObject.AddComponent<Text>();
                        text.font = getDefaultFont();
                        text.fontSize = int.Parse(node.Attributes["size"].InnerText);
                        text.text = node.Attributes["value"].InnerText;

                        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                            float.Parse(node.Attributes["width"].InnerText));
                        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                            float.Parse(node.Attributes["height"].InnerText));

                        break;
                    case "instance":
                        attribute = node.Attributes["isBtn"];
                        if (attribute != null)
                        {
//                            Image image = rect.gameObject.AddComponent<Image>();
//                            Button button = rect.gameObject.AddComponent<Button>();
                        }
                        else
                        {
                            createChild(node.ChildNodes, rect.gameObject);
                        }
                        break;
                    case "bitmap":
                        RawImage rawImage = rect.gameObject.AddComponent<RawImage>();

                        Texture texture =
                            AssetDatabase.LoadAssetAtPath<Texture>("Assets/Resources/UI/hero/" +
                                                                   node.Attributes["path"].InnerText);
                        if (texture != null)
                        {
                            rawImage.texture = texture;
                            rawImage.SetNativeSize();
                        }
                        break;
                }
            }
        }

        private Font _defalutFont;
        private Font getDefaultFont()
        {
            if (_defalutFont == null)
            {
                object[] fonts = AssetDatabase.LoadAllAssetsAtPath("Library/unity default resources");
                foreach (object font in fonts)
                {
                    if (font is Font)
                    {
                        _defalutFont = font as Font;
                        break;
                    }
                }
            }
            return _defalutFont;
        }
    }
}