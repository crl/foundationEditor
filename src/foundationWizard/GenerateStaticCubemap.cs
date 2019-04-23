namespace foundationEditor
{
    using UnityEditor;
    using UnityEngine;


    public class GenerateStaticCubemap : ScriptableWizard
    {

        public Transform renderPosition;
        public Cubemap cubemap;

        void OnWizardUpdate()
        {
            helpString = "Select transform to render" +
                         " from and cubemap to render into";
            if (renderPosition != null && cubemap != null)
            {
                isValid = true;
            }
            else
            {
                isValid = false;
            }
        }
        void OnWizardCreate()
        {
            GameObject go = new GameObject("CubemapCamera");
            go.AddComponent<Camera>();

            go.transform.position = renderPosition.position;
            go.transform.rotation = Quaternion.identity;

            go.GetComponent<Camera>().RenderToCubemap(cubemap);

            DestroyImmediate(go);
        }

        [MenuItem("Tools/Render Cubemap")]
        static void RenderCubemap()
        {
            ScriptableWizard.DisplayWizard("Render CubeMap", typeof(GenerateStaticCubemap), "Render!");
        }

    }
}