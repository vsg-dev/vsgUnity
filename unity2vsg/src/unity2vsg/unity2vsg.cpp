/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

#include <unity2vsg/unity2vsg.h>

#include <unity2vsg/DebugLog.h>
#include <unity2vsg/GraphicsPipelineBuilder.h>
#include <unity2vsg/ShaderUtils.h>

#include <vsg/all.h>
#include <vsg/core/Objects.h>

using namespace unity2vsg;

void unity2vsg_ExportMesh(unity2vsg::MeshData mesh)
{
    vsg::ref_ptr<vsg::MatrixTransform> root = vsg::MatrixTransform::create();

    // setup the GraphicsPiplineBuilder
    vsg::ref_ptr<vsg::GraphicsPipelineBuilder> pipelinebuilder = vsg::GraphicsPipelineBuilder::create();
    vsg::ref_ptr<vsg::GraphicsPipelineBuilder::Traits> traits = vsg::GraphicsPipelineBuilder::Traits::create();

    // vertex input
    auto inputarrays = vsg::DataList{createVsgArray<vsg::vec3>(mesh.verticies.ptr, mesh.verticies.length)}; // always have verticies
    vsg::GraphicsPipelineBuilder::Traits::InputAttributeDescriptions inputAttributes = {{{0, VK_FORMAT_R32G32B32_SFLOAT}}};
    uint32_t inputshaderatts = VERTEX;

    if (mesh.normals.length > 0) // normals
    {
        inputarrays.push_back(createVsgArray<vsg::vec3>(mesh.normals.ptr, mesh.normals.length));
        inputAttributes.push_back({{1, VK_FORMAT_R32G32B32_SFLOAT}});
        inputshaderatts |= NORMAL;
    }
    if (mesh.tangents.length > 0) // normals
    {
        inputarrays.push_back(createVsgArray<vsg::vec3>(mesh.tangents.ptr, mesh.tangents.length));
        inputAttributes.push_back({{2, VK_FORMAT_R32G32B32_SFLOAT}});
        inputshaderatts |= TANGENT;
    }
    if (mesh.colors.length > 0) // colors
    {
        inputarrays.push_back(createVsgArray<vsg::vec4>(mesh.colors.ptr, mesh.colors.length));
        inputAttributes.push_back({{3, VK_FORMAT_R32G32B32A32_SFLOAT}});
        inputshaderatts |= COLOR;
    }
    if (mesh.uv0.length > 0) // uv set 0
    {
        inputarrays.push_back(createVsgArray<vsg::vec2>(mesh.uv0.ptr, mesh.uv0.length));
        inputAttributes.push_back({{4, VK_FORMAT_R32G32_SFLOAT}});
        inputshaderatts |= TEXCOORD0;
    }

    traits->vertexAttributeDescriptions[VK_VERTEX_INPUT_RATE_VERTEX] = inputAttributes;

    // descriptor sets layout
    vsg::GraphicsPipelineBuilder::Traits::DescriptorBindingSet bindingSet;
    uint32_t shaderMode = LIGHTING;

    /*if (mesh.uv0.length > 0)
    {
        bindingSet[VK_SHADER_STAGE_FRAGMENT_BIT].push_back( { 0, VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER } );
    }

    traits->descriptorLayouts =
    {
        bindingSet
    };*/

    // shaders
    vsg::ShaderModules shaders{
        vsg::ShaderModule::create(VK_SHADER_STAGE_VERTEX_BIT, "main", createFbxVertexSource(shaderMode, inputshaderatts)),
        vsg::ShaderModule::create(VK_SHADER_STAGE_FRAGMENT_BIT, "main", createFbxFragmentSource(shaderMode, inputshaderatts))};

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

    // topology
    traits->primitiveTopology = VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;

    // create our graphics pipeline
    pipelinebuilder->build(traits);

    // add a stategroup and add a bindgraphics pipleline with the graphics pipeline we just created
    vsg::ref_ptr<vsg::StateGroup> stateGroup = vsg::StateGroup::create();
    root->addChild(stateGroup);

    auto bindGraphicsPipeline = vsg::BindGraphicsPipeline::create(pipelinebuilder->getGraphicsPipeline());
    stateGroup->add(bindGraphicsPipeline);

    // now create a geometry using the input arrays we have created
    auto geometry = vsg::Geometry::create();

    geometry->_arrays = inputarrays;

    // for now convert the int32 array indicies to uint16
    vsg::ref_ptr<vsg::ushortArray> indiciesushort(new vsg::ushortArray(mesh.triangles.length));
    for (uint32_t i = 0; i < mesh.triangles.length; i++)
    {
        indiciesushort->set(i, static_cast<uint16_t>(mesh.triangles.ptr[i]));
    }

    geometry->_indices = indiciesushort; //createVsgArray<uint16_t>(reinterpret_cast<uint16_t*>(mesh.triangles.ptr), mesh.triangles.length);
    geometry->_commands = {vsg::DrawIndexed::create(mesh.triangles.length, 1, 0, 0, 0)};

    // add the geometry
    stateGroup->addChild(geometry);

    // write the graph to file
    vsg::vsgReaderWriter io;
    io.writeFile(root.get(), "C:\\Work\\VSG\\meshexport.vsga");

    // we're done so release the arrays before vsg ref_ptr tries to delete them (at the mo they are C# memory)
    for (auto array : inputarrays)
    {
        array->dataRelease();
    }
}

