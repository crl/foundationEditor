using System;
using System.Collections.Generic;
using foundation;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace foundationEditor
{
    [CustomEditor(typeof (UnitCFG))]
    [CanEditMultipleObjects]
    public class UnitCFGInspector : BaseInspector<UnitCFG>
    {
        private static bool isShowBone = false;
        private bool isAnimatorInStage = false;
        private int selectedHashCode;
        private Dictionary<Transform, bool> modelBones;
        protected float rotationY = 180;
        private List<AnimationClip> animationClips = new List<AnimationClip>();
        private ParticleSystem[] particleSystems = new ParticleSystem[0];
        private PlayAnimationEditor playAnimationEditor;
        private Animator animator;
        private ReorderableList replaceTexturesList;
        protected override void OnEnable()
        {
            base.OnEnable();

            SerializedProperty p = serializedObject.FindProperty("textureSets");
            replaceTexturesList = new ReorderableList(serializedObject,p,true, true, true, true);

            replaceTexturesList.drawHeaderCallback = (Rect rect) =>
            {
                GUI.Label(rect, "ReplaceTextures");
            };
            replaceTexturesList.onRemoveCallback = (ReorderableList list) =>
            {
                if (EditorUtility.DisplayDialog("警告", "是否真的要删除这个名称？", "是", "否"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };
            replaceTexturesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = replaceTexturesList.serializedProperty.GetArrayElementAtIndex(index);
                var textuRelative = element.FindPropertyRelative("texture");
                if (textuRelative.objectReferenceValue == null)
                {
                    GUI.color = Color.red;
                }
                rect.y += 2;
                float width = rect.width - 80;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight),
                    textuRelative, GUIContent.none);

                var keyRelative = element.FindPropertyRelative("index");
                EditorGUI.PropertyField(new Rect(rect.x+width, rect.y, 80, EditorGUIUtility.singleLineHeight),
                    keyRelative, GUIContent.none);

                GUI.color = Color.white;
            };

            if (playAnimationEditor == null)
            {
                playAnimationEditor = new PlayAnimationEditor();
            }
            animator = mTarget.GetComponent<Animator>();

            isAnimatorInStage = go.activeInHierarchy && animator!=null;
            transform = go.transform;

            modelBones = AvatarBoneDrawHelper.GetModelBones(transform);
            object o = AvatarBoneDrawHelper.GetHumanBones(serializedObject, modelBones);
            modelBones = AvatarBoneDrawHelper.GetModelBones(transform, false, o);

            animationClips.Clear();
            rotationY = transform.eulerAngles.y;

            AnimatorClipRef animatorClipRef = go.GetComponent<AnimatorClipRef>();
            if (animatorClipRef != null)
            {
                foreach (AnimationClip clip in animatorClipRef.animationClips)
                {
                    animationClips.Add(clip);
                }
            }

            if (animationClips.Count == 0 && animator!=null)
            {
                if (animator.runtimeAnimatorController != null)
                {
                    foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                    {
                        animationClips.Add(clip);
                    }
                }
            }
            particleSystems = go.GetComponentsInChildren<ParticleSystem>();

            /*if (mTarget.replaceTextures.Count > 0)
            {
                mTarget.textureSets = new List<TextureSet>();
                foreach (Texture replaceTexture in mTarget.replaceTextures)
                {
                    TextureSet set = new TextureSet();
                    set.texture = replaceTexture;
                    mTarget.textureSets.Add(set);
                }
                mTarget.replaceTextures.Clear();
                EditorUtility.SetDirty(mTarget);
            }*/
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            playAnimationEditor.Stop();
        }

        protected override void drawInspectorGUI()
        {
            mTarget.nameY = EditorGUILayout.FloatField("名字位置:", mTarget.nameY);
            mTarget.unitType = (UnitType) EditorGUILayout.EnumPopup("类型", (Enum) mTarget.unitType);
            mTarget.configID = EditorGUILayout.TextField("配置id", mTarget.configID);
            mTarget.strArgs = EditorGUILayout.TextField("字符串参数", mTarget.strArgs);
            mTarget.baseY = EditorGUILayout.FloatField("基位置:", mTarget.baseY);

            if (mTarget.unitType == UnitType.Avatar || mTarget.unitType == UnitType.Npc ||
                mTarget.unitType == UnitType.Monster || mTarget.unitType == UnitType.Mount)
            {
                isShowBone = EditorGUILayout.Toggle("ShowBone", isShowBone);
                if (isAnimatorInStage)
                {
                    AnimationPathSceneUI.OnInspectorGUI(go, animator);
                }
            }

            if (GUILayout.Button("Raycast"))
            {
                Vector3 v = mTarget.transform.position;
                v.y += 50f;
                Ray ray = new Ray(v, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, Physics.AllLayers))
                {
                    UnityEditor.Undo.RegisterCompleteObjectUndo(mTarget, "Raycast");
                    mTarget.transform.position = hit.point;
                }
            }

            replaceTexturesList.DoLayoutList();
        }

        private float size = 0.05f;

        protected override void drawSceneGUI()
        {
            Vector3 v = new Vector3();
            v.y = mTarget.nameY;

            v = getWorldByLocal(v);

            Handles.color = Color.red;
            Handles.CubeHandleCap(0, v, Quaternion.identity, size,Event.current.type);
            v.y -= 0.1f;

            if (UnitType.Start != mTarget.unitType)
            {
                Handles.Label(v, "nameY");
            }
            if (isAnimatorInStage)
            {
                AnimationPathSceneUI.OnSceneGUI();
            }

            if (isShowBone)
            {
                AvatarBoneDrawHelper.DrawSkeleton(transform, modelBones);
                Color oldColor = Color.green;

                Color color = Color.green;
                Transform[] bones = transform.GetComponentsInChildren<Transform>(true);
                int len = bones.Length;
                int[] zoomHosControl = new int[len];
                for (int i = 0; i < len; i++)
                {
                    Transform bone = bones[i];
                    int hashCode = bone.name.GetHashCode();
                    color = oldColor;
                    if (bone.gameObject.activeInHierarchy == false)
                    {
                        if (bone.gameObject.activeSelf == true)
                        {
                            color = Color.grey;
                        }
                        else
                        {
                            color = Color.red;
                        }
                    }

                    if (hashCode == selectedHashCode)
                    {
                        color = Color.yellow;
                    }
                    else
                    {
                        color.a = 0.5f;
                    }

                    Handles.color = color;

                    float ss = HandleUtility.GetHandleSize(bone.position) * controlSize;

                     Handles.FreeMoveHandle(bone.position, bone.localRotation, ss / 2, Vector3.zero,
                        (controlID, p, rotation, s, eventType) =>
                        {
                            zoomHosControl[i] = controlID;
                            Handles.SphereHandleCap(controlID, p, rotation, s, eventType);
                        });

                    if (GUIUtility.hotControl != 0)
                    {
                        if (GUIUtility.hotControl == zoomHosControl[i])
                        {
                            selectedHashCode = hashCode;
                            EditorGUIUtility.PingObject(bone.gameObject);
                        }
                    }

                    GameObject selected = Selection.activeGameObject;
                    if (selected != null && selected == bone.gameObject)
                    {
                        selectedHashCode = hashCode;
                    }
                }
            }


            Handles.BeginGUI();
            if (animationClips.Count > 0)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(100));
                foreach (AnimationClip animationClip in animationClips)
                {
                    if (animationClip == null)
                    {
                        continue;
                    }
                    if (GUILayout.Button(animationClip.name))
                    {
                        if (Application.isPlaying == false)
                        {
                            playAnimationEditor.Play(animationClip, go);
                        }
                        else
                        {
                            animator.speed = 1.0f;
                            animator.CrossFade(animationClip.name, 0f, -1);
                        }
                    }
                }

                rotationY = GUILayout.HorizontalSlider(rotationY, 0, 360);
                if (GUI.changed)
                {
                    Vector3 rt = transform.eulerAngles;
                    rt.y = rotationY;
                    transform.eulerAngles = rt;
                }

                EditorGUILayout.EndVertical();
            }
            if (particleSystems.Length > 0)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(100));
                foreach (ParticleSystem particleSystem in particleSystems)
                {
                    if (GUILayout.Button(particleSystem.name))
                    {
                        ParticleSystem ps = GetRoot(particleSystem);
                        ps.Play();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            Handles.EndGUI();
        }

        public static ParticleSystem GetRoot(ParticleSystem ps)
        {
            if (ps == null)
            {
                return null;
            }
            Transform parent = ps.transform;
            while ((parent.parent != null) && (parent.parent.gameObject.GetComponent<ParticleSystem>() != null))
            {
                parent = parent.parent;
            }
            return parent.gameObject.GetComponent<ParticleSystem>();
        }

    }
}
