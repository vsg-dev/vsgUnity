/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

using System.Collections.Generic;
using UnityEngine;

using vsgUnity.Native;

namespace vsgUnity
{
    public static class GraphBuilder
    {
        public static void LaunchViewer(string fileName, bool useCamData, Camera camera)
        {
            GraphBuilderInterface.unity2vsg_LaunchViewer(fileName, useCamData ? 1 : 0, NativeUtils.CreateCameraData(camera));
        }

        public struct ExportSettings
        {
            public bool autoAddCullNodes;
            public bool zeroRootTransform;
            public string standardShaderMappingPath;
            public string standardTerrainShaderMappingPath;
        }

        public static void Export(GameObject[] gameObjects, string saveFileName, ExportSettings settings)
        {
            MeshConverter.ClearCaches();
            TextureConverter.ClearCaches();
            MaterialConverter.ClearCaches();
            ShaderMappingIO.ClearCaches();

            GraphBuilderInterface.unity2vsg_BeginExport();

            List<PipelineData> storePipelines = new List<PipelineData>();

            bool insideLODGroup = false;
            bool firstNodeAdded = false;

            System.Action<GameObject> processGameObject = null;
            processGameObject = (GameObject go) =>
            {
                // determine the gameObject type
                Transform gotrans = go.transform;

                bool nodeAdded = false;

                // does it have a none identiy local matrix
                if (gotrans.localPosition != Vector3.zero || gotrans.localRotation != Quaternion.identity || gotrans.localScale != Vector3.one)
                {
                    if (firstNodeAdded || !settings.zeroRootTransform)
                    {
                        // add as a transform
                        TransformData transformdata = TransformConverter.CreateTransformData(gotrans);
                        GraphBuilderInterface.unity2vsg_AddTransformNode(transformdata);
                        nodeAdded = true;
                    }
                }

                // do we need to insert a group
                if (!nodeAdded)// && gotrans.childCount > 0)
                {
                    //add as a group
                    GraphBuilderInterface.unity2vsg_AddGroupNode();
                    nodeAdded = true;
                }

                firstNodeAdded = true;
                bool meshexported = false;

                // get the meshrender here so we can check if the LOD exports the the mesh
                MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();

                // does this node have an LOD group
                LODGroup lodgroup = go.GetComponent<LODGroup>();
                if (lodgroup != null && !insideLODGroup)
                {
                    // rather than process the children we figure out which renderers are in which children and add them as LOD children
                    LOD[] lods = lodgroup.GetLODs();
                    if (lods.Length > 0)
                    {
                        // get bounds from first renderer
                        if (lods[0].renderers.Length > 0)
                        {
                            CullData lodCullData = new CullData();
                            Bounds bounds = new Bounds(lods[0].renderers[0].bounds.center, lods[0].renderers[0].bounds.size);
                            foreach(Renderer boundsrenderer in lods[0].renderers)
                            {
                                if(boundsrenderer != null) bounds.Encapsulate(boundsrenderer.bounds);
                            }
                            Vector3 center = bounds.center - gotrans.position;
                            CoordSytemConverter.Convert(ref center);
                            lodCullData.center = NativeUtils.ToNative(center);
                            lodCullData.radius = bounds.size.magnitude * 0.5f;
                            GraphBuilderInterface.unity2vsg_AddLODNode(lodCullData);

                            insideLODGroup = true;

                            for (int i = 0; i < lods.Length; i++)
                            {
                                // for now just support one renderer and assume it's under a seperate child gameObject
                                if (lods[i].renderers.Length == 0) continue;

                                LODChildData lodChild = new LODChildData();
                                lodChild.minimumScreenHeightRatio = lods[i].screenRelativeTransitionHeight;
                                GraphBuilderInterface.unity2vsg_AddLODChild(lodChild);

                                foreach (Renderer lodrenderer in lods[i].renderers)
                                {
                                    if (lodrenderer == meshRenderer)
                                    {
                                        meshexported = true;
                                        ExportMesh(meshFilter.sharedMesh, meshRenderer, gotrans, settings, storePipelines);
                                    }
                                    else if(lodrenderer != null)
                                    {
                                        // now process the renderers gameobject, it'll be added to the group we just created by adding an LOD child
                                        processGameObject(lodrenderer.gameObject);
                                    }
                                }

                                GraphBuilderInterface.unity2vsg_EndNode();
                            }

                            insideLODGroup = false;

                            GraphBuilderInterface.unity2vsg_EndNode(); // end the lod node
                        }
                    }
                }
                else
                {
                    // transverse any children
                    for (int i = 0; i < gotrans.childCount; i++)
                    {
                        processGameObject(gotrans.GetChild(i).gameObject);
                    }
                }

                // does it have a mesh
                if (!meshexported && meshFilter && meshFilter.sharedMesh && meshRenderer)
                {
                    Mesh mesh = meshFilter.sharedMesh;
                    ExportMesh(mesh, meshRenderer, gotrans, settings, storePipelines);
                }

                // does this node have a terrain
                Terrain terrain = go.GetComponent<Terrain>();
                if (terrain != null)
                {
                    ExportTerrainMesh(terrain, settings, storePipelines);
                }

                // if we added a group or transform step out
                if (nodeAdded)
                {
                    GraphBuilderInterface.unity2vsg_EndNode();
                }
            };

            foreach(GameObject go in gameObjects)
            {
                processGameObject(go);
            }

            //GraphBuilderInterface.unity2vsg_EndNode(); // step out of convert coord system node

            GraphBuilderInterface.unity2vsg_EndExport(saveFileName);
            NativeLog.PrintReport();
        }

