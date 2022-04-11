/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth
Copyright(c) 2022 Christian Schott (InstruNEXT GmbH)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace vsgUnity.Editor
{
    public class ExporterSettings : ScriptableObject
    {
        public enum Mode {
            Scene,
            Object
        }

        public Mode ExportMode = Mode.Scene;
        public GameObject ExportTarget = null;

        public string ExportDirectory = "";
        public string ExportFileName = "export";

        public GraphBuilder.ExportSettings GraphBuilderSettings = new GraphBuilder.ExportSettings();

        public bool BinaryExport = true;
        public bool ShowPreview = true;
        public bool MatchSceneCamera = true;

        public string GetFinalSaveFileName() 
        {
            string exportname = string.IsNullOrEmpty(ExportFileName) ? "export" : Path.GetFileNameWithoutExtension(ExportFileName);
            return Path.Combine(ExportDirectory, exportname) + (BinaryExport ? ".vsgb" : ".vsga");
        }

        public static ExporterSettings CreateNew() 
        {
            var result = createAsset();
            result.GraphBuilderSettings.autoAddCullNodes = false;
            result.GraphBuilderSettings.zeroRootTransform = false;
            result.GraphBuilderSettings.keepIdentityTransforms = false;
            result.ExportDirectory = Application.dataPath;
            EditorUtility.SetDirty(result);
            return result;
        }

        private static ExporterSettings createAsset() 
        {
            var asset = ScriptableObject.CreateInstance<ExporterSettings> ();
            // save in the same directory as this script
            string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(asset)));
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/vsgUnityExporterSettings.asset");
            AssetDatabase.CreateAsset (asset, assetPathAndName);
            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh();
            return asset;
        }
    }
}
