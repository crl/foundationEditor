using System;
using foundation;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class NGUISettings
{
    #region Generic Get and Set methods
    /// <summary>
    /// Save the specified boolean value in settings.
    /// </summary>

    static public void SetBool(string name, bool val) { EditorPrefs.SetBool(name, val); }

    /// <summary>
    /// Save the specified integer value in settings.
    /// </summary>

    static public void SetInt(string name, int val) { EditorPrefs.SetInt(name, val); }

    /// <summary>
    /// Save the specified float value in settings.
    /// </summary>

    static public void SetFloat(string name, float val) { EditorPrefs.SetFloat(name, val); }

    /// <summary>
    /// Save the specified string value in settings.
    /// </summary>

    static public void SetString(string name, string val) { EditorPrefs.SetString(name, val); }

    /// <summary>
    /// Save the specified color value in settings.
    /// </summary>

    static public void SetColor(string name, Color c) { SetString(name, c.r + " " + c.g + " " + c.b + " " + c.a); }

    /// <summary>
    /// Save the specified enum value to settings.
    /// </summary>

    static public void SetEnum(string name, System.Enum val) { SetString(name, val.ToString()); }

    /// <summary>
    /// Save the specified object in settings.
    /// </summary>

    static public void Set(string name, Object obj)
    {
        if (obj == null)
        {
            EditorPrefs.DeleteKey(name);
        }
        else
        {
            if (obj != null)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                if (!string.IsNullOrEmpty(path))
                {
                    EditorPrefs.SetString(name, path);
                }
                else
                {
                    EditorPrefs.SetString(name, obj.GetInstanceID().ToString());
                }
            }
            else EditorPrefs.DeleteKey(name);
        }
    }

    /// <summary>
    /// Get the previously saved boolean value.
    /// </summary>

    static public bool GetBool(string name, bool defaultValue) { return EditorPrefs.GetBool(name, defaultValue); }

    /// <summary>
    /// Get the previously saved integer value.
    /// </summary>

    static public int GetInt(string name, int defaultValue) { return EditorPrefs.GetInt(name, defaultValue); }

    /// <summary>
    /// Get the previously saved float value.
    /// </summary>

    static public float GetFloat(string name, float defaultValue) { return EditorPrefs.GetFloat(name, defaultValue); }

    /// <summary>
    /// Get the previously saved string value.
    /// </summary>

    static public string GetString(string name, string defaultValue) { return EditorPrefs.GetString(name, defaultValue); }

    /// <summary>
    /// Get a previously saved color value.
    /// </summary>

    static public Color GetColor(string name, Color c)
    {
        string strVal = GetString(name, c.r + " " + c.g + " " + c.b + " " + c.a);
        string[] parts = strVal.Split(' ');

        if (parts.Length == 4)
        {
            float.TryParse(parts[0], out c.r);
            float.TryParse(parts[1], out c.g);
            float.TryParse(parts[2], out c.b);
            float.TryParse(parts[3], out c.a);
        }
        return c;
    }

    /// <summary>
    /// Get a previously saved enum from settings.
    /// </summary>

    static public T GetEnum<T>(string name, T defaultValue)
    {
        string val = GetString(name, defaultValue.ToString());
        string[] names = System.Enum.GetNames(typeof(T));
        System.Array values = System.Enum.GetValues(typeof(T));

        for (int i = 0; i < names.Length; ++i)
        {
            if (names[i] == val)
                return (T)values.GetValue(i);
        }
        return defaultValue;
    }
    #endregion
    static public bool showTransformHandles
    {
        get { return GetBool("NGUI Transform Handles", false); }
        set { SetBool("NGUI Transform Handles", value); }
    }

    static public bool minimalisticLook
    {
        get { return GetBool("NGUI Minimalistic", false); }
        set { SetBool("NGUI Minimalistic", value); }
    }
}


public class NGUIEditorTools
{

    static Texture2D mBackdropTex;
    static Texture2D mContrastTex;
    static Texture2D mGradientTex;

