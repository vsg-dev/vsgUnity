/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

using System.Collections.Generic;
using UnityEngine;

using vsgUnity.Native;

namespace vsgUnity
{
    public static class TransformConverter
    {
        public static TransformData CreateTransformData(Transform transform)
        {
            TransformData transformdata = new TransformData();
            Matrix4x4 matrix = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
            transformdata.matrix.data = new float[]
            {
                matrix[0, 0], matrix[0, 1], matrix[0, 2], matrix[0, 3],
                matrix[1, 0], matrix[1, 1], matrix[1, 2], matrix[1, 3],
                matrix[2, 0], matrix[2, 1], matrix[2, 2], matrix[2, 3],
                matrix[3, 0], matrix[3, 1], matrix[3, 2], matrix[3, 3]
            };
            transformdata.matrix.length = transformdata.matrix.data.Length;
            return transformdata;
        }

        public static Transform[] CreateTransformPath(Transform child, Transform parent)
        {
            if (child == parent) return new Transform[] { child };

            Transform current = child;
            List<Transform> path = new List<Transform>();
            while(current != parent && current != null)
            {
                path.Add(current);
                current = current.parent;
            }
            if (current == null) return null;
            path.Add(current);
            return path.ToArray();
        }
    }
}
