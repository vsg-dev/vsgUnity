#pragma once

/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

#include <unity2vsg/Export.h>

#include <vsg/core/Array.h>
#include <vsg/core/Data.h>
#include <vsg/maths/vec2.h>
#include <vsg/maths/vec3.h>
#include <vsg/maths/vec4.h>

#include "GraphicsPipelineBuilder.h"

namespace unity2vsg
{
    // types used to pass data from Unity C# to native code
    struct ByteArray
    {
        uint8_t* ptr;
        uint32_t length;
    };

    struct ShortArray
    {
        int16_t* ptr;
        uint32_t length;
    };

    struct UShortArray
    {
        uint16_t* ptr;
        uint32_t length;
    };

    struct IntArray
    {
        int32_t* ptr;
        uint32_t length;
    };

    struct UIntArray
    {
        uint32_t* ptr;
        uint32_t length;
    };

    struct FloatArray
    {
        float* ptr;
        uint32_t length;
    };

    struct DoubleArray
    {
        double* ptr;
        uint32_t length;
    };

    struct Vec2Array
    {
        vsg::vec2* ptr;
        uint32_t length;
    };

    struct Vec3Array
    {
        vsg::vec3* ptr;
        uint32_t length;
    };

    struct Vec4Array
    {
        vsg::vec4* ptr;
        uint32_t length;
    };

    struct DescriptorBindingsArray
    {
        vsg::GraphicsPipelineBuilder::Traits::DescriptorBinding* ptr;
        uint32_t length;
    };

    struct MeshData
    {
        const char* id;
        Vec3Array verticies;
        IntArray triangles;
        Vec3Array normals;
        Vec3Array tangents;
        Vec4Array colors;
        Vec2Array uv0;
        Vec2Array uv1;
        uint32_t use32BitIndicies;
    };

    struct IndexBufferData
    {
        const char* id; // same as mesh id
        IntArray triangles;
        uint32_t use32BitIndicies;
    };

    struct VertexBuffersData
    {
        const char* id; // same as mesh id
        Vec3Array verticies;
        Vec3Array normals;
        Vec3Array tangents;
        Vec4Array colors;
        Vec2Array uv0;
        Vec2Array uv1;
    };

    struct DrawIndexedData
    {
        const char* id; // mesh id + sub mesh index
        uint32_t indexCount;
        uint32_t firstIndex;
        int32_t vertexOffset;
        uint32_t instanceCount;
        uint32_t firstInstance;
    };

    struct TextureData
    {
        const char* id;
        uint32_t channel;
        ByteArray pixels;
        VkFormat format;
        uint32_t width;
        uint32_t height;
        uint32_t depth;
        uint32_t anisoLevel;
        VkSamplerAddressMode wrapMode;
        VkFilter filterMode;
        VkSamplerMipmapMode mipmapMode;
        uint32_t mipmapCount;
        float mipmapBias;
    };

    struct TextureDataArray
    {
        const char* id;
        uint32_t channel;
        //TextureData* ptr;
        //uint32_t length;
    };

    struct ShaderData
    {
        const char* vertexSource;
        const char* fragmentSource;
        UIntArray vertexSpecializationData;
        UIntArray fragmentSpecializationData;
        const char* customDefines;
    };

    struct PipelineData
    {
        const char* id;
        uint32_t hasNormals;
        uint32_t hasTangents;
        uint32_t hasColors;
        uint32_t uvChannelCount;
        uint32_t useAlpha;
        DescriptorBindingsArray vertexDescriptorBindings;
        DescriptorBindingsArray fragmentDescriptorBindings;
        ShaderData shader;
    };

    struct TransformData
    {
        FloatArray matrix;
    };

    struct CameraData
    {
        vsg::vec3 position;
        vsg::vec3 lookAt;
        vsg::vec3 upDir;
        float fov;
        float nearZ;
        float farZ;
    };

    struct CullData
    {
        vsg::vec3 center;
        float radius;
    };

    struct LODChildData
    {
        float minimumScreenHeightRatio;
    };

    struct LightData
    {
        vsg::vec4 color;
        float intensity;
    };

