
#include <vsg2unity/Debug.h>

void vsg2unity_Debug_SetDebugLogCallback(StringArgFuncPtr aFunctionPointer)
{
    DebugLog = aFunctionPointer;
}


