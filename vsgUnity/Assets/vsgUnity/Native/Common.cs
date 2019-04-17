//----------------------------------------------
//            vsgUnity: Native
// Writen by Thomas Hogarth
// Common.cs
//----------------------------------------------

using System;
using System.Runtime;
using System.Runtime.InteropServices;
using UnityEngine;

namespace vsgUnity.Native
{

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

	public static class Memory
	{
#if VSGUNITY_USE_NATIVE
	#if VSGUNITY_NATIVE_INTERNAL_IMPORT
		[DllImport ("__Internal")]
	#else
		[DllImport("vsgUnityNative", EntryPoint = "vsgUnity_Native_DeleteNativeObject")]
	#endif
		private static extern void vsgUnity_Native_DeleteNativeObject(IntPtr anObjectPointer, bool isArray);
#else
		private static void vsgUnity_Native_DeleteNativeObject(IntPtr anObjectPointer, bool isArray) { }
#endif

		public static void DeleteNativeObject(IntPtr anObjectPointer, bool isArray)
		{
			vsgUnity_Native_DeleteNativeObject(anObjectPointer, isArray);
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
	}

}
