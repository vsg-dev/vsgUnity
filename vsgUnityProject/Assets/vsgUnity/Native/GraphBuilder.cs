using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace vsgUnity.Native
{
    public static class GraphBuilder
    {
        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_BeginExport")]
        private static extern void  unity2vsg_BeginExport();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_EndExport")]
        private static extern void unity2vsg_EndExport([MarshalAs(UnmanagedType.LPStr)] string saveFileName);

        //
        // Nodes
        //

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddGroupNode")]
        private static extern void unity2vsg_AddGroupNode();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddTransformNode")]
        private static extern void unity2vsg_AddTransformNode(TransformData transform);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddCullNode")]
        private static extern void unity2vsg_AddCullNode(CullData cull);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddCullGroupNode")]
        private static extern void unity2vsg_AddCullGroupNode(CullData cull);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddLODNode")]
        private static extern void unity2vsg_AddLODNode(CullData cull);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddLODChild")]
        private static extern void unity2vsg_AddLODChild(LODChildData lodChildData);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddStateGroupNode")]
        private static extern void unity2vsg_AddStateGroupNode();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddCommandsNode")]
        private static extern void unity2vsg_AddCommandsNode();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddVertexIndexDrawNode")]
        private static extern void unity2vsg_AddVertexIndexDrawNode(VertexIndexDrawData mesh);

        //
        // Meta Data
        //

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddStringValue")]
        private static extern void unity2vsg_AddStringValue([ MarshalAs(UnmanagedType.LPStr) ] string name, [ MarshalAs(UnmanagedType.LPStr) ] string value);

        //
        // Commands
        //

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddBindGraphicsPipelineCommand", CallingConvention = CallingConvention.StdCall)]
        private static extern int unity2vsg_AddBindGraphicsPipelineCommand(PipelineData pipeline, int addToStateGroup);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddBindIndexBufferCommand")]
        private static extern void unity2vsg_AddBindIndexBufferCommand(IndexBufferData data);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddBindVertexBuffersCommand")]
        private static extern void unity2vsg_AddBindVertexBuffersCommand(VertexBuffersData data);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddDrawIndexedCommand")]
        private static extern void unity2vsg_AddDrawIndexedCommand(DrawIndexedData data);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_CreateBindDescriptorSetCommand")]
        private static extern void unity2vsg_CreateBindDescriptorSetCommand(int addToStateGroup);

        //
        // Descriptors
        //

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddDescriptorImage")]
        private static extern void unity2vsg_AddDescriptorImage(DescriptorImageData texture);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddTextureArrayDescriptor")]
        private static extern void unity2vsg_AddTextureArrayDescriptor(DescriptorImagesData textureArray);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddImageDataToActiveDescriptorImagesArray")]
        private static extern void unity2vsg_AddImageDataToActiveDescriptorImagesArray(ImageData texture);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_CreateDescriptorImagesFromActiveArray")]
        private static extern void unity2vsg_CreateDescriptorImagesFromActiveArray(DescriptorImagesData data);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_EndNode")]
        private static extern void unity2vsg_EndNode();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_LaunchViewer")]
        private static extern void unity2vsg_LaunchViewer([ MarshalAs(UnmanagedType.LPStr) ] string fileName, int useCamData, CameraData camdata);

        public static void LaunchViewer(string fileName, bool useCamData, Camera camera)
        {
            unity2vsg_LaunchViewer(fileName, useCamData ? 1 : 0, NativeUtils.CreateCameraData(camera));
        }

        public struct ExportSettings
        {
            public bool autoAddCullNodes;
            public string standardShaderMappingPath;
            public string standardTerrainShaderMappingPath;
        }

        public static void Export(GameObject[] gameObjects, string saveFileName, ExportSettings settings)
        {
            MeshConverter.ClearCaches();
            TextureConverter.ClearCaches();
            MaterialConverter.ClearCaches();

            GraphBuilder.unity2vsg_BeginExport();

            /*
            // add a root node to convert coord system
            TransformData convertData = new TransformData();
            convertData.matrix.data = new float[]
            {
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            };
            convertData.matrix.length = convertData.matrix.data.Length;

            // add root convert
            GraphBuilder.unity2vsg_AddTransform(convertData);
            */

            Dictionary<System.IntPtr, DescriptorImagesData> textureArrayCache = new Dictionary<System.IntPtr, DescriptorImagesData>();
            List<PipelineData> storePipelines = new List<PipelineData>();

            System.Action<GameObject> processGameObject = null;
            processGameObject = (GameObject go) =>
            {
                // determine the gameObject type
                Transform gotrans = go.transform;

                bool nodeAdded = false;

                // does it have a none identiy local matrix
                if (gotrans.localPosition != Vector3.zero || gotrans.localRotation != Quaternion.identity || gotrans.localScale != Vector3.one)
                {
                    Matrix4x4 convert = Matrix4x4.identity;
                    //convert[1, 1] = -1.0f;
                    //convert[2, 2] = -1.0f;

                    TransformData transformdata = new TransformData();
                    Matrix4x4 matrix = Matrix4x4.TRS(gotrans.localPosition, gotrans.localRotation, gotrans.localScale) * convert;
                    transformdata.matrix.data = new float[]{
                        matrix[0, 0], matrix[0, 1], matrix[0, 2], matrix[0, 3],
                        matrix[1, 0], matrix[1, 1], matrix[1, 2], matrix[1, 3],
                        matrix[2, 0], matrix[2, 1], matrix[2, 2], matrix[2, 3],
                        matrix[3, 0], matrix[3, 1], matrix[3, 2], matrix[3, 3]};
                    transformdata.matrix.length = transformdata.matrix.data.Length;

                    // add as a transform
                    GraphBuilder.unity2vsg_AddTransformNode(transformdata);
                    nodeAdded = true;
                }

                // do we need to insert a group
                if (!nodeAdded && gotrans.childCount > 0)
                {
                    //add as a group
                    GraphBuilder.unity2vsg_AddGroupNode();
                    nodeAdded = true;
                }

                // does it have a mesh
                MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();

                if (meshFilter && meshRenderer)
                {
                    bool addedCullGroup = false;
                    if (settings.autoAddCullNodes)
                    {
                        CullData culldata = new CullData();
                        culldata.center = meshRenderer.bounds.center;
                        culldata.radius = meshRenderer.bounds.size.magnitude;
                        GraphBuilder.unity2vsg_AddCullGroupNode(culldata);
                        addedCullGroup = true;
                    }

                    //
                    Mesh mesh = meshFilter.sharedMesh;
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
 
                            /*
                            GraphBuilder.unity2vsg_AddCommandsNode();

                            // bind verts was here

                            GraphBuilder.unity2vsg_EndNode();
                            */

                            // create mesh data, if the mesh has already been created we only need to pass the ID to the addGeometry function
                            foreach (int shaderkey in meshMaterials.Keys)
                            {
                                List<MaterialInfo> mds = new List<MaterialInfo>(meshMaterials[shaderkey].Keys);

                                if (mds.Count == 0) continue;

                                // add stategroup and pipeline for shader
                                GraphBuilder.unity2vsg_AddStateGroupNode();

                                PipelineData pipelineData = NativeUtils.CreatePipelineData(meshInfo); //WE NEED INFO ABOUT THE SHADER SO WE CAN BUILD A PIPLE LINE
                                pipelineData.descriptorBindings = NativeUtils.WrapArray(mds[0].descriptorBindings.ToArray());
                                pipelineData.shaderStages = mds[0].shaderStages.ToNative();
                                pipelineData.useAlpha = mds[0].useAlpha;
                                pipelineData.id = NativeUtils.ToNative(NativeUtils.GetIDForPipeline(pipelineData));
                                storePipelines.Add(pipelineData);

                                if (GraphBuilder.unity2vsg_AddBindGraphicsPipelineCommand(pipelineData, 1) == 1)
                                {

                                    GraphBuilder.unity2vsg_AddCommandsNode();

                                    VertexBuffersData vertexBuffersData = MeshConverter.GetOrCreateVertexBuffersData(meshInfo);
                                    GraphBuilder.unity2vsg_AddBindVertexBuffersCommand(vertexBuffersData);

                                    IndexBufferData indexBufferData = MeshConverter.GetOrCreateIndexBufferData(meshInfo);
                                    GraphBuilder.unity2vsg_AddBindIndexBufferCommand(indexBufferData);


                                    foreach (MaterialInfo md in mds)
                                    {
                                        foreach (DescriptorImageData t in md.imageDescriptors)
                                        {
                                            GraphBuilder.unity2vsg_AddDescriptorImage(t);
                                        }
                                        if (md.imageDescriptors.Count > 0) GraphBuilder.unity2vsg_CreateBindDescriptorSetCommand(0);

                                        foreach (int submeshIndex in meshMaterials[shaderkey][md])
                                        {
                                            DrawIndexedData drawIndexedData = MeshConverter.GetOrCreateDrawIndexedData(meshInfo, submeshIndex);
                                            GraphBuilder.unity2vsg_AddDrawIndexedCommand(drawIndexedData);
                                        }
                                    }

                                    GraphBuilder.unity2vsg_EndNode(); // step out of commands node for descriptors and draw indexed commands
                                }
                                GraphBuilder.unity2vsg_EndNode(); // step out of stategroup node for shader
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
                                    GraphBuilder.unity2vsg_AddStateGroupNode();

                                    PipelineData pipelineData = NativeUtils.CreatePipelineData(meshInfo); //WE NEED INFO ABOUT THE SHADER SO WE CAN BUILD A PIPLE LINE
                                    pipelineData.descriptorBindings = NativeUtils.WrapArray(mds[0].descriptorBindings.ToArray());
                                    pipelineData.shaderStages = mds[0].shaderStages.ToNative();
                                    pipelineData.useAlpha = mds[0].useAlpha;
                                    pipelineData.id = NativeUtils.ToNative(NativeUtils.GetIDForPipeline(pipelineData));
                                    storePipelines.Add(pipelineData);

                                    if (GraphBuilder.unity2vsg_AddBindGraphicsPipelineCommand(pipelineData, 1) == 1)
                                    {

                                        foreach (DescriptorImageData t in mds[0].imageDescriptors)
                                        {
                                            GraphBuilder.unity2vsg_AddDescriptorImage(t);
                                        }
                                        if (mds[0].imageDescriptors.Count > 0) GraphBuilder.unity2vsg_CreateBindDescriptorSetCommand(1);

                                        VertexIndexDrawData vertexIndexDrawData = MeshConverter.GetOrCreateVertexIndexDrawData(meshInfo);
                                        GraphBuilder.unity2vsg_AddVertexIndexDrawNode(vertexIndexDrawData);

                                        GraphBuilder.unity2vsg_EndNode(); // step out of vertex index draw node
                                    }
                                    GraphBuilder.unity2vsg_EndNode(); // step out of stategroup node
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("ExportMesh: Unable to export mesh, mesh is not readable. Please enabled read/write in the models import settings.");
                    }

                    if (addedCullGroup)
                    {
                        GraphBuilder.unity2vsg_EndNode();
                    }
                }

                // does this node have a terrain
                Terrain terrain = go.GetComponent<Terrain>();
                if(terrain != null)
                {
                    ExportTerrainMesh(terrain, settings, textureArrayCache, storePipelines);
                }

                // does this node have an LOD group
                LODGroup lodgroup = go.GetComponent<LODGroup>();
                if (lodgroup != null)
                {
                    // rather than process the children we figure out which renderers are in which children and add them as LOD children
                    LOD[] lods = lodgroup.GetLODs();
                    if (lods.Length > 0)
                    {
                        // get bounds from first renderer
                        if (lods[0].renderers.Length > 0)
                        {
                            CullData lodCullData = new CullData();
                            lodCullData.center = lods[0].renderers[0].bounds.center;
                            lodCullData.radius = lods[0].renderers[0].bounds.size.magnitude;
                            unity2vsg_AddLODNode(lodCullData);

                            for (int i = 0; i < lods.Length; i++)
                            {
                                // for now just support one renderer and assume it's under a seperate child gameObject
                                if (lods[i].renderers.Length == 0) continue;
                                LODChildData lodChild = new LODChildData();
                                lodChild.minimumScreenHeightRatio = lods[i].screenRelativeTransitionHeight;
                                unity2vsg_AddLODChild(lodChild);

                                // now process the renderers gameobject, it'll be added to the group we just created by adding an LOD child
                                processGameObject(lods[i].renderers[0].gameObject);

                                unity2vsg_EndNode();
                            }

                            unity2vsg_EndNode(); // end the lod node
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

                // if we added a group or transform step out
                if (nodeAdded)
                {
                    GraphBuilder.unity2vsg_EndNode();
                }
            };

            foreach(GameObject go in gameObjects)
            {
                processGameObject(go);
            }

            //GraphBuilder.unity2vsg_EndNode(); // step out of convert coord system node

            GraphBuilder.unity2vsg_EndExport(saveFileName);
        }

        private static void ExportTerrainMesh(Terrain terrain, ExportSettings settings, Dictionary<System.IntPtr, DescriptorImagesData> textureArrayCache = null, List<PipelineData> storePipelines = null)
        {
            TerrainShaderMapping terrainShaderMapping = null;

            if (terrain.materialType == Terrain.MaterialType.Custom)
            {
                // try and load a shader mapping file to match the custom terrain material
                terrainShaderMapping = ShaderMappingIO.ReadFromJsonFile<TerrainShaderMapping>(terrain.materialTemplate.shader);
            }
            else if (settings.standardTerrainShaderMappingPath != null)
            {
                // load the shader mapping file
                terrainShaderMapping = ShaderMappingIO.ReadFromJsonFile<TerrainShaderMapping>(ShaderMappingIO.GetPathForShaderMappingFile("DefaultTerrain"));
            }

            if (terrainShaderMapping == null)
            {
                Debug.Log("GraphBuilder: Failed to load Terrain Shader Mapping file for terrain '" + terrain.name + "'.");
                return;
            }


            int samplew = terrain.terrainData.heightmapWidth;
            int sampleh = terrain.terrainData.heightmapHeight;

            int cellw = terrain.terrainData.heightmapWidth - 1;
            int cellh = terrain.terrainData.heightmapHeight - 1;

            Vector3 size = terrain.terrainData.size;

            Vector2 cellsize = new Vector3(size.x / cellw, size.z / cellh);
            Vector2 uvcellsize = new Vector2(1.0f / cellw, 1.0f / cellh);

            float[,] terrainHeights = terrain.terrainData.GetHeights(0, 0, samplew, sampleh);

            int vertcount = samplew * sampleh;
            Vector3[] verts = new Vector3[vertcount];
            Vector3[] normals = new Vector3[vertcount];
            Vector2[] uvs = new Vector2[vertcount];

            int[] indicies = new int[(cellw * cellh) * 6];

            // Build vertices and UVs
            for (int y = 0; y < samplew; y++)
            {
                for (int x = 0; x < sampleh; x++)
                {
                    verts[y * samplew + x] = new Vector3(x * cellsize.x, terrainHeights[y, x] * size.y, y * cellsize.y);
                    normals[y * samplew + x] = terrain.terrainData.GetInterpolatedNormal((float)x / (float)samplew, (float)y / (float)sampleh);
                    uvs[y * samplew + x] = new Vector2(x * uvcellsize.x, y * uvcellsize.y);
                }
            }

            int index = 0;
            for (int y = 0; y < cellw; y++)
            {
                for (int x = 0; x < cellh; x++)
                {
                    indicies[index++] = (y * samplew) + x;
                    indicies[index++] = ((y + 1) * samplew) + x;
                    indicies[index++] = (y * samplew) + x + 1;

                    indicies[index++] = ((y + 1) * samplew) + x;
                    indicies[index++] = ((y + 1) * samplew) + x + 1;
                    indicies[index++] = (y * samplew) + x + 1;
                }
            }

            // add stategroup and pipeline for shader
            GraphBuilder.unity2vsg_AddStateGroupNode();

            PipelineData pipelineData = new PipelineData();
            pipelineData.hasNormals = 1;
            pipelineData.uvChannelCount = 1;
            pipelineData.useAlpha = 0;

            List<string> shaderDefines = new List<string>() { "VSG_LIGHTING" };
            List<VkDescriptorSetLayoutBinding> fragBindings = new List<VkDescriptorSetLayoutBinding>();
            List<int> shaderConsts = new List<int>();

            // convert layers into textures
            TerrainLayer[] layers = terrain.terrainData.terrainLayers;
            List<ImageData> layerDiffuseTextureDatas = new List<ImageData>();
            DescriptorImagesData layerDiffuseTextureArray = new DescriptorImagesData() { id = NativeUtils.ToNative(terrain.GetInstanceID().ToString()+"-diffuse"), channel = 0 };
            List<ImageData> layerMaskTextureDatas = new List<ImageData>();
            DescriptorImagesData layerMaskTextureArray = new DescriptorImagesData() { id = NativeUtils.ToNative(terrain.GetInstanceID().ToString()+"-mask"), channel = 1 };

            for(int i = 0; i < layers.Length; i++)
            {
                ImageData layerData = TextureConverter.GetOrCreateImageData(layers[i].diffuseTexture);
                layerDiffuseTextureDatas.Add(layerData);
            }
            Debug.Log("Alpha tex count: " + terrain.terrainData.alphamapTextureCount);
            for (int i = 0; i < terrain.terrainData.alphamapTextureCount; i++)
            {
                Texture2D srcmask = terrain.terrainData.GetAlphamapTexture(i);

                ImageData splatData = new ImageData();
                if (!TextureConverter.GetImageDataFromCache(srcmask.GetInstanceID(), out splatData))
                {
                    // alpha masks need to be converteds
                    RenderTexture rt = RenderTexture.GetTemporary(srcmask.width, srcmask.height, 0, RenderTextureFormat.ARGB32);
                    Graphics.Blit(srcmask, rt);
                    Texture2D convmask = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                    Graphics.SetRenderTarget(rt);
                    convmask.ReadPixels(new Rect(0.0f, 0.0f, rt.width, rt.height), 0, 0, false);
                    convmask.Apply(false, false);

                    splatData = TextureConverter.CreateImageData(convmask, false);
                    TextureConverter.AddImageDataToCache(splatData, srcmask.GetInstanceID());
                }

                layerMaskTextureDatas.Add(splatData);
            }


            if (layerDiffuseTextureDatas.Count > 0)
            {

                if (textureArrayCache != null) textureArrayCache.Add(layerDiffuseTextureArray.id, layerDiffuseTextureArray);
                fragBindings.Add(new VkDescriptorSetLayoutBinding() { binding = 0, descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, stageFlags = VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT, descriptorCount = (uint)layerDiffuseTextureDatas.Count });

                shaderConsts.Add(layerDiffuseTextureDatas.Count);

                shaderDefines.Add("VSG_TERRAIN_LAYERS");
            }
            
            if (layerMaskTextureDatas.Count > 0)
            {
                if (textureArrayCache != null) textureArrayCache.Add(layerMaskTextureArray.id, layerMaskTextureArray);
                fragBindings.Add(new VkDescriptorSetLayoutBinding() { binding = 1, descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, stageFlags = VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT, descriptorCount = (uint)layerMaskTextureDatas.Count });

                shaderConsts.Add(layerMaskTextureDatas.Count);
            }

            pipelineData.descriptorBindings = NativeUtils.WrapArray(fragBindings.ToArray());
            ShaderStagesInfo shaderStagesInfo = MaterialConverter.GetOrCreateShaderStagesInfo(terrainShaderMapping.shaders.ToArray(), string.Join(",", shaderDefines.ToArray()), shaderConsts.ToArray());
            pipelineData.shaderStages = shaderStagesInfo.ToNative();

            pipelineData.id = NativeUtils.ToNative(NativeUtils.GetIDForPipeline(pipelineData));
            storePipelines.Add(pipelineData);


            if (GraphBuilder.unity2vsg_AddBindGraphicsPipelineCommand(pipelineData, 1) == 1)
            {
                if(layerDiffuseTextureDatas.Count > 0)
                {
                    foreach (ImageData tex in layerDiffuseTextureDatas)
                    {
                        unity2vsg_AddImageDataToActiveDescriptorImagesArray(tex);
                    }
                    unity2vsg_CreateDescriptorImagesFromActiveArray(layerDiffuseTextureArray);
                }
                
                if (layerMaskTextureDatas.Count > 0)
                {
                    foreach (ImageData tex in layerMaskTextureDatas)
                    {
                        unity2vsg_AddImageDataToActiveDescriptorImagesArray(tex);
                    }
                    unity2vsg_CreateDescriptorImagesFromActiveArray(layerMaskTextureArray);
                }

                GraphBuilder.unity2vsg_CreateBindDescriptorSetCommand(1);
                

                MeshInfo mesh = null;
                if (!MeshConverter.GetMeshInfoFromCache(terrain.GetInstanceID(), out mesh))
                {
                    mesh = new MeshInfo
                    {
                        id = terrain.GetInstanceID(),
                        verticies = NativeUtils.WrapArray(verts),
                        normals = NativeUtils.WrapArray(normals),
                        uv0 = NativeUtils.WrapArray(uvs),
                        triangles = NativeUtils.WrapArray(indicies),
                        use32BitIndicies = 1
                    };

                    MeshConverter.AddMeshInfoToCache(mesh, terrain.GetInstanceID());
                }

                GraphBuilder.unity2vsg_AddVertexIndexDrawNode(MeshConverter.GetOrCreateVertexIndexDrawData(mesh));
                GraphBuilder.unity2vsg_EndNode(); // step out of vertex index draw node
            }
            GraphBuilder.unity2vsg_EndNode(); // step out of stategroup node

        }
    }

}
