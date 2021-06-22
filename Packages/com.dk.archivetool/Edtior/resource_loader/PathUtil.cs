using System.IO;

using UnityEngine;

namespace DK.Archive
{
    public class PathUtil
    {
#if UNITY_STANDALONE
        public static string osDir = "Win";
#elif UNITY_ANDROID
        public static string osDir = "Android";
#elif UNITY_IPHONE
        public static string osDir = "iOS";
#else
        public static string osDir = "";
#endif

        public static string resourceMapFileName = "ResourceMap.json";
        public static string assetBundleRoot = "AssetBundles/";
        public static string patchRoot = "Patch/";

        // Building Paths
        public static string VersionRoot()
        {
            return Path.Combine(Application.persistentDataPath, osDir);
        }

        public static string AssetBundleRoot()
        {
            return Path.Combine(VersionRoot(), assetBundleRoot);
        }

        public static string PatchRoot()
        {
            return Path.Combine(VersionRoot(), patchRoot);
        }

        // Loading Paths
        public static string GetAssetBundlePath(string subPath)
        {
            string path = Path.Combine(AssetBundleRoot(), subPath).Replace("\\", "/");
            if (File.Exists(path))
                return path;
            else
            {
                path = StreamingAssetsPath(Path.Combine(assetBundleRoot, subPath)).Replace("\\", "/");
                return path;
            }
        }

        public static string GetPatchPath(string subPath)
        {
            string path = Path.Combine(PatchRoot(), subPath).Replace("\\", "/");
            if (File.Exists(path))
                return path;
            else
            {
                path = StreamingAssetsPath(Path.Combine(patchRoot, subPath)).Replace("\\", "/");
                return path;
            }
        }

        public static string PersistentAssetPath(string path)
        {
            return Path.Combine(Application.persistentDataPath, path);
        }

        public static string StreamingAssetsPath(string path = "")
        {
            return Path.Combine(Application.streamingAssetsPath, path);
        }

        // URL paths, deprecated
        public static string ConvertToStreamingURL(string path)
        {
            string result = "";
            if (Application.platform == RuntimePlatform.Android)
                result = "jar:file://" + Application.dataPath + "!/assets/" + path;
            else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                result = Application.streamingAssetsPath + "/" + path;
            else
                result = "file:///" + Application.streamingAssetsPath + "/" + path;
            return result;
        }
    }
}
