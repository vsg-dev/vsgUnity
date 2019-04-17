
#include <vsg2unity/DataTypes.h>

#include <cfloat>

void vsg2unity_DataTypes_DeleteNativeObject(void* anObjectPointer, bool isArray)
{
    if(isArray)
    {
        delete [] anObjectPointer;
    }
    else
    {
        delete anObjectPointer;
    }
}


FloatArray vsg2unity_Tests_GetXValues(Vec2Array aPointsArray)
{
    float* floatarray = new float[aPointsArray.length];
    FloatArray result;
    
    for(int i=0; i<aPointsArray.length; i++)
    {
        floatarray[i] = aPointsArray.ptr[i].x();
    }
    
    result.ptr = floatarray;
    result.length = aPointsArray.length;
    return result;
}

