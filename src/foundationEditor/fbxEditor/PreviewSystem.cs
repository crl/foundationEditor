using foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using AnimatorStateMachine = UnityEditor.Animations.AnimatorStateMachine;

namespace foundationEditor
{
    public class PreviewSystem
    {
        public BaseEditorWindow parentEditorWindow;


        private Renderer selectedRenderer;
        private GameObject previewPrefab;
        private PlayParticleSystemEditor playParticleSystemEditor;
        private List<AnimationClip> animationClips = new List<AnimationClip>();
        private ParticleSystem[] particleSystems = new ParticleSystem[0];
        private Renderer[] renderers = new Renderer[0];
        private AvatarPreview m_AvatarPreview;
        private bool m_FirstInitialization = false;
        private AnimatorController m_Controller;
        private AnimatorState m_State;
        private AnimatorStateMachine m_StateMachine;
        private AnimationClip m_Clip;

        private GameObject m_NameInstance;

        private Material currentSelectedMaterial;
        private int currentSelectedIndexMaterial = -1;
        private string playingClipName;
        private AvatarMask avatarMask;
        private bool m_ShowCollider = true;

        private Rect previewRect;

        private PreviewCameraDrawLineBounds previewCameraDrawLineBounds;
        private UnitCFG unitCfg;
        private AnimatorClipRef animatorClipRef;
        public PreviewSystem()
        {
            EditorTickManager.Add(update);
        }

        private void Init()
        {
            if (m_AvatarPreview == null)
            {
                m_AvatarPreview = new AvatarPreview(null, null);
                m_AvatarPreview.OnAvatarChangeFunc = new AvatarPreview.OnAvatarChange(this.SetPreviewAvatar);
            }
            if (previewCameraDrawLineBounds == null)
            {
                previewCameraDrawLineBounds = new PreviewCameraDrawLineBounds();
            }

            if (playParticleSystemEditor == null)
            {
                playParticleSystemEditor = new PlayParticleSystemEditor();
                if (parentEditorWindow != null)
                {
                    playParticleSystemEditor.parentEditorWindow = parentEditorWindow;
                }
            }

            if (m_NameInstance == null)
            {
                this.m_NameInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                m_NameInstance.transform.localScale = Vector3.one * 0.1f;
                EditorUtils.InitInstantiatedPreviewRecursive(this.m_NameInstance);
                this.m_AvatarPreview.previewUtility.AddSingleGO(this.m_NameInstance);
            }
        }


        public void OnDestroy()
        {
            this.DestroyController();
            if (m_AvatarPreview != null)
            {
                m_AvatarPreview.OnDisable();
                m_AvatarPreview = null;
            }

            if (m_NameInstance != null)
            {
                GameObject.DestroyImmediate(m_NameInstance);
                m_NameInstance = null;
            }

            if (playParticleSystemEditor != null)
            {
                playParticleSystemEditor.Stop();
            }
        }

        public void OnGUI()
        {
            previewRect = GUILayoutUtility.GetRect(500, 500);
        }

