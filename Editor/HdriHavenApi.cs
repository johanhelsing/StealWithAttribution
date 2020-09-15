using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace StealWithAttribution.Editor
{
    public static class HdriHavenApi
    {
        public enum Resolution { _1k, _2k, _4k, _8k, _16k }

        [Serializable]
        public class AllMetaData
        {
            public string version;
            public Dictionary<string, AssetMetaData> assets;
        }

        [Serializable]
        public class AssetMetaData
        {
            public string author;
            public string license;
            public File[] files;
        }

        [Serializable]
        public class File
        {
            public string url;
            public int size;
        }

        private static AllMetaData metadata;
        public static async UniTask<AllMetaData> GetMetaDataAsync()
        {
            return metadata ??
                   (metadata = await WebApi.Get<AllMetaData>("https://hdrihaven.com/php/assetninja.php?v=1.1"));
        }

        public static async UniTask<AssetMetaData> GetAssetMetaDataAsync(string id)
        {
            var all = await GetMetaDataAsync();
            all.assets.TryGetValue(id, out var value);
            return value;
        }

        public static async UniTask<string> GetTextureUrlAsync(string id, Resolution resolution)
        {
            return (await GetAssetMetaDataAsync(id))
                .files.Select(file => file.url)
                .FirstOrDefault(url => url.Contains(resolution.ToString()));
        }

        public static string GetIdFromUrl(string url)
        {
            if (!url.StartsWith("https://hdrihaven.com")) return null;
            var query = new Uri(url).Query;
            var id = query.Split('&').FirstOrDefault()?.Replace("?h=", "");
            return id;
        }

        public static string GetAssetPreviewUrl(string id) => $"https://hdrihaven.com/files/hdri_images/thumbnails/{id}.jpg";

        private static Dictionary<string, Texture2D> previews = new Dictionary<string, Texture2D>();
        public static async UniTask<Texture> GetAssetPreviewTexture(string id)
        {
            if (previews.TryGetValue(id, out var texture)) return texture;
            var url = GetAssetPreviewUrl(id);
            return previews[id] = await Utils.GetPngOrJpgTexture(url);
        }
    }
}