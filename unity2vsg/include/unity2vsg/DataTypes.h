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
        Vec4Array colors;
		Vec2Array uv0;
        Vec2Array uv1;
	};

    struct PipelineData
    {
        uint32_t id;
        uint32_t hasNormals;
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
}

extern "C"
{
    // call delete or delete [] on a native pointer
    UNITY2VSG_EXPORT void unity2vsg_DataTypes_DeleteNativeObject(void* anObjectPointer, bool isArray);
	
    // 
    UNITY2VSG_EXPORT unity2vsg::FloatArray unity2vsg_Tests_GetXValues(unity2vsg::Vec2Array aPointsArray);
}