class LeafDataCollection : public vsg::Visitor
{
public:
    vsg::ref_ptr<vsg::Objects> objects;

    LeafDataCollection()
    {
        objects = new vsg::Objects;
    }

    void apply(vsg::Object& object) override
    {
        if (typeid(object) == typeid(vsg::Texture))
        {
            vsg::Texture* texture = static_cast<vsg::Texture*>(&object);
            if (texture->_textureData)
            {
                objects->addChild(texture->_textureData);
            }
        }

        object.traverse(*this);
    }

    void apply(vsg::Geometry& geometry) override
    {
        for (auto& data : geometry._arrays)
        {
            objects->addChild(data);
        }
        if (geometry._indices)
        {
            objects->addChild(geometry._indices);
        }
    }

    void apply(vsg::VertexIndexDraw& vid) override
    {
        for (auto& data : vid._arrays)
        {
            objects->addChild(data);
        }
        if (vid._indices)
        {
            objects->addChild(vid._indices);
        }
    }

    void apply(vsg::BindVertexBuffers& bvb) override
    {
        for (auto& data : bvb.getArrays())
        {
            objects->addChild(data);
        }
    }

    void apply(vsg::BindIndexBuffer& bib) override
    {
        if (bib.getIndices())
        {
            objects->addChild(vsg::ref_ptr<vsg::Data>(bib.getIndices()));
        }
    }

    void apply(vsg::StateGroup& stategroup) override
    {
        for (auto& command : stategroup.getStateCommands())
        {
            command->accept(*this);
        }

        stategroup.traverse(*this);
    }
};

class LeafDataRelease : public vsg::Visitor
{
public:
    LeafDataRelease() {}

    void apply(vsg::Object& object) override
    {
        if (typeid(object) == typeid(vsg::Texture))
        {
            vsg::Texture* texture = static_cast<vsg::Texture*>(&object);
            if (texture->_textureData)
            {
                texture->_textureData->dataRelease();
            }
        }

        object.traverse(*this);
    }

    void apply(vsg::Geometry& geometry) override
    {
        for (auto& data : geometry._arrays)
        {
            data->dataRelease();
        }
        if (geometry._indices)
        {
            geometry._indices->dataRelease();
        }
    }

    void apply(vsg::VertexIndexDraw& vid) override
    {
        for (auto& data : vid._arrays)
        {
            data->dataRelease();
        }
        if (vid._indices)
        {
            vid._indices->dataRelease();
        }
    }

    void apply(vsg::BindVertexBuffers& bvb) override
    {
        for (auto& data : bvb.getArrays())
        {
            data->dataRelease();
        }
    }

    void apply(vsg::BindIndexBuffer& bib) override
    {
        if (bib.getIndices())
        {
            bib.getIndices()->dataRelease();
        }
    }

    void apply(vsg::StateGroup& stategroup) override
    {
        for (auto& command : stategroup.getStateCommands())
        {
            command->accept(*this);
        }

        stategroup.traverse(*this);
    }
};

class GraphBuilder : public vsg::Object
{
public:
    GraphBuilder()
    {
        _root = vsg::MatrixTransform::create();
        pushNodeToStack(_root);
    }

    //
    // Nodes
    //

    void addGroup()
    {
        auto group = vsg::Group::create();
        if (!addChildToHead(group))
        {
            DebugLog("GraphBuilder Warning: Current head is not a group");
        }
        pushNodeToStack(group);
    }

    void addMatrixTrasform(const TransformData& data)
    {
        vsg::mat4 matrix = vsg::mat4(data.matrix.ptr[0], data.matrix.ptr[1], data.matrix.ptr[2], data.matrix.ptr[3],
                                     data.matrix.ptr[4], data.matrix.ptr[5], data.matrix.ptr[6], data.matrix.ptr[7],
                                     data.matrix.ptr[8], data.matrix.ptr[9], data.matrix.ptr[10], data.matrix.ptr[11],
                                     data.matrix.ptr[12], data.matrix.ptr[13], data.matrix.ptr[14], data.matrix.ptr[15]);

        auto transform = vsg::MatrixTransform::create(matrix);

        if (!addChildToHead(transform))
        {
            DebugLog("GraphBuilder Error: Current head is not a group");
        }
        pushNodeToStack(transform);
    }

    void addCullNode(CullData cull)
    {
        auto cullNode = vsg::CullNode::create(vsg::sphere(cull.center, cull.radius), nullptr);
        if (!addChildToHead(cullNode))
        {
            DebugLog("GraphBuilder Error: Current head is not a group");
        }
        pushNodeToStack(cullNode);
    }

