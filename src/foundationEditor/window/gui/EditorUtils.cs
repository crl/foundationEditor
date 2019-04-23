using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace foundationEditor
{
    public class EditorUtils
    {

        #region 数据定义
        private static GUIContent[] labelIcons;
        private static GUIContent[] largeIcons;

        /// <summary>
        /// Label类型icon 显示文字的
        /// </summary>
        public enum LabelIcon
        {
            Gray = 0,
            Blue,
            Teal,
            Green,
            Yellow,
            Orange,
            Red,
            Purple
        }
        /// <summary>
        /// 其他icon不显示文字
        /// </summary>
        public enum Icon
        {
            CircleGray = 0,
            CircleBlue,
            CircleTeal,
            CircleGreen,
            CircleYellow,
            CircleOrange,
            CircleRed,
            CirclePurple,
            DiamondGray,
            DiamondBlue,
            DiamondTeal,
            DiamondGreen,
            DiamondYellow,
            DiamondOrange,
            DiamondRed,
            DiamondPurple
        }
        #endregion

        static private Texture2D backdropTex;
        static public Texture2D backdropTexture
        {
            get
            {
                if (backdropTex == null)
                {
                    backdropTex = CreateCheckerTex(new Color(0.1f, 0.1f, 0.1f, 0.5f), new Color(0.2f, 0.2f, 0.2f, 0.5f));
                }
                return backdropTex;
            }
        }

        public Vector3 SceneScreenToWorldPoint(SceneView sceneView, Vector3 sceneScreenPoint)
        {
            Camera sceneCamera = sceneView.camera;
            float screenHeight = sceneCamera.orthographicSize * 2f;
            float screenWidth = screenHeight * sceneCamera.aspect;

            Vector3 worldPos = new Vector3(
                (sceneScreenPoint.x / sceneCamera.pixelWidth) * screenWidth - screenWidth * 0.5f,
                ((-(sceneScreenPoint.y) / sceneCamera.pixelHeight) * screenHeight + screenHeight * 0.5f),
                0f);
            worldPos += sceneCamera.transform.position;
            worldPos.z = 0f;

            return worldPos;
        }


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

        private static Dictionary<string, string[]> cacheGetDependenciesMap = new Dictionary<string, string[]>();
        public static string[] GetDependencies(string prefab)
        {
            string[] result = null;
            if (cacheGetDependenciesMap.TryGetValue(prefab, out result) == false)
            {
                result = AssetDatabase.GetDependencies(new string[] { prefab });
                cacheGetDependenciesMap.Add(prefab, result);
            }
            return result;
        }

        public static void ClearDependenciesCache()
        {
            cacheGetDependenciesMap.Clear();
        }


        public static Texture2D CreateTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public static Texture2D CreateCircleTex(int r, Color32 col)
        {
            int cx = r;
            int cy = r;

            int size = r * 2;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color32[] colors = texture.GetPixels32();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color32(0, 0, 0, 0);
            }

            int x, y, px, nx, py, ny, d;
            for (x = 0; x < r; x++)
            {
                d = Mathf.RoundToInt(Mathf.Sqrt(r * r - x * x));
                for (y = 0; y < d; y++)
                {
                    px = cx + x;
                    nx = cx - x;
                    py = cy + y;
                    ny = cy - y;

                    colors[px * size + py] = col;
                    colors[nx * size + py] = col;
                    colors[px * size + ny] = col;
                    colors[nx * size + ny] = col;
                }
            }
            texture.SetPixels32(colors);
            texture.Apply();
            return texture;
        }

        public static void SetIcon(GameObject gObj, LabelIcon icon)
        {
            if (labelIcons == null)
            {
                labelIcons = GetTextures("sv_label_", string.Empty, 0, 8);
            }

            SetIcon(gObj, labelIcons[(int)icon].image as Texture2D);
        }

        public Texture2D GetIcon(LabelIcon icon)
        {
            if (labelIcons == null)
            {
                labelIcons = GetTextures("sv_label_", string.Empty, 0, 8);
            }
            return labelIcons[(int) icon].image as Texture2D;
        }
        public Texture2D GetIcon(Icon icon)
        {
            if (largeIcons == null)
            {
                largeIcons = GetTextures("sv_icon_dot", "_pix16_gizmo", 0, 16);
            }
            return largeIcons[(int)icon].image as Texture2D;
        }

        public static void SetIcon(GameObject gObj, Icon icon)
        {
            if (largeIcons == null)
            {
                largeIcons = GetTextures("sv_icon_dot", "_pix16_gizmo", 0, 16);
            }

            SetIcon(gObj, largeIcons[(int)icon].image as Texture2D);
        }

        public static int PreviewCullingLayer
        {
            get
            {
                var flags = BindingFlags.Static | BindingFlags.NonPublic;
                var propInfo = typeof(Camera).GetProperty("PreviewCullingLayer", flags);
                int previewLayer = (int)propInfo.GetValue(null, new object[0]);
                return previewLayer;
            }
        }

        private static void SetIcon(GameObject gObj, Texture2D texture)
        {
            var ty = typeof(EditorGUIUtility);

            var mi = ty.GetMethod("SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
            mi.Invoke(null, new object[] { gObj, texture });
        }
        private static GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count)
        {
            GUIContent[] guiContentArray = new GUIContent[count];

            var t = typeof(EditorGUIUtility);
            var mi = t.GetMethod("IconContent", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
            for (int index = 0; index < count; ++index)
            {
                guiContentArray[index] = mi.Invoke(null, new object[] { baseName + (object)(startIndex + index) + postFix }) as GUIContent;
            }

            return guiContentArray;
        }

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



        static public void DrawOutline(Rect rect, Color color)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Texture2D tex = EditorGUIUtility.whiteTexture;
                GUI.color = color;
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
                GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
                GUI.color = Color.white;
            }
        }

        public static void DrawSprite(Rect rect, Sprite sprite, bool drawBox = true,bool rawPixel=true)
        {
            if (drawBox)
            {
                GUI.Box(rect, "");
            }

            if (sprite != null)
            {
                Texture t = sprite.texture;
                Rect tr = sprite.textureRect;
                Rect r = new Rect(tr.x / t.width, tr.y / t.height, tr.width / t.width, tr.height / t.height);

                if (rawPixel)
                {
                    Rect inneRect = new Rect();
                    inneRect.width = tr.width;
                    inneRect.height = tr.height;

                    inneRect.x = rect.x + (rect.width - tr.width)/2;
                    inneRect.y = rect.y + (rect.height - tr.height)/2;

                    rect = inneRect;
                }
                else
                {
                    rect.x += 2;
                    rect.y += 2;
                    rect.width -= 4;
                    rect.height -= 4;
                }
                GUI.DrawTextureWithTexCoords(rect, t, r);
            }
        }

        public static void GetRenderableBoundsRecurse(ref Bounds bounds, GameObject go)
        {
            MeshRenderer component = go.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
            MeshFilter filter = go.GetComponent(typeof(MeshFilter)) as MeshFilter;
            if (((component != null) && (filter != null)) && (filter.sharedMesh != null))
            {
                if (bounds.extents == Vector3.zero)
                {
                    bounds = component.bounds;
                }
                else
                {
                    bounds.Encapsulate(component.bounds);
                }
            }
            SkinnedMeshRenderer renderer2 = go.GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
            if ((renderer2 != null) && (renderer2.sharedMesh != null))
            {
                if (bounds.extents == Vector3.zero)
                {
                    bounds = renderer2.bounds;
                }
                else
                {
                    bounds.Encapsulate(renderer2.bounds);
                }
            }
            SpriteRenderer renderer3 = go.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            if ((renderer3 != null) && (renderer3.sprite != null))
            {
                if (bounds.extents == Vector3.zero)
                {
                    bounds = renderer3.bounds;
                }
                else
                {
                    bounds.Encapsulate(renderer3.bounds);
                }
            }
            IEnumerator enumerator = go.transform.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Transform current = (Transform)enumerator.Current;
                    GetRenderableBoundsRecurse(ref bounds, current.gameObject);
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }
        public static Vector3 GetRenderableCenterRecurse(GameObject go, int minDepth, int maxDepth)
        {
            Vector3 zero = Vector3.zero;
            float num = GetRenderableCenterRecurse(ref zero, go, 0, minDepth, maxDepth);
            if (num > 0f)
            {
                return (Vector3)(zero / num);
            }
            return go.transform.position;
        }

        private static float GetRenderableCenterRecurse(ref Vector3 center, GameObject go, int depth, int minDepth, int maxDepth)
        {
            if (depth > maxDepth)
            {
                return 0f;
            }
            float num = 0f;
            if (depth > minDepth)
            {
                MeshRenderer component = go.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
                MeshFilter filter = go.GetComponent(typeof(MeshFilter)) as MeshFilter;
                SkinnedMeshRenderer renderer2 = go.GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
                SpriteRenderer renderer3 = go.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
                if (((component == null) && (filter == null)) && ((renderer2 == null) && (renderer3 == null)))
                {
                    num = 1f;
                    center += go.transform.position;
                }
                else if ((component != null) && (filter != null))
                {
                    if (Vector3.Distance(component.bounds.center, go.transform.position) < 0.01f)
                    {
                        num = 1f;
                        center += go.transform.position;
                    }
                }
                else if (renderer2 != null)
                {
                    if (Vector3.Distance(renderer2.bounds.center, go.transform.position) < 0.01f)
                    {
                        num = 1f;
                        center += go.transform.position;
                    }
                }
                else if ((renderer3 != null) && (Vector3.Distance(renderer3.bounds.center, go.transform.position) < 0.01f))
                {
                    num = 1f;
                    center += go.transform.position;
                }
            }
            depth++;
            IEnumerator enumerator = go.transform.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Transform current = (Transform)enumerator.Current;
                    num += GetRenderableCenterRecurse(ref center, current.gameObject, depth, minDepth, maxDepth);
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
                
            }
            return num;
        }

        public static void GotoScenePoint(Vector3 position,bool usePivotY=true)
        {
            UnityEngine.Object[] intialFocus = Selection.objects;
            GameObject tempFocusView = new GameObject("Temp Focus View");
          
            try
            {
                SceneView lastActiveSceneView = SceneView.lastActiveSceneView;

                if (usePivotY)
                {
                    Vector3 pivot = lastActiveSceneView.pivot;
                    position.y = pivot.y;
                }
                tempFocusView.transform.position = position;
                Selection.objects = new UnityEngine.Object[] {tempFocusView};
                lastActiveSceneView.FrameSelected();
                Selection.objects = intialFocus;
            }
            catch (NullReferenceException)
            {
                //do nothing
            }
            UnityEngine.Object.DestroyImmediate(tempFocusView);
        }

        public static void AddTag(string tag)
        {
            if (!isHasTag(tag))
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty it = tagManager.GetIterator();
                while (it.NextVisible(true))
                {
                    if (it.name == "tags")
                    {
                        SerializedProperty dataPoint;
                        for (int i = 0; i < it.arraySize; i++)
                        {
                            dataPoint = it.GetArrayElementAtIndex(i);
                            if (string.IsNullOrEmpty(dataPoint.stringValue))
                            {
                                dataPoint.stringValue = tag;
                                Debug.Log(i + " AddTag:" + tag);
                                tagManager.ApplyModifiedProperties();
                                return;
                            }
                        }
                        int index = it.arraySize;
                        it.arraySize += 1;
                        dataPoint = it.GetArrayElementAtIndex(index);
                        if (dataPoint!=null)
                        {
                            dataPoint.stringValue = tag;
                            Debug.Log(index + " AddTag:" + tag);
                            tagManager.ApplyModifiedProperties();
                        }
                    }
                }
            }
        }

        public static void AddLayer(string layer)
        {
            if (!isHasLayer(layer))
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty it = tagManager.GetIterator();
                while (it.NextVisible(true))
                {
                    if (it.name.Equals("layers"))
                    {
                        SerializedProperty dataPoint;
                        for (int i = 8; i < it.arraySize; i++)
                        {
                            dataPoint = it.GetArrayElementAtIndex(i);
                            if (string.IsNullOrEmpty(dataPoint.stringValue))
                            {
                                dataPoint.stringValue = layer;
                                Debug.Log(i + " AddLayer:" + layer);
                                tagManager.ApplyModifiedProperties();
                                return;
                            }
                        }
                       
                    }
                }
            }
        }


        public static void AddSortingLayer(string layer)
        {
            if (!isHasSortingLayer(layer))
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty it = tagManager.GetIterator();
                while (it.NextVisible(true))
                {
                    //Debug.Log(it.name);
                    if (it.name.Equals("m_SortingLayers"))
                    {
                        Type internalEditorUtilityType = typeof (InternalEditorUtility);
                        MethodInfo GetSortingLayerName = internalEditorUtilityType.GetMethod("GetSortingLayerName",
                            BindingFlags.Static | BindingFlags.NonPublic);
                        MethodInfo SetSortingLayerName = internalEditorUtilityType.GetMethod("SetSortingLayerName",
                            BindingFlags.Static | BindingFlags.NonPublic);

                        MethodInfo AddSortingLayer = internalEditorUtilityType.GetMethod("AddSortingLayer",
                            BindingFlags.Static | BindingFlags.NonPublic);

                        for (int i = 0; i < it.arraySize; i++)
                        {
                            string stringName = (string) GetSortingLayerName.Invoke(null, new object[] {i});
                            if (string.IsNullOrEmpty(stringName))
                            {
                                Debug.Log(i + " AddSortingLayer:" + layer);
                              
                                SetSortingLayerName.Invoke(null, new object[] {i, layer});
                                tagManager.ApplyModifiedProperties();
                                return;
                            }
                        }

                        int index = it.arraySize;
                        AddSortingLayer.Invoke(null, null);
                        SetSortingLayerName.Invoke(null, new object[] {index, layer});
                        Debug.Log(index + " AddSortingLayer:" + layer);
                        tagManager.ApplyModifiedProperties();
                    }
                }
            }
        }
        public static bool HasRenderableParts(GameObject go)
        {
            MeshRenderer[] componentsInChildren = go.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in componentsInChildren)
            {
                MeshFilter component = renderer.gameObject.GetComponent<MeshFilter>();
                if ((component != null) && (component.sharedMesh != null))
                {
                    return true;
                }
            }
            SkinnedMeshRenderer[] rendererArray3 = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer2 in rendererArray3)
            {
                if (renderer2.sharedMesh != null)
                {
                    return true;
                }
            }
            SpriteRenderer[] rendererArray5 = go.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer renderer3 in rendererArray5)
            {
                if (renderer3.sprite != null)
                {
                    return true;
                }
            }
            return false;
        }


        public static void InitInstantiatedPreviewRecursive(GameObject go)
        {
            go.hideFlags = HideFlags.HideAndDontSave;
            go.layer = PreviewCullingLayer;
            IEnumerator enumerator = go.transform.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Transform current = (Transform)enumerator.Current;
                    InitInstantiatedPreviewRecursive(current.gameObject);
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }


        public static Camera GetMainCamera()
        {
            if (Camera.current != null)
            {
                return Camera.current;
            }

            if (Camera.main != null)
            {
                return Camera.main;
            }

            SceneView sceneView = SceneView.sceneViews[0] as SceneView;
            if (sceneView != null)
            {
                return sceneView.camera;
            }

            return null;
        }

        public static void Destory(GameObject container)
        {
            int len = container.transform.childCount;
            for (int i = len - 1; i > -1; i--)
            {
                Transform t = container.transform.GetChild(i);
                Destory(t.gameObject);
            }
            GameObject.DestroyImmediate(container);
        }

        private static bool isHasTag(string tag)
        {
            for (int i = 0; i < InternalEditorUtility.tags.Length; i++)
            {
                if (InternalEditorUtility.tags[i].Contains(tag))
                    return true;
            }
            return false;
        }

        private static bool isHasSortingLayer(string layer)
        {
            Type internalEditorUtilityType = typeof(InternalEditorUtility);
            PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
            string[] sortingLayerNamess = (string[])sortingLayersProperty.GetValue(null, new object[0]);

            foreach (string sortingLayer in sortingLayerNamess)
            {
                if (sortingLayer.Contains(layer))
                    return true;
            }
            return false;
        }

        private static bool isHasLayer(string layer)
        {
            for (int i = 0; i < InternalEditorUtility.layers.Length; i++)
            {
                if (InternalEditorUtility.layers[i].Contains(layer))
                    return true;
            }
            return false;
        }

        public static void SetEnabledRecursive(GameObject go, bool enabled)
        {
            foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = enabled;
            }
        }

        public static GameObject InstantiateForAnimatorPreview(GameObject original)
        {
            if (original == null)
            {
                throw new ArgumentException("The prefab you want to instantiate is null.");
            }
            GameObject go = GameObject.Instantiate(original, Vector3.zero, Quaternion.identity) as GameObject;
            go.name = go.name + "AnimatorPreview";
            go.tag = "Untagged";
            InitInstantiatedPreviewRecursive(go);
            Animator[] componentsInChildren = go.GetComponentsInChildren<Animator>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                Animator animator = componentsInChildren[i];
                animator.enabled = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.logWarnings = false;
                animator.fireEvents = false;
            }
            if (componentsInChildren.Length == 0)
            {
                Animator animator2 = go.AddComponent<Animator>();
                animator2.enabled = false;
                animator2.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator2.logWarnings = false;
                animator2.fireEvents = false;
            }
            return go;
        }



    }
}