#pragma once

/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth
Copyright(c) 2022 Christian Schott (InstruNEXT GmbH)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

#include <unity2vsg/Export.h>
#include <unity2vsg/NativeUtils.h>

extern "C"
{
    UNITY2VSG_EXPORT void unity2vsg_BeginExport();
    UNITY2VSG_EXPORT void unity2vsg_EndExport(const char* saveFileName);

    // add nodes
    UNITY2VSG_EXPORT void unity2vsg_AddGroupNode();
    UNITY2VSG_EXPORT void unity2vsg_AddTransformNode(unity2vsg::TransformData transform);
    UNITY2VSG_EXPORT void unity2vsg_AddCullNode(unity2vsg::CullData cull);
    UNITY2VSG_EXPORT void unity2vsg_AddCullGroupNode(unity2vsg::CullData cull);
    UNITY2VSG_EXPORT void unity2vsg_AddLODNode(unity2vsg::CullData cull);
    UNITY2VSG_EXPORT void unity2vsg_AddLODChild(unity2vsg::LODChildData lodChildData);
    UNITY2VSG_EXPORT void unity2vsg_AddStateGroupNode();
    UNITY2VSG_EXPORT void unity2vsg_AddCommandsNode();
    UNITY2VSG_EXPORT void unity2vsg_AddVertexIndexDrawNode(unity2vsg::VertexIndexDrawData mesh);
    UNITY2VSG_EXPORT void unity2vsg_AddSkybox(unity2vsg::DescriptorImageData skyboxTexture);
    UNITY2VSG_EXPORT void unity2vsg_AddLightNode(unity2vsg::LightData lightData);

    // add meta data to nodes
    UNITY2VSG_EXPORT void unity2vsg_AddStringValue(const char* name, const char* value);
    UNITY2VSG_EXPORT void unity2vsg_AddFloatArray(const char* name, unity2vsg::FloatArray data);

    // add command to commands node if one is current head or last stategroup node
    UNITY2VSG_EXPORT int unity2vsg_AddBindGraphicsPipelineCommand(unity2vsg::PipelineData pipeline, uint32_t addToStateGroup);
    UNITY2VSG_EXPORT void unity2vsg_AddBindIndexBufferCommand(unity2vsg::IndexBufferData data);
    UNITY2VSG_EXPORT void unity2vsg_AddBindVertexBuffersCommand(unity2vsg::VertexBuffersData data);
    UNITY2VSG_EXPORT void unity2vsg_AddDrawIndexedCommand(unity2vsg::DrawIndexedData data);
    // create and add a binddescriptorset command using the current list of descriptors
    UNITY2VSG_EXPORT void unity2vsg_CreateBindDescriptorSetCommand(uint32_t addToStateGroup);
    //UNITY2VSG_EXPORT void unity2vsg_CreateBindViewDescriptorSetCommand(uint32_t addToStateGroup);

    // add descriptor to current descriptors list that will be bound by BindDescriptors call
    UNITY2VSG_EXPORT void unity2vsg_AddDescriptorImage(unity2vsg::DescriptorImageData texture);

    UNITY2VSG_EXPORT void unity2vsg_AddDescriptorBufferFloat(unity2vsg::DescriptorFloatUniformData data);
    UNITY2VSG_EXPORT void unity2vsg_AddDescriptorBufferFloatArray(unity2vsg::DescriptorFloatArrayUniformData data);
    UNITY2VSG_EXPORT void unity2vsg_AddDescriptorBufferFloatBuffer(unity2vsg::DescriptorFloatBufferUniformData data);
    UNITY2VSG_EXPORT void unity2vsg_AddDescriptorBufferVector(unity2vsg::DescriptorVectorUniformData data);
    UNITY2VSG_EXPORT void unity2vsg_AddDescriptorBufferVectorArray(unity2vsg::DescriptorVectorArrayUniformData data);

    UNITY2VSG_EXPORT void unity2vsg_EndNode();

    UNITY2VSG_EXPORT void unity2vsg_LaunchViewer(const char* filename, uint32_t useCamData, unity2vsg::CameraData camdata);
}
