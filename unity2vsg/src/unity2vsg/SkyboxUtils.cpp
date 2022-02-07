/* <editor-fold desc="MIT License">

Copyright(c) 2018 Robert Osfield
Copyright(c) 2022 Christian Schott (InstruNEXT GmbH)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

#include <unity2vsg/SkyboxUtils.h>

#include <vsg/state/ShaderStage.h>
#include <vsg/state/DescriptorSet.h>
#include <vsg/state/VertexInputState.h>
#include <vsg/state/RasterizationState.h>
#include <vsg/state/DepthStencilState.h>
#include <vsg/state/InputAssemblyState.h>
#include <vsg/state/MultisampleState.h>
#include <vsg/state/ColorBlendState.h>
#include <vsg/state/DescriptorImage.h>
#include <vsg/nodes/StateGroup.h>
#include <vsg/nodes/VertexIndexDraw.h>
#include <vsg/nodes/MatrixTransform.h>
#include <vsg/maths/transform.h>
 
namespace unity2vsg
{
    
const auto skybox_vert = R"(
    #version 450
    #extension GL_ARB_separate_shader_objects : enable
    layout(push_constant) uniform PushConstants {
        mat4 projection;
        mat4 modelView;
    } pc;
    layout(location = 0) in vec3 osg_Vertex;
    layout(location = 0) out vec3 outUVW;
    out gl_PerVertex{ vec4 gl_Position; };
    void main()
    {
        outUVW = osg_Vertex;
        // Remove translation
        mat4 modelView = pc.modelView;
        modelView[3] = vec4(0.0, 0.0, 0.0, 1.0);
        gl_Position = pc.projection * modelView * vec4(osg_Vertex, 1.0);
    }
    )";

const auto skybox_frag = R"(
    #version 450
    #extension GL_ARB_separate_shader_objects : enable
    layout(binding = 0) uniform samplerCube envMap;
    layout(location = 0) in vec3 inUVW;
    layout(location = 0) out vec4 outColor;
    void main()
    {
        outColor = vec4(textureLod(envMap, inUVW, 0).xyz, 1.0);
    }
    )";

vsg::ref_ptr<vsg::Node> createSkybox(const vsg::ref_ptr<vsg::DescriptorImage> &cubemapTexture)
{
    auto vertexShader = vsg::ShaderStage::create(VK_SHADER_STAGE_VERTEX_BIT,
                                                 "main",
                                                 skybox_vert);
    auto fragmentShader = vsg::ShaderStage::create(VK_SHADER_STAGE_FRAGMENT_BIT,
                                                   "main",
                                                   skybox_frag);
    const vsg::ShaderStages shaders{vertexShader, fragmentShader};

    // set up graphics pipeline
    vsg::DescriptorSetLayoutBindings descriptorBindings{
          {0,
           VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER,
           1,
           VK_SHADER_STAGE_FRAGMENT_BIT,
           nullptr}};

    auto descriptorSetLayout =
          vsg::DescriptorSetLayout::create(descriptorBindings);

    vsg::PushConstantRanges pushConstantRanges{
          {VK_SHADER_STAGE_VERTEX_BIT,
           0,
           128}  // projection view, and model matrices, actual push constant
                 // calls automatically provided by the VSG's DispatchTraversal
    };

    vsg::VertexInputState::Bindings vertexBindingsDescriptions{
          VkVertexInputBindingDescription{0,
                                          sizeof(vsg::vec3),
                                          VK_VERTEX_INPUT_RATE_VERTEX}};

    vsg::VertexInputState::Attributes vertexAttributeDescriptions{
          VkVertexInputAttributeDescription{0,
                                            0,
                                            VK_FORMAT_R32G32B32_SFLOAT,
                                            0}};

    auto rasterState = vsg::RasterizationState::create();
    rasterState->cullMode = VK_CULL_MODE_FRONT_BIT;

    auto depthState = vsg::DepthStencilState::create();
    depthState->depthTestEnable = VK_FALSE;
    depthState->depthWriteEnable = VK_FALSE;

    vsg::GraphicsPipelineStates pipelineStates{
          vsg::VertexInputState::create(vertexBindingsDescriptions,
                                        vertexAttributeDescriptions),
          vsg::InputAssemblyState::create(),
          rasterState,
          vsg::MultisampleState::create(),
          vsg::ColorBlendState::create(),
          depthState};

    auto pipelineLayout = vsg::PipelineLayout::create(
          vsg::DescriptorSetLayouts{descriptorSetLayout},
          pushConstantRanges);
    auto pipeline = vsg::GraphicsPipeline::create(pipelineLayout,
                                                  shaders,
                                                  pipelineStates);
    auto bindGraphicsPipeline = vsg::BindGraphicsPipeline::create(pipeline);

    auto descriptorSet = vsg::DescriptorSet::create(descriptorSetLayout,
                                                    vsg::Descriptors{cubemapTexture});
    auto bindDescriptorSet =
          vsg::BindDescriptorSet::create(VK_PIPELINE_BIND_POINT_GRAPHICS,
                                         pipelineLayout,
                                         0,
                                         descriptorSet);

    auto root = vsg::StateGroup::create();
    root->add(bindGraphicsPipeline);
    root->add(bindDescriptorSet);

    auto vertices = vsg::vec3Array::create({// Back
                                            {-1.0f, -1.0f, -1.0f},
                                            {1.0f, -1.0f, -1.0f},
                                            {-1.0f, 1.0f, -1.0f},
                                            {1.0f, 1.0f, -1.0f},

                                            // Front
                                            {-1.0f, -1.0f, 1.0f},
                                            {1.0f, -1.0f, 1.0f},
                                            {-1.0f, 1.0f, 1.0f},
                                            {1.0f, 1.0f, 1.0f},

                                            // Left
                                            {-1.0f, -1.0f, -1.0f},
                                            {-1.0f, -1.0f, 1.0f},
                                            {-1.0f, 1.0f, -1.0f},
                                            {-1.0f, 1.0f, 1.0f},

                                            // Right
                                            {1.0f, -1.0f, -1.0f},
                                            {1.0f, -1.0f, 1.0f},
                                            {1.0f, 1.0f, -1.0f},
                                            {1.0f, 1.0f, 1.0f},

                                            // Bottom
                                            {-1.0f, -1.0f, -1.0f},
                                            {-1.0f, -1.0f, 1.0f},
                                            {1.0f, -1.0f, -1.0f},
                                            {1.0f, -1.0f, 1.0f},

                                            // Top
                                            {-1.0f, 1.0f, -1.0f},
                                            {-1.0f, 1.0f, 1.0f},
                                            {1.0f, 1.0f, -1.0f},
                                            {1.0f, 1.0f, 1.0}});

    auto indices = vsg::ushortArray::create({// Back
                                             0,
                                             2,
                                             1,
                                             1,
                                             2,
                                             3,

                                             // Front
                                             6,
                                             4,
                                             5,
                                             7,
                                             6,
                                             5,

                                             // Left
                                             10,
                                             8,
                                             9,
                                             11,
                                             10,
                                             9,

                                             // Right
                                             14,
                                             13,
                                             12,
                                             15,
                                             13,
                                             14,

                                             // Bottom
                                             17,
                                             16,
                                             19,
                                             19,
                                             16,
                                             18,

                                             // Top
                                             23,
                                             20,
                                             21,
                                             22,
                                             20,
                                             23});

    auto draw = vsg::VertexIndexDraw::create();
    draw->assignIndices(indices);
    draw->indexCount = indices->size();
    draw->assignArrays(vsg::DataList{vertices});
    draw->instanceCount = 1;
    root->addChild(draw);


    auto xform = vsg::MatrixTransform::create(
          vsg::rotate(vsg::PI * 0.5, 1.0, 0.0, 0.0));
    xform->addChild(root);

    return xform;
}

}
