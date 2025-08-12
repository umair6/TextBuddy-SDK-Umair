using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TextBuddy.Core
{



    public class TBWebResponse
    {
        public bool Success;
        public long StatusCode;
        public string ErrorMessage;
        public string ResponseText;
    }


    public static class TBWebClient
    {
        public static Task<TBWebResponse> PostAsync(
            string baseUrl,
            string endpoint,
            string apiKey,
            string payload,
            int timeoutSeconds = 10
        )
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
            return SendAsync(baseUrl, endpoint, apiKey, UnityWebRequest.kHttpVerbPOST, bodyRaw, timeoutSeconds);
        }

        public static Task<TBWebResponse> GetAsync(
            string baseUrl,
            string endpoint,
            string apiKey,
            string queryString = null,
            int timeoutSeconds = 10
        )
        {
            string fullEndpoint = endpoint;
            if (!string.IsNullOrEmpty(queryString))
                fullEndpoint += (queryString.StartsWith("?") ? queryString : "?" + queryString);

            return SendAsync(baseUrl, fullEndpoint, apiKey, UnityWebRequest.kHttpVerbGET, null, timeoutSeconds);
        }

        private static async Task<TBWebResponse> SendAsync(
            string baseUrl,
            string endpoint,
            string apiKey,
            string method,
            byte[] bodyRaw,
            int timeoutSeconds
        ) 
        {
            string url = baseUrl.TrimEnd('/') + endpoint;
            TBLogger.Info("SendAsync::URL::" + url);
            using var www = new UnityWebRequest(url, method)
            {
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = timeoutSeconds
            };

            if (bodyRaw != null)
            {
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.SetRequestHeader("Content-Type", "application/json");
            }

            if (apiKey != null)
            {
                www.SetRequestHeader("X-TB-ApiKey", apiKey);
            }

            await www.SendWebRequest().AsTask();
            return ToResponse(www);
        }

        private static TBWebResponse ToResponse(UnityWebRequest req)
        {
            var res = new TBWebResponse
            {
                StatusCode = req.responseCode,
                ResponseText = req.downloadHandler?.text
            };

#if UNITY_2020_2_OR_NEWER
            res.Success = req.result == UnityWebRequest.Result.Success;
#else
            res.Success = !req.isNetworkError && !req.isHttpError;
#endif
            res.ErrorMessage = res.Success ? null : req.error;
            return res;
        }

        private static Task AsTask(this UnityWebRequestAsyncOperation op)
        {
            var tcs = new TaskCompletionSource<object>();
            op.completed += _ => tcs.SetResult(null);
            return tcs.Task;
        }

    }
}