    void addCullGroup(CullData cull)
    {
        auto cullGroup = vsg::CullGroup::create(vsg::sphere(cull.center, cull.radius));
        if (!addChildToHead(cullGroup))
        {
            DebugLog("GraphBuilder Error: Current head is not a group");
        }
        pushNodeToStack(cullGroup);
    }

    void addStateGroup()
    {
        auto stategroup = vsg::StateGroup::create();
        if (!addChildToHead(stategroup))
        {
            DebugLog("GraphBuilder Error: Current head is not a group");
        }
        pushNodeToStack(stategroup);
        _activeStateGroup = stategroup;
    }

    void addCommands()
    {
        auto commands = vsg::Commands::create();
        if (!addChildToHead(commands))
        {
            DebugLog("GraphBuilder Error: Current head is not a group");
        }
        pushNodeToStack(commands);
    }

    void addVertexIndexDraw(const MeshData& data)
    {
        vsg::ref_ptr<vsg::Node> geomNode;

        // has a geometry with this ID already been created
        std::string idstr = std::string(data.id);

        if (_geometryCache.find(idstr) != _geometryCache.end())
        {
            geomNode = _geometryCache[idstr];
        }
        else
        {
            auto geometry = vsg::VertexIndexDraw::create();

            // vertex inputs
            auto inputarrays = vsg::DataList{createVsgArray<vsg::vec3>(data.verticies.ptr, data.verticies.length)}; // always have verticies

            if (data.normals.length > 0) inputarrays.push_back(createVsgArray<vsg::vec3>(data.normals.ptr, data.normals.length));
            if (data.uv0.length > 0) inputarrays.push_back(createVsgArray<vsg::vec2>(data.uv0.ptr, data.uv0.length));

            geometry->_arrays = inputarrays;

            // for now convert the int32 array indicies to uint16
            vsg::ref_ptr<vsg::ushortArray> indiciesushort(new vsg::ushortArray(data.triangles.length));
            for (uint32_t i = 0; i < data.triangles.length; i++)
            {
                indiciesushort->set(i, static_cast<uint16_t>(data.triangles.ptr[i]));
            }

            geometry->_indices = indiciesushort; //createVsgArray<uint16_t>(reinterpret_cast<uint16_t*>(mesh.triangles.ptr), mesh.triangles.length);
            geometry->indexCount = static_cast<uint32_t>(indiciesushort->valueCount());
            geometry->instanceCount = 1;

            _geometryCache[idstr] = geometry;
            geomNode = geometry;
        }

        if (!addChildToHead(geomNode))
        {
            DebugLog("GraphBuilder Error: Current head is not a group");
        }

        pushNodeToStack(geomNode);
    }

    void addGeometry(const MeshData& data)
    {
        vsg::ref_ptr<vsg::Node> geomNode;

        // has a geometry with this ID already been created
        std::string idstr = std::string(data.id);

        if (_geometryCache.find(idstr) != _geometryCache.end())
        {
            geomNode = _geometryCache[idstr];
        }
        else
        {
            auto geometry = vsg::Geometry::create();

            // vertex inputs
            auto inputarrays = vsg::DataList{createVsgArray<vsg::vec3>(data.verticies.ptr, data.verticies.length)}; // always have verticies

            if (data.normals.length > 0) inputarrays.push_back(createVsgArray<vsg::vec3>(data.normals.ptr, data.normals.length));
            if (data.uv0.length > 0) inputarrays.push_back(createVsgArray<vsg::vec2>(data.uv0.ptr, data.uv0.length));

            geometry->_arrays = inputarrays;

            // for now convert the int32 array indicies to uint16
            vsg::ref_ptr<vsg::ushortArray> indiciesushort(new vsg::ushortArray(data.triangles.length));
            for (uint32_t i = 0; i < data.triangles.length; i++)
            {
                indiciesushort->set(i, static_cast<uint16_t>(data.triangles.ptr[i]));
            }

            geometry->_indices = indiciesushort; //createVsgArray<uint16_t>(reinterpret_cast<uint16_t*>(mesh.triangles.ptr), mesh.triangles.length);
            geometry->_commands = {vsg::DrawIndexed::create(static_cast<uint32_t>(indiciesushort->valueCount()), 1, 0, 0, 0)};

            _geometryCache[idstr] = geometry;
            geomNode = geometry;
        }

        if (!addChildToHead(geomNode))
        {
            DebugLog("GraphBuilder Error: Current head is not a group");
        }

        pushNodeToStack(geomNode);
    }

    //
    // Meta data
    //

    void addStringValue(std::string name, std::string value)
    {
        getHead()->setValue(name, value.c_str());
    }

    //
    // Commands
    //

