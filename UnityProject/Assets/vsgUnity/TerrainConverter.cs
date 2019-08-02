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
    public static class TerrainConverter
    {
        public class TerrainInfo
        {
            public ShaderMapping shaderMapping;
            public MeshInfo terrainMesh;

            // standard terrain material info
            public List<VkDescriptorSetLayoutBinding> descriptorBindings = new List<VkDescriptorSetLayoutBinding>();
            public List<string> shaderDefines = new List<string>();
            public List<int> shaderConsts = new List<int>();

            public List<ImageData> diffuseTextureDatas = new List<ImageData>();
            public List<ImageData> maskTextureDatas = new List<ImageData>();
            public List<Vector4> diffuseScales = new List<Vector4>();
            public Vector4 terrainSize = Vector4.one;

            // custom terrain material info
            public MaterialInfo customMaterial;
        }

        public static TerrainInfo CreateTerrainInfo(Terrain terrain, GraphBuilder.ExportSettings settings)
        {
            TerrainInfo terrainInfo = new TerrainInfo();
            bool usingCustomMaterial = false;

            if (terrain.materialType == Terrain.MaterialType.Custom)
            {
                // try and load a shader mapping file to match the custom terrain material
                terrainInfo.shaderMapping = ShaderMappingIO.ReadFromJsonFile(terrain.materialTemplate.shader);
                if (terrainInfo.shaderMapping != null) usingCustomMaterial = true;
            }
            else
            {
                // load the default terrain shader mapping file
                terrainInfo.shaderMapping = ShaderMappingIO.ReadFromJsonFile(ShaderMappingIO.GetPathForShaderMappingFile("DefaultTerrain"));
            }

            if (terrainInfo.shaderMapping == null)
            {
                // no mapping loaded, use a default shader so we can at least export and render the terrain mesh
                NativeLog.WriteLine("GraphBuilder: Failed to load Terrain Shader Mapping file for terrain '" + terrain.name + "'.");
                terrainInfo.shaderMapping = ShaderMappingIO.ReadFromJsonFile(ShaderMappingIO.GetPathForShaderMappingFile("Default"));
                usingCustomMaterial = true;
                return null;
            }

            terrainInfo.shaderDefines.Add("VSG_LIGHTING");

            // build mesh
            int samplew = terrain.terrainData.heightmapWidth;
            int sampleh = terrain.terrainData.heightmapHeight;

            int cellw = terrain.terrainData.heightmapWidth - 1;
            int cellh = terrain.terrainData.heightmapHeight - 1;

            Vector3 size = terrain.terrainData.size;

            Vector2 cellsize = new Vector3(size.x / cellw, size.z / cellh);
            Vector2 uvcellsize = new Vector2(1.0f / cellw, 1.0f / cellh);

            float[,] terrainHeights = terrain.terrainData.GetHeights(0, 0, samplew, sampleh);

            int vertcount = samplew * sampleh;
            Vector3[] verts = new Vector3[vertcount];
            Vector3[] normals = new Vector3[vertcount];
            Vector2[] uvs = new Vector2[vertcount];

            int[] indicies = new int[(cellw * cellh) * 6];

            // vertices and UVs
            for (int y = 0; y < samplew; y++)
            {
                for (int x = 0; x < sampleh; x++)
                {
                    verts[y * samplew + x].Set(x * cellsize.x, terrainHeights[y, x] * size.y, y * cellsize.y);
                    normals[y * samplew + x] = terrain.terrainData.GetInterpolatedNormal((float)x / (float)samplew, (float)y / (float)sampleh);
                    uvs[y * samplew + x].Set(x * uvcellsize.x, y * uvcellsize.y);
                }
            }

            // triangles
            int index = 0;
            for (int y = 0; y < cellw; y++)
            {
                for (int x = 0; x < cellh; x++)
                {
                    indicies[index++] = (y * samplew) + x;
                    indicies[index++] = ((y + 1) * samplew) + x;
                    indicies[index++] = (y * samplew) + x + 1;

                    indicies[index++] = ((y + 1) * samplew) + x;
                    indicies[index++] = ((y + 1) * samplew) + x + 1;
                    indicies[index++] = (y * samplew) + x + 1;
                }
            }

            // convert to meshinfo
            MeshInfo mesh = null;
            if (!MeshConverter.GetMeshInfoFromCache(terrain.GetInstanceID(), out mesh))
            {
                mesh = new MeshInfo
                {
                    id = terrain.GetInstanceID(),
                    verticies = NativeUtils.WrapArray(verts),
                    normals = NativeUtils.WrapArray(normals),
                    uv0 = NativeUtils.WrapArray(uvs),
                    triangles = NativeUtils.WrapArray(indicies),
                    use32BitIndicies = 1
                };

                MeshConverter.AddMeshInfoToCache(mesh, terrain.GetInstanceID());
            }
            terrainInfo.terrainMesh = mesh;
            terrainInfo.terrainSize = size;

            // gather material info

            if (!usingCustomMaterial)
            {
                // use standard terrain layers
                TerrainLayer[] layers = terrain.terrainData.terrainLayers;
                for (int i = 0; i < layers.Length; i++)
                {
                    ImageData layerData = TextureConverter.GetOrCreateImageData(layers[i].diffuseTexture);
                    terrainInfo.diffuseTextureDatas.Add(layerData);

                    terrainInfo.diffuseScales.Add(new Vector4(1.0f / layers[i].tileSize.x, 1.0f / layers[i].tileSize.y));
                }

                for (int i = 0; i < terrain.terrainData.alphamapTextureCount; i++)
                {
                    Texture2D srcmask = terrain.terrainData.GetAlphamapTexture(i);
                    ImageData splatData = TextureConverter.GetOrCreateImageData(srcmask);

                    terrainInfo.maskTextureDatas.Add(splatData);
                }

                if (terrainInfo.diffuseTextureDatas.Count > 0)
                {
                    terrainInfo.descriptorBindings.Add(new VkDescriptorSetLayoutBinding() { binding = 0, descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, stageFlags = VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT, descriptorCount = (uint)terrainInfo.diffuseTextureDatas.Count });
                    terrainInfo.descriptorBindings.Add(new VkDescriptorSetLayoutBinding() { binding = 2, descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, stageFlags = VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT, descriptorCount = (uint)terrainInfo.diffuseScales.Count });

                    terrainInfo.shaderConsts.Add(terrainInfo.diffuseTextureDatas.Count);
                    terrainInfo.shaderDefines.Add("VSG_TERRAIN_LAYERS");
                }

                terrainInfo.descriptorBindings.Add(new VkDescriptorSetLayoutBinding() { binding = 3, descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, stageFlags = VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT, descriptorCount = 1 });

                if (terrainInfo.maskTextureDatas.Count > 0)
                {
                    terrainInfo.descriptorBindings.Add(new VkDescriptorSetLayoutBinding() { binding = 1, descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, stageFlags = VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT, descriptorCount = (uint)terrainInfo.maskTextureDatas.Count });
                    terrainInfo.shaderConsts.Add(terrainInfo.maskTextureDatas.Count);
                }
            }
            else
            {
                Material customMaterial = terrain.materialTemplate;
                terrainInfo.customMaterial = MaterialConverter.CreateMaterialData(customMaterial, terrainInfo.shaderMapping);
                terrainInfo.customMaterial.customDefines = terrainInfo.shaderDefines;
            }

            return terrainInfo;
        }
    }
}
