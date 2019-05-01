#pragma once

/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

#include <vsg/all.h>

namespace vsg
{
    class GraphicsPipelineBuilder : public Inherit<Object, GraphicsPipelineBuilder>
    {
    public:
        class Traits : public Inherit<Object, Traits>
        {
        public:
            ShaderModules shaderModules;

            using VertexInputAttributeDescription = std::pair<uint32_t, VkFormat>;                // location index, format
            using StructInputAttributeDescription = std::vector<VertexInputAttributeDescription>; // list of vertex input desccriptions making an entire struct for a single binding
            using InputAttributeDescriptions = std::vector<StructInputAttributeDescription>;

            std::map<VkVertexInputRate, InputAttributeDescriptions> vertexAttributeDescriptions;

            using DescriptorBinding = std::pair<uint32_t, VkDescriptorType>; // binding index, descriptor type
            using DescriptorBindngs = std::vector<DescriptorBinding>;
            using DescriptorBindingSet = std::map<VkShaderStageFlags, DescriptorBindngs>;

            std::vector<DescriptorBindingSet> descriptorLayouts;

            ColorBlendState::ColorBlendAttachments colorBlendAttachments;
            VkPrimitiveTopology primitiveTopology = VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
        };

        GraphicsPipelineBuilder();

        virtual void build(ref_ptr<Traits> traits);

        ref_ptr<GraphicsPipeline> getGraphicsPipeline() const { return _graphicsPipeline; }

        static size_t sizeOf(VkFormat format);
        static size_t sizeOf(const Traits::StructInputAttributeDescription& structDescription);

    protected:
        ref_ptr<GraphicsPipeline> _graphicsPipeline;
    };
    VSG_type_name(vsg::GraphicsPipelineBuilder)
} // namespace vsg
