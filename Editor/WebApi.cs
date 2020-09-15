using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
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

        public static async UniTask<T> Get<T>(string url)
        {
            var json = await GetText(url);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}