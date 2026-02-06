using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Minesweeper.Game.Services
{
    [Serializable]
    public class ChatRequest
    {
        public string message;
        public string system;
    }

    [Serializable]
    public class ChatResponse
    {
        public string reply;
    }

    public static class OnlineChatService
    {
        public static IEnumerator Ask(string url, int timeoutSeconds, string system, string message, Action<string> onDone)
        {
            var req = new ChatRequest { message = message, system = system };
            string json = JsonUtility.ToJson(req);

            using var www = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = Mathf.Max(3, timeoutSeconds);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                onDone?.Invoke(null);
                yield break;
            }

            try
            {
                var res = JsonUtility.FromJson<ChatResponse>(www.downloadHandler.text);
                onDone?.Invoke(res != null ? res.reply : null);
            }
            catch
            {
                onDone?.Invoke(null);
            }
        }
    }
}
