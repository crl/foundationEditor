using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    /// <summary>
    /// 场景摄像机拍照功能
    /// </summary>
    public class SceneCameraPreview
    {
        private float aspect;
        private GameObject editorPreview;
        public int previewResolution=400;
        public GameObject testPrefab;
        public Vector3 testPositon=Vector3.zero;
        public Quaternion testRotation=Quaternion.identity;

        private Camera getSceneCamera()
        {
            GameObject cameraGo = GameObject.FindGameObjectWithTag("MainCamera");
            Camera camera = Camera.main;
            if (cameraGo != null)
            {
                camera = cameraGo.GetComponent<Camera>();
            }
            else if (camera == null)
            {
                camera = Camera.current;
                if (camera == null)
                {
                    camera = ((SceneView)SceneView.sceneViews[0]).camera;
                }
            }
            return camera;
        }

        private Camera getPreviewCamera()
        {
            Camera previewCam;
            if (editorPreview == null)
            {
                editorPreview = new GameObject("editorPreview");
                editorPreview.hideFlags = HideFlags.HideAndDontSave;
                previewCam = editorPreview.AddComponent<Camera>();
            }
            else
            {
                previewCam = editorPreview.GetComponent<Camera>();
            }

            return previewCam;
        }

        public void renderPreview(Vector3 position, Vector3 lookAt)
        {
            if (!EditorApplication.isPlaying)
            {
                Camera camera = getSceneCamera();
                aspect = camera.pixelHeight / (float)camera.pixelWidth;
                int w = previewResolution;
                int h = Mathf.RoundToInt(previewResolution * aspect);

                RenderTexture rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB, 1);

                Camera previewCam = getPreviewCamera();
                previewCam.CopyFrom(camera);

                editorPreview.transform.position = position;
                editorPreview.transform.LookAt(lookAt);

                GameObject testGameObject=null;
                if (testPrefab != null)
                {
                    testGameObject = GameObject.Instantiate(testPrefab, testPositon, testRotation) as GameObject;
                    testGameObject.hideFlags=HideFlags.HideAndDontSave;
                }

                previewCam.targetTexture = rt;
                previewCam.Render();
                previewCam.targetTexture = null;
                editorPreview.SetActive(false);

                if (testGameObject != null)
                {
                    GameObject.DestroyImmediate(testGameObject);
                }

                h = previewResolution;
                w = Mathf.RoundToInt(h / aspect);
                if (w > previewResolution)
                {
                    w = previewResolution;
                    h = Mathf.RoundToInt(w * aspect);
                }
               
                Rect rect = GUILayoutUtility.GetRect(w, h);
                GUI.Box(rect, "");

                float offset = (rect.width - w) / 2;
                rect.x += offset;
                rect.width = w;
                Rect guiRect = rect;
                int pad = 2;
                guiRect.x += pad;
                guiRect.y += pad;
                guiRect.width -= 2 * pad;
                guiRect.height -= 2 * pad;
                GUI.DrawTexture(guiRect, rt, ScaleMode.ScaleToFit, false);
                RenderTexture.ReleaseTemporary(rt);

                testPrefab = EditorGUILayout.ObjectField("testAvatar", testPrefab,
                    typeof(GameObject), false) as GameObject;
            }
        }

        public void renderPreview(Vector3 position, Quaternion rotation)
        {
            if (!EditorApplication.isPlaying)
            {
                Camera camera = getSceneCamera();
                aspect = camera.pixelHeight / (float)camera.pixelWidth;
                int w = previewResolution;
                int h = Mathf.RoundToInt(previewResolution * aspect);

                RenderTexture rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB, 1);

                Camera previewCam = getPreviewCamera();
                previewCam.CopyFrom(camera);

                editorPreview.transform.position = position;
                editorPreview.transform.rotation = rotation;

                previewCam.targetTexture = rt;
                previewCam.Render();
                previewCam.targetTexture = null;
                editorPreview.SetActive(false);

                h = previewResolution;
                w = Mathf.RoundToInt(h / aspect);
                if (w > previewResolution)
                {
                    w = previewResolution;
                    h = Mathf.RoundToInt(w * aspect);
                }

                Rect rect = GUILayoutUtility.GetRect(w, h);
                GUI.Box(rect, "");

                float offset = (rect.width - w) / 2;
                rect.x += offset;
                rect.width = w;
                Rect guiRect = rect;
                int pad = 2;
                guiRect.x += pad;
                guiRect.y += pad;
                guiRect.width -= 2 * pad;
                guiRect.height -= 2 * pad;
                GUI.DrawTexture(guiRect, rt, ScaleMode.ScaleToFit, false);
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        public void Dispose()
        {
            if (editorPreview != null)
            {
                GameObject.DestroyImmediate(editorPreview);
                editorPreview = null;
            }
        }
    }
}