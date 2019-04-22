
using System.Runtime;
using System.Runtime.InteropServices;

namespace vsgUnity.Native
{
    public static class NativeTests
    {
        [DllImport("unity2vsgd", EntryPoint = "unity2vsg_Tests_GetXValues")]
        private static extern NativeFloatArray unity2vsg_Tests_GetXValues(Vec2Array points);

        public static float[] GetXValues(UnityEngine.Vector2[] points)
        {
            NativeFloatArray nativefloats = unity2vsg_Tests_GetXValues(Convert.FromLocal(points));
            FloatArray floatarray = Convert.FromNative(nativefloats);
            Memory.DeleteNativeObject(nativefloats.ptr, true);
            return floatarray.data;
        }


        //
        // Convert a mesh

        [DllImport("unity2vsgd", EntryPoint = "unity2vsg_ConvertMesh")]
        private static extern void unity2vsg_ConvertMesh(Mesh mesh);

        public static void ConvertMesh(UnityEngine.Mesh umesh)
        {
            Mesh mesh = new Mesh();
            mesh.verticies = new Vec3Array();
            mesh.verticies.data = umesh.vertices;
            mesh.verticies.length = umesh.vertexCount;

            mesh.triangles = new IntArray();
            mesh.triangles.data = umesh.triangles;
            mesh.triangles.length = mesh.triangles.data.Length;

            mesh.normals = new Vec3Array();
            mesh.normals.data = umesh.normals;
            mesh.normals.length = umesh.vertexCount;

            unity2vsg_ConvertMesh(mesh);
        }
    }
}