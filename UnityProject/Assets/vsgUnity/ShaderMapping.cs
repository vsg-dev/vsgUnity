/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

using vsgUnity.Native;

namespace vsgUnity
{
    /// <summary>
    /// VertexAttributeDependancies
    /// Represents a vertex input required by a shader and any defines that depend on it
    /// </summary>

    [Serializable]
    public class VertexAttributeDependancies : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public VertexAttribute attributeType;

        [SerializeField]
        protected string attributeTypeString; // this is used for serialization so we can store enum as string in json 

        public List<string> dependantDefines = new List<string>(); // shader defines that depend on this input

        /// <summary>
        /// Should this Input be bound in the pipleline etc
        /// </summary>
        /// <param name="defines"></param>
        /// <returns></returns>
        
        public bool ShouldBind(List<string> defines)
        {
            if (dependantDefines == null || dependantDefines.Count == 0) return true;
            if (dependantDefines[0].ToUpper() == "ALL") return true;
            if (dependantDefines[0].ToUpper() == "NONE") return false;
            foreach(string define in defines)
            {
                if (dependantDefines.Contains(define)) return true;
            }
            return false;
        }

        //
        // ISerializationCallbackReceiver implementations

        public void OnBeforeSerialize()
        {
            attributeTypeString = attributeType.ToString();
        }

