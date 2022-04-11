/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth
Copyright(c) 2022 Christian Schott (InstruNEXT GmbH)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

using System.Runtime.InteropServices;

namespace vsgUnity.Native
{
    public static class GraphBuilderInterface
    {
        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_BeginExport")]
        public static extern void unity2vsg_BeginExport();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_EndExport")]
        public static extern void unity2vsg_EndExport([MarshalAs(UnmanagedType.LPStr)] string saveFileName);

        //
        // Nodes
        //

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddGroupNode")]
        public static extern void unity2vsg_AddGroupNode();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddTransformNode")]
        public static extern void unity2vsg_AddTransformNode(TransformData transform);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddCullNode")]
        public static extern void unity2vsg_AddCullNode(CullData cull);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddCullGroupNode")]
        public static extern void unity2vsg_AddCullGroupNode(CullData cull);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddLODNode")]
        public static extern void unity2vsg_AddLODNode(CullData cull);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddLODChild")]
        public static extern void unity2vsg_AddLODChild(LODChildData lodChildData);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddStateGroupNode")]
        public static extern void unity2vsg_AddStateGroupNode();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddCommandsNode")]
        public static extern void unity2vsg_AddCommandsNode();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddVertexIndexDrawNode")]
        public static extern void unity2vsg_AddVertexIndexDrawNode(VertexIndexDrawData mesh);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddSkybox")]
        public static extern void unity2vsg_AddSkybox(DescriptorImageData skyboxTexture);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddLightNode")]
        public static extern void unity2vsg_AddLightNode(LightData lightData);

        //
        // Meta Data
        //

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddStringValue")]
        public static extern void unity2vsg_AddStringValue([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string value);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddFloatArray")]
        public static extern void unity2vsg_AddFloatArray([MarshalAs(UnmanagedType.LPStr)] string name, FloatArray data);

        //
        // Commands
        //

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddBindGraphicsPipelineCommand", CallingConvention = CallingConvention.StdCall)]
        public static extern int unity2vsg_AddBindGraphicsPipelineCommand(PipelineData pipeline, int addToStateGroup);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddBindIndexBufferCommand")]
        public static extern void unity2vsg_AddBindIndexBufferCommand(IndexBufferData data);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddBindVertexBuffersCommand")]
        public static extern void unity2vsg_AddBindVertexBuffersCommand(VertexBuffersData data);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddDrawIndexedCommand")]
        public static extern void unity2vsg_AddDrawIndexedCommand(DrawIndexedData data);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_CreateBindDescriptorSetCommand")]
        public static extern void unity2vsg_CreateBindDescriptorSetCommand(int addToStateGroup);

        //
        // Descriptors
        //

        // images

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddDescriptorImage")]
        public static extern void unity2vsg_AddDescriptorImage(DescriptorImageData texture);

        // uniform buffers

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddDescriptorBufferFloat")]
        public static extern void unity2vsg_AddDescriptorBufferFloat(DescriptorFloatUniformData data);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddDescriptorBufferFloatArray")]
        public static extern void unity2vsg_AddDescriptorBufferFloatArray(DescriptorFloatArrayUniformData data);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddDescriptorBufferFloatBuffer")]
        public static extern void unity2vsg_AddDescriptorBufferFloatBuffer(DescriptorFloatBufferUniformData data);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddDescriptorBufferVector")]
        public static extern void unity2vsg_AddDescriptorBufferVector(DescriptorVectorUniformData data);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddDescriptorBufferVectorArray")]
        public static extern void unity2vsg_AddDescriptorBufferVectorArray(DescriptorVectorArrayUniformData data);
        
        //
        //

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_EndNode")]
        public static extern void unity2vsg_EndNode();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_LaunchViewer")]
        public static extern void unity2vsg_LaunchViewer([MarshalAs(UnmanagedType.LPStr)] string fileName, int useCamData, CameraData camdata);
    }
}
