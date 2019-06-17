/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace vsgUnity
{
    public enum VkFilter
    {
        VK_FILTER_NEAREST = 0,
        VK_FILTER_LINEAR = 1,
        VK_FILTER_CUBIC_IMG = 1000015000,
        VK_FILTER_CUBIC_EXT = VK_FILTER_CUBIC_IMG,
        VK_FILTER_BEGIN_RANGE = VK_FILTER_NEAREST,
        VK_FILTER_END_RANGE = VK_FILTER_LINEAR,
        VK_FILTER_RANGE_SIZE = (VK_FILTER_LINEAR - VK_FILTER_NEAREST + 1),
        VK_FILTER_MAX_ENUM = 0x7FFFFFFF
    }

    public enum VkSamplerMipmapMode
    {
        VK_SAMPLER_MIPMAP_MODE_NEAREST = 0,
        VK_SAMPLER_MIPMAP_MODE_LINEAR = 1,
        VK_SAMPLER_MIPMAP_MODE_BEGIN_RANGE = VK_SAMPLER_MIPMAP_MODE_NEAREST,
        VK_SAMPLER_MIPMAP_MODE_END_RANGE = VK_SAMPLER_MIPMAP_MODE_LINEAR,
        VK_SAMPLER_MIPMAP_MODE_RANGE_SIZE = (VK_SAMPLER_MIPMAP_MODE_LINEAR - VK_SAMPLER_MIPMAP_MODE_NEAREST + 1),
        VK_SAMPLER_MIPMAP_MODE_MAX_ENUM = 0x7FFFFFFF
    }

    public enum VkSamplerAddressMode
    {
        VK_SAMPLER_ADDRESS_MODE_REPEAT = 0,
        VK_SAMPLER_ADDRESS_MODE_MIRRORED_REPEAT = 1,
        VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_EDGE = 2,
        VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_BORDER = 3,
        VK_SAMPLER_ADDRESS_MODE_MIRROR_CLAMP_TO_EDGE = 4,
        VK_SAMPLER_ADDRESS_MODE_BEGIN_RANGE = VK_SAMPLER_ADDRESS_MODE_REPEAT,
        VK_SAMPLER_ADDRESS_MODE_END_RANGE = VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_BORDER,
        VK_SAMPLER_ADDRESS_MODE_RANGE_SIZE = (VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_BORDER - VK_SAMPLER_ADDRESS_MODE_REPEAT + 1),
        VK_SAMPLER_ADDRESS_MODE_MAX_ENUM = 0x7FFFFFFF
    }

    public enum VkDescriptorType
    {
        VK_DESCRIPTOR_TYPE_SAMPLER = 0,
        VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER = 1,
        VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE = 2,
        VK_DESCRIPTOR_TYPE_STORAGE_IMAGE = 3,
        VK_DESCRIPTOR_TYPE_UNIFORM_TEXEL_BUFFER = 4,
        VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER = 5,
        VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER = 6,
        VK_DESCRIPTOR_TYPE_STORAGE_BUFFER = 7,
        VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC = 8,
        VK_DESCRIPTOR_TYPE_STORAGE_BUFFER_DYNAMIC = 9,
        VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT = 10,
        VK_DESCRIPTOR_TYPE_INLINE_UNIFORM_BLOCK_EXT = 1000138000,
        VK_DESCRIPTOR_TYPE_ACCELERATION_STRUCTURE_NV = 1000165000,
        VK_DESCRIPTOR_TYPE_BEGIN_RANGE = VK_DESCRIPTOR_TYPE_SAMPLER,
        VK_DESCRIPTOR_TYPE_END_RANGE = VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT,
        VK_DESCRIPTOR_TYPE_RANGE_SIZE = (VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT - VK_DESCRIPTOR_TYPE_SAMPLER + 1),
        VK_DESCRIPTOR_TYPE_MAX_ENUM = 0x7FFFFFFF
    }

    [System.Flags]
    public enum VkShaderStageFlagBits
    {
        VK_SHADER_STAGE_VERTEX_BIT = 0x00000001,
        VK_SHADER_STAGE_TESSELLATION_CONTROL_BIT = 0x00000002,
        VK_SHADER_STAGE_TESSELLATION_EVALUATION_BIT = 0x00000004,
        VK_SHADER_STAGE_GEOMETRY_BIT = 0x00000008,
        VK_SHADER_STAGE_FRAGMENT_BIT = 0x00000010,
        VK_SHADER_STAGE_COMPUTE_BIT = 0x00000020,
        VK_SHADER_STAGE_ALL_GRAPHICS = 0x0000001F,
        VK_SHADER_STAGE_ALL = 0x7FFFFFFF,
        VK_SHADER_STAGE_RAYGEN_BIT_NV = 0x00000100,
        VK_SHADER_STAGE_ANY_HIT_BIT_NV = 0x00000200,
        VK_SHADER_STAGE_CLOSEST_HIT_BIT_NV = 0x00000400,
        VK_SHADER_STAGE_MISS_BIT_NV = 0x00000800,
        VK_SHADER_STAGE_INTERSECTION_BIT_NV = 0x00001000,
        VK_SHADER_STAGE_CALLABLE_BIT_NV = 0x00002000,
        VK_SHADER_STAGE_TASK_BIT_NV = 0x00000040,
        VK_SHADER_STAGE_MESH_BIT_NV = 0x00000080,
        VK_SHADER_STAGE_FLAG_BITS_MAX_ENUM = 0x7FFFFFFF
    }

    public enum VkFormat
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

    public struct VkDescriptorSetLayoutBinding : IEquatable<VkDescriptorSetLayoutBinding>
    {
        public uint binding;
        public VkDescriptorType descriptorType;
        public uint descriptorCount;
        public VkShaderStageFlagBits stageFlags;
        public IntPtr pImmutableSamplers;

        public bool Equals(VkDescriptorSetLayoutBinding b)
        {
            return binding == b.binding &&
                descriptorType == b.descriptorType &&
                descriptorCount == b.descriptorCount &&
                stageFlags == b.stageFlags;
        }
    }

    public static class Vulkan
    {
        public static VkSamplerAddressMode vkSamplerAddressModeForWrapMode(TextureWrapMode wrap)
        {
            switch (wrap)
            {
                case TextureWrapMode.Repeat: return VkSamplerAddressMode.VK_SAMPLER_ADDRESS_MODE_REPEAT;
                case TextureWrapMode.Clamp: return VkSamplerAddressMode.VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_EDGE;
                case TextureWrapMode.Mirror:
                case TextureWrapMode.MirrorOnce: return VkSamplerAddressMode.VK_SAMPLER_ADDRESS_MODE_MIRRORED_REPEAT;
                default: break;
            }
            return VkSamplerAddressMode.VK_SAMPLER_ADDRESS_MODE_MAX_ENUM; // unknown
        }

        public static VkFilter vkFilterForFilterMode(FilterMode filtermode)
        {
            switch (filtermode)
            {
                case FilterMode.Point: return VkFilter.VK_FILTER_NEAREST;
                case FilterMode.Bilinear: return VkFilter.VK_FILTER_LINEAR;
                case FilterMode.Trilinear: return VkFilter.VK_FILTER_LINEAR;
                default: break;
            }
            return VkFilter.VK_FILTER_MAX_ENUM; // unknown
        }

        public static VkSamplerMipmapMode vkSamplerMipmapModeForFilterMode(FilterMode filtermode)
        {
            switch (filtermode)
            {
                case FilterMode.Point: return VkSamplerMipmapMode.VK_SAMPLER_MIPMAP_MODE_NEAREST;
                case FilterMode.Bilinear: return VkSamplerMipmapMode.VK_SAMPLER_MIPMAP_MODE_NEAREST;
                case FilterMode.Trilinear: return VkSamplerMipmapMode.VK_SAMPLER_MIPMAP_MODE_LINEAR;
                default: break;
            }
            return VkSamplerMipmapMode.VK_SAMPLER_MIPMAP_MODE_MAX_ENUM; // unknown
        }

        public static VkFormat vkFormatForGraphicsFormat(GraphicsFormat format)
        {
            switch (format)
            {
                case GraphicsFormat.None: return VkFormat.UNDEFINED;
                case GraphicsFormat.R8_SRGB: return VkFormat.R8_SRGB;
                case GraphicsFormat.R8G8_SRGB: return VkFormat.R8G8_SRGB;
                case GraphicsFormat.R8G8B8_SRGB: return VkFormat.UNDEFINED;
                case GraphicsFormat.R8G8B8A8_SRGB: return VkFormat.R8G8B8A8_SRGB;
                case GraphicsFormat.R8_UNorm: return VkFormat.R8_UNORM;
                case GraphicsFormat.R8G8_UNorm: return VkFormat.R8G8_UNORM;
                case GraphicsFormat.R8G8B8_UNorm: return VkFormat.UNDEFINED;
                case GraphicsFormat.R8G8B8A8_UNorm: return VkFormat.R8G8B8A8_UNORM;
                case GraphicsFormat.R8_SNorm: return VkFormat.R8_SNORM;
                case GraphicsFormat.R8G8_SNorm: return VkFormat.R8G8_SNORM;
                case GraphicsFormat.R8G8B8_SNorm: return VkFormat.UNDEFINED;
                case GraphicsFormat.R8G8B8A8_SNorm: return VkFormat.R8G8B8A8_SNORM;
                case GraphicsFormat.R8_UInt: return VkFormat.R8_UINT;
                case GraphicsFormat.R8G8_UInt: return VkFormat.R8G8_UINT;
                case GraphicsFormat.R8G8B8_UInt: return VkFormat.UNDEFINED;
                case GraphicsFormat.R8G8B8A8_UInt: return VkFormat.R8G8B8A8_UINT;
                case GraphicsFormat.R8_SInt: return VkFormat.R8_SINT;
                case GraphicsFormat.R8G8_SInt: return VkFormat.R8G8_SINT;
                case GraphicsFormat.R8G8B8_SInt: return VkFormat.UNDEFINED;
                case GraphicsFormat.R8G8B8A8_SInt: return VkFormat.R8G8B8A8_SINT;
                case GraphicsFormat.R16_UNorm: return VkFormat.R16_UNORM;
                case GraphicsFormat.R16G16_UNorm: return VkFormat.R16G16_UNORM;
                case GraphicsFormat.R16G16B16_UNorm: return VkFormat.UNDEFINED;
                case GraphicsFormat.R16G16B16A16_UNorm: return VkFormat.R16G16B16A16_UNORM;
                case GraphicsFormat.R16_SNorm: return VkFormat.R16_SNORM;
                case GraphicsFormat.R16G16_SNorm: return VkFormat.R16G16_SNORM;
                case GraphicsFormat.R16G16B16_SNorm: return VkFormat.UNDEFINED;
                case GraphicsFormat.R16G16B16A16_SNorm: return VkFormat.R16G16B16A16_SNORM;
                case GraphicsFormat.R16_UInt: return VkFormat.R16_UINT;
                case GraphicsFormat.R16G16_UInt: return VkFormat.R16G16_UINT;
                case GraphicsFormat.R16G16B16_UInt: return VkFormat.UNDEFINED;
                case GraphicsFormat.R16G16B16A16_UInt: return VkFormat.R16G16B16A16_UINT;
                case GraphicsFormat.R16_SInt: return VkFormat.R16_SINT;
                case GraphicsFormat.R16G16_SInt: return VkFormat.R16G16_SINT;
                case GraphicsFormat.R16G16B16_SInt: return VkFormat.UNDEFINED;
                case GraphicsFormat.R16G16B16A16_SInt: return VkFormat.R16G16B16A16_SINT;
                case GraphicsFormat.R32_UInt: return VkFormat.R32_UINT;
                case GraphicsFormat.R32G32_UInt: return VkFormat.R32G32_UINT;
                case GraphicsFormat.R32G32B32_UInt: return VkFormat.UNDEFINED;
                case GraphicsFormat.R32G32B32A32_UInt: return VkFormat.R32G32B32A32_UINT;
                case GraphicsFormat.R32_SInt: return VkFormat.R32_SINT;
                case GraphicsFormat.R32G32_SInt: return VkFormat.R32G32_SINT;
                case GraphicsFormat.R32G32B32_SInt: return VkFormat.UNDEFINED;
                case GraphicsFormat.R32G32B32A32_SInt: return VkFormat.R32G32B32A32_SINT;
                case GraphicsFormat.R16_SFloat: return VkFormat.R16_SFLOAT;
                case GraphicsFormat.R16G16_SFloat: return VkFormat.R16G16_SFLOAT;
                case GraphicsFormat.R16G16B16_SFloat: return VkFormat.UNDEFINED;
                case GraphicsFormat.R16G16B16A16_SFloat: return VkFormat.R16G16B16A16_SFLOAT;
                case GraphicsFormat.R32_SFloat: return VkFormat.R32_SFLOAT;
                case GraphicsFormat.R32G32_SFloat: return VkFormat.R32G32_SFLOAT;
                case GraphicsFormat.R32G32B32_SFloat: return VkFormat.UNDEFINED;
                case GraphicsFormat.R32G32B32A32_SFloat: return VkFormat.R32G32B32A32_SFLOAT;
                case GraphicsFormat.B8G8R8_SRGB: return VkFormat.B8G8R8_SRGB;
                case GraphicsFormat.B8G8R8A8_SRGB: return VkFormat.B8G8R8A8_SRGB;
                case GraphicsFormat.B8G8R8_UNorm: return VkFormat.B8G8R8_UNORM;
                case GraphicsFormat.B8G8R8A8_UNorm: return VkFormat.B8G8R8A8_UNORM;
                case GraphicsFormat.B8G8R8_SNorm: return VkFormat.B8G8R8_SNORM;
                case GraphicsFormat.B8G8R8A8_SNorm: return VkFormat.B8G8R8A8_SNORM;
                case GraphicsFormat.B8G8R8_UInt: return VkFormat.B8G8R8_UINT;
                case GraphicsFormat.B8G8R8A8_UInt: return VkFormat.B8G8R8A8_UINT;
                case GraphicsFormat.B8G8R8_SInt: return VkFormat.B8G8R8_SINT;
                case GraphicsFormat.B8G8R8A8_SInt: return VkFormat.B8G8R8A8_SINT;
                case GraphicsFormat.R4G4B4A4_UNormPack16: return VkFormat.R4G4B4A4_UNORM_PACK16;
                case GraphicsFormat.B4G4R4A4_UNormPack16: return VkFormat.B4G4R4A4_UNORM_PACK16;
                case GraphicsFormat.R5G6B5_UNormPack16: return VkFormat.UNDEFINED;
                case GraphicsFormat.B5G6R5_UNormPack16: return VkFormat.B5G6R5_UNORM_PACK16;
                case GraphicsFormat.R5G5B5A1_UNormPack16: return VkFormat.R5G5B5A1_UNORM_PACK16;
                case GraphicsFormat.B5G5R5A1_UNormPack16: return VkFormat.B5G5R5A1_UNORM_PACK16;
                case GraphicsFormat.A1R5G5B5_UNormPack16: return VkFormat.A1R5G5B5_UNORM_PACK16;
                case GraphicsFormat.E5B9G9R9_UFloatPack32: return VkFormat.E5B9G9R9_UFLOAT_PACK32;
                case GraphicsFormat.B10G11R11_UFloatPack32: return VkFormat.B10G11R11_UFLOAT_PACK32;
                case GraphicsFormat.A2B10G10R10_UNormPack32: return VkFormat.A2B10G10R10_UNORM_PACK32;
                case GraphicsFormat.A2B10G10R10_UIntPack32: return VkFormat.A2B10G10R10_UINT_PACK32;
                case GraphicsFormat.A2B10G10R10_SIntPack32: return VkFormat.A2B10G10R10_SINT_PACK32;
                case GraphicsFormat.A2R10G10B10_UNormPack32: return VkFormat.A2R10G10B10_UNORM_PACK32;
                case GraphicsFormat.A2R10G10B10_UIntPack32: return VkFormat.A2R10G10B10_UINT_PACK32;
                case GraphicsFormat.A2R10G10B10_SIntPack32: return VkFormat.A2R10G10B10_SINT_PACK32;
                case GraphicsFormat.A2R10G10B10_XRSRGBPack32: return VkFormat.UNDEFINED;
                case GraphicsFormat.A2R10G10B10_XRUNormPack32: return VkFormat.UNDEFINED;
                case GraphicsFormat.R10G10B10_XRSRGBPack32: return VkFormat.UNDEFINED;
                case GraphicsFormat.R10G10B10_XRUNormPack32: return VkFormat.UNDEFINED;
                case GraphicsFormat.A10R10G10B10_XRSRGBPack32: return VkFormat.UNDEFINED;
                case GraphicsFormat.A10R10G10B10_XRUNormPack32: return VkFormat.UNDEFINED;

                //
                // compressed formats

                // S3TC/DXT/BC
                case GraphicsFormat.RGBA_DXT1_SRGB: return VkFormat.BC1_RGBA_SRGB_BLOCK;
                case GraphicsFormat.RGBA_DXT1_UNorm: return VkFormat.BC1_RGBA_UNORM_BLOCK;
                case GraphicsFormat.RGBA_DXT3_SRGB: return VkFormat.BC3_SRGB_BLOCK;
                case GraphicsFormat.RGBA_DXT3_UNorm: return VkFormat.BC3_UNORM_BLOCK;
                case GraphicsFormat.RGBA_DXT5_SRGB: return VkFormat.BC2_SRGB_BLOCK;
                case GraphicsFormat.RGBA_DXT5_UNorm: return VkFormat.BC2_UNORM_BLOCK;
                case GraphicsFormat.R_BC4_UNorm: return VkFormat.BC4_UNORM_BLOCK;
                case GraphicsFormat.R_BC4_SNorm: return VkFormat.BC4_SNORM_BLOCK;
                case GraphicsFormat.RG_BC5_UNorm: return VkFormat.BC5_UNORM_BLOCK;
                case GraphicsFormat.RG_BC5_SNorm: return VkFormat.BC5_SNORM_BLOCK;
                case GraphicsFormat.RGB_BC6H_UFloat: return VkFormat.BC6H_UFLOAT_BLOCK;
                case GraphicsFormat.RGB_BC6H_SFloat: return VkFormat.BC6H_SFLOAT_BLOCK;
                case GraphicsFormat.RGBA_BC7_SRGB: return VkFormat.BC7_SRGB_BLOCK;
                case GraphicsFormat.RGBA_BC7_UNorm: return VkFormat.BC7_UNORM_BLOCK;

                // PVRTC
                case GraphicsFormat.RGB_PVRTC_2Bpp_SRGB: return VkFormat.PVRTC1_2BPP_SRGB_BLOCK_IMG;
                case GraphicsFormat.RGB_PVRTC_2Bpp_UNorm: return VkFormat.PVRTC1_2BPP_UNORM_BLOCK_IMG;
                case GraphicsFormat.RGB_PVRTC_4Bpp_SRGB: return VkFormat.PVRTC1_4BPP_SRGB_BLOCK_IMG;
                case GraphicsFormat.RGB_PVRTC_4Bpp_UNorm: return VkFormat.PVRTC1_4BPP_UNORM_BLOCK_IMG;
                case GraphicsFormat.RGBA_PVRTC_2Bpp_SRGB: return VkFormat.PVRTC2_2BPP_SRGB_BLOCK_IMG;
                case GraphicsFormat.RGBA_PVRTC_2Bpp_UNorm: return VkFormat.PVRTC2_2BPP_UNORM_BLOCK_IMG;
                case GraphicsFormat.RGBA_PVRTC_4Bpp_SRGB: return VkFormat.PVRTC2_4BPP_SRGB_BLOCK_IMG;
                case GraphicsFormat.RGBA_PVRTC_4Bpp_UNorm: return VkFormat.PVRTC2_4BPP_UNORM_BLOCK_IMG;

                // ETC
                case GraphicsFormat.RGB_ETC_UNorm: return VkFormat.ETC2_R8G8B8_UNORM_BLOCK;
                case GraphicsFormat.RGB_ETC2_SRGB: return VkFormat.ETC2_R8G8B8_SRGB_BLOCK;
                case GraphicsFormat.RGB_ETC2_UNorm: return VkFormat.ETC2_R8G8B8_UNORM_BLOCK;
                case GraphicsFormat.RGB_A1_ETC2_SRGB: return VkFormat.ETC2_R8G8B8A1_SRGB_BLOCK;
                case GraphicsFormat.RGB_A1_ETC2_UNorm: return VkFormat.ETC2_R8G8B8A1_UNORM_BLOCK;
                case GraphicsFormat.RGBA_ETC2_SRGB: return VkFormat.ETC2_R8G8B8A8_SRGB_BLOCK;
                case GraphicsFormat.RGBA_ETC2_UNorm: return VkFormat.ETC2_R8G8B8A8_UNORM_BLOCK;
                case GraphicsFormat.R_EAC_UNorm: return VkFormat.EAC_R11_UNORM_BLOCK;
                case GraphicsFormat.R_EAC_SNorm: return VkFormat.EAC_R11_SNORM_BLOCK;
                case GraphicsFormat.RG_EAC_UNorm: return VkFormat.EAC_R11G11_UNORM_BLOCK;
                case GraphicsFormat.RG_EAC_SNorm: return VkFormat.EAC_R11G11_SNORM_BLOCK;

                // ASTC
                case GraphicsFormat.RGBA_ASTC4X4_SRGB: return VkFormat.ASTC_4x4_SRGB_BLOCK;
                case GraphicsFormat.RGBA_ASTC4X4_UNorm: return VkFormat.ASTC_4x4_UNORM_BLOCK;
                case GraphicsFormat.RGBA_ASTC5X5_SRGB: return VkFormat.ASTC_5x5_SRGB_BLOCK;
                case GraphicsFormat.RGBA_ASTC5X5_UNorm: return VkFormat.ASTC_5x5_UNORM_BLOCK;
                case GraphicsFormat.RGBA_ASTC6X6_SRGB: return VkFormat.ASTC_6x6_SRGB_BLOCK;
                case GraphicsFormat.RGBA_ASTC6X6_UNorm: return VkFormat.ASTC_6x6_UNORM_BLOCK;
                case GraphicsFormat.RGBA_ASTC8X8_SRGB: return VkFormat.ASTC_8x8_SRGB_BLOCK;
                case GraphicsFormat.RGBA_ASTC8X8_UNorm: return VkFormat.ASTC_8x8_UNORM_BLOCK;
                case GraphicsFormat.RGBA_ASTC10X10_SRGB: return VkFormat.ASTC_10x10_SRGB_BLOCK;
                case GraphicsFormat.RGBA_ASTC10X10_UNorm: return VkFormat.ASTC_10x10_UNORM_BLOCK;
                case GraphicsFormat.RGBA_ASTC12X12_SRGB: return VkFormat.ASTC_12x12_SRGB_BLOCK;
                case GraphicsFormat.RGBA_ASTC12X12_UNorm: return VkFormat.ASTC_12x12_UNORM_BLOCK;

                default: break;
            }
            return VkFormat.UNDEFINED;
        }
    }
}
