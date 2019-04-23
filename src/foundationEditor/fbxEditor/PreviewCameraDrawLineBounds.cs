using foundation;
using UnityEngine;

namespace foundationEditor
{
    public class PreviewCameraDrawLineBounds
    {
        public Vector3[] list= new Vector3[24];
        private BoxCollider boxCollider;
        private Transform instanceTransform;
        private GameObject prefab;
        static Material lineMaterial;

        static void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                lineMaterial.SetInt("_ZWrite", 0);
            }
        }

        public void SetView(GameObject go, GameObject prefab)
        {
            instanceTransform = go.transform;
            this.prefab = prefab;
            Refreash();
        }

        private void Refreash()
        {
            if (prefab != null)
            {
                this.boxCollider = prefab.GetComponent<Collider>() as BoxCollider;
            }
        }


        private Vector3 oldCenter;
        private Vector3 oldSize;

        public void Update(Camera cam)
        {
            if (boxCollider == null)
            {
                Refreash();
                if (boxCollider == null)
                {
                    return;
                }
            }

            if (oldCenter != boxCollider.center || oldSize != boxCollider.size)
            {
                oldCenter = boxCollider.center;
                oldSize = boxCollider.size;
                list = VectorUtils.CalcCubeVertex(oldCenter, oldSize / 2);
            }

            RenderTexture.active = cam.targetTexture;

            CreateLineMaterial();
            lineMaterial.SetPass(0);

            GL.PushMatrix();
            GL.LoadProjectionMatrix(cam.projectionMatrix);
            GL.LoadIdentity();
            GL.MultMatrix(cam.worldToCameraMatrix * instanceTransform.localToWorldMatrix);

            GL.Begin(GL.LINES);
            GL.Color(Color.green);

            for (int i = 1; i < 24; i++)
            {
                GL.Vertex(list[i - 1]);
                GL.Vertex(list[i]);
            }

            GL.End();
            GL.PopMatrix();
        }

       
    }
}