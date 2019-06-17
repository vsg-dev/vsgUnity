/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

using System;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace vsgUnity.Native
{
    public static class NativeLog
    {
        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_Debug_SetDebugLogCallback")]
        private static extern void unity2vsg_Debug_SetDebugLogCallback(IntPtr aCallbackFuntion);

        delegate void DebugLogDelegate(string aMessageString);

        static DebugLogDelegate _callbackdelegate = null;

        [MonoPInvokeCallback(typeof(DebugLogDelegate))]
        public static void
        InstallDebugLogCallback()
        {
            _callbackdelegate = new DebugLogDelegate(WriteLogLineDelegate);

            // Convert delegate into a function pointer to pass to native plugin
            IntPtr callbackpointer = Marshal.GetFunctionPointerForDelegate(_callbackdelegate);

            // Pass the pointer to native plugin
            unity2vsg_Debug_SetDebugLogCallback(callbackpointer);
        }

        static void WriteLogLineDelegate(string str)
        {
            WriteLine("vsgUnityNative: " + str);
        }

        public static StringBuilder _reportBuilder = new StringBuilder();

        public static void WriteLine(string msg)
        {
            _reportBuilder.AppendLine(msg);
        }

        public static void PrintReport()
        {
            Debug.Log("vsgUnity Export Report:\n" + _reportBuilder.ToString());
            _reportBuilder.Clear();
        }
    }

}
