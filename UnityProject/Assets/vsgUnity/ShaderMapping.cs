using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace vsgUnity
{
    [Serializable]
    public class ShaderResource
    {
        public VkShaderStageFlagBits stages;
        public UnityEngine.Object source;
        public string entryPointName = "main";

        public string GetSourceFilePath() 
        {
            return Directory.GetParent(Application.dataPath).FullName + "/" + AssetDatabase.GetAssetPath(source);
        }
    }

    [Serializable]
    public class VertexAttributeDependancies
    {
        public VertexAttribute attributeType;
        public List<string> dependantDefines = new List<string>(); // shader defines that depend on this input
    }

    [Serializable]
    public class ShaderMapping : ScriptableObject
    {
        public Shader sourceShader;
        public List<ShaderResource> shaders = new List<ShaderResource>();
        public List<UniformMapping> uniformMappings = new List<UniformMapping>();
        public List<VertexAttributeDependancies> vertexDependancies = new List<VertexAttributeDependancies>(); // vertex inputs for this shader
    }

    public static class ShaderMappings
    {
        private static Dictionary<Shader, ShaderMapping> _shaderMappingCache = new Dictionary<Shader, ShaderMapping>();

        [OnBeginExport]
        private static void refreshCache() 
        {
            clearCache();
            foreach (var guid in AssetDatabase.FindAssets("t:" + typeof(ShaderMapping).Name))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mapping = AssetDatabase.LoadMainAssetAtPath(path) as ShaderMapping;
                if (mapping && mapping.sourceShader) 
                {
                    _shaderMappingCache.Add(mapping.sourceShader, mapping);
                }
            }
        }

        [OnEndExport]
        private static void clearCache()
        {
            _shaderMappingCache.Clear();
        }

        public static ShaderMapping GetShaderMapping(Shader shader)
        {
            if (!shader)
                return null;
            if (_shaderMappingCache.TryGetValue(shader, out ShaderMapping mapping))
                return mapping;
            Debug.LogWarning("No shader mapping found for shader: " + shader.name);
            return null;
        }

#if UNITY_EDITOR
        
        [MenuItem("Assets/Create/VSG/Shader Mapping")]
        static void CreateShaderMapping()
        {
            Shader activeShader = Selection.activeObject as Shader;
            if (activeShader) {
                var mapping = createAsset(activeShader.name);
                initMappingFromShader(mapping, activeShader);
            } else {
                createAsset("NewShaderMapping");
            } 
        }

        [MenuItem("CONTEXT/ShaderMapping/Initialize From Source Shader")]
        static void InitShaderMapping(MenuCommand command)
        {
            var mapping = command.context as ShaderMapping;
            if (mapping)
                initMappingFromShader(mapping, mapping.sourceShader);
        }

        private static void initMappingFromShader(ShaderMapping mapping, Shader shader)
        {
            mapping.sourceShader = shader;

            mapping.shaders = new List<ShaderResource>()
            {
                new ShaderResource() { stages = VkShaderStageFlagBits.VK_SHADER_STAGE_VERTEX_BIT },
                new ShaderResource() { stages = VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT }
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
                if (tryGetUniformMapping(shader, i, out UniformMapping uniformMapping)) 
                    mapping.uniformMappings.Add(uniformMapping);
            }
        }

        private static bool tryGetUniformMapping(Shader shader, int property, out UniformMapping mapping) 
        {
            mapping = new UniformMapping();
            mapping.mappingSources.Add(new UniformMapping.UniformSource());
            mapping.mappingSources[0].unityPropName = ShaderUtil.GetPropertyName(shader, property);
            switch (ShaderUtil.GetPropertyType(shader, property)) 
            {
                case ShaderUtil.ShaderPropertyType.TexEnv:
                    switch (ShaderUtil.GetTexDim(shader, property))
                    {
                        case TextureDimension.Tex2D:
                            mapping.mappingSources[0].uniformType = UniformMapping.UniformType.Texture2DUniform;
                            return true;
                        case TextureDimension.Tex2DArray:
                            mapping.mappingSources[0].uniformType = UniformMapping.UniformType.Texture2DArrayUniform;
                            return true;
                    }
                    return false;
                case ShaderUtil.ShaderPropertyType.Float:
                case ShaderUtil.ShaderPropertyType.Range:
                    mapping.mappingSources[0].uniformType = UniformMapping.UniformType.FloatUniform;
                    return true;
                case ShaderUtil.ShaderPropertyType.Vector:
                    mapping.mappingSources[0].uniformType = UniformMapping.UniformType.Vec4Uniform;
                    return true;
                case ShaderUtil.ShaderPropertyType.Color:
                    mapping.mappingSources[0].uniformType = UniformMapping.UniformType.ColorUniform;
                    return true;
            }
            return false;
        }

        private static ShaderMapping createAsset(string name) 
        {
            var asset = ScriptableObject.CreateInstance<ShaderMapping> ();
            var path =  getAssetPath(name);
            AssetDatabase.CreateAsset (asset, path);
            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(asset);
            return asset;
        }

        private static string getAssetPath(string name) 
        {
            // get current path
            string path = "Assets";
            foreach (var obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                var objPath = AssetDatabase.GetAssetPath(obj);
                if (File.Exists(objPath)) 
                {
                    path = Path.GetDirectoryName(objPath);
                    break;
                }  
            }
            return AssetDatabase.GenerateUniqueAssetPath (path + "/" + name.Replace("/", "-") + ".asset");
        }
#endif
    }

}
