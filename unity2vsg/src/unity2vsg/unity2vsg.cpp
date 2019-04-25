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

void unity2vsg_ExportMesh(unity2vsg::MeshData mesh)
{
	vsg::ref_ptr<vsg::MatrixTransform> root = vsg::MatrixTransform::create();

    // setup the GraphicsPiplineBuilder
    vsg::ref_ptr<vsg::GraphicsPipelineBuilder> pipelinebuilder = vsg::GraphicsPipelineBuilder::create();
    vsg::ref_ptr<vsg::GraphicsPipelineBuilder::Traits> traits = vsg::GraphicsPipelineBuilder::Traits::create();

    // vertex input
    auto inputarrays = vsg::DataList{ createVsgArray<vsg::vec3>(mesh.verticies.ptr, mesh.verticies.length) }; // always have verticies
    vsg::GraphicsPipelineBuilder::Traits::BindingFormats inputformats = { { VK_FORMAT_R32G32B32_SFLOAT } };
    uint32_t inputshaderatts = VERTEX;

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
    geometry->_commands = { vsg::DrawIndexed::create(mesh.triangles.length, 1, 0, 0, 0) };

    // add the geometry
    stateGroup->addChild(geometry);

    // write the graph to file
    vsg::vsgReaderWriter io;
    io.writeFile(root.get(), "C:\\Work\\VSG\\meshexport.vsga");

    // we're done so release the arrays before vsg ref_ptr tries to delete them (at the mo they are C# memory)
    for(auto array : inputarrays)
    {
        array->dataRelease();
    }
}

class ReleaseArrays : public vsg::Visitor
{
public:
    ReleaseArrays()
    {
    }

    void apply(vsg::Node& node)
    {
        node.traverse(*this);
    }

    void apply(vsg::Group& group)
    {
        if (auto geometry = dynamic_cast<vsg::Geometry*>(&group); geometry != nullptr)
        {
            apply(*geometry);
        }
        else
        {
            group.traverse(*this);
        }
    }

    void apply(vsg::Geometry& geometry)
    {
        if (!geometry._arrays.empty())
        {
            for(vsg::ref_ptr<vsg::Data>& dataArray : geometry._arrays)
            {
                dataArray->accept(*this);
            }
        }
    }