    void addBindGraphicsPipelineCommand(const PipelineData& data, bool addToActiveStateGroup)
    {
        std::string idstr = std::string(data.id);
        vsg::ref_ptr<vsg::BindGraphicsPipeline> bindGraphicsPipeline;

        if (_bindGraphicsPipelineCache.find(idstr) != _bindGraphicsPipelineCache.end())
        {
            bindGraphicsPipeline = _bindGraphicsPipelineCache[idstr];
        }
        else
        {
            vsg::ref_ptr<vsg::GraphicsPipelineBuilder> pipelinebuilder = vsg::GraphicsPipelineBuilder::create();
            vsg::ref_ptr<vsg::GraphicsPipelineBuilder::Traits> traits = vsg::GraphicsPipelineBuilder::Traits::create();

            // vertex input
            vsg::GraphicsPipelineBuilder::Traits::InputAttributeDescriptions inputAttributes = {{{0, VK_FORMAT_R32G32B32_SFLOAT}}};
            uint32_t inputshaderatts = VERTEX;

            if (data.hasNormals)
            {
                inputAttributes.push_back({{1, VK_FORMAT_R32G32B32_SFLOAT}});
                inputshaderatts |= NORMAL;
            }
            if (data.hasTangents)
            {
                inputAttributes.push_back({{2, VK_FORMAT_R32G32B32_SFLOAT}});
                inputshaderatts |= TANGENT;
            }
            if (data.hasColors)
            {
                inputAttributes.push_back({{3, VK_FORMAT_R32G32B32A32_SFLOAT}});
                inputshaderatts |= COLOR;
            }
            if (data.uvChannelCount > 0) // uv set 0
            {
                inputAttributes.push_back({{4, VK_FORMAT_R32G32_SFLOAT}});
                inputshaderatts |= TEXCOORD0;
            }

            traits->vertexAttributeDescriptions[VK_VERTEX_INPUT_RATE_VERTEX] = inputAttributes;

            // descriptor sets layout
            vsg::GraphicsPipelineBuilder::Traits::DescriptorBindingSet bindingSet;
            uint32_t shaderMode = LIGHTING;
            if (data.fragmentImageSamplerCount > 0) shaderMode |= DIFFUSE_MAP;

            vsg::GraphicsPipelineBuilder::Traits::DescriptorBindngs vertexBindings;
            for (uint32_t i = 0; i < data.vertexImageSamplerCount; i++)
            {
                vertexBindings.push_back({i, VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER});
            }
            bindingSet[VK_SHADER_STAGE_VERTEX_BIT] = vertexBindings;

            vsg::GraphicsPipelineBuilder::Traits::DescriptorBindngs fragmentBindings;
            for (uint32_t i = 0; i < data.fragmentImageSamplerCount; i++)
            {
                fragmentBindings.push_back({i, VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER});
            }
            bindingSet[VK_SHADER_STAGE_FRAGMENT_BIT] = fragmentBindings;

            traits->descriptorLayouts = {bindingSet};

            // shaders
            vsg::ShaderModules shaders;

            uint32_t shaderkey = getShaderKey(shaderMode, inputshaderatts);

            if (_shaderModulesCache.find(shaderkey) != _shaderModulesCache.end())
            {
                shaders = _shaderModulesCache[shaderkey];
            }
            else
            {
                shaders = {
                    vsg::ShaderModule::create(VK_SHADER_STAGE_VERTEX_BIT, "main", createFbxVertexSource(shaderMode, inputshaderatts)),
                    vsg::ShaderModule::create(VK_SHADER_STAGE_FRAGMENT_BIT, "main", createFbxFragmentSource(shaderMode, inputshaderatts))};

                ShaderCompiler shaderCompiler;
                if (!shaderCompiler.compile(shaders))
                {
                    DebugLog("GraphBuilder Error: Failed to compile shaders.");
                    return;
                }

                _shaderModulesCache[shaderkey] = shaders;
            }

            traits->shaderModules = shaders;

            // topology
            traits->primitiveTopology = VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;

            // alpha blending
            if (data.useAlpha == 1)
            {
                vsg::ColorBlendState::ColorBlendAttachments colorBlendAttachments;
                VkPipelineColorBlendAttachmentState colorBlendAttachment = {};
                colorBlendAttachment.blendEnable = VK_TRUE;
                colorBlendAttachment.colorWriteMask = VK_COLOR_COMPONENT_R_BIT |
                                                      VK_COLOR_COMPONENT_G_BIT |
                                                      VK_COLOR_COMPONENT_B_BIT |
                                                      VK_COLOR_COMPONENT_A_BIT;

                colorBlendAttachment.srcColorBlendFactor = VK_BLEND_FACTOR_SRC_ALPHA;
                colorBlendAttachment.dstColorBlendFactor = VK_BLEND_FACTOR_ONE_MINUS_SRC_ALPHA;
                colorBlendAttachment.colorBlendOp = VK_BLEND_OP_ADD;
                colorBlendAttachment.srcAlphaBlendFactor = VK_BLEND_FACTOR_ONE;
                colorBlendAttachment.dstAlphaBlendFactor = VK_BLEND_FACTOR_ZERO;
                colorBlendAttachment.alphaBlendOp = VK_BLEND_OP_ADD;

                traits->colorBlendAttachments.push_back(colorBlendAttachment);
            }

            // create our graphics pipeline
            pipelinebuilder->build(traits);

            bindGraphicsPipeline = vsg::BindGraphicsPipeline::create(pipelinebuilder->getGraphicsPipeline());
            _bindGraphicsPipelineCache[idstr] = bindGraphicsPipeline;
        }

        if (addToActiveStateGroup)
        {
            if (!addStateCommandToActiveStateGroup(bindGraphicsPipeline))
            {
                DebugLog("GraphBuilder Error: No active StateGroup");
            }
        }
        else
        {
            if (!addCommandToHead(bindGraphicsPipeline))
            {
                DebugLog("GraphBuilder Error: Current head is not a Commands node");
            }
        }

        _activeGraphicsPipeline = bindGraphicsPipeline->getPipeline();
    }

