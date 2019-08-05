/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

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
        static readonly Vector3 kLeftToRightScalingVector = new Vector3(-1.0f, 1.0f, 1.0f);

        static Vector3 _conversionVector = kLeftToRightScalingVector;

        public static void Convert(ref Vector3 vec)
        {
            vec.x *= _conversionVector.x;
            vec.y *= _conversionVector.y;
            vec.z *= _conversionVector.z;
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
            vec.x *= _conversionVector.x;
            vec.y *= _conversionVector.y;
            vec.z *= _conversionVector.z;
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
            Vector3 fromAxis = new Vector3(quat.x, quat.y, quat.z);
            float axisFlipScale = true ? -1.0f : 1.0f;
            Vector3 toAxis = axisFlipScale * Vector3.Scale(fromAxis, _conversionVector);

            quat.x = toAxis.x;
            quat.y = toAxis.y;
            quat.z = toAxis.z;
        }

        public static void Convert(ref Matrix4x4 mat)
        {
            Matrix4x4 convert = Matrix4x4.Scale(_conversionVector);
            mat = (convert * mat * convert);
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
