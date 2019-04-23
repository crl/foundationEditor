using System.Collections;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class EditorGUIUtilityExt
    {
        private static Hashtable s_TextGUIContents;

        static EditorGUIUtilityExt()
        {
            s_TextGUIContents=new Hashtable();
        }

        public static GUIContent TextContent(string textAndTooltip)
        {
            if (textAndTooltip == null)
            {
                textAndTooltip = "";
            }
            string str = textAndTooltip;
            GUIContent content = (GUIContent)s_TextGUIContents[str];
            if (content == null)
            {
                string[] nameAndTooltipString = GetNameAndTooltipString(textAndTooltip);
                content = new GUIContent(nameAndTooltipString[1]);
                if (nameAndTooltipString[2] != null)
                {
                    content.tooltip = nameAndTooltipString[2];
                }
                s_TextGUIContents[str] = content;
            }
            return content;
        }

        internal static string[] GetNameAndTooltipString(string nameAndTooltip)
        {
            //nameAndTooltip = LocalizationDatabase.GetLocalizedString(nameAndTooltip);
            string[] strArray = new string[3];
            char[] separator = new char[] { '|' };
            string[] strArray2 = nameAndTooltip.Split(separator);
            switch (strArray2.Length)
            {
                case 0:
                    strArray[0] = "";
                    strArray[1] = "";
                    return strArray;

                case 1:
                    strArray[0] = strArray2[0].Trim();
                    strArray[1] = strArray[0];
                    return strArray;

                case 2:
                    strArray[0] = strArray2[0].Trim();
                    strArray[1] = strArray[0];
                    strArray[2] = strArray2[1].Trim();
                    return strArray;
            }
            Debug.LogError("Error in Tooltips: Too many strings in line beginning with '" + strArray2[0] + "'");
            return strArray;
        }

    }
}