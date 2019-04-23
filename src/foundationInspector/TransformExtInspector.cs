//----------------------------------------------
//			  NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using foundationEditor;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(Transform))]
public class TransformExtInspector : DecoratorEditor
{
    private GameObject mTargetGO;
    private GameObject sourcePrefab;
    private Transform mTarget;

    public TransformExtInspector() : base("TransformInspector")
    {
    }

    void OnEnable ()
	{
        mTarget=target as Transform;
	    mTargetGO = mTarget.gameObject;
	}

	/// <summary>
	/// Draw the inspector widget.
	/// </summary>

	public override void OnInspectorGUI ()
	{
	    if (mTargetGO)
	    {
	        PrefabType prefabType = PrefabUtility.GetPrefabType(mTargetGO);
	        if (prefabType == PrefabType.MissingPrefabInstance)
	        {
	            EditorGUILayout.BeginHorizontal();
	            sourcePrefab =
	                EditorGUILayout.ObjectField(GUIContent.none, sourcePrefab, typeof(GameObject), false) as GameObject;

	            Vector3 postion = mTarget.transform.position;
	            Vector3 scale = mTarget.transform.localScale;
	            Quaternion rotation = mTarget.transform.rotation;
	            if (GUILayout.Button("replace", EditorStyles.miniButton))
	            {
	                mTargetGO = PrefabUtility.ConnectGameObjectToPrefab(mTargetGO, sourcePrefab);
	                mTargetGO.transform.position = postion;
	                mTargetGO.transform.rotation = rotation;
	                mTargetGO.transform.localScale = scale;
	                Selection.activeGameObject = mTargetGO;
                    return;
	            }
	            EditorGUILayout.EndHorizontal();
	        }
	    }

        base.OnInspectorGUI();
	}
}
