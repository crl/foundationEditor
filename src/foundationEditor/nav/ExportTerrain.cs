using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public enum SaveFormat
    {
        Triangles,
        Quads
    }

    public enum SaveResolution
    {
        Full,
        Half,
        Quarter,
        Eighth,
        Sixteenth
    }

    public class ExportTerrain : ScriptableWizard
    {
        [MenuItem("Tools/Export/ExportTerrain")]
        public static void Export()
        {
            ScriptableWizard.DisplayWizard("ExportTerrain", typeof(ExportTerrain), "确定");
        }

        private static TerrainData terrainData;
        private static Vector3 terrainPos;
    
        public void OnEnable()
        {
            terrainData = null;
        }

        public SaveFormat saveFormat = SaveFormat.Triangles;
        public SaveResolution saveResolution = SaveResolution.Half;

        protected override bool DrawWizardGUI()
        {
            if (!terrainData)
            {
                GameObject terrainGameObject = Selection.activeObject as GameObject;
                TerrainCollider terrainCollider;
                Terrain terrainObject = null;
                if (terrainGameObject == null)
                {
                    return doNotFound();
                }

                terrainObject = terrainGameObject.GetComponent<Terrain>();
                terrainCollider = terrainGameObject.GetComponent<TerrainCollider>();

                if (terrainObject == null)
                {
                    return doNotFound();
                }

                terrainData = terrainObject.terrainData;
                terrainPos = terrainObject.transform.position;

                if (terrainData == null)
                {
                    if (terrainCollider != null)
                    {
                        terrainData = terrainCollider.terrainData;
                    }
                }
            }

            if (terrainData==null)
            {
                return doNotFound();
            }
            return base.DrawWizardGUI();
        }

        private bool doNotFound()
        {
            GUILayout.Label("No terrain found");
            if (GUILayout.Button("Cancel"))
            {
                this.Close();
            }
            return false;
        }

        protected virtual void OnWizardCreate()
        {
            var fileName = EditorUtility.SaveFilePanel("Export .obj file", "", "Terrain", "obj");

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            int w = terrainData.heightmapWidth;
            int h = terrainData.heightmapHeight;
            Vector3 meshScale = terrainData.size;
            int tRes = (int) Mathf.Pow(2, (int) saveResolution);
            meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
            var uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
            var tData = terrainData.GetHeights(0, 0, w, h);

            w = (int) ((w - 1.0f) / tRes + 1);
            h = (int) ((h - 1) / tRes + 1);
            var tVertices = new Vector3[w * h];
            var tUV = new Vector2[w * h];

            int[] tPolys;
            if (saveFormat == SaveFormat.Triangles)
            {
                tPolys = new int[(w - 1) * (h - 1) * 6];
            }
            else
            {
                tPolys = new int[(w - 1) * (h - 1) * 4];
            }

            // Build vertices and UVs
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(x, tData[x * tRes, y * tRes], y)) +
                                           terrainPos;
                    tUV[y * w + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
                }
            }

            var index = 0;
            if (saveFormat == SaveFormat.Triangles)
            {
                // Build triangle indices: 3 indices into vertex array for each triangle
                for (int y = 0; y < h - 1; y++)
                {
                    for (int x = 0; x < w - 1; x++)
                    {
                        // For each grid cell output two triangles
                        tPolys[index++] = (y * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x;
                        tPolys[index++] = (y * w) + x + 1;

                        tPolys[index++] = ((y + 1) * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x + 1;
                        tPolys[index++] = (y * w) + x + 1;
                    }
                }
            }
            else
            {
                // Build quad indices: 4 indices into vertex array for each quad
                for (int y = 0; y < h - 1; y++)
                {
                    for (int x = 0; x < w - 1; x++)
                    {
                        // For each grid cell output one quad
                        tPolys[index++] = (y * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x;
                        tPolys[index++] = ((y + 1) * w) + x + 1;
                        tPolys[index++] = (y * w) + x + 1;
                    }
                }
            }
            StreamWriter sw = new StreamWriter(fileName);
            // Export to .obj
            try
            {
                sw.WriteLine("# Unity terrain OBJ File");

                // Write vertices
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                counter = tCount = 0;
                totalCount = (tVertices.Length * 2 +
                              (saveFormat == SaveFormat.Triangles ? tPolys.Length / 3 : tPolys.Length / 4)) / 1000;
                StringBuilder sb;
                for (int i = 0; i < tVertices.Length; i++)
                {
                    UpdateProgress();
                    sb = new StringBuilder("v ", 20);
                    // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
                    // Which is important when you're exporting huge terrains.
                    sb.Append(tVertices[i].x.ToString())
                        .Append(" ")
                        .Append(tVertices[i].y.ToString())
                        .Append(" ")
                        .Append(tVertices[i].z.ToString());
                    sw.WriteLine(sb);
                }

                // Write UVs
                for (int i = 0; i < tUV.Length; i++)
                {
                    UpdateProgress();
                    sb = new StringBuilder("vt ", 22);
                    sb.Append(tUV[i].x.ToString()).Append(" ").Append(tUV[i].y.ToString());
                    sw.WriteLine(sb);
                }
                if (saveFormat == SaveFormat.Triangles)
                {
                    // Write triangles
                    for (int i = 0; i < tPolys.Length; i += 3)
                    {
                        UpdateProgress();
                        sb = new StringBuilder("f ", 43);
                        sb.Append(tPolys[i] + 1)
                            .Append("/")
                            .Append(tPolys[i] + 1)
                            .Append(" ")
                            .Append(tPolys[i + 1] + 1)
                            .Append("/")
                            .Append(tPolys[i + 1] + 1)
                            .Append(" ")
                            .Append(tPolys[i + 2] + 1)
                            .Append("/")
                            .Append(tPolys[i + 2] + 1);
                        sw.WriteLine(sb);
                    }
                }
                else
                {
                    // Write quads
                    for (int i = 0; i < tPolys.Length; i += 4)
                    {
                        UpdateProgress();
                        sb = new StringBuilder("f ", 57);
                        sb.Append(tPolys[i] + 1)
                            .Append("/")
                            .Append(tPolys[i] + 1)
                            .Append(" ")
                            .Append(tPolys[i + 1] + 1)
                            .Append("/")
                            .Append(tPolys[i + 1] + 1)
                            .Append(" ")
                            .Append(tPolys[i + 2] + 1)
                            .Append("/")
                            .Append(tPolys[i + 2] + 1)
                            .Append(" ")
                            .Append(tPolys[i + 3] + 1)
                            .Append("/")
                            .Append(tPolys[i + 3] + 1);
                        sw.WriteLine(sb);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Error saving file: " + ex.Message);
            }
            finally
            {
                sw.Close();
            }


            terrainData = null;
            EditorUtility.ClearProgressBar();
        }

        private int counter;
        private int tCount;
        private int totalCount;

        void UpdateProgress()
        {
            if (counter++ == 1000)
            {
                counter = 0;
                EditorUtility.DisplayProgressBar("Saving...", "", Mathf.InverseLerp(0, totalCount, ++tCount));
            }
        }
    }
}