        public void OnAfterDeserialize()
        {
            if (!System.Enum.TryParse<VertexAttribute>(attributeTypeString, out attributeType)) attributeType = VertexAttribute.Position;
        }
    }

    /// <summary>
    /// UniformMapping
    /// Map a unity uniform/property to a vsg unifrom/descriptor
    /// </summary>

    [Serializable]
    public class UniformMapping : ISerializationCallbackReceiver
    {
        public enum UniformType
        {
            UnkownUniform = 0,
            FloatUniform,
            Vec4Uniform,
            ColorUniform,
            Matrix4x4Uniform,
            Texture2DUniform,
            Texture2DArrayUniform
        }

        [NonSerialized]
        public UniformType uniformType;

        [SerializeField]
        protected string uniformTypeString; // this is used for serialization so we can store enum as string in json 

        [NonSerialized]
        public VkShaderStageFlagBits stages;

        [SerializeField]
        protected string stagesString; // this is used for serialization so we can store enum as string in json

        // unity side data
        public string unityPropName; // name of the property in the unity shader

        // vsg side data
        public int vsgBindingIndex; // the descriptor binding index of the uniorm in the vsg shader

        public List<string> vsgDefines = new List<string>(); // any custom defines in the vsg shader associated with the uniform

        /// <summary>
        /// Get the data value from the passed material matching this uniform
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public object GetDataFromMaterial(Material material)
        {
            if (uniformType == UniformType.Texture2DUniform)
            {
                if (material.HasProperty(unityPropName))
                    return material.GetTexture(unityPropName);
            }
            else if(uniformType == UniformType.Texture2DArrayUniform)
            {
                if (material.HasProperty(unityPropName))
                    return material.GetTexture(unityPropName);
            }
            else if (uniformType == UniformType.FloatUniform)
            {
                if (material.HasProperty(unityPropName))
                    return material.GetFloat(unityPropName);
                else return 1.0f;
            }
            else if(uniformType == UniformType.Vec4Uniform)
            {
                if (material.HasProperty(unityPropName))
                    return material.GetVector(unityPropName);
                return Vector4.one;
            }
            else if (uniformType == UniformType.ColorUniform)
            {
                if (material.HasProperty(unityPropName))
                    return material.GetColor(unityPropName);
                else return Color.white;
            }
            return null;
        }

        //
        // ISerializationCallbackReceiver implementations

        public void OnBeforeSerialize()
        {
            uniformTypeString = uniformType.ToString();

            List<string> enumstrs = new List<string>();
            if ((stages & VkShaderStageFlagBits.VK_SHADER_STAGE_VERTEX_BIT) == VkShaderStageFlagBits.VK_SHADER_STAGE_VERTEX_BIT) enumstrs.Add("VertexStage");
            if ((stages & VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT) == VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT) enumstrs.Add("FragmentStage");
            stagesString = enumstrs.Count > 0 ? String.Join("|", enumstrs.ToArray()) : "UnkownStage";
        }

        public void OnAfterDeserialize()
        {
            if (!System.Enum.TryParse<UniformType>(uniformTypeString, out uniformType)) uniformType = UniformType.UnkownUniform;

            stages = (VkShaderStageFlagBits)0;
            if (string.IsNullOrEmpty(stagesString) || stagesString == "UnkownStage") return;

            string[] enumstrs = stagesString.Split('|');
            foreach (string enumstr in enumstrs)
            {
                if (enumstr == "VertexStage") stages |= VkShaderStageFlagBits.VK_SHADER_STAGE_VERTEX_BIT;
                if (enumstr == "FragmentStage") stages |= VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT;
            }
        }
    }

    /// <summary>
    /// UniformMappedData
    /// Helper class to pair up a uniform mapping to its data
    /// </summary>

    public class UniformMappedData
    {
        public UniformMappedData() { }
        public UniformMappedData(UniformMapping srcMapping, Material srcMaterial)
        {
            mapping = srcMapping;
            data = mapping.GetDataFromMaterial(srcMaterial);
        }

        public UniformMapping mapping; // the mapping this data represents
        public object data; // the data stored generically
    }

    /// <summary>
    /// ShaderResource
    /// Warps info about vsg shader source file and the stages it implements
    /// </summary>

    [Serializable]
    public class ShaderResource : ISerializationCallbackReceiver
    {
        public string sourceFile;

        [NonSerialized]
        public VkShaderStageFlagBits stages;

        [SerializeField]
        protected string stagesString; // this is used for serialization so we can store enum as string in json

        //
        // ISerializationCallbackReceiver implementations

        public void OnBeforeSerialize()
        {
            List<string> enumstrs = new List<string>();
            if ((stages & VkShaderStageFlagBits.VK_SHADER_STAGE_VERTEX_BIT) == VkShaderStageFlagBits.VK_SHADER_STAGE_VERTEX_BIT) enumstrs.Add("VertexStage");
            if ((stages & VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT) == VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT) enumstrs.Add("FragmentStage");
            stagesString = enumstrs.Count > 0 ? String.Join("|", enumstrs.ToArray()) : "UnkownStage";
        }

        public void OnAfterDeserialize()
        {
            stages = (VkShaderStageFlagBits)0;
            if (string.IsNullOrEmpty(stagesString) || stagesString == "UnkownStage") return;

            string[] enumstrs = stagesString.Split('|');
            foreach(string enumstr in enumstrs)
            {
                if (enumstr == "VertexStage") stages |= VkShaderStageFlagBits.VK_SHADER_STAGE_VERTEX_BIT;
                if (enumstr == "FragmentStage") stages |= VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT;
            }
        }
    }

    /// <summary>
    /// ShaderMapping
    /// Map a unity shader to a vsg shader including its unifrom mappings, shader resources etc
    /// </summary>

    [Serializable]
    public class ShaderMapping
    {
        // unity side data
        public string unityShaderName; // name of the unity shader

        // vsg side data
        public List<ShaderResource> shaders = new List<ShaderResource>(); // shader resources used to mimic the unity shader

        public List<UniformMapping> uniformMappings = new List<UniformMapping>(); // mappings of unity properties/uniforms to vsg descriptors/uniforms

        public List<VertexAttributeDependancies> vertexDependancies = new List<VertexAttributeDependancies>(); // vertex inputs for this shader

        public VertexAttributeDependancies GetVertexDependanciesForAttributeType(VertexAttribute attributeType)
        {
            foreach (VertexAttributeDependancies input in vertexDependancies)
            {
                if (input.attributeType == attributeType) return input;
            }
            return null;
        }

        // return all the uniform mappings with a given uniform type
        public UniformMapping[] GetUniformMappingsOfType(UniformMapping.UniformType type)
        {
            List<UniformMapping> result = new List<UniformMapping>();
            foreach (UniformMapping mapping in uniformMappings)
            {
                if (mapping.uniformType == type) result.Add(mapping);
            }
            return result.ToArray();
        }

        public UniformMappedData[] GetUniformDatasFromMaterial(Material material)
        {
            // ensure the material and shader is valid
            if (material == null || material.shader == null) return new UniformMappedData[] { };

            List<UniformMappedData> results = new List<UniformMappedData>();
            foreach (UniformMapping mapping in uniformMappings)
            {
                results.Add(new UniformMappedData(mapping, material));
            }
            return results.ToArray();
        }

        public UniformMappedData[] GetUniformDatasOfTypeFromMaterial(UniformMapping.UniformType type, Material material)
        {
            // ensure the material and shader is valid
            if (material == null || material.shader == null) return null;

            UniformMapping[] texmappings = GetUniformMappingsOfType(type);
            List<UniformMappedData> results = new List<UniformMappedData>();
            foreach (UniformMapping mapping in texmappings)
            {
                results.Add(new UniformMappedData(mapping, material));
            }
            return results.ToArray();
        }

        public static readonly ShaderMapping defaultShaderMapping = new ShaderMapping
        {

        };
    }

    /// <summary>
    /// ShaderMappingIO
    /// Functions for reading and writting shadermapping json files
    /// </summary>
    
    public static class ShaderMappingIO
    {
        public static Dictionary<string, ShaderMapping> _shaderMappingCache = new Dictionary<string, ShaderMapping>();
        public static string _shaderMappingsDirectory = string.Empty;


        static ShaderMappingIO() 
        {
            SceneGraphExporter.OnBeginExport += ClearCaches;
            SceneGraphExporter.OnEndExport += ClearCaches;
        }

        private static void ClearCaches()
        {
            _shaderMappingCache.Clear();
            _shaderMappingsDirectory = string.Empty;
        }

        //
        // functions for read/write

        public static ShaderMapping ReadFromJsonFile(string filePath)
        {
            ShaderMapping mapping;

            if (_shaderMappingCache.ContainsKey(filePath))
            {
                mapping = _shaderMappingCache[filePath];
            }
            else
            {
                if (!File.Exists(filePath)) return null;

                string contents = File.ReadAllText(filePath);
                mapping = JsonUtility.FromJson<ShaderMapping>(contents);

                // expand paths to shaders
                string dir = Path.GetDirectoryName(filePath);
                for (int i = 0; i < mapping.shaders.Count; i++)
                {
                    mapping.shaders[i].sourceFile = Path.Combine(dir, mapping.shaders[i].sourceFile);
                }

                _shaderMappingCache.Add(filePath, mapping);
            }

            return mapping;
        }

        public static void WriteToJsonFile(ShaderMapping mapping, string filePath)
        {
            string contents = JsonUtility.ToJson(mapping, true);
            File.WriteAllText(filePath, contents);
        }

        public static string GetFileNameForShaderMapping(string name)
        {
            return name.Replace("/", "+") + "-ShaderMapping";
        }

        public static string GetPathForAsset(string assetName)
        {
            string[] assetGUID = AssetDatabase.FindAssets(assetName);
            if (assetGUID == null || assetGUID.Length == 0) return string.Empty;
            string datapath = Application.dataPath;
            datapath = datapath.Remove(datapath.Length - ("Assets").Length);
            return Path.Combine(datapath, AssetDatabase.GUIDToAssetPath(assetGUID[0]));
        }

        public static string GetPathForShaderMappingFile(string name)
        {
            string path = string.Empty;
            if (string.IsNullOrEmpty(_shaderMappingsDirectory))
            {
                path = GetPathForAsset(GetFileNameForShaderMapping(name));
                if(!string.IsNullOrEmpty(path)) _shaderMappingsDirectory = Path.GetDirectoryName(path);
            }
            else
            {
                path = Path.Combine(_shaderMappingsDirectory, GetFileNameForShaderMapping(name) + ".json");
            }
            return path;
        }

        public static string GetPathForShaderMappingFile(Shader shader)
        {
            return GetPathForShaderMappingFile(shader.name);
        }

        public static ShaderMapping ReadFromJsonFile(Shader shader)
        {
            return ReadFromJsonFile(GetPathForShaderMappingFile(shader));
        }

        //
        // function for creating a template shadermapping json file for a specific shader

        public static ShaderMapping CreateTemplateForShader(Shader shader)
        {
            ShaderMapping mapping = new ShaderMapping()
            {
                unityShaderName = shader.name,
                shaders = new List<ShaderResource>()
                {
                    new ShaderResource() { sourceFile = shader.name, stages = VkShaderStageFlagBits.VK_SHADER_STAGE_VERTEX_BIT | VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT }
                }
            };

            mapping.vertexDependancies = new List<VertexAttributeDependancies>
            {
                new VertexAttributeDependancies { attributeType = VertexAttribute.Position, dependantDefines = new List<string>{ "ALL" } },
                new VertexAttributeDependancies { attributeType = VertexAttribute.Normal, dependantDefines = new List<string>{ "VSG_LIGHTING", "VSG_NORMAL_MAP" } },
                new VertexAttributeDependancies { attributeType = VertexAttribute.Tangent, dependantDefines = new List<string>{ "VSG_NORMAL_MAP" } },
                new VertexAttributeDependancies { attributeType = VertexAttribute.Color, dependantDefines = new List<string>{ "NONE" } },
                new VertexAttributeDependancies { attributeType = VertexAttribute.TexCoord0, dependantDefines = new List<string>{ "VSG_DIFFUSE_MAP", "VSG_NORMAL_MAP" } }
            };

            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                ShaderUtil.ShaderPropertyType proptype = ShaderUtil.GetPropertyType(shader, i);
                string propname = ShaderUtil.GetPropertyName(shader, i);

                if (proptype == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    TextureDimension texdims = ShaderUtil.GetTexDim(shader, i);

                    UniformMapping.UniformType textype = UniformMapping.UniformType.UnkownUniform;
                    switch (texdims)
                    {
                        case TextureDimension.Tex2D:
                            {
                                textype = UniformMapping.UniformType.Texture2DUniform;
                                break;
                            }
                        case TextureDimension.Tex2DArray:
                            {
                                textype = UniformMapping.UniformType.Texture2DArrayUniform;
                                break;
                            }
                        default: break;
                    }

                    if (textype == UniformMapping.UniformType.UnkownUniform) continue;

                    mapping.uniformMappings.Add(new UniformMapping() { unityPropName = propname, uniformType = textype });
                }
                else if (proptype == ShaderUtil.ShaderPropertyType.Float || proptype == ShaderUtil.ShaderPropertyType.Range)
                {
                    mapping.uniformMappings.Add(new UniformMapping() { unityPropName = propname, uniformType = UniformMapping.UniformType.FloatUniform });
                }
                else if (proptype == ShaderUtil.ShaderPropertyType.Vector)
                {
                    mapping.uniformMappings.Add(new UniformMapping() { unityPropName = propname, uniformType = UniformMapping.UniformType.Vec4Uniform });
                }
                else if (proptype == ShaderUtil.ShaderPropertyType.Color)
                {
                    mapping.uniformMappings.Add(new UniformMapping() { unityPropName = propname, uniformType = UniformMapping.UniformType.ColorUniform });
                }
            }
            return mapping;
        }

        public static void CreateTemplateFileForShader(Shader shader)
        {
            ShaderMapping mapping = CreateTemplateForShader(shader);
            string filePath = Path.Combine(Application.dataPath, GetFileNameForShaderMapping(shader.name) + "-Template.json");
            WriteToJsonFile(mapping, filePath);
        }
    }
}
