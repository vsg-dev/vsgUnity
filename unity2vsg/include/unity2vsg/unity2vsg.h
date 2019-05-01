#pragma once

/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

#include <unity2vsg/Export.h>
#include <unity2vsg/NativeUtils.h>

extern "C"
{
	UNITY2VSG_EXPORT void unity2vsg_ExportMesh(unity2vsg::MeshData mesh);


    UNITY2VSG_EXPORT void unity2vsg_BeginExport();
    UNITY2VSG_EXPORT void unity2vsg_EndExport(const char* saveFileName);

    UNITY2VSG_EXPORT void unity2vsg_AddGroup();
    UNITY2VSG_EXPORT void unity2vsg_AddTransform(unity2vsg::TransformData transform);
    UNITY2VSG_EXPORT void unity2vsg_AddGeometry(unity2vsg::MeshData mesh);

    UNITY2VSG_EXPORT void unity2vsg_AddStringValue(const char* name, const char* value);

    UNITY2VSG_EXPORT void unity2vsg_AddStateGroup();
    UNITY2VSG_EXPORT void unity2vsg_AddBindGraphicsPipeline(unity2vsg::PipelineData pipeline);
    UNITY2VSG_EXPORT void unity2vsg_AddTexture(unity2vsg::TextureData texture);
    UNITY2VSG_EXPORT void unity2vsg_BindDescriptors();

    UNITY2VSG_EXPORT void unity2vsg_EndNode();

    UNITY2VSG_EXPORT void unity2vsg_LaunchViewer(const char* filename, uint32_t useCamData, unity2vsg::CameraData  camdata);
}
