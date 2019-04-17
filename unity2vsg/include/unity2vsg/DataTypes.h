//
//  DataTypes.h
//  vsgUnity
//
//  Created by Thomas Hogarth
//
#pragma once

#include <vsg2unity/Export.h>

#include <vsg/vec2.h>
#include <vsg/vec3.h>
#include <vsg/vec4.h>

struct IntArray
{
    int* ptr;
    unsigned int length;
};

struct FloatArray
{
    float* ptr;
    unsigned int length;
};

struct Vec2Array
{
    vsg::vec2* ptr;
    unsigned int length;
};

extern "C"
{
	void VSG2UNITY_EXPORT vsg2unity_DataTypes_DeleteNativeObject(void* anObjectPointer, bool isArray);
	
	FloatArray VSG2UNITY_EXPORT vsg2unity_Tests_GetXValues(Vec2Array aPointsArray);
}

