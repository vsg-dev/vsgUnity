/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth
Copyright(c) 2022 Christian Schott (InstruNEXT GmbH)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using vsgUnity.Native;

namespace vsgUnity
{

public class OnBeginExportAttribute : Attribute { }
public class OnEndExportAttribute : Attribute { }
public class OnProcessGameObjectAttribute : Attribute { }

public class SceneGraphExporter : System.IDisposable
{
    private event Action OnBeginExport = delegate {};
    private event Action<GameObject> OnProcessGameObject = delegate {};
    private event Action OnEndExport = delegate {};

    public readonly string ExportPath;
    public readonly GraphBuilder.ExportSettings ExportSettings;

    private bool firstNodeAdded = false;
    private int insideLODGroupCount = 0;
    public bool InsideLODGroup { get { return insideLODGroupCount > 0; } }

    private List<PipelineData> storePipelines = new List<PipelineData>();

    public SceneGraphExporter(string path, GraphBuilder.ExportSettings settings) 
    {
        ExportPath = path;
        ExportSettings = settings;
        loadExporterHooks();
        NativeLog.InstallDebugLogCallback();
        GraphBuilderInterface.unity2vsg_BeginExport();
        OnBeginExport.Invoke();
    }

    private void loadExporterHooks()
    {
        foreach (var type in typeof(SceneGraphExporter).Assembly.GetTypes())
        {
            if (!type.IsSealed || !type.IsClass) continue;
            foreach (var method in type.GetRuntimeMethods())
            {
                if (!method.IsStatic) continue;
                if (method.GetCustomAttribute<OnBeginExportAttribute>() != null) 
                    OnBeginExport += (Action)method.CreateDelegate(typeof(Action));
                if (method.GetCustomAttribute<OnEndExportAttribute>() != null) 
                    OnEndExport += (Action)method.CreateDelegate(typeof(Action));
                if (method.GetCustomAttribute<OnProcessGameObjectAttribute>() != null) 
                    OnProcessGameObject += (Action<GameObject>)method.CreateDelegate(typeof(Action<GameObject>));
            }
        }
    }

    public Node CreateNodeForGameObject(GameObject go) 
    {
        Node node;
        {
            bool useAsZeroNode = !firstNodeAdded && ExportSettings.zeroRootTransform;
            Transform trans = go.transform;
            bool isIdentityTransform = trans.localPosition == Vector3.zero && trans.localRotation == Quaternion.identity && trans.localScale == Vector3.one;
            if (useAsZeroNode || (isIdentityTransform && !ExportSettings.keepIdentityTransforms)) {
                // add as a group
                node = new GroupNode();
            } else {
                // add as a transform
                node = new TransformNode(TransformConverter.CreateTransformData(trans));
            }
            firstNodeAdded = true;
        }
        OnProcessGameObject.Invoke(go);
        return node;
    }

    public LODNode CreateLODNode(CullData cullData) 
    {
        var lodNode = new LODNode(cullData);
        lodNode.OnLeavingNode += () => { insideLODGroupCount--; };
        insideLODGroupCount++;
        return lodNode;
    }

    public CullGroupNode BeginMeshExport(Vector3 position, Renderer renderer) 
    {
        if (ExportSettings.autoAddCullNodes)
        {
            return new CullGroupNode(NativeUtils.GetCullData(position, renderer));
        }
        return null;
    }

    public PipelineData CreateGraphicsPipeline(MeshInfo meshInfo, MaterialInfo materialInfo) 
    {
        PipelineData pipelineData = NativeUtils.CreatePipelineData(meshInfo); //WE NEED INFO ABOUT THE SHADER SO WE CAN BUILD A PIPLE LINE
        pipelineData.descriptorBindings = NativeUtils.WrapArray(materialInfo.descriptorBindings.ToArray());
        pipelineData.shaderStages = materialInfo.shaderStages.ToNative();
        pipelineData.useAlpha = materialInfo.useAlpha;
        pipelineData.id = NativeUtils.ToNative(NativeUtils.GetIDForPipeline(pipelineData));
        storePipelines.Add(pipelineData);
        return pipelineData;
    }

    public PipelineData CreateGraphicsPipeline(TerrainConverter.TerrainInfo terrainInfo)
    {
        PipelineData pipelineData = new PipelineData();
        pipelineData.hasNormals = 1;
        pipelineData.uvChannelCount = 1;
        pipelineData.useAlpha = 0;
        if (terrainInfo.customMaterial == null) {
            pipelineData.descriptorBindings = NativeUtils.WrapArray(terrainInfo.descriptorBindings.ToArray());
            ShaderStagesInfo shaderStagesInfo = MaterialConverter.GetOrCreateShaderStagesInfo(terrainInfo.shaderMapping.shaders.ToArray(), string.Join(",", terrainInfo.shaderDefines.ToArray()), terrainInfo.shaderConsts.ToArray());
            pipelineData.shaderStages = shaderStagesInfo.ToNative();
        } else {
            pipelineData.descriptorBindings = NativeUtils.WrapArray(terrainInfo.customMaterial.descriptorBindings.ToArray());
            pipelineData.shaderStages = terrainInfo.customMaterial.shaderStages.ToNative();  
        }
        pipelineData.id = NativeUtils.ToNative(NativeUtils.GetIDForPipeline(pipelineData));
        storePipelines.Add(pipelineData);
        return pipelineData;
    }

    public void Dispose () 
    {
        OnEndExport.Invoke();
        GraphBuilderInterface.unity2vsg_EndExport(ExportPath);
        NativeLog.PrintReport();
    }
}

// disposable util class, that ends nodes automatically when they go out of scope

public abstract class Node : System.IDisposable
{
    public event System.Action OnLeavingNode = delegate {};

