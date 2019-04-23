using foundation;
using UnityEditor;
using UnityEngine;
namespace foundationEditor
{
    [CustomEditor(typeof (CullingMask))]
    public class CullingMaskEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CullingMask mask = target as CullingMask;
            mask.setOffset();

            var p = mask.gameObject.transform.parent.gameObject;
            var img = UIUtils.GetImage(p, "glowImg");
            if (img)
            {
                img.rectTransform.localPosition = mask.GetComponent<RectTransform>().localPosition;
                img.rectTransform.sizeDelta = mask.GetComponent<RectTransform>().sizeDelta;
            }

        }

    }
}

