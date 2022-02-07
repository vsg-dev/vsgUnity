/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth
Copyright(c) 2022 Christian Schott (InstruNEXT GmbH)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

#include <unity2vsg/unity2vsg.h>

#include <unity2vsg/DebugLog.h>
#include <unity2vsg/GraphicsPipelineBuilder.h>
#include <unity2vsg/ShaderUtils.h>
#include <unity2vsg/SkyboxUtils.h>

#include <vsg/all.h>
#include <vsg/core/Objects.h>

using namespace unity2vsg;

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
        vsg::dmat4 matrix(data.matrix.data[0], data.matrix.data[1], data.matrix.data[2], data.matrix.data[3],
                          data.matrix.data[4], data.matrix.data[5], data.matrix.data[6], data.matrix.data[7],
                          data.matrix.data[8], data.matrix.data[9], data.matrix.data[10], data.matrix.data[11],
                          data.matrix.data[12], data.matrix.data[13], data.matrix.data[14], data.matrix.data[15]);

        auto transform = vsg::MatrixTransform::create(matrix);

        if (!addChildToHead(transform))
        {
            DebugLog("GraphBuilder Error: Current head is not a group");
        }
        pushNodeToStack(transform);
    }

    void addCullNode(CullData cull)
    {
        vsg::dvec3 center(cull.center.data[0], cull.center.data[1], cull.center.data[2]);
        auto cullNode = vsg::CullNode::create(vsg::dsphere(center, cull.radius), nullptr);
        if (!addChildToHead(cullNode))
        {
            DebugLog("GraphBuilder Error: Current head is not a group");
        }
        pushNodeToStack(cullNode);
    }

    void addCullGroup(CullData cull)
    {
        vsg::dvec3 center(cull.center.data[0], cull.center.data[1], cull.center.data[2]);
        auto cullGroup = vsg::CullGroup::create(vsg::dsphere(center, cull.radius));
        if (!addChildToHead(cullGroup))
        {
            DebugLog("GraphBuilder Error: Current head is not a group");
        }
        pushNodeToStack(cullGroup);
    }

    void addLOD(CullData cull)
    {
        vsg::vec3 center = vsg::vec3(cull.center.data[0], cull.center.data[1], cull.center.data[2]);
        auto lod = vsg::LOD::create();
        lod->bound.set(center, cull.radius);
        if (!addChildToHead(lod))
        {
            DebugLog("GraphBuilder Error: Current head is not a group");
        }
        pushNodeToStack(lod);
    }

    void addLODChild(LODChildData lodChild)
    {
        auto group = vsg::Group::create();
        if (!addLODChildToHead(group, lodChild))
        {
            DebugLog("GraphBuilder Warning: Current head is not an LOD");
        }
        pushNodeToStack(group);
    }

    void addLightNode(LightData lightData)
    {
        auto light = [&lightData] () -> vsg::ref_ptr<vsg::Light>
        {
            switch(lightData.type) {
            case 0:
            {
                auto point = vsg::PointLight::create();
                point->position = vsg::dvec3(lightData.position.data[0], lightData.position.data[1], lightData.position.data[2]);
                return point;
            }
            case 1:
            {
                auto directional = vsg::DirectionalLight::create();
                directional->direction = vsg::dvec3(lightData.direction.data[0], lightData.direction.data[1], lightData.direction.data[2]);
                return directional;
            }
            case 2:
            {
                auto spot = vsg::SpotLight::create();
                spot->position = vsg::dvec3(lightData.position.data[0], lightData.position.data[1], lightData.position.data[2]);
                spot->direction = vsg::dvec3(lightData.direction.data[0], lightData.direction.data[1], lightData.direction.data[2]);
                spot->innerAngle = lightData.innerAngle;
                spot->outerAngle = lightData.outerAngle;
                return spot;
            }
            case 3:
                return vsg::AmbientLight::create();
            default:
                return {};
            }
        } ();

        if (light) {
            light->color = vsg::dvec3(lightData.color.data[0], lightData.color.data[1], lightData.color.data[2]);
            light->intensity = lightData.intensity;
            if (!addChildToHead(light))
            {
                DebugLog("GraphBuilder Error: Current head is not a group");
            }
            pushNodeToStack(light);
        }
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

    void addVertexIndexDraw(const VertexIndexDrawData& data)
    {
        vsg::ref_ptr<vsg::Node> geomNode;

        if (_vertexIndexDrawCache.find(data.id) != _vertexIndexDrawCache.end())
        {
            geomNode = _vertexIndexDrawCache[data.id];
        }
        else
        {
            auto geometry = vsg::VertexIndexDraw::create();

            // vertex inputs
            auto inputarrays = vsg::DataList{createExternalVsgArray<vsg::vec3>(data.verticies.data, data.verticies.length)}; // always have verticies
            if (data.normals.length > 0) inputarrays.push_back(createExternalVsgArray<vsg::vec3>(data.normals.data, data.normals.length));
            if (data.tangents.length > 0) inputarrays.push_back(createExternalVsgArray<vsg::vec4>(data.tangents.data, data.tangents.length));
            if (data.colors.length > 0) inputarrays.push_back(createExternalVsgArray<vsg::vec4>(data.colors.data, data.colors.length));
            if (data.uv0.length > 0) inputarrays.push_back(createExternalVsgArray<vsg::vec2>(data.uv0.data, data.uv0.length));
            if (data.uv1.length > 0) inputarrays.push_back(createExternalVsgArray<vsg::vec2>(data.uv1.data, data.uv1.length));
            geometry->assignArrays(inputarrays);

            if (data.use32BitIndicies == 0)
            {
                // for now convert the int32 array indicies to uint16
                vsg::ref_ptr<vsg::ushortArray> indiciesushort(new vsg::ushortArray(data.triangles.length));
                for (int32_t i = 0; i < data.triangles.length; i++)
                {
                    indiciesushort->set(i, static_cast<uint16_t>(data.triangles.data[i]));
                }

                geometry->assignIndices(indiciesushort);
            }
            else
            {
                vsg::ref_ptr<vsg::uintArray> indiciesuint(new vsg::uintArray(data.triangles.length));
                for (int32_t i = 0; i < data.triangles.length; i++)
                {
                    indiciesuint->set(i, static_cast<uint32_t>(data.triangles.data[i]));
                }

                geometry->assignIndices(indiciesuint);
            }

            geometry->indexCount = data.triangles.length;
            geometry->instanceCount = 1;

            _vertexIndexDrawCache[data.id] = geometry;
            geomNode = geometry;
        }

        if (!addChildToHead(geomNode))
        {
            DebugLog("GraphBuilder Error: Current head is not a group");
        }

        pushNodeToStack(geomNode);
    }

    void addSkybox(const DescriptorImageData &skyboxTexture)
    {
        _root->addChild(createSkybox(createTexture(skyboxTexture)));
    }

    //
    // Meta data
    //

    void addStringValue(std::string name, std::string value)
    {
        getHead()->setValue(name, value.c_str());
    }

    void addFloatArray(std::string name, const FloatArray &array)
    {
        getHead()->setValue(name, createExternalVsgArray<float>(array.data, array.length));
    }

    //
    // Commands
    //

    vsg::ref_ptr<vsg::ShaderModule> getOrCreateShaderModule(VkShaderStageFlagBits stage, std::string shaderSourceFile, uint32_t inputAtts, uint32_t shaderMode, std::string customDefStr)
    {
        auto split = [](const std::string& str, const char& seperator) {
            std::vector<std::string> elements;

            std::string::size_type prev_pos = 0, pos = 0;

            while ((pos = str.find(seperator, pos)) != std::string::npos)
            {
                auto substring = str.substr(prev_pos, pos - prev_pos);
                elements.push_back(substring);
                prev_pos = ++pos;
            }

            elements.push_back(str.substr(prev_pos, pos - prev_pos));

            return elements;
        };

        std::string shaderkey = std::to_string((int)stage) + "," + shaderSourceFile + "," + std::to_string(inputAtts) + "," + customDefStr;
        std::vector<std::string> customdefs = customDefStr.empty() ? std::vector<std::string>() : split(customDefStr, ',');

        vsg::ref_ptr<vsg::ShaderModule> shaderModule;

        if (_shaderModulesCache.find(shaderkey) != _shaderModulesCache.end())
        {
            shaderModule = _shaderModulesCache[shaderkey];
        }
        else
        {
            if (!shaderSourceFile.empty())
            {
                shaderModule = vsg::ShaderModule::create(readGLSLShader(shaderSourceFile, shaderMode, inputAtts, customdefs));
            }
            else
            {
                if (stage == VK_SHADER_STAGE_VERTEX_BIT)
                {
                    shaderModule = vsg::ShaderModule::create(createFbxVertexSource(shaderMode, inputAtts, customdefs));
                }
                else
                {
                    shaderModule = vsg::ShaderModule::create(createFbxFragmentSource(shaderMode, inputAtts, customdefs));
                }
                _shaderModulesCache[shaderkey] = shaderModule;
            }
        }

        return shaderModule;
    }

    vsg::ref_ptr<vsg::ShaderStage> createShaderStage(VkShaderStageFlagBits stage, vsg::ref_ptr<vsg::ShaderModule> shaderModule, UIntArray specializationConstants)
    {
        auto shaderStage = vsg::ShaderStage::create(stage, "main", shaderModule);

        for(int i = 0; i<specializationConstants.length; ++i)
        {
            shaderStage->specializationConstants[static_cast<uint32_t>(i)] = vsg::uintValue::create(specializationConstants.data[i]);
        }

        return shaderStage;
    }

    bool addBindGraphicsPipelineCommand(const PipelineData& data, bool addToActiveStateGroup)
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
                inputAttributes.push_back({{2, VK_FORMAT_R32G32B32A32_SFLOAT}});
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
            if (data.uvChannelCount > 1) // uv set 1
            {
                inputAttributes.push_back({{5, VK_FORMAT_R32G32_SFLOAT}});
                inputshaderatts |= TEXCOORD1;
            }

            traits->vertexAttributeDescriptions[VK_VERTEX_INPUT_RATE_VERTEX] = inputAttributes;

            // descriptor sets layout
            vsg::GraphicsPipelineBuilder::Traits::DescriptorBindingSet bindingSet;
            uint32_t shaderMode = 0;

            for (int32_t i = 0; i < data.descriptorBindings.length; i++)
            {
                VkDescriptorSetLayoutBinding dslb = data.descriptorBindings.data[i];
                vsg::GraphicsPipelineBuilder::Traits::DescriptorBinding binding = {dslb.binding, dslb.descriptorType, dslb.descriptorCount};
                bindingSet[dslb.stageFlags].push_back(binding);
            }

            traits->descriptorLayouts = {bindingSet};

            // setup shaders
            vsg::ShaderStages shaders;

            for (int i = 0; i < data.shaderStages.stagesCount; i++)
            {
                ShaderStageData& shaderStageData = data.shaderStages.stages[i];
                std::string customDefs = std::string(shaderStageData.customDefines);

                if ((shaderStageData.stages & VK_SHADER_STAGE_VERTEX_BIT) == VK_SHADER_STAGE_VERTEX_BIT)
                {
                    std::string vertDefines = customDefs + ", VSG_VERTEX_CODE";
                    auto vertShaderModule = getOrCreateShaderModule(VK_SHADER_STAGE_VERTEX_BIT, std::string(shaderStageData.source), inputshaderatts, shaderMode, vertDefines);
                    shaders.push_back(createShaderStage(VK_SHADER_STAGE_VERTEX_BIT, vertShaderModule, shaderStageData.specializationData));
                }
                if ((shaderStageData.stages & VK_SHADER_STAGE_FRAGMENT_BIT) == VK_SHADER_STAGE_FRAGMENT_BIT)
                {
                    std::string fragDefines = customDefs + ", VSG_FRAGMENT_CODE";
                    auto fragShaderModule = getOrCreateShaderModule(VK_SHADER_STAGE_FRAGMENT_BIT, std::string(shaderStageData.source), inputshaderatts, shaderMode, fragDefines);
                    shaders.push_back(createShaderStage(VK_SHADER_STAGE_FRAGMENT_BIT, fragShaderModule, shaderStageData.specializationData));
                }
            }

            traits->shaderStages = shaders;

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
                return false;
            }
        }
        else
        {
            if (!addCommandToHead(bindGraphicsPipeline))
            {
                DebugLog("GraphBuilder Error: Current head is not a Commands node");
                return false;
            }
        }

        _activeGraphicsPipeline = bindGraphicsPipeline->pipeline;
        return true;
    }

    void addBindIndexBufferCommand(unity2vsg::IndexBufferData data)
    {
        vsg::ref_ptr<vsg::Command> cmd;

        if (_bindIndexBufferCache.find(data.id) != _bindIndexBufferCache.end())
        {
            cmd = _bindIndexBufferCache[data.id];
        }
        else
        {
            if (data.use32BitIndicies == 0)
            {
                // for now convert the int32 array indicies to uint16
                vsg::ref_ptr<vsg::ushortArray> indiciesushort(new vsg::ushortArray(data.triangles.length));
                for (int32_t i = 0; i < data.triangles.length; i++)
                {
                    indiciesushort->set(i, static_cast<uint16_t>(data.triangles.data[i]));
                }
                cmd = vsg::BindIndexBuffer::create(indiciesushort);
            }
            else
            {
                vsg::ref_ptr<vsg::uintArray> indiciesuint(new vsg::uintArray(data.triangles.length));
                for (int32_t i = 0; i < data.triangles.length; i++)
                {
                    indiciesuint->set(i, static_cast<uint32_t>(data.triangles.data[i]));
                }
                cmd = vsg::BindIndexBuffer::create(indiciesuint);
            }
            _bindIndexBufferCache[data.id] = cmd;
        }
        addCommandToHead(cmd);
    }

    void addBindVertexBuffersCommand(unity2vsg::VertexBuffersData data)
    {
        vsg::ref_ptr<vsg::Command> cmd;

        if (_bindVertexBuffersCache.find(data.id) != _bindVertexBuffersCache.end())
        {
            cmd = _bindVertexBuffersCache[data.id];
        }
        else
        {
            auto inputarrays = vsg::DataList{createExternalVsgArray<vsg::vec3>(data.verticies.data, data.verticies.length)}; // always have verticies

            if (data.normals.length > 0) inputarrays.push_back(createExternalVsgArray<vsg::vec3>(data.normals.data, data.normals.length));
            if (data.tangents.length > 0) inputarrays.push_back(createExternalVsgArray<vsg::vec4>(data.tangents.data, data.tangents.length));
            if (data.colors.length > 0) inputarrays.push_back(createExternalVsgArray<vsg::vec4>(data.colors.data, data.colors.length));
            if (data.uv0.length > 0) inputarrays.push_back(createExternalVsgArray<vsg::vec2>(data.uv0.data, data.uv0.length));
            if (data.uv1.length > 0) inputarrays.push_back(createExternalVsgArray<vsg::vec2>(data.uv1.data, data.uv1.length));

            cmd = vsg::BindVertexBuffers::create(0, inputarrays);
            _bindVertexBuffersCache[data.id] = cmd;
        }

        addCommandToHead(cmd);
    }

    void addDrawIndexedCommand(unity2vsg::DrawIndexedData data)
    {
        vsg::ref_ptr<vsg::Command> cmd;

        if (_drawIndexedCache.find(data.id) != _drawIndexedCache.end())
        {
            cmd = _drawIndexedCache[data.id];
        }
        else
        {
            cmd = vsg::DrawIndexed::create(data.indexCount, data.instanceCount, data.firstIndex, data.vertexOffset, data.firstInstance);
            _drawIndexedCache[data.id] = cmd;
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
            auto descriptorSet = vsg::DescriptorSet::create(_activeGraphicsPipeline->layout->setLayouts.front(), _descriptors);
            bindDescriptorSet = vsg::BindDescriptorSet::create(VK_PIPELINE_BIND_POINT_GRAPHICS, _activeGraphicsPipeline->layout, 0, descriptorSet);
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

    vsg::ref_ptr<vsg::DescriptorImage> createTexture(const DescriptorImageData& data, bool useCache = true)
    {
        vsg::ref_ptr<vsg::DescriptorImage> texture;

        // has a texture with this ID already been created
        if (useCache && _textureCache.find(data.id) != _textureCache.end())
        {
            texture = _textureCache[data.id];
        }
        else
        {
            vsg::ImageInfoList imageInfos;
            for (int i = 0; i < data.descriptorCount; i++)
            {
                vsg::ref_ptr<vsg::Data> texdata = createDataForTexture(data.images[i]);
                if (!texdata.valid())
                {
                    DebugLog("GraphBuilder Error: Unable to handle texture format");
                    return {};
                }

                vsg::ref_ptr<vsg::Sampler> sampler = createSamplerForTextureData(data.images[i]);

                imageInfos.push_back(vsg::ImageInfo::create(sampler, texdata));
            }

            texture = vsg::DescriptorImage::create(imageInfos, data.binding, 0, VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER);

            if (useCache) _textureCache[data.id] = texture;
        }

        return texture;
    }

    void addTexture(const DescriptorImageData& data)
    {
        auto texture = createTexture(data);
        _descriptors.push_back(texture);
        _descriptorObjectIds.push_back(std::to_string(data.id));
    }

    //
    // Uniforms

    void addDescriptorBuffer(DescriptorFloatUniformData data)
    {
        vsg::ref_ptr<vsg::floatValue> floatval = vsg::ref_ptr<vsg::floatValue>(new vsg::floatValue());
        floatval->value() = data.value;
        _descriptors.push_back(vsg::DescriptorBuffer::create(floatval, data.binding));
        _descriptorObjectIds.push_back(std::to_string(data.id));
    }

    void addDescriptorBuffer(DescriptorFloatArrayUniformData data)
    {
        vsg::DataList vallist;
        for (int i = 0; i < data.value.length; i++)
        {
            vsg::ref_ptr<vsg::floatValue> floatval = vsg::ref_ptr<vsg::floatValue>(new vsg::floatValue());
            floatval->value() = data.value.data[i];
            vallist.push_back(floatval);
        }

        _descriptors.push_back(vsg::DescriptorBuffer::create(vallist, data.binding));
        _descriptorObjectIds.push_back(std::to_string(data.id));
    }

    void addDescriptorBuffer(DescriptorVectorUniformData data)
    {
        //std::cout << "native id " << data.id << " binding " << data.binding << std::endl;
        vsg::ref_ptr<vsg::vec4Value> vecval = vsg::ref_ptr<vsg::vec4Value>(new vsg::vec4Value());
        //vecval->value() = data.value;
        vecval->value() = vsg::vec4(data.value.data[0], data.value.data[1], data.value.data[2], data.value.data[3]);
        _descriptors.push_back(vsg::DescriptorBuffer::create(vecval, data.binding));
        _descriptorObjectIds.push_back(std::to_string(data.id));
    }

    void addDescriptorBuffer(DescriptorVectorArrayUniformData data)
    {
        vsg::DataList vallist;
        for (int i = 0; i < data.value.length; i++)
        {
            vsg::ref_ptr<vsg::vec4Value> vecval = vsg::ref_ptr<vsg::vec4Value>(new vsg::vec4Value());
            vecval->value() = data.value.data[i];
            vallist.push_back(vecval);
        }

        _descriptors.push_back(vsg::DescriptorBuffer::create(vallist, data.binding));
        _descriptorObjectIds.push_back(std::to_string(data.id));
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

    vsg::LOD* getHeadAsLOD()
    {
        if (_nodeStack.size() == 0) return nullptr;
        return dynamic_cast<vsg::LOD*>(_nodeStack[_nodeStack.size() - 1].get());
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

    bool addLODChildToHead(vsg::ref_ptr<vsg::Node> node, LODChildData lodData)
    {
        vsg::LOD* headLOD = getHeadAsLOD();
        if (headLOD != nullptr)
        {
            vsg::LOD::Child lod;
            lod.node = node;
            lod.minimumScreenHeightRatio = lodData.minimumScreenHeightRatio;
            headLOD->addChild(lod);
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

    void writeFile(std::string fileName)
    {
        vsg::VSG io;
        io.write(_root, fileName);
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

    // caches

    std::map<int, vsg::ref_ptr<vsg::Command>> _bindVertexBuffersCache;
    std::map<int, vsg::ref_ptr<vsg::Command>> _bindIndexBufferCache;
    std::map<int, vsg::ref_ptr<vsg::Command>> _drawIndexedCache;
    std::map<int, vsg::ref_ptr<vsg::VertexIndexDraw>> _vertexIndexDrawCache;

    // map of shader modules to the masks used to create them
    std::map<std::string, vsg::ref_ptr<vsg::ShaderModule>> _shaderModulesCache;

    // map of descriptorimage to the ImageData ID they represent
    std::map<int, vsg::ref_ptr<vsg::DescriptorImage>> _textureCache;

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

void unity2vsg_AddLODNode(unity2vsg::CullData cull)
{
    _builder->addLOD(cull);
}

void unity2vsg_AddLODChild(unity2vsg::LODChildData lodChildData)
{
    _builder->addLODChild(lodChildData);
}

void unity2vsg_AddLightNode(unity2vsg::LightData lightData)
{
    _builder->addLightNode(lightData);
}

void unity2vsg_AddStateGroupNode()
{
    _builder->addStateGroup();
}

void unity2vsg_AddCommandsNode()
{
    _builder->addCommands();
}

void unity2vsg_AddVertexIndexDrawNode(unity2vsg::VertexIndexDrawData mesh)
{
    _builder->addVertexIndexDraw(mesh);
}

void unity2vsg_AddSkybox(unity2vsg::DescriptorImageData skyboxTexture)
{
    _builder->addSkybox(skyboxTexture);
}


//
// Meta data
//

void unity2vsg_AddStringValue(const char* name, const char* value)
{
    _builder->addStringValue(std::string(name), std::string(value));
}

void unity2vsg_AddFloatArray(const char* name, unity2vsg::FloatArray data)
{
    _builder->addFloatArray(std::string(name), data);
}

//
// Commands
//

int unity2vsg_AddBindGraphicsPipelineCommand(unity2vsg::PipelineData pipeline, uint32_t addToStateGroup)
{
    return _builder->addBindGraphicsPipelineCommand(pipeline, addToStateGroup == 1) ? 1 : 0;
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

void unity2vsg_AddDescriptorImage(unity2vsg::DescriptorImageData texture)
{
    _builder->addTexture(texture);
}

void unity2vsg_AddDescriptorBufferFloat(unity2vsg::DescriptorFloatUniformData data)
{
    _builder->addDescriptorBuffer(data);
}

void unity2vsg_AddDescriptorBufferFloatArray(unity2vsg::DescriptorFloatArrayUniformData data)
{
    _builder->addDescriptorBuffer(data);
}

void unity2vsg_AddDescriptorBufferVector(unity2vsg::DescriptorVectorUniformData data)
{
    _builder->addDescriptorBuffer(data);
}

void unity2vsg_AddDescriptorBufferVectorArray(unity2vsg::DescriptorVectorArrayUniformData data)
{
    _builder->addDescriptorBuffer(data);
}

void unity2vsg_EndNode()
{
    _builder->popNodeFromStack();
}

void unity2vsg_LaunchViewer(const char* filename, uint32_t useCamData, unity2vsg::CameraData camdata)
{
    try
    {
        vsg::VSG io;
        vsg::ref_ptr<vsg::Node> vsg_scene = io.read_cast<vsg::Node>(filename);

        /*std::stringstream ss;
        ss << "cam pos: " << camdata.position.x << ", " << camdata.position.y << ", " << camdata.position.z << std::endl;
        ss << "cam look: " << camdata.lookAt.x << ", " << camdata.lookAt.y << ", " << camdata.lookAt.z << std::endl;
        ss << "cam up: " << camdata.upDir.x << ", " << camdata.upDir.y << ", " << camdata.upDir.z << std::endl;
        //DebugLog(ss.str());
        */

        if (!vsg_scene.valid()) return;

        auto windowTraits = vsg::WindowTraits::create();
        windowTraits->windowTitle = "vsg export - " + std::string(filename);
        windowTraits->width = 800;
        windowTraits->height = 600;

        // create the viewer and assign window(s) to it
        auto viewer = vsg::Viewer::create();

        vsg::ref_ptr<vsg::Window> window(vsg::Window::create(windowTraits));
        if (!window.valid())
        {
            std::cout << "Could not create windows." << std::endl;
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
            lookAt = vsg::ref_ptr<vsg::LookAt>(new vsg::LookAt(vsg::dvec3(camdata.position.data[0], camdata.position.data[1], camdata.position.data[2]), vsg::dvec3(camdata.lookAt.data[0], camdata.lookAt.data[1], camdata.lookAt.data[2]), vsg::dvec3(camdata.upDir.data[0], camdata.upDir.data[1], camdata.upDir.data[2])));
        }
        else
        {
            perspective = vsg::ref_ptr<vsg::Perspective>(new vsg::Perspective(30.0, static_cast<double>(window->extent2D().width) / static_cast<double>(window->extent2D().height), nearFarRatio * radius, radius * 4.5));
            lookAt = vsg::ref_ptr<vsg::LookAt>(new vsg::LookAt(centre + vsg::dvec3(0.0, 0.0, -radius * 3.5), centre, vsg::dvec3(0.0, 1.0, 0.0)));
        }

        auto camera = vsg::Camera::create(perspective, lookAt, vsg::ViewportState::create(window->extent2D()));

        auto commandGraph = vsg::createCommandGraphForView(window, camera, vsg_scene);
        viewer->assignRecordAndSubmitTaskAndPresentation({commandGraph});

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

            viewer->update();

            viewer->recordAndSubmit();

            viewer->present();
        }
    }
    catch (...)
    {
        DebugLog("An exception occured during vsg preview.");
        return;
    }
}
