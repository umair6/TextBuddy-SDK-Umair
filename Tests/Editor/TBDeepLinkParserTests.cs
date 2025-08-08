using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TextBuddy.core;

namespace TextBuddy.Tests
{
    public class TBDeepLinkParserTests
    {
        [Test]
        public void Constructor_ValidUrl_ShouldParseSuccessfully()
        {
            string url = "textbuddy-123://textbuddy/confirm?status=success&id=abc123";
            var parser = new TBDeepLinkParser(url);

            Assert.AreEqual("textbuddy-123", parser.Scheme);
            Assert.AreEqual("textbuddy", parser.HostName);
            Assert.AreEqual("/confirm", parser.Path);
        }

        [Test]
        public void Constructor_InvalidUrl_ShouldSetIsParsedFalse()
        {
            string invalidUrl = "this-is-not-a-valid-url";
            var parser = new TBDeepLinkParser(invalidUrl);

            Assert.IsNull(parser.HostName);
            Assert.IsNull(parser.Path);
            Assert.IsNull(parser.ParseQuery());
        }

        [Test]
        public void ParseQuery_ValidQuery_ShouldReturnDictionary()
        {
            string url = "textbuddy://confirm?status=success&id=abc123&name=John%20Doe";
            var parser = new TBDeepLinkParser(url);

            Dictionary<string, string> queryParams = parser.ParseQuery();

            Assert.IsNotNull(queryParams);
            Assert.AreEqual(3, queryParams.Count);
            Assert.AreEqual("success", queryParams["status"]);
            Assert.AreEqual("abc123", queryParams["id"]);
            Assert.AreEqual("John Doe", queryParams["name"]);
        }

        [Test]
        public void ParseQuery_EmptyQuery_ShouldReturnNull()
        {
            string url = "textbuddy://confirm";
            var parser = new TBDeepLinkParser(url);

            Assert.IsNull(parser.ParseQuery());
        }

        [Test]
        public void ParseQuery_MalformedQuery_ShouldReturnPartialOrNull()
        {
            string url = "textbuddy://confirm?status=success&badparam&x=y";
            var parser = new TBDeepLinkParser(url);

            Dictionary<string, string> queryParams = parser.ParseQuery();

            Assert.IsNotNull(queryParams);
            Assert.IsTrue(queryParams.ContainsKey("status"));
            Assert.IsTrue(queryParams.ContainsKey("x"));
            Assert.IsFalse(queryParams.ContainsKey("badparam")); // should be skipped or ignored
        }
    }
}

