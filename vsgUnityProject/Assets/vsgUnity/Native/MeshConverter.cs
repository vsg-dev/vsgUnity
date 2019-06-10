using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using vsgUnity.Native;

namespace vsgUnity
{
    public class MeshInfo : IEquatable<MeshInfo>
    {
        public int id;
        public Vec3Array verticies;
        public Vec3Array normals;
        public Vec4Array tangents;
        public ColorArray colors;
        public Vec2Array uv0;
        public Vec2Array uv1;

        public IntArray triangles;
        public int use32BitIndicies;

        public struct SubMesh : IEquatable<SubMesh>
        {
            public uint firstIndex;
            public uint indexCount;

            public bool Equals(SubMesh b)
            {
                return firstIndex == b.firstIndex && indexCount == b.indexCount;
            }
        }
        public List<SubMesh> submeshs = new List<SubMesh>();
        

        public bool Equals(MeshInfo b)
        {
            return use32BitIndicies == b.use32BitIndicies &&
                verticies.Equals(b.verticies) &&
                normals.Equals(b.normals) &&
                tangents.Equals(b.tangents) &&
                colors.Equals(b.colors) &&
                uv0.Equals(b.uv0) &&
                uv1.Equals(b.uv1) &&
                triangles.Equals(b.triangles) &&
                submeshs.SequenceEqual<SubMesh>(b.submeshs);
        }
    }

    public static class MeshConverter
    {
        // mesh info, index buffers and vertex draw indexed always use the same id as the unity mesh they're based off
        public static Dictionary<int, MeshInfo> _meshInfoCache = new Dictionary<int, MeshInfo>();
        public static Dictionary<int, IndexBufferData> _indexBufferDataCache = new Dictionary<int, IndexBufferData>();
        public static Dictionary<int, VertexIndexDrawData> _vertexIndexDrawDataCache = new Dictionary<int, VertexIndexDrawData>();

        // vertex buffers needs an id based off the unity mesh they come from (need to extend to account for shaders using only a subset of attributes)
        public static Dictionary<int, VertexBuffersData> _vertexBuffersDataCache = new Dictionary<int, VertexBuffersData>();

        // draw indexed needs and id based off the unity mesh id and submesh index they represent
        public static Dictionary<int, Dictionary<int, DrawIndexedData>> _drawIndexedDataCache = new Dictionary<int, Dictionary<int, DrawIndexedData>>();
        static int _drawIndexedIDCount = 0;

        public static void ClearCaches()
        {
            _meshInfoCache.Clear();
            _indexBufferDataCache.Clear();
            _vertexIndexDrawDataCache.Clear();
            _vertexBuffersDataCache.Clear();
            _drawIndexedDataCache.Clear();
            _drawIndexedIDCount = 0;
        }

        public static MeshInfo GetOrCreateMeshInfo(Mesh mesh)
        {
            if (_meshInfoCache.ContainsKey(mesh.GetInstanceID()))
            {
                return _meshInfoCache[mesh.GetInstanceID()];
            }
            else
            {
                return CreateMeshInfo(mesh);
            }
        }

        public static MeshInfo CreateMeshInfo(Mesh mesh)
        {
            MeshInfo meshinfo = new MeshInfo();
            meshinfo.id = mesh.GetInstanceID();

            meshinfo.triangles = NativeUtils.WrapArray(mesh.triangles);
            meshinfo.use32BitIndicies = mesh.indexFormat == IndexFormat.UInt32 ? 1 : 0;

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                MeshInfo.SubMesh submesh = new MeshInfo.SubMesh
                {
                    firstIndex = mesh.GetIndexStart(i),
                    indexCount = mesh.GetIndexCount(i)
                };
                meshinfo.submeshs.Add(submesh);
            }

            meshinfo.verticies = NativeUtils.WrapArray(mesh.vertices);
            meshinfo.normals = NativeUtils.WrapArray(mesh.normals);
            meshinfo.tangents = NativeUtils.WrapArray(mesh.tangents);
            meshinfo.colors = NativeUtils.WrapArray(mesh.colors);
            meshinfo.uv0 = NativeUtils.WrapArray(mesh.uv);
            meshinfo.uv1 = NativeUtils.WrapArray(mesh.uv2);

            _meshInfoCache[meshinfo.id] = meshinfo;

            return meshinfo;
        }

        public static bool CacheContainsMeshInfoWithID(int cacheID)
        {
            return _meshInfoCache.ContainsKey(cacheID);
        }

        public static bool GetMeshInfoFromCache(int cacheID, out MeshInfo result)
        {
            if (_meshInfoCache.ContainsKey(cacheID))
            {
                result = _meshInfoCache[cacheID];
                return true;
            }
            result = null;
            return false;
        }

