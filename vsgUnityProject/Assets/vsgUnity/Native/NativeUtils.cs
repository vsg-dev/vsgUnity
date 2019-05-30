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
    public struct UIntArray
    {
        public uint[] data;
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

    public struct DescriptorBinding
    {
        public int index;
        public VkDescriptorType type;
        public int count;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DescriptorBindingsArray
    {
        public DescriptorBinding[] data;
        public int length;
    };

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

    [StructLayout(LayoutKind.Sequential)]
    public struct TextureData
    {
        public string id;
        public int channel;
        public ByteArray pixels;
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
    }

    public struct MaterialData
    {
        public string id;
        public TextureData[] textures;
        public Vector4 diffuseColor;
        public int useAlpha;
        public string[] customDefines;
        public DescriptorBinding[] vertexDescriptorBindings;
        public DescriptorBinding[] fragmentDescriptorBindings;
    }

    public struct ShaderData
    {
        public string vertexSource;
        public string fragmentSource;
        public UIntArray vertexSpecializationData;
        public UIntArray fragmentSpecializationData;
        public string customDefines;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PipelineData
    {
        public string id;
        public int hasNormals;
        public int hasTangents;
        public int hasColors;
        public int uvChannelCount;
        public int useAlpha;
        public DescriptorBindingsArray vertexDescriptorBindings;
        public DescriptorBindingsArray fragmentDescriptorBindings;
        public ShaderData shader;
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
    public struct LODChildData
    {
        public float minimumScreenHeightRatio;
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
            idstr += data.useAlpha == 1 ? "1" : "0";
            idstr += data.vertexDescriptorBindings.length.ToString(); // need better id for these
            idstr += data.fragmentDescriptorBindings.length.ToString();
            idstr += GetIDForShader(data.shader);
            return idstr;
        }

        public static string GetIDForShader(ShaderData data)
        {
            string idstr = "";
            idstr += string.IsNullOrEmpty(data.vertexSource) ? "0" : data.vertexSource;
            idstr += string.IsNullOrEmpty(data.fragmentSource) ? "0" : data.fragmentSource;
            idstr += data.vertexSpecializationData.length.ToString();
            idstr += data.fragmentSpecializationData.length.ToString();
            idstr += string.IsNullOrEmpty(data.customDefines) ? "0" : data.customDefines;
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
            texdata.format = Vulkan.vkFormatForGraphicsFormat(texture.graphicsFormat);
            texdata.width = texture.width;
            texdata.height = texture.height;
            texdata.anisoLevel = texture.anisoLevel;
            texdata.wrapMode = Vulkan.vkSamplerAddressModeForWrapMode(texture.wrapMode);
            texdata.filterMode = Vulkan.vkFilterForFilterMode(texture.filterMode);
            texdata.mipmapMode = Vulkan.vkSamplerMipmapModeForFilterMode(texture.filterMode);
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

        public struct TextureMapping
        {
            public int channel;
            public string define;
        }

        public static Dictionary<string, TextureMapping> _StandardMaterialChannelLookup = new Dictionary<string, TextureMapping>()
        {
            { "_MainTex", new TextureMapping(){ channel = 0, define = "VSG_DIFFUSE_MAP" } }/*,
            { "_BumpMap", 1 },
            { "_Occlusion", 2 },
            { "_SpecGlossMap", 3 }*/
        };

        public static MaterialData CreateMaterialData(Material material, ref Dictionary<string, TextureData> cache, Dictionary<string, TextureMapping> channelLookup = null)
        {
            if (channelLookup == null)
            {
                channelLookup = _StandardMaterialChannelLookup;
            }

            MaterialData matdata = new MaterialData();
            matdata.id = material.GetInstanceID().ToString();

            Dictionary<string, Texture> texturemap = GetValidTexturesForMaterial(material);

            List<TextureData> texdatas = new List<TextureData>();
            List<DescriptorBinding> vertBindings = new List<DescriptorBinding>();
            List<DescriptorBinding> fragBindings = new List<DescriptorBinding>();
            List<string> customdefs = new List<string>();

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
                        texdata.channel = channelLookup[key].channel;
                        texdata.id = texid;
                        fragBindings.Add(new DescriptorBinding() { index = channelLookup[key].channel, type = VkDescriptorType.VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, count = 1 });
                        customdefs.Add(channelLookup[key].define);
                    }
                    else
                    {
                        TextureSupportIssues issues = GetSupportIssuesForTexture(tex);
                        if (issues == TextureSupportIssues.None)
                        {
                            texdata = CreateTextureData(texturemap[key], channelLookup[key].channel);
                        }
                        else
                        {
                            texdata = CreateTextureData(Texture2D.whiteTexture, channelLookup[key].channel);

                            Debug.LogWarning(NativeUtils.GetTextureSupportReport(issues, tex));
                        }

                        fragBindings.Add(new DescriptorBinding() { index = channelLookup[key].channel, type = VkDescriptorType.VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, count = 1 });
                        customdefs.Add(channelLookup[key].define);

                        cache.Add(texid, texdata);
                    }
                    texdatas.Add(texdata);
                }
            }

            matdata.textures = texdatas.ToArray();
            matdata.vertexDescriptorBindings = vertBindings.ToArray();
            matdata.fragmentDescriptorBindings = fragBindings.ToArray();

            if (material.HasProperty("_Color")) matdata.diffuseColor = material.color;
            else matdata.diffuseColor = Color.white;

            string rendertype = material.GetTag("RenderType", true, "Opaque");
            matdata.useAlpha = rendertype.Contains("Transparent") ? 1 : 0;
            if (matdata.useAlpha == 1) customdefs.Add("VSG_BLEND");

            string lightmode = material.GetTag("LightMode", true, "ForwardBase");
            if (lightmode != "Always") customdefs.Add("VSG_LIGHTING");

            matdata.customDefines = customdefs.ToArray();

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

            VkFormat format = Vulkan.vkFormatForGraphicsFormat(texture.graphicsFormat);
            if (format == VkFormat.UNDEFINED) issues |= TextureSupportIssues.Format;

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
