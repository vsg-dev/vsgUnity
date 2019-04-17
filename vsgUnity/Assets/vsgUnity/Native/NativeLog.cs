//----------------------------------------------
//            vsgUnity: Native
// Writen by Thomas Hogarth
// NativeLog.cs
//----------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace vsgUnity.Native
{
	public static class NativeLog
	{
#if VSGUNITY_USE_NATIVE
	#if VSGUNITY_NATIVE_INTERNAL_IMPORT
		[DllImport ("__Internal")]
	#else
		[DllImport("vsgUnityNative", EntryPoint = "vsgUnity_Native_SetDebugLogCallback")]
	#endif
		private static extern void vsgUnity_Native_SetDebugLogCallback(IntPtr aCallbackFuntion);
#else
		private static void vsgUnity_Native_SetDebugLogCallback(IntPtr aCallbackFuntion) { }
#endif

		delegate void DebugLogDelegate(string aMessageString);

		[MonoPInvokeCallback (typeof (DebugLogDelegate))]
		public static void InstallDebugLogCallback()
		{
#if VSGUNITY_USE_NATIVE
			DebugLogDelegate callbackdelegate = new DebugLogDelegate(WriteLogLineDelegate);
			 
			// Convert delegate into a function pointer to pass to native plugin
			IntPtr callbackpointer = Marshal.GetFunctionPointerForDelegate(callbackdelegate);
			  
			// Pass the pointer to native plugin
			vsgUnity_Native_SetDebugLogCallback(callbackpointer);
#endif
		}

		static void WriteLogLineDelegate(string str)
		{
		    Debug.Log("vsgUnityNative: " + str);
		}
	}

} // end Hbx Native namespace
