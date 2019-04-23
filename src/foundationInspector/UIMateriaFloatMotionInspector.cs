using System.Linq;
using System.Collections.Generic;
using foundation;
using gameSDK;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{


    [CustomEditor(typeof(UIMateriaFloatMotion))]
    public class UIMateriaFloatMotionInspector : BaseInspector<UIMateriaFloatMotion>
    {
        private List<string> keys = new List<string>(10);

        private Material replaceMaterial;

        private ASDictionary<string, float> def = new ASDictionary<float>();
        private ASDictionary<string, float> max = new ASDictionary<float>();
        private ASDictionary<string, float> min = new ASDictionary<float>();
        private ASDictionary<string, Color> colors = new ASDictionary<Color>();
        private string[] colorKeys = new string[0];

        protected override void OnEnable()
        {
            base.OnEnable();

            replaceMaterial = mTarget.replaceMaterial;
            updateKeys();
        }

        protected void updateKeys()
        {
            if (mTarget.replaceMaterial != null)
            {
                Shader shader = mTarget.replaceMaterial.shader;
                int len = ShaderUtil.GetPropertyCount(shader);
                keys.Clear();
                max.Clear();
                max.Clear();
                def.Clear();
                for (int i = 0; i < len; i++)
                {
                    string g = ShaderUtil.GetPropertyName(shader, i);
                    ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(shader, i);
                    if (type== ShaderUtil.ShaderPropertyType.Range)
                    {
                        def[g] = ShaderUtil.GetRangeLimits(shader, i, 0);
                        min[g] = ShaderUtil.GetRangeLimits(shader, i, 1);
                        max[g] = ShaderUtil.GetRangeLimits(shader, i, 2);

                        keys.Add(g);
                    }

                    if (type == ShaderUtil.ShaderPropertyType.Color)
                    {
                        Color color = mTarget.replaceMaterial.GetColor(g);
                        colors.Add(g, color);
                        colorKeys=colors.Keys.ToArray();
                    }
                }
            }
        }

        protected override void drawInspectorGUI()
        {
            base.drawInspectorGUI();
            if (mTarget.replaceMaterial != replaceMaterial)
            {
                replaceMaterial = mTarget.replaceMaterial;
                updateKeys();
            }
            int selectedIndex;
            if (colors.Count > 0)
            {
                selectedIndex = ArrayUtility.IndexOf(colorKeys, mTarget.colorKey);
                selectedIndex = EditorGUILayout.Popup("ColorKey", selectedIndex, colorKeys);
                if (selectedIndex < 0)
                {
                    selectedIndex = 0;
                    mTarget.color = colors[colorKeys[selectedIndex]];
                }
                mTarget.colorKey = colorKeys[selectedIndex];
                mTarget.color = EditorGUILayout.ColorField("Color", mTarget.color);
            }
            else
            {
                mTarget.colorKey = "";
            }


            if (keys.Count > 0)
            {
                if (keys.Count > 1)
                {
                    selectedIndex = keys.IndexOf(mTarget.strengthKey);
                    selectedIndex = EditorGUILayout.Popup("strengthKey", selectedIndex, keys.ToArray());
                    if (selectedIndex < 0)
                    {
                        selectedIndex = 0;
                    }
                    mTarget.strengthKey = keys[selectedIndex];
                    if (mTarget.strengthValue == -1)
                    {
                        mTarget.strengthValue = def[mTarget.strengthKey];
                    }
                    float strengthValue = EditorGUILayout.FloatField("strength", mTarget.strengthValue);

                    strengthValue = Mathf.Min(strengthValue, max[mTarget.strengthKey]);
                    strengthValue = Mathf.Max(strengthValue, min[mTarget.strengthKey]);
                    mTarget.strengthValue = strengthValue;
                }
                else
                {
                    mTarget.strengthKey = "";
                    mTarget.strengthValue = -1;
                }

                selectedIndex = keys.IndexOf(mTarget.animationKey);
                selectedIndex = EditorGUILayout.Popup("AnimationKey", selectedIndex, keys.ToArray());
                if (selectedIndex <0)
                {
                    selectedIndex = 0;
                }
                mTarget.animationKey = keys[selectedIndex];
            }

            float startValue = mTarget.startValue;
            float endValue = mTarget.endValue;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MinVal:", startValue.ToString(),GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("MaxVal:", endValue.ToString(), GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.MinMaxSlider("value", ref startValue, ref endValue, min[mTarget.animationKey],
                max[mTarget.animationKey]);
            if (EditorGUI.EndChangeCheck())
            {
                mTarget.startValue = startValue;
                mTarget.endValue = endValue;
            }
        }
    }
}