    // create a vsg Array from a pointer and length, by default the ownership of the memory will be external to vsg still
    // so be sure to call Array dataRelease before the ref_ptr tries to delete the memory

    template<typename T>
    vsg::ref_ptr<vsg::Array<T>> createVsgArray(T* ptr, uint32_t length)
    {
        return vsg::ref_ptr<vsg::Array<T>>(new vsg::Array<T>(static_cast<size_t>(length), ptr));
    }

    VkSamplerCreateInfo vkSamplerCreateInfoForTextureData(const TextureData& data)
    {
        bool mipmappingRequired = data.mipmapCount > 1;

        VkSamplerCreateInfo samplerInfo = {};
        samplerInfo.sType = VK_STRUCTURE_TYPE_SAMPLER_CREATE_INFO;
        samplerInfo.minFilter = data.filterMode;
        samplerInfo.magFilter = data.filterMode;
        samplerInfo.mipmapMode = data.mipmapMode;
        samplerInfo.addressModeU = data.wrapMode;
        samplerInfo.addressModeV = data.wrapMode;
        samplerInfo.addressModeW = data.wrapMode;

        // requres Logical device to have deviceFeatures.samplerAnisotropy = VK_TRUE; set when creating the vsg::Deivce
        samplerInfo.anisotropyEnable = data.anisoLevel > 1.0f ? VK_TRUE : VK_FALSE;
        samplerInfo.maxAnisotropy = static_cast<float>(data.anisoLevel);

        if (mipmappingRequired)
        {
            samplerInfo.minLod = 0;
            samplerInfo.maxLod = static_cast<float>(data.mipmapCount);
            samplerInfo.mipLodBias = 0;
        }
        else
        {
            samplerInfo.minLod = 0;
            samplerInfo.maxLod = 0;
            samplerInfo.mipLodBias = 0;
        }

        samplerInfo.borderColor = VK_BORDER_COLOR_INT_OPAQUE_BLACK;

        samplerInfo.unnormalizedCoordinates = VK_FALSE;
        samplerInfo.compareEnable = VK_FALSE;

        return samplerInfo;
    }

    struct VkFormatSizeInfo
    {
        vsg::Data::Layout layout; 
        uint32_t blockSize; //bit size of block
    };