    void addBindIndexBufferCommand(unity2vsg::IndexBufferData data)
    {
        std::string idstr = std::string(data.id);

        vsg::ref_ptr<vsg::Command> cmd;

        if (_bindIndexBufferCache.find(idstr) != _bindIndexBufferCache.end())
        {
            cmd = _bindIndexBufferCache[idstr];
        }
        else
        {
            // for now convert the int32 array indicies to uint16
            vsg::ref_ptr<vsg::ushortArray> indiciesushort(new vsg::ushortArray(data.triangles.length));
            for (uint32_t i = 0; i < data.triangles.length; i++)
            {
                indiciesushort->set(i, static_cast<uint16_t>(data.triangles.ptr[i]));
            }
            cmd = vsg::BindIndexBuffer::create(indiciesushort);
            _bindIndexBufferCache[idstr] = cmd;
        }
        addCommandToHead(cmd);
    }

    void addBindVertexBuffersCommand(unity2vsg::VertexBuffersData data)
    {
        std::string idstr = std::string(data.id);

        vsg::ref_ptr<vsg::Command> cmd;

        if (_bindVertexBuffersCache.find(idstr) != _bindVertexBuffersCache.end())
        {
            cmd = _bindVertexBuffersCache[idstr];
        }
        else
        {
            auto inputarrays = vsg::DataList{createVsgArray<vsg::vec3>(data.verticies.ptr, data.verticies.length)}; // always have verticies

            if (data.normals.length > 0) inputarrays.push_back(createVsgArray<vsg::vec3>(data.normals.ptr, data.normals.length));
            if (data.uv0.length > 0) inputarrays.push_back(createVsgArray<vsg::vec2>(data.uv0.ptr, data.uv0.length));

            cmd = vsg::BindVertexBuffers::create(0, inputarrays);
            _bindVertexBuffersCache[idstr] = cmd;
        }

        addCommandToHead(cmd);
    }

    void addDrawIndexedCommand(unity2vsg::DrawIndexedData data)
    {
        std::string idstr = std::string(data.id);

        vsg::ref_ptr<vsg::Command> cmd;

        if (_drawIndexedCache.find(idstr) != _drawIndexedCache.end())
        {
            cmd = _drawIndexedCache[idstr];
        }
        else
        {
            cmd = vsg::DrawIndexed::create(data.indexCount, data.instanceCount, data.firstIndex, data.vertexOffset, data.firstInstance);
            _drawIndexedCache[idstr] = cmd;
        }
        addCommandToHead(cmd);
    }

    void createBindDescriptorSetCommand(bool addToStateGroup)
    {
        if (addToStateGroup && !_activeStateGroup.valid())
        {
            DebugLog("GraphBuilder Error: Can't bind descriptors no StateGroup active.");
            return;
        }

        if (!_activeGraphicsPipeline.valid())
        {
            DebugLog("GraphBuilder Error: Can't bind descriptors until a graphicspipeline has been added.");
            return;
        }
        if (_descriptors.empty())
        {
            DebugLog("GraphBuilder Error: No descriptors to bind.");
            return;
        }

        // create an id combining all the object ids for the current list of descriptors, then use it to see if a matching binddescriptorset exists in the cache
        std::string fullid = "";
        for (std::vector<std::string>::const_iterator p = _descriptorObjectIds.begin(); p != _descriptorObjectIds.end(); p++)
        {
            fullid += *p;
            if (p != _descriptorObjectIds.end() - 1)
                fullid += "-";
        }

        vsg::ref_ptr<vsg::BindDescriptorSet> bindDescriptorSet;

        if (_bindDescriptorSetCache.find(fullid) != _bindDescriptorSetCache.end())
        {
            bindDescriptorSet = _bindDescriptorSetCache[fullid];
        }
        else
        {
            auto descriptorSet = vsg::DescriptorSet::create(_activeGraphicsPipeline->getPipelineLayout()->getDescriptorSetLayouts(), _descriptors);
            bindDescriptorSet = vsg::BindDescriptorSet::create(VK_PIPELINE_BIND_POINT_GRAPHICS, _activeGraphicsPipeline->getPipelineLayout(), 0, descriptorSet);
            _bindDescriptorSetCache[fullid] = bindDescriptorSet;
        }

        if (addToStateGroup)
        {
            if (!addStateCommandToActiveStateGroup(bindDescriptorSet))
            {
                DebugLog("GraphBuilder Error: No Active StateGroup");
            }
        }
        else
        {
            if (!addCommandToHead(bindDescriptorSet))
            {
                DebugLog("GraphBuilder Error: Current head is not a Commands node");
            }
        }

        _descriptors.clear();
        _descriptorObjectIds.clear();
    }

