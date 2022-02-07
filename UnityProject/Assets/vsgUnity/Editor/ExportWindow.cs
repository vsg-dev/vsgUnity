/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth
Copyright(c) 2022 Christian Schott (InstruNEXT GmbH)

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
        private int _cameraSelectionIndex = 0;
        private List<Camera> _previewCameras = new List<Camera>();
        private List<string> _previewCameraNames = new List<string>();
        private string _feedbackText = "Select an export object.";
        private ExporterSettings _exporterSettings = null;

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
            ExportWindow window = (ExportWindow) EditorWindow.GetWindow(typeof (ExportWindow), false, "vsgUnity");
            window.Show();
        }

        void OnEnable() 
        {
            if (_exporterSettings == null) {
                var settingsGUIDs = AssetDatabase.FindAssets("t:" + typeof(ExporterSettings).Name);
                if (settingsGUIDs.Length > 0) {
                    var path = AssetDatabase.GUIDToAssetPath(settingsGUIDs[0]);
                    _exporterSettings = AssetDatabase.LoadAssetAtPath<ExporterSettings>(path);
                }
                if (_exporterSettings == null)
                    _exporterSettings = ExporterSettings.CreateNew();
            }
        }

        void OnGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope()) {
                using (new EditorGUI.IndentLevelScope()) {
                    EditorGUIUtility.labelWidth = 180;
                    EditorGUILayout.LabelField("VulkanSceneGraph Exporter", EditorStyles.largeLabel);
                    EditorGUILayout.Separator();
                    EditorGUILayout.Separator();

                    // TODO: use SerializedObject & SerializedProperties instead 

                    // target object
                    _exporterSettings.ExportMode = (ExporterSettings.Mode) EditorGUILayout.EnumPopup("Mode", _exporterSettings.ExportMode);
                    if (_exporterSettings.ExportMode == ExporterSettings.Mode.Object) 
                        _exporterSettings.ExportTarget = (GameObject) EditorGUILayout.ObjectField("Target Object", _exporterSettings.ExportTarget, typeof (GameObject), true);

                    EditorGUILayout.Separator();

                    // filename
                    _exporterSettings.ExportFileName = EditorGUILayout.TextField("File Name", _exporterSettings.ExportFileName);

                    // directory selection
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var dir = _exporterSettings.ExportDirectory;
                        EditorGUILayout.LabelField(new GUIContent("Folder", dir), new GUIContent(dir, dir), GUILayout.MinWidth(10));

                        if (GUILayout.Button("...", GUILayout.MaxWidth(GUI.skin.button.lineHeight * 2.0f)))
                        {
                            string defaultFolder = string.IsNullOrEmpty(dir) ? Application.dataPath : dir;
                            string selectedfolder = EditorUtility.OpenFolderPanel("Select export file path", defaultFolder, "");

                            if (!string.IsNullOrEmpty(selectedfolder))
                            {
                                _exporterSettings.ExportDirectory = selectedfolder;
                                EditorUtility.SetDirty(this);
                            }
                        }
                    }
                    _exporterSettings.BinaryExport = EditorGUILayout.Toggle("Binary", _exporterSettings.BinaryExport);

                    EditorGUILayout.Separator();
                    {
                        var graphBulderSettings = _exporterSettings.GraphBuilderSettings;
                        graphBulderSettings.autoAddCullNodes = EditorGUILayout.Toggle("Add Cull Nodes", graphBulderSettings.autoAddCullNodes);
                        graphBulderSettings.zeroRootTransform = EditorGUILayout.Toggle("Zero Root Transform", graphBulderSettings.zeroRootTransform);
                        graphBulderSettings.keepIdentityTransforms = EditorGUILayout.Toggle("Keep Identity Transforms", graphBulderSettings.keepIdentityTransforms);
                        graphBulderSettings.skybox = (Cubemap) EditorGUILayout.ObjectField("Skybox", graphBulderSettings.skybox, typeof (Cubemap), true);
                        EditorGUILayout.LabelField(graphBulderSettings.standardTerrainShaderMappingPath);
                        _exporterSettings.GraphBuilderSettings = graphBulderSettings;
                    }

                    EditorGUILayout.Separator();

                    // preview
                    using (var previewGroup = new EditorGUILayout.ToggleGroupScope("Preview", _exporterSettings.ShowPreview))
                    {
                        _exporterSettings.ShowPreview = previewGroup.enabled;
                        _exporterSettings.MatchSceneCamera = EditorGUILayout.Toggle("Match Camera", _exporterSettings.MatchSceneCamera);

                        if (previewGroup.enabled)
                            PopulatePreviewCamerasList();
                        _cameraSelectionIndex = EditorGUILayout.Popup(_cameraSelectionIndex, _previewCameraNames.ToArray());
                    }

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

                    if (check.changed) {
                        EditorUtility.SetDirty(_exporterSettings);
                    }
                }
            }
        }

        void ExportTarget()
        {
            float starttick = Time.realtimeSinceStartup;
            string finalSaveFileName = _exporterSettings.GetFinalSaveFileName();

            GraphBuilder.Export(exportGameObjects(), finalSaveFileName, _exporterSettings.GraphBuilderSettings);

            _feedbackText = "Exported in " + (Time.realtimeSinceStartup - starttick) + " seconds";
            EditorUtility.SetDirty(this);

            if (_exporterSettings.ShowPreview) 
                GraphBuilder.LaunchViewer(finalSaveFileName, _exporterSettings.MatchSceneCamera, _previewCameras[_cameraSelectionIndex]); // this currently blocks
        }

        void FixImportSettings()
        {
            foreach(GameObject go in exportGameObjects())
                FixImportSettings(go);
        }

        private  GameObject[] exportGameObjects() 
        {
            if (_exporterSettings.ExportMode == ExporterSettings.Mode.Object)
            {
                if (_exporterSettings.ExportTarget)
                    return new GameObject[] { _exporterSettings.ExportTarget };
            }
            else
            {
                return SceneManager.GetActiveScene().GetRootGameObjects();
            }
            return new GameObject[0];
        }

        private void PopulatePreviewCamerasList()
        {
            _previewCameras.Clear();
            _previewCameraNames.Clear();

            if (SceneView.lastActiveSceneView) {
                _previewCameras.Add(SceneView.lastActiveSceneView.camera);
                _previewCameraNames.Add("Scene View");
            }
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

        private void FixImportSettings(GameObject gameObject)
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