    VkFormatSizeInfo GetSizeInfoForFormat(VkFormat format)
    {
        VkFormatSizeInfo sizeInfo;
        sizeInfo.layout.maxNumMipmaps = 1; // sensible default

        switch (format)
        {
            case VK_FORMAT_R4G4_UNORM_PACK8:
                sizeInfo.blockSize = 8;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R4G4B4A4_UNORM_PACK16:
            case VK_FORMAT_B4G4R4A4_UNORM_PACK16:
            case VK_FORMAT_R5G6B5_UNORM_PACK16:
            case VK_FORMAT_B5G6R5_UNORM_PACK16:
            case VK_FORMAT_R5G5B5A1_UNORM_PACK16:
            case VK_FORMAT_B5G5R5A1_UNORM_PACK16:
            case VK_FORMAT_A1R5G5B5_UNORM_PACK16:
                sizeInfo.blockSize = 16;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R8_UNORM:
            case VK_FORMAT_R8_SNORM:
            case VK_FORMAT_R8_USCALED:
            case VK_FORMAT_R8_SSCALED:
            case VK_FORMAT_R8_UINT:
            case VK_FORMAT_R8_SINT:
            case VK_FORMAT_R8_SRGB:
                sizeInfo.blockSize = 8;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R8G8_UNORM:
            case VK_FORMAT_R8G8_SNORM:
            case VK_FORMAT_R8G8_USCALED:
            case VK_FORMAT_R8G8_SSCALED:
            case VK_FORMAT_R8G8_UINT:
            case VK_FORMAT_R8G8_SINT:
            case VK_FORMAT_R8G8_SRGB:
                sizeInfo.blockSize = 16;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R8G8B8_UNORM:
            case VK_FORMAT_R8G8B8_SNORM:
            case VK_FORMAT_R8G8B8_USCALED:
            case VK_FORMAT_R8G8B8_SSCALED:
            case VK_FORMAT_R8G8B8_UINT:
            case VK_FORMAT_R8G8B8_SINT:
            case VK_FORMAT_R8G8B8_SRGB:
            case VK_FORMAT_B8G8R8_UNORM:
            case VK_FORMAT_B8G8R8_SNORM:
            case VK_FORMAT_B8G8R8_USCALED:
            case VK_FORMAT_B8G8R8_SSCALED:
            case VK_FORMAT_B8G8R8_UINT:
            case VK_FORMAT_B8G8R8_SINT:
            case VK_FORMAT_B8G8R8_SRGB:
                sizeInfo.blockSize = 24;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R8G8B8A8_UNORM:
            case VK_FORMAT_R8G8B8A8_SNORM:
            case VK_FORMAT_R8G8B8A8_USCALED:
            case VK_FORMAT_R8G8B8A8_SSCALED:
            case VK_FORMAT_R8G8B8A8_UINT:
            case VK_FORMAT_R8G8B8A8_SINT:
            case VK_FORMAT_R8G8B8A8_SRGB:
            case VK_FORMAT_B8G8R8A8_UNORM:
            case VK_FORMAT_B8G8R8A8_SNORM:
            case VK_FORMAT_B8G8R8A8_USCALED:
            case VK_FORMAT_B8G8R8A8_SSCALED:
            case VK_FORMAT_B8G8R8A8_UINT:
            case VK_FORMAT_B8G8R8A8_SINT:
            case VK_FORMAT_B8G8R8A8_SRGB:
                sizeInfo.blockSize = 32;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_A8B8G8R8_UNORM_PACK32:
            case VK_FORMAT_A8B8G8R8_SNORM_PACK32:
            case VK_FORMAT_A8B8G8R8_USCALED_PACK32:
            case VK_FORMAT_A8B8G8R8_SSCALED_PACK32:
            case VK_FORMAT_A8B8G8R8_UINT_PACK32:
            case VK_FORMAT_A8B8G8R8_SINT_PACK32:
            case VK_FORMAT_A8B8G8R8_SRGB_PACK32:
                sizeInfo.blockSize = 32;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_A2R10G10B10_UNORM_PACK32:
            case VK_FORMAT_A2R10G10B10_SNORM_PACK32:
            case VK_FORMAT_A2R10G10B10_USCALED_PACK32:
            case VK_FORMAT_A2R10G10B10_SSCALED_PACK32:
            case VK_FORMAT_A2R10G10B10_UINT_PACK32:
            case VK_FORMAT_A2R10G10B10_SINT_PACK32:
            case VK_FORMAT_A2B10G10R10_UNORM_PACK32:
            case VK_FORMAT_A2B10G10R10_SNORM_PACK32:
            case VK_FORMAT_A2B10G10R10_USCALED_PACK32:
            case VK_FORMAT_A2B10G10R10_SSCALED_PACK32:
            case VK_FORMAT_A2B10G10R10_UINT_PACK32:
            case VK_FORMAT_A2B10G10R10_SINT_PACK32:
                sizeInfo.blockSize = 32;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R16_UNORM:
            case VK_FORMAT_R16_SNORM:
            case VK_FORMAT_R16_USCALED:
            case VK_FORMAT_R16_SSCALED:
            case VK_FORMAT_R16_UINT:
            case VK_FORMAT_R16_SINT:
            case VK_FORMAT_R16_SFLOAT:
                sizeInfo.blockSize = 16;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R16G16_UNORM:
            case VK_FORMAT_R16G16_SNORM:
            case VK_FORMAT_R16G16_USCALED:
            case VK_FORMAT_R16G16_SSCALED:
            case VK_FORMAT_R16G16_UINT:
            case VK_FORMAT_R16G16_SINT:
            case VK_FORMAT_R16G16_SFLOAT:
                sizeInfo.blockSize = 32;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R16G16B16_UNORM:
            case VK_FORMAT_R16G16B16_SNORM:
            case VK_FORMAT_R16G16B16_USCALED:
            case VK_FORMAT_R16G16B16_SSCALED:
            case VK_FORMAT_R16G16B16_UINT:
            case VK_FORMAT_R16G16B16_SINT:
            case VK_FORMAT_R16G16B16_SFLOAT:
                sizeInfo.blockSize = 48;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R16G16B16A16_UNORM:
            case VK_FORMAT_R16G16B16A16_SNORM:
            case VK_FORMAT_R16G16B16A16_USCALED:
            case VK_FORMAT_R16G16B16A16_SSCALED:
            case VK_FORMAT_R16G16B16A16_UINT:
            case VK_FORMAT_R16G16B16A16_SINT:
            case VK_FORMAT_R16G16B16A16_SFLOAT:
                sizeInfo.blockSize = 64;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R32_UINT:
            case VK_FORMAT_R32_SINT:
            case VK_FORMAT_R32_SFLOAT:
                sizeInfo.blockSize = 32;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R32G32_UINT:
            case VK_FORMAT_R32G32_SINT:
            case VK_FORMAT_R32G32_SFLOAT:
                sizeInfo.blockSize = 64;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R32G32B32_UINT:
            case VK_FORMAT_R32G32B32_SINT:
            case VK_FORMAT_R32G32B32_SFLOAT:
                sizeInfo.blockSize = 96;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R32G32B32A32_UINT:
            case VK_FORMAT_R32G32B32A32_SINT:
            case VK_FORMAT_R32G32B32A32_SFLOAT:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R64_UINT:
            case VK_FORMAT_R64_SINT:
            case VK_FORMAT_R64_SFLOAT:
                sizeInfo.blockSize = 64;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R64G64_UINT:
            case VK_FORMAT_R64G64_SINT:
            case VK_FORMAT_R64G64_SFLOAT:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R64G64B64_UINT:
            case VK_FORMAT_R64G64B64_SINT:
            case VK_FORMAT_R64G64B64_SFLOAT:
                sizeInfo.blockSize = 192;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_R64G64B64A64_UINT:
            case VK_FORMAT_R64G64B64A64_SINT:
            case VK_FORMAT_R64G64B64A64_SFLOAT:
                sizeInfo.blockSize = 256;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_B10G11R11_UFLOAT_PACK32:
            case VK_FORMAT_E5B9G9R9_UFLOAT_PACK32:
                sizeInfo.blockSize = 32;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_D16_UNORM:
                sizeInfo.blockSize = 16;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_X8_D24_UNORM_PACK32:
                sizeInfo.blockSize = 32;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_D32_SFLOAT:
                sizeInfo.blockSize = 32;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_S8_UINT:
                sizeInfo.blockSize = 8;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_D16_UNORM_S8_UINT:
                sizeInfo.blockSize = 24;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_D24_UNORM_S8_UINT:
                sizeInfo.blockSize = 32;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_D32_SFLOAT_S8_UINT:
                sizeInfo.blockSize = 40;//???
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_BC1_RGB_UNORM_BLOCK:
            case VK_FORMAT_BC1_RGB_SRGB_BLOCK:
            case VK_FORMAT_BC1_RGBA_UNORM_BLOCK:
            case VK_FORMAT_BC1_RGBA_SRGB_BLOCK:
                sizeInfo.blockSize = 64;
                sizeInfo.layout.blockWidth = 4;
                sizeInfo.layout.blockHeight = 4;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_BC2_UNORM_BLOCK:
            case VK_FORMAT_BC2_SRGB_BLOCK:
            case VK_FORMAT_BC3_UNORM_BLOCK:
            case VK_FORMAT_BC3_SRGB_BLOCK:
            case VK_FORMAT_BC4_UNORM_BLOCK:
            case VK_FORMAT_BC4_SNORM_BLOCK:
            case VK_FORMAT_BC5_UNORM_BLOCK:
            case VK_FORMAT_BC5_SNORM_BLOCK:
            case VK_FORMAT_BC6H_UFLOAT_BLOCK:
            case VK_FORMAT_BC6H_SFLOAT_BLOCK:
            case VK_FORMAT_BC7_UNORM_BLOCK:
            case VK_FORMAT_BC7_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 4;
                sizeInfo.layout.blockHeight = 4;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ETC2_R8G8B8_UNORM_BLOCK:
            case VK_FORMAT_ETC2_R8G8B8_SRGB_BLOCK:
            case VK_FORMAT_ETC2_R8G8B8A1_UNORM_BLOCK:
            case VK_FORMAT_ETC2_R8G8B8A1_SRGB_BLOCK:
                sizeInfo.blockSize = 64;
                sizeInfo.layout.blockWidth = 4;
                sizeInfo.layout.blockHeight = 4;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ETC2_R8G8B8A8_UNORM_BLOCK:
            case VK_FORMAT_ETC2_R8G8B8A8_SRGB_BLOCK:
            case VK_FORMAT_EAC_R11_UNORM_BLOCK:
            case VK_FORMAT_EAC_R11_SNORM_BLOCK:
            case VK_FORMAT_EAC_R11G11_UNORM_BLOCK:
            case VK_FORMAT_EAC_R11G11_SNORM_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 4;
                sizeInfo.layout.blockHeight = 4;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ASTC_4x4_UNORM_BLOCK:
            case VK_FORMAT_ASTC_4x4_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 4;
                sizeInfo.layout.blockHeight = 4;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ASTC_5x4_UNORM_BLOCK:
            case VK_FORMAT_ASTC_5x4_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 5;
                sizeInfo.layout.blockHeight = 4;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ASTC_5x5_UNORM_BLOCK:
            case VK_FORMAT_ASTC_5x5_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 5;
                sizeInfo.layout.blockHeight = 5;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ASTC_6x5_UNORM_BLOCK:
            case VK_FORMAT_ASTC_6x5_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 6;
                sizeInfo.layout.blockHeight = 5;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ASTC_6x6_UNORM_BLOCK:
            case VK_FORMAT_ASTC_6x6_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 6;
                sizeInfo.layout.blockHeight = 6;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ASTC_8x5_UNORM_BLOCK:
            case VK_FORMAT_ASTC_8x5_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 8;
                sizeInfo.layout.blockHeight = 5;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ASTC_8x6_UNORM_BLOCK:
            case VK_FORMAT_ASTC_8x6_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 8;
                sizeInfo.layout.blockHeight = 6;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ASTC_8x8_UNORM_BLOCK:
            case VK_FORMAT_ASTC_8x8_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 8;
                sizeInfo.layout.blockHeight = 8;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ASTC_10x5_UNORM_BLOCK:
            case VK_FORMAT_ASTC_10x5_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 10;
                sizeInfo.layout.blockHeight = 5;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ASTC_10x6_UNORM_BLOCK:
            case VK_FORMAT_ASTC_10x6_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 10;
                sizeInfo.layout.blockHeight = 6;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ASTC_10x8_UNORM_BLOCK:
            case VK_FORMAT_ASTC_10x8_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 10;
                sizeInfo.layout.blockHeight = 8;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ASTC_10x10_UNORM_BLOCK:
            case VK_FORMAT_ASTC_10x10_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 10;
                sizeInfo.layout.blockHeight = 10;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ASTC_12x10_UNORM_BLOCK:
            case VK_FORMAT_ASTC_12x10_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 12;
                sizeInfo.layout.blockHeight = 10;
                sizeInfo.layout.blockDepth = 1;
                break;
            case VK_FORMAT_ASTC_12x12_UNORM_BLOCK:
            case VK_FORMAT_ASTC_12x12_SRGB_BLOCK:
                sizeInfo.blockSize = 128;
                sizeInfo.layout.blockWidth = 12;
                sizeInfo.layout.blockHeight = 12;
                sizeInfo.layout.blockDepth = 1;
                break;
            default:
                sizeInfo.blockSize = 1;
                sizeInfo.layout.blockWidth = 1;
                sizeInfo.layout.blockHeight = 1;
                sizeInfo.layout.blockDepth = 1;
                break;
        }
            return sizeInfo;
    }

} // namespace unity2vsg
