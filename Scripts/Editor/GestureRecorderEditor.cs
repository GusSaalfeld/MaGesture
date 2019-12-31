using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GestureRecorder))]
[CanEditMultipleObjects]
public class GestureRecorderEditor : Editor
{
    //SerializedProperty filename;

    private void OnEnable()
    {
        //filename = serializedObject.FindProperty("filename");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GestureRecorder recorder = (GestureRecorder)target;

        //EditorGUILayout.PropertyField(filename);

        if (GUILayout.Button("Record Hand Snapshot"))
        {
            Debug.Log("clicked");
            recorder.StoreHandSnapshot();
        }
    }
}
