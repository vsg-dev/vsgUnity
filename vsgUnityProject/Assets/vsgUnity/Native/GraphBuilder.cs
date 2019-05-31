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
        private static extern void unity2vsg_AddVertexIndexDrawNode(MeshData mesh);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddGeometryNode")]
        private static extern void unity2vsg_AddGeometryNode(MeshData mesh);

        //
        // Meta Data
        //

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddStringValue")]
        private static extern void unity2vsg_AddStringValue([ MarshalAs(UnmanagedType.LPStr) ] string name, [ MarshalAs(UnmanagedType.LPStr) ] string value);

        //
        // Commands
        //

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddBindGraphicsPipelineCommand")]
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

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddTextureDescriptor")]
        private static extern void unity2vsg_AddTextureDescriptor(TextureData texture);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddTextureArrayDescriptor")]
        private static extern void unity2vsg_AddTextureArrayDescriptor(TextureDataArray textureArray);

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
            public string terrainVertexShaderPath;
            public string terrainFragmentShaderPath;
        }

        public static void Export(GameObject[] gameObjects, string saveFileName, ExportSettings settings)
        {
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

            Dictionary<string, MeshData>meshCache = new Dictionary<string, MeshData>();
            Dictionary<string, IndexBufferData>indexCache = new Dictionary<string, IndexBufferData>();
            Dictionary<string, VertexBuffersData>vertexCache = new Dictionary<string, VertexBuffersData>();
            Dictionary<string, DrawIndexedData>drawCache = new Dictionary<string, DrawIndexedData>();
            Dictionary<string, TextureData>textureCache = new Dictionary<string, TextureData>();
            Dictionary<string, TextureDataArray> textureArrayCache = new Dictionary<string, TextureDataArray>();
            Dictionary<string, MaterialData>materialCache = new Dictionary<string, MaterialData>();

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
                        string meshidstr = mesh.GetInstanceID().ToString();

                        MeshData fullMeshData;
                        if (meshCache.ContainsKey(meshidstr))
                        {
                            fullMeshData = meshCache[meshidstr];
                        }
                        else
                        {
                            fullMeshData = NativeUtils.CreateMeshData(mesh, -1);
                            meshCache.Add(meshidstr, fullMeshData);
                        }

                        int subMeshCount = mesh.subMeshCount;

                        // shader instance id, Material Data, sub mesh indicies
                        Dictionary<string, Dictionary<MaterialData, List<int>>> meshMaterials = new Dictionary<string, Dictionary<MaterialData, List<int>>>();
                        for (int matindex = 0; matindex < materials.Length && matindex < subMeshCount; matindex++)
                        {
                            Material mat = materials[matindex];
                            if (mat == null) continue;

                            string matid = mat.GetInstanceID().ToString();
                            string matshaderid = NativeUtils.GetShaderIDForMaterial(mat);

                            MaterialData matdata;

                            if (materialCache.ContainsKey(matid))
                            {
                                matdata = materialCache[matid];
                            }
                            else
                            {
                                matdata = NativeUtils.CreateMaterialData(mat, ref textureCache);
                            }

                            if (!meshMaterials.ContainsKey(matshaderid)) meshMaterials.Add(matshaderid, new Dictionary<MaterialData, List<int>>());
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
                            foreach (string shaderkey in meshMaterials.Keys)
                            {
                                List<MaterialData> mds = new List<MaterialData>(meshMaterials[shaderkey].Keys);

                                if (mds.Count == 0) continue;

                                // add stategroup and pipeline for shader
                                GraphBuilder.unity2vsg_AddStateGroupNode();

                                PipelineData pipelineData = NativeUtils.CreatePipelineData(fullMeshData); //WE NEED INFO ABOUT THE SHADER SO WE CAN BUILD A PIPLE LINE
                                pipelineData.vertexDescriptorBindings.data = mds[0].vertexDescriptorBindings;
                                pipelineData.vertexDescriptorBindings.length = mds[0].vertexDescriptorBindings.Length;
                                pipelineData.fragmentDescriptorBindings.data = mds[0].fragmentDescriptorBindings;
                                pipelineData.fragmentDescriptorBindings.length = mds[0].fragmentDescriptorBindings.Length;
                                pipelineData.shader.customDefines = mds[0].customDefines == null ? "" : string.Join(",", mds[0].customDefines);
                                pipelineData.useAlpha = mds[0].useAlpha;
                                pipelineData.id = NativeUtils.GetIDForPipeline(pipelineData);

                                if (GraphBuilder.unity2vsg_AddBindGraphicsPipelineCommand(pipelineData, 1) == 1)
                                {

                                    GraphBuilder.unity2vsg_AddCommandsNode();

                                    VertexBuffersData vertexBuffersData;
                                    if (vertexCache.ContainsKey(meshidstr))
                                    {
                                        vertexBuffersData = vertexCache[meshidstr];
                                    }
                                    else
                                    {
                                        vertexBuffersData = new VertexBuffersData();
                                        vertexBuffersData.id = mesh.GetInstanceID().ToString();
                                        vertexBuffersData.verticies = fullMeshData.verticies;
                                        vertexBuffersData.normals = fullMeshData.normals;
                                        vertexBuffersData.uv0 = fullMeshData.uv0;
                                        vertexCache.Add(meshidstr, vertexBuffersData);
                                    }

                                    GraphBuilder.unity2vsg_AddBindVertexBuffersCommand(vertexBuffersData);

                                    IndexBufferData indexBufferData;

                                    if (indexCache.ContainsKey(meshidstr))
                                    {
                                        indexBufferData = indexCache[meshidstr];
                                    }
                                    else
                                    {
                                        indexBufferData = new IndexBufferData();
                                        indexBufferData.id = mesh.GetInstanceID().ToString();
                                        indexBufferData.triangles.data = mesh.triangles;
                                        indexBufferData.triangles.length = indexBufferData.triangles.data.Length;
                                        indexBufferData.use32BitIndicies = fullMeshData.use32BitIndicies;
                                        indexCache.Add(meshidstr, indexBufferData);
                                    }

                                    GraphBuilder.unity2vsg_AddBindIndexBufferCommand(indexBufferData);


                                    foreach (MaterialData md in mds)
                                    {
                                        foreach (TextureData t in md.textures)
                                        {
                                            GraphBuilder.unity2vsg_AddTextureDescriptor(t);
                                        }
                                        if (md.textures.Length > 0) GraphBuilder.unity2vsg_CreateBindDescriptorSetCommand(0);

                                        foreach (int submeshIndex in meshMaterials[shaderkey][md])
                                        {
                                            DrawIndexedData drawIndexedData = new DrawIndexedData();
                                            drawIndexedData.id = mesh.GetInstanceID().ToString() + "-" + submeshIndex.ToString();
                                            drawIndexedData.indexCount = (int)mesh.GetIndexCount(submeshIndex);
                                            drawIndexedData.firstIndex = (int)mesh.GetIndexStart(submeshIndex);
                                            //Debug.Log("index count: " + mesh.GetIndexCount(submeshIndex).ToString() + " first index: " + mesh.GetIndexStart(submeshIndex).ToString() + " total indicies " + indexBufferData.triangles.length);
                                            drawIndexedData.instanceCount = 1;

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
                            List<string> sids = new List<string>(meshMaterials.Keys);
                            if (sids.Count > 0)
                            {
                                List<MaterialData> mds = new List<MaterialData>(meshMaterials[sids[0]].Keys);

                                if (mds.Count > 0)
                                {
                                    // add stategroup and pipeline for shader
                                    GraphBuilder.unity2vsg_AddStateGroupNode();

                                    PipelineData pipelineData = NativeUtils.CreatePipelineData(fullMeshData); //WE NEED INFO ABOUT THE SHADER SO WE CAN BUILD A PIPLE LINE
                                    pipelineData.vertexDescriptorBindings.data = mds[0].vertexDescriptorBindings;
                                    pipelineData.vertexDescriptorBindings.length = mds[0].vertexDescriptorBindings.Length;
                                    pipelineData.fragmentDescriptorBindings.data = mds[0].fragmentDescriptorBindings;
                                    pipelineData.fragmentDescriptorBindings.length = mds[0].fragmentDescriptorBindings.Length;
                                    pipelineData.shader.customDefines = mds[0].customDefines == null ? "" : string.Join(",", mds[0].customDefines);
                                    pipelineData.useAlpha = mds[0].useAlpha;
                                    pipelineData.id = NativeUtils.GetIDForPipeline(pipelineData);

                                    if (GraphBuilder.unity2vsg_AddBindGraphicsPipelineCommand(pipelineData, 1) == 1)
                                    {

                                        foreach (TextureData t in mds[0].textures)
                                        {
                                            GraphBuilder.unity2vsg_AddTextureDescriptor(t);
                                        }
                                        if (mds[0].textures.Length > 0) GraphBuilder.unity2vsg_CreateBindDescriptorSetCommand(1);

                                        GraphBuilder.unity2vsg_AddVertexIndexDrawNode(fullMeshData);

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
                    ExportTerrainMesh(terrain, settings, meshCache, textureArrayCache);
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

        private static void ExportTerrainMesh(Terrain terrain, ExportSettings settings, Dictionary<string, MeshData> meshCache = null, Dictionary<string, TextureDataArray> textureArrayCache = null)
        {
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
            List<DescriptorBinding> fragBindings = new List<DescriptorBinding>();

            // convert layers into textures
            TerrainLayer[] layers = terrain.terrainData.terrainLayers;
            List<TextureData> textureDatas = new List<TextureData>();
            TextureDataArray textureArray = new TextureDataArray() { length = 0 };
            foreach (TerrainLayer layer in layers)
            {
                textureDatas.Add(NativeUtils.CreateTextureData(layer.diffuseTexture, 0));
            }
            if(textureDatas.Count > 0)
            {
                fragBindings.Add(new DescriptorBinding() { index = 0, type = VkDescriptorType.VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, count = textureDatas.Count });
                shaderDefines.Add("VSG_DIFFUSE_MAP");
                textureArray.data = textureDatas.ToArray();
                textureArray.length = textureArray.data.Length;
                if (textureArrayCache != null) textureArrayCache.Add(terrain.GetInstanceID().ToString(), textureArray);

                pipelineData.shader.fragmentSpecializationData.data = new uint[] { 1 };
                pipelineData.shader.fragmentSpecializationData.length = 1;
            }

            pipelineData.fragmentDescriptorBindings.data = fragBindings.ToArray();
            pipelineData.fragmentDescriptorBindings.length = pipelineData.fragmentDescriptorBindings.data.Length;

            pipelineData.shader.customDefines = string.Join(",", shaderDefines.ToArray());

            // use custom shader if present
            if (!string.IsNullOrEmpty(settings.terrainVertexShaderPath) && !string.IsNullOrEmpty(settings.terrainFragmentShaderPath))
            {
                pipelineData.shader.vertexSource = settings.terrainVertexShaderPath;
                pipelineData.shader.fragmentSource = settings.terrainFragmentShaderPath;
            }

            pipelineData.id = NativeUtils.GetIDForPipeline(pipelineData);

            if (GraphBuilder.unity2vsg_AddBindGraphicsPipelineCommand(pipelineData, 1) == 1)
            {

                if(textureArray.length > 0)
                {
                    GraphBuilder.unity2vsg_AddTextureArrayDescriptor(textureArray);
                    GraphBuilder.unity2vsg_CreateBindDescriptorSetCommand(1);
                }

                MeshData mesh = new MeshData();
                mesh.id = terrain.GetInstanceID().ToString();
                mesh.verticies.data = verts;
                mesh.verticies.length = vertcount;
                mesh.normals.data = normals;
                mesh.normals.length = vertcount;
                mesh.uv0.data = uvs;
                mesh.uv0.length = vertcount;
                mesh.triangles.data = indicies;
                mesh.triangles.length = indicies.Length;
                mesh.use32BitIndicies = 1;

                if (meshCache != null)
                {
                    meshCache.Add(mesh.id, mesh);
                }

                GraphBuilder.unity2vsg_AddVertexIndexDrawNode(mesh);
                GraphBuilder.unity2vsg_EndNode(); // step out of vertex index draw node
            }
            GraphBuilder.unity2vsg_EndNode(); // step out of stategroup node

        }
    }

}