        private Vector2 aniScrollPosition;
        private Rect aniTotalRect;
        public void DrawRect(Rect rect)
        {
            Init();
            this.InitController();
            Event e = Event.current;

            this.previewRect = rect;
            this.drawPreview(rect, e);

            Vector3 floorPos = new Vector3(0f, 0f, 0f);
            if (this.m_AvatarPreview.PreviewObject != null)
            {
                var bodyPosition = EditorUtils.GetRenderableCenterRecurse(this.m_AvatarPreview.PreviewObject, 2, 8);
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

            if (m_ShowCollider)
            {
                //previewCameraDrawLineBounds.Update(this.m_AvatarPreview.m_PreviewUtility.camera);
            }


            Rect uiRect = previewRect;
            uiRect.x += 5;
            uiRect.y += 55;
            uiRect.height = 20;
            uiRect.width = 80;

            this.doDrawAnimatorClipRef(previewRect, e);
            this.doDrawRenderers(previewRect, e);
            this.doDrawTextures(previewRect, e);

            aniScrollPosition = GUI.BeginScrollView(new Rect(uiRect.x, uiRect.y, 110, previewRect.height - uiRect.y), aniScrollPosition, aniTotalRect);

            uiRect.y = uiRect.x = 0;
            uiRect = this.doDrawAnimationClips(uiRect, e);
            uiRect = this.doDrawParticle(uiRect, e);
            aniTotalRect.height = uiRect.y;
            GUI.EndScrollView();
        }

        private Rect doDrawTextures(Rect position, Event e)
        {
            if (currentSelectedMaterial == null || unitCfg == null || unitCfg.hasTexeture == false)
            {
                return position;
            }
            var height = position.height;
            position.width = position.height = 100;
            position.x += 150;
            position.y = height - 110;

            List<TextureSet> textSets = unitCfg.getTextureSets(currentSelectedIndexMaterial);
            if (textSets == null)
            {
                textSets = unitCfg.getTextureSets(0);
            }

            if (textSets == null)
            {
                return position;
            }

            foreach (TextureSet textureSet in textSets)
            {
                Texture texture = textureSet.texture;
                if (texture == null) continue;
                if (GUI.Button(position, texture))
                {
                    chageMainTexture(texture);
                }
                position.x += 110;
            }

            return position;
        }


        private Rect doDrawRenderers(Rect position, Event e)
        {
            if (renderers.Length == 0)
            {
                return position;
            }

            position.y += 55;
            position.x = position.xMax - 205;
            position.width = 200;
            position.height = 40;
            EditorGUILayout.BeginVertical(GUILayout.Width(100));

            int bonesCount = 0;
            int materialIndex = 0;
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
                if (GUI.Button(position, name))
                {
                    currentSelectedIndexMaterial = materialIndex;
                    currentSelectedMaterial = material;
                    if (selectedRenderer != null)
                    {
                        UnityEditor.EditorUtility.SetSelectedRenderState(selectedRenderer, EditorSelectedRenderState.Hidden);
                    }
                    UnityEditor.EditorUtility.SetSelectedRenderState(renderer, EditorSelectedRenderState.Wireframe);
                    Selection.activeTransform = renderer.transform;
                    selectedRenderer = renderer;
                }
                GUI.color = Color.white;
                position.y += 45;
                materialIndex++;
            }

            EditorGUILayout.EndVertical();

            return position;
        }

        private Rect doDrawParticle(Rect position, Event e)
        {
            if (particleSystems.Length > 0)
            {
                foreach (ParticleSystem particleSystem in particleSystems)
                {
                    if (GUI.Button(position, particleSystem.name))
                    {
                        //playParticleSystemEditor.Play(particleSystem, previewInstance);
                    }
                    position.y += 25;
                }
                if (GUI.Button(position, "所有粒子"))
                {
                    // playParticleSystemEditor.Play(particleSystems.ToList(), previewInstance);
                }
                position.y += 25;
                if (GUI.Button(position, "停止粒子"))
                {
                    playParticleSystemEditor.Stop();
                }
                position.y += 25;
            }
            return position;
        }

        private Rect doDrawAnimatorClipRef(Rect position, Event e)
        {
            if (animatorClipRef == null)
            {
                return position;
            }
            position.width = 250;
            position.height = 16;
            position.y += 4;
            EditorGUI.BeginChangeCheck();

            if (animatorClipRef.controller == null)
            {
                GUI.color = Color.red;
            }
            else
            {
                GUI.color = new Color(0, 1, 1, 1);
            }
            RuntimeAnimatorController controller = EditorGUI.ObjectField(position, animatorClipRef.controller,
                typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(animatorClipRef, "resetAnimatorClipRef");
                animatorClipRef.controller = controller;
                UnityEditor.EditorUtility.SetDirty(previewPrefab);
            }
            GUI.color = Color.white;

            return position;
        }

