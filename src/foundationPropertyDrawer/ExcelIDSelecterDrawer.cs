using System.Collections.Generic;
using foundation;
using gameSDK;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [CustomPropertyDrawer(typeof(ExcelIDSelecterAttribute))]
    public class ExcelIDSelecterDrawer : PropertyDrawer
    {
        public static ASDictionary<string, List<ExcelRefVO>> ExcelIDMapping = new ASDictionary<List<ExcelRefVO>>();

        public static ASDictionary<string, ExcelMapVO> ExcelMapMapping = new ASDictionary<ExcelMapVO>();

        public static void AutoFill()
        {
            string path = EditorConfigUtils.GetPrifix(ExcelFileIDType.Npc);

            Dictionary<string,object> o=(Dictionary<string, object>)FileHelper.GetAMF(path, false);

            path = EditorConfigUtils.GetPrifix(ExcelFileIDType.Monster);
            path = EditorConfigUtils.GetPrifix(ExcelFileIDType.Collection);
        }


        public static void Fill(string excelID, List<ExcelRefVO> ids)
        {
            if (string.IsNullOrEmpty(excelID))
            {
                return;
            }
            ExcelIDMapping[excelID] = ids;
        }
        public static ExcelRefVO Get(string excelID, string id)
        {
            if (string.IsNullOrEmpty(excelID) || string.IsNullOrEmpty(id))
            {
                return null;
            }
            List<ExcelRefVO> ids = null;
            if(ExcelIDMapping.TryGetValue(excelID,out ids))
            {
                foreach (ExcelRefVO vo in ids)
                {
                    if (vo.id == id)
                    {
                        return vo;
                    }
                }
            }
            return null;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ExcelIDSelecterAttribute selecterAttribute = attribute as ExcelIDSelecterAttribute;
            string excelFileID = selecterAttribute.excelFileID;
            if (string.IsNullOrEmpty(excelFileID))
            {
                EditorGUI.PropertyField(position, property);
                return;
            }

            float w = position.width;
            position.width = w - 80;
            EditorGUI.PropertyField(position, property);

            position.x += position.width;
            position.width = 80;

            ExcelRefVO vo = Get(excelFileID, property.stringValue);
            string selectedName = ".";
            if (vo != null)
            {
                selectedName = vo.name;
            }
            if (GUI.Button(position, selectedName, EditorStyles.miniButton))
            {
                showMenu(excelFileID, property);
            }
        }

        private void showMenu(string excelFileID, SerializedProperty property)
        {
            List<ExcelRefVO> ids = null;
            if (ExcelIDMapping.TryGetValue(excelFileID, out ids))
            {
                GenericMenu scenesGenericMenu = new GenericMenu();
                foreach (ExcelRefVO id in ids)
                {
                    GUIContent content = new GUIContent(id.name + "(" + id.id + ")");
                    scenesGenericMenu.AddItem(content, false, (object o) =>
                    {
                        ExcelRefVO vo = o as ExcelRefVO;
                        property.stringValue = vo.id;
                        property.serializedObject.ApplyModifiedProperties();
                    }, id);
                }
                scenesGenericMenu.ShowAsContext();
            }
        }

        public static void AddMapItem(ExcelMapVO excelVO)
        {
            ExcelMapMapping.Add(excelVO.uri, excelVO);
        }

        public static void ShowView(ExcelRefVO vo)
        {
            ProjectPrefabWindow window = EditorWindow.GetWindow<ProjectPrefabWindow>();
            window.searchView(vo.uri);
        }
    }
}