        public static void AddMeshInfoToCache(MeshInfo meshInfo, int cacheID)
        {
            _meshInfoCache[cacheID] = meshInfo;
        }

        public static IndexBufferData GetOrCreateIndexBufferData(MeshInfo meshInfo)
        {
            if (_indexBufferDataCache.ContainsKey(meshInfo.id))
            {
                return _indexBufferDataCache[meshInfo.id];
            }
            else
            {
                return CreateIndexBufferData(meshInfo);
            }
        }

        public static IndexBufferData CreateIndexBufferData(MeshInfo meshInfo)
        {
            IndexBufferData indexBufferData = new IndexBufferData();
            indexBufferData.id = meshInfo.id;
            indexBufferData.triangles = meshInfo.triangles;
            indexBufferData.use32BitIndicies = meshInfo.use32BitIndicies;

            _indexBufferDataCache[meshInfo.id] = indexBufferData;

            return indexBufferData;
        }

        public static VertexBuffersData GetOrCreateVertexBuffersData(MeshInfo meshInfo)
        {
            if (_vertexBuffersDataCache.ContainsKey(meshInfo.id))
            {
                return _vertexBuffersDataCache[meshInfo.id];
            }
            else
            {
                return CreateVertexBuffersData(meshInfo);
            }
        }

        public static VertexBuffersData CreateVertexBuffersData(MeshInfo meshInfo)
        {
            VertexBuffersData vertexBuffers = new VertexBuffersData();
            vertexBuffers.id = meshInfo.id;

            vertexBuffers.verticies = meshInfo.verticies;
            vertexBuffers.normals = meshInfo.normals;
            vertexBuffers.tangents = meshInfo.tangents;
            vertexBuffers.colors = meshInfo.colors;
            vertexBuffers.uv0 = meshInfo.uv0;
            vertexBuffers.uv1 = meshInfo.uv1;

            _vertexBuffersDataCache[meshInfo.id] = vertexBuffers;

            return vertexBuffers;
        }

        public static VertexIndexDrawData GetOrCreateVertexIndexDrawData(MeshInfo meshInfo)
        {
            if (_vertexIndexDrawDataCache.ContainsKey(meshInfo.id))
            {
                return _vertexIndexDrawDataCache[meshInfo.id];
            }
            else
            {
                return CreateVertexIndexDrawData(meshInfo);
            }
        }

        public static VertexIndexDrawData CreateVertexIndexDrawData(MeshInfo meshInfo)
        {
            VertexIndexDrawData vertexIndexDrawData = new VertexIndexDrawData();
            vertexIndexDrawData.id = meshInfo.id;
            vertexIndexDrawData.triangles = meshInfo.triangles;
            vertexIndexDrawData.use32BitIndicies = meshInfo.use32BitIndicies;

            vertexIndexDrawData.verticies = meshInfo.verticies;
            vertexIndexDrawData.normals = meshInfo.normals;
            vertexIndexDrawData.tangents = meshInfo.tangents;
            vertexIndexDrawData.colors = meshInfo.colors;
            vertexIndexDrawData.uv0 = meshInfo.uv0;
            vertexIndexDrawData.uv1 = meshInfo.uv1;

            _vertexIndexDrawDataCache[meshInfo.id] = vertexIndexDrawData;

            return vertexIndexDrawData;
        }

        public static DrawIndexedData GetOrCreateDrawIndexedData(MeshInfo meshInfo, int submeshIndex)
        {
            if (_drawIndexedDataCache.ContainsKey(meshInfo.id) && _drawIndexedDataCache[meshInfo.id].ContainsKey(submeshIndex))
            {
                return _drawIndexedDataCache[meshInfo.id][submeshIndex];
            }
            else
            {
                return CreateDrawIndexedData(meshInfo, submeshIndex);
            }
        }

        public static DrawIndexedData CreateDrawIndexedData(MeshInfo meshInfo, int submeshIndex)
        {
            DrawIndexedData drawIndexedData = new DrawIndexedData();
            if (submeshIndex >= meshInfo.submeshs.Count) return drawIndexedData;

            drawIndexedData.id = _drawIndexedIDCount;
            drawIndexedData.indexCount = meshInfo.submeshs[submeshIndex].indexCount;
            drawIndexedData.firstIndex = meshInfo.submeshs[submeshIndex].firstIndex;

            drawIndexedData.instanceCount = 1;
            drawIndexedData.firstInstance = 0;

            if (!_drawIndexedDataCache.ContainsKey(meshInfo.id)) _drawIndexedDataCache.Add(meshInfo.id, new Dictionary<int, DrawIndexedData>());

            _drawIndexedDataCache[meshInfo.id][submeshIndex] = drawIndexedData;
            _drawIndexedIDCount++;

            return drawIndexedData;
        }
    }

}
