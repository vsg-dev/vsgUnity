/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

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
        public static GraphBuilder.ExportSettings _settings = new GraphBuilder.ExportSettings();
        protected static bool _hasInited = false;

        public static bool _binaryExport = true;
        public static bool _showPreview = true;
        public static bool _matchSceneCamera = true;

        public static int _cameraSelectionIndex = 0;
        public static List<Camera>_previewCameras = new List<Camera>();
        public static List<string>_previewCameraNames = new List<string>();

        string _feedbackText = "Select an export object.";
        bool _isExporting = false;

        // test code
        /*[MenuItem("Window/VulkanSceneGraph/Run Snippet")]
        static void RunSnippet()
        {
            ShaderMappingIO.CreateTemplateFileForShader(Shader.Find("Standard"));
            //ShaderMappingIO.CreateTemplateFileForShader(Shader.Find("CTS/CTS Terrain Shader Advanced Trial"));
        }*/

        // open window
        [MenuItem("Window/VulkanSceneGraph/Exporter")]
        static void Init()
        {  
            // Get existing open window or if none, make a new one:
            ExportWindow window = (ExportWindow) EditorWindow.GetWindow(typeof (ExportWindow), true, "vsgUnity");

            if(!_hasInited)
            {
                _settings.autoAddCullNodes = false;
                _settings.zeroRootTransform = false;

                _settings.standardTerrainShaderMappingPath = PathForShaderAsset("standardTerrain-ShaderMapping");

                _hasInited = true;
            }

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

        static string PathForShaderAsset(string shaderFileName)
        {
            string[] shaderGUID = AssetDatabase.FindAssets(shaderFileName);
            if (shaderGUID == null || shaderGUID.Length == 0) return string.Empty;
            string datapath = Application.dataPath;
            datapath = datapath.Remove(datapath.Length - ("Assets").Length);
            return Path.Combine(datapath, AssetDatabase.GUIDToAssetPath(shaderGUID[0]));
        }

        void ExportTarget()
        {
            if (_isExporting) return;

            _isExporting = true;

            NativeLog.InstallDebugLogCallback(); // why do we need to do this every time?
            float starttick = Time.realtimeSinceStartup;

            string exportname = string.IsNullOrEmpty(_exportFileName) ? "export" : Path.GetFileNameWithoutExtension(_exportFileName);
            string finalSaveFileName = Path.Combine(_exportDirectory, exportname) + (_binaryExport ? ".vsgb"
                                                                                     : ".vsga");

            if (_exportTarget != null)
            {
                GraphBuilder.Export(new GameObject[]{_exportTarget}, finalSaveFileName, _settings);
            }
            else
            {
                Scene scene = SceneManager.GetActiveScene();
                GraphBuilder.Export(scene.GetRootGameObjects(), finalSaveFileName, _settings);
            }

            _feedbackText = "Exported in " + (Time.realtimeSinceStartup - starttick) + " seconds";
            EditorUtility.SetDirty(this);

            if (_showPreview) GraphBuilder.LaunchViewer(finalSaveFileName, _matchSceneCamera, _previewCameras[_cameraSelectionIndex]); // this currently blocks

            _isExporting = false;
        }

        void OnGUI()
        {
            GUILayout.Label("VulkanSceneGraph Exporter", EditorStyles.boldLabel);

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            // target object
            _exportTarget = (GameObject) EditorGUILayout.ObjectField("Specific Object", _exportTarget, typeof (GameObject), true);

            EditorGUILayout.Separator();

            // filename
            _exportFileName = EditorGUILayout.TextField("File Name", _exportFileName);

            // directory selection
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Folder");
                GUILayout.Label(new GUIContent(_exportDirectory, _exportDirectory), GUILayout.MaxHeight(100), GUILayout.MinWidth(20), GUILayout.MaxWidth(275));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("...", GUILayout.MaxWidth(GUI.skin.button.lineHeight * 2.0f)))
                {
                    string defaultFolder = string.IsNullOrEmpty(_exportDirectory) ? Application.dataPath : _exportDirectory;

                    string selectedfolder = EditorUtility.OpenFolderPanel("Select export file path", defaultFolder, "");

                    if (!string.IsNullOrEmpty(selectedfolder))
                    {
                        _exportDirectory = selectedfolder;
                        EditorUtility.SetDirty(this);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            _binaryExport = EditorGUILayout.Toggle("Binary", _binaryExport);

            _settings.autoAddCullNodes = EditorGUILayout.Toggle("Add Cull Nodes", _settings.autoAddCullNodes);
            _settings.zeroRootTransform = EditorGUILayout.Toggle("Zero Root Transform", _settings.zeroRootTransform);

            EditorGUILayout.LabelField(_settings.standardTerrainShaderMappingPath);

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
            if (_exportTarget != null)
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

            List<Texture>allTextures = new List<Texture>();
            foreach(MeshRenderer renderer in renderers)
            {
                Material[] sharedMaterials = renderer.sharedMaterials;

                foreach(Material m in sharedMaterials)
                {
                    if (m == null) continue;
                    //
                    Texture[] textures = MaterialConverter.GetValidTexturesForMaterial(m);
                    allTextures.AddRange(textures);
                }
            }

            List<Mesh>allMeshes = new List<Mesh>();
            foreach(MeshFilter filter in filters)
            {
                if (filter.sharedMesh != null) allMeshes.Add(filter.sharedMesh);
            }

            EditorUtility.DisplayProgressBar("Fixing Textures", "", 0.0f);
            int progress = 0;

            string report = string.Empty;

            foreach(Texture tex in allTextures)
            {
                TextureConverter.TextureSupportIssues issues = TextureConverter.GetSupportIssuesForTexture(tex);
                if (issues != TextureConverter.TextureSupportIssues.None && (issues & TextureConverter.TextureSupportIssues.Dimensions) != TextureConverter.TextureSupportIssues.Dimensions)
                {
                    report += TextureConverter.GetTextureSupportReport(issues, tex);

                    EditorUtility.DisplayProgressBar("Fixing Textures", "Processing " + tex.name, (float)((float) progress / (float) allTextures.Count));

                    string path = AssetDatabase.GetAssetPath(tex);
                    TextureImporter importer = (TextureImporter) TextureImporter.GetAtPath(path);
                    if (importer == null) continue;

                    if ((issues & TextureConverter.TextureSupportIssues.ReadWrite) == TextureConverter.TextureSupportIssues.ReadWrite)
                    {
                        importer.isReadable = true;
                    }
                    if ((issues & TextureConverter.TextureSupportIssues.Format) == TextureConverter.TextureSupportIssues.Format)
                    {
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("Standalone");
                        platformSettings.overridden = true;
                        platformSettings.format = TextureImporterFormat.RGBA32;
                        importer.SetPlatformTextureSettings(platformSettings);
                    }
                    if ((issues & TextureConverter.TextureSupportIssues.Dimensions) == TextureConverter.TextureSupportIssues.Dimensions)
                    {
                    }
                    importer.SaveAndReimport();
                }
                else if ((issues & TextureConverter.TextureSupportIssues.Dimensions) == TextureConverter.TextureSupportIssues.Dimensions)
                {
                    Debug.Log("Texture '" + tex.name + "' is using an unspported dimension '" + tex.dimension.ToString() + "' an cannot be converted.");
                }
                progress++;
            }

            EditorUtility.DisplayProgressBar("Fixing Meshes", "", 0.0f);
            progress = 0;

            foreach(Mesh mesh in allMeshes)
            {
                EditorUtility.DisplayProgressBar("Fixing Meshes", "Processing " + mesh.name, (float)((float) progress / (float) allMeshes.Count));

                if (!mesh.isReadable)
                {
                    report += "Mesh '" + mesh.name + "' is not readable.\n";
                    string path = AssetDatabase.GetAssetPath(mesh);
                    ModelImporter importer = (ModelImporter) ModelImporter.GetAtPath(path);
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
