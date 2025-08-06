
#if UNITY_ANDROID
using System.IO;
using System.Xml;
using TextBuddy;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;


public class AndroidPostProcessor : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 100;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string manifestPath = Path.Combine(path, "src/main/AndroidManifest.xml");

        if (!File.Exists(manifestPath))
        {
            Debug.LogError("[TextBuddy SDK] AndroidManifest.xml not found!");
            return;
        }


        string configPath = "Assets/TextBuddy/Runtime/ScriptableObjects/TextBuddyConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<TextBuddyConfig>(configPath);
        if (config == null)
        {
            UnityEngine.Debug.Log("[TextBuddy] Failed to load TextBuddyConfig from path: " + configPath);
            return;
        }

        string gameID = config.TextBuddyGameID;
        if (string.IsNullOrWhiteSpace(gameID))
        {
            UnityEngine.Debug.Log("[TextBuddy] gameID is not set in TextBuddyConfig.");
            return;
        }
        string CustomScheme = "textbuddy-"+ gameID;


        var xmlDoc = new XmlDocument();
        xmlDoc.Load(manifestPath);

        XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsMgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

        // Find all <activity> nodes
        XmlNodeList activities = xmlDoc.GetElementsByTagName("activity");
        XmlElement launcherActivity = null;

        foreach (XmlElement activity in activities)
        {
            XmlNodeList intentFilters = activity.GetElementsByTagName("intent-filter");

            foreach (XmlElement filter in intentFilters)
            {
                bool isLauncher = false;
                bool isMain = false;

                foreach (XmlElement child in filter.ChildNodes)
                {
                    if (child.Name == "action" &&
                        child.GetAttribute("android:name") == "android.intent.action.MAIN")
                    {
                        isMain = true;
                    }

                    if (child.Name == "category" &&
                        child.GetAttribute("android:name") == "android.intent.category.LAUNCHER")
                    {
                        isLauncher = true;
                    }
                }

                if (isMain && isLauncher)
                {
                    launcherActivity = activity;
                    break;
                }
            }

            if (launcherActivity != null)
                break;
        }

        if (launcherActivity == null)
        {
            Debug.LogWarning("[TextBuddy SDK] Launcher activity not found. Cannot add intent-filter.");
            return;
        }

        // Check if intent-filter for our scheme already exists
        bool alreadyExists = false;
        XmlNodeList existingFilters = launcherActivity.GetElementsByTagName("intent-filter");
        foreach (XmlElement filter in existingFilters)
        {
            XmlNodeList dataTags = filter.GetElementsByTagName("data");
            foreach (XmlElement dataTag in dataTags)
            {
                string scheme = dataTag.GetAttribute("android:scheme");
                if (scheme == CustomScheme)
                {
                    alreadyExists = true;
                    break;
                }
            }
        }

        if (alreadyExists)
        {
            Debug.Log("[TextBuddy SDK] Deep link scheme already present.");
            return;
        }

        // Create intent-filter
        XmlElement intentFilter = xmlDoc.CreateElement("intent-filter");

        XmlElement action = xmlDoc.CreateElement("action");
        action.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.intent.action.VIEW");
        intentFilter.AppendChild(action);

        XmlElement category1 = xmlDoc.CreateElement("category");
        category1.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.intent.category.DEFAULT");
        intentFilter.AppendChild(category1);

        XmlElement category2 = xmlDoc.CreateElement("category");
        category2.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.intent.category.BROWSABLE");
        intentFilter.AppendChild(category2);

        XmlElement data = xmlDoc.CreateElement("data");
        data.SetAttribute("scheme", "http://schemas.android.com/apk/res/android", CustomScheme);
        intentFilter.AppendChild(data);

        launcherActivity.AppendChild(intentFilter);

        xmlDoc.Save(manifestPath);
        Debug.Log("[TextBuddy SDK] Deep link intent-filter injected for scheme: " + CustomScheme);
    }

}
#endif
