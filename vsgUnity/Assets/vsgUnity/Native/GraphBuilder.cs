using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace vsgUnity.Native
{
    public static class GraphBuilder
    {
        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_BeginExport")]
        private static extern void
        unity2vsg_BeginExport();

        //
        // Nodes
        //

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_EndExport")]
        private static extern void unity2vsg_EndExport([ MarshalAs(UnmanagedType.LPStr) ] string saveFileName);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddGroupNode")]
        private static extern void unity2vsg_AddGroupNode();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddTransformNode")]
        private static extern void unity2vsg_AddTransformNode(TransformData transform);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddCullNode")]
        private static extern void unity2vsg_AddCullNode();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddCullGroupNode")]
        private static extern void unity2vsg_AddCullGroupNode();

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
        private static extern void unity2vsg_AddBindGraphicsPipelineCommand(PipelineData pipeline, int addToStateGroup);

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

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_EndNode")]
        private static extern void unity2vsg_EndNode();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_LaunchViewer")]
        private static extern void unity2vsg_LaunchViewer([ MarshalAs(UnmanagedType.LPStr) ] string fileName, int useCamData, CameraData camdata);

        public static void LaunchViewer(string fileName, bool useCamData, Camera camera)
        {
            unity2vsg_LaunchViewer(fileName, useCamData ? 1 : 0, NativeUtils.CreateCameraData(camera));
        }

        public static void Export(GameObject[] gameObjects, string saveFileName)
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
            Dictionary<string, MaterialData>materialCache = new Dictionary<string, MaterialData>();

            System.Action<GameObject>processGameObject = null;
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
                        Dictionary<string, Dictionary<MaterialData, List<int>>>meshMaterials = new Dictionary<string, Dictionary<MaterialData, List<int>>>();
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

                            meshMaterials [matshaderid]
                            [matdata]
                                .Add(matindex);
                        }

                        if (subMeshCount > 0)
                        {
                            // create mesh data, if the mesh has already been created we only need to pass the ID to the addGeometry function
                            foreach(string shaderkey in meshMaterials.Keys)
                            {
                                List<MaterialData>mds = new List<MaterialData>(meshMaterials[shaderkey].Keys);

                                if (mds.Count == 0) continue;

                                // add stategroup and pipeline for shader
                                GraphBuilder.unity2vsg_AddStateGroupNode();

                                PipelineData pipelineData = NativeUtils.CreatePipelineData(fullMeshData); //WE NEED INFO ABOUT THE SHADER SO WE CAN BUILD A PIPLE LINE
                                pipelineData.fragmentImageSamplerCount = mds[0].textures.Length;
                                pipelineData.id = NativeUtils.GetIDForPipeline(pipelineData);

                                GraphBuilder.unity2vsg_AddBindGraphicsPipelineCommand(pipelineData, 1);

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
                                    indexCache.Add(meshidstr, indexBufferData);
                                }

                                GraphBuilder.unity2vsg_AddBindIndexBufferCommand(indexBufferData);

                                foreach(MaterialData md in mds)
                                {
                                    foreach(TextureData t in md.textures)
                                    {
                                        GraphBuilder.unity2vsg_AddTextureDescriptor(t);
                                    }
                                    if (md.textures.Length > 0) GraphBuilder.unity2vsg_CreateBindDescriptorSetCommand(0);

                                    foreach(int submeshIndex in meshMaterials [shaderkey]
                                            [md])
                                    {
                                        DrawIndexedData drawIndexedData = new DrawIndexedData();
                                        drawIndexedData.id = mesh.GetInstanceID().ToString() + "-" + submeshIndex.ToString();
                                        drawIndexedData.indexCount = (int) mesh.GetIndexCount(submeshIndex);
                                        drawIndexedData.firstIndex = (int) mesh.GetIndexStart(submeshIndex);
                                        //Debug.Log("index count: " + mesh.GetIndexCount(submeshIndex).ToString() + " first index: " + mesh.GetIndexStart(submeshIndex).ToString() + " total indicies " + indexBufferData.triangles.length);
                                        drawIndexedData.instanceCount = 1;

                                        GraphBuilder.unity2vsg_AddDrawIndexedCommand(drawIndexedData);

                                        /*MeshData meshdata = new MeshData();
                                        meshdata.id = mesh.GetInstanceID().ToString() + "-" + submeshIndex.ToString();

                                        if (!meshCache.ContainsKey(meshdata.id))
                                        {
                                            meshdata.verticies = new Vec3Array();
                                            meshdata.verticies.data = mesh.vertices;
                                            meshdata.verticies.length = mesh.vertexCount;

                                            meshdata.triangles = new IntArray();
                                            meshdata.triangles.data = mesh.GetTriangles(submeshIndex);
                                            meshdata.triangles.length = meshdata.triangles.data.Length;

                                            meshdata.normals = new Vec3Array();
                                            meshdata.normals.data = mesh.normals;
                                            meshdata.normals.length = meshdata.normals.data.Length;

                                            /*meshdata.tangents = new Vec3Array();
                                            meshdata.tangents.data = mesh.tangents;
                                            meshdata.tangents.length = meshdata.tangents.data.Length;*/

                                        /*meshdata.colors = new ColorArray();
                                        meshdata.colors.data = mesh.colors;
                                        meshdata.colors.length = meshdata.colors.data.Length;*/

                                        /*     meshdata.uv0 = new Vec2Array();
                                             meshdata.uv0.data = mesh.uv;
                                             meshdata.uv0.length = meshdata.uv0.data.Length;

                                             meshCache.Add(meshdata.id, meshdata);
                                         }*/

                                        //GraphBuilder.unity2vsg_AddGeometryNode(meshdata);
                                        //GraphBuilder.unity2vsg_EndNode(); // step out of geometry
                                    }
                                }
                                GraphBuilder.unity2vsg_EndNode(); // step out of stategroup/commands node
                                GraphBuilder.unity2vsg_EndNode(); // step out of stategroup/commands node
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("ExportMesh: Unable to export mesh, mesh is not readable. Please enabled read/write in the models import settings.");
                    }
                }

                // transverse any children
                for (int i = 0; i < gotrans.childCount; i++)
                {
                    processGameObject(gotrans.GetChild(i).gameObject);
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
    }

}