    //
    // Descriptors
    //

    void addTexture(const TextureData& data)
    {
        vsg::ref_ptr<vsg::Texture> texture;

        std::string idstr = std::string(data.id);

        // has a texture with this ID already been created
        if (_textureCache.find(idstr) != _textureCache.end())
        {
            texture = _textureCache[idstr];
        }
        else
        {
            texture = vsg::Texture::create();

            vsg::ref_ptr<vsg::Data> texdata;
            VkFormat format = vkFormatForTexFormat(data.format);

            if (data.depth == 1)
            {
                switch (format)
                {
                case VK_FORMAT_R8_UNORM:
                {
                    texdata = vsg::ref_ptr<vsg::Data>(new vsg::ubyteArray2D(data.width, data.height, data.pixels.ptr));
                    break;
                }
                case VK_FORMAT_R8G8_UNORM:
                {
                    texdata = vsg::ref_ptr<vsg::Data>(new vsg::ubvec2Array2D(data.width, data.height, reinterpret_cast<vsg::ubvec2*>(data.pixels.ptr)));
                    break;
                }
                case VK_FORMAT_R8G8B8A8_UNORM:
                {
                    texdata = vsg::ref_ptr<vsg::Data>(new vsg::ubvec4Array2D(data.width, data.height, reinterpret_cast<vsg::ubvec4*>(data.pixels.ptr)));
                    break;
                }
                default:
                {
                    texdata = vsg::ref_ptr<vsg::Data>(new vsg::ubyteArray2D(data.width, data.height, data.pixels.ptr));
                    break;
                }
                }
            }
            else if (data.depth > 1)
            {
                switch (format)
                {
                case VK_FORMAT_R8_UNORM:
                {
                    texdata = vsg::ref_ptr<vsg::Data>(new vsg::ubyteArray3D(data.width, data.height, data.depth, data.pixels.ptr));
                    break;
                }
                case VK_FORMAT_R8G8_UNORM:
                {
                    texdata = vsg::ref_ptr<vsg::Data>(new vsg::ubvec2Array3D(data.width, data.height, data.depth, reinterpret_cast<vsg::ubvec2*>(data.pixels.ptr)));
                    break;
                }
                case VK_FORMAT_R8G8B8A8_UNORM:
                {
                    texdata = vsg::ref_ptr<vsg::Data>(new vsg::ubvec4Array3D(data.width, data.height, data.depth, reinterpret_cast<vsg::ubvec4*>(data.pixels.ptr)));
                    break;
                }
                default:
                {
                    texdata = vsg::ref_ptr<vsg::Data>(new vsg::ubyteArray3D(data.width, data.height, data.depth, data.pixels.ptr));
                    break;
                }
                }
            }

            texdata->setFormat(format);

            texture->_textureData = texdata;
            texture->_samplerInfo = vkSamplerCreateInfoForTextureData(data);

            _textureCache[idstr] = texture;
        }

        texture->_dstBinding = data.channel;

        _descriptors.push_back(texture);
        _descriptorObjectIds.push_back(std::string(data.id));
    }

    //
    // Helpers
    //

    vsg::Node* getHead()
    {
        if (_nodeStack.size() == 0) return nullptr;
        return _nodeStack[_nodeStack.size() - 1];
    }

    vsg::Group* getHeadAsGroup()
    {
        if (_nodeStack.size() == 0) return nullptr;
        return dynamic_cast<vsg::Group*>(_nodeStack[_nodeStack.size() - 1].get());
    }

    vsg::StateGroup* getHeadAsStateGroup()
    {
        if (_nodeStack.size() == 0) return nullptr;
        return dynamic_cast<vsg::StateGroup*>(_nodeStack[_nodeStack.size() - 1].get());
    }

    vsg::Commands* getHeadAsCommandsNode()
    {
        if (_nodeStack.size() == 0) return nullptr;
        return dynamic_cast<vsg::Commands*>(_nodeStack[_nodeStack.size() - 1].get());
    }

    bool addChildToHead(vsg::ref_ptr<vsg::Node> node)
    {
        vsg::Group* headGroup = getHeadAsGroup();
        if (headGroup != nullptr)
        {
            headGroup->addChild(node);
            return true;
        }
        return false;
    }

    bool addCommandToHead(vsg::ref_ptr<vsg::Command> command)
    {
        vsg::Commands* headCommands = getHeadAsCommandsNode();
        if (headCommands != nullptr)
        {
            headCommands->addChild(command);
            return true;
        }
        return false;
    }

