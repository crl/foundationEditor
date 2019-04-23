using foundation;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine;

namespace foundationEditor
{
    [CustomEditor(typeof(ImageText))]
    [CanEditMultipleObjects]
    public class ImageTextEditor : Editor
    {

        private GridLayoutGroup group;
        private ImageText t;
        private void OnEnable()
        {
            //Debug.Log("ImageTextEditor");
            t = target as ImageText;
            group = t.gameObject.GetComponent<GridLayoutGroup>();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
           
            t.Show(t.message,t.title);

            GUI.changed = false;
            t.upkAniVo = (UpkAniVO)EditorGUILayout.ObjectField("upkAsset", t.upkAniVo, typeof (UpkAniVO), false);
            if (GUILayout.Button("Reset Settings"))
            {
                ResetSetting(t);
            }
        }

        private void ResetSetting(ImageText t)
        {
            
            group.childAlignment = TextAnchor.MiddleCenter;
            group.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            group.constraintCount = 100;
            group.startAxis = GridLayoutGroup.Axis.Vertical;
            group.spacing = Vector2.zero;
            if (t.transform.childCount > 0)
            {
                Transform tt = t.transform.GetChild(0);
                Image image = tt.GetComponent<Image>();
                if (image != null)
                {
                    if (image.sprite != null)
                    {
                        Vector2 v = new Vector2(image.sprite.rect.width, image.sprite.rect.height);
                        group.cellSize = v;
                    }
                }
            }
        }

    }
}
