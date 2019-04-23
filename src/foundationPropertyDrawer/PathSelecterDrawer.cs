using System.IO;
using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [CustomPropertyDrawer(typeof(PathSelecterAttribute))]
    public class PathSelecterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float w = position.width;
            position.width = w - 50;
            EditorGUI.PropertyField(position, property);

            position.x += position.width;
            position.width =50;
            if (GUI.Button(position, "select", EditorStyles.miniButton))
            {
                PathSelecterAttribute selecterAttribute = attribute as PathSelecterAttribute;
                string extention = selecterAttribute.extention;
                string value = property.stringValue;

                string path = "";
                switch (selecterAttribute.type)
                {
                    case PathSelecterType.SKILL:
                        path = EditorConfigUtils.GetProjectResource("All/skill/");
                        break;
                    case PathSelecterType.STOTY:
                        path = EditorConfigUtils.GetProjectResource("All/story/");
                        break;

                    case PathSelecterType.SKILL_STOTY:

                        path = EditorConfigUtils.GetProjectResource("All/");
                        if (string.IsNullOrEmpty(value) == false)
                        {
                            path = EditorConfigUtils.GetProjectResource("All/story/")+ value + "." + extention;
                            if (File.Exists(path) == false)
                            {
                                path = EditorConfigUtils.GetProjectResource("All/skill/")+ value + "." + extention;
                            }
                            path = FileHelper.GetFullPathParent(path);
                        }
                        break;

                    default:
                        path = EditorConfigUtils.ProjectResource;
                        break;
                }

                string fullPath = EditorUtility.OpenFilePanel("选取文件", path,extention);
                if (string.IsNullOrEmpty(fullPath) == false)
                {
                    string[] list=fullPath.As3Split("All/skill/");
                    if (list.Length < 2)
                    {
                        list = fullPath.As3Split("All/story/");
                    }

                    if (list.Length > 1)
                    {
                        fullPath = list[1];
                    }

                    list=fullPath.As3Split(".");
                    if (list.Length > 0)
                    {
                        fullPath = list[0];
                    }

                    property.stringValue = fullPath;
                }
            }
        }
    }
}