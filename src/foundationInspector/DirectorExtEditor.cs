using System;
using foundation;
using UnityEditor;
using UnityEngine;
/*using UnityEngine.Playables;

namespace foundationEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PlayableDirector))]
    public class DirectorExtEditor : DecoratorEditor
    {
        private PlayableDirector mTarget;
        public DirectorExtEditor() : base("DirectorEditor")
        {
        }

        void OnEnable()
        {
            mTarget=target as PlayableDirector;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            PlayableAsset playableAsset = mTarget.playableAsset;
            if (GUILayout.Button("Editor") && playableAsset!=null)
            {
                Type type = ObjectFactory.Locate("UnityEditor.Timeline.TimelineWindow");
                EditorWindow.GetWindow(type);

                if (mTarget.gameObject.activeInHierarchy==false)
                {
                    Selection.activeObject = playableAsset;
                }
                Event.current.Use();
            }
        }
    }
}*/