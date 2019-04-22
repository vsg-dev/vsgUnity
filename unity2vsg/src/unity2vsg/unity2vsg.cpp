/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

#include <unity2vsg/unity2vsg.h>

#include <unity2vsg/Debug.h>
#include <unity2vsg/GraphicsPipelineBuilder.h>
#include <unity2vsg/ShaderUtils.h>

#include <vsg/all.h>

using namespace unity2vsg;

void unity2vsg_ConvertMesh(unity2vsg::Mesh mesh)
{
    DebugLog("Here0");

	vsg::ref_ptr<vsg::MatrixTransform> root = vsg::MatrixTransform::create();

    // setup the GraphicsPiplineBuilder
    vsg::ref_ptr<vsg::GraphicsPipelineBuilder> pipelinebuilder = vsg::GraphicsPipelineBuilder::create();
    vsg::ref_ptr<vsg::GraphicsPipelineBuilder::Traits> traits = vsg::GraphicsPipelineBuilder::Traits::create();

    DebugLog("Here1");

    // vertex input
    auto inputarrays = vsg::DataList{ createVsgArray<vsg::vec3>(mesh.verticies.ptr, mesh.verticies.length) }; // always have verticies
    vsg::GraphicsPipelineBuilder::Traits::BindingFormats inputformats = { { VK_FORMAT_R32G32B32_SFLOAT } };
    uint32_t inputshaderatts = VERTEX;

    DebugLog("Here2");

    if (mesh.normals.length > 0) // normals
    {
        inputarrays.push_back(createVsgArray<vsg::vec3>(mesh.normals.ptr, mesh.normals.length));
        inputformats.push_back({ VK_FORMAT_R32G32B32_SFLOAT });
        inputshaderatts |= NORMAL;
    }
    if (mesh.uv0.length > 0) // uv set 0
    {
        inputarrays.push_back(createVsgArray<vsg::vec2>(mesh.uv0.ptr, mesh.uv0.length));
        inputformats.push_back({ VK_FORMAT_R32G32_SFLOAT });
        inputshaderatts |= TEXCOORD0;
    }

    DebugLog("Here3");

    traits->vertexAttributes[VK_VERTEX_INPUT_RATE_VERTEX] = inputformats;

    // descriptor sets layout
    vsg::GraphicsPipelineBuilder::Traits::BindingSet descriptors;
    uint32_t shaderMode = LIGHTING;

    descriptors[VK_SHADER_STAGE_FRAGMENT_BIT] = {
        VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER,
    };
    /*traits->descriptorLayouts =
    {
        descriptors
    };*/

    DebugLog("Here4");

    // shaders
    vsg::ShaderModules shaders{
        vsg::ShaderModule::create(VK_SHADER_STAGE_VERTEX_BIT, "main", createFbxVertexSource(shaderMode, inputshaderatts)),
        vsg::ShaderModule::create(VK_SHADER_STAGE_FRAGMENT_BIT, "main", createFbxFragmentSource(shaderMode, inputshaderatts))
    };

    ShaderCompiler shaderCompiler;
    if (!shaderCompiler.compile(shaders))
    {
        for (auto array : inputarrays) // release arrays before exit
        {
            array->dataRelease();
        }
        DebugLog("Error, failed to compile shaders.");
        return;
    }

    traits->shaderModules = shaders;

    DebugLog("Here5");

    // topology
    traits->primitiveTopology = VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;

    // create our graphics pipeline
    pipelinebuilder->build(traits);

    DebugLog("Here6");

    // add a stategroup and add a bindgraphics pipleline with the graphics pipeline we just created
    vsg::ref_ptr<vsg::StateGroup> stateGroup = vsg::StateGroup::create();
    root->addChild(stateGroup);

    auto bindGraphicsPipeline = vsg::BindGraphicsPipeline::create(pipelinebuilder->getGraphicsPipeline());
    stateGroup->add(bindGraphicsPipeline);

    DebugLog("Here7");

    // now create a geometry using the input arrays we have created
    auto geometry = vsg::Geometry::create();

    geometry->_arrays = inputarrays;

    // for now convert the int32 array indicies to uint16
    vsg::ref_ptr<vsg::ushortArray> indiciesushort(new vsg::ushortArray(mesh.triangles.length));
    for (uint32_t i = 0; i < mesh.triangles.length; i++)
    {
        indiciesushort->set(i, static_cast<uint16_t>(mesh.triangles.ptr[i]));
    }

    DebugLog("Here8");

    geometry->_indices = indiciesushort; //createVsgArray<uint16_t>(reinterpret_cast<uint16_t*>(mesh.triangles.ptr), mesh.triangles.length);
    geometry->_commands = { vsg::DrawIndexed::create(mesh.triangles.length, 1, 0, 0, 0) };

    // add the geometry
    stateGroup->addChild(geometry);

    DebugLog("Here9");

    // write the graph to file
    vsg::vsgReaderWriter io;
    io.writeFile(root.get(), "C:\\Work\\VSG\\unityexport.vsga");

    DebugLog("Here10");

    // we're done so release the arrays before vsg ref_ptr tries to delete them (at the mo they are C# memory)
    for(auto array : inputarrays)
    {
        array->dataRelease();
    }

    DebugLog("Here11");

}

