using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using StealWithAttribution.Editor;
using UnityEditor;
using UnityEngine.Networking;

namespace StealWithAttribution.Sketchfab.Editor
{
    public static class SketchfabApi
    {
        private const string BaseUri = "https://sketchfab.com";
        private const string Api = "https://api.sketchfab.com";

        private const string ClientId = "IUO8d5VVOIUCzWQArQ3VuXfbwx5QekZfLeDlpOmW";

        private static readonly string PasswordLoginUrl =
            $"{BaseUri}/oauth2/token/?grant_type=password&client_id={ClientId}";

        private const string AccessTokenKey = "stealwithattribution_sketchfab_access_token";

        private static string AccessToken
        {
            get => EditorPrefs.GetString(AccessTokenKey);
            set => EditorPrefs.SetString(AccessTokenKey, value);
        }

        private static Dictionary<string, string> Headers => new Dictionary<string, string>
        {
            {"Authorization", $"Bearer {AccessToken}"}
        };

        [Serializable]
        public class LoginResponse
        {
            public string access_token;
            public string refresh_token;
        }

        public static async UniTask Login(string user, string pass)
        {
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("username", user),
                new MultipartFormDataSection("password", pass)
            };

            var response = await WebApi.Post<LoginResponse>(PasswordLoginUrl, formData);
            AccessToken = response.access_token;
        }

        [Serializable]
        public class DownloadUrlResponse
        {
            [Serializable]
            public class Entry
            {
                public string url;
            }

            public Entry gltf;
        }

        public static async UniTask<string> GetDownloadUrl(string uid)
        {
            var uri = $"{Api}/v3/models/{uid}/download";
            var response = await WebApi.Get<DownloadUrlResponse>(uri, Headers);
            var downloadUrl = response.gltf.url;
            // Debug.Log(downloadUrl);
            return downloadUrl;
        }

        [Serializable]
        public class ModelMetadata
        {
            // public string uid;
            // public int animationCount;
            public License license;
            public string name;
            public bool isDownloadable;

            [Serializable]
            public class License
            {
                public string uri;
                public string label;
                public string fullName;
                public string requirements;
                public string url;
                public string slug;
            }
        }

        public static async UniTask<ModelMetadata> GetModelMetadata(string uid)
        {
            var uri = $"{Api}/v3/models/{uid}";
            var response = await WebApi.Get<ModelMetadata>(uri, Headers);
            return response;
        }

        public static string GetUidFromUrl(string url) => url.Split('-').Last().Trim();
    }
}