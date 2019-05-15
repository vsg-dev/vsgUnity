
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace vsgUnity.Native
{
    public static class NativeTests
    {
        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_Tests_GetXValues")]
        private static extern NativeFloatArray
        unity2vsg_Tests_GetXValues(Vec2Array points);

        public static float[] GetXValues(Vector2[] points)
        {
            NativeFloatArray nativefloats = unity2vsg_Tests_GetXValues(Convert.FromLocal(points));
            FloatArray floatarray = Convert.FromNative(nativefloats);
            Memory.DeleteNativeObject(nativefloats.ptr, true);
            return floatarray.data;
        }

        //
        // Convert a mesh

        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_ExportMesh")]
        private static extern void
        unity2vsg_ExportMesh(MeshData mesh);

        public static void ExportMesh(Mesh umesh)
        {
            if (!umesh.isReadable)
            {
                Debug.LogWarning("ExportMesh: Unable to export mesh, mesh is not readable. Please enabled read/write in the models import settings.");
                return;
            }

            MeshData mesh = new MeshData();
            mesh.verticies = new Vec3Array();
            mesh.verticies.data = umesh.vertices;
            mesh.verticies.length = umesh.vertexCount;

            mesh.triangles = new IntArray();
            mesh.triangles.data = umesh.triangles;
            mesh.triangles.length = mesh.triangles.data.Length;

            mesh.normals = new Vec3Array();
            mesh.normals.data = umesh.normals;
            mesh.normals.length = umesh.vertexCount;

            unity2vsg_ExportMesh(mesh);
        }
    }
}
