/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

#include <unity2vsg/GraphicsPipelineBuilder.h>

#include <iostream>
#include <sstream>

using namespace vsg;

GraphicsPipelineBuilder::GraphicsPipelineBuilder() :
    Inherit()
{
}

void GraphicsPipelineBuilder::build(ref_ptr<Traits> traits)
{
    // set up graphics pipeline

    // create descriptor layouts
    DescriptorSetLayouts descriptorSetLayouts;

    for (auto& bindingSet : traits->descriptorLayouts)
    {
        DescriptorSetLayoutBindings setLayoutBindings;
        for (auto& stageBindings : bindingSet) // could order of stage type in map be important here??
        {
            for (auto& stageBinding : stageBindings.second)
            {
                setLayoutBindings.push_back({stageBinding.index, stageBinding.type, stageBinding.count, stageBindings.first, nullptr});
            }
        }
        descriptorSetLayouts.push_back(DescriptorSetLayout::create(setLayoutBindings));
    }

    // TODO
    descriptorSetLayouts.push_back(ViewDescriptorSetLayout::create());

    // create vertex bindings and attributes
    VertexInputState::Bindings vertexBindingsDescriptions;
    VertexInputState::Attributes vertexAttributeDescriptions;
    uint32_t locationIndex = 0;
    uint32_t bindingIndex = 0;

    for (auto& rate : {VK_VERTEX_INPUT_RATE_VERTEX, VK_VERTEX_INPUT_RATE_INSTANCE})
    {
        Traits::InputAttributeDescriptions rateDescriptions = traits->vertexAttributeDescriptions[rate];

        for (uint32_t i = 0; i < rateDescriptions.size(); i++)
        {
            Traits::StructInputAttributeDescription& structDescription = rateDescriptions[i];
            // sum the size of this struct
            uint32_t totalsize = static_cast<uint32_t>(sizeOf(structDescription));

            vertexBindingsDescriptions.push_back(VkVertexInputBindingDescription{bindingIndex, totalsize, rate});

            uint32_t offset = 0;
            for (uint32_t l = 0; l < structDescription.size(); l++)
            {
                vertexAttributeDescriptions.push_back(VkVertexInputAttributeDescription{structDescription[l].first, bindingIndex, structDescription[l].second, offset});
                offset += static_cast<uint32_t>(sizeOf(structDescription[l].second));
                locationIndex++;
            }

            bindingIndex++;
        }
    }

    //auto shaderStages = ShaderStages::create(traits->shaderModules);
    //shaderStages->setSpecializationInfos(traits->specializationInfos);

    GraphicsPipelineStates pipelineStates{
        VertexInputState::create(vertexBindingsDescriptions, vertexAttributeDescriptions),
        InputAssemblyState::create(traits->primitiveTopology),
        RasterizationState::create(),
        MultisampleState::create(),
        traits->colorBlendAttachments.size() > 0 ? ColorBlendState::create(traits->colorBlendAttachments) : ColorBlendState::create(),
        DepthStencilState::create()};

    PushConstantRanges pushConstantRanges{
        {VK_SHADER_STAGE_VERTEX_BIT, 0, 128} // projection view, and model matrices
    };

    auto pipelineLayout = PipelineLayout::create(descriptorSetLayouts, pushConstantRanges);
    _graphicsPipeline = GraphicsPipeline::create(pipelineLayout, traits->shaderStages, pipelineStates);
}

size_t GraphicsPipelineBuilder::sizeOf(VkFormat format)
{
    switch (format)
    {
    // float
    case VkFormat::VK_FORMAT_R32G32B32A32_SFLOAT: return sizeof(vec4);
    case VkFormat::VK_FORMAT_R32G32B32_SFLOAT: return sizeof(vec3);
    case VkFormat::VK_FORMAT_R32G32_SFLOAT: return sizeof(vec2);
    case VkFormat::VK_FORMAT_R32_SFLOAT: return sizeof(float);

    // uint8
    case VkFormat::VK_FORMAT_B8G8R8A8_UINT: return sizeof(ubvec4);
    case VkFormat::VK_FORMAT_B8G8R8_UINT: return sizeof(ubvec3);
    case VkFormat::VK_FORMAT_R8G8_UINT: return sizeof(ubvec2);
    case VkFormat::VK_FORMAT_R8_UINT: return sizeof(uint8_t);

    // uint16
    case VkFormat::VK_FORMAT_R16_UINT: return sizeof(uint16_t);

    // uint32
    case VkFormat::VK_FORMAT_R32_UINT: return sizeof(uint32_t);

    default: break;
    }
    return 0;
}
size_t GraphicsPipelineBuilder::sizeOf(const Traits::StructInputAttributeDescription& structDescription)
{
    size_t size = 0;
    for (const auto& vid : structDescription)
    {
        size += sizeOf(vid.second);
    }
    return size;
}
