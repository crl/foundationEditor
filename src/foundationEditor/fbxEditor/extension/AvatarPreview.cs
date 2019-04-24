namespace UnityEditor
{
    using foundationEditor;
    using System;
    using System.Globalization;
    using UnityEditor.Animations;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.Rendering;
    using Object = UnityEngine.Object;

    internal class AvatarPreview
    {
        public int fps = 60;
        private const string k2DPref = "Avatarpreview2D";
        private const float kFloorAlpha = 0.5f;
        private const float kFloorFadeDuration = 0.2f;
        private const float kFloorScale = 5f;
        private const float kFloorScaleSmall = 0.2f;
        private const float kFloorShadowAlpha = 0.3f;
        private const float kFloorTextureScale = 4f;
        private const string kIkPref = "AvatarpreviewShowIK";
        private const string kReferencePref = "AvatarpreviewShowReference";
        private const string kSpeedPref = "AvatarpreviewSpeed";
        private const float kTimeControlRectHeight = 21f;
        private bool m_2D;
        private float m_AvatarScale = 1f;
        private float m_BoundingVolumeScale;
        private GameObject m_DirectionInstance;
        private Material m_FloorMaterial;
        private Material m_FloorMaterialSmall;
        private Mesh m_FloorPlane;
        private Texture2D m_FloorTexture;
        private bool m_IKOnFeet = false;
        private bool m_IsValid;
        private float m_LastNormalizedTime = -1000f;
        private float m_LastStartTime = -1000f;
        private float m_LastStopTime = -1000f;
        private float m_NextFloorHeight = 0f;
        private bool m_NextTargetIsForward = true;
        private OnAvatarChange m_OnAvatarChangeFunc = null;
        private PreviewPopupOptions m_Option;
        private GameObject m_PivotInstance;
        public Vector3 m_PivotPositionOffset = Vector3.zero;
        private float m_PrevFloorHeight = 0f;
        private Vector2 m_PreviewDir = new Vector2(120f, -20f);
        private int m_PreviewHint = "Preview".GetHashCode();
        private GameObject m_PreviewInstance;
        private int m_PreviewSceneHint = "PreviewSene".GetHashCode();
        public PreviewRenderUtility m_PreviewUtility;
        private GameObject m_ReferenceInstance;
        private GameObject m_RootInstance;
        private Material m_ShadowMaskMaterial;
        private Material m_ShadowPlaneMaterial;
        private bool m_ShowIKOnFeetButton = true;
        private bool m_ShowReference = false;
        private Motion m_SourcePreviewMotion;
        private UnityEngine.Animator m_SourceScenePreviewAnimator;
        protected ViewTool m_ViewTool = ViewTool.None;
        private float m_ZoomFactor = 1f;
        private const string s_PreviewSceneStr = "PreviewSene";
        private const string s_PreviewStr = "Preview";
        private static Styles s_Styles;
        public TimeControl timeControl;

        public AvatarPreview(UnityEngine.Animator previewObjectInScene, Motion objectOnSameAsset)
        {
            this.InitInstance(previewObjectInScene, objectOnSameAsset);
        }

        public void AvatarTimeControlGUI(Rect rect)
        {
            Rect rect2 = rect;
            rect2.height = 21f;
            this.timeControl.DoTimeControl(rect2);
            rect.y = rect.yMax - 20f;
            float num = this.timeControl.currentTime - this.timeControl.startTime;
            object[] args = new object[] { (int)num, this.Repeat(Mathf.FloorToInt(num * this.fps), this.fps), this.timeControl.normalizedTime, Mathf.FloorToInt(this.timeControl.currentTime * this.fps) };
            EditorGUI.DropShadowLabel(new Rect(rect.x, rect.y, rect.width, 20f), String.Format("{0,2}:{1:00} ({2:000.0%}) Frame {3}", args));
        }

        private static GameObject CalculatePreviewGameObject(UnityEngine.Animator selectedAnimator, Motion motion, ModelImporterAnimationType animationType)
        {
            AnimationClip firstAnimationClipFromMotion = GetFirstAnimationClipFromMotion(motion);
            GameObject preview = AvatarPreviewSelection.GetPreview(animationType);
            if (IsValidPreviewGameObject(preview, ModelImporterAnimationType.None))
            {
                return preview;
            }
            if ((selectedAnimator != null) && IsValidPreviewGameObject(selectedAnimator.gameObject, animationType))
            {
                return selectedAnimator.gameObject;
            }
            preview = FindBestFittingRenderableGameObjectFromModelAsset(firstAnimationClipFromMotion, animationType);
            if (preview != null)
            {
                return preview;
            }
            if (animationType == ModelImporterAnimationType.Human)
            {
                return GetHumanoidFallback();
            }
            if (animationType == ModelImporterAnimationType.Generic)
            {
                return GetGenericAnimationFallback();
            }
            return null;
        }

        public void DoAvatarPreview(Rect rect, GUIStyle background)
        {
            this.Init();
            /*Rect position = new Rect(rect.xMax - 16f, rect.yMax - 16f, 16f, 16f);
            if (EditorGUI.DropdownButton(position, GUIContent.none, FocusType.Passive, GUIStyle.none))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(EditorGUIUtility.TrTextContent("Auto", null, (Texture)null), false, new GenericMenu.MenuFunction2(this.SetPreviewAvatarOption), PreviewPopupOptions.Auto);
                menu.AddItem(EditorGUIUtility.TrTextContent("Unity Model", null, (Texture)null), false, new GenericMenu.MenuFunction2(this.SetPreviewAvatarOption), PreviewPopupOptions.DefaultModel);
                menu.AddItem(EditorGUIUtility.TrTextContent("Other...", null, (Texture)null), false, new GenericMenu.MenuFunction2(this.SetPreviewAvatarOption), PreviewPopupOptions.Other);
                menu.ShowAsContext();
            }*/
            Rect rect3 = rect;
            rect3.yMin += 21f;
            rect3.height = Mathf.Max(rect3.height, 64f);
            int controlID = GUIUtility.GetControlID(this.m_PreviewHint, FocusType.Passive, rect3);
            Event current = Event.current;
            if ((current.GetTypeForControl(controlID) == EventType.Repaint) && this.m_IsValid)
            {
                this.DoRenderPreview(rect3, background);
                this.previewUtility.EndAndDrawPreview(rect3);
            }

            this.AvatarTimeControlGUI(rect);
            // GUI.DrawTexture(position, s_Styles.avatarIcon.image);
            int num2 = GUIUtility.GetControlID(this.m_PreviewSceneHint, FocusType.Passive);
            EventType typeForControl = current.GetTypeForControl(num2);
            this.DoAvatarPreviewDrag(typeForControl);

            //把区域让出来
            rect3.y += 50;
            rect3.x += 130;
            rect3.width -= 330;
            this.HandleViewTool(current, typeForControl, num2, rect3);
            this.DoAvatarPreviewFrame(current, typeForControl, rect3);
            if (!this.m_IsValid)
            {
                Rect rect4 = rect3;
                rect4.yMax -= (rect4.height / 2f) - 16f;
                EditorGUI.DropShadowLabel(rect4, "No model is available for preview.\nPlease drag a model into this Preview Area.");
            }
            if (current.type == EventType.Repaint)
            {
                EditorGUIUtility.AddCursorRect(rect3, this.currentCursor);
            }
        }

        public void DoAvatarPreviewDrag(EventType type)
        {
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

        public void DoAvatarPreviewFrame(Event evt, EventType type, Rect previewRect)
        {
            if ((type == EventType.KeyDown) && (evt.keyCode == KeyCode.F))
            {
                this.ResetPreviewFocus();
                this.m_ZoomFactor = this.m_AvatarScale;
                evt.Use();
            }
            if ((type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.G))
            {
                this.m_PivotPositionOffset = this.GetCurrentMouseWorldPosition(evt, previewRect) - this.bodyPosition;
                evt.Use();
            }
        }

        public void DoAvatarPreviewOrbit(Event evt, Rect previewRect)
        {
            if (this.is2D)
            {
                this.is2D = false;
            }
            this.m_PreviewDir -= (Vector2)(((evt.delta * (!evt.shift ? ((float)1) : ((float)3))) / Mathf.Min(previewRect.width, previewRect.height)) * 140f);
            this.m_PreviewDir.y = Mathf.Clamp(this.m_PreviewDir.y, -90f, 90f);
            evt.Use();
        }

        public void DoAvatarPreviewPan(Event evt)
        {
            Camera camera = this.previewUtility.camera;
            Vector3 position = camera.WorldToScreenPoint(this.bodyPosition + this.m_PivotPositionOffset);
            Vector3 vector2 = new Vector3(-evt.delta.x, evt.delta.y, 0f);
            position += (Vector3)(vector2 * Mathf.Lerp(0.25f, 2f, this.m_ZoomFactor * 0.5f));
            Vector3 vector5 = camera.ScreenToWorldPoint(position) - (this.bodyPosition + this.m_PivotPositionOffset);
            this.m_PivotPositionOffset += vector5;
            evt.Use();
        }

        public void DoAvatarPreviewZoom(Event evt, float delta)
        {
            float num = -delta * 0.05f;
            this.m_ZoomFactor += this.m_ZoomFactor * num;
            this.m_ZoomFactor = Mathf.Max(this.m_ZoomFactor, this.m_AvatarScale / 10f);
            evt.Use();
        }

        public void DoPreviewSettings(Rect position)
        {
            position.height = 20;
            this.Init();

            GUI.Box(position, GUIContent.none, s_Styles.timeScrubber);
            position.y = 2;
            position.x = position.xMax - 230;
            position.width = 30;

            if (this.m_ShowIKOnFeetButton)
            {
                position.x -= 30;
                EditorGUI.BeginChangeCheck();
                this.m_IKOnFeet = GUI.Toggle(position, this.m_IKOnFeet, s_Styles.ik, s_Styles.preButton);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool("AvatarpreviewShowIK", this.m_IKOnFeet);
                }
                position.x += 30;
            }
            EditorGUI.BeginChangeCheck();
            GUI.Toggle(position, this.is2D, s_Styles.is2D, s_Styles.preButton);
            if (EditorGUI.EndChangeCheck())
            {
                this.is2D = !this.is2D;
                EditorPrefs.SetBool("Avatarpreview2D", this.is2D);
            }
            position.x += 30;

            EditorGUI.BeginChangeCheck();
            this.m_ShowReference = GUI.Toggle(position, this.m_ShowReference, s_Styles.pivot, s_Styles.preButton);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("AvatarpreviewShowReference", this.m_ShowReference);
            }
            position.x += 30;

            GUI.Box(position, s_Styles.speedScale, s_Styles.preLabel);
            position.x += 30;

            EditorGUI.BeginChangeCheck();
            position.width = 90;
            this.timeControl.playbackSpeed = this.PreviewSlider(position, this.timeControl.playbackSpeed, 0.03f);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetFloat("AvatarpreviewSpeed", this.timeControl.playbackSpeed);
            }
            position.x += 90;

            position.width = 50;
            GUI.Label(position, this.timeControl.playbackSpeed.ToString("f2", CultureInfo.InvariantCulture.NumberFormat), s_Styles.preLabel);
        }

        public void DoRenderPreview(Rect previewRect, GUIStyle background)
        {
            Quaternion bodyRotation;
            Quaternion rootRotation;
            Vector3 zero;
            Vector3 pivotPosition;
            float prevFloorHeight;
            float num2;
            Matrix4x4 matrixx;
            SphericalHarmonicsL2 ambientProbe = RenderSettings.ambientProbe;
            this.previewUtility.BeginPreview(previewRect, background);
            Vector3 rootPosition = this.rootPosition;
            if ((this.Animator != null) && this.Animator.isHuman)
            {
                rootRotation = this.Animator.rootRotation;
                zero = this.Animator.rootPosition;
                bodyRotation = this.Animator.bodyRotation;
                pivotPosition = this.Animator.pivotPosition;
            }
            else if ((this.Animator != null) && this.Animator.hasRootMotion)
            {
                rootRotation = this.Animator.rootRotation;
                zero = this.Animator.rootPosition;
                bodyRotation = Quaternion.identity;
                pivotPosition = Vector3.zero;
            }
            else
            {
                rootRotation = Quaternion.identity;
                zero = Vector3.zero;
                bodyRotation = Quaternion.identity;
                pivotPosition = Vector3.zero;
            }
            this.SetupPreviewLightingAndFx(ambientProbe);
            Vector3 forward = (Vector3)(bodyRotation * Vector3.forward);
            forward[1] = 0f;
            Quaternion directionRot = Quaternion.LookRotation(forward);
            Vector3 directionPos = zero;
            Quaternion pivotRot = rootRotation;
            this.PositionPreviewObjects(pivotRot, pivotPosition, bodyRotation, this.bodyPosition, directionRot, rootRotation, zero, directionPos, this.m_AvatarScale);
            bool flag = !this.is2D ? (Mathf.Abs((float)(this.m_NextFloorHeight - this.m_PrevFloorHeight)) > (this.m_ZoomFactor * 0.01f)) : false;
            if (flag)
            {
                float num3 = (this.m_NextFloorHeight >= this.m_PrevFloorHeight) ? 0.8f : 0.2f;
                prevFloorHeight = (this.timeControl.normalizedTime >= num3) ? this.m_NextFloorHeight : this.m_PrevFloorHeight;
                num2 = Mathf.Clamp01(Mathf.Abs((float)(this.timeControl.normalizedTime - num3)) / 0.2f);
            }
            else
            {
                prevFloorHeight = this.m_PrevFloorHeight;
                num2 = !this.is2D ? 1f : 0.5f;
            }
            Quaternion q = !this.is2D ? Quaternion.identity : Quaternion.Euler(-90f, 0f, 0f);
            Vector3 position = this.m_ReferenceInstance.transform.position;
            position.y = prevFloorHeight;
            RenderTexture texture = this.RenderPreviewShadowmap(this.previewUtility.lights[0], this.m_BoundingVolumeScale / 2f, this.bodyPosition, position, out matrixx);
            float num4 = !this.is2D ? this.m_ZoomFactor : 1f;
            this.previewUtility.camera.orthographic = this.is2D;
            if (this.is2D)
            {
                this.previewUtility.camera.orthographicSize = 2f * this.m_ZoomFactor;
            }
            this.previewUtility.camera.nearClipPlane = 0.5f * num4;
            this.previewUtility.camera.farClipPlane = 100f * this.m_AvatarScale;
            Quaternion quaternion6 = Quaternion.Euler(-this.m_PreviewDir.y, -this.m_PreviewDir.x, 0f);
            Vector3 vector7 = (((Vector3)(quaternion6 * ((Vector3.forward * -5.5f) * num4))) + rootPosition) + this.m_PivotPositionOffset;
            this.previewUtility.camera.transform.position = vector7;
            this.previewUtility.camera.transform.rotation = quaternion6;
            this.SetPreviewCharacterEnabled(true, this.m_ShowReference);
            this.previewUtility.Render(this.m_Option != PreviewPopupOptions.DefaultModel, true);
            this.SetPreviewCharacterEnabled(false, false);
            Vector2 vector8 = -new Vector2(position.x, !this.is2D ? position.z : position.y);
            Material floorMaterial = this.m_FloorMaterial;
            Matrix4x4 matrix = Matrix4x4.TRS(position, q, (Vector3)((Vector3.one * 5f) * this.m_AvatarScale));
            floorMaterial.mainTextureOffset = (Vector2)(((vector8 * 5f) * 0.08f) * (1f / this.m_AvatarScale));
            floorMaterial.SetTexture("_ShadowTexture", texture);
            floorMaterial.SetMatrix("_ShadowTextureMatrix", matrixx);
            floorMaterial.SetVector("_Alphas", new Vector4(0.5f * num2, 0.3f * num2, 0f, 0f));
            floorMaterial.renderQueue = 0x3e8;
            Graphics.DrawMesh(this.m_FloorPlane, matrix, floorMaterial, EditorUtils.PreviewCullingLayer, this.previewUtility.camera, 0);
            if (flag)
            {
                bool flag2 = this.m_NextFloorHeight > this.m_PrevFloorHeight;
                float b = !flag2 ? this.m_PrevFloorHeight : this.m_NextFloorHeight;
                float a = !flag2 ? this.m_NextFloorHeight : this.m_PrevFloorHeight;
                float num7 = ((b != prevFloorHeight) ? 1f : (1f - num2)) * Mathf.InverseLerp(a, b, zero.y);
                position.y = b;
                Material floorMaterialSmall = this.m_FloorMaterialSmall;
                floorMaterialSmall.mainTextureOffset = (Vector2)((vector8 * 0.2f) * 0.08f);
                floorMaterialSmall.SetTexture("_ShadowTexture", texture);
                floorMaterialSmall.SetMatrix("_ShadowTextureMatrix", matrixx);
                floorMaterialSmall.SetVector("_Alphas", new Vector4(0.5f * num7, 0f, 0f, 0f));
                Matrix4x4 matrixx3 = Matrix4x4.TRS(position, q, (Vector3)((Vector3.one * 0.2f) * this.m_AvatarScale));
                Graphics.DrawMesh(this.m_FloorPlane, matrixx3, floorMaterialSmall, EditorUtils.PreviewCullingLayer, this.previewUtility.camera, 0);
            }
            CameraClearFlags clearFlags = this.previewUtility.camera.clearFlags;
            this.previewUtility.camera.clearFlags = CameraClearFlags.Nothing;
            this.previewUtility.Render(false, true);
            this.previewUtility.camera.clearFlags = clearFlags;
            RenderTexture.ReleaseTemporary(texture);
        }

        public void DoSelectionChange()
        {
            this.m_OnAvatarChangeFunc();
        }

        public static GameObject FindBestFittingRenderableGameObjectFromModelAsset(UnityEngine.Object asset, ModelImporterAnimationType animationType)
        {
            if (asset != null)
            {
                ModelImporter atPath = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as ModelImporter;
                if (atPath == null)
                {
                    return null;
                }
                GameObject target = AssetDatabase.LoadMainAssetAtPath(atPath.CalculateBestFittingPreviewGameObjectX()) as GameObject;
                if (IsValidPreviewGameObject(target, ModelImporterAnimationType.None))
                {
                    return target;
                }
            }
            return null;
        }

        public static ModelImporterAnimationType GetAnimationType(GameObject go)
        {
            Animator component = go.GetComponent<UnityEngine.Animator>();
            if (component != null)
            {
                Avatar avatar = component.avatar;
                if ((avatar != null) && avatar.isHuman)
                {
                    return ModelImporterAnimationType.Human;
                }
                return ModelImporterAnimationType.Generic;
            }
            if (go.GetComponent<Animation>() != null)
            {
                return ModelImporterAnimationType.Legacy;
            }
            return ModelImporterAnimationType.None;
        }

        public static ModelImporterAnimationType GetAnimationType(Motion motion)
        {
            AnimationClip firstAnimationClipFromMotion = GetFirstAnimationClipFromMotion(motion);
            if (firstAnimationClipFromMotion != null)
            {
                if (firstAnimationClipFromMotion.legacy)
                {
                    return ModelImporterAnimationType.Legacy;
                }
                if (firstAnimationClipFromMotion.humanMotion)
                {
                    return ModelImporterAnimationType.Human;
                }
                return ModelImporterAnimationType.Generic;
            }
            return ModelImporterAnimationType.None;
        }

        protected Vector3 GetCurrentMouseWorldPosition(Event evt, Rect previewRect)
        {
            Camera camera = this.previewUtility.camera;
            float scaleFactor = this.previewUtility.GetScaleFactor(previewRect.width, previewRect.height);
            Vector3 position = new Vector3((evt.mousePosition.x - previewRect.x) * scaleFactor, (previewRect.height - (evt.mousePosition.y - previewRect.y)) * scaleFactor, 0f)
            {
                z = Vector3.Distance(this.bodyPosition, camera.transform.position)
            };
            return camera.ScreenToWorldPoint(position);
        }

        private static AnimationClip GetFirstAnimationClipFromMotion(Motion motion)
        {
            AnimationClip clip = motion as AnimationClip;
            if (clip != null)
            {
                return clip;
            }
            BlendTree tree = motion as BlendTree;
            if (tree != null)
            {
                AnimationClip[] animationClipsFlattened = tree.GetAnimationClipsFlattenedX();
                if (animationClipsFlattened.Length > 0)
                {
                    return animationClipsFlattened[0];
                }
            }
            return null;
        }

        private static GameObject GetGenericAnimationFallback()
        {
            return (GameObject)EditorGUIUtility.Load("Avatar/DefaultGeneric.fbx");
        }

        private static GameObject GetHumanoidFallback()
        {
            return (GameObject)EditorGUIUtility.Load("Avatar/DefaultAvatar.fbx");
        }

        protected void HandleMouseDown(Event evt, int id, Rect previewRect)
        {
            if ((this.viewTool != ViewTool.None) && previewRect.Contains(evt.mousePosition))
            {
                EditorGUIUtility.SetWantsMouseJumping(1);
                evt.Use();
                GUIUtility.hotControl = id;
            }
        }

        protected void HandleMouseDrag(Event evt, int id, Rect previewRect)
        {
            if ((this.m_PreviewInstance != null) && (GUIUtility.hotControl == id))
            {
                switch (this.m_ViewTool)
                {
                    case ViewTool.Pan:
                        this.DoAvatarPreviewPan(evt);
                        return;

                    case ViewTool.Zoom:
                        this.DoAvatarPreviewZoom(evt, -HandleUtility.niceMouseDeltaZoom * (!evt.shift ? 0.5f : 2f));
                        return;

                    case ViewTool.Orbit:
                        this.DoAvatarPreviewOrbit(evt, previewRect);
                        return;
                }
                Debug.Log("Enum value not handled");
            }
        }

        protected void HandleMouseUp(Event evt, int id)
        {
            if (GUIUtility.hotControl == id)
            {
                this.m_ViewTool = ViewTool.None;
                GUIUtility.hotControl = 0;
                EditorGUIUtility.SetWantsMouseJumping(0);
                evt.Use();
            }
        }

        protected void HandleViewTool(Event evt, EventType eventType, int id, Rect previewRect)
        {
            switch (eventType)
            {
                case EventType.MouseDown:
                    this.HandleMouseDown(evt, id, previewRect);
                    break;

                case EventType.MouseUp:
                    this.HandleMouseUp(evt, id);
                    break;

                case EventType.MouseDrag:
                    this.HandleMouseDrag(evt, id, previewRect);
                    break;

                case EventType.ScrollWheel:
                    this.DoAvatarPreviewZoom(evt, HandleUtility.niceMouseDeltaZoom * (!evt.shift ? 0.5f : 2f));
                    break;
            }
        }

        private void Init()
        {
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
                this.m_FloorTexture = (Texture2D)EditorGUIUtility.Load("Avatar/Textures/AvatarFloor.png");
            }
            if (this.m_FloorMaterial == null)
            {
                Shader shader = EditorGUIUtility.LoadRequired("Previews/PreviewPlaneWithShadow.shader") as Shader;
                this.m_FloorMaterial = new Material(shader);
                this.m_FloorMaterial.mainTexture = this.m_FloorTexture;
                this.m_FloorMaterial.mainTextureScale = (Vector2)((Vector2.one * 5f) * 4f);
                this.m_FloorMaterial.SetVector("_Alphas", new Vector4(0.5f, 0.3f, 0f, 0f));
                this.m_FloorMaterial.hideFlags = HideFlags.HideAndDontSave;
                this.m_FloorMaterialSmall = new Material(this.m_FloorMaterial);
                this.m_FloorMaterialSmall.mainTextureScale = (Vector2)((Vector2.one * 0.2f) * 4f);
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
        }

        public void InitInstance(UnityEngine.Animator scenePreviewObject, Motion motion)
        {
            this.m_SourcePreviewMotion = motion;
            this.m_SourceScenePreviewAnimator = scenePreviewObject;
            if (this.m_PreviewInstance == null)
            {
                GameObject go = CalculatePreviewGameObject(scenePreviewObject, motion, this.animationClipType);
                this.SetupBounds(go);
            }
            if (this.timeControl == null)
            {
                this.timeControl = new TimeControl();
            }
            if (this.m_ReferenceInstance == null)
            {
                GameObject original = (GameObject)EditorGUIUtility.Load("Avatar/dial_flat.prefab");
                this.m_ReferenceInstance = UnityEngine.Object.Instantiate<GameObject>(original, Vector3.zero, Quaternion.identity);
                EditorUtilityX.InitInstantiatedPreviewRecursive(this.m_ReferenceInstance);
                this.previewUtility.AddSingleGO(this.m_ReferenceInstance);
            }
            if (this.m_DirectionInstance == null)
            {
                GameObject obj4 = (GameObject)EditorGUIUtility.Load("Avatar/arrow.fbx");
                this.m_DirectionInstance = UnityEngine.Object.Instantiate<GameObject>(obj4, Vector3.zero, Quaternion.identity);
                EditorUtilityX.InitInstantiatedPreviewRecursive(this.m_DirectionInstance);
                this.previewUtility.AddSingleGO(this.m_DirectionInstance);
            }
            if (this.m_PivotInstance == null)
            {
                GameObject obj5 = (GameObject)EditorGUIUtility.Load("Avatar/root.fbx");
                this.m_PivotInstance = UnityEngine.Object.Instantiate<GameObject>(obj5, Vector3.zero, Quaternion.identity);
                EditorUtilityX.InitInstantiatedPreviewRecursive(this.m_PivotInstance);
                this.previewUtility.AddSingleGO(this.m_PivotInstance);
            }
            if (this.m_RootInstance == null)
            {
                GameObject obj6 = (GameObject)EditorGUIUtility.Load("Avatar/root.fbx");
                this.m_RootInstance = Object.Instantiate<GameObject>(obj6, Vector3.zero, Quaternion.identity);
                EditorUtilityX.InitInstantiatedPreviewRecursive(this.m_RootInstance);
                this.previewUtility.AddSingleGO(this.m_RootInstance);
            }
            this.m_IKOnFeet = EditorPrefs.GetBool("AvatarpreviewShowIK", false);
            this.m_ShowReference = EditorPrefs.GetBool("AvatarpreviewShowReference", true);
            this.is2D = EditorPrefs.GetBool("Avatarpreview2D", EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D);
            this.timeControl.playbackSpeed = EditorPrefs.GetFloat("AvatarpreviewSpeed", 1f);
            this.SetPreviewCharacterEnabled(false, false);
            this.m_PivotPositionOffset = Vector3.zero;
        }

        public static bool IsValidPreviewGameObject(GameObject target, ModelImporterAnimationType requiredClipType)
        {
            if ((target != null) && !target.activeSelf)
            {
                Debug.LogWarning("Can't preview inactive object, using fallback object");
            }
            return ((((target != null) && target.activeSelf) && GameObjectInspector.HasRenderableParts(target)) && ((requiredClipType == ModelImporterAnimationType.None) || (GetAnimationType(target) == requiredClipType)));
        }

        public void OnDisable()
        {
            if (this.m_PreviewUtility != null)
            {
                this.m_PreviewUtility.Cleanup();
                this.m_PreviewUtility = null;
            }
            if (this.timeControl != null)
            {
                this.timeControl.OnDisable();
            }
        }

        private void PositionPreviewObjects(Quaternion pivotRot, Vector3 pivotPos, Quaternion bodyRot, Vector3 bodyPos, Quaternion directionRot, Quaternion rootRot, Vector3 rootPos, Vector3 directionPos, float scale)
        {
            this.m_ReferenceInstance.transform.position = rootPos;
            this.m_ReferenceInstance.transform.rotation = rootRot;
            this.m_ReferenceInstance.transform.localScale = (Vector3)((Vector3.one * scale) * 1.25f);
            this.m_DirectionInstance.transform.position = directionPos;
            this.m_DirectionInstance.transform.rotation = directionRot;
            this.m_DirectionInstance.transform.localScale = (Vector3)((Vector3.one * scale) * 2f);
            this.m_PivotInstance.transform.position = pivotPos;
            this.m_PivotInstance.transform.rotation = pivotRot;
            this.m_PivotInstance.transform.localScale = (Vector3)((Vector3.one * scale) * 0.1f);
            this.m_RootInstance.transform.position = bodyPos;
            this.m_RootInstance.transform.rotation = bodyRot;
            this.m_RootInstance.transform.localScale = (Vector3)((Vector3.one * scale) * 0.25f);
            if (this.Animator != null)
            {
                float normalizedTime = this.timeControl.normalizedTime;
                float num2 = this.timeControl.deltaTime / (this.timeControl.stopTime - this.timeControl.startTime);
                if (((normalizedTime - num2) < 0f) || ((normalizedTime - num2) >= 1f))
                {
                    this.m_PrevFloorHeight = this.m_NextFloorHeight;
                }
                if (((this.m_LastNormalizedTime != -1000f) && (this.timeControl.startTime == this.m_LastStartTime)) && (this.timeControl.stopTime == this.m_LastStopTime))
                {
                    float num3 = (normalizedTime - num2) - this.m_LastNormalizedTime;
                    if (num3 > 0.5f)
                    {
                        num3--;
                    }
                    else if (num3 < -0.5f)
                    {
                        num3++;
                    }
                }
                this.m_LastNormalizedTime = normalizedTime;
                this.m_LastStartTime = this.timeControl.startTime;
                this.m_LastStopTime = this.timeControl.stopTime;
                if (this.m_NextTargetIsForward)
                {
                    this.m_NextFloorHeight = this.Animator.targetPosition.y;
                }
                else
                {
                    this.m_PrevFloorHeight = this.Animator.targetPosition.y;
                }
                this.m_NextTargetIsForward = !this.m_NextTargetIsForward;
                this.Animator.SetTarget(AvatarTarget.Root, !this.m_NextTargetIsForward ? ((float)0) : ((float)1));
            }
        }

        private float PreviewSlider(Rect position, float val, float snapThreshold)
        {
            val = GUI.HorizontalSlider(position, val, 0.1f, 2f, s_Styles.preSlider, s_Styles.preSliderThumb);
            if ((val > (0.25f - snapThreshold)) && (val < (0.25f + snapThreshold)))
            {
                val = 0.25f;
                return val;
            }
            if ((val > (0.5f - snapThreshold)) && (val < (0.5f + snapThreshold)))
            {
                val = 0.5f;
                return val;
            }
            if ((val > (0.75f - snapThreshold)) && (val < (0.75f + snapThreshold)))
            {
                val = 0.75f;
                return val;
            }
            if ((val > (1f - snapThreshold)) && (val < (1f + snapThreshold)))
            {
                val = 1f;
                return val;
            }
            if ((val > (1.25f - snapThreshold)) && (val < (1.25f + snapThreshold)))
            {
                val = 1.25f;
                return val;
            }
            if ((val > (1.5f - snapThreshold)) && (val < (1.5f + snapThreshold)))
            {
                val = 1.5f;
                return val;
            }
            if ((val > (1.75f - snapThreshold)) && (val < (1.75f + snapThreshold)))
            {
                val = 1.75f;
            }
            return val;
        }

        private RenderTexture RenderPreviewShadowmap(Light light, float scale, Vector3 center, Vector3 floorPos, out Matrix4x4 outShadowMatrix)
        {
            Assert.IsTrue(Event.current.type == EventType.Repaint);
            Camera camera = this.previewUtility.camera;
            camera.orthographic = true;
            camera.orthographicSize = scale * 2f;
            camera.nearClipPlane = 1f * scale;
            camera.farClipPlane = 25f * scale;
            camera.transform.rotation = !this.is2D ? light.transform.rotation : Quaternion.identity;
            camera.transform.position = center - ((Vector3)(light.transform.forward * (scale * 5.5f)));
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
            Matrix4x4 matrixx = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f));
            outShadowMatrix = (matrixx * camera.projectionMatrix) * camera.worldToCameraMatrix;
            camera.orthographic = false;
            camera.clearFlags = clearFlags;
            camera.backgroundColor = backgroundColor;
            camera.targetTexture = targetTexture;
            return texture2;
        }

        private int Repeat(int t, int length)
        {
            return (((t % length) + length) % length);
        }

        public void ResetPreviewFocus()
        {
            this.m_PivotPositionOffset = this.bodyPosition - this.rootPosition;
        }

        public void ResetPreviewInstance()
        {
            Object.DestroyImmediate(this.m_PreviewInstance);
            GameObject go = CalculatePreviewGameObject(this.m_SourceScenePreviewAnimator, this.m_SourcePreviewMotion, this.animationClipType);
            this.SetupBounds(go);
        }

        public void SetPreview(GameObject gameObject)
        {
            AvatarPreviewSelection.SetPreview(this.animationClipType, gameObject);
            Object.DestroyImmediate(this.m_PreviewInstance);
            this.InitInstance(this.m_SourceScenePreviewAnimator, this.m_SourcePreviewMotion);
            if (this.m_OnAvatarChangeFunc != null)
            {
                this.m_OnAvatarChangeFunc();
            }
        }

        private void SetPreviewAvatarOption(object obj)
        {
            this.m_Option = (PreviewPopupOptions)obj;
            if (this.m_Option == PreviewPopupOptions.Auto)
            {
                this.SetPreview(null);
            }
            else if (this.m_Option == PreviewPopupOptions.DefaultModel)
            {
                this.SetPreview(GetHumanoidFallback());
            }
            else if (this.m_Option == PreviewPopupOptions.Other)
            {
                //ObjectSelectorOperation.Start(this);
            }
        }

        private void SetPreviewCharacterEnabled(bool enabled, bool showReference)
        {
            if (this.m_PreviewInstance != null)
            {
                PreviewRenderUtilityX.SetEnabledRecursive(this.m_PreviewInstance, enabled);
            }
            PreviewRenderUtilityX.SetEnabledRecursive(this.m_ReferenceInstance, showReference && enabled);
            PreviewRenderUtilityX.SetEnabledRecursive(this.m_DirectionInstance, showReference && enabled);
            PreviewRenderUtilityX.SetEnabledRecursive(this.m_PivotInstance, showReference && enabled);
            PreviewRenderUtilityX.SetEnabledRecursive(this.m_RootInstance, showReference && enabled);
        }

        private void SetupBounds(GameObject go)
        {
            this.m_IsValid = (go != null) && (go != GetGenericAnimationFallback());
            if (go != null)
            {
                this.m_PreviewInstance = EditorUtilityX.InstantiateForAnimatorPreview(go);
                this.previewUtility.AddSingleGO(this.m_PreviewInstance);
                Bounds bounds = new Bounds(this.m_PreviewInstance.transform.position, Vector3.zero);
                GameObjectInspector.GetRenderableBoundsRecurse(ref bounds, this.m_PreviewInstance);
                this.m_BoundingVolumeScale = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
                if ((this.Animator != null) && this.Animator.isHuman)
                {
                    this.m_AvatarScale = this.m_ZoomFactor = this.Animator.humanScale;
                }
                else
                {
                    this.m_AvatarScale = this.m_ZoomFactor = this.m_BoundingVolumeScale / 2f;
                }
            }
        }

        private void SetupPreviewLightingAndFx(SphericalHarmonicsL2 probe)
        {
            this.previewUtility.lights[0].intensity = 1.4f;
            this.previewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0f);
            this.previewUtility.lights[1].intensity = 1.4f;
            RenderSettings.ambientMode = AmbientMode.Custom;
            RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.1f, 1f);
            RenderSettings.ambientProbe = probe;
        }

        public ModelImporterAnimationType animationClipType
        {
            get
            {
                return GetAnimationType(this.m_SourcePreviewMotion);
            }
        }

        public UnityEngine.Animator Animator
        {
            get
            {
                return ((this.m_PreviewInstance == null) ? null : (this.m_PreviewInstance.GetComponent(typeof(UnityEngine.Animator)) as UnityEngine.Animator));
            }
        }

        public Vector3 bodyPosition
        {
            get
            {
                if ((this.Animator != null) && this.Animator.isHuman)
                {
                    return this.Animator.bodyPositionInternalX();
                }
                if (this.m_PreviewInstance != null)
                {
                    return GameObjectInspector.GetRenderableCenterRecurse(this.m_PreviewInstance, 1, 8);
                }
                return Vector3.zero;
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

        public bool IKOnFeet
        {
            get
            {
                return this.m_IKOnFeet;
            }
        }

        public bool is2D
        {
            get
            {
                return this.m_2D;
            }
            set
            {
                this.m_2D = value;
                if (this.m_2D)
                {
                    this.m_PreviewDir = new Vector2();
                }
            }
        }

        public OnAvatarChange OnAvatarChangeFunc
        {
            set
            {
                this.m_OnAvatarChangeFunc = value;
            }
        }

        public GameObject PreviewObject
        {
            get
            {
                return this.m_PreviewInstance;
            }
        }

        internal PreviewRenderUtility previewUtility
        {
            get
            {
                if (this.m_PreviewUtility == null)
                {
                    this.m_PreviewUtility = new PreviewRenderUtility();
                    this.m_PreviewUtility.camera.fieldOfView = 30f;
                    this.m_PreviewUtility.camera.allowHDR = false;
                    this.m_PreviewUtility.camera.allowMSAA = false;
                    this.m_PreviewUtility.ambientColor = new Color(0.1f, 0.1f, 0.1f, 0f);
                    this.m_PreviewUtility.lights[0].intensity = 1.4f;
                    this.m_PreviewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0f);
                    this.m_PreviewUtility.lights[1].intensity = 1.4f;
                }
                return this.m_PreviewUtility;
            }
        }

        public Vector3 rootPosition
        {
            get
            {
                return ((this.m_PreviewInstance == null) ? Vector3.zero : this.m_PreviewInstance.transform.position);
            }
        }

        public bool ShowIKOnFeetButton
        {
            get
            {
                return this.m_ShowIKOnFeetButton;
            }
            set
            {
                this.m_ShowIKOnFeetButton = value;
            }
        }

        protected ViewTool viewTool
        {
            get
            {
                Event current = Event.current;
                if (this.m_ViewTool == ViewTool.None)
                {
                    bool flag = current.control && (Application.platform == RuntimePlatform.OSXEditor);
                    bool actionKey = EditorGUI.actionKey;
                    bool flag3 = (!actionKey && !flag) && !current.alt;
                    if ((((current.button <= 0) && flag3) || ((current.button <= 0) && actionKey)) || (current.button == 2))
                    {
                        this.m_ViewTool = ViewTool.Pan;
                    }
                    else if (((current.button <= 0) && flag) || ((current.button == 1) && current.alt))
                    {
                        this.m_ViewTool = ViewTool.Zoom;
                    }
                    else if (((current.button <= 0) && current.alt) || (current.button == 1))
                    {
                        this.m_ViewTool = ViewTool.Orbit;
                    }
                }
                return this.m_ViewTool;
            }
        }


        public delegate void OnAvatarChange();

        private enum PreviewPopupOptions
        {
            Auto,
            DefaultModel,
            Other
        }

        private class Styles
        {
            //public GUIContent avatarIcon = EditorGUIUtility.TrIconContent<Avatar>("Changes the model to use for previewing.");
            public GUIContent ik = EditorGUIUtility.TrTextContent("IK", "Toggles feet IK preview", (Texture)null);
            public GUIContent is2D = EditorGUIUtility.TrTextContent("2D", "Toggles 2D preview mode", (Texture)null);
            public GUIContent pivot = EditorGUIUtility.TrIconContent("AvatarPivot", "Displays avatar's pivot and mass center");
            public GUIStyle preButton = "preButton";
            public GUIStyle preLabel = "preLabel";
            public GUIStyle preSlider = "preSlider";
            public GUIStyle preSliderThumb = "preSliderThumb";
            public GUIContent speedScale = EditorGUIUtility.TrIconContent("SpeedScale", "Changes animation preview speed");

            public GUIStyle timeScrubber = "TimeScrubber";
        }

        protected enum ViewTool
        {
            None,
            Pan,
            Zoom,
            Orbit
        }
    }
}
