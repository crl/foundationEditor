using UnityEngine;

namespace foundationEditor
{
    public class ConfigUI:EditorUI
    {
        public GameObject selectPrefab;
        //private Rect rect = new Rect(0, 0, 150, 150);

        public override void onRender()
        {
            if (selectPrefab == null)
            {
                return;
            }

            //Texture2D texture2D = AssetPreview.GetAssetPreview(selectPrefab);
            
        }


     
    }
}