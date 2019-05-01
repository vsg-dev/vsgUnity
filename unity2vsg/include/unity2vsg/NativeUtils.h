#pragma once

/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

#include <unity2vsg/Export.h>

#include <vsg/maths/vec2.h>
#include <vsg/maths/vec3.h>
#include <vsg/maths/vec4.h>
#include <vsg/core/Array.h>

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

	struct MeshData
	{
        uint32_t id;
		Vec3Array verticies;
		IntArray triangles;
		Vec3Array normals;
        Vec3Array tangents;
        Vec4Array colors;
		Vec2Array uv0;
        Vec2Array uv1;
	};

    enum TexFormat
    {
        R8_UNORM = 0,
        R8G8_UNORM = 1,
        R8G8B8_UNORM = 2,
        R8G8B8A8_UNORM = 3,
        BC1_RGB_UNORM = 4, //dxt1
        BC1_RGBA_UNORM = 5, //dxt1
        UnsupportedFormat = 9999
    };

    enum MipmapFilterMode
    {
        Point = 0,
        Bilinear = 1,
        Trilinear = 2,
        UnsupportedFilterMode = 9999
    };

    enum WrapMode
    {
        Repeat = 0,
        Clamp = 1,
        Mirror = 2,
        MirrorOnce = 3,
        UnsupportedWrapMode = 9999
    };

    struct TextureData
    {
        uint32_t id;
        uint32_t channel;
        ByteArray pixels;
        TexFormat format;
        uint32_t width;
        uint32_t height;
        uint32_t depth;
        uint32_t anisoLevel;
        WrapMode wrapMode;
        MipmapFilterMode filterMode;
        uint32_t mipmapCount;
        float mipmapBias;
    };

    struct PipelineData
    {
        uint32_t id;
        uint32_t hasNormals;
        uint32_t hasTangents;
        uint32_t hasColors;
        uint32_t uvChannelCount;
        uint32_t vertexImageSamplerCount;
        uint32_t fragmentImageSamplerCount;
        uint32_t vertexUniformCount;
        uint32_t fragmentUniformCount;
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

    
    VkFormat vkFormatForTexFormat(TexFormat format)
    {
        switch (format)
        {
            case TexFormat::R8_UNORM: return VkFormat::VK_FORMAT_R8_UNORM;
            case TexFormat::R8G8_UNORM: return VkFormat::VK_FORMAT_R8G8_UNORM;
            case TexFormat::R8G8B8_UNORM: return VkFormat::VK_FORMAT_R8G8B8_UNORM;
            case TexFormat::R8G8B8A8_UNORM: return VkFormat::VK_FORMAT_R8G8B8A8_UNORM;
            case TexFormat::BC1_RGB_UNORM: return VkFormat::VK_FORMAT_BC1_RGB_UNORM_BLOCK;
            case TexFormat::BC1_RGBA_UNORM: return VkFormat::VK_FORMAT_BC1_RGBA_UNORM_BLOCK;
            default: break;
        }
        return VkFormat::VK_FORMAT_R8_UNORM;
    }


    VkSamplerAddressMode vkSamplerAddressModeForWrapMode(WrapMode wrap)
    {
        switch (wrap)
        {
            case WrapMode::Repeat: return VK_SAMPLER_ADDRESS_MODE_REPEAT;
            case WrapMode::Clamp: return VK_SAMPLER_ADDRESS_MODE_CLAMP_TO_EDGE;
            case WrapMode::Mirror:
            case WrapMode::MirrorOnce: return VK_SAMPLER_ADDRESS_MODE_MIRRORED_REPEAT;
            default: break;
        }
        return VK_SAMPLER_ADDRESS_MODE_MAX_ENUM; // unknown
    }

    std::pair<VkFilter, VkSamplerMipmapMode> vkFilterAndSamplerMipmapModeForMipmapFilterMode(MipmapFilterMode filtermode)
    {
        switch (filtermode)
        {
            case MipmapFilterMode::Point: return { VK_FILTER_NEAREST, VK_SAMPLER_MIPMAP_MODE_NEAREST };
            case MipmapFilterMode::Bilinear: return { VK_FILTER_LINEAR, VK_SAMPLER_MIPMAP_MODE_NEAREST };
            case MipmapFilterMode::Trilinear: return { VK_FILTER_LINEAR, VK_SAMPLER_MIPMAP_MODE_LINEAR };
            default: break;
        }
        return { VK_FILTER_MAX_ENUM, VK_SAMPLER_MIPMAP_MODE_MAX_ENUM }; // unknown
    }

    VkSamplerCreateInfo vkSamplerCreateInfoForTextureData(const TextureData& data)
    {
        auto minFilterMipmapMode = vkFilterAndSamplerMipmapModeForMipmapFilterMode(data.filterMode);
        auto magFilterMipmapMode = vkFilterAndSamplerMipmapModeForMipmapFilterMode(data.filterMode);
        bool mipmappingRequired = data.filterMode == MipmapFilterMode::Trilinear;

        VkSamplerCreateInfo samplerInfo = {};
        samplerInfo.sType = VK_STRUCTURE_TYPE_SAMPLER_CREATE_INFO;
        samplerInfo.minFilter = minFilterMipmapMode.first;
        samplerInfo.magFilter = magFilterMipmapMode.first;
        samplerInfo.mipmapMode = minFilterMipmapMode.second;
        samplerInfo.addressModeU = vkSamplerAddressModeForWrapMode(data.wrapMode);
        samplerInfo.addressModeV = vkSamplerAddressModeForWrapMode(data.wrapMode);
        samplerInfo.addressModeW = vkSamplerAddressModeForWrapMode(data.wrapMode);

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
}

