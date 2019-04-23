using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace foundationEditor
{
    public class SceneAutoSave : EditorWindow
    {

        private bool autoSaveScene = true;
        private bool showMessage = true;
        private bool isStarted = false;
        private int intervalScene;
        private DateTime lastSaveTimeScene = DateTime.Now;

        //private string projectPath = Application.dataPath;
        private string scenePath;

//        [MenuItem("Tools/SceneAutoSave")]
//        static void Init()
//        {
//            SceneAutoSave saveWindow = (SceneAutoSave)EditorWindow.GetWindow(typeof(SceneAutoSave));
//            saveWindow.Show();
//        }

        void OnGUI()
        {
            autoSaveScene = EditorGUILayout.BeginToggleGroup("Auto save", autoSaveScene);
            intervalScene = EditorGUILayout.IntSlider("Interval (minutes)", intervalScene, 1, 10);
            if (isStarted)
            {
                EditorGUILayout.LabelField("Last save:", "" + lastSaveTimeScene);
            }
            EditorGUILayout.EndToggleGroup();
            showMessage = EditorGUILayout.BeginToggleGroup("Show Message", showMessage);
            EditorGUILayout.EndToggleGroup();
        }


        void Update()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                return;
            }
            scenePath = scene.path;
            if (autoSaveScene)
            {
                if (DateTime.Now.Minute >= (lastSaveTimeScene.Minute + intervalScene) || DateTime.Now.Minute == 59 && DateTime.Now.Second == 59)
                {
                    saveScene(scene);
                }
            }
            else {
                isStarted = false;
            }

        }

        void saveScene(Scene scene)
        {
            EditorSceneManager.SaveScene(scene);
            lastSaveTimeScene = DateTime.Now;
            isStarted = true;
            if (showMessage)
            {
                Debug.Log("AutoSave saved: " + scenePath + " on " + lastSaveTimeScene);
            }
        }
    }
}