        // Bind the descriptors in a materialinfo, should be called from within a StateGroup
        
        private static void BindDescriptors(MaterialInfo materialInfo, bool addToStateGroup)
        {
            bool addedAny = false;
            foreach (DescriptorImageData t in materialInfo.imageDescriptors)
            {
                GraphBuilderInterface.unity2vsg_AddDescriptorImage(t);
                addedAny = true;
            }
            foreach (DescriptorVectorUniformData t in materialInfo.vectorDescriptors)
            {
                GraphBuilderInterface.unity2vsg_AddDescriptorBufferVector(t);
                addedAny = true;
            }
            foreach (DescriptorFloatUniformData t in materialInfo.floatDescriptors)
            {
                GraphBuilderInterface.unity2vsg_AddDescriptorBufferFloat(t);
                addedAny = true;
            }
            if (addedAny) GraphBuilderInterface.unity2vsg_CreateBindDescriptorSetCommand(addToStateGroup ? 1 : 0);
        }

        private static void ExportMesh(Mesh mesh, MeshRenderer meshRenderer, Transform gotrans,  ExportSettings settings, List<PipelineData> storePipelines = null)
        {
            bool addedCullGroup = false;
            if (settings.autoAddCullNodes)
            {
                CullData culldata = new CullData();
                Vector3 center = meshRenderer.bounds.center - gotrans.position;
                CoordSytemConverter.Convert(ref center);
                culldata.center = NativeUtils.ToNative(center);
                culldata.radius = meshRenderer.bounds.size.magnitude * 0.5f;
                GraphBuilderInterface.unity2vsg_AddCullGroupNode(culldata);
                addedCullGroup = true;
            }

            //
            Material[] materials = meshRenderer.sharedMaterials;

            if (mesh != null && mesh.isReadable && mesh.vertexCount > 0 && mesh.GetIndexCount(0) > 0)
            {
                int meshid = mesh.GetInstanceID();

                MeshInfo meshInfo = MeshConverter.GetOrCreateMeshInfo(mesh);

                int subMeshCount = mesh.subMeshCount;

                // shader instance id, Material Data, sub mesh indicies
                Dictionary<int, Dictionary<MaterialInfo, List<int>>> meshMaterials = new Dictionary<int, Dictionary<MaterialInfo, List<int>>>();
                for (int matindex = 0; matindex < materials.Length && matindex < subMeshCount; matindex++)
                {
                    Material mat = materials[matindex];
                    if (mat == null) continue;

                    MaterialInfo matdata = MaterialConverter.GetOrCreateMaterialData(mat);
                    int matshaderid = matdata.shaderStages.id;

                    if (!meshMaterials.ContainsKey(matshaderid)) meshMaterials.Add(matshaderid, new Dictionary<MaterialInfo, List<int>>());
                    if (!meshMaterials[matshaderid].ContainsKey(matdata)) meshMaterials[matshaderid].Add(matdata, new List<int>());

                    meshMaterials[matshaderid][matdata].Add(matindex);
                }

                if (subMeshCount > 1)
                {

                    // create mesh data, if the mesh has already been created we only need to pass the ID to the addGeometry function
                    foreach (int shaderkey in meshMaterials.Keys)
                    {
                        List<MaterialInfo> mds = new List<MaterialInfo>(meshMaterials[shaderkey].Keys);

                        if (mds.Count == 0) continue;

                        // add stategroup and pipeline for shader
                        GraphBuilderInterface.unity2vsg_AddStateGroupNode();

                        PipelineData pipelineData = NativeUtils.CreatePipelineData(meshInfo); //WE NEED INFO ABOUT THE SHADER SO WE CAN BUILD A PIPLE LINE
                        pipelineData.descriptorBindings = NativeUtils.WrapArray(mds[0].descriptorBindings.ToArray());
                        pipelineData.shaderStages = mds[0].shaderStages.ToNative();
                        pipelineData.useAlpha = mds[0].useAlpha;
                        pipelineData.id = NativeUtils.ToNative(NativeUtils.GetIDForPipeline(pipelineData));
                        storePipelines.Add(pipelineData);

                        if (GraphBuilderInterface.unity2vsg_AddBindGraphicsPipelineCommand(pipelineData, 1) == 1)
                        {

                            GraphBuilderInterface.unity2vsg_AddCommandsNode();

                            VertexBuffersData vertexBuffersData = MeshConverter.GetOrCreateVertexBuffersData(meshInfo);
                            GraphBuilderInterface.unity2vsg_AddBindVertexBuffersCommand(vertexBuffersData);

                            IndexBufferData indexBufferData = MeshConverter.GetOrCreateIndexBufferData(meshInfo);
                            GraphBuilderInterface.unity2vsg_AddBindIndexBufferCommand(indexBufferData);


                            foreach (MaterialInfo md in mds)
                            {
                                BindDescriptors(md, false);

                                foreach (int submeshIndex in meshMaterials[shaderkey][md])
                                {
                                    DrawIndexedData drawIndexedData = MeshConverter.GetOrCreateDrawIndexedData(meshInfo, submeshIndex);
                                    GraphBuilderInterface.unity2vsg_AddDrawIndexedCommand(drawIndexedData);
                                }
                            }

                            GraphBuilderInterface.unity2vsg_EndNode(); // step out of commands node for descriptors and draw indexed commands
                        }
                        GraphBuilderInterface.unity2vsg_EndNode(); // step out of stategroup node for shader
                    }
                }
                else
                {
                    List<int> sids = new List<int>(meshMaterials.Keys);
                    if (sids.Count > 0)
                    {
                        List<MaterialInfo> mds = new List<MaterialInfo>(meshMaterials[sids[0]].Keys);

                        if (mds.Count > 0)
                        {
                            // add stategroup and pipeline for shader
                            GraphBuilderInterface.unity2vsg_AddStateGroupNode();

                            PipelineData pipelineData = NativeUtils.CreatePipelineData(meshInfo); //WE NEED INFO ABOUT THE SHADER SO WE CAN BUILD A PIPLE LINE
                            pipelineData.descriptorBindings = NativeUtils.WrapArray(mds[0].descriptorBindings.ToArray());
                            pipelineData.shaderStages = mds[0].shaderStages.ToNative();
                            pipelineData.useAlpha = mds[0].useAlpha;
                            pipelineData.id = NativeUtils.ToNative(NativeUtils.GetIDForPipeline(pipelineData));
                            storePipelines.Add(pipelineData);

                            if (GraphBuilderInterface.unity2vsg_AddBindGraphicsPipelineCommand(pipelineData, 1) == 1)
                            {
                                BindDescriptors(mds[0], true);

                                VertexIndexDrawData vertexIndexDrawData = MeshConverter.GetOrCreateVertexIndexDrawData(meshInfo);
                                GraphBuilderInterface.unity2vsg_AddVertexIndexDrawNode(vertexIndexDrawData);

                                GraphBuilderInterface.unity2vsg_EndNode(); // step out of vertex index draw node
                            }
                            GraphBuilderInterface.unity2vsg_EndNode(); // step out of stategroup node
                        }
                    }
                }
            }
            else
            {
                string reason = mesh == null ? "mesh is null." : (!mesh.isReadable ? "mesh '" + mesh.name + "' is not readable. Please enabled read/write in the models import settings." : "mesh '" + mesh.name + "' has an unknown error.");
                NativeLog.WriteLine("ExportMesh: Unable to export mesh for gameobject " + gotrans.gameObject.name + ", " + reason);
            }

            if (addedCullGroup)
            {
                GraphBuilderInterface.unity2vsg_EndNode();
            }
        }

