//----------------------------------------------
//            vsgUnity: Native
// Writen by Thomas Hogarth
// DataTypes.cs
//----------------------------------------------

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace vsgUnity.Native
{

    public static class Library
    {
        public const string libraryName = "unity2vsgd";
    }

    //
    // Local Unity types, should match layout of types in unity2vg DataTypes.h, used to pass data from C# to native code
    //

	[StructLayout(LayoutKind.Sequential)]
	public struct IntArray
	{
		public int[] data;
		public int length;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FloatArray
	{
		public float[] data;
		public int length;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Vec2Array
	{
		public Vector2[] data;
		public int length;
	}

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec3Array
    {
        public Vector3[] data;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec4Array
    {
        public Vector4[] data;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MeshData
    {
        public int id;
        public Vec3Array verticies;
        public IntArray triangles;
        public Vec3Array normals;
        public Vec4Array colors;
        public Vec2Array uv0;
        public Vec2Array uv1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PipelineData
    {
        public int id;
        public int hasNormals;
        public int hasColors;
        public int uvChannelCount;
        public int vertexImageSamplerCount;
        public int fragmentImageSamplerCount;
        public int vertexUniformCount;
        public int fragmentUniformCount;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct TransformData
    {
        public FloatArray matrix;
    }

    //
    // Native types for data returned from native code to C#
    //

    [StructLayout(LayoutKind.Sequential)]
	public struct NativeIntArray
	{
		public IntPtr ptr;
		public int length;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NativeFloatArray
	{
		public IntPtr ptr;
		public int length;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NativeVec2Array
	{
		public IntPtr ptr;
		public int length;
	}

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeVec3Array
    {
        public IntPtr ptr;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeVec4Array
    {
        public IntPtr ptr;
        public int length;
    }

    public static class Memory
	{
#if UNITY_IPHONE
		[DllImport ("__Internal")]
#else
        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_DataTypes_DeleteNativeObject")]
#endif
        private static extern void unity2vsg_DataTypes_DeleteNativeObject(IntPtr anObjectPointer, bool isArray);

		public static void DeleteNativeObject(IntPtr anObjectPointer, bool isArray)
		{
            unity2vsg_DataTypes_DeleteNativeObject(anObjectPointer, isArray);
		}
	}

    public static class Convert
	{
	 	private static T[] CreateArray<T>(IntPtr array, int length)
		{
	         T[] result = new T[length];
	         int size = Marshal.SizeOf(typeof(T));
	 
	         if (IntPtr.Size == 4) {
	             // 32-bit system
	             for (int i = 0; i < result.Length; i++) {
	                 result [i] = (T)Marshal.PtrToStructure (array, typeof(T));
	                 array = new IntPtr (array.ToInt32 () + size);
	             }
	         } else {
	             // probably 64-bit system
	             for (int i = 0; i < result.Length; i++) {
	                 result [i] = (T)Marshal.PtrToStructure (array, typeof(T));
	                 array = new IntPtr(array.ToInt64 () + size);
	             }
	         }
	         return result;
     	}

		public static IntArray FromLocal(int[] anArray)
		{
			IntArray result;
			result.data = anArray;
			result.length = anArray.Length;
			return result;
		}

		public static FloatArray FromLocal(float[] anArray)
		{
			FloatArray result;
			result.data = anArray;
			result.length = anArray.Length;
			return result;
		}

		public static Vec2Array FromLocal(Vector2[] anArray)
		{
			Vec2Array result;
			result.data = anArray;
			result.length = anArray.Length;
			return result;
		}

        public static Vec3Array FromLocal(Vector3[] anArray)
        {
            Vec3Array result;
            result.data = anArray;
            result.length = anArray.Length;
            return result;
        }

        public static Vec4Array FromLocal(Vector4[] anArray)
        {
            Vec4Array result;
            result.data = anArray;
            result.length = anArray.Length;
            return result;
        }

        public static IntArray FromNative(NativeIntArray aNativeArray)
		{
			IntArray result;
			result.data = CreateArray<int>(aNativeArray.ptr, aNativeArray.length);
			result.length = result.data.Length;
			return result;
		}

		public static FloatArray FromNative(NativeFloatArray aNativeArray)
		{
			FloatArray result;
			result.data = CreateArray<float>(aNativeArray.ptr, aNativeArray.length);
			result.length = result.data.Length;
			return result;
		}

		public static Vec2Array FromNative(NativeVec2Array aNativeArray)
		{
			Vec2Array result;
			result.data = CreateArray<Vector2>(aNativeArray.ptr, aNativeArray.length);
			result.length = result.data.Length;
			return result;
		}

        public static Vec3Array FromNative(NativeVec3Array aNativeArray)
        {
            Vec3Array result;
            result.data = CreateArray<Vector3>(aNativeArray.ptr, aNativeArray.length);
            result.length = result.data.Length;
            return result;
        }

        public static Vec4Array FromNative(NativeVec4Array aNativeArray)
        {
            Vec4Array result;
            result.data = CreateArray<Vector4>(aNativeArray.ptr, aNativeArray.length);
            result.length = result.data.Length;
            return result;
        }
    }

}
