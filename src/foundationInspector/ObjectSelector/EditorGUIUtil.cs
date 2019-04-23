﻿using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class EditorGUIUtil
{
    public static string SearchField(Rect position, string text)
    {
        Rect position2 = position;
        position2.width -= 15f;
        text = EditorGUI.TextField(position2, text, new GUIStyle("SearchTextField"));
        Rect position3 = position;
        position3.x += position.width - 15f;
        position3.width = 15f;
        if (GUI.Button(position3, GUIContent.none, string.IsNullOrEmpty(text) ? "SearchCancelButtonEmpty" : "SearchCancelButton"))
        {
            text = string.Empty;
            GUIUtility.keyboardControl = 0;
        }
        return text;
    }

    public static string SearchField(string value, params GUILayoutOption[] options)
    {
        MethodInfo info = typeof(EditorGUILayout).GetMethod("ToolbarSearchField", BindingFlags.NonPublic | BindingFlags.Static, null, new System.Type[] { typeof(string), typeof(GUILayoutOption[]) }, null);
        if (info != null)
        {
            value = (string)info.Invoke(null, new object[] { value, options });
        }
        return value;
    }

    public static bool ObjectPickerField(SerializedProperty property, Action<UnityEngine.Object> itemSelectedCallback = null, string folderPath = "Assets")
    {
        if (GUILayout.Button("C", EditorStyles.miniButton, GUILayout.Width(24f)))
        {
            ObjectSelectorWindow.ShowObjectPicker(property, itemSelectedCallback, folderPath);
        }
        return true;
    }
}