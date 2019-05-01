using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

using vsgUnity.Native;

namespace vsgUnity.Editor
{
    public class ExportWindow : EditorWindow
    {
        public static string _exportDirectory = "";
        public static string _exportFileName = "export";

        public static GameObject _exportTarget = null;

        public static bool _binaryExport = true;
        public static bool _showPreview = true;
        public static bool _matchSceneCamera = true;

        public static int _cameraSelectionIndex = 0;
        public static List<Camera> _previewCameras = new List<Camera>();
        public static List<string> _previewCameraNames = new List<string>();

        string _feedbackText = "Select an export object.";
        bool _isExporting = false;

        // open window
        [MenuItem("Window/VulkanSceneGraph/Exporter")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            ExportWindow window = (ExportWindow)EditorWindow.GetWindow(typeof(ExportWindow), true, "vsgUnity");

            if (string.IsNullOrEmpty(_exportDirectory))
            {
                _exportDirectory = Application.dataPath;
            }

            if (_exportTarget == null)
            {
                _exportTarget = Selection.transforms.Length > 0 ? Selection.transforms[0].gameObject : null;
            }

            PopulatePreviewCamerasList();

            window.Show();
        }

        static void PopulatePreviewCamerasList()
        {
            _previewCameras.Clear();
            _previewCameraNames.Clear();

            _previewCameras.Add(SceneView.lastActiveSceneView.camera);
            _previewCameraNames.Add("Scene View");
            Camera[] sceneCameras = Camera.allCameras;
            for (int i = 0; i < sceneCameras.Length; i++)
            {
                _previewCameras.Add(sceneCameras[i]);
                _previewCameraNames.Add(sceneCameras[i].gameObject.name);
            }

            if(_cameraSelectionIndex >= _previewCameras.Count)
            {
                _cameraSelectionIndex = 0;
            }
        }

        void ExportTarget()
        {
            if (_isExporting || _exportTarget == null) return;
            _isExporting = true;

            NativeLog.InstallDebugLogCallback(); // why do we need to do this every time?
            float starttick = Time.realtimeSinceStartup;

            string exportname = string.IsNullOrEmpty(_exportFileName) ? "export" : Path.GetFileNameWithoutExtension(_exportFileName);
            string finalSaveFileName = Path.Combine(_exportDirectory, exportname) + (_binaryExport ? ".vsgb" : "vsga");

            GraphBuilder.Export(_exportTarget, finalSaveFileName);

            _feedbackText = "Exported in " + (Time.realtimeSinceStartup - starttick) + " seconds";
            EditorUtility.SetDirty(this);

            GraphBuilder.LaunchViewer(finalSaveFileName, _matchSceneCamera, _previewCameras[_cameraSelectionIndex]); // this currently blocks

            _isExporting = false;
        }

        void OnGUI()
        {
            GUILayout.Label("VulkanSceneGraph Exporter", EditorStyles.boldLabel);

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            // target object
            _exportTarget = (GameObject)EditorGUILayout.ObjectField("Export Object", _exportTarget, typeof(GameObject), true);

            EditorGUILayout.Separator();

            // filename
            _exportFileName = EditorGUILayout.TextField("File Name", _exportFileName);

            // directory selection
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Folder");
                GUILayout.Label(_exportDirectory);

                if (GUILayout.Button("...", GUILayout.MaxWidth(GUI.skin.button.lineHeight * 2.0f)))
                {
                    string defaultFolder = string.IsNullOrEmpty(_exportDirectory) ? Application.dataPath : _exportDirectory;

                    string selectedfolder = EditorUtility.OpenFolderPanel("Select export file path", defaultFolder, "");

                    if(!string.IsNullOrEmpty(selectedfolder))
                    {
                        _exportDirectory = selectedfolder;
                        EditorUtility.SetDirty(this);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            _binaryExport = EditorGUILayout.Toggle("Binary", _binaryExport);

            EditorGUILayout.Separator();

            // preview
            _showPreview = EditorGUILayout.BeginToggleGroup("Preview", _showPreview);
            {
                _matchSceneCamera = EditorGUILayout.Toggle("Match Camera", _matchSceneCamera);

                PopulatePreviewCamerasList();
                _cameraSelectionIndex = EditorGUILayout.Popup(_cameraSelectionIndex, _previewCameraNames.ToArray());
            }
            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Separator();

            if (GUILayout.Button("Export"))
            {
                ExportTarget();
            }

            EditorGUILayout.LabelField(_feedbackText, EditorStyles.centeredGreyMiniLabel);
        }
    }
}
