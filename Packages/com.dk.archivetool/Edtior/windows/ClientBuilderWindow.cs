using System.IO;
using UnityEditor;
using UnityEngine;

namespace DK.Archive
{
    public partial class ClientBuilderWindow : EditorWindow
    {
        public string versionString;
        public bool buildAssetBundles;
        
        private const string VersionInfoPath = "Assets/DK-Extensions-Configs/archive_tool/version_info.asset";
        private const string ArchiveConfigPath = "Assets/DK-Extensions-Configs/archive_tool/archive_config.asset";
        
        // private float smallBtnHeight = 30;
        private const float MediumBtnHeight = 50;
        // private float bigBtnHeight = 60;
        
        private VersionInfo VersionInfo
        {
            get
            {
                if (_versionInfo == null) LoadVersionFile();
                return _versionInfo;
            }
        }
        private VersionInfo _versionInfo;

        private static ArchiveConfig ArchiveConfig
        {
            get
            {
                if (_archiveConfig == null || _archiveConfigEditor == null) LoadArchiveConfigFile();
                return _archiveConfig;
            }
        }
        private static ArchiveConfig _archiveConfig;
        private static Editor _archiveConfigEditor;
        
        [MenuItem("DK/Package Tool")]
        private static void InitWindow()
        {
            var window = (ClientBuilderWindow)EditorWindow.GetWindow(typeof(ClientBuilderWindow));
            window.Init();
            window.Show();
        }

        private void OnGUI()
        {
            var style = new GUIStyle {normal = {textColor = Color.white}, fontSize = 28};

            // version info
            GUILayout.Label($"Ver:{versionString}", style, GUILayout.Height(MediumBtnHeight));
            
            // configs
            if (_archiveConfigEditor == null) LoadArchiveConfigFile();
            _archiveConfigEditor.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            buildAssetBundles = GUILayout.Toggle(buildAssetBundles, "Build Asset Bundles", GUILayout.Height(MediumBtnHeight));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OpenVersionFolder", GUILayout.Height(MediumBtnHeight))) OpenVersionFolder();
            if (GUILayout.Button("OpenPersistentFolder", GUILayout.Height(MediumBtnHeight))) OpenPersistentFolder();
            if (GUILayout.Button("ClearPersistentFolder", GUILayout.Height(MediumBtnHeight))) ClearPersistentFolder();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("BuildBundle", GUILayout.Height(MediumBtnHeight))) BuildBundle();
            if (GUILayout.Button("CopyHotfix", GUILayout.Height(MediumBtnHeight))) CopyHotfix();
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Build Client", GUILayout.Height(MediumBtnHeight))) BuildClient();
        }

        private void Init()
        {
            versionString = VersionInfo.Version;
            LoadArchiveConfigFile();
        }

        private void LoadVersionFile()
        {
            if (!File.Exists(VersionInfoPath))
            {
                var directory = Path.GetDirectoryName(VersionInfoPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    AssetDatabase.Refresh();
                }

                _versionInfo = ScriptableObject.CreateInstance(typeof(VersionInfo)) as VersionInfo;
                AssetDatabase.CreateAsset(_versionInfo, VersionInfoPath);
                AssetDatabase.SaveAssets();
            }
            else
            {
                _versionInfo = AssetDatabase.LoadAssetAtPath(VersionInfoPath, typeof(VersionInfo)) as VersionInfo;
            }
        }

        private static void LoadArchiveConfigFile()
        {
            if (!File.Exists(ArchiveConfigPath))
            {
                var directory = Path.GetDirectoryName(ArchiveConfigPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    AssetDatabase.Refresh();
                }

                _archiveConfig = ScriptableObject.CreateInstance(typeof(ArchiveConfig)) as ArchiveConfig;
                AssetDatabase.CreateAsset(_archiveConfig, ArchiveConfigPath);
                AssetDatabase.SaveAssets();
            }
            else
            {
                _archiveConfig = AssetDatabase.LoadAssetAtPath(ArchiveConfigPath, typeof(ArchiveConfig)) as ArchiveConfig;
            }
            
            _archiveConfigEditor = Editor.CreateEditor(_archiveConfig);
        }

