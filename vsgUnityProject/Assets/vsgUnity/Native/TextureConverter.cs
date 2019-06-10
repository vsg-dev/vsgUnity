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

        public static void ClearCaches()
        {
            _imageDataCache.Clear();
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
            if (issues != TextureSupportIssues.None)
            {
                texdata = CreateImageData(Texture2D.whiteTexture);
                Debug.LogWarning(GetTextureSupportReport(issues, texture));
                return texdata;
            }

            switch (texture.dimension)
            {
                case TextureDimension.Tex2D: PopulateImageData(texture as Texture2D, ref texdata); break;
                case TextureDimension.Tex3D: PopulateImageData(texture as Texture3D, ref texdata); break;
                default: break;
            }

            // add to the cache (double check it doesn't exist already)
            if (!_imageDataCache.ContainsKey(texdata.id) && addToCache)
            {
                _imageDataCache[texdata.id] = texdata;
            }

            return texdata;
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
            texdata.pixels.data = texture.GetRawTextureData(); //Color32ArrayToByteArray(texture.GetPixels32());
            texdata.pixels.length = texdata.pixels.data.Length;
            texdata.mipmapCount = texture.mipmapCount;
            texdata.mipmapBias = texture.mipMapBias;
            return true;
        }

        public static bool PopulateImageData(Texture3D texture, ref ImageData texdata)
        {
            if (!PopulateImageData(texture as Texture, ref texdata)) return false;
            texdata.depth = texture.depth;
            //texdata.pixels.data = Color32ArrayToByteArray(texture.GetPixels32());
            texdata.pixels.length = texdata.pixels.data.Length;
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

            if (texture.dimension != TextureDimension.Tex2D) issues |= TextureSupportIssues.Dimensions; //&& texture.dimension != TextureDimension.Tex3D

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