    void apply(vsg::vec3Array& vertices)
    {
        vertices.dataRelease();
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

    void addGroup()
    {
        auto group = vsg::Group::create();
        if (!addChildToHead(group))
        {
            DebugLog("ExportState Warning: Current head is not a group");
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

    void addGeometry(const MeshData& data)
    {
        vsg::ref_ptr<vsg::Geometry> geometry;

        // has a geometry with this ID already been created
        if (_geometryCache.find(data.id) != _geometryCache.end())
        {
            geometry = _geometryCache[data.id];
        }
        else
        {
            geometry = vsg::Geometry::create();

            // vertex inputs
            auto inputarrays = vsg::DataList{ createVsgArray<vsg::vec3>(data.verticies.ptr, data.verticies.length) }; // always have verticies

            if (data.normals.length > 0)inputarrays.push_back(createVsgArray<vsg::vec3>(data.normals.ptr, data.normals.length));
            if (data.uv0.length > 0) inputarrays.push_back(createVsgArray<vsg::vec2>(data.uv0.ptr, data.uv0.length));

            geometry->_arrays = inputarrays;

            // for now convert the int32 array indicies to uint16
            vsg::ref_ptr<vsg::ushortArray> indiciesushort(new vsg::ushortArray(data.triangles.length));
            for (uint32_t i = 0; i < data.triangles.length; i++)
            {
                indiciesushort->set(i, static_cast<uint16_t>(data.triangles.ptr[i]));
            }

            geometry->_indices = indiciesushort; //createVsgArray<uint16_t>(reinterpret_cast<uint16_t*>(mesh.triangles.ptr), mesh.triangles.length);
            geometry->_commands = { vsg::DrawIndexed::create(static_cast<uint32_t>(indiciesushort->valueCount()), 1, 0, 0, 0) };

            _geometryCache[data.id] = geometry;
        }

        if (!addChildToHead(geometry))
        {
            DebugLog("GraphBuilder Error: Current head is not a group");
        }

        pushNodeToStack(geometry);
    }

    void addStateGroup()
    {
        auto stategroup = vsg::StateGroup::create();
        if (!addChildToHead(stategroup))
        {
            DebugLog("GraphBuilder Error: Current head is not a group");
        }
        pushNodeToStack(stategroup);
    }

    void addBindGraphicsPipeline(PipelineData data)
    {
        vsg::ref_ptr<vsg::GraphicsPipelineBuilder> pipelinebuilder = vsg::GraphicsPipelineBuilder::create();
        vsg::ref_ptr<vsg::GraphicsPipelineBuilder::Traits> traits = vsg::GraphicsPipelineBuilder::Traits::create();

        // vertex input
        vsg::GraphicsPipelineBuilder::Traits::BindingFormats inputformats = { { VK_FORMAT_R32G32B32_SFLOAT } };
        uint32_t inputshaderatts = VERTEX;

        if (data.hasNormals)
        {
            inputformats.push_back({ VK_FORMAT_R32G32B32_SFLOAT });
            inputshaderatts |= NORMAL;
        }
        if (data.uvChannelCount > 0) // uv set 0
        {
            inputformats.push_back({ VK_FORMAT_R32G32_SFLOAT });
            inputshaderatts |= TEXCOORD0;
        }

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
                vsg::ShaderModule::create(VK_SHADER_STAGE_FRAGMENT_BIT, "main", createFbxFragmentSource(shaderMode, inputshaderatts))
            };

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

        // create our graphics pipeline
        pipelinebuilder->build(traits);

        auto bindGraphicsPipeline = vsg::BindGraphicsPipeline::create(pipelinebuilder->getGraphicsPipeline());

        if (!addStateCommandToHead(bindGraphicsPipeline))
        {
            DebugLog("GraphBuilder Error: Current head is StateGroup");
        }
    }

    vsg::Node* getHead()
    {
        if(_nodeStack.size() == 0) return nullptr;
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

    bool addStateCommandToHead(vsg::ref_ptr<vsg::StateCommand> command)
    {
        vsg::StateGroup* headStateGroup = getHeadAsStateGroup();
        if (headStateGroup != nullptr)
        {
            headStateGroup->add(command);
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
    }

    uint32_t getShaderKey(uint32_t x, uint32_t y)
    {
        return (x << 16) | (y && 0xFFFF);
    }

    void writeFile(std::string fileName, bool useBinary)
    {
        vsg::vsgReaderWriter io;
        io.writeFile(_root.get(), fileName + (useBinary ? ".vsgb" : ".vsga"));
    }

    void releaseObjects()
    {
        ReleaseArrays releaser;
        _root->accept(releaser);
    }

    vsg::ref_ptr<vsg::MatrixTransform> _root;

    // the stack of nodes added, last node is the current head being acted on
    std::vector<vsg::ref_ptr<vsg::Node>> _nodeStack;

    // map of exisitng Geometryies to the MeshData ID the represent
    std::map<uint32_t, vsg::ref_ptr<vsg::Geometry>> _geometryCache;

    // map of shader modules to the masks used to create them
    std::map<uint32_t, vsg::ShaderModules> _shaderModulesCache;
    
    std::string _saveFileName;
};

vsg::ref_ptr<GraphBuilder> _builder;

void unity2vsg_BeginExport()
{
    _builder = vsg::ref_ptr<GraphBuilder>(new GraphBuilder());
}

void unity2vsg_EndExport(const char* saveFileName, uint32_t useBinary, uint32_t launchViewer)
{
    _builder->writeFile(std::string(saveFileName), (bool)useBinary);

    _builder->releaseObjects();
}

void unity2vsg_AddGroup()
{
    _builder->addGroup();
}

void unity2vsg_AddTransform(unity2vsg::TransformData transform)
{
    _builder->addMatrixTrasform(transform);
}

void unity2vsg_AddGeometry(unity2vsg::MeshData mesh)
{
    _builder->addGeometry(mesh);
}

void unity2vsg_AddStateGroup()
{
    _builder->addStateGroup();
}

void unity2vsg_AddBindGraphicsPipeline(unity2vsg::PipelineData pipeline)
{
    _builder->addBindGraphicsPipeline(pipeline);
}

void unity2vsg_EndNode()
{
    _builder->popNodeFromStack();
}
