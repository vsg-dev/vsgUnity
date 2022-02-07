/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth
Copyright(c) 2022 Christian Schott (InstruNEXT GmbH)

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

        [System.Serializable]
        public struct ExportSettings
        {
            public bool autoAddCullNodes;
            public bool zeroRootTransform;
            public bool keepIdentityTransforms;
            public Cubemap skybox;
            public string standardShaderMappingPath;
            public string standardTerrainShaderMappingPath;
        }

        public static void Export(GameObject[] gameObjects, string saveFileName, ExportSettings settings)
        {
            using (var vsgExporter = new SceneGraphExporter(saveFileName, settings)) {
                // add ambient light
                if (LightConverter.CreateAmbientLight(out LightData ambientLight))
                    using (new LightNode(ambientLight)) {}

                // add skybox
                if (settings.skybox)
                    ExportSkybox(settings.skybox);

                foreach(GameObject go in gameObjects)
                {
                    processGameObject(go, vsgExporter);
                }
            }
        }

        private static void processGameObject(GameObject go, SceneGraphExporter vsgExporter) 
        {
            using (var node = vsgExporter.CreateNodeForGameObject(go)) {
                var gotrans = go.transform;
                bool meshexported = false;

                // get the meshrender here so we can check if the LOD exports the the mesh
                MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();

                // does this node have an LOD group
                LODGroup lodgroup = go.GetComponent<LODGroup>();
                if (lodgroup != null && !vsgExporter.InsideLODGroup)                                                                                            
                {
                    // rather than process the children we figure out which renderers are in which children and add them as LOD children
                    meshexported = ExportLODGroup(lodgroup, vsgExporter, meshRenderer, meshFilter, gotrans);
                }
                else
                {
                    // traverse any children
                    foreach (Transform child in gotrans)
                    {
                        processGameObject(child.gameObject, vsgExporter);
                    }
                }

                // does it have a mesh
                if (!meshexported && meshFilter && meshFilter.sharedMesh && meshRenderer)
                {
                    ExportMesh(meshFilter.sharedMesh, meshRenderer, gotrans, vsgExporter);
                }

                // does it contain a light component
                Light light = go.GetComponent<Light>();
                if (light != null) {
                    ExportLight(light);
                }

                // does this node have a terrain
                Terrain terrain = go.GetComponent<Terrain>();
                if (terrain) 
                {
                    ExportTerrainMesh(terrain, vsgExporter);
                }
            }
        }

        private static void ExportLight(Light light) 
        {
            if (LightConverter.CreateLightData(light, out LightData lightData)) 
            {
                //Debug.Log(lightData);
                using (new LightNode(lightData)) {}
            }
        }

        private static void ExportSkybox(Cubemap skyTexture)
        {
            ImageData skyboxData = TextureConverter.GetOrCreateImageData(skyTexture);
            DescriptorImageData skyboxDescriptor = MaterialConverter.GetOrCreateDescriptorImageData(skyboxData, 0);
            GraphBuilderInterface.unity2vsg_AddSkybox(skyboxDescriptor);
        }

        private static bool ExportLODGroup(LODGroup lodgroup, SceneGraphExporter vsgExporter, MeshRenderer meshRenderer, MeshFilter meshFilter, Transform gotrans) {
            var meshExported = false;
            LOD[] lods = lodgroup.GetLODs();
            if (lods.Length > 0 && lods[0].renderers.Length > 0)
            {
                using (var lodNode = vsgExporter.CreateLODNode(NativeUtils.GetCullData(gotrans.position, lods[0].renderers))) {
                    foreach (var lod in lods)
                    {
                        // for now just support one renderer and assume it's under a seperate child gameObject
                        if (lod.renderers.Length == 0) continue;

                        using (lodNode.AddLODChild(new LODChildData() { minimumScreenHeightRatio = lod.screenRelativeTransitionHeight })) {
                            foreach (Renderer lodrenderer in lod.renderers) {
                                if(lodrenderer == meshRenderer)
                                {
                                    meshExported = true;
                                    ExportMesh(meshFilter.sharedMesh, meshRenderer, gotrans, vsgExporter);
                                }
                                else if(lodrenderer != null)
                                {
                                    // now process the renderers gameobject, it'll be added to the group we just created by adding an LOD child
                                    processGameObject(lodrenderer.gameObject, vsgExporter);
                                }
                            }
                        }
                    }
                }
            }
            return meshExported;
        }

        private static void ExportMesh(Mesh mesh, MeshRenderer meshRenderer, Transform gotrans, SceneGraphExporter exporter)
        {
            using (exporter.BeginMeshExport(gotrans.position, meshRenderer)) {
                if (mesh != null && mesh.isReadable && mesh.vertexCount > 0 && mesh.GetIndexCount(0) > 0)
                {
                    MeshInfo meshInfo = MeshConverter.GetOrCreateMeshInfo(mesh);

                    // shader instance id, Material Data, sub mesh indicies
                    Dictionary<int, Dictionary<MaterialInfo, List<int>>> meshMaterials = 
                        MaterialConverter.ConvertMaterials(meshRenderer.sharedMaterials, meshInfo.submeshs.Count);

                    if (meshInfo.submeshs.Count > 1)
                    {
                        // create mesh data, if the mesh has already been created we only need to pass the ID to the addGeometry function
                        foreach (int shaderkey in meshMaterials.Keys)
                        {
                            List<MaterialInfo> mds = new List<MaterialInfo>(meshMaterials[shaderkey].Keys);

                            if (mds.Count == 0) continue;

                            using (var stateGroup = new StateGroupNode(exporter.CreateGraphicsPipeline(meshInfo, mds[0]))) {
                                using (var commands = new CommandsNode()) {
                                    commands.BindVertexBuffers(MeshConverter.GetOrCreateVertexBuffersData(meshInfo));
                                    commands.BindIndexBuffer(MeshConverter.GetOrCreateIndexBufferData(meshInfo));

                                    foreach (MaterialInfo md in mds)
                                    {
                                        using (var bindDescriptorSetCommand = commands.CreateBindDescriptorSetCommand())
                                            bindDescriptorSetCommand.AddDescriptors(md);

                                        foreach (int submeshIndex in meshMaterials[shaderkey][md])
                                            commands.DrawIndexed(MeshConverter.GetOrCreateDrawIndexedData(meshInfo, submeshIndex));
                                    }
                                }
                            }
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
                                using (var stateGroup = new StateGroupNode(exporter.CreateGraphicsPipeline(meshInfo, mds[0]))) {
                                    using (var bindDescriptorSetCommand = stateGroup.CreateBindDescriptorSetCommand())
                                        bindDescriptorSetCommand.AddDescriptors(mds[0]);

                                    VertexIndexDrawData vertexIndexDrawData = MeshConverter.GetOrCreateVertexIndexDrawData(meshInfo);
                                    using (new VertexIndexDrawNode(vertexIndexDrawData)) {}
                                }
                            }
                        }
                    }
                }
                else
                {
                    string reason = mesh == null ? "mesh is null." : (!mesh.isReadable ? "mesh '" + mesh.name + "' is not readable. Please enabled read/write in the models import settings." : "mesh '" + mesh.name + "' has an unknown error.");
                    NativeLog.WriteLine("ExportMesh: Unable to export mesh for gameobject " + gotrans.gameObject.name + ", " + reason);
                }
            }
        }

        private static void ExportTerrainMesh(Terrain terrain, SceneGraphExporter exporter)
        {
            TerrainConverter.TerrainInfo terrainInfo = TerrainConverter.CreateTerrainInfo(terrain, exporter.ExportSettings);
            
            if (terrainInfo == null || ((terrainInfo.diffuseTextureDatas.Count == 0 || terrainInfo.maskTextureDatas.Count == 0) && terrainInfo.customMaterial == null)) return;

            using (var stateGroup = new StateGroupNode(exporter.CreateGraphicsPipeline(terrainInfo))) {
                using (var bindDescriptorSetCommand = stateGroup.CreateBindDescriptorSetCommand()) {
                    if (terrainInfo.customMaterial == null) {
                        if (terrainInfo.diffuseTextureDatas.Count > 0)
                            bindDescriptorSetCommand.images.Add(MaterialConverter.GetOrCreateDescriptorImageData(terrainInfo.diffuseTextureDatas.ToArray(), 0));

                        if (terrainInfo.diffuseScales.Count > 0) {
                            bindDescriptorSetCommand.vectorArrays.Add(new DescriptorVectorArrayUniformData() {
                                binding = 2,
                                value = NativeUtils.WrapArray(terrainInfo.diffuseScales.ToArray())
                            });
                        }

                        bindDescriptorSetCommand.vectors.Add(new DescriptorVectorUniformData() {
                            binding = 3,
                            value = NativeUtils.ToNative(terrainInfo.terrainSize)
                        });

                        if (terrainInfo.maskTextureDatas.Count > 0)
                            bindDescriptorSetCommand.images.Add(MaterialConverter.GetOrCreateDescriptorImageData(terrainInfo.maskTextureDatas.ToArray(), 1));
                    } else {
                        bindDescriptorSetCommand.AddDescriptors(terrainInfo.customMaterial);
                    }
                }
                
                using (new VertexIndexDrawNode(MeshConverter.GetOrCreateVertexIndexDrawData(terrainInfo.terrainMesh))) {}
            }
        }
    }

}
