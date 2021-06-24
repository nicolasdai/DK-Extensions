using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using System.IO;
#endif

namespace DK.Archive
{
    // custom bundle builder, build each asset into one single asset bundle
    // dependencies will be counted, any asset referenced by other asset more than once, will be build into one single asset bundle
    public partial class ClientBuilderWindow
    {
        private static void BuildAssetBundles()
        {
            var profileSettings = AddressableAssetSettingsDefaultObject.Settings.profileSettings;
            var profileId = profileSettings.GetProfileId("Default");
            AddressableAssetSettingsDefaultObject.Settings.activeProfileId = profileId;
            AddressableAssetSettings.BuildPlayerContent();
        }
    }

    public class PreprocessBuildWithReport : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("Prebuild");
#if UNITY_IOS
            if (report.summary.platform == BuildTarget.iOS) // Check if the build is for iOS
            {
                // Increment build number if proper int, ignore otherwise
                int currentBuildNumber;
                if (int.TryParse(PlayerSettings.iOS.buildNumber, out currentBuildNumber))
                {
                    string newBuildNumber = (currentBuildNumber + 1).ToString();
                    Debug.Log("Setting new iOS build number to " + newBuildNumber);
                    PlayerSettings.iOS.buildNumber = newBuildNumber;
                }
                else
                {
                    Debug.LogError("Failed to parse build number " + PlayerSettings.iOS.buildNumber + " as int.");
                }
            }
#endif

#if UNITY_ANDROID
            if (report.summary.platform == BuildTarget.Android)
            {

            }
#endif
        }
    }

    public class ExcemptFromEncryption : IPostprocessBuildWithReport // Will execute after XCode project is built
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            Debug.Log("Post build");
#if UNITY_IOS
            if (report.summary.platform == BuildTarget.iOS) // Check if the build is for iOS 
            {
                string plistPath = report.summary.outputPath + "/Info.plist";

                PlistDocument plist = new PlistDocument(); // Read Info.plist file into memory
                plist.ReadFromString(File.ReadAllText(plistPath));

                PlistElementDict rootDict = plist.root;
                rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

                File.WriteAllText(plistPath, plist.WriteToString()); // Override Info.plist
            }
#endif
        }
    }
}