    public void Dispose() {
        OnLeavingNode.Invoke();
        GraphBuilderInterface.unity2vsg_EndNode();
    }
}

public class GroupNode : Node
{
    public GroupNode() {
        GraphBuilderInterface.unity2vsg_AddGroupNode();
    }
}

public class TransformNode : Node
{
    public TransformNode(TransformData transform) {
        GraphBuilderInterface.unity2vsg_AddTransformNode(transform);
    }
}

public class CullGroupNode : Node
{
    public CullGroupNode(CullData cullData) {
        GraphBuilderInterface.unity2vsg_AddCullGroupNode(cullData);
    }
}

public class CullNode : Node
{
    public CullNode(CullData cullData) {
        GraphBuilderInterface.unity2vsg_AddCullNode(cullData);
    }
}


public class LODNode : Node
{
    public LODNode(CullData cullData) {
        GraphBuilderInterface.unity2vsg_AddLODNode(cullData);
    }

    public LODChildNode AddLODChild(LODChildData childData) {
        return new LODChildNode(childData);
    }
}

public class LODChildNode : Node
{
    public LODChildNode(LODChildData childData) {
        GraphBuilderInterface.unity2vsg_AddLODChild(childData);
    }
}

public class LightNode : Node
{
    public LightNode(LightData lightData) {
        GraphBuilderInterface.unity2vsg_AddLightNode(lightData);
    }
}

public class StateGroupNode : Node
{
    public StateGroupNode(PipelineData pipelineData) {
        GraphBuilderInterface.unity2vsg_AddStateGroupNode();
        GraphBuilderInterface.unity2vsg_AddBindGraphicsPipelineCommand(pipelineData, 1);
    }

    public BindDescriptorSetCommand CreateBindDescriptorSetCommand() {
        return new BindDescriptorSetCommand(this);
    }
}

public class VertexIndexDrawNode : Node
{
    public VertexIndexDrawNode(VertexIndexDrawData indexedDrawData) {
        GraphBuilderInterface.unity2vsg_AddVertexIndexDrawNode(indexedDrawData);
    }
}

public class CommandsNode : Node
{
    public CommandsNode() {
        GraphBuilderInterface.unity2vsg_AddCommandsNode();
    }

    public void BindGraphicsPipeline(PipelineData pipeline) {
        GraphBuilderInterface.unity2vsg_AddBindGraphicsPipelineCommand(pipeline, 0);
    }

    public void BindIndexBuffer(IndexBufferData indexData) {
        GraphBuilderInterface.unity2vsg_AddBindIndexBufferCommand(indexData);
    }

    public void BindVertexBuffers(VertexBuffersData vertexData) {
        GraphBuilderInterface.unity2vsg_AddBindVertexBuffersCommand(vertexData);
    }

    public void DrawIndexed(DrawIndexedData drawIndexed) {
        GraphBuilderInterface.unity2vsg_AddDrawIndexedCommand(drawIndexed);
    }

    public BindDescriptorSetCommand CreateBindDescriptorSetCommand() {
        return new BindDescriptorSetCommand(this);
    }
}

public class BindDescriptorSetCommand : System.IDisposable
{
    private bool addToStateGroup;
    public List<DescriptorImageData> images = new List<DescriptorImageData>();
    public List<DescriptorVectorUniformData> vectors = new List<DescriptorVectorUniformData>();
    public List<DescriptorVectorArrayUniformData> vectorArrays = new List<DescriptorVectorArrayUniformData>();
    public List<DescriptorFloatUniformData> floats = new List<DescriptorFloatUniformData>();
    public List<DescriptorFloatArrayUniformData> floatArrays = new List<DescriptorFloatArrayUniformData>();
    public List<DescriptorFloatBufferUniformData> floatBuffers = new List<DescriptorFloatBufferUniformData>();

    public BindDescriptorSetCommand(StateGroupNode s) {
        addToStateGroup = true;
    }

    public BindDescriptorSetCommand(CommandsNode c) {
        addToStateGroup = false;
    }

    public void AddDescriptors(MaterialInfo materialInfo) {
        images.AddRange(materialInfo.imageDescriptors);
        vectors.AddRange(materialInfo.vectorDescriptors);
        floats.AddRange(materialInfo.floatDescriptors);
        floatBuffers.AddRange(materialInfo.floatBufferDescriptors);
    }

    public void Dispose() {
        foreach(var data in images)
            GraphBuilderInterface.unity2vsg_AddDescriptorImage(data);
        foreach(var data in vectors)
            GraphBuilderInterface.unity2vsg_AddDescriptorBufferVector(data);
        foreach(var data in vectorArrays)
            GraphBuilderInterface.unity2vsg_AddDescriptorBufferVectorArray(data);
        foreach(var data in floats)
            GraphBuilderInterface.unity2vsg_AddDescriptorBufferFloat(data);
        foreach(var data in floatArrays)
            GraphBuilderInterface.unity2vsg_AddDescriptorBufferFloatArray(data);
        foreach(var data in floatBuffers)
            GraphBuilderInterface.unity2vsg_AddDescriptorBufferFloatBuffer(data);
        if (images.Count > 0 || vectors.Count > 0 || vectorArrays.Count > 0 || floats.Count > 0 || floatArrays.Count > 0 || floatBuffers.Count > 0)
            GraphBuilderInterface.unity2vsg_CreateBindDescriptorSetCommand(addToStateGroup ? 1 : 0);
    }
}

}