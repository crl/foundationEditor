using foundation;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace foundationEditor
{
    public class PreviewSystem
    {
        public BaseEditorWindow parentEditorWindow;

        private class Styles
        {
            public GUIContent avatarIcon = EditorGUIUtility.IconContent("Avatar Icon",
                "Changes the model to use for previewing.");

            public GUIContent ik = new GUIContent("IK", "Activates feet IK preview");

            public GUIContent pivot = EditorGUIUtility.IconContent("AvatarPivot",
                "Displays avatar's pivot and mass center");

            public GUIStyle preButton = "preButton";
            public GUIStyle preLabel = "preLabel";
            public GUIStyle preSlider = "preSlider";
            public GUIStyle preSliderThumb = "preSliderThumb";

            public GUIContent speedScale =
                EditorGUIUtility.IconContent("SpeedScale", "Changes animation preview speed");
        }

        private float rotationY = 180;
        private Renderer selectedRenderer;
        private GameObject previewInstance;
        private GameObject previewPrefab;
        private PlayAnimationEditor playAnimationEditor;
        private PlayParticleSystemEditor playParticleSystemEditor;
        private List<AnimationClip> animationClips = new List<AnimationClip>();
        private ParticleSystem[] particleSystems = new ParticleSystem[0];
        private Renderer[] renderers = new Renderer[0];
        private PreviewRenderUtility m_PreviewUtility;
        private Texture2D m_FloorTexture;
        private Mesh m_FloorPlane;

        private Material m_FloorMaterial;
        private Material m_ShadowPlaneMaterial;
        private Material m_FloorMaterialSmall;
        private Material m_ShadowMaskMaterial;

        private GameObject m_ReferenceInstance;
        private GameObject m_RootInstance;
        private GameObject m_NameInstance;

        private Styles s_Styles;
        private Rect previewRect;
        private Rect totalRect;

        private Vector3 bodyPosition = Vector3.zero;
        private Vector2 m_PreviewDir = new Vector2(-30, -30); //new Vector2(120f, -20f);
        private Vector3 m_PivotPositionOffset = new Vector3(0, 0.5f, 0); //Vector3.zero;
        private float m_BoundingVolumeScale = 1;
        private float m_ZoomFactor = 1f;
        private float m_AvatarScale = 1f;
        private ViewTool m_ViewTool;
        private bool m_ShowReference = false;
        private bool m_AniLoop = false;
        private bool m_ShowCollider = false;
        private PreviewCameraDrawLineBounds previewCameraDrawLineBounds;
        private UnitCFG unitCfg;
        private AnimatorClipRef animatorClipRef;
        public PreviewSystem()
        {

        }

        private void Init()
        {

            if (this.m_PreviewUtility == null)
            {
                this.m_PreviewUtility = new PreviewRenderUtility(true);
                this.m_PreviewUtility.cameraFieldOfView = 30f;
                this.m_PreviewUtility.camera.cullingMask = 1 << EditorUtils.PreviewCullingLayer;
                this.m_PreviewUtility.camera.allowHDR = true;
                this.m_PreviewUtility.camera.allowMSAA = true;

                previewCameraDrawLineBounds = new PreviewCameraDrawLineBounds();
            }

            if (playAnimationEditor == null)
            {
                playAnimationEditor = new PlayAnimationEditor();

                if (parentEditorWindow != null)
                {
                    playAnimationEditor.parentEditorWindow = parentEditorWindow;
                }
            }
            if (playParticleSystemEditor == null)
            {
                playParticleSystemEditor = new PlayParticleSystemEditor();
                if (parentEditorWindow != null)
                {
                    playParticleSystemEditor.parentEditorWindow = parentEditorWindow;
                }
            }

            if (s_Styles == null)
            {
                s_Styles = new Styles();
            }
            if (this.m_FloorPlane == null)
            {
                this.m_FloorPlane = Resources.GetBuiltinResource(typeof(Mesh), "New-Plane.fbx") as Mesh;
            }
            if (this.m_FloorTexture == null)
            {
                this.m_FloorTexture = (Texture2D) EditorGUIUtility.Load("Avatar/Textures/AvatarFloor.png");
            }
            if (this.m_FloorMaterial == null)
            {
                Shader shader = EditorGUIUtility.LoadRequired("Previews/PreviewPlaneWithShadow.shader") as Shader;
                this.m_FloorMaterial = new Material(shader);
                this.m_FloorMaterial.mainTexture = this.m_FloorTexture;
                this.m_FloorMaterial.mainTextureScale = (Vector2.one * 5f) * 4f;
                this.m_FloorMaterial.SetVector("_Alphas", new Vector4(0.5f, 0.3f, 0f, 0f));
                this.m_FloorMaterial.hideFlags = HideFlags.HideAndDontSave;
                this.m_FloorMaterialSmall = new Material(this.m_FloorMaterial);
                this.m_FloorMaterialSmall.mainTextureScale = (Vector2.one * 0.2f) * 4f;
                this.m_FloorMaterialSmall.hideFlags = HideFlags.HideAndDontSave;
            }
            if (this.m_ShadowMaskMaterial == null)
            {
                Shader shader2 = EditorGUIUtility.LoadRequired("Previews/PreviewShadowMask.shader") as Shader;
                this.m_ShadowMaskMaterial = new Material(shader2);
                this.m_ShadowMaskMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            if (this.m_ShadowPlaneMaterial == null)
            {
                Shader shader3 = EditorGUIUtility.LoadRequired("Previews/PreviewShadowPlaneClip.shader") as Shader;
                this.m_ShadowPlaneMaterial = new Material(shader3);
                this.m_ShadowPlaneMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            if (this.m_ReferenceInstance == null)
            {
                GameObject original = (GameObject) EditorGUIUtility.Load("Avatar/dial_flat.prefab");
                this.m_ReferenceInstance =
                    (GameObject) GameObject.Instantiate(original, Vector3.zero, Quaternion.identity);
                InitInstantiatedPreviewRecursive(this.m_ReferenceInstance);
                //this.previewUtility.AddSingleGO(this.m_ReferenceInstance);
            }
            if (this.m_RootInstance == null)
            {
                GameObject obj6 = (GameObject) EditorGUIUtility.Load("Avatar/root.fbx");
                this.m_RootInstance = (GameObject) GameObject.Instantiate(obj6, Vector3.zero, Quaternion.identity);
                InitInstantiatedPreviewRecursive(this.m_RootInstance);
                //this.previewUtility.AddSingleGO(this.m_RootInstance);
            }
            if (m_NameInstance == null)
            {
                this.m_NameInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                m_NameInstance.transform.localScale = Vector3.one * 0.1f;
                InitInstantiatedPreviewRecursive(this.m_NameInstance);
                //this.previewUtility.AddSingleGO(this.m_NameInstance);
            }
            this.SetPreviewCharacterEnabled(false, false);
        }

        private static void InitInstantiatedPreviewRecursive(GameObject go)
        {
            go.hideFlags = HideFlags.HideAndDontSave;
            go.layer = EditorUtils.PreviewCullingLayer;
            foreach (Transform transform in go.transform)
            {
                InitInstantiatedPreviewRecursive(transform.gameObject);
            }
        }


        public void OnDestroy()
        {
            if (m_PreviewUtility != null)
            {
                m_PreviewUtility.Cleanup();
                m_PreviewUtility = null;
            }
            if (previewInstance != null)
            {
                GameObject.DestroyImmediate(previewInstance);
            }
            if (m_ReferenceInstance != null)
            {
                GameObject.DestroyImmediate(m_ReferenceInstance);
                GameObject.DestroyImmediate(m_RootInstance);
                GameObject.DestroyImmediate(m_NameInstance);
            }
            if (m_ShadowMaskMaterial != null)
            {
                GameObject.DestroyImmediate(m_ShadowMaskMaterial);
                GameObject.DestroyImmediate(m_ShadowPlaneMaterial);
                GameObject.DestroyImmediate(m_FloorMaterial);
                GameObject.DestroyImmediate(m_FloorMaterialSmall);
            }

            if (shadowTexture != null)
            {
                RenderTexture.ReleaseTemporary(shadowTexture);
            }
        }

        public void OnGUI()
        {
            previewRect = GUILayoutUtility.GetRect(500, 500);
        }

        private RenderTexture shadowTexture;
        public Vector2 aniScrollPosition = Vector2.zero;
        public Rect aniTotalRect = new Rect(0,0,1,0);
        public void DrawRect(Rect rect)
        {
            Init();
            this.totalRect = rect;
            this.previewRect = rect;

            if (this.previewRect.width < 10)
            {
                this.previewRect.width = 10;
            }
            if (this.previewRect.height < 10)
            {
                this.previewRect.height = 10;
            }

            Event e = Event.current;
            m_PreviewUtility.BeginPreview(previewRect, (GUIStyle) "PreBackground");
            bool oldFog = SetupPreviewLightingAndFx();

            Vector3 floorPos = new Vector3(0f, 0f, 0f);
            if (previewInstance != null)
            {
                bodyPosition = EditorUtils.GetRenderableCenterRecurse(previewInstance, 2, 8);
                m_RootInstance.transform.position = bodyPosition;
                m_RootInstance.transform.rotation = previewInstance.transform.rotation;
                m_RootInstance.transform.localScale = (Vector3.one * m_AvatarScale) * 0.5f;

                m_ReferenceInstance.transform.rotation = previewInstance.transform.rotation;
                Vector3 namePosition = bodyPosition;
                if (unitCfg != null)
                {
                    namePosition.y = unitCfg.nameY;
                }
                else
                {
                    namePosition.y = 0;
                }
                m_NameInstance.transform.position = namePosition;
            }

            Matrix4x4 matrixx;
            if (shadowTexture != null)
            {
                RenderTexture.ReleaseTemporary(shadowTexture);
            }
            shadowTexture = this.RenderPreviewShadowmap(this.m_PreviewUtility.lights[0],
                this.m_BoundingVolumeScale / 2f, bodyPosition, floorPos, out matrixx);

            this.m_PreviewUtility.camera.nearClipPlane = 0.5f * m_ZoomFactor;
            this.m_PreviewUtility.camera.farClipPlane = 100f * m_AvatarScale;
            Quaternion quaternion6 = Quaternion.Euler(-this.m_PreviewDir.y, -this.m_PreviewDir.x, 0f);
            Vector3 vector7 = quaternion6 * Vector3.forward * -5.5f * this.m_ZoomFactor + this.m_PivotPositionOffset;
            this.m_PreviewUtility.camera.transform.position = vector7;
            this.m_PreviewUtility.camera.transform.rotation = quaternion6;


            Quaternion identity = Quaternion.identity;
            Material floorMaterial = this.m_FloorMaterial;
            Matrix4x4 matrix = Matrix4x4.TRS(floorPos, identity, (Vector3.one * 5f) * this.m_AvatarScale);
            floorMaterial.mainTextureOffset =
                ((-new Vector2(floorPos.x, floorPos.z) * 5f) * 0.08f) * (1f / this.m_AvatarScale);
            floorMaterial.SetTexture("_ShadowTexture", shadowTexture);
            floorMaterial.SetMatrix("_ShadowTextureMatrix", matrixx);
            floorMaterial.SetVector("_Alphas", new Vector4(0.5f, 0.3f, 0f, 0f));
            Graphics.DrawMesh(this.m_FloorPlane, matrix, floorMaterial, EditorUtils.PreviewCullingLayer,
                this.m_PreviewUtility.camera, 0);


            SetPreviewCharacterEnabled(true, m_ShowReference);
            m_PreviewUtility.camera.Render();
            if (m_ShowCollider)
            {
                previewCameraDrawLineBounds.Update(m_PreviewUtility.camera);
            }

            SetPreviewCharacterEnabled(false, false);
            TeardownPreviewLightingAndFx(oldFog);
            m_PreviewUtility.EndAndDrawPreview(previewRect);


            Rect uiRect = previewRect;
            uiRect.x += 5;
            uiRect.y += 5;
            uiRect.height = 20;
            uiRect.width = 80;
            m_ShowReference = GUI.Toggle(uiRect, m_ShowReference, "helper");
            uiRect.y += 20;

            aniScrollPosition = GUI.BeginScrollView(new Rect(uiRect.x,uiRect.y,110,previewRect.height-uiRect.y), aniScrollPosition, aniTotalRect);

            uiRect.y = uiRect.x = 0;
            foreach (AnimationClip animationClip in animationClips)
            {
                if (animationClip == null)
                {
                    GUI.color = Color.red;
                    GUI.Button(uiRect, "miss", EditorStyles.miniButton);
                }
                else
                {
                    GUI.color = Color.white;
                    string clipName = animationClip.name;
                    if (clipName == playingClipseName)
                    {
                        GUI.color = new Color(0, 1f, 1f, 1f);
                    }

                    if (GUI.Button(uiRect, clipName, EditorStyles.miniButton))
                    {
                        string path = AssetDatabase.GetAssetPath(animationClip);
                        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        Selection.activeGameObject = go;
                        EditorGUIUtility.PingObject(go);

                        playAnimation(animationClip);
                        e.Use();
                        return;
                    }
                    AnimationClipSettings clipSetting = AnimationUtility.GetAnimationClipSettings(animationClip);
                    uiRect.x += 80;
                    EditorGUI.BeginChangeCheck();
                    GUI.Toggle(uiRect, clipSetting.loopTime, "");
                    if (EditorGUI.EndChangeCheck())
                    {
                        Selection.activeObject = animationClip;
                        EditorGUIUtility.PingObject(animationClip);
                    }
                    uiRect.x -= 80;
                    if (clipName == "idle" && autoDefaultClipse)
                    {
                        autoDefaultClipse = false;
                        playAnimation(animationClip);
                    }
                }
                uiRect.y += 25;
                GUI.color = Color.white;
            }
           

            if (animatorClipRef!=null)
            {
                Rect tempRect = previewRect;
                tempRect.width = 250;
                tempRect.height = 16;
                tempRect.x += 120;
                tempRect.y += 30;
                EditorGUI.BeginChangeCheck();

                if (animatorClipRef.controller == null)
                {
                    GUI.color = Color.red;
                }
                else
                {
                    GUI.color = new Color(0, 1, 1, 1);
                }
                RuntimeAnimatorController controller = EditorGUI.ObjectField(tempRect, animatorClipRef.controller,
                    typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(animatorClipRef, "resetAnimatorClipRef");
                    animatorClipRef.controller = controller;
                    EditorUtility.SetDirty(previewPrefab);
                }
                GUI.color = Color.white;
            }


            if (autoDefaultClipse)
            {
                foreach (AnimationClip animationClip in animationClips)
                {
                    if (animationClip == null)
                    {
                        continue;
                    }
                    if (animationClip.name.ToLower().IndexOf("idle") != -1 || animationClips.Count == 1)
                    {
                        autoDefaultClipse = false;
                        playAnimation(animationClip);
                        break;
                    }
                }
            }

            rotationY = GUI.HorizontalSlider(uiRect, rotationY, 0, 360);
            if (GUI.changed && previewInstance || isForceChange)
            {
                Vector3 rt = previewInstance.transform.eulerAngles;
                rt.y = rotationY;
                previewInstance.transform.eulerAngles = rt;
            }
            uiRect.y += 35;
            if (particleSystems.Length > 0)
            {
                foreach (ParticleSystem particleSystem in particleSystems)
                {
                    if (GUI.Button(uiRect, particleSystem.name))
                    {
                        playParticleSystemEditor.Play(particleSystem, previewInstance);
                    }
                    uiRect.y += 25;
                }
                if (GUI.Button(uiRect, "所有粒子"))
                {
                    playParticleSystemEditor.Play(particleSystems.ToList(), previewInstance);
                }
                uiRect.y += 25;
                if (GUI.Button(uiRect, "停止粒子"))
                {
                    playParticleSystemEditor.Stop();
                }
                uiRect.y += 25;
            }
            aniTotalRect.height = uiRect.y;
            GUI.EndScrollView();

            if (renderers.Length > 0)
            {
                Rect rect2 = totalRect;
                rect2.y += 5;
                rect2.x = totalRect.xMax - 205;
                rect2.width = 200;
                rect2.height = 40;
                EditorGUILayout.BeginVertical(GUILayout.Width(100));

                int bonesCount = 0;
                int materialIndex =0;
                foreach (Renderer renderer in renderers)
                {
                    Mesh mesh = null;
                    Material material = null;
                    if (renderer is SkinnedMeshRenderer)
                    {
                        mesh = (renderer as SkinnedMeshRenderer).sharedMesh;
                        bonesCount = (renderer as SkinnedMeshRenderer).bones.Length;
                        material = (renderer as SkinnedMeshRenderer).sharedMaterials[0];
                    }
                    else if (renderer is MeshRenderer)
                    {
                        MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                        if (meshFilter != null)
                        {
                            mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                        }
                        material = (renderer as MeshRenderer).sharedMaterials[0];
                    }

                    if (mesh == null)
                    {
                        continue;
                    }

                    string name = mesh.name.Replace("(Clone)", "");
                    if (material != null)
                    {
                        name += "\nshader:" + material.shader.name;
                    }
                    name += "\nbone:" + bonesCount + " triangle:" + mesh.triangles.Length / 3;

                    if (material == currentSelectedMaterial)
                    {
                        GUI.color = new Color(0, 1f, 1f, 1f);
                    }
                    else if (unitCfg != null && unitCfg.hasTexeture)
                    {
                        GUI.color = new Color(0, 1f, 0.5f, 1f);
                    }
                    if (GUI.Button(rect2, name))
                    {
                        currentSelectedIndexMaterial = materialIndex;
                        currentSelectedMaterial = material;
                        if (selectedRenderer != null)
                        {
                            EditorUtility.SetSelectedRenderState(selectedRenderer, EditorSelectedRenderState.Hidden);
                        }
                        EditorUtility.SetSelectedRenderState(renderer, EditorSelectedRenderState.Wireframe);
                        Selection.activeTransform = renderer.transform;
                        selectedRenderer = renderer;
                    }
                    GUI.color = Color.white;
                    rect2.y += 45;
                    materialIndex++;
                }

                EditorGUILayout.EndVertical();
            }


            if (animationClips.Count > 0)
            {
                uiRect = previewRect;
                uiRect.y += 5;
                uiRect.x += 80;
                uiRect.height = 20;
                uiRect.width = 80;

                EditorGUI.BeginChangeCheck();
                m_AniLoop = GUI.Toggle(uiRect, m_AniLoop, "aniLoop");
                if (EditorGUI.EndChangeCheck())
                {
                    playAnimationEditor.isLooping = m_AniLoop;
                }
            }

            uiRect = previewRect;
            uiRect.y += 5;
            uiRect.x += 160;
            uiRect.height = 20;
            uiRect.width = 80;
            m_ShowCollider = GUI.Toggle(uiRect, m_ShowCollider, "collider");


            Rect textRect = previewRect;
            textRect.width = textRect.height = 100;
            textRect.x += 150;
            textRect.y = previewRect.height - 110;
            if (currentSelectedMaterial && unitCfg && unitCfg.hasTexeture)
            {
                List<TextureSet> textSets = unitCfg.getTextureSets(currentSelectedIndexMaterial);
                if (textSets == null)
                {
                    textSets = unitCfg.getTextureSets(0);
                }

                if (textSets != null)
                {
                    foreach (TextureSet textureSet in textSets)
                    {
                        Texture texture = textureSet.texture;
                        if (texture != null)
                        {
                            if (GUI.Button(textRect, texture))
                            {
                                chageMainTexture(texture);
                            }
                            textRect.x += 110;
                        }
                    }
                }
            }

            DoAvatarPreviewDrag(e);
            HandleViewTool(e);
            if (e.type == EventType.Repaint)
            {
                Rect cursorRect = previewRect;
                cursorRect.x += 100;
                cursorRect.width -= 200;
                EditorGUIUtility.AddCursorRect(previewRect, this.currentCursor);
            }

            isForceChange = false;
        }

        private void chageMainTexture(Texture texture)
        {
            if (currentSelectedMaterial && texture)
            {
                Undo.RegisterCompleteObjectUndo(currentSelectedMaterial, "resetMainText");
                currentSelectedMaterial.mainTexture = texture;
            }
        }

      

        private bool SetupPreviewLightingAndFx()
        {
            m_PreviewUtility.lights[0].intensity = 1.4f;
            m_PreviewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0f);
            m_PreviewUtility.lights[1].intensity = 1.4f;
            Color ambient = new Color(0.1f, 0.1f, 0.1f, 0f);
            InternalEditorUtility.SetCustomLighting(m_PreviewUtility.lights, ambient);
            bool fog = RenderSettings.fog;
            Unsupported.SetRenderSettingsUseFogNoDirty(false);
            return fog;
        }

        private static void TeardownPreviewLightingAndFx(bool oldFog)
        {
            Unsupported.SetRenderSettingsUseFogNoDirty(oldFog);
            InternalEditorUtility.RemoveCustomLighting();
        }

        private void playAnimation(AnimationClip animationClip)
        {
            playingClipseName = animationClip.name;
            playAnimationEditor.Play(animationClip, previewInstance, m_AniLoop);
        }

        public void DoAvatarPreviewDrag(Event e)
        {
            EventType type = e.type;
            if (type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            }
            else if (type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                GameObject gameObject = DragAndDrop.objectReferences[0] as GameObject;
                if (gameObject != null)
                {
                    DragAndDrop.AcceptDrag();
                    this.SetPreview(gameObject);
                }
            }
        }

        protected void HandleViewTool(Event e)
        {
            EventType type = e.type;
            switch (type)
            {
                case EventType.MouseDown:
                    if (previewRect.Contains(e.mousePosition) && (this.viewTool != ViewTool.None))
                    {
                        EditorGUIUtility.SetWantsMouseJumping(1);
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    this.m_ViewTool = ViewTool.None;
                    e.Use();
                    break;

                case EventType.MouseDrag:
                    switch (m_ViewTool)
                    {
                        case ViewTool.Orbit:

                            this.m_PreviewDir -=
                                (Vector2) (e.delta / Mathf.Min(previewRect.width, previewRect.height) * 140f);
                            this.m_PreviewDir.y = Mathf.Clamp(this.m_PreviewDir.y, -90f, 90f);
                            e.Use();
                            break;
                        case ViewTool.Pan:

                            Camera camera = this.m_PreviewUtility.camera;
                            Vector3 position =
                                camera.WorldToScreenPoint(this.bodyPosition + this.m_PivotPositionOffset);
                            Vector3 vector2 = new Vector3(-e.delta.x, e.delta.y, 0f);
                            position += (Vector3) (vector2 * Mathf.Lerp(0.25f, 2f, this.m_ZoomFactor * 0.5f));
                            Vector3 vector3 = camera.ScreenToWorldPoint(position) -
                                              (this.bodyPosition + this.m_PivotPositionOffset);
                            this.m_PivotPositionOffset += vector3;
                            e.Use();
                            break;
                    }
                    break;

                case EventType.ScrollWheel:
                    float num = -HandleUtility.niceMouseDeltaZoom * 0.05f;
                    this.m_ZoomFactor += this.m_ZoomFactor * num;
                    this.m_ZoomFactor = Mathf.Max(this.m_ZoomFactor, this.m_AvatarScale / 10f);
                    e.Use();
                    break;
            }
        }

        private Camera CameraUI;
        private bool isForceChange = false;
        private bool autoDefaultClipse = true;
        private string playingClipseName;
        private Material currentSelectedMaterial;
        private int currentSelectedIndexMaterial=-1;

        public void SetPreview(GameObject prefab)
        {
            currentSelectedMaterial = null;
            currentSelectedIndexMaterial = -1;
            isForceChange = true;
            previewPrefab = prefab;
            unitCfg = null;
            animatorClipRef = null;
            if (previewInstance != null)
            {
                GameObject.DestroyImmediate(previewInstance);
            }
            if (playAnimationEditor != null)
            {
                playParticleSystemEditor.Stop();
            }
            if (playAnimationEditor != null)
            {
                playAnimationEditor.Stop();
            }
            if (previewPrefab != null)
            {
                RectTransform rectTransform = previewPrefab.GetComponent<RectTransform>();
                unitCfg = previewPrefab.GetComponent<UnitCFG>();
                if (rectTransform != null)
                {
                    GameObject uiCamera = new GameObject("UICamera");
                    CameraUI = uiCamera.AddComponent<Camera>();
                    CameraUI.clearFlags = CameraClearFlags.Depth;
                    CameraUI.cullingMask = 1 << EditorUtils.PreviewCullingLayer;
                    CameraUI.orthographic = true;
                    CameraUI.orthographicSize = 5;
                    CameraUI.nearClipPlane = -10;
                    CameraUI.farClipPlane = 100;

                    GameObject uiCanvas = new GameObject("Canvas");
                    uiCanvas.transform.SetParent(uiCamera.transform);

                    uiCanvas.AddComponent<RectTransform>();
                    Canvas canvas = uiCanvas.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.pixelPerfect = true;
                    canvas.worldCamera = CameraUI;
                    canvas.planeDistance = 10;
                    canvas.sortingLayerName = "UI";

                    CanvasScaler canvasScaler = uiCanvas.AddComponent<CanvasScaler>();
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

                    Camera mainCamera = EditorUtils.GetMainCamera();

                    canvasScaler.referenceResolution = new Vector2(mainCamera.pixelWidth, mainCamera.pixelHeight);
                    canvasScaler.referencePixelsPerUnit = 100f;

                    GameObject go =
                        GameObject.Instantiate(previewPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    if (go.activeSelf == false)
                    {
                        go.SetActive(true);
                    }
                    InitInstantiatedPreviewRecursive(go);
                    //this.m_PreviewUtility.AddSingleGO(go);
                    go.transform.SetParent(uiCanvas.transform, false);
                    go.SetActive(true);
                    previewInstance = uiCamera;
                }
                else
                {
                    CameraUI = null;
                    previewInstance = GameObject.Instantiate(previewPrefab) as GameObject;
                    if (previewInstance.activeSelf == false)
                    {
                        previewInstance.SetActive(true);
                    }
                    previewInstance.transform.localPosition = Vector3.zero;
                    previewCameraDrawLineBounds.SetView(previewInstance, previewPrefab);
                }
                InitInstantiatedPreviewRecursive(previewInstance);

                Bounds bounds = new Bounds(this.previewInstance.transform.position, Vector3.zero);
                EditorUtils.GetRenderableBoundsRecurse(ref bounds, this.previewInstance);
                this.m_BoundingVolumeScale = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
                if (this.m_BoundingVolumeScale <= 0)
                {
                    this.m_BoundingVolumeScale = 1;
                }
                this.m_AvatarScale = this.m_ZoomFactor = this.m_BoundingVolumeScale / 2f;

                animationClips.Clear();
                animatorClipRef = previewPrefab.GetComponentInChildren<AnimatorClipRef>();
                if (animatorClipRef != null)
                {
                    foreach (AnimationClip clip in animatorClipRef.animationClips)
                    {
                        animationClips.Add(clip);
                    }
                }
                else
                {
                    Animator animator = previewPrefab.GetComponentInChildren<Animator>();
                    if (animator != null)
                    {
                        RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;
                        if (runtimeAnimatorController != null)
                        {
                            foreach (AnimationClip clip in runtimeAnimatorController.animationClips)
                            {
                                animationClips.Add(clip);
                            }
                        }
                    }
                }
                autoDefaultClipse = true;
                particleSystems = previewInstance.GetComponentsInChildren<ParticleSystem>();
                renderers = previewInstance.GetComponentsInChildren<Renderer>();
            }
        }

        public GameObject GetPreview()
        {
            return previewInstance;
        }


        public void OnEnable()
        {
        }

        public void OnDisable()
        {
            if (shadowTexture != null)
            {
                RenderTexture.ReleaseTemporary(shadowTexture);
                shadowTexture = null;
            }
            if (playAnimationEditor != null)
            {
                playAnimationEditor.Stop();
            }

            if (playParticleSystemEditor != null)
            {
                playParticleSystemEditor.Stop();
            }
            if (previewInstance != null)
            {
                GameObject.DestroyImmediate(previewInstance);
                previewInstance = null;
            }

            this.m_ViewTool = ViewTool.None;
        }

        protected ViewTool viewTool
        {
            get
            {
                Event current = Event.current;
                if (this.m_ViewTool == ViewTool.None)
                {
                    //bool flag = current.control && (Application.platform == RuntimePlatform.OSXEditor);
                    bool actionKey = EditorGUI.actionKey;
                    if (current.button == 2)
                    {
                        this.m_ViewTool = ViewTool.Pan;
                    }
                    else if (current.button == 1)
                    {
                        this.m_ViewTool = ViewTool.Orbit;
                    }
                }
                return this.m_ViewTool;
            }
        }

        protected MouseCursor currentCursor
        {
            get
            {
                switch (this.m_ViewTool)
                {
                    case ViewTool.Pan:
                        return MouseCursor.Pan;

                    case ViewTool.Zoom:
                        return MouseCursor.Zoom;

                    case ViewTool.Orbit:
                        return MouseCursor.Orbit;
                }
                return MouseCursor.Arrow;
            }
        }

        private RenderTexture RenderPreviewShadowmap(Light light, float scale, Vector3 center, Vector3 floorPos,
            out Matrix4x4 outShadowMatrix)
        {
            Camera camera = this.m_PreviewUtility.camera;
            camera.orthographic = true;
            camera.orthographicSize = scale * 2f;
            camera.nearClipPlane = 1f * scale;
            camera.farClipPlane = 25f * scale;
            camera.transform.rotation = light.transform.rotation;
            camera.transform.position = center - ((Vector3) (light.transform.forward * (scale * 5.5f)));
            CameraClearFlags clearFlags = camera.clearFlags;
            camera.clearFlags = CameraClearFlags.Color;
            Color backgroundColor = camera.backgroundColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            RenderTexture targetTexture = camera.targetTexture;
            RenderTexture texture2 = RenderTexture.GetTemporary(0x100, 0x100, 0x10);
            texture2.isPowerOfTwo = true;
            texture2.wrapMode = TextureWrapMode.Clamp;
            texture2.filterMode = FilterMode.Bilinear;
            camera.targetTexture = texture2;
            this.SetPreviewCharacterEnabled(true, false);
            this.m_PreviewUtility.camera.Render();
            RenderTexture.active = texture2;
            GL.PushMatrix();
            GL.LoadOrtho();
            this.m_ShadowMaskMaterial.SetPass(0);
            GL.Begin(7);
            GL.Vertex3(0f, 0f, -99f);
            GL.Vertex3(1f, 0f, -99f);
            GL.Vertex3(1f, 1f, -99f);
            GL.Vertex3(0f, 1f, -99f);
            GL.End();
            GL.LoadProjectionMatrix(camera.projectionMatrix);
            GL.LoadIdentity();
            GL.MultMatrix(camera.worldToCameraMatrix);
            this.m_ShadowPlaneMaterial.SetPass(0);
            GL.Begin(7);
            float x = 5f * scale;
            GL.Vertex(floorPos + new Vector3(-x, 0f, -x));
            GL.Vertex(floorPos + new Vector3(x, 0f, -x));
            GL.Vertex(floorPos + new Vector3(x, 0f, x));
            GL.Vertex(floorPos + new Vector3(-x, 0f, x));
            GL.End();
            GL.PopMatrix();
            Matrix4x4 matrixx = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity,
                new Vector3(0.5f, 0.5f, 0.5f));
            outShadowMatrix = (matrixx * camera.projectionMatrix) * camera.worldToCameraMatrix;
            camera.orthographic = false;
            camera.clearFlags = clearFlags;
            camera.backgroundColor = backgroundColor;
            camera.targetTexture = targetTexture;
            return texture2;
        }

        private void SetPreviewCharacterEnabled(bool enabled, bool showReference)
        {
            if (previewInstance != null)
            {
                this.previewInstance.SetRendererEnabledRecursive(enabled);
            }
            this.m_ReferenceInstance.SetRendererEnabledRecursive(showReference && enabled);
            this.m_NameInstance.SetRendererEnabledRecursive(showReference);
            this.m_RootInstance.SetRendererEnabledRecursive(showReference && enabled);
        }

    }
}