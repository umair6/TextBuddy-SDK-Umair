using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TextBuddy.core
{
    /// <summary>
    /// Provides methods to validate deep link query parameters using HMAC-SHA256.
    /// </summary>
    public static class TBDeepLinkValidator
    {
        /// <summary>
        /// Validates that the signature in the query parameters matches the calculated HMAC.
        /// </summary>
        /// <param name="parameters">The parsed query parameters from the deep link.</param>
        /// <param name="apiKey">The shared API key used to compute the HMAC.</param>
        /// <param name="signatureKey">The name of the query parameter that contains the HMAC signature (e.g. "sig").</param>
        /// <returns>True if the signature is valid, false otherwise.</returns>
        public static bool ValidateQueryParams(
            Dictionary<string, string> parameters,
            string apiKey,
            string signatureKey = "sig")
        {
            if (string.IsNullOrWhiteSpace(signatureKey))
            {
                TBLogger.Warning("[DeepLinkValidator] Signature key cannot be null or empty.");
                return false;
            }

            if (parameters == null || !parameters.TryGetValue(signatureKey, out string receivedSignature))
            {
                TBLogger.Warning($"[DeepLinkValidator] Missing '{signatureKey}' parameter in query.");
                return false;
            }

            try
            {
                // Step 1: Sort keys and exclude signature key
                var sortedKeys = parameters.Keys
                                           .Where(k => !string.Equals(k, signatureKey, StringComparison.OrdinalIgnoreCase))
                                           .OrderBy(k => k)
                                           .ToList();

                var queryBuilder = new StringBuilder();
                foreach (var key in sortedKeys)
                {
                    if (queryBuilder.Length > 0)
                        queryBuilder.Append("&");

                    string encodedKey = Uri.EscapeDataString(key);
                    string encodedValue = Uri.EscapeDataString(parameters[key]);
                    queryBuilder.Append($"{encodedKey}={encodedValue}");
                }

                string queryString = queryBuilder.ToString();

                // Step 2: Generate HMAC-SHA256 signature
                string calculatedSignature;
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiKey)))
                {
                    byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));
                    calculatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }

                // Step 3: Compare signatures
                bool isValid = string.Equals(receivedSignature, calculatedSignature, StringComparison.OrdinalIgnoreCase);

                TBLogger.Info($"[DeepLinkValidator] {signatureKey} received:   {receivedSignature}");
                TBLogger.Info($"[DeepLinkValidator] {signatureKey} calculated: {calculatedSignature}");

                return isValid;
            }
            catch (Exception ex)
            {
                TBLogger.Error($"[DeepLinkValidator] Exception during validation: {ex.Message}");
                return false;
            }
        }
    }
}
