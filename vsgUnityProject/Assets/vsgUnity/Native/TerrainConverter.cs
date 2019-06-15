using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vsgUnity.Native
{
    public static class TerrainConverter
    {
        public class TerrainInfo
        {
            public TerrainShaderMapping shaderMapping;
            public MeshInfo terrainMesh;
            public List<VkDescriptorSetLayoutBinding> descriptorBindings = new List<VkDescriptorSetLayoutBinding>();
            public List<string> shaderDefines = new List<string>();
            public List<int> shaderConsts = new List<int>();

            public List<ImageData> diffuseTextureDatas = new List<ImageData>();
            public List<ImageData> maskTextureDatas = new List<ImageData>();
            public List<Vector4> diffuseScales = new List<Vector4>();
            public Vector4 terrainSize = Vector4.one;

        }

        public static TerrainInfo CreateTerrainInfo(Terrain terrain, GraphBuilder.ExportSettings settings)
        {
            TerrainShaderMapping terrainShaderMapping = null;
         
            if (terrain.materialType == Terrain.MaterialType.Custom)
            {
                // try and load a shader mapping file to match the custom terrain material
                terrainShaderMapping = ShaderMappingIO.ReadFromJsonFile<TerrainShaderMapping>(terrain.materialTemplate.shader);
            }

            if (terrainShaderMapping == null && settings.standardTerrainShaderMappingPath != null)
            {
                // load the shader mapping file
                terrainShaderMapping = ShaderMappingIO.ReadFromJsonFile<TerrainShaderMapping>(ShaderMappingIO.GetPathForShaderMappingFile("DefaultTerrain"));
            }

            if (terrainShaderMapping == null)
            {
                Debug.Log("GraphBuilder: Failed to load Terrain Shader Mapping file for terrain '" + terrain.name + "'.");
                return null;
            }

            TerrainInfo terrainInfo = new TerrainInfo();
            terrainInfo.shaderMapping = terrainShaderMapping;
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
                    verts[y * samplew + x] = new Vector3(x * cellsize.x, terrainHeights[y, x] * size.y, y * cellsize.y);
                    normals[y * samplew + x] = terrain.terrainData.GetInterpolatedNormal((float)x / (float)samplew, (float)y / (float)sampleh);
                    uvs[y * samplew + x] = new Vector2(x * uvcellsize.x, y * uvcellsize.y);
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

            // gather textures

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

                ImageData splatData = new ImageData();
                if (!TextureConverter.GetImageDataFromCache(srcmask.GetInstanceID(), out splatData))
                {
                    // alpha masks need to be converteds
                    RenderTexture rt = RenderTexture.GetTemporary(srcmask.width, srcmask.height, 0, RenderTextureFormat.ARGB32);
                    Graphics.Blit(srcmask, rt);
                    Texture2D convmask = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                    Graphics.SetRenderTarget(rt);
                    convmask.ReadPixels(new Rect(0.0f, 0.0f, rt.width, rt.height), 0, 0, false);
                    convmask.Apply(false, false);

                    splatData = TextureConverter.CreateImageData(convmask, false);
                    TextureConverter.AddImageDataToCache(splatData, srcmask.GetInstanceID());
                }

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

            return terrainInfo;
        }
    }
}
