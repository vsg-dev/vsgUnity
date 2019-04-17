//
//  Debug.h
//  vsgUnity
//
//  Created by Thomas Hogarth
//
#pragma once

#include <vsg2unity/Export.h>

typedef void (*StringArgFuncPtr)( const char * );
StringArgFuncPtr DebugLog;

extern "C"
{
    void VSG2UNITY_EXPORT vsg2unity_Debug_SetDebugLogCallback(StringArgFuncPtr aFunctionPointer);
}