    bool addStateCommandToActiveStateGroup(vsg::ref_ptr<vsg::StateCommand> command)
    {
        if (_activeStateGroup != nullptr)
        {
            _activeStateGroup->add(command);
            return true;
        }
        return false;
    }

    void pushNodeToStack(vsg::ref_ptr<vsg::Node> node)
    {
        _nodeStack.push_back(node);
    }

    void popNodeFromStack()
    {
        _nodeStack.pop_back();
    }

    PipelineData createPipelineDataFromMeshData(const MeshData& mesh)
    {
        PipelineData data;
        data.hasNormals = mesh.normals.length > 0;
        data.hasColors = mesh.colors.length > 0;
        data.uvChannelCount = mesh.uv0.length > 0 ? 1 : 0;
        return data;
    }

    uint32_t getShaderKey(uint32_t x, uint32_t y)
    {
        return (x << 16) | (y && 0xFFFF);
    }

    void writeFile(std::string fileName)
    {
        LeafDataCollection leafDataCollection;
        _root->accept(leafDataCollection);
        _root->setObject("batch", leafDataCollection.objects);

        vsg::vsgReaderWriter io;
        io.writeFile(_root.get(), fileName);
    }

    void releaseObjects()
    {
        LeafDataRelease releaser;
        _root->accept(releaser);
    }

    vsg::ref_ptr<vsg::MatrixTransform> _root;

    // the stack of nodes added, last node is the current head being acted on
    std::vector<vsg::ref_ptr<vsg::Node>> _nodeStack;

    // the current active stategroup
    vsg::ref_ptr<vsg::StateGroup> _activeStateGroup;

    // the current active graphics pipelines
    vsg::ref_ptr<vsg::GraphicsPipeline> _activeGraphicsPipeline;

    // the current set of descriptors being built
    vsg::Descriptors _descriptors;

    // the unique ids of the of the descriptos list being built
    std::vector<std::string> _descriptorObjectIds;

    // map of exisitng Geometryies/VertexIndexedDraws to the MeshData ID they represent
    std::map<std::string, vsg::ref_ptr<vsg::Node>> _geometryCache;

    std::map<std::string, vsg::ref_ptr<vsg::Command>> _bindVertexBuffersCache;
    std::map<std::string, vsg::ref_ptr<vsg::Command>> _bindIndexBufferCache;
    std::map<std::string, vsg::ref_ptr<vsg::Command>> _drawIndexedCache;

    // map of shader modules to the masks used to create them
    std::map<uint32_t, vsg::ShaderModules> _shaderModulesCache;

    // map of textures to the TextureData ID they represent
    std::map<std::string, vsg::ref_ptr<vsg::Texture>> _textureCache;

    // map of bind descriptor set to IDs
    std::map<std::string, vsg::ref_ptr<vsg::BindDescriptorSet>> _bindDescriptorSetCache;

    // map of bind graphics piplelines to IDs
    std::map<std::string, vsg::ref_ptr<vsg::BindGraphicsPipeline>> _bindGraphicsPipelineCache;

    std::string _saveFileName;
};

vsg::ref_ptr<GraphBuilder> _builder;

void unity2vsg_BeginExport()
{
    if (_builder.valid())
    {
        DebugLog("GraphBuilder Error: Export already in progress.");
        return;
    }
    _builder = vsg::ref_ptr<GraphBuilder>(new GraphBuilder());
}

void unity2vsg_EndExport(const char* saveFileName)
{
    _builder->writeFile(std::string(saveFileName));

    _builder->releaseObjects();
    _builder = nullptr;
}

void unity2vsg_AddGroupNode()
{
    _builder->addGroup();
}

void unity2vsg_AddTransformNode(unity2vsg::TransformData transform)
{
    _builder->addMatrixTrasform(transform);
}

void unity2vsg_AddCullNode(unity2vsg::CullData cull)
{
    _builder->addCullNode(cull);
}

void unity2vsg_AddCullGroupNode(unity2vsg::CullData cull)
{
    _builder->addCullGroup(cull);
}

void unity2vsg_AddStateGroupNode()
{
    _builder->addStateGroup();
}

void unity2vsg_AddCommandsNode()
{
    _builder->addCommands();
}

void unity2vsg_AddVertexIndexDrawNode(unity2vsg::MeshData mesh)
{
    _builder->addVertexIndexDraw(mesh);
}

void unity2vsg_AddGeometryNode(unity2vsg::MeshData mesh)
{
    _builder->addGeometry(mesh);
}

//
// Meta data
//

void unity2vsg_AddStringValue(const char* name, const char* value)
{
    _builder->addStringValue(std::string(name), std::string(value));
}

//
// Commands
//

void unity2vsg_AddBindGraphicsPipelineCommand(unity2vsg::PipelineData pipeline, uint32_t addToStateGroup)
{
    _builder->addBindGraphicsPipelineCommand(pipeline, addToStateGroup == 1);
}

