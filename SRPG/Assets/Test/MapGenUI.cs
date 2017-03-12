using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestMapGenerator))]
public class MapGenUI : Editor {

	public TestMapGenerator current
	{
		get
		{
			return (TestMapGenerator)target;
		}
	}

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();


		if (GUILayout.Button("Save"))
			current.Save();
		if (GUILayout.Button("Load"))
			current.Load();

		if (GUI.changed)
			current.UpdateMarker ();
	}

}
