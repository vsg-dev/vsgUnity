/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth
Copyright(c) 2022 Christian Schott (InstruNEXT GmbH)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vsgUnity
{
    public static class CoordSytemConverter
    {
        // flip y and z axis
        static readonly Matrix4x4 _conversionMatrix = new Matrix4x4(
            new Vector4(1f, 0f, 0f, 0f), 
            new Vector4(0f, 0f, 1f, 0f),
            new Vector4(0f, 1f, 0f, 0f),
            new Vector4(0f, 0f, 0f, 1f));

        public static void Convert(ref Vector3 vec)
        {
            var tmp = vec.y;
            vec.y = vec.z;
            vec.z = tmp;
        }

        public static void Convert(Vector3[] vecArray)
        {
            for(int i = 0; i < vecArray.Length; i++)
            {
                Convert(ref vecArray[i]);
            }
        }

        public static void Convert(ref Vector4 vec)
        {
            var tmp = vec.y;
            vec.y = vec.z;
            vec.z = tmp;
        }

        public static void Convert(Vector4[] vecArray)
        {
            for (int i = 0; i < vecArray.Length; i++)
            {
                Convert(ref vecArray[i]);
            }
        }


        public static void Convert(ref Quaternion quat)
        {
            // not sure if this is correct, but it does not seem to be used anywhere
            Vector3 fromAxis = new Vector3(quat.x, quat.y, quat.z);
            float axisFlipScale = true ? -1.0f : 1.0f;
            Vector3 toAxis = axisFlipScale * fromAxis;
            quat.x = axisFlipScale * toAxis.x;
            quat.y = axisFlipScale * toAxis.z;
            quat.z = axisFlipScale * toAxis.y;
        }

        public static void Convert(ref Matrix4x4 mat)
        {
            mat = (_conversionMatrix * mat * _conversionMatrix);
        }

        public static void FlipTriangleFaces(int[] indices)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                int temp = indices[i];
                indices[i] = indices[i + 2];
                indices[i + 2] = temp;
            }
        }
    }

} // end vsgUnity namespace
