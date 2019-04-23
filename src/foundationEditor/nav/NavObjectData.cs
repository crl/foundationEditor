using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace foundationEditor
{
    /// <summary>
    /// NAV地图的导出;
    /// </summary>
    public class NavObjectData
    {
        private List<string> vertices;
        private List<string> triangles;
        private List<string> uvs;
        private int triangleIndex;

        public NavObjectData()
        {
            vertices = new List<string>();
            triangles = new List<string>();
            uvs = new List<string>();

            writeVertice("g default");
            writeUV("g pCube");
            writeTriangle("");
            triangleIndex = 1;
        }


        [MenuItem("Tools/Export/OBJ")]
        static void ExportSelecter()
        {
            GameObject[] gameObjects = Selection.gameObjects;

            if (gameObjects.Length<1)
            {
                return;
            }
            GameObject active = Selection.activeGameObject;
            if (active == null)
            {
                active = gameObjects[0];
            }
            List<MeshFilter> meshRenderers = new List<MeshFilter>();
            List<SkinnedMeshRenderer> skinMeshRenderers = new List<SkinnedMeshRenderer>();
            foreach (GameObject gameObject in gameObjects)
            {
                Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    if (renderer is MeshRenderer)
                    {
                        MeshFilter f = renderer.gameObject.GetComponent<MeshFilter>();
                        if (f) meshRenderers.Add(f);
                    }

                    if (renderer is SkinnedMeshRenderer)
                    {
                        skinMeshRenderers.Add((SkinnedMeshRenderer) renderer);
                    }
                }
            }


            NavObjectData v=new NavObjectData(); 
            v.export(meshRenderers.ToArray(),skinMeshRenderers.ToArray());

            string prefix = Application.dataPath + "/Exports/";
            if (Directory.Exists(prefix)==false)
            {
                Directory.CreateDirectory(prefix);
            }
            v.save(prefix + active.name + ".obj");

            EditorUtility.OpenWithDefaultApp(prefix);
        }


        [MenuItem("Tools/Export/NavOBJ")]
        static void ExportNavOBJ()
        {
            GameObject[] gameObjects = Selection.gameObjects;

            if (gameObjects.Length < 1)
            {
                return;
            }
            GameObject active = Selection.activeGameObject;
            if (active == null)
            {
                active = gameObjects[0];
            }

            List<MeshFilter> meshRenderers = new List<MeshFilter>();
            List<SkinnedMeshRenderer> skinMeshRenderers = new List<SkinnedMeshRenderer>();
            foreach (GameObject gameObject in gameObjects)
            {
                Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    if (renderer is MeshRenderer)
                    {
                        GameObject go = renderer.gameObject;
                        SerializedObject so = new SerializedObject(go);
                        SerializedProperty property = so.FindProperty("m_StaticEditorFlags");
                        if ((property.intValue & 8) != 0)
                        {
                            MeshFilter f = renderer.gameObject.GetComponent<MeshFilter>();
                            if (f) meshRenderers.Add(f);
                        }
                    }
                }
            }

            NavObjectData v = new NavObjectData();
            v.export(meshRenderers.ToArray(), skinMeshRenderers.ToArray());

            string prefix = Application.dataPath + "/Exports/";
            if (Directory.Exists(prefix) == false)
            {
                Directory.CreateDirectory(prefix);
            }
            v.save(prefix + active.name + ".obj");

            EditorUtility.OpenWithDefaultApp(prefix);
        }


        [MenuItem("Tools/Export/NavMesh")]
        static void ExportNavMesh()
        {
            NavMeshTriangulation triangulatedNavMesh = NavMesh.CalculateTriangulation();

            Mesh mesh = new Mesh();
            mesh.name = "ExportedNavMesh";
            mesh.vertices = triangulatedNavMesh.vertices;
            mesh.triangles = triangulatedNavMesh.indices;

            Matrix4x4 matrix4X4=new Matrix4x4();
            matrix4X4.SetTRS(Vector3.zero, Quaternion.identity, new Vector3(-1,1,1));

            NavObjectData v = new NavObjectData();
            v.exportMesh(mesh, matrix4X4);

            string prefix = Application.dataPath + "/Exports/";
            if (Directory.Exists(prefix) == false)
            {
                Directory.CreateDirectory(prefix);
            }
            string path = prefix + "navMesh.obj";
            v.save(path);


            AssetDatabase.Refresh();
            UnityEngine.Object ob = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (ob)
            {
                Selection.activeObject = ob;
                EditorGUIUtility.PingObject(ob);
            }

            EditorUtility.OpenWithDefaultApp(prefix);
        }

        public void export(MeshFilter[] meshRenderers,SkinnedMeshRenderer[] skinMeshRenderers=null)
        {
            Matrix4x4 append = new Matrix4x4();
            append.SetTRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1));
            if (meshRenderers != null)
            {
                foreach (MeshFilter meshFilter in meshRenderers)
                {
                    Matrix4x4 matrix4X4 = meshFilter.gameObject.transform.localToWorldMatrix;
                    exportMesh(meshFilter.sharedMesh, append*matrix4X4);
                }
            }

            if (skinMeshRenderers != null)
            {
                foreach (SkinnedMeshRenderer meshFilter in skinMeshRenderers)
                {
                    Matrix4x4 matrix4X4 = meshFilter.gameObject.transform.localToWorldMatrix;
                    exportMesh(meshFilter.sharedMesh, append*matrix4X4);
                }
            }
        }

        public bool hasData()
        {
            return vertices.Count > 0;
        }

        private void exportMesh(Mesh mesh, Matrix4x4 matrix4X4)
        {
            if (mesh == null)
            {
                return;
            }
            Vector4 tv;
            foreach (Vector3 v in mesh.vertices)
            {
                tv = v;
                tv.w = 1;
                tv = matrix4X4 * tv;
                this.writeVertice("v " + tv.x + " " + tv.y + " " + tv.z);
            }

            foreach (Vector2 uv in mesh.uv)
            {
                this.writeUV("vt " + uv.x + " " + uv.y);
            }

            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                this.writeTriangle("f " + (triangles[i + 2] + triangleIndex) + " " + (triangles[i + 1] + triangleIndex) +
                                   " " +
                                   (triangles[i ] + triangleIndex));
            }

            triangleIndex += mesh.vertices.Length;
        }

        public void save(string path)
        {
            FileStream fs = File.Open(path, FileMode.Create);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string vertex in vertices)
            {
                stringBuilder.AppendLine(vertex);
            }
            foreach (string uv in uvs)
            {
                stringBuilder.AppendLine(uv);
            }

            foreach (string vertex in triangles)
            {
                stringBuilder.AppendLine(vertex);
            }
            byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();

        }

        internal void writeVertice(string value)
        {
            vertices.Add(value);
        }

        internal void writeTriangle(string value)
        {
            triangles.Add(value);
        }

        internal void writeUV(string value)
        {
            uvs.Add(value);
        }
    }
}
