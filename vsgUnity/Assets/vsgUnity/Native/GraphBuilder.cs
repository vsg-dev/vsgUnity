using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


namespace vsgUnity.Native
{
    public static class GraphBuilder
    {
        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_BeginExport")]
        private static extern void unity2vsg_BeginExport();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_EndExport")]
        private static extern void unity2vsg_EndExport([MarshalAs(UnmanagedType.LPStr)] string saveFileName);


        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddGroup")]
        private static extern void unity2vsg_AddGroup();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddTransform")]
        private static extern void unity2vsg_AddTransform(TransformData transform);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddGeometry")]
        private static extern void unity2vsg_AddGeometry(MeshData mesh);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddStringValue")]
        private static extern void unity2vsg_AddStringValue([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string value);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddStateGroup")]
        private static extern void unity2vsg_AddStateGroup();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddBindGraphicsPipeline")]
        private static extern void unity2vsg_AddBindGraphicsPipeline(PipelineData pipeline);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_AddTexture")]
        private static extern void unity2vsg_AddTexture(TextureData texture);

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_BindDescriptors")]
        private static extern void unity2vsg_BindDescriptors();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_EndNode")]
        private static extern void unity2vsg_EndNode();

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_LaunchViewer")]
        private static extern void unity2vsg_LaunchViewer([MarshalAs(UnmanagedType.LPStr)] string fileName, int useCamData, CameraData camdata);


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

            Dictionary<int, MeshData> meshCache = new Dictionary<int, MeshData>();
            Dictionary<int, TextureData> textureCache = new Dictionary<int, TextureData>();

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
                    transformdata.matrix.data = new float[]
                    {
                        matrix[0, 0], matrix[0, 1], matrix[0, 2], matrix[0, 3],
                        matrix[1, 0], matrix[1, 1], matrix[1, 2], matrix[1, 3],
                        matrix[2, 0], matrix[2, 1], matrix[2, 2], matrix[2, 3],
                        matrix[3, 0], matrix[3, 1], matrix[3, 2], matrix[3, 3]
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
                    Material material = meshRenderer.sharedMaterial;

                    // gather textures
                    Dictionary<string, Texture> allTextures = NativeUtils.GetTexturesForMaterial(material);
                    List<Texture> textures = new List<Texture>();

                    if (allTextures.Count > 0)
                    {
                        Texture maintex = allTextures.ContainsKey("_MainTex") ? allTextures["_MainTex"] : null;

                        if (maintex)
                        {
                            NativeUtils.TextureSupportIssues issues = NativeUtils.GetSupportIssuesForTexture(maintex);
                            if (issues != NativeUtils.TextureSupportIssues.None)
                            {
                                Debug.LogWarning(NativeUtils.GetTextureSupportReport(issues, maintex));
                            }
                            else
                            {
                                textures.Add(maintex);
                            }
                        }
                    }


                    if (mesh != null && mesh.isReadable)
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

                            /*meshdata.tangents = new Vec3Array();
                            meshdata.tangents.data = mesh.tangents;
                            meshdata.tangents.length = meshdata.tangents.data.Length;*/

                            /*meshdata.colors = new ColorArray();
                            meshdata.colors.data = mesh.colors;
                            meshdata.colors.length = meshdata.colors.data.Length;*/

                            meshdata.uv0 = new Vec2Array();
                            meshdata.uv0.data = mesh.uv;
                            meshdata.uv0.length = meshdata.uv0.data.Length;

                            meshCache.Add(meshdata.id, meshdata);
                        }

                        // for now before we add a mesh insert a stategroup and add a pipeline
                        GraphBuilder.unity2vsg_AddStateGroup();

                        PipelineData pipelineData = NativeUtils.CreatePipeLineData(meshCache[meshdata.id]);
                        pipelineData.fragmentImageSamplerCount = textures.Count;

                        GraphBuilder.unity2vsg_AddBindGraphicsPipeline(pipelineData);

                        GraphBuilder.unity2vsg_AddGeometry(meshdata);
                        GraphBuilder.unity2vsg_EndNode(); // step out of geometry

                        // add textures
                        if(textures.Count > 0)
                        {
                            Texture maintex = textures[0];

                            if (maintex)
                            {
                                TextureData texdata;
                                if (textureCache.ContainsKey(maintex.GetInstanceID()))
                                {
                                    texdata = new TextureData();
                                    texdata.id = maintex.GetInstanceID();
                                }
                                else
                                {
                                    texdata = NativeUtils.CreateTextureData(maintex);
                                    textureCache.Add(maintex.GetInstanceID(), texdata);
                                }
                                texdata.channel = 0;
                                GraphBuilder.unity2vsg_AddTexture(texdata);

                                GraphBuilder.unity2vsg_BindDescriptors();
                            }
                        }

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

            foreach (GameObject go in gameObjects)
            {
                processGameObject(go);
            }

            //GraphBuilder.unity2vsg_EndNode(); // step out of convert coord system node

            GraphBuilder.unity2vsg_EndExport(saveFileName);
        }
    }

}
