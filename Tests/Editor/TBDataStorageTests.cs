using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using TextBuddy.core;

namespace TextBuddy.Tests
{
    public class TBDataStorageTests
    {
        private const string TestKey = "TEST_KEY";
        private const string TestValue = "HelloTextBuddy";
        private const string DefaultValue = "DefaultVal";

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(TestKey);
        }

        [Test]
        public void SetString_ShouldStoreValue()
        {
            TBDataStorage.SetString(TestKey, TestValue);
            Assert.IsTrue(PlayerPrefs.HasKey(TestKey));
        }

        [Test]
        public void GetString_ShouldReturnStoredValue()
        {
            PlayerPrefs.SetString(TestKey, TestValue);
            string result = TBDataStorage.GetString(TestKey, DefaultValue);
            Assert.AreEqual(TestValue, result);
        }

        [Test]
        public void GetString_ShouldReturnDefaultValue_WhenKeyDoesNotExist()
        {
            string result = TBDataStorage.GetString("NON_EXISTENT_KEY", DefaultValue);
            Assert.AreEqual(DefaultValue, result);
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(TestKey);
        }
    }
}
