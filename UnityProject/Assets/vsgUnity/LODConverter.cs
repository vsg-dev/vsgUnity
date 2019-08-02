/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

using System.Collections.Generic;
using UnityEngine;

namespace vsgUnity
{
    public class LODNode
    {
        public List<GameObject> gameObjects = new List<GameObject>();
    }

    public static class LODConverter
    {
        public static LODNode CreateLOD(LODGroup group, LOD lod)
        {
            Transform root = group.transform;
            LODNode lodNode = new LODNode();

            Renderer[] renderers = lod.renderers;
            List<Transform[]> rendererTransformPaths = new List<Transform[]>();

            // filter the renderers into gameobjects with unique roots in the group (i.e. if two renderers share a parent just add the parent gameobject)
            for(int i = 0; i < renderers.Length; i++)
            {
                rendererTransformPaths.Add(TransformConverter.CreateTransformPath(renderers[i].transform, root));
            }

            return lodNode;
        }
    }
}
