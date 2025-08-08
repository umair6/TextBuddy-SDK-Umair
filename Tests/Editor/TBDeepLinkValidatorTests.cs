using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using TextBuddy.core;
using System;

namespace TextBuddy.Tests
{
    public class TBDeepLinkValidatorTests
    {
        private const string ApiKey = "test_secret_key";

        private static string GenerateSignature(Dictionary<string, string> parameters, string apiKey, string signatureKey = "sig")
        {
            var sortedKeys = new List<string>(parameters.Keys);
            sortedKeys.Remove(signatureKey);
            sortedKeys.Sort();

            var builder = new StringBuilder();
            foreach (var key in sortedKeys)
            {
                if (builder.Length > 0) builder.Append("&");
                builder.Append($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(parameters[key])}");
            }

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        [Test]
        public void ValidateQueryParams_ValidSignature_ReturnsTrue()
        {
            var parameters = new Dictionary<string, string>
            {
                { "userID", "123" },
                { "gameID", "abc" }
            };
            string signature = GenerateSignature(parameters, ApiKey);
            parameters["sig"] = signature;

            bool result = TBDeepLinkValidator.ValidateQueryParams(parameters, ApiKey);
            Assert.IsTrue(result);
        }

        [Test]
        public void ValidateQueryParams_InvalidSignature_ReturnsFalse()
        {
            var parameters = new Dictionary<string, string>
            {
                { "userID", "123" },
                { "gameID", "abc" },
                { "sig", "invalidsignature" }
            };

            bool result = TBDeepLinkValidator.ValidateQueryParams(parameters, ApiKey);
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateQueryParams_MissingSignatureParam_ReturnsFalse()
        {
            var parameters = new Dictionary<string, string>
            {
                { "userID", "123" },
                { "gameID", "abc" }
            };

            bool result = TBDeepLinkValidator.ValidateQueryParams(parameters, ApiKey);
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateQueryParams_NullParameters_ReturnsFalse()
        {
            bool result = TBDeepLinkValidator.ValidateQueryParams(null, ApiKey);
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateQueryParams_EmptySignatureKey_ReturnsFalse()
        {
            var parameters = new Dictionary<string, string>
            {
                { "userID", "123" },
                { "gameID", "abc" },
                { "sig", "anything" }
            };

            bool result = TBDeepLinkValidator.ValidateQueryParams(parameters, ApiKey, "");
            Assert.IsFalse(result);
        }

        [Test]
        public void ValidateQueryParams_CustomSignatureKey_ValidSignature_ReturnsTrue()
        {
            var parameters = new Dictionary<string, string>
            {
                { "userID", "123" },
                { "gameID", "abc" }
            };
            string customKey = "signature";
            parameters[customKey] = GenerateSignature(parameters, ApiKey, customKey);

            bool result = TBDeepLinkValidator.ValidateQueryParams(parameters, ApiKey, customKey);
            Assert.IsTrue(result);
        }

        [Test]
        public void ValidateQueryParams_ExceptionInEncoding_ReturnsFalse()
        {
            var parameters = new Dictionary<string, string>
            {
                { "userID", "123" },
                { "gameID", "abc" },
                { "sig", null } // will trigger exception in EscapeDataString
            };

            bool result = TBDeepLinkValidator.ValidateQueryParams(parameters, ApiKey);
            Assert.IsFalse(result);
        }
    }
}