    /// <summary>
    /// Returns a blank usable 1x1 white texture.
    /// </summary>

    static public Texture2D blankTexture
    {
        get
        {
            return EditorGUIUtility.whiteTexture;
        }
    }

    /// <summary>
    /// Returns a usable texture that looks like a dark checker board.
    /// </summary>

    static public Texture2D backdropTexture
    {
        get
        {
            if (mBackdropTex == null) mBackdropTex = CreateCheckerTex(
                new Color(0.1f, 0.1f, 0.1f, 0.5f),
                new Color(0.2f, 0.2f, 0.2f, 0.5f));
            return mBackdropTex;
        }
    }

    /// <summary>
    /// Returns a usable texture that looks like a high-contrast checker board.
    /// </summary>

    static public Texture2D contrastTexture
    {
        get
        {
            if (mContrastTex == null) mContrastTex = CreateCheckerTex(
                new Color(0f, 0.0f, 0f, 0.5f),
                new Color(1f, 1f, 1f, 0.5f));
            return mContrastTex;
        }
    }

    /// <summary>
    /// Gradient texture is used for title bars / headers.
    /// </summary>

    static public Texture2D gradientTexture
    {
        get
        {
            if (mGradientTex == null) mGradientTex = CreateGradientTex();
            return mGradientTex;
        }
    }

    /// <summary>
    /// Create a white dummy texture.
    /// </summary>

    static Texture2D CreateDummyTex()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.name = "[Generated] Dummy Texture";
        tex.hideFlags = HideFlags.DontSave;
        tex.filterMode = FilterMode.Point;
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// Create a checker-background texture
    /// </summary>

    static Texture2D CreateCheckerTex(Color c0, Color c1)
    {
        Texture2D tex = new Texture2D(16, 16);
        tex.name = "[Generated] Checker Texture";
        tex.hideFlags = HideFlags.DontSave;

        for (int y = 0; y < 8; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c1);
        for (int y = 8; y < 16; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c0);
        for (int y = 0; y < 8; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c0);
        for (int y = 8; y < 16; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c1);

        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return tex;
    }

    /// <summary>
    /// Create a gradient texture
    /// </summary>

    static Texture2D CreateGradientTex()
    {
        Texture2D tex = new Texture2D(1, 16);
        tex.name = "[Generated] Gradient Texture";
        tex.hideFlags = HideFlags.DontSave;

        Color c0 = new Color(1f, 1f, 1f, 0f);
        Color c1 = new Color(1f, 1f, 1f, 0.4f);

        for (int i = 0; i < 16; ++i)
        {
            float f = Mathf.Abs((i / 15f) * 2f - 1f);
            f *= f;
            tex.SetPixel(0, i, Color.Lerp(c0, c1, f));
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    /// <summary>
    /// Draws the tiled texture. Like GUI.DrawTexture() but tiled instead of stretched.
    /// </summary>

    static public void DrawTiledTexture(Rect rect, Texture tex)
    {
        GUI.BeginGroup(rect);
        {
            int width = Mathf.RoundToInt(rect.width);
            int height = Mathf.RoundToInt(rect.height);

            for (int y = 0; y < height; y += tex.height)
            {
                for (int x = 0; x < width; x += tex.width)
                {
                    GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
                }
            }
        }
        GUI.EndGroup();
    }

    /// <summary>
    /// Draw a single-pixel outline around the specified rectangle.
    /// </summary>

    static public void DrawOutline(Rect rect)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Texture2D tex = contrastTexture;
            GUI.color = Color.white;
            DrawTiledTexture(new Rect(rect.xMin, rect.yMax, 1f, -rect.height), tex);
            DrawTiledTexture(new Rect(rect.xMax, rect.yMax, 1f, -rect.height), tex);
            DrawTiledTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
            DrawTiledTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
        }
    }

    /// <summary>
    /// Draw a single-pixel outline around the specified rectangle.
    /// </summary>