        private Rect doDrawAnimationClips(Rect position, Event e)
        {
            foreach (AnimationClip animationClip in animationClips)
            {
                if (animationClip == null)
                {
                    GUI.color = Color.red;
                    GUI.Button(position, "miss", EditorStyles.miniButton);
                }
                else
                {
                    GUI.color = Color.white;
                    string clipName = animationClip.name;
                    if (clipName == playingClipName)
                    {
                        GUI.color = new Color(0, 1f, 1f, 1f);
                    }

                    if (GUI.Button(position, clipName, EditorStyles.miniButton))
                    {
                        string path = AssetDatabase.GetAssetPath(animationClip);
                        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        Selection.activeGameObject = go;
                        EditorGUIUtility.PingObject(go);

                        playAnimation(animationClip);
                        e.Use();
                    }
                    AnimationClipSettings clipSetting = AnimationUtility.GetAnimationClipSettings(animationClip);
                    position.x += 80;
                    EditorGUI.BeginChangeCheck();
                    GUI.Toggle(position, clipSetting.loopTime, "");
                    if (EditorGUI.EndChangeCheck())
                    {
                        Selection.activeObject = animationClip;
                        EditorGUIUtility.PingObject(animationClip);
                    }
                    position.x -= 80;
                }
                position.y += 25;
                GUI.color = Color.white;
            }
            return position;
        }

        private void drawPreview(Rect rect, Event e)
        {
            bool flag = true;
            if (e != null)
            {
                flag = e.type == EventType.Repaint;
            }

            if (flag)
            {
                this.m_AvatarPreview.timeControl.Update();
            }

            if (this.m_Clip)
            {
                AnimationClipSettings animationClipSettings = AnimationUtility.GetAnimationClipSettings(m_Clip);
                this.m_AvatarPreview.timeControl.loop = true;
                if (flag && (this.m_AvatarPreview.PreviewObject != null))
                {
                    if (!m_Clip.legacy && (this.m_AvatarPreview.Animator != null))
                    {
                        if (this.m_State != null)
                        {
                            this.m_State.iKOnFeet = this.m_AvatarPreview.IKOnFeet;
                        }

                        var dura = animationClipSettings.stopTime - animationClipSettings.startTime;
                        var cur = this.m_AvatarPreview.timeControl.currentTime - animationClipSettings.startTime;
                        var tot = animationClipSettings.stopTime - animationClipSettings.startTime;
                        float normalizedTime = (dura == 0f) ? 0f : (cur / tot);
                        this.m_AvatarPreview.Animator.Play(0, 0, normalizedTime);
                        this.m_AvatarPreview.Animator.Update(this.m_AvatarPreview.timeControl.deltaTime);
                    }
                    else
                    {
                        m_Clip.SampleAnimation(this.m_AvatarPreview.PreviewObject,
                            this.m_AvatarPreview.timeControl.currentTime);
                    }
                }
            }

            this.m_AvatarPreview.DoPreviewSettings(rect);
            rect.yMin += 21;
            this.m_AvatarPreview.DoAvatarPreview(rect, (GUIStyle)"PreBackground");
        }

        private void chageMainTexture(Texture texture)
        {
            if (currentSelectedMaterial && texture)
            {
                Undo.RegisterCompleteObjectUndo(currentSelectedMaterial, "resetMainText");
                currentSelectedMaterial.mainTexture = texture;
            }
        }

        private void playAnimation(AnimationClip value)
        {
            if (!value)
            {
                return;
            }

            this.m_Clip = value;
            this.playingClipName = value.name;

            this.DestroyController();

            var pos = this.m_AvatarPreview.m_PivotPositionOffset;
            this.m_AvatarPreview.InitInstance(null, m_Clip);
            this.m_AvatarPreview.m_PivotPositionOffset = pos;
        }



        public void SetPreview(GameObject prefab)
        {
            this.m_Clip = null;
            currentSelectedMaterial = null;
            currentSelectedIndexMaterial = -1;
            previewPrefab = prefab;
            unitCfg = null;
            animatorClipRef = null;

            AnimationClip defaultClip = null;
            if (previewPrefab != null)
            {
                animationClips.Clear();
                animatorClipRef = previewPrefab.GetComponentInChildren<AnimatorClipRef>();
                unitCfg = previewPrefab.GetComponentInChildren<UnitCFG>();
                if (animatorClipRef != null)
                {
                    foreach (AnimationClip clip in animatorClipRef.animationClips)
                    {
                        if (clip.name.ToLower().IndexOf("idle") != 0)
                        {
                            defaultClip = clip;
                        }
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
                                if (clip.name.ToLower().IndexOf("idle") != 0)
                                {
                                    defaultClip = clip;
                                }
                                animationClips.Add(clip);
                            }
                        }
                    }
                }
                particleSystems = previewPrefab.GetComponentsInChildren<ParticleSystem>();
                renderers = previewPrefab.GetComponentsInChildren<Renderer>();
            }

