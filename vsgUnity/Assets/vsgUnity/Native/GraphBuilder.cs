using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


namespace vsgUnity.Native
{
    public static class GraphBuilder
    {
        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_BeginExport")]
        private static extern void unity2vsg_BeginExport(string saveFileName);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_EndExport")]
        private static extern void unity2vsg_EndExport();


        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddGroup")]
        private static extern void unity2vsg_AddGroup();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddTransform")]
        private static extern void unity2vsg_AddTransform(TransformData transform);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddGeometry")]
        private static extern void unity2vsg_AddGeometry(MeshData mesh);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddStateGroup")]
        private static extern void unity2vsg_AddStateGroup();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddBindGraphicsPipeline")]
        private static extern void unity2vsg_AddBindGraphicsPipeline(PipelineData pipeline);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_EndNode")]
        private static extern void unity2vsg_EndNode();

        public static PipelineData CreatePipeLineData(MeshData meshData)
        {
            PipelineData pipeline = new PipelineData();
            pipeline.hasNormals = meshData.normals.length > 0 ? 1 : 0;
            pipeline.hasColors = meshData.colors.length > 0 ? 1 : 0;
            pipeline.uvChannelCount = meshData.uv0.length > 0 ? 1 : 0;
            return pipeline;
        }

        public static void Export(GameObject gameObject)
        {
            GraphBuilder.unity2vsg_BeginExport("hello");

            Dictionary<int, MeshData> meshCache = new Dictionary<int, MeshData>();

            System.Action<GameObject> processGameObject = null;
            processGameObject = (GameObject go) =>
            {
                // determine the gameObject type
                Transform gotrans = go.transform;

                bool nodeAdded = false;

                // does it have a none identiy local matrix
                if (gotrans.localPosition != Vector3.zero || gotrans.localRotation != Quaternion.identity || gotrans.localScale != Vector3.one)
                {
                    TransformData transformdata = new TransformData();
                    Matrix4x4 matrix = Matrix4x4.TRS(gotrans.localPosition, gotrans.localRotation, gotrans.localScale);
                    transformdata.matrix.data = new float[]
                    {
                        matrix.m00, matrix.m01, matrix.m02, matrix.m03,
                        matrix.m10, matrix.m11, matrix.m12, matrix.m13,
                        matrix.m20, matrix.m21, matrix.m22, matrix.m23,
                        matrix.m30, matrix.m31, matrix.m32, matrix.m33
                    };
                    transformdata.matrix.length = transformdata.matrix.data.Length;

                    // add as a transform
                    GraphBuilder.unity2vsg_AddTransform(transformdata);
                    nodeAdded = true;
                }

                // do we need to insert a group
                if (!nodeAdded && gotrans.childCount > 0)
                {
                    //add as a group
                    GraphBuilder.unity2vsg_AddGroup();
                    nodeAdded = true;
                }

                // does it have a mesh
                MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
                if (meshFilter && meshRenderer)
                {
                    Mesh mesh = meshFilter.sharedMesh;

                    if (mesh.isReadable)
                    {
                        // create mesh data, if the mesh has already been created we only need to pass the ID to the addGeometry function
                        MeshData meshdata = new MeshData();
                        meshdata.id = mesh.GetInstanceID();

                        if (!meshCache.ContainsKey(meshdata.id))
                        {
                            meshdata.verticies = new Vec3Array();
                            meshdata.verticies.data = mesh.vertices;
                            meshdata.verticies.length = mesh.vertexCount;

                            meshdata.triangles = new IntArray();
                            meshdata.triangles.data = mesh.triangles;
                            meshdata.triangles.length = meshdata.triangles.data.Length;

                            meshdata.normals = new Vec3Array();
                            meshdata.normals.data = mesh.normals;
                            meshdata.normals.length = meshdata.normals.data.Length;

                            meshCache.Add(meshdata.id, meshdata);
                        }

                        // for now before we add a mesh insert a stategroup and add a pipeline
                        GraphBuilder.unity2vsg_AddStateGroup();

                        PipelineData pipelineData = GraphBuilder.CreatePipeLineData(meshCache[meshdata.id]);
                        GraphBuilder.unity2vsg_AddBindGraphicsPipeline(pipelineData);

                        GraphBuilder.unity2vsg_AddGeometry(meshdata);
                        GraphBuilder.unity2vsg_EndNode(); // step out of geometry

                        GraphBuilder.unity2vsg_EndNode(); // step out of stategroup
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

            processGameObject(gameObject);

            GraphBuilder.unity2vsg_EndExport();
        }
    }

}