        // [HorizontalGroup("Build")]
        // [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
        public void BuildClient()
        {
            if (!EditorUtility.DisplayDialog(
                $"Build Client For: {EditorUserBuildSettings.activeBuildTarget}",
                "Confirm to begin, may take a while",
                "Confirm",
                "Cancel")) return;
            
            if (buildAssetBundles)
            {
                BuildAssetBundles();
            }

            BuildTargetPackage();
            UpdateVersionInfo();
        }

        // [HorizontalGroup("Buttons")]
        // [Button(ButtonSizes.Large)]
        public static void OpenVersionFolder()
        {
            EditorUtility.RevealInFinder("./version/");
        }

        // [HorizontalGroup("Buttons")]
        // [Button(ButtonSizes.Large)]
        public static void OpenPersistentFolder()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        // [HorizontalGroup("Buttons")]
        // [Button(ButtonSizes.Large)]
        public static void ClearPersistentFolder()
        {
            if (!Directory.Exists(Application.persistentDataPath)) return;
            
            var di = new DirectoryInfo(Application.persistentDataPath);
            foreach (var file in di.GetFiles())
            {
                file.Delete();
            }

            foreach (var folder in di.GetDirectories())
            {
                // if (folder.Name.Contains("Android") || folder.Name.Contains("iOS"))
                //     continue;

                folder.Delete(true);
            }
        }

        // [HorizontalGroup("Debug")]
        // [Button(ButtonSizes.Medium)]
        public static void BuildBundle()
        {
            if (EditorUtility.DisplayDialog(
                $"Build Asset Bundles For: {EditorUserBuildSettings.activeBuildTarget}",
                "Confirm to begin, may take a while",
                "Confirm",
                "Cancel"))
                BuildAssetBundles();
        }

        // [HorizontalGroup("Debug")]
        // [Button(ButtonSizes.Medium)]
        public static void CopyHotfix()
        {
            if (_archiveConfig == null || _archiveConfigEditor == null) LoadArchiveConfigFile();
            
            // copy hotfix to streaming path
            var assemblyPath = Application.dataPath + "/../Library/ScriptAssemblies/";

            var patchPath = _archiveConfig.hotfixDesPath;
            if (string.IsNullOrEmpty(patchPath))
            {
                patchPath = "Assets/_BinaryAssets/Hotfix/";
            }
            
            if (!Directory.Exists(patchPath))
                Directory.CreateDirectory(patchPath);

            File.Copy(
                Path.Combine(assemblyPath, _archiveConfig.hotfixDllName),
                Path.Combine(patchPath, "HotFix.bytes"),
                true);

            Debug.Log($"Copied file from [{assemblyPath}] to [{patchPath}]");

            AssetDatabase.Refresh();
        }

        private void BuildTargetPackage()
        {
            // copy hotfix
            try
            {
                CopyHotfix();
            }
            catch
            {
                Debug.Log("Not Hotfix Found!");
            }

            // set bundle version
            PlayerSettings.bundleVersion = versionString;

            // build scenes
            var startScenes = EditorBuildSettings.scenes;
#if UNITY_ANDROID
            // set keystore info
            PlayerSettings.keystorePass = ArchiveConfig.keystorePassword;
            PlayerSettings.keyaliasPass = ArchiveConfig.aliasPassword;
            
            BuildPipeline.BuildPlayer(startScenes, "./version/" + PlayerSettings.productName.Replace(" ", "_") + "_v" + versionString + ".apk", EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);
#elif UNITY_IOS
            BuildPipeline.BuildPlayer(startScenes, "./version/" + "unity_xcode", EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);
#endif
        }

        private void UpdateVersionInfo()
        {
            VersionInfo.AddBuild();
            EditorUtility.SetDirty(VersionInfo);
            AssetDatabase.SaveAssets();
            versionString = VersionInfo.Version;
        }
        
        private static string GetPlatformDir()
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "Win";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                default:
                    return "Android";
            }
        }
    }
}
