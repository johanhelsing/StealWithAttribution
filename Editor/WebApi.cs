using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace StealWithAttribution.Editor
{
    public static class WebApi
    {
        private static async UniTask<string> GetText(string url)
        {
            var request = await UnityWebRequest.Get(url).SendWebRequest();
            return request.downloadHandler.text;
        }

        private static async UniTask<string> GetText(string url, Dictionary<string, string> headers)
        {
            var request = UnityWebRequest.Get(url);
            foreach (var header in headers)
            {
                // Debug.Log($"{header.Key}: {header.Value}");
                request.SetRequestHeader(header.Key, header.Value);
            }
            await request.SendWebRequest();
            return request.downloadHandler.text;
        }

        private static async UniTask<string> PostText(string url,
            List<IMultipartFormSection> form)
        {
            var request = await UnityWebRequest.Post(url, form).SendWebRequest();
            return request.downloadHandler.text;
        }

        public static async UniTask<T> Get<T>(string url)
        {
            var json = await GetText(url);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async UniTask<T> Get<T>(string url, Dictionary<string, string> headers)
        {
            var json = await GetText(url, headers);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async UniTask<T> Post<T>(string url, List<IMultipartFormSection> form)
        {
            var json = await PostText(url, form);
            Debug.Log(json);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}