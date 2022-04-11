/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth
Copyright(c) 2022 Christian Schott (InstruNEXT GmbH)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

using vsgUnity.Native;

namespace vsgUnity
{
    [Serializable]
    public class UniformMapping
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

        [Serializable]
        public class UniformSource 
        {
            public UniformType uniformType;
            public string unityPropName;
            public string[] valueConversionExpressions;

            public bool IsTextureUniform() 
            {
                return uniformType == UniformType.Texture2DUniform || uniformType == UniformType.Texture2DArrayUniform;
            }

            public bool TryGetTextureUniformFromMaterial(Material material, out Texture texture)
            {
                texture = null;
                if (IsTextureUniform())
                {
                    if (material.HasProperty(unityPropName)) {
                        texture = material.GetTexture(unityPropName);
                        return texture != null;
                    } else {
                        // TODO: default texture
                        //int index = material.shader.FindPropertyIndex(unityPropName);
                        //return material.shader.GetPropertyTextureDefaultName(index);
                    }
                }
                return false;
            }

            public bool TryGetConvertedFloatUniformDataFromMaterial(Material material, out float[] data)
            {
                if (!TryGetFloatUniformFromMaterial(material, out data)) {
                    int count = GetFloatUniformCount();
                    if (count == 0)
                        return false;
                    data = new float[count];
                }
                if (valueConversionExpressions != null) {
                    var vals = new object[data.Length]; // wrap floats in object[] so String.Format() uses proper overload
                    for (int i = 0; i < data.Length; i++) 
                        vals[i] = data[i];
                    for (int i = 0; i < valueConversionExpressions.Length && i < data.Length; i++) {
                        var expression = String.Format(valueConversionExpressions[i], vals);
                        if (ExpressionEvaluator.Evaluate(expression, out float result)) {
                            data[i] = result;
                        } else {
                            throw new Exception("Evaluating Uniform Source failed " + expression);
                        }
                    }
                }
                return true;
            }

            public int GetFloatUniformCount()
            {
                switch (uniformType) {
                    case UniformType.FloatUniform:
                        return 1;
                    case UniformType.Vec4Uniform:
                    case UniformType.ColorUniform:
                        return 4;
                    case UniformType.Matrix4x4Uniform:
                        return 16;
                }
                return 0;
            }

            public bool TryGetFloatUniformFromMaterial(Material material, out float[] data) 
            {
                data = null;
                if (material.HasProperty(unityPropName)) {
                    switch (uniformType) {
                        case UniformType.FloatUniform:
                            data = ToFloatArray(material.GetFloat(unityPropName));
                            return true;
                        case UniformType.Vec4Uniform:
                            data = ToFloatArray(material.GetVector(unityPropName));
                            return true;
                        case UniformType.ColorUniform:
                            data = ToFloatArray(material.GetColor(unityPropName));
                            return true;
                        case UniformType.Matrix4x4Uniform:
                            data = ToFloatArray(material.GetMatrix(unityPropName));
                            return true;
                    }
                } else {
                    int index = material.shader.FindPropertyIndex(unityPropName);
                    if (index != -1) {
                        switch (uniformType) {
                            case UniformType.FloatUniform:
                                data = ToFloatArray(material.shader.GetPropertyDefaultFloatValue(index));
                                return true;
                            case UniformType.Vec4Uniform:
                            case UniformType.ColorUniform:
                                data = ToFloatArray(material.shader.GetPropertyDefaultVectorValue(index));
                                return true;
                        }
                    }
                }
                return false;
            }

            private float[] ToFloatArray(float value)
            {
                return new float[] { value };
            }

            private float[] ToFloatArray(Vector4 value)
            {
                return new float[] { value.x, value.y, value.z, value.w };
            }

            private float[] ToFloatArray(Color value)
            {
                return new float[] { value.r, value.g, value.b, value.a };
            }

            private float[] ToFloatArray(Matrix4x4 value)
            {
                return new float[]
                {
                    value[0, 0], value[1, 0], value[2, 0], value[3, 0], /* column 0 */
                    value[0, 1], value[1, 1], value[2, 1], value[3, 1], /* column 1 */
                    value[0, 2], value[1, 2], value[2, 2], value[3, 2], /* column 2 */
                    value[0, 3], value[1, 3], value[2, 3], value[3, 3]  /* column 3 */
                };
            }
        }

        public VkShaderStageFlagBits stages;
        public List<UniformSource> mappingSources = new List<UniformSource>();
        public int vsgBindingIndex; // the descriptor binding index of the uniorm in the vsg shader
        public List<string> vsgDefines = new List<string>(); // any custom defines in the vsg shader associated with the uniform

        public bool TryGetTextureUniform(Material material, out Texture texture) 
        {
            texture = null;
            if (mappingSources.Count == 1) 
                return mappingSources[0].TryGetTextureUniformFromMaterial(material, out texture);
            return false;
        }

        public bool TryGetFloatUniformData(Material material, out FloatArray floatArray)
        {
            List<float> buffer = new List<float>();
            foreach(var mappingSource in mappingSources) {
                if (mappingSource.TryGetConvertedFloatUniformDataFromMaterial(material, out float[] data))
                    buffer.AddRange(data);
            }

            floatArray = NativeUtils.WrapArray(buffer.ToArray());
            return buffer.Count > 0;
        }

        public VkDescriptorType GetDescriptorType() 
        {
            VkDescriptorType descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_MAX_ENUM;
            for (int i = 0; i < mappingSources.Count; i++) {
                if (mappingSources[i].IsTextureUniform())
                {
                    // only allow one mapping source for Texture2D uniforms
                    if (i > 0)
                        return VkDescriptorType.VK_DESCRIPTOR_TYPE_MAX_ENUM;
                    descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER;
                } else {
                    descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
                }
            }
            return descriptorType;
        } 
    }
}
