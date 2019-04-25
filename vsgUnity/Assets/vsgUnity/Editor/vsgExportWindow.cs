using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using vsgUnity.Native;

public class vsgExportWindow : EditorWindow
{
    string saveFilePath = "Hello World";
    public GameObject _exportTarget = null;

    string _feedbackText = "Select an export object.";

    // open window
    [MenuItem("Window/VSG/Exporter")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        vsgExportWindow window = (vsgExportWindow)EditorWindow.GetWindow(typeof(vsgExportWindow));

        window._exportTarget = Selection.transforms.Length > 0 ? Selection.transforms[0].gameObject : null;

        window.Show();
    }

    void ExportTarget()
    {
        NativeLog.InstallDebugLogCallback(); // why do we need to do this every time?
        float starttick = Time.realtimeSinceStartup;
        GraphBuilder.Export(_exportTarget);

        _feedbackText = "Exported in " + (Time.realtimeSinceStartup - starttick) + " seconds";
        EditorUtility.SetDirty(this);
    }

    void OnGUI()
    {
        GUILayout.Label("VulkanSceneGraph Exporter", EditorStyles.boldLabel);

        saveFilePath = EditorGUILayout.TextField("Output Filepath", saveFilePath);

        _exportTarget = (GameObject)EditorGUILayout.ObjectField("Export Object", _exportTarget, typeof(GameObject), true);

        if(GUILayout.Button("Export"))
        {
            ExportTarget();
        }

        EditorGUILayout.LabelField(_feedbackText);

        //myBool = EditorGUILayout.Toggle("Toggle", myBool);
        //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
    }
}
