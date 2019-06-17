/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace vsgUnity.Native
{

    public static class Library
    {
        //public const string libraryName = "unity2vsgd";
        public const string libraryName = "unity2vsg";
    }

    //
    // Array types
    //

    public struct NativeArray
    {
        public IntPtr data;
        public int length;
    }
    
    public struct ByteArray : IEquatable<ByteArray>
    {
        public byte[] data;
        public int length;

        public bool Equals(ByteArray b)
        {
            if (data == null && b.data == null) return true;
            if (data == null || b.data == null) return false;
            return length == b.length &&
                data.SequenceEqual<byte>(b.data);
        }
    }

    public struct IntArray : IEquatable<IntArray>
    {
        public int[] data;
        public int length;

        public bool Equals(IntArray b)
        {
            if (data == null && b.data == null) return true;
            if (data == null || b.data == null) return false;
            return length == b.length &&
                data.SequenceEqual<int>(b.data);
        }
    }

    public struct UIntArray : IEquatable<UIntArray>
    {
        public uint[] data;
        public int length;

        public bool Equals(UIntArray b)
        {
            if (data == null && b.data == null) return true;
            if (data == null || b.data == null) return false;
            return length == b.length &&
                data.SequenceEqual<uint>(b.data);
        }
    }

    public struct FloatArray : IEquatable<FloatArray>
    {
        public float[] data;
        public int length;

        public bool Equals(FloatArray b)
        {
            if (data == null && b.data == null) return true;
            if (data == null || b.data == null) return false;
            return length == b.length &&
                data.SequenceEqual<float>(b.data);
        }
    }

    public struct Vec2Array : IEquatable<Vec2Array>
    {
        public Vector2[] data;
        public int length;

        public bool Equals(Vec2Array b)
        {
            if (data == null && b.data == null) return true;
            if (data == null || b.data == null) return false;
            return length == b.length &&
                data.SequenceEqual<Vector2>(b.data);
        }
    }

    public struct Vec3Array : IEquatable<Vec3Array>
    {
        public Vector3[] data;
        public int length;

        public bool Equals(Vec3Array b)
        {
            if (data == null && b.data == null) return true;
            if (data == null || b.data == null) return false;
            return length == b.length &&
                data.SequenceEqual<Vector3>(b.data);
        }
    }

    public struct Vec4Array : IEquatable<Vec4Array>
    {
        public Vector4[] data;
        public int length;

        public bool Equals(Vec4Array b)
        {
            if (data == null && b.data == null) return true;
            if (data == null || b.data == null) return false;
            return length == b.length &&
                data.SequenceEqual<Vector4>(b.data);
        }
    }

    public struct ColorArray : IEquatable<ColorArray>
    {
        public Color[] data;
        public int length;

        public bool Equals(ColorArray b)
        {
            if (data == null && b.data == null) return true;
            if (data == null || b.data == null) return false;
            return length == b.length &&
                data.SequenceEqual<Color>(b.data);
        }
    }

    public struct DescriptorSetLayoutBindingsArray : IEquatable<DescriptorSetLayoutBindingsArray>
    {
        public VkDescriptorSetLayoutBinding[] data;
        public int length;

        public bool Equals(DescriptorSetLayoutBindingsArray b)
        {
            if (data == null && b.data == null) return true;
            if (data == null || b.data == null) return false;
            return length == b.length &&
                data.SequenceEqual<VkDescriptorSetLayoutBinding>(b.data);
        }
    }

    //
    // Mesh/Vertex/Draw command types
    //

    public struct VertexIndexDrawData : IEquatable<VertexIndexDrawData>
    {
        public int id;
        public Vec3Array verticies;
        public IntArray triangles;
        public Vec3Array normals;
        public Vec4Array tangents;
        public ColorArray colors;
        public Vec2Array uv0;
        public Vec2Array uv1;
        public int use32BitIndicies;

        public bool Equals(VertexIndexDrawData b)
        {
            return use32BitIndicies == b.use32BitIndicies &&
                verticies.Equals(b.verticies) &&
                triangles.Equals(b.triangles) &&
                normals.Equals(b.normals) &&
                tangents.Equals(b.tangents) &&
                colors.Equals(b.colors) &&
                uv0.Equals(b.uv0) &&
                uv1.Equals(b.uv1);
        }
    }

    public struct IndexBufferData : IEquatable<IndexBufferData>
    {
        public int id; // same as mesh id
        public IntArray triangles;
        public int use32BitIndicies;

        public bool Equals(IndexBufferData b)
        {
            return use32BitIndicies == b.use32BitIndicies &&
                triangles.Equals(b.triangles);
        }
    }

    public struct VertexBuffersData : IEquatable<VertexBuffersData>
    {
        public int id; // same as mesh id
        public Vec3Array verticies;
        public Vec3Array normals;
        public Vec4Array tangents;
        public ColorArray colors;
        public Vec2Array uv0;
        public Vec2Array uv1;

        public bool Equals(VertexBuffersData b)
        {
            return verticies.Equals(b.verticies) &&
                normals.Equals(b.normals) &&
                tangents.Equals(b.tangents) &&
                colors.Equals(b.colors) &&
                uv0.Equals(b.uv0) &&
                uv1.Equals(b.uv1);
        }
    }

    public struct DrawIndexedData : IEquatable<DrawIndexedData>
    {
        public int id;
        public uint indexCount;
        public uint firstIndex;
        public uint vertexOffset;
        public uint instanceCount;
        public uint firstInstance;

        public bool Equals(DrawIndexedData b)
        {
            return indexCount == b.indexCount &&
                firstIndex == b.firstIndex &&
                vertexOffset == b.vertexOffset &&
                instanceCount == b.instanceCount &&
                firstInstance == b.firstInstance;
        }
    }

    //
    // Image types
    //

    public struct ImageData : IEquatable<ImageData>
    {
        public int id;
        public NativeArray pixels;
        public VkFormat format;
        public int width;
        public int height;
        public int depth;
        public int anisoLevel;
        public VkSamplerAddressMode wrapMode;
        public VkFilter filterMode;
        public VkSamplerMipmapMode mipmapMode;
        public int mipmapCount;
        public float mipmapBias;

        public bool Equals(ImageData b)
        {
            return format == b.format &&
                width == b.width &&
                height == b.height &&
                depth == b.depth &&
                anisoLevel == b.anisoLevel &&
                wrapMode == b.wrapMode &&
                filterMode == b.filterMode &&
                mipmapMode == b.mipmapMode &&
                mipmapCount == b.mipmapCount &&
                mipmapBias == b.mipmapBias &&
                pixels.Equals(b.pixels);
        }
    }

    //
    // Descriptor types
    //

    public struct DescriptorImageData : IEquatable<DescriptorImageData>
    {
        public int id;
        public int binding;
        public ImageData[] image;
        public int descriptorCount;

        public bool Equals(DescriptorImageData b)
        {
            return binding == b.binding && image.Equals(b.image);
        }
    }

    public struct DescriptorFloatUniformData : IEquatable<DescriptorFloatUniformData>
    {
        public int id;
        public int binding;
        public float value;

        public bool Equals(DescriptorFloatUniformData b)
        {
            return binding == b.binding && value == b.value;
        }
    }

    public struct DescriptorFloatArrayUniformData : IEquatable<DescriptorFloatArrayUniformData>
    {
        public int id;
        public int binding;
        public FloatArray value;

        public bool Equals(DescriptorFloatArrayUniformData b)
        {
            return binding == b.binding && value.Equals(b.value);
        }
    }

    public struct DescriptorVectorUniformData : IEquatable<DescriptorVectorUniformData>
    {
        public int id;
        public int binding;
        public Vector4 value;

        public bool Equals(DescriptorVectorUniformData b)
        {
            return binding == b.binding && value.Equals(b.value);
        }
    }

    public struct DescriptorVectorArrayUniformData : IEquatable<DescriptorVectorArrayUniformData>
    {
        public int id;
        public int binding;
        public Vec4Array value;

        public bool Equals(DescriptorVectorArrayUniformData b)
        {
            return binding == b.binding && value.Equals(b.value);
        }
    }

    //
    // Shader and pipeline types
    //

    public struct ShaderStageData : IEquatable<ShaderStageData>
    {
        public int id;
        public VkShaderStageFlagBits stages;
        public NativeArray specializationData;
        public IntPtr customDefines;
        public IntPtr source;

        public bool Equals(ShaderStageData b)
        {
            return stages == b.stages &&
                source == b.source &&
                customDefines == b.customDefines &&
                specializationData.Equals(b.specializationData);
        }
    }

    public struct ShaderStagesData: IEquatable<ShaderStagesData>
    {
        public int id;
        public ShaderStageData[] stages;
        public int stagesCount;

        public bool Equals(ShaderStagesData b)
        {
            if (stages == null && b.stages == null) return true;
            if (stages == null || b.stages == null) return false;
            return stagesCount == b.stagesCount &&
                stages.SequenceEqual<ShaderStageData>(b.stages);
        }
    }

    public struct PipelineData : IEquatable<PipelineData>
    {
        public IntPtr id;
        public int hasNormals;
        public int hasTangents;
        public int hasColors;
        public int uvChannelCount;
        public int useAlpha;
        public DescriptorSetLayoutBindingsArray descriptorBindings;
        public ShaderStagesData shaderStages;

        public bool Equals(PipelineData b)
        {
            return hasNormals == b.hasNormals &&
                hasTangents == b.hasTangents &&
                hasColors == b.hasColors &&
                uvChannelCount == b.uvChannelCount &&
                useAlpha == b.useAlpha &&
                descriptorBindings.Equals(b.descriptorBindings) &&
                shaderStages.Equals(b.shaderStages);
        }
    };

    //
    // Node creation types
    //

    public struct TransformData
    {
        public FloatArray matrix;
    }

    public struct CullData
    {
        public Vector3 center;
        public float radius;
    }

    public struct LODChildData
    {
        public float minimumScreenHeightRatio;
    }

    public struct CameraData
    {
        public Vector3 position;
        public Vector3 lookAt;
        public Vector3 upDir;
        public float fov;
        public float nearZ;
        public float farZ;
    }

    public static class NativeUtils
    {
        public static PipelineData CreatePipelineData(MeshInfo meshData)
        {
            PipelineData pipeline = new PipelineData();
            pipeline.hasNormals = meshData.normals.length > 0 ? 1 : 0;
            pipeline.hasTangents = meshData.tangents.length > 0 ? 1 : 0;
            pipeline.hasColors = meshData.colors.length > 0 ? 1 : 0;
            pipeline.uvChannelCount = 0;
            pipeline.uvChannelCount += meshData.uv0.length > 0 ? 1 : 0;
            pipeline.uvChannelCount += meshData.uv1.length > 0 ? 1 : 0;
            return pipeline;
        }

        public static string GetIDForPipeline(PipelineData data)
        {
            string idstr = "";
            idstr += data.hasNormals == 1 ? "1" : "0";
            idstr += data.hasTangents == 1 ? "1" : "0";
            idstr += data.hasColors == 1 ? "1" : "0";
            idstr += data.uvChannelCount.ToString();
            idstr += data.useAlpha == 1 ? "1" : "0";
            idstr += data.descriptorBindings.length.ToString(); // need better id for these
            idstr += data.shaderStages.id.ToString();
            return idstr;
        }

        public static CameraData CreateCameraData(Camera camera)
        {
            CameraData camdata = new CameraData();
            camdata.position = camera.gameObject.transform.position;
            camdata.lookAt = camdata.position + camera.gameObject.transform.forward;
            camdata.upDir = camera.gameObject.transform.up;
            camdata.fov = camera.fieldOfView;
            camdata.nearZ = camera.nearClipPlane;
            camdata.farZ = camera.farClipPlane;
            return camdata;
        }

        public static ByteArray WrapArray(byte[] anArray)
        {
            ByteArray result;
            result.length = anArray != null ? anArray.Length : 0;
            result.data = anArray;
            return result;
        }

        public static IntArray WrapArray(int[] anArray)
        {
            IntArray result;
            result.length = anArray != null ? anArray.Length : 0;
            result.data = anArray;
            return result;
        }

        public static UIntArray WrapArray(uint[] anArray)
        {
            UIntArray result;
            result.length = anArray != null ? anArray.Length : 0;
            result.data = anArray;
            return result;
        }

        public static FloatArray WrapArray(float[] anArray)
        {
            FloatArray result;
            result.length = anArray != null ? anArray.Length : 0;
            result.data = anArray;
            return result;
        }

        public static Vec2Array WrapArray(Vector2[] anArray)
        {
            Vec2Array result;
            result.length = anArray != null ? anArray.Length : 0;
            result.data = anArray;
            return result;
        }

        public static Vec3Array WrapArray(Vector3[] anArray)
        {
            Vec3Array result;
            result.length = anArray != null ? anArray.Length : 0;
            result.data = anArray;
            return result;
        }

        public static Vec4Array WrapArray(Vector4[] anArray)
        {
            Vec4Array result;
            result.length = anArray != null ? anArray.Length : 0;
            result.data = anArray;
            return result;
        }

        public static ColorArray WrapArray(Color[] anArray)
        {
            ColorArray result;
            result.length = anArray != null ? anArray.Length : 0;
            result.data = anArray;
            return result;
        }

        public static DescriptorSetLayoutBindingsArray WrapArray(VkDescriptorSetLayoutBinding[] anArray)
        {
            DescriptorSetLayoutBindingsArray result;
            result.length = anArray != null ? anArray.Length : 0;
            result.data = anArray;
            return result;
        }

        static List<IntPtr> _nativePointersCache = new List<IntPtr>();

        public static IntPtr ToNative(string str)
        {
            IntPtr ptr = Marshal.StringToHGlobalAnsi(str);
            _nativePointersCache.Add(ptr);
            return ptr;
        }

        public static NativeArray ToNative(IntArray array)
        {
            IntPtr ptr;
            if (array.length > 0)
            {
                ptr = Marshal.AllocCoTaskMem(sizeof(int) * array.length);
                Marshal.Copy(array.data, 0, ptr, array.length);
                _nativePointersCache.Add(ptr);
            }
            else
            {
                ptr = IntPtr.Zero;
            }

            NativeArray narray = new NativeArray
            {
                data = ptr,
                length = array.length
            };
            return narray;
        }

        public static NativeArray ToNative(ByteArray array)
        {
            IntPtr ptr;
            if (array.length > 0)
            {
                ptr = Marshal.AllocCoTaskMem(sizeof(byte) * array.length);
                Marshal.Copy(array.data, 0, ptr, array.length);
                _nativePointersCache.Add(ptr);
            }
            else
            {
                ptr = IntPtr.Zero;
            }

            NativeArray narray = new NativeArray
            {
                data = ptr,
                length = array.length
            };
            return narray;
        }

        public static NativeArray ToNative(byte[] array)
        {
            IntPtr ptr;
            if (array.Length > 0)
            {
                ptr = Marshal.AllocCoTaskMem(sizeof(byte) * array.Length);
                Marshal.Copy(array, 0, ptr, array.Length);
                _nativePointersCache.Add(ptr);
            }
            else
            {
                ptr = IntPtr.Zero;
            }

            NativeArray narray = new NativeArray
            {
                data = ptr,
                length = array.Length
            };
            return narray;
        }
    }

}