void unity2vsg_AddBindIndexBufferCommand(unity2vsg::IndexBufferData data)
{
    _builder->addBindIndexBufferCommand(data);
}

void unity2vsg_AddBindVertexBuffersCommand(unity2vsg::VertexBuffersData data)
{
    _builder->addBindVertexBuffersCommand(data);
}

void unity2vsg_AddDrawIndexedCommand(unity2vsg::DrawIndexedData data)
{
    _builder->addDrawIndexedCommand(data);
}

void unity2vsg_CreateBindDescriptorSetCommand(uint32_t addToStateGroup)
{
    _builder->createBindDescriptorSetCommand(addToStateGroup == 1);
}

//
// Desccriptos
//

void unity2vsg_AddTextureDescriptor(unity2vsg::TextureData texture)
{
    _builder->addTexture(texture);
}

void unity2vsg_EndNode()
{
    _builder->popNodeFromStack();
}

void unity2vsg_LaunchViewer(const char* filename, uint32_t useCamData, unity2vsg::CameraData camdata)
{
    try
    {
        vsg::vsgReaderWriter io;
        vsg::ref_ptr<vsg::Node> vsg_scene = io.read<vsg::Node>(filename);

        std::stringstream ss;
        ss << "cam pos: " << camdata.position.x << ", " << camdata.position.y << ", " << camdata.position.z << std::endl;
        ss << "cam look: " << camdata.lookAt.x << ", " << camdata.lookAt.y << ", " << camdata.lookAt.z << std::endl;
        ss << "cam up: " << camdata.upDir.x << ", " << camdata.upDir.y << ", " << camdata.upDir.z << std::endl;
        //DebugLog(ss.str());

        if (!vsg_scene.valid()) return;

        auto windowTraits = vsg::Window::Traits::create();
        windowTraits->windowTitle = "vsg export - " + std::string(filename);
        windowTraits->width = 800;
        windowTraits->height = 600;

        // create the viewer and assign window(s) to it
        auto viewer = vsg::Viewer::create();

        vsg::ref_ptr<vsg::Window> window(vsg::Window::create(windowTraits));
        if (!window)
        {
            std::cout << "Could not create windows." << std::endl;
            _builder->releaseObjects();
            return;
        }

        viewer->addWindow(window);

        // compute the bounds of the scene graph to help position camera
        vsg::ComputeBounds computeBounds;
        vsg_scene->accept(computeBounds);
        vsg::dvec3 centre = (computeBounds.bounds.min + computeBounds.bounds.max) * 0.5;
        double radius = vsg::length(computeBounds.bounds.max - computeBounds.bounds.min) * 0.6;
        double nearFarRatio = 0.0001;

        // set up the camera
        vsg::ref_ptr<vsg::Perspective> perspective;
        vsg::ref_ptr<vsg::LookAt> lookAt;

        if (useCamData == 1)
        {
            perspective = vsg::ref_ptr<vsg::Perspective>(new vsg::Perspective(camdata.fov, static_cast<double>(window->extent2D().width) / static_cast<double>(window->extent2D().height), camdata.nearZ, camdata.farZ));
            lookAt = vsg::ref_ptr<vsg::LookAt>(new vsg::LookAt(vsg::dvec3(camdata.position.x, camdata.position.y, camdata.position.z), vsg::dvec3(camdata.lookAt.x, camdata.lookAt.y, camdata.lookAt.z), vsg::dvec3(camdata.upDir.x, camdata.upDir.y, camdata.upDir.z)));
        }
        else
        {
            perspective = vsg::ref_ptr<vsg::Perspective>(new vsg::Perspective(30.0, static_cast<double>(window->extent2D().width) / static_cast<double>(window->extent2D().height), nearFarRatio * radius, radius * 4.5));
            lookAt = vsg::ref_ptr<vsg::LookAt>(new vsg::LookAt(centre + vsg::dvec3(0.0, 0.0, -radius * 3.5), centre, vsg::dvec3(0.0, 1.0, 0.0)));
        }

        vsg::ref_ptr<vsg::Camera> camera(new vsg::Camera(perspective, lookAt, vsg::ViewportState::create(window->extent2D())));

        // add a GraphicsStage tp the Window to do dispatch of the command graph to the commnad buffer(s)
        window->addStage(vsg::GraphicsStage::create(vsg_scene, camera));

        // compile the Vulkan objects
        viewer->compile();

        // add close handler to respond the close window button and pressing esape
        viewer->addEventHandler(vsg::CloseHandler::create(viewer));
        viewer->addEventHandler(vsg::Trackball::create(camera));

        // rendering main loop
        while (viewer->advanceToNextFrame())
        {
            // pass any events into EventHandlers assigned to the Viewer
            viewer->handleEvents();

            viewer->populateNextFrame();

            viewer->submitNextFrame();
        }
    }
    catch (...)
    {
        DebugLog("An exception occured during vsg preview.");
        return;
    }
}
