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
                //_exportTarget = Selection.transforms.Length > 0 ? Selection.transforms[0].gameObject : null;
            }

            PopulatePreviewCamerasList();

            window.Show();
        }

        void ExportTarget()
        {
            if (_isExporting) return;

            _isExporting = true;

            NativeLog.InstallDebugLogCallback(); // why do we need to do this every time?
            float starttick = Time.realtimeSinceStartup;

            string exportname = string.IsNullOrEmpty(_exportFileName) ? "export" : Path.GetFileNameWithoutExtension(_exportFileName);
            string finalSaveFileName = Path.Combine(_exportDirectory, exportname) + (_binaryExport ? ".vsgb" : ".vsga");

            if (_exportTarget != null)
            {
                GraphBuilder.Export(new GameObject[] { _exportTarget }, finalSaveFileName);
            }
            else
            {
                Scene scene = SceneManager.GetActiveScene();
                GraphBuilder.Export(scene.GetRootGameObjects(), finalSaveFileName);
            }

            _feedbackText = "Exported in " + (Time.realtimeSinceStartup - starttick) + " seconds";
            EditorUtility.SetDirty(this);

            if(_showPreview) GraphBuilder.LaunchViewer(finalSaveFileName, _matchSceneCamera, _previewCameras[_cameraSelectionIndex]); // this currently blocks

            _isExporting = false;
        }

        void OnGUI()
        {
            GUILayout.Label("VulkanSceneGraph Exporter", EditorStyles.boldLabel);

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            // target object
            _exportTarget = (GameObject)EditorGUILayout.ObjectField("Specific Object", _exportTarget, typeof(GameObject), true);

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

            if (GUILayout.Button("Fix Import Settings"))
            {
                FixImportSettings();
            }

            EditorGUILayout.LabelField(_feedbackText, EditorStyles.centeredGreyMiniLabel);
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

            if (_cameraSelectionIndex >= _previewCameras.Count)
            {
                _cameraSelectionIndex = 0;
            }
        }

        static void FixImportSettings()
        {
            if(_exportTarget != null)
            {
                FixImportSettings(_exportTarget);
            }
            else
            {
                Scene scene = SceneManager.GetActiveScene();
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach(GameObject go in rootObjects)
                {
                    FixImportSettings(go);
                }
            }
        }

        static void FixImportSettings(GameObject gameObject)
        {
            MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            MeshFilter[] filters = gameObject.GetComponentsInChildren<MeshFilter>();

            List<Texture> allTextures = new List<Texture>();
            foreach(MeshRenderer renderer in renderers)
            {
                Material[] sharedMaterials = renderer.sharedMaterials;

                foreach (Material m in sharedMaterials)
                {
                    if (m == null) continue;
                    Dictionary<string, Texture> textures = NativeUtils.GetValidTexturesForMaterial(m);
                    allTextures.AddRange(textures.Values);
                }
            }

            List<Mesh> allMeshes = new List<Mesh>();
            foreach(MeshFilter filter in filters)
            {
                if (filter.sharedMesh != null) allMeshes.Add(filter.sharedMesh);
            }

            EditorUtility.DisplayProgressBar("Fixing Textures", "", 0.0f);
            int progress = 0;

            string report = string.Empty;

            foreach (Texture tex in allTextures)
            {
                NativeUtils.TextureSupportIssues issues = NativeUtils.GetSupportIssuesForTexture(tex);
                if(issues != NativeUtils.TextureSupportIssues.None && (issues & NativeUtils.TextureSupportIssues.Dimensions) != NativeUtils.TextureSupportIssues.Dimensions)
                {
                    report += NativeUtils.GetTextureSupportReport(issues, tex);

                    EditorUtility.DisplayProgressBar("Fixing Textures", "Processing " + tex.name, (float)((float)progress/(float)allTextures.Count));

                    string path = AssetDatabase.GetAssetPath(tex);
                    TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
                    if (importer == null) continue;

                    if ((issues & NativeUtils.TextureSupportIssues.ReadWrite) == NativeUtils.TextureSupportIssues.ReadWrite)
                    {
                        importer.isReadable = true;
                    }
                    if ((issues & NativeUtils.TextureSupportIssues.Format) == NativeUtils.TextureSupportIssues.Format)
                    {
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Standalone");
                        platformSettings.overridden = true;
                        platformSettings.format = TextureImporterFormat.RGBA32;
                        importer.SetPlatformTextureSettings(platformSettings);
                    }
                    if ((issues & NativeUtils.TextureSupportIssues.Dimensions) == NativeUtils.TextureSupportIssues.Dimensions)
                    {

                    }
                    importer.SaveAndReimport();
                }
                else if((issues & NativeUtils.TextureSupportIssues.Dimensions) == NativeUtils.TextureSupportIssues.Dimensions)
                {
                    Debug.Log("Texture '" + tex.name + "' is using an unspported dimension '" + tex.dimension.ToString() + "' an cannot be converted.");
                }
                progress++;
            }

            EditorUtility.DisplayProgressBar("Fixing Meshes", "", 0.0f);
            progress = 0;

            foreach (Mesh mesh in allMeshes)
            {
                EditorUtility.DisplayProgressBar("Fixing Meshes", "Processing " + mesh.name, (float)((float)progress / (float)allMeshes.Count));

                if (!mesh.isReadable)
                {
                    report += "Mesh '" + mesh.name + "' is not readable.\n";
                    string path = AssetDatabase.GetAssetPath(mesh);
                    ModelImporter importer = (ModelImporter)ModelImporter.GetAtPath(path);
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                }
                progress++;
            }

            EditorUtility.ClearProgressBar();

            Debug.Log("Fix report for GameObject '" + gameObject.name + "':\n" + report);
        }
    }
}
