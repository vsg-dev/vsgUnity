//----------------------------------------------
//            vsgUnity: Native
// Writen by Thomas Hogarth
// NativeUtils.cs
//----------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace vsgUnity.Native
{

    public static class Library
    {
        //public const string libraryName = "unity2vsgd";
        public const string libraryName = "unity2vsg";
    }

    //
    // Local Unity types, should match layout of types in unity2vg DataTypes.h, used to pass data from C# to native code
    //

    [StructLayout(LayoutKind.Sequential)]
    public struct ByteArray
    {
        public byte[] data;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IntArray
    {
        public int[] data;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FloatArray
    {
        public float[] data;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec2Array
    {
        public Vector2[] data;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec3Array
    {
        public Vector3[] data;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec4Array
    {
        public Vector4[] data;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ColorArray
    {
        public Color[] data;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MeshData
    {
        public string id;
        public Vec3Array verticies;
        public IntArray triangles;
        public Vec3Array normals;
        public Vec3Array tangents;
        public ColorArray colors;
        public Vec2Array uv0;
        public Vec2Array uv1;
        public int use32BitIndicies;
    }

    public struct IndexBufferData
    {
        public string id; // same as mesh id
        public IntArray triangles;
        public int use32BitIndicies;
    }

    public struct VertexBuffersData
    {
        public string id; // same as mesh id
        public Vec3Array verticies;
        public Vec3Array normals;
        public Vec3Array tangents;
        public Vec4Array colors;
        public Vec2Array uv0;
        public Vec2Array uv1;
    }

    public struct DrawIndexedData
    {
        public string id; // mesh id + sub mesh index
        public int indexCount;
        public int firstIndex;
        public int vertexOffset;
        public int instanceCount;
        public int firstInstance;
    }

    public enum VKTexFormat
    {
        UNDEFINED = 0,
        R4G4_UNORM_PACK8 = 1,
        R4G4B4A4_UNORM_PACK16 = 2,
        B4G4R4A4_UNORM_PACK16 = 3,
        R5G6B5_UNORM_PACK16 = 4,
        B5G6R5_UNORM_PACK16 = 5,
        R5G5B5A1_UNORM_PACK16 = 6,
        B5G5R5A1_UNORM_PACK16 = 7,
        A1R5G5B5_UNORM_PACK16 = 8,
        R8_UNORM = 9,
        R8_SNORM = 10,
        R8_USCALED = 11,
        R8_SSCALED = 12,
        R8_UINT = 13,
        R8_SINT = 14,
        R8_SRGB = 15,
        R8G8_UNORM = 16,
        R8G8_SNORM = 17,
        R8G8_USCALED = 18,
        R8G8_SSCALED = 19,
        R8G8_UINT = 20,
        R8G8_SINT = 21,
        R8G8_SRGB = 22,
        R8G8B8_UNORM = 23,
        R8G8B8_SNORM = 24,
        R8G8B8_USCALED = 25,
        R8G8B8_SSCALED = 26,
        R8G8B8_UINT = 27,
        R8G8B8_SINT = 28,
        R8G8B8_SRGB = 29,
        B8G8R8_UNORM = 30,
        B8G8R8_SNORM = 31,
        B8G8R8_USCALED = 32,
        B8G8R8_SSCALED = 33,
        B8G8R8_UINT = 34,
        B8G8R8_SINT = 35,
        B8G8R8_SRGB = 36,
        R8G8B8A8_UNORM = 37,
        R8G8B8A8_SNORM = 38,
        R8G8B8A8_USCALED = 39,
        R8G8B8A8_SSCALED = 40,
        R8G8B8A8_UINT = 41,
        R8G8B8A8_SINT = 42,
        R8G8B8A8_SRGB = 43,
        B8G8R8A8_UNORM = 44,
        B8G8R8A8_SNORM = 45,
        B8G8R8A8_USCALED = 46,
        B8G8R8A8_SSCALED = 47,
        B8G8R8A8_UINT = 48,
        B8G8R8A8_SINT = 49,
        B8G8R8A8_SRGB = 50,
        A8B8G8R8_UNORM_PACK32 = 51,
        A8B8G8R8_SNORM_PACK32 = 52,
        A8B8G8R8_USCALED_PACK32 = 53,
        A8B8G8R8_SSCALED_PACK32 = 54,
        A8B8G8R8_UINT_PACK32 = 55,
        A8B8G8R8_SINT_PACK32 = 56,
        A8B8G8R8_SRGB_PACK32 = 57,
        A2R10G10B10_UNORM_PACK32 = 58,
        A2R10G10B10_SNORM_PACK32 = 59,
        A2R10G10B10_USCALED_PACK32 = 60,
        A2R10G10B10_SSCALED_PACK32 = 61,
        A2R10G10B10_UINT_PACK32 = 62,
        A2R10G10B10_SINT_PACK32 = 63,
        A2B10G10R10_UNORM_PACK32 = 64,
        A2B10G10R10_SNORM_PACK32 = 65,
        A2B10G10R10_USCALED_PACK32 = 66,
        A2B10G10R10_SSCALED_PACK32 = 67,
        A2B10G10R10_UINT_PACK32 = 68,
        A2B10G10R10_SINT_PACK32 = 69,
        R16_UNORM = 70,
        R16_SNORM = 71,
        R16_USCALED = 72,
        R16_SSCALED = 73,
        R16_UINT = 74,
        R16_SINT = 75,
        R16_SFLOAT = 76,
        R16G16_UNORM = 77,
        R16G16_SNORM = 78,
        R16G16_USCALED = 79,
        R16G16_SSCALED = 80,
        R16G16_UINT = 81,
        R16G16_SINT = 82,
        R16G16_SFLOAT = 83,
        R16G16B16_UNORM = 84,
        R16G16B16_SNORM = 85,
        R16G16B16_USCALED = 86,
        R16G16B16_SSCALED = 87,
        R16G16B16_UINT = 88,
        R16G16B16_SINT = 89,
        R16G16B16_SFLOAT = 90,
        R16G16B16A16_UNORM = 91,
        R16G16B16A16_SNORM = 92,
        R16G16B16A16_USCALED = 93,
        R16G16B16A16_SSCALED = 94,
        R16G16B16A16_UINT = 95,
        R16G16B16A16_SINT = 96,
        R16G16B16A16_SFLOAT = 97,
        R32_UINT = 98,
        R32_SINT = 99,
        R32_SFLOAT = 100,
        R32G32_UINT = 101,
        R32G32_SINT = 102,
        R32G32_SFLOAT = 103,
        R32G32B32_UINT = 104,
        R32G32B32_SINT = 105,
        R32G32B32_SFLOAT = 106,
        R32G32B32A32_UINT = 107,
        R32G32B32A32_SINT = 108,
        R32G32B32A32_SFLOAT = 109,
        R64_UINT = 110,
        R64_SINT = 111,
        R64_SFLOAT = 112,
        R64G64_UINT = 113,
        R64G64_SINT = 114,
        R64G64_SFLOAT = 115,
        R64G64B64_UINT = 116,
        R64G64B64_SINT = 117,
        R64G64B64_SFLOAT = 118,
        R64G64B64A64_UINT = 119,
        R64G64B64A64_SINT = 120,
        R64G64B64A64_SFLOAT = 121,
        B10G11R11_UFLOAT_PACK32 = 122,
        E5B9G9R9_UFLOAT_PACK32 = 123,
        D16_UNORM = 124,
        X8_D24_UNORM_PACK32 = 125,
        D32_SFLOAT = 126,
        S8_UINT = 127,
        D16_UNORM_S8_UINT = 128,
        D24_UNORM_S8_UINT = 129,
        D32_SFLOAT_S8_UINT = 130,
        BC1_RGB_UNORM_BLOCK = 131,
        BC1_RGB_SRGB_BLOCK = 132,
        BC1_RGBA_UNORM_BLOCK = 133,
        BC1_RGBA_SRGB_BLOCK = 134,
        BC2_UNORM_BLOCK = 135,
        BC2_SRGB_BLOCK = 136,
        BC3_UNORM_BLOCK = 137,
        BC3_SRGB_BLOCK = 138,
        BC4_UNORM_BLOCK = 139,
        BC4_SNORM_BLOCK = 140,
        BC5_UNORM_BLOCK = 141,
        BC5_SNORM_BLOCK = 142,
        BC6H_UFLOAT_BLOCK = 143,
        BC6H_SFLOAT_BLOCK = 144,
        BC7_UNORM_BLOCK = 145,
        BC7_SRGB_BLOCK = 146,
        ETC2_R8G8B8_UNORM_BLOCK = 147,
        ETC2_R8G8B8_SRGB_BLOCK = 148,
        ETC2_R8G8B8A1_UNORM_BLOCK = 149,
        ETC2_R8G8B8A1_SRGB_BLOCK = 150,
        ETC2_R8G8B8A8_UNORM_BLOCK = 151,
        ETC2_R8G8B8A8_SRGB_BLOCK = 152,
        EAC_R11_UNORM_BLOCK = 153,
        EAC_R11_SNORM_BLOCK = 154,
        EAC_R11G11_UNORM_BLOCK = 155,
        EAC_R11G11_SNORM_BLOCK = 156,
        ASTC_4x4_UNORM_BLOCK = 157,
        ASTC_4x4_SRGB_BLOCK = 158,
        ASTC_5x4_UNORM_BLOCK = 159,
        ASTC_5x4_SRGB_BLOCK = 160,
        ASTC_5x5_UNORM_BLOCK = 161,
        ASTC_5x5_SRGB_BLOCK = 162,
        ASTC_6x5_UNORM_BLOCK = 163,
        ASTC_6x5_SRGB_BLOCK = 164,
        ASTC_6x6_UNORM_BLOCK = 165,
        ASTC_6x6_SRGB_BLOCK = 166,
        ASTC_8x5_UNORM_BLOCK = 167,
        ASTC_8x5_SRGB_BLOCK = 168,
        ASTC_8x6_UNORM_BLOCK = 169,
        ASTC_8x6_SRGB_BLOCK = 170,
        ASTC_8x8_UNORM_BLOCK = 171,
        ASTC_8x8_SRGB_BLOCK = 172,
        ASTC_10x5_UNORM_BLOCK = 173,
        ASTC_10x5_SRGB_BLOCK = 174,
        ASTC_10x6_UNORM_BLOCK = 175,
        ASTC_10x6_SRGB_BLOCK = 176,
        ASTC_10x8_UNORM_BLOCK = 177,
        ASTC_10x8_SRGB_BLOCK = 178,
        ASTC_10x10_UNORM_BLOCK = 179,
        ASTC_10x10_SRGB_BLOCK = 180,
        ASTC_12x10_UNORM_BLOCK = 181,
        ASTC_12x10_SRGB_BLOCK = 182,
        ASTC_12x12_UNORM_BLOCK = 183,
        ASTC_12x12_SRGB_BLOCK = 184,
        G8B8G8R8_422_UNORM = 1000156000,
        B8G8R8G8_422_UNORM = 1000156001,
        G8_B8_R8_3PLANE_420_UNORM = 1000156002,
        G8_B8R8_2PLANE_420_UNORM = 1000156003,
        G8_B8_R8_3PLANE_422_UNORM = 1000156004,
        G8_B8R8_2PLANE_422_UNORM = 1000156005,
        G8_B8_R8_3PLANE_444_UNORM = 1000156006,
        R10X6_UNORM_PACK16 = 1000156007,
        R10X6G10X6_UNORM_2PACK16 = 1000156008,
        R10X6G10X6B10X6A10X6_UNORM_4PACK16 = 1000156009,
        G10X6B10X6G10X6R10X6_422_UNORM_4PACK16 = 1000156010,
        B10X6G10X6R10X6G10X6_422_UNORM_4PACK16 = 1000156011,
        G10X6_B10X6_R10X6_3PLANE_420_UNORM_3PACK16 = 1000156012,
        G10X6_B10X6R10X6_2PLANE_420_UNORM_3PACK16 = 1000156013,
        G10X6_B10X6_R10X6_3PLANE_422_UNORM_3PACK16 = 1000156014,
        G10X6_B10X6R10X6_2PLANE_422_UNORM_3PACK16 = 1000156015,
        G10X6_B10X6_R10X6_3PLANE_444_UNORM_3PACK16 = 1000156016,
        R12X4_UNORM_PACK16 = 1000156017,
        R12X4G12X4_UNORM_2PACK16 = 1000156018,
        R12X4G12X4B12X4A12X4_UNORM_4PACK16 = 1000156019,
        G12X4B12X4G12X4R12X4_422_UNORM_4PACK16 = 1000156020,
        B12X4G12X4R12X4G12X4_422_UNORM_4PACK16 = 1000156021,
        G12X4_B12X4_R12X4_3PLANE_420_UNORM_3PACK16 = 1000156022,
        G12X4_B12X4R12X4_2PLANE_420_UNORM_3PACK16 = 1000156023,
        G12X4_B12X4_R12X4_3PLANE_422_UNORM_3PACK16 = 1000156024,
        G12X4_B12X4R12X4_2PLANE_422_UNORM_3PACK16 = 1000156025,
        G12X4_B12X4_R12X4_3PLANE_444_UNORM_3PACK16 = 1000156026,
        G16B16G16R16_422_UNORM = 1000156027,
        B16G16R16G16_422_UNORM = 1000156028,
        G16_B16_R16_3PLANE_420_UNORM = 1000156029,
        G16_B16R16_2PLANE_420_UNORM = 1000156030,
        G16_B16_R16_3PLANE_422_UNORM = 1000156031,
        G16_B16R16_2PLANE_422_UNORM = 1000156032,
        G16_B16_R16_3PLANE_444_UNORM = 1000156033,
        PVRTC1_2BPP_UNORM_BLOCK_IMG = 1000054000,
        PVRTC1_4BPP_UNORM_BLOCK_IMG = 1000054001,
        PVRTC2_2BPP_UNORM_BLOCK_IMG = 1000054002,
        PVRTC2_4BPP_UNORM_BLOCK_IMG = 1000054003,
        PVRTC1_2BPP_SRGB_BLOCK_IMG = 1000054004,
        PVRTC1_4BPP_SRGB_BLOCK_IMG = 1000054005,
        PVRTC2_2BPP_SRGB_BLOCK_IMG = 1000054006,
        PVRTC2_4BPP_SRGB_BLOCK_IMG = 1000054007
    }

    public enum MipmapFilterMode {
        Point = 0,
        Bilinear = 1,
        Trilinear = 2,
        Unsupported = 9999
    }

    public enum WrapMode {
        Repeat = 0,
        Clamp = 1,
        Mirror = 2,
        MirrorOnce = 3,
        Unsupported = 9999
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TextureData
    {
        public string id;
        public int channel;
        public ByteArray pixels;
        public VKTexFormat format;
        public int width;
        public int height;
        public int depth;
        public int anisoLevel;
        public WrapMode wrapMode;
        public MipmapFilterMode filterMode;
        public int mipmapCount;
        public float mipmapBias;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialData
    {
        public string id;
        public TextureData[] textures;
        public Vector4 diffuseColor;
        public int useAlpha;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PipelineData
    {
        public string id;
        public int hasNormals;
        public int hasTangents;
        public int hasColors;
        public int uvChannelCount;
        public int vertexImageSamplerCount;
        public int fragmentImageSamplerCount;
        public int vertexUniformCount;
        public int fragmentUniformCount;
        public int useAlpha;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct TransformData
    {
        public FloatArray matrix;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CullData
    {
        public Vector3 center;
        public float radius;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraData
    {
        public Vector3 position;
        public Vector3 lookAt;
        public Vector3 upDir;
        public float fov;
        public float nearZ;
        public float farZ;
    }

    //
    // Native types for data returned from native code to C#
    //

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeIntArray
    {
        public IntPtr ptr;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeFloatArray
    {
        public IntPtr ptr;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeVec2Array
    {
        public IntPtr ptr;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeVec3Array
    {
        public IntPtr ptr;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeVec4Array
    {
        public IntPtr ptr;
        public int length;
    }

    public static class NativeUtils
    {
        public static PipelineData CreatePipelineData(MeshData meshData)
        {
            PipelineData pipeline = new PipelineData();
            pipeline.hasNormals = meshData.normals.length > 0 ? 1 : 0;
            pipeline.hasTangents = meshData.tangents.length > 0 ? 1 : 0;
            pipeline.hasColors = meshData.colors.length > 0 ? 1 : 0;
            pipeline.uvChannelCount = meshData.uv0.length > 0 ? 1 : 0;
            return pipeline;
        }

        public static string GetIDForPipeline(PipelineData data)
        {
            string idstr = "";
            idstr += data.hasNormals == 1 ? "1" : "0";
            idstr += data.hasTangents == 1 ? "1" : "0";
            idstr += data.hasColors == 1 ? "1" : "0";
            idstr += data.uvChannelCount.ToString();
            idstr += data.vertexImageSamplerCount.ToString();
            idstr += data.vertexUniformCount.ToString();
            idstr += data.fragmentImageSamplerCount.ToString();
            idstr += data.fragmentUniformCount.ToString();
            idstr += data.useAlpha == 1 ? "1" : "0";
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

        public static MeshData CreateMeshData(Mesh mesh, int subMeshIndex = -1)
        {
            MeshData meshdata = new MeshData();
            meshdata.id = mesh.GetInstanceID().ToString() + (subMeshIndex >= 0 ? subMeshIndex.ToString() : "");

            meshdata.verticies = new Vec3Array();
            meshdata.verticies.data = mesh.vertices;
            meshdata.verticies.length = mesh.vertexCount;

            meshdata.triangles = new IntArray();
            meshdata.triangles.data = subMeshIndex >= 0 ? mesh.GetTriangles(subMeshIndex) : mesh.triangles;
            meshdata.triangles.length = meshdata.triangles.data.Length;
            meshdata.use32BitIndicies = mesh.indexFormat == IndexFormat.UInt32 ? 1 : 0;

            meshdata.normals = new Vec3Array();
            meshdata.normals.data = mesh.normals;
            meshdata.normals.length = meshdata.normals.data.Length;

            /*meshdata.tangents = new Vec3Array();
            meshdata.tangents.data = mesh.tangents;
            meshdata.tangents.length = meshdata.tangents.data.Length;*/

            /*meshdata.colors = new ColorArray();
            meshdata.colors.data = mesh.colors;
            meshdata.colors.length = meshdata.colors.data.Length;*/

            meshdata.uv0 = new Vec2Array();
            meshdata.uv0.data = mesh.uv;
            meshdata.uv0.length = meshdata.uv0.data.Length;

            return meshdata;
        }

        //
        // Textures
        //

        public static VKTexFormat GetTextureFormat(GraphicsFormat format)
        {
            switch (format)
            {
                case GraphicsFormat.None: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R8_SRGB: return VKTexFormat.R8_SRGB;
                case GraphicsFormat.R8G8_SRGB: return VKTexFormat.R8G8_SRGB;
                case GraphicsFormat.R8G8B8_SRGB: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R8G8B8A8_SRGB: return VKTexFormat.R8G8B8A8_SRGB;
                case GraphicsFormat.R8_UNorm: return VKTexFormat.R8_UNORM;
                case GraphicsFormat.R8G8_UNorm: return VKTexFormat.R8G8_UNORM;
                case GraphicsFormat.R8G8B8_UNorm: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R8G8B8A8_UNorm: return VKTexFormat.R8G8B8A8_UNORM;
                case GraphicsFormat.R8_SNorm: return VKTexFormat.R8_SNORM;
                case GraphicsFormat.R8G8_SNorm: return VKTexFormat.R8G8_SNORM;
                case GraphicsFormat.R8G8B8_SNorm: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R8G8B8A8_SNorm: return VKTexFormat.R8G8B8A8_SNORM;
                case GraphicsFormat.R8_UInt: return VKTexFormat.R8_UINT;
                case GraphicsFormat.R8G8_UInt: return VKTexFormat.R8G8_UINT;
                case GraphicsFormat.R8G8B8_UInt: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R8G8B8A8_UInt: return VKTexFormat.R8G8B8A8_UINT;
                case GraphicsFormat.R8_SInt: return VKTexFormat.R8_SINT;
                case GraphicsFormat.R8G8_SInt: return VKTexFormat.R8G8_SINT;
                case GraphicsFormat.R8G8B8_SInt: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R8G8B8A8_SInt: return VKTexFormat.R8G8B8A8_SINT;
                case GraphicsFormat.R16_UNorm: return VKTexFormat.R16_UNORM;
                case GraphicsFormat.R16G16_UNorm: return VKTexFormat.R16G16_UNORM;
                case GraphicsFormat.R16G16B16_UNorm: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R16G16B16A16_UNorm: return VKTexFormat.R16G16B16A16_UNORM;
                case GraphicsFormat.R16_SNorm: return VKTexFormat.R16_SNORM;
                case GraphicsFormat.R16G16_SNorm: return VKTexFormat.R16G16_SNORM;
                case GraphicsFormat.R16G16B16_SNorm: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R16G16B16A16_SNorm: return VKTexFormat.R16G16B16A16_SNORM;
                case GraphicsFormat.R16_UInt: return VKTexFormat.R16_UINT;
                case GraphicsFormat.R16G16_UInt: return VKTexFormat.R16G16_UINT;
                case GraphicsFormat.R16G16B16_UInt: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R16G16B16A16_UInt: return VKTexFormat.R16G16B16A16_UINT;
                case GraphicsFormat.R16_SInt: return VKTexFormat.R16_SINT;
                case GraphicsFormat.R16G16_SInt: return VKTexFormat.R16G16_SINT;
                case GraphicsFormat.R16G16B16_SInt: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R16G16B16A16_SInt: return VKTexFormat.R16G16B16A16_SINT;
                case GraphicsFormat.R32_UInt: return VKTexFormat.R32_UINT;
                case GraphicsFormat.R32G32_UInt: return VKTexFormat.R32G32_UINT;
                case GraphicsFormat.R32G32B32_UInt: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R32G32B32A32_UInt: return VKTexFormat.R32G32B32A32_UINT;
                case GraphicsFormat.R32_SInt: return VKTexFormat.R32_SINT;
                case GraphicsFormat.R32G32_SInt: return VKTexFormat.R32G32_SINT;
                case GraphicsFormat.R32G32B32_SInt: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R32G32B32A32_SInt: return VKTexFormat.R32G32B32A32_SINT;
                case GraphicsFormat.R16_SFloat: return VKTexFormat.R16_SFLOAT;
                case GraphicsFormat.R16G16_SFloat: return VKTexFormat.R16G16_SFLOAT;
                case GraphicsFormat.R16G16B16_SFloat: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R16G16B16A16_SFloat: return VKTexFormat.R16G16B16A16_SFLOAT;
                case GraphicsFormat.R32_SFloat: return VKTexFormat.R32_SFLOAT;
                case GraphicsFormat.R32G32_SFloat: return VKTexFormat.R32G32_SFLOAT;
                case GraphicsFormat.R32G32B32_SFloat: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R32G32B32A32_SFloat: return VKTexFormat.R32G32B32A32_SFLOAT;
                case GraphicsFormat.B8G8R8_SRGB: return VKTexFormat.B8G8R8_SRGB;
                case GraphicsFormat.B8G8R8A8_SRGB: return VKTexFormat.B8G8R8A8_SRGB;
                case GraphicsFormat.B8G8R8_UNorm: return VKTexFormat.B8G8R8_UNORM;
                case GraphicsFormat.B8G8R8A8_UNorm: return VKTexFormat.B8G8R8A8_UNORM;
                case GraphicsFormat.B8G8R8_SNorm: return VKTexFormat.B8G8R8_SNORM;
                case GraphicsFormat.B8G8R8A8_SNorm: return VKTexFormat.B8G8R8A8_SNORM;
                case GraphicsFormat.B8G8R8_UInt: return VKTexFormat.B8G8R8_UINT;
                case GraphicsFormat.B8G8R8A8_UInt: return VKTexFormat.B8G8R8A8_UINT;
                case GraphicsFormat.B8G8R8_SInt: return VKTexFormat.B8G8R8_SINT;
                case GraphicsFormat.B8G8R8A8_SInt: return VKTexFormat.B8G8R8A8_SINT;
                case GraphicsFormat.R4G4B4A4_UNormPack16: return VKTexFormat.R4G4B4A4_UNORM_PACK16;
                case GraphicsFormat.B4G4R4A4_UNormPack16: return VKTexFormat.B4G4R4A4_UNORM_PACK16;
                case GraphicsFormat.R5G6B5_UNormPack16: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.B5G6R5_UNormPack16: return VKTexFormat.B5G6R5_UNORM_PACK16;
                case GraphicsFormat.R5G5B5A1_UNormPack16: return VKTexFormat.R5G5B5A1_UNORM_PACK16;
                case GraphicsFormat.B5G5R5A1_UNormPack16: return VKTexFormat.B5G5R5A1_UNORM_PACK16;
                case GraphicsFormat.A1R5G5B5_UNormPack16: return VKTexFormat.A1R5G5B5_UNORM_PACK16;
                case GraphicsFormat.E5B9G9R9_UFloatPack32: return VKTexFormat.E5B9G9R9_UFLOAT_PACK32;
                case GraphicsFormat.B10G11R11_UFloatPack32: return VKTexFormat.B10G11R11_UFLOAT_PACK32;
                case GraphicsFormat.A2B10G10R10_UNormPack32: return VKTexFormat.A2B10G10R10_UNORM_PACK32;
                case GraphicsFormat.A2B10G10R10_UIntPack32: return VKTexFormat.A2B10G10R10_UINT_PACK32;
                case GraphicsFormat.A2B10G10R10_SIntPack32: return VKTexFormat.A2B10G10R10_SINT_PACK32;
                case GraphicsFormat.A2R10G10B10_UNormPack32: return VKTexFormat.A2R10G10B10_UNORM_PACK32;
                case GraphicsFormat.A2R10G10B10_UIntPack32: return VKTexFormat.A2R10G10B10_UINT_PACK32;
                case GraphicsFormat.A2R10G10B10_SIntPack32: return VKTexFormat.A2R10G10B10_SINT_PACK32;
                case GraphicsFormat.A2R10G10B10_XRSRGBPack32: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.A2R10G10B10_XRUNormPack32: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R10G10B10_XRSRGBPack32: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.R10G10B10_XRUNormPack32: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.A10R10G10B10_XRSRGBPack32: return VKTexFormat.UNDEFINED;
                case GraphicsFormat.A10R10G10B10_XRUNormPack32: return VKTexFormat.UNDEFINED;
                
                //
                // compressed formats

                // S3TC/DXT/BC
                case GraphicsFormat.RGBA_DXT1_SRGB: return VKTexFormat.BC1_RGBA_SRGB_BLOCK;
                case GraphicsFormat.RGBA_DXT1_UNorm: return VKTexFormat.BC1_RGBA_UNORM_BLOCK;
                case GraphicsFormat.RGBA_DXT3_SRGB: return VKTexFormat.BC3_SRGB_BLOCK;
                case GraphicsFormat.RGBA_DXT3_UNorm: return VKTexFormat.BC3_UNORM_BLOCK;
                case GraphicsFormat.RGBA_DXT5_SRGB: return VKTexFormat.BC2_SRGB_BLOCK;
                case GraphicsFormat.RGBA_DXT5_UNorm: return VKTexFormat.BC2_UNORM_BLOCK;
                case GraphicsFormat.R_BC4_UNorm: return VKTexFormat.BC4_UNORM_BLOCK;
                case GraphicsFormat.R_BC4_SNorm: return VKTexFormat.BC4_SNORM_BLOCK;
                case GraphicsFormat.RG_BC5_UNorm: return VKTexFormat.BC5_UNORM_BLOCK;
                case GraphicsFormat.RG_BC5_SNorm: return VKTexFormat.BC5_SNORM_BLOCK;
                case GraphicsFormat.RGB_BC6H_UFloat: return VKTexFormat.BC6H_UFLOAT_BLOCK;
                case GraphicsFormat.RGB_BC6H_SFloat: return VKTexFormat.BC6H_SFLOAT_BLOCK;
                case GraphicsFormat.RGBA_BC7_SRGB: return VKTexFormat.BC7_SRGB_BLOCK;
                case GraphicsFormat.RGBA_BC7_UNorm: return VKTexFormat.BC7_UNORM_BLOCK;

                // PVRTC
                case GraphicsFormat.RGB_PVRTC_2Bpp_SRGB: return VKTexFormat.PVRTC1_2BPP_SRGB_BLOCK_IMG;
                case GraphicsFormat.RGB_PVRTC_2Bpp_UNorm: return VKTexFormat.PVRTC1_2BPP_UNORM_BLOCK_IMG;
                case GraphicsFormat.RGB_PVRTC_4Bpp_SRGB: return VKTexFormat.PVRTC1_4BPP_SRGB_BLOCK_IMG;
                case GraphicsFormat.RGB_PVRTC_4Bpp_UNorm: return VKTexFormat.PVRTC1_4BPP_UNORM_BLOCK_IMG;
                case GraphicsFormat.RGBA_PVRTC_2Bpp_SRGB: return VKTexFormat.PVRTC2_2BPP_SRGB_BLOCK_IMG;
                case GraphicsFormat.RGBA_PVRTC_2Bpp_UNorm: return VKTexFormat.PVRTC2_2BPP_UNORM_BLOCK_IMG;
                case GraphicsFormat.RGBA_PVRTC_4Bpp_SRGB: return VKTexFormat.PVRTC2_4BPP_SRGB_BLOCK_IMG;
                case GraphicsFormat.RGBA_PVRTC_4Bpp_UNorm: return VKTexFormat.PVRTC2_4BPP_UNORM_BLOCK_IMG;

                // ETC
                case GraphicsFormat.RGB_ETC_UNorm: return VKTexFormat.ETC2_R8G8B8_UNORM_BLOCK;
                case GraphicsFormat.RGB_ETC2_SRGB: return VKTexFormat.ETC2_R8G8B8_SRGB_BLOCK;
                case GraphicsFormat.RGB_ETC2_UNorm: return VKTexFormat.ETC2_R8G8B8_UNORM_BLOCK;
                case GraphicsFormat.RGB_A1_ETC2_SRGB: return VKTexFormat.ETC2_R8G8B8A1_SRGB_BLOCK;
                case GraphicsFormat.RGB_A1_ETC2_UNorm: return VKTexFormat.ETC2_R8G8B8A1_UNORM_BLOCK;
                case GraphicsFormat.RGBA_ETC2_SRGB: return VKTexFormat.ETC2_R8G8B8A8_SRGB_BLOCK;
                case GraphicsFormat.RGBA_ETC2_UNorm: return VKTexFormat.ETC2_R8G8B8A8_UNORM_BLOCK;
                case GraphicsFormat.R_EAC_UNorm: return VKTexFormat.EAC_R11_UNORM_BLOCK;
                case GraphicsFormat.R_EAC_SNorm: return VKTexFormat.EAC_R11_SNORM_BLOCK;
                case GraphicsFormat.RG_EAC_UNorm: return VKTexFormat.EAC_R11G11_UNORM_BLOCK;
                case GraphicsFormat.RG_EAC_SNorm: return VKTexFormat.EAC_R11G11_SNORM_BLOCK;

                // ASTC
                case GraphicsFormat.RGBA_ASTC4X4_SRGB: return VKTexFormat.ASTC_4x4_SRGB_BLOCK;
                case GraphicsFormat.RGBA_ASTC4X4_UNorm: return VKTexFormat.ASTC_4x4_UNORM_BLOCK;
                case GraphicsFormat.RGBA_ASTC5X5_SRGB: return VKTexFormat.ASTC_5x5_SRGB_BLOCK;
                case GraphicsFormat.RGBA_ASTC5X5_UNorm: return VKTexFormat.ASTC_5x5_UNORM_BLOCK;
                case GraphicsFormat.RGBA_ASTC6X6_SRGB: return VKTexFormat.ASTC_6x6_SRGB_BLOCK;
                case GraphicsFormat.RGBA_ASTC6X6_UNorm: return VKTexFormat.ASTC_6x6_UNORM_BLOCK;
                case GraphicsFormat.RGBA_ASTC8X8_SRGB: return VKTexFormat.ASTC_8x8_SRGB_BLOCK;
                case GraphicsFormat.RGBA_ASTC8X8_UNorm: return VKTexFormat.ASTC_8x8_UNORM_BLOCK;
                case GraphicsFormat.RGBA_ASTC10X10_SRGB: return VKTexFormat.ASTC_10x10_SRGB_BLOCK;
                case GraphicsFormat.RGBA_ASTC10X10_UNorm: return VKTexFormat.ASTC_10x10_UNORM_BLOCK;
                case GraphicsFormat.RGBA_ASTC12X12_SRGB: return VKTexFormat.ASTC_12x12_SRGB_BLOCK;
                case GraphicsFormat.RGBA_ASTC12X12_UNorm: return VKTexFormat.ASTC_12x12_UNORM_BLOCK;
                
                default: break;
            }
            return VKTexFormat.UNDEFINED;
        }

        public static MipmapFilterMode GetTextureFilterMode(FilterMode filter)
        {
            switch (filter)
            {
                case FilterMode.Point: return MipmapFilterMode.Point;
                case FilterMode.Bilinear: return MipmapFilterMode.Bilinear;
                case FilterMode.Trilinear: return MipmapFilterMode.Trilinear;
                default: break;
            }
            return MipmapFilterMode.Unsupported;
        }

        public static WrapMode GetTextureWrapMode(TextureWrapMode wrap)
        {
            switch (wrap)
            {
                case TextureWrapMode.Repeat: return WrapMode.Repeat;
                case TextureWrapMode.Clamp: return WrapMode.Clamp;
                case TextureWrapMode.Mirror: return WrapMode.Mirror;
                case TextureWrapMode.MirrorOnce: return WrapMode.MirrorOnce;
                default: break;
            }
            return WrapMode.Unsupported;
        }

        public static TextureData CreateTextureData(Texture texture, int channel)
        {
            TextureData texdata = new TextureData();
            texdata.channel = channel;

            switch (texture.dimension)
            {
                case TextureDimension.Tex2D: PopulateTextureData(texture as Texture2D, ref texdata); break;
                case TextureDimension.Tex3D: PopulateTextureData(texture as Texture3D, ref texdata); break;
                default: break;
            }

            return texdata;
        }

        public static bool PopulateTextureData(Texture2D texture, ref TextureData texdata)
        {
            if (!PopulateTextureData(texture as Texture, ref texdata)) return false;
            texdata.depth = 1;
            texdata.pixels.data = texture.GetRawTextureData(); //Color32ArrayToByteArray(texture.GetPixels32());
            texdata.pixels.length = texdata.pixels.data.Length;
            texdata.mipmapCount = texture.mipmapCount;
            texdata.mipmapBias = texture.mipMapBias;
            return true;
        }

        public static bool PopulateTextureData(Texture3D texture, ref TextureData texdata)
        {
            if (!PopulateTextureData(texture as Texture, ref texdata)) return false;
            texdata.depth = texture.depth;
            texdata.pixels.data = Color32ArrayToByteArray(texture.GetPixels32());
            texdata.pixels.length = texdata.pixels.data.Length;
            return true;
        }

        //
        // Populate the base data accesible via Texture, exludes pixel data and depth
        //
        public static bool PopulateTextureData(Texture texture, ref TextureData texdata)
        {
            texdata.id = texture.GetInstanceID().ToString();
            texdata.format = GetTextureFormat(texture.graphicsFormat);
            texdata.width = texture.width;
            texdata.height = texture.height;
            texdata.anisoLevel = texture.anisoLevel;
            texdata.wrapMode = GetTextureWrapMode(texture.wrapMode);
            texdata.filterMode = GetTextureFilterMode(texture.filterMode);
            texdata.mipmapCount = 1;
            texdata.mipmapBias = 0.0f;
            return true;
        }

        //
        // Materials
        //

        public static Dictionary<string, Texture> GetTexturesForMaterial(Material mat)
        {
            Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

            if (mat == null) return textures;

            Shader shader = mat.shader;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propname = ShaderUtil.GetPropertyName(shader, i);
                    Texture texture = mat.GetTexture(propname);
                    textures.Add(propname, texture);
                }
            }
            return textures;
        }

        public static Dictionary<string, Texture> GetValidTexturesForMaterial(Material mat)
        {
            Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

            if (mat == null) return textures;

            Shader shader = mat.shader;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propname = ShaderUtil.GetPropertyName(shader, i);
                    Texture texture = mat.GetTexture(propname);
                    if (texture != null) textures.Add(propname, texture);
                }
            }
            return textures;
        }

        public static string[] GetUsedTextureNames(Material mat)
        {
            List<string> names = new List<string>();

            if (mat == null) return names.ToArray();

            Shader shader = mat.shader;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propname = ShaderUtil.GetPropertyName(shader, i);
                    Texture texture = mat.GetTexture(propname);
                    if (texture != null) names.Add(propname);
                }
            }
            return names.ToArray();
        }

        public static Dictionary<string, int> _StandardMaterialChannelLookupDictionary = new Dictionary<string, int>()
        {
            { "_MainTex", 0 }/*,
            { "_BumpMap", 1 },
            { "_Occlusion", 2 },
            { "_SpecGlossMap", 3 }*/
        };

        public static MaterialData CreateMaterialData(Material material, ref Dictionary<string, TextureData> cache, Dictionary<string, int> channelLookup = null)
        {
            if (channelLookup == null)
            {
                channelLookup = _StandardMaterialChannelLookupDictionary;
            }

            MaterialData matdata = new MaterialData();
            matdata.id = material.GetInstanceID().ToString();

            Dictionary<string, Texture> texturemap = GetValidTexturesForMaterial(material);

            List<TextureData> texdatas = new List<TextureData>();

            foreach (string key in channelLookup.Keys)
            {
                if (texturemap.ContainsKey(key))
                {
                    Texture tex = texturemap[key];
                    string texid = tex.GetInstanceID().ToString();

                    TextureData texdata = new TextureData();

                    // is it in the cache
                    if (cache.ContainsKey(texid))
                    {
                        texdata.channel = channelLookup[key];
                        texdata.id = texid;
                    }
                    else
                    {
                        TextureSupportIssues issues = GetSupportIssuesForTexture(tex);
                        if (issues == TextureSupportIssues.None)
                        {
                            texdata = CreateTextureData(texturemap[key], channelLookup[key]);
                        }
                        else
                        {
                            texdata = CreateTextureData(Texture2D.whiteTexture, channelLookup[key]);

                            Debug.LogWarning(NativeUtils.GetTextureSupportReport(issues, tex));
                        }
                        cache.Add(texid, texdata);
                    }
                    texdatas.Add(texdata);
                }
            }

            matdata.textures = texdatas.ToArray();

            if (material.HasProperty("_Color")) matdata.diffuseColor = material.color;
            else matdata.diffuseColor = Color.white;

            string rendertype = material.GetTag("RenderType", true, "Opaque");
            matdata.useAlpha = rendertype.Contains("Transparent") ? 1 : 0;

            return matdata;
        }

        // shader id consists of "(shader instance id)-(shader key words)-(used texture names)

        public static string GetShaderIDForMaterial(Material mat)
        {
            string idstr = mat.shader != null ? mat.shader.GetInstanceID().ToString() : "null";
            idstr += "-" + (mat.shaderKeywords.Length > 0 ? String.Join("|", mat.shaderKeywords) : "none");
            string[] texnames = GetUsedTextureNames(mat);
            idstr += "-" + (texnames.Length > 0 ? String.Join("|", texnames) : "none");

            return idstr;
        }

        private static byte[] Color32ArrayToByteArray(Color32[] colors)
        {
            if (colors == null || colors.Length == 0)
                return null;

            int lengthOfColor32 = Marshal.SizeOf(typeof (Color32));
            int length = lengthOfColor32 * colors.Length;
            byte[] bytes = new byte[length];

            GCHandle handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(ptr, bytes, 0, length);
            }
            finally
            {
                if (handle != default(GCHandle))
                    handle.Free();
            }

            return bytes;
        }

        [Flags]
        public enum TextureSupportIssues {
            None = 0,
            Dimensions = 1,
            Format = 2,
            ReadWrite = 4
        }

        public static TextureSupportIssues GetSupportIssuesForTexture(Texture texture)
        {
            TextureSupportIssues issues = TextureSupportIssues.None;

            if (!texture.isReadable) issues |= TextureSupportIssues.ReadWrite;

            VKTexFormat format = GetTextureFormat(texture.graphicsFormat);
            if (format == VKTexFormat.UNDEFINED) issues |= TextureSupportIssues.Format;

            if (texture.dimension != TextureDimension.Tex2D) issues |= TextureSupportIssues.Dimensions; //&& texture.dimension != TextureDimension.Tex3D

            return issues;
        }

        //
        // Returns empty string if texture is support,
        // otherwise returns description of unsupported feature
        //
        public static string GetTextureSupportReport(Texture texture)
        {
            TextureSupportIssues issues = GetSupportIssuesForTexture(texture);
            return GetTextureSupportReport(issues, texture);
        }

        public static string GetTextureSupportReport(TextureSupportIssues issues, Texture texture)
        {
            string report = string.Empty;

            if ((issues & TextureSupportIssues.ReadWrite) == TextureSupportIssues.ReadWrite) report += "Read/Write not enabled. Please enabled Read/Write in import settings.\n";
            if ((issues & TextureSupportIssues.Format) == TextureSupportIssues.Format) report += "Format '" + texture.graphicsFormat.ToString() + "' unsupported. Please select a supported format (RGBA32) in import settings.\n";
            if ((issues & TextureSupportIssues.Dimensions) == TextureSupportIssues.Dimensions) report += "Unsupported Texture dimension '" + texture.dimension.ToString() + "'. Try selecting another 'Texture Shape' (2D) in immport settings.\n";

            if (!string.IsNullOrEmpty(report))
            {
                report = texture.name + " has the following issues:\n" + report;
            }

            return report;
        }
    }

    public static class Memory
    {
#if UNITY_IPHONE
        [DllImport("__Internal")]
#else
        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_DataTypes_DeleteNativeObject")]
#endif
        private static extern void
        unity2vsg_DataTypes_DeleteNativeObject(IntPtr anObjectPointer, bool isArray);

        public static void DeleteNativeObject(IntPtr anObjectPointer, bool isArray)
        {
            unity2vsg_DataTypes_DeleteNativeObject(anObjectPointer, isArray);
        }
    }

    public static class Convert
    {
        private static T[] CreateArray<T>(IntPtr array, int length)
        {
            T[] result = new T[length];
            int size = Marshal.SizeOf(typeof (T));

            if (IntPtr.Size == 4)
            {
                // 32-bit system
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = (T) Marshal.PtrToStructure(array, typeof (T));
                    array = new IntPtr(array.ToInt32() + size);
                }
            }
            else
            {
                // probably 64-bit system
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = (T) Marshal.PtrToStructure(array, typeof (T));
                    array = new IntPtr(array.ToInt64() + size);
                }
            }
            return result;
        }

        public static IntArray FromLocal(int[] anArray)
        {
            IntArray result;
            result.data = anArray;
            result.length = anArray.Length;
            return result;
        }

        public static FloatArray FromLocal(float[] anArray)
        {
            FloatArray result;
            result.data = anArray;
            result.length = anArray.Length;
            return result;
        }

        public static Vec2Array FromLocal(Vector2[] anArray)
        {
            Vec2Array result;
            result.data = anArray;
            result.length = anArray.Length;
            return result;
        }

        public static Vec3Array FromLocal(Vector3[] anArray)
        {
            Vec3Array result;
            result.data = anArray;
            result.length = anArray.Length;
            return result;
        }

        public static Vec4Array FromLocal(Vector4[] anArray)
        {
            Vec4Array result;
            result.data = anArray;
            result.length = anArray.Length;
            return result;
        }

        public static IntArray FromNative(NativeIntArray aNativeArray)
        {
            IntArray result;
            result.data = CreateArray<int>(aNativeArray.ptr, aNativeArray.length);
            result.length = result.data.Length;
            return result;
        }

        public static FloatArray FromNative(NativeFloatArray aNativeArray)
        {
            FloatArray result;
            result.data = CreateArray<float>(aNativeArray.ptr, aNativeArray.length);
            result.length = result.data.Length;
            return result;
        }

        public static Vec2Array FromNative(NativeVec2Array aNativeArray)
        {
            Vec2Array result;
            result.data = CreateArray<Vector2>(aNativeArray.ptr, aNativeArray.length);
            result.length = result.data.Length;
            return result;
        }

        public static Vec3Array FromNative(NativeVec3Array aNativeArray)
        {
            Vec3Array result;
            result.data = CreateArray<Vector3>(aNativeArray.ptr, aNativeArray.length);
            result.length = result.data.Length;
            return result;
        }

        public static Vec4Array FromNative(NativeVec4Array aNativeArray)
        {
            Vec4Array result;
            result.data = CreateArray<Vector4>(aNativeArray.ptr, aNativeArray.length);
            result.length = result.data.Length;
            return result;
        }
    }

}
