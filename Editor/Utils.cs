using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace StealWithAttribution.Editor
{
    public static class Utils
    {
        public static string AssetsFolder => Application.dataPath;
        public static string ResolveAssetPath(string assetPath) => Path.GetFullPath($"{AssetsFolder}/../{assetPath}");

        public static async UniTask<byte[]> GetBytesAsync(UnityWebRequest request, IProgress<float> progress = null)
        {
            await request.SendWebRequest().ToUniTask(progress);
            return request.downloadHandler.data;
        }

        public static async UniTask<Texture2D> GetPngOrJpgTexture(string url)
        {
            var request = await UnityWebRequestTexture.GetTexture(url).SendWebRequest();
            return ((DownloadHandlerTexture)request.downloadHandler).texture;
        }

        public static async UniTask<string> DownloadAndSaveFile(string url, string destination)
        {
            return await DownloadAndSaveFile(url, destination, null);
        }

        public static async UniTask WriteAllBytesAsync(byte[] bytes, string destination)
        {
            using (var fileStream = File.OpenWrite(destination))
            {
                await fileStream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public static async UniTask<string> DownloadAndSaveFile(string url, string destination, IProgress<float> progress )
        {
            Debug.Log($"Downloading {url}...");
            var bytes = await GetBytesAsync(UnityWebRequest.Get(url), progress);
            Debug.Log($"Done Downloading {url}");
            await WriteAllBytesAsync(bytes, destination);
            return destination;
        }

        public static string FileNameFromUrl(string url)
        {
            var uri = new Uri(url);
            var fileName=  Path.GetFileName(uri.AbsolutePath);
            return fileName;
        }

        public static string AssetDatabasePathFromAbsolute(string absolute)
        {
            var absoluteUnixStyle = absolute.Replace("\\", "/");
            Debug.Assert(absoluteUnixStyle.StartsWith(AssetsFolder));
            return absoluteUnixStyle.Replace(AssetsFolder, "Assets");
        }

        public static async UniTask<IEnumerable<string>> DownloadFilesToDirectory(IEnumerable<string> urls,
            string directory)
        {
            var downloads = urls
                .Select(url => DownloadAndSaveFile(url, $"{directory}/{FileNameFromUrl(url)}"));
            return await UniTask.WhenAll(downloads);
        }
    }
}