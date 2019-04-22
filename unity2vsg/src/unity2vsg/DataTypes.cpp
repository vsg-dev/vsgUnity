/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

#include <unity2vsg/DataTypes.h>

#include <cfloat>

using namespace unity2vsg;


void unity2vsg_DataTypes_DeleteNativeObject(void* anObjectPointer, bool isArray)
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


unity2vsg::FloatArray unity2vsg_Tests_GetXValues(unity2vsg::Vec2Array aPointsArray)
{
    float* floatarray = new float[aPointsArray.length];
    unity2vsg::FloatArray result;
    
    for(int i=0; i<aPointsArray.length; i++)
    {
        floatarray[i] = aPointsArray.ptr[i].x;
    }
    
    result.ptr = floatarray;
    result.length = aPointsArray.length;
    return result;
}

