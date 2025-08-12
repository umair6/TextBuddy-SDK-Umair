using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TextBuddy.Core
{
    public enum ConnectError
    {
        None = 0,
        Network = 1,          // no HTTP status (DNS/socket), generic transport
        Timeout = 2,          // best-effort detect via error text
        Http4xx = 3,
        Http5xx = 4,
        JsonParse = 5,
        SignatureValidation = 6,
        Unknown = 99
    }

    public sealed class TextBuddyWebConnectResult
    {
        public bool Success { get; set; }
        public ConnectError Error { get; set; }
        public long HttpStatus { get; set; }     // 0 if no response
        public string Message { get; set; }      // human-readable error
        public string Raw { get; set; }          // raw response body (for logs)

        // Domain data
        public string PhoneNumber { get; set; }
        public bool IsSubscribed { get; set; }
    }

    public static class TextBuddyWebConnect
    {
        /// <summary>
        /// Calls /connect, validates signature with apiKey, and returns a typed result (no throws).
        /// </summary>
        public static async Task<TextBuddyWebConnectResult> ConnectAsync(
            string baseUrl,
            string gameID,
            string userID,
            string apiKey,
            int timeoutSeconds = 10)
        {
            // Build payload
            var payload = JsonConvert.SerializeObject(new Dictionary<string, string>
            {
                ["gameID"] = gameID,
                ["userID"] = userID ?? ""
            });

            // Call API (no API key header here; adjust if your server expects it)
            TextBuddyWebResponse res = await TextBuddyWebClient.PostAsync(
                baseUrl, "/connect", apiKey: null, payload: payload, timeoutSeconds: timeoutSeconds);

            // Transport / HTTP errors
            if (!res.Success)
            {
                return new TextBuddyWebConnectResult
                {
                    Success = false,
                    Error = MapTransport(res),
                    HttpStatus = res.StatusCode,
                    Message = string.IsNullOrEmpty(res.ErrorMessage) ? "Request failed" : res.ErrorMessage,
                    Raw = res.ResponseText
                };
            }

            // Parse JSON
            Dictionary<string, string> dict;
            try
            {
                dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(res.ResponseText);
            }
            catch (Exception ex)
            {
                return new TextBuddyWebConnectResult
                {
                    Success = false,
                    Error = ConnectError.JsonParse,
                    HttpStatus = res.StatusCode,
                    Message = "Invalid JSON: " + ex.Message,
                    Raw = res.ResponseText
                };
            }

            // Validate signature
            bool valid = DeepLinkValidator.ValidateQueryParams(dict, apiKey);
            if (!valid)
            {
                return new TextBuddyWebConnectResult
                {
                    Success = false,
                    Error = ConnectError.SignatureValidation,
                    HttpStatus = res.StatusCode,
                    Message = "Response signature validation failed",
                    Raw = res.ResponseText
                };
            }

            // Extract domain data
            dict.TryGetValue("phoneNumber", out string phone);
            dict.TryGetValue("userIDStatus", out string status);
            bool isSubscribed = string.Equals(status, "subscribed", StringComparison.OrdinalIgnoreCase);

            return new TextBuddyWebConnectResult
            {
                Success = true,
                Error = ConnectError.None,
                HttpStatus = res.StatusCode,
                Message = null,
                Raw = res.ResponseText,
                PhoneNumber = phone,
                IsSubscribed = isSubscribed
            };
        }

        private static ConnectError MapTransport(TextBuddyWebResponse res)
        {
            // No HTTP status usually means network/transport (DNS/socket/timeout)
            if (res.StatusCode == 0)
            {
                var msg = res.ErrorMessage?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(msg) && (msg.Contains("timed out") || msg.Contains("timeout")))
                    return ConnectError.Timeout;
                return ConnectError.Network;
            }

            if (res.StatusCode >= 500) return ConnectError.Http5xx;
            if (res.StatusCode >= 400) return ConnectError.Http4xx;
            return ConnectError.Unknown;
        }
    }
}
