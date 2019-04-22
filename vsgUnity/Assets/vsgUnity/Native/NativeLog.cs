//----------------------------------------------
//            vsgUnity: Native
// Writen by Thomas Hogarth
// NativeLog.cs
//----------------------------------------------

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace vsgUnity.Native
{
	public static class NativeLog
	{

        [DllImport("unity2vsgd", EntryPoint = "unity2vsg_Debug_SetDebugLogCallback")]
		private static extern void unity2vsg_Debug_SetDebugLogCallback(IntPtr aCallbackFuntion);


		delegate void DebugLogDelegate(string aMessageString);

		[MonoPInvokeCallback (typeof (DebugLogDelegate))]
		public static void InstallDebugLogCallback()
		{
			DebugLogDelegate callbackdelegate = new DebugLogDelegate(WriteLogLineDelegate);
			 
			// Convert delegate into a function pointer to pass to native plugin
			IntPtr callbackpointer = Marshal.GetFunctionPointerForDelegate(callbackdelegate);
			  
			// Pass the pointer to native plugin
			unity2vsg_Debug_SetDebugLogCallback(callbackpointer);
        }

        static void WriteLogLineDelegate(string str)
		{
		    Debug.Log("vsgUnityNative: " + str);
		}
	}

}
