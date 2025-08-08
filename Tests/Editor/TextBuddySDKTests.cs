using NUnit.Framework;
using UnityEngine;
using TextBuddy.core;
using System.Collections.Generic;
namespace TextBuddy.Tests
{
    public class TextBuddySDKTests
    {
        private GameObject sdkObject;
        private TextBuddySDK sdk;

        [SetUp]
        public void SetUp()
        {
            sdkObject = new GameObject("TextBuddySDK");
            sdk = sdkObject.AddComponent<TextBuddySDK>();

            // Inject a test config using reflection or internal access
            var config = ScriptableObject.CreateInstance<TextBuddyConfig>();
            config.TextBuddyAPIKey = "dummy_key";
            config.TextBuddyGameID = "test_game_id";
            typeof(TextBuddySDK)
                .GetField("config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(sdk, config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(sdkObject);
        }

        [Test]
        public void IsInitialized_ReturnsFalseByDefault()
        {
            Assert.IsFalse(sdk.IsInitialized());
        }

        [Test]
        public void IsUserSubscribed_ReturnsFalseInitially()
        {
            Assert.IsFalse(sdk.IsUserSubscribed());
        }

        [Test]
        public void InitialiseTextBuddy_SetsInitializingStatus()
        {
            sdk.InitialiseTextBuddy();
            // Reflection check because sdkInitializationStatus is private
            var status = typeof(TextBuddySDK)
                .GetField("sdkInitializationStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(sdk);

            Assert.AreEqual(TextBuddySDK.InitializationStatus.Initializing, status);
        }

        [Test]
        public void Subscribe_ChangesStatusToPending()
        {
            sdk.Subscribe();

            var status = typeof(TextBuddySDK)
                .GetField("subscriptionStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(sdk);

            Assert.AreEqual(TextBuddySDK.SubscriptionStatus.Pending, status);
        }

        [Test]
        public void UnSubscribe_ChangesStatusToUnsubscribed_AndClearsUserId()
        {
            // Pretend user was subscribed
            typeof(TextBuddySDK)
                .GetField("subscriptionStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(sdk, TextBuddySDK.SubscriptionStatus.Subscribed);

            typeof(TextBuddySDK)
                .GetField("textBuddyUserID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(sdk, "user123");

            sdk.UnSubscribe();

            var status = typeof(TextBuddySDK)
                .GetField("subscriptionStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(sdk);

            Assert.AreEqual(TextBuddySDK.SubscriptionStatus.UnSubscribed, status);

            Assert.AreEqual("", sdk.TextBuddyUserID());
        }

        [Test]
        public void IsTextBuddyURL_ValidHost_ReturnsTrue()
        {
            var result = typeof(TextBuddySDK)
                .GetMethod("IsTextBuddyURL", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(sdk, new object[] { "textbuddy" });

            Assert.IsTrue((bool)result);
        }

        [Test]
        public void IsTextBuddyURL_InvalidHost_ReturnsFalse()
        {
            var result = typeof(TextBuddySDK)
                .GetMethod("IsTextBuddyURL", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(sdk, new object[] { "example" });

            Assert.IsFalse((bool)result);
        }
    }
}