        private static void ExportTerrainMesh(Terrain terrain, ExportSettings settings, List<PipelineData> storePipelines = null)
        {
            TerrainConverter.TerrainInfo terrainInfo = TerrainConverter.CreateTerrainInfo(terrain, settings);

            if (terrainInfo == null || ((terrainInfo.diffuseTextureDatas.Count == 0 || terrainInfo.maskTextureDatas.Count == 0) && terrainInfo.customMaterial == null)) return;

            // add stategroup and pipeline for shader
            GraphBuilderInterface.unity2vsg_AddStateGroupNode();

            PipelineData pipelineData = new PipelineData();
            pipelineData.hasNormals = 1;
            pipelineData.uvChannelCount = 1;
            pipelineData.useAlpha = 0;

            if (terrainInfo.customMaterial == null)
            {
                pipelineData.descriptorBindings = NativeUtils.WrapArray(terrainInfo.descriptorBindings.ToArray());
                ShaderStagesInfo shaderStagesInfo = MaterialConverter.GetOrCreateShaderStagesInfo(terrainInfo.shaderMapping.shaders.ToArray(), string.Join(",", terrainInfo.shaderDefines.ToArray()), terrainInfo.shaderConsts.ToArray());
                pipelineData.shaderStages = shaderStagesInfo.ToNative();

                pipelineData.id = NativeUtils.ToNative(NativeUtils.GetIDForPipeline(pipelineData));
                storePipelines.Add(pipelineData);

                if (GraphBuilderInterface.unity2vsg_AddBindGraphicsPipelineCommand(pipelineData, 1) == 1)
                {
                    if (terrainInfo.diffuseTextureDatas.Count > 0)
                    {
                        DescriptorImageData layerDiffuseTextureArray = MaterialConverter.GetOrCreateDescriptorImageData(terrainInfo.diffuseTextureDatas.ToArray(), 0);
                        GraphBuilderInterface.unity2vsg_AddDescriptorImage(layerDiffuseTextureArray);
                    }

                    if (terrainInfo.diffuseScales.Count > 0)
                    {
                        DescriptorVectorArrayUniformData scalesDescriptor = new DescriptorVectorArrayUniformData();
                        scalesDescriptor.binding = 2;
                        scalesDescriptor.value = NativeUtils.WrapArray(terrainInfo.diffuseScales.ToArray());
                        GraphBuilderInterface.unity2vsg_AddDescriptorBufferVectorArray(scalesDescriptor);
                    }

                    DescriptorVectorUniformData sizeDescriptor = new DescriptorVectorUniformData();
                    sizeDescriptor.binding = 3;
                    sizeDescriptor.value = NativeUtils.ToNative(terrainInfo.terrainSize);
                    GraphBuilderInterface.unity2vsg_AddDescriptorBufferVector(sizeDescriptor);

                    if (terrainInfo.maskTextureDatas.Count > 0)
                    {
                        DescriptorImageData layerMaskTextureArray = MaterialConverter.GetOrCreateDescriptorImageData(terrainInfo.maskTextureDatas.ToArray(), 1);
                        GraphBuilderInterface.unity2vsg_AddDescriptorImage(layerMaskTextureArray);
                    }

                    GraphBuilderInterface.unity2vsg_CreateBindDescriptorSetCommand(1);

                    GraphBuilderInterface.unity2vsg_AddVertexIndexDrawNode(MeshConverter.GetOrCreateVertexIndexDrawData(terrainInfo.terrainMesh));
                    GraphBuilderInterface.unity2vsg_EndNode(); // step out of vertex index draw node
                }
            }
            else
            {
                pipelineData.descriptorBindings = NativeUtils.WrapArray(terrainInfo.customMaterial.descriptorBindings.ToArray());
                pipelineData.shaderStages = terrainInfo.customMaterial.shaderStages.ToNative();
                pipelineData.id = NativeUtils.ToNative(NativeUtils.GetIDForPipeline(pipelineData));
                storePipelines.Add(pipelineData);

                if (GraphBuilderInterface.unity2vsg_AddBindGraphicsPipelineCommand(pipelineData, 1) == 1)
                {
                    BindDescriptors(terrainInfo.customMaterial, true);

                    GraphBuilderInterface.unity2vsg_AddVertexIndexDrawNode(MeshConverter.GetOrCreateVertexIndexDrawData(terrainInfo.terrainMesh));
                    GraphBuilderInterface.unity2vsg_EndNode(); // step out of vertex index draw node
                }

                
            }
            GraphBuilderInterface.unity2vsg_EndNode(); // step out of stategroup node

        }
    }

}
