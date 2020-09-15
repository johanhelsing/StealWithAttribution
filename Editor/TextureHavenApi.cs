using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace StealWithAttribution.Editor
{
    public static class TextureHavenApi
    {
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
                   (metadata = await WebApi.Get<AllMetaData>("https://texturehaven.com/php/assetninja.php?v=1.1"));
        }
    }
}