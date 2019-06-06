using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace vsgUnity
{
    [System.Serializable]
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

        [System.NonSerialized]
        public UniformType uniformType;

        [SerializeField]
        protected string uniformTypeString; // this is used for serialization so we can store enum as string in json 

        // unity side data
        public string unityPropName; // name of the property in the unity shader

        // vsg side data
        public int vsgBindingIndex; // the descriptor binding index of the uniorm in the vsg shader
        public string vsgDefine; // any custom define in the vsg shader associated with the uniform

        //
        // ISerializationCallbackReceiver implementations

        public void OnBeforeSerialize()
        {
            uniformTypeString = uniformType.ToString();
        }

        public void OnAfterDeserialize()
        {
            if (!System.Enum.TryParse<UniformType>(uniformTypeString, out uniformType)) uniformType = UniformType.UnkownUniform;
        }
    }

    [System.Serializable]
    public class ShaderMapping
    {
        // unity side data
        public string unityShaderName; // name of the unity shader

        // vsg side data
        public string vsgShaderName; // base name of the vsg shader, .vert and .frag extensions are added to get the two files needed

        public List<UniformMapping> uniformMappings; // mappings of unity properties/uniforms to vsg descriptors/uniforms

        public static ShaderMapping readFromJsonFile<T>(string filePath) where T : ShaderMapping
        {
            if (!File.Exists(filePath)) return null;

            string contents = File.ReadAllText(filePath);
            return JsonUtility.FromJson<T>(contents);
        }

        public static void writeToJsonFile(ShaderMapping mapping, string filePath)
        {
            string contents = JsonUtility.ToJson(mapping, true);
            File.WriteAllText(filePath, contents);
        }

        public static void createTemplateFile()
        {
            string filePath = Path.Combine(Application.dataPath, "templateMaterialMapping.json");

            ShaderMapping mapping = new ShaderMapping()
            {
                unityShaderName = "Standard",
                vsgShaderName = "standardShader",
                uniformMappings = new List<UniformMapping>()
                {
                    new UniformMapping() { uniformType = UniformMapping.UniformType.Texture2DUniform, unityPropName = "_MainTex", vsgBindingIndex = 0, vsgDefine = "VSG_DIFFUSE_MAP" }
                }
            };

            writeToJsonFile(mapping, filePath);
        }

        public static void createTemplateForShader(Shader shader)
        {
            string filePath = Path.Combine(Application.dataPath, shader.name + "Mapping.json");

            ShaderMapping mapping = new ShaderMapping()
            {
                unityShaderName = shader.name,
                vsgShaderName = shader.name
            };

            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                ShaderUtil.ShaderPropertyType proptype = ShaderUtil.GetPropertyType(shader, i);
                string propname = ShaderUtil.GetPropertyName(shader, i);

                if (proptype == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    TextureDimension texdims = ShaderUtil.GetTexDim(shader, i);

                    UniformMapping.UniformType textype = UniformMapping.UniformType.UnkownUniform;
                    switch(texdims)
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
                else if(proptype == ShaderUtil.ShaderPropertyType.Float)
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

            writeToJsonFile(mapping, filePath);
        }
    }

    [System.Serializable]
    public class TerrainMaterialMapping : ShaderMapping
    {
        public List<UniformMapping> layerDiffuseMappings; // if empty use standard terrain layers, if multiple Texture2Ds combine into array, if single Texture2DArray use directly
        public List<UniformMapping> layerNormalMappings; // if empty use standard terrain layers, if multiple Texture2Ds combine into array, if single Texture2DArray use directly
        public int layerCountConstantIndex;

        public List<UniformMapping> splatMappings; // if empty use standard terrain splats, if multiple Texture2Ds combine into array, if single Texture2DArray use directly
        public int splatCountConstantIndex;
    }
}
