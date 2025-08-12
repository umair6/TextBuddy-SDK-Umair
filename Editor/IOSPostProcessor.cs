#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using TextBuddy.Core;

namespace TextBuddy.Editor
{
    public class IOSPostProcessor
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS) return;
            var config = TextBuddy.Editor.TextBuddyEditorHelper.LoadConfigFromResources();
            if (config == null)
            {
                UnityEngine.Debug.Log("[TextBuddy] Failed to load TextBuddyConfig from Resource Folder" );
                return;
            }

            string gameID = config.TextBuddyGameID;
            if (string.IsNullOrWhiteSpace(gameID))
            {
                UnityEngine.Debug.Log("[TextBuddy] gameID is not set in TextBuddyConfig.");
                return;
            }
            string URLScheme = "textbuddy-"+ gameID;
            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;
            PlistElementArray urlTypes;
            if (rootDict.values.ContainsKey("CFBundleURLTypes"))
            {
                urlTypes = rootDict["CFBundleURLTypes"].AsArray();
            }
            else
            {
                urlTypes = rootDict.CreateArray("CFBundleURLTypes");
            }
            var urlDict = urlTypes.AddDict();
            urlDict.SetString("CFBundleURLName", "com.textbuddy.sdk");
            var schemes = urlDict.CreateArray("CFBundleURLSchemes");
            schemes.AddString(URLScheme);

            plist.WriteToFile(plistPath);
            UnityEngine.Debug.Log("Info.plist updated with permissions and URL scheme.");
        }
    }
}
#endif