            if (animationClips.Count > 0)
            {
                this.m_Clip = animationClips[0];
                if (defaultClip)
                {
                    this.m_Clip = defaultClip;
                }
                this.playingClipName = this.m_Clip.name;
            }

            this.m_AvatarPreview.InitInstance(null, m_Clip);
            if (m_Clip)
            {
                this.m_AvatarPreview.fps = Mathf.RoundToInt(m_Clip.frameRate);
                this.m_AvatarPreview.ShowIKOnFeetButton = m_Clip.isHumanMotion;
            }

            this.m_AvatarPreview.SetPreview(previewPrefab);
            this.m_AvatarPreview.ResetPreviewFocus();

            if (this.m_AvatarPreview.timeControl.currentTime == float.NegativeInfinity)
            {
                this.m_AvatarPreview.timeControl.Update();
            }

            if (this.m_AvatarPreview.PreviewObject)
            {
                //previewCameraDrawLineBounds.SetView(this.m_AvatarPreview.PreviewObject, previewPrefab);
            }
        }

        private void SetPreviewAvatar()
        {
            this.DestroyController();
            this.InitController();
        }

        private void InitController()
        {
            if (!this.m_Clip)
            {
                return;
            }

            if (!this.m_Clip.legacy && ((this.m_AvatarPreview != null) && (this.m_AvatarPreview.Animator != null)))
            {
                bool flag = true;
                if (this.m_Controller == null)
                {
                    this.m_Controller = new AnimatorController();
                    this.m_Controller.pushUndoX(false);
                    this.m_Controller.hideFlags = HideFlags.HideAndDontSave;
                    this.m_Controller.AddLayer("preview");
                    this.m_StateMachine = this.m_Controller.layers[0].stateMachine;
                    this.m_StateMachine.pushUndoX(false);
                    this.m_StateMachine.hideFlags = HideFlags.HideAndDontSave;
                    if (this.avatarMask != null)
                    {
                        AnimatorControllerLayer[] layers = this.m_Controller.layers;
                        layers[0].avatarMask = this.avatarMask;
                        this.m_Controller.layers = layers;
                    }
                    flag = false;
                }
                if (this.m_State == null)
                {
                    this.m_State = this.m_StateMachine.AddState("preview");
                    this.m_State.pushUndoX(false);
                    AnimatorControllerLayer[] layers = this.m_Controller.layers;
                    this.m_State.motion = this.m_Clip;
                    this.m_Controller.layers = layers;
                    this.m_State.iKOnFeet = this.m_AvatarPreview.IKOnFeet;
                    this.m_State.hideFlags = HideFlags.HideAndDontSave;
                    flag = false;
                }
                AnimatorController.SetAnimatorController(this.m_AvatarPreview.Animator, this.m_Controller);
                if (this.m_AvatarPreview.Animator.GetEffectiveAnimatorControllerX() != this.m_Controller)
                {
                    AnimatorController.SetAnimatorController(this.m_AvatarPreview.Animator, this.m_Controller);
                }
                if (!flag)
                {
                    this.m_AvatarPreview.Animator.Play(0, 0, 0f);
                    this.m_AvatarPreview.Animator.Update(0f);
                    if (this.m_FirstInitialization)
                    {
                        this.m_AvatarPreview.ResetPreviewFocus();
                        this.m_FirstInitialization = false;
                    }
                }
            }
        }

        private void DestroyController()
        {
            if ((this.m_AvatarPreview != null) && (this.m_AvatarPreview.Animator != null))
            {
                AnimatorController.SetAnimatorController(this.m_AvatarPreview.Animator, null);
            }
            UnityEngine.Object.DestroyImmediate(this.m_Controller);
            UnityEngine.Object.DestroyImmediate(this.m_State);
            this.m_Controller = null;
            this.m_StateMachine = null;
            this.m_State = null;
        }

        public void update(float t)
        {
            if (this.m_AvatarPreview == null || !this.m_Clip)
            {
                return;
            }
            parentEditorWindow.Repaint();
        }

        public void OnEnable()
        {
            EditorTickManager.Add(update);
        }

        public void OnDisable()
        {
            EditorTickManager.Remove(update);
        }
    }
}