    static public void DrawOutline(Rect rect, Color color)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Texture2D tex = blankTexture;
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
            GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
            GUI.color = Color.white;
        }
    }

    /// <summary>
    /// Draw a selection outline around the specified rectangle.
    /// </summary>

    static public void DrawOutline(Rect rect, Rect relative, Color color)
    {
        if (Event.current.type == EventType.Repaint)
        {
            // Calculate where the outer rectangle would be
            float x = rect.xMin + rect.width * relative.xMin;
            float y = rect.yMax - rect.height * relative.yMin;
            float width = rect.width * relative.width;
            float height = -rect.height * relative.height;
            relative = new Rect(x, y, width, height);

            // Draw the selection
            DrawOutline(relative, color);
        }
    }

    /// <summary>
    /// Draw a selection outline around the specified rectangle.
    /// </summary>

    static public void DrawOutline(Rect rect, Rect relative)
    {
        if (Event.current.type == EventType.Repaint)
        {
            // Calculate where the outer rectangle would be
            float x = rect.xMin + rect.width * relative.xMin;
            float y = rect.yMax - rect.height * relative.yMin;
            float width = rect.width * relative.width;
            float height = -rect.height * relative.height;
            relative = new Rect(x, y, width, height);

            // Draw the selection
            DrawOutline(relative);
        }
    }

    /// <summary>
    /// Draw a 9-sliced outline.
    /// </summary>

    static public void DrawOutline(Rect rect, Rect outer, Rect inner)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Color green = new Color(0.4f, 1f, 0f, 1f);

            DrawOutline(rect, new Rect(outer.x, inner.y, outer.width, inner.height));
            DrawOutline(rect, new Rect(inner.x, outer.y, inner.width, outer.height));
            DrawOutline(rect, outer, green);
        }
    }

    /// <summary>
    /// Draw a checkered background for the specified texture.
    /// </summary>

    static public Rect DrawBackground(Texture2D tex, float ratio)
    {
        Rect rect = GUILayoutUtility.GetRect(0f, 0f);
        rect.width = Screen.width - rect.xMin;
        rect.height = rect.width * ratio;
        GUILayout.Space(rect.height);

        if (Event.current.type == EventType.Repaint)
        {
            Texture2D blank = blankTexture;
            Texture2D check = backdropTexture;

            // Lines above and below the texture rectangle
            GUI.color = new Color(0f, 0f, 0f, 0.2f);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin - 1, rect.width, 1f), blank);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), blank);
            GUI.color = Color.white;

            // Checker background
            DrawTiledTexture(rect, check);
        }
        return rect;
    }

    /// <summary>
    /// Draw a visible separator in addition to adding some padding.
    /// </summary>

    static public void DrawSeparator()
    {
        GUILayout.Space(12f);

        if (Event.current.type == EventType.Repaint)
        {
            Texture2D tex = blankTexture;
            Rect rect = GUILayoutUtility.GetLastRect();
            GUI.color = new Color(0f, 0f, 0f, 0.25f);
            GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
            GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
            GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
            GUI.color = Color.white;
        }
    }

    /// <summary>
    /// Convenience function that displays a list of sprites and returns the selected value.
    /// </summary>

    static public string DrawList(string field, string[] list, string selection, params GUILayoutOption[] options)
    {
        if (list != null && list.Length > 0)
        {
            int index = 0;
            if (string.IsNullOrEmpty(selection)) selection = list[0];

            // We need to find the sprite in order to have it selected
            if (!string.IsNullOrEmpty(selection))
            {
                for (int i = 0; i < list.Length; ++i)
                {
                    if (selection.Equals(list[i], System.StringComparison.OrdinalIgnoreCase))
                    {
                        index = i;
                        break;
                    }
                }
            }

            // Draw the sprite selection popup
            index = string.IsNullOrEmpty(field) ?
                EditorGUILayout.Popup(index, list, options) :
                EditorGUILayout.Popup(field, index, list, options);

            return list[index];
        }
        return null;
    }

    /// <summary>
    /// Convenience function that displays a list of sprites and returns the selected value.
    /// </summary>

    static public string DrawAdvancedList(string field, string[] list, string selection, params GUILayoutOption[] options)
    {
        if (list != null && list.Length > 0)
        {
            int index = 0;
            if (string.IsNullOrEmpty(selection)) selection = list[0];

            // We need to find the sprite in order to have it selected
            if (!string.IsNullOrEmpty(selection))
            {
                for (int i = 0; i < list.Length; ++i)
                {
                    if (selection.Equals(list[i], System.StringComparison.OrdinalIgnoreCase))
                    {
                        index = i;
                        break;
                    }
                }
            }

            // Draw the sprite selection popup
            index = string.IsNullOrEmpty(field) ?
                DrawPrefixList(index, list, options) :
                DrawPrefixList(field, index, list, options);

            return list[index];
        }
        return null;
    }
    static public bool DrawPrefixButton(string text)
    {
        return GUILayout.Button(text, "DropDown", GUILayout.Width(76f));
    }

    static public bool DrawPrefixButton(string text, params GUILayoutOption[] options)
    {
        return GUILayout.Button(text, "DropDown", options);
    }

    static public int DrawPrefixList(int index, string[] list, params GUILayoutOption[] options)
    {
        return EditorGUILayout.Popup(index, list, "DropDown", options);
    }

    static public int DrawPrefixList(string text, int index, string[] list, params GUILayoutOption[] options)
    {
        return EditorGUILayout.Popup(text, index, list, "DropDown", options);
    }

    static public bool DrawHeader(string text) { return DrawHeader(text, text, false, NGUISettings.minimalisticLook); }
    static public bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
    {
        bool state = EditorPrefs.GetBool(key, true);

        if (!minimalistic) GUILayout.Space(3f);
        if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal();
        GUI.changed = false;

        if (minimalistic)
        {
            if (state) text = "\u25BC" + (char)0x200a + text;
            else text = "\u25BA" + (char)0x200a + text;

            GUILayout.BeginHorizontal();
            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
            if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }
        else
        {
            text = "<b><size=11>" + text + "</size></b>";
            if (state) text = "\u25BC " + text;
            else text = "\u25BA " + text;
            if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
        }

        if (GUI.changed) EditorPrefs.SetBool(key, state);

        if (!minimalistic) GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn && !state) GUILayout.Space(3f);
        return state;
    }

    /// <summary>
    /// Draw a sprite preview.
    /// </summary>
    static public void DrawSprite(Texture2D tex, Rect drawRect, Color color, Rect textureRect, Vector4 border)
    {
        NGUIEditorTools.DrawSprite(tex, drawRect, color, null,
            Mathf.RoundToInt(textureRect.x),
            Mathf.RoundToInt(tex.height - textureRect.y - textureRect.height),
            Mathf.RoundToInt(textureRect.width),
            Mathf.RoundToInt(textureRect.height),
            Mathf.RoundToInt(border.x),
            Mathf.RoundToInt(border.y),
            Mathf.RoundToInt(border.z),
            Mathf.RoundToInt(border.w));
    }

    /// <summary>
    /// Draw a sprite preview.
    /// </summary>

    static public void DrawSprite(Texture2D tex, Rect drawRect, Color color, Material mat,
        int x, int y, int width, int height, int borderLeft, int borderBottom, int borderRight, int borderTop)
    {
        if (!tex) return;

        // Create the texture rectangle that is centered inside rect.
        Rect outerRect = drawRect;
        outerRect.width = width;
        outerRect.height = height;

        if (width > 0)
        {
            float f = drawRect.width / outerRect.width;
            outerRect.width *= f;
            outerRect.height *= f;
        }

        if (drawRect.height > outerRect.height)
        {
            outerRect.y += (drawRect.height - outerRect.height) * 0.5f;
        }
        else if (outerRect.height > drawRect.height)
        {
            float f = drawRect.height / outerRect.height;
            outerRect.width *= f;
            outerRect.height *= f;
        }

        if (drawRect.width > outerRect.width) outerRect.x += (drawRect.width - outerRect.width) * 0.5f;

        // Draw the background
        DrawTiledTexture(outerRect, NGUIEditorTools.backdropTexture);

        // Draw the sprite
        GUI.color = color;

        if (mat == null)
        {
            Rect uv = new Rect(x, y, width, height);
            uv = MathExtendUtils.ConvertToTexCoords(uv, tex.width, tex.height);
            GUI.DrawTextureWithTexCoords(outerRect, tex, uv, true);
        }
        else
        {
            // NOTE: There is an issue in Unity that prevents it from clipping the drawn preview
            // using BeginGroup/EndGroup, and there is no way to specify a UV rect... le'suq.
            UnityEditor.EditorGUI.DrawPreviewTexture(outerRect, tex, mat);
        }

        if (Selection.activeGameObject == null || Selection.gameObjects.Length == 1)
        {
            // Draw the border indicator lines
            GUI.BeginGroup(outerRect);
            {
                tex = contrastTexture;
                GUI.color = Color.white;

                if (borderLeft > 0)
                {
                    float x0 = (float)borderLeft / width * outerRect.width - 1;
                    DrawTiledTexture(new Rect(x0, 0f, 1f, outerRect.height), tex);
                }

                if (borderRight > 0)
                {
                    float x1 = (float)(width - borderRight) / width * outerRect.width - 1;
                    DrawTiledTexture(new Rect(x1, 0f, 1f, outerRect.height), tex);
                }

                if (borderBottom > 0)
                {
                    float y0 = (float)(height - borderBottom) / height * outerRect.height - 1;
                    DrawTiledTexture(new Rect(0f, y0, outerRect.width, 1f), tex);
                }

                if (borderTop > 0)
                {
                    float y1 = (float)borderTop / height * outerRect.height - 1;
                    DrawTiledTexture(new Rect(0f, y1, outerRect.width, 1f), tex);
                }
            }
            GUI.EndGroup();

            // Draw the lines around the sprite
            Handles.color = Color.black;
            Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMin), new Vector3(outerRect.xMin, outerRect.yMax));
            Handles.DrawLine(new Vector3(outerRect.xMax, outerRect.yMin), new Vector3(outerRect.xMax, outerRect.yMax));
            Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMin), new Vector3(outerRect.xMax, outerRect.yMin));
            Handles.DrawLine(new Vector3(outerRect.xMin, outerRect.yMax), new Vector3(outerRect.xMax, outerRect.yMax));

            // Sprite size label
            string text = string.Format("Sprite Size: {0}x{1}", Mathf.RoundToInt(width), Mathf.RoundToInt(height));
            EditorGUI.DropShadowLabel(GUILayoutUtility.GetRect(Screen.width, 18f), text);
        }
    }

    /// <summary>
    /// Begin drawing the content area.
    /// </summary>

    static public void BeginContents() { BeginContents(NGUISettings.minimalisticLook); }

    static bool mEndHorizontal = false;
    static public void BeginContents(bool minimalistic)
    {
        if (!minimalistic)
        {
            mEndHorizontal = true;
            GUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
        }
        else
        {
            mEndHorizontal = false;
            EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
            GUILayout.Space(10f);
        }
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
    }
    static public void EndContents()
    {
        GUILayout.Space(3f);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (mEndHorizontal)
        {
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(3f);
    }

    static public void RegisterUndo(string name, params Object[] objects)
    {
        if (objects != null && objects.Length > 0)
        {
            UnityEditor.Undo.RecordObjects(objects, name);

            foreach (Object obj in objects)
            {
                if (obj == null) continue;
                EditorUtility.SetDirty(obj);
            }
        }
    }

    static public void SetLabelWidth(float width)
    {
        EditorGUIUtility.labelWidth = width;
    }

    static public void DrawPadding()
    {
        if (!NGUISettings.minimalisticLook)
            GUILayout.Space(18f);
    }
}
