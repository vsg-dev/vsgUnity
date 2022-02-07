/* <editor-fold desc="MIT License">

Copyright(c) 2019 Thomas Hogarth
Copyright(c) 2022 Christian Schott (InstruNEXT GmbH)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

using vsgUnity.Native;

namespace vsgUnity
{
    /// <summary>
    /// TextureConverter
    /// Helper class for converting Unity Textures to vsgUnity image data structs for passing to the exporter
    /// </summary>
    /// 
    public static class TextureConverter
    {
        public static Dictionary<int, ImageData> _imageDataCache = new Dictionary<int, ImageData>();
        public static List<Texture2D> _convertedTextures = new List<Texture2D>();

        static TextureConverter() 
        {
            SceneGraphExporter.OnBeginExport += ClearCaches;
            SceneGraphExporter.OnEndExport += ClearCaches;
        }

        private static void ClearCaches()
        {
            _imageDataCache.Clear();

            foreach(Texture2D tex in _convertedTextures)
            {
                Texture2D.DestroyImmediate(tex);
            }
            _convertedTextures.Clear();
        }

        /// <summary>
        /// Either create a new ImageData representing the passed texture or if this texture has already
        /// been converted return the ImageData from the cache
        /// </summary>
        /// <param name="texture"></param>
        /// <returns>ImageData representing the texture</returns>

        public static ImageData GetOrCreateImageData(Texture texture)
        {
            if (_imageDataCache.ContainsKey(texture.GetInstanceID()))
            {
                return _imageDataCache[texture.GetInstanceID()];
            }
            {
                return CreateImageData(texture);
            }
        }

        /// <summary>
        /// Create a new image data representing the passed texture
        /// </summary>
        /// <param name="texture"></param>
        /// <returns>ImageData representing the texture</returns>

        public static ImageData CreateImageData(Texture texture, bool addToCache = true)
        {
            ImageData texdata = new ImageData();

            TextureSupportIssues issues = GetSupportIssuesForTexture(texture);

            if ((issues & TextureSupportIssues.Format) == TextureSupportIssues.Format && texture.dimension == TextureDimension.Tex2D)
            {
                Texture2D source = texture as Texture2D;
                RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(source, rt);
                Texture2D converted = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                Graphics.SetRenderTarget(rt);
                converted.ReadPixels(new Rect(0.0f, 0.0f, rt.width, rt.height), 0, 0, false);
                converted.Apply(false, false);
                RenderTexture.ReleaseTemporary(rt);
                _convertedTextures.Add(converted);

                texdata = TextureConverter.CreateImageData(converted, false);
                TextureConverter.AddImageDataToCache(texdata, source.GetInstanceID());

                return texdata;
            }
            else if (issues != TextureSupportIssues.None)
            {
                texdata = CreateImageData(Texture2D.whiteTexture);
                NativeLog.WriteLine(GetTextureSupportReport(issues, texture));
                return texdata;
            }

            switch (texture.dimension)
            {
                case TextureDimension.Tex2D: PopulateImageData(texture as Texture2D, ref texdata); break;
                case TextureDimension.Tex3D: PopulateImageData(texture as Texture3D, ref texdata); break;
                case TextureDimension.Cube: PopulateImageData(texture as Cubemap, ref texdata); break;
                default: break;
            }

            // add to the cache (double check it doesn't exist already)
            if (!_imageDataCache.ContainsKey(texdata.id) && addToCache)
            {
                _imageDataCache[texdata.id] = texdata;
            }

            return texdata;
        }

        /// <summary>
        /// Either create new ImageDatas representing the passed texture array or if this texture has already
        /// been converted return the ImageDatas from the cache
        /// </summary>
        /// <param name="texture"></param>
        /// <returns>ImageData representing the texture</returns>

        public static ImageData[] GetOrCreateImageData(Texture2DArray texture)
        {
            if (_imageDataCache.ContainsKey(texture.GetInstanceID()))
            {
                List<ImageData> datas = new List<ImageData>();
                for (int i = 0; i < texture.depth; i++)
                {
                    int id = texture.GetInstanceID() * (i + 1);
                    datas.Add(_imageDataCache[id]);
                }
                return datas.ToArray();
            }
            return CreateImageDatas(texture);
        }

        /// <summary>
        /// Create an array of new image data representing the passed texture array
        /// </summary>
        /// <param name="texture"></param>
        /// <returns>ImageData array representing the texture array</returns>

        public static ImageData[] CreateImageDatas(Texture2DArray texture, bool addToCache = true)
        {
            List<ImageData> texdatas = new List<ImageData>();

            TextureSupportIssues issues = GetSupportIssuesForTexture(texture);
            if (issues != TextureSupportIssues.None)
            {
                texdatas.Add(CreateImageData(Texture2D.whiteTexture));
                NativeLog.WriteLine(GetTextureSupportReport(issues, texture));
                return texdatas.ToArray();
            }

            for (int i = 0; i < texture.depth; i++)
            {
                ImageData texdata = new ImageData();
                PopulateImageData(texture, i, ref texdata);
                texdata.id = texture.GetInstanceID() * (i + 1); // hack some kind of id for the texture
                texdatas.Add(texdata);

                // add to the cache (double check it doesn't exist already)
                if (!_imageDataCache.ContainsKey(texdata.id) && addToCache)
                {
                    _imageDataCache[texdata.id] = texdata;
                }
            }

            return texdatas.ToArray();
        }

        public static bool CacheContainsImageDataForTexture(Texture texture)
        {
            return _imageDataCache.ContainsKey(texture.GetInstanceID());
        }

        public static bool GetImageDataFromCache(int cacheID, out ImageData result)
        {
            if (_imageDataCache.ContainsKey(cacheID))
            {
                result = _imageDataCache[cacheID];
                return true;
            }
            result = default;
            return false;
        }

        public static void AddImageDataToCache(ImageData imageData, int cacheID)
        {
            _imageDataCache[cacheID] = imageData;
        }

        public static bool PopulateImageData(Texture2D texture, ref ImageData texdata)
        {
            if (!PopulateImageData(texture as Texture, ref texdata)) return false;
            texdata.depth = 1;
            texdata.pixels = NativeUtils.ToNative(texture.GetRawTextureData()); //Color32ArrayToByteArray(texture.GetPixels32());
            texdata.mipmapCount = texture.mipmapCount;
            texdata.mipmapBias = texture.mipMapBias;
            return true;
        }

        public static bool PopulateImageData(Texture3D texture, ref ImageData texdata)
        {
            if (!PopulateImageData(texture as Texture, ref texdata)) return false;
            texdata.depth = texture.depth;
            //texdata.pixels.data = Color32ArrayToByteArray(texture.GetPixels32());
            //texdata.pixels.length = texdata.pixels.data.Length;
            
            return true;
        }

        public static bool PopulateImageData(Cubemap cubemap, ref ImageData texdata)
        {
            var rt = RenderTexture.GetTemporary(cubemap.width, cubemap.height * 6, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            var mappedCubeTex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            _convertedTextures.Add(mappedCubeTex);

            for (int face = 0; face < 6; face++) {
                Graphics.CopyTexture(cubemap, face, 0, 0, 0, cubemap.width, cubemap.height, rt, 0, 0, 0, face * cubemap.height);
            }

            // read back
            RenderTexture.active = rt;
            mappedCubeTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
            mappedCubeTex.Apply(false, false);
            RenderTexture.active = null;

            if (!TextureConverter.PopulateImageData(mappedCubeTex as Texture, ref texdata))
                return false;
            texdata.pixels = NativeUtils.ToNative(mappedCubeTex.GetRawTextureData());
            texdata.viewType = (int)VkImageViewType.VK_IMAGE_VIEW_TYPE_CUBE;
            texdata.depth = 6;
            texdata.height = texdata.width;
            return true;
        }

        public static bool PopulateImageData(Texture2DArray texture, int index, ref ImageData texdata)
        {
            if (!PopulateImageData(texture as Texture, ref texdata)) return false;
            texdata.depth = 1;
            texdata.format = VkFormat.R8G8B8A8_UNORM;
            texdata.pixels = NativeUtils.ToNative(Color32ArrayToByteArray(texture.GetPixels32(index, 0)));
            texdata.mipmapCount = 1;
            return true;
        }

        /// <summary>
        /// Populate the base data accesible via Texture, exludes pixel data and depth
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="texdata"></param>
        /// <returns></returns>

        public static bool PopulateImageData(Texture texture, ref ImageData texdata)
        {
            texdata.id = texture.GetInstanceID();
            texdata.format = Vulkan.vkFormatForGraphicsFormat(texture.graphicsFormat);
            texdata.width = texture.width;
            texdata.height = texture.height;
            texdata.anisoLevel = texture.anisoLevel;
            texdata.wrapMode = Vulkan.vkSamplerAddressModeForWrapMode(texture.wrapMode);
            texdata.filterMode = Vulkan.vkFilterForFilterMode(texture.filterMode);
            texdata.mipmapMode = Vulkan.vkSamplerMipmapModeForFilterMode(texture.filterMode);
            texdata.mipmapCount = 1;
            texdata.mipmapBias = 0.0f;
            texdata.viewType = -1;
            return true;
        }

        private static byte[] Color32ArrayToByteArray(Color32[] colors)
        {
            if (colors == null || colors.Length == 0)
                return null;

            int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
            int length = lengthOfColor32 * colors.Length;
            byte[] bytes = new byte[length];

            GCHandle handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(ptr, bytes, 0, length);
            }
            finally
            {
                if (handle != default(GCHandle))
                    handle.Free();
            }

            return bytes;
        }

        /// <summary>
        /// So textures will not export properly so this enum represents the reasons why a texture may fail to convert
        /// </summary>

        [System.Flags]
        public enum TextureSupportIssues
        {
            None = 0,
            Dimensions = 1,
            Format = 2,
            ReadWrite = 4
        }

        public static TextureSupportIssues GetSupportIssuesForTexture(Texture texture)
        {
            TextureSupportIssues issues = TextureSupportIssues.None;

            if (!texture.isReadable) issues |= TextureSupportIssues.ReadWrite;

            VkFormat format = Vulkan.vkFormatForGraphicsFormat(texture.graphicsFormat);
            if (format == VkFormat.UNDEFINED) issues |= TextureSupportIssues.Format;

            switch(texture.dimension) {
                case TextureDimension.Tex2D:
                case TextureDimension.Tex2DArray:
                case TextureDimension.Cube:
                    break;
                default:
                    issues |= TextureSupportIssues.Dimensions;
                    break;
            }

            return issues;
        }

        /// <summary>
        /// Returns empty string if texture is support,
        /// otherwise returns description of unsupported features
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        /// 
        public static string GetTextureSupportReport(Texture texture)
        {
            TextureSupportIssues issues = GetSupportIssuesForTexture(texture);
            return GetTextureSupportReport(issues, texture);
        }

        /// <summary>
        /// Returns empty string if texture is support,
        /// otherwise returns description of unsupported features 
        /// </summary>
        /// <param name="issues"></param>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static string GetTextureSupportReport(TextureSupportIssues issues, Texture texture)
        {
            string report = string.Empty;

            if ((issues & TextureSupportIssues.ReadWrite) == TextureSupportIssues.ReadWrite) report += "Read/Write not enabled. Please enabled Read/Write in import settings.\n";
            if ((issues & TextureSupportIssues.Format) == TextureSupportIssues.Format) report += "Format '" + texture.graphicsFormat.ToString() + "' unsupported. Please select a supported format (RGBA32) in import settings.\n";
            if ((issues & TextureSupportIssues.Dimensions) == TextureSupportIssues.Dimensions) report += "Unsupported Texture dimension '" + texture.dimension.ToString() + "'. Try selecting another 'Texture Shape' (2D) in immport settings.\n";

            if (!string.IsNullOrEmpty(report))
            {
                report = texture.name + " has the following issues:\n" + report;
            }

            return report;
        }
    }

}
