using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using UnityEngine;

namespace DK.Archive
{
    public class ResourceHelper
    {
        #region Sub Classes
        [Serializable]
        public class ResourceInfoList
        {
            public List<ResourceInfo> infoList = new List<ResourceInfo>();
            public List<DependenceInfo> depenInfoList = new List<DependenceInfo>();
        }

        [Serializable]
        public class ResourceInfo
        {
            public string resourceName;
            public string resourcePath;
            public string assetBundleName;
        }

        [Serializable]
        public class DependenceInfo
        {
            public string assetbundleName;
            public string[] depenAssetsList;
        }
        #endregion

        #region Singleton
        public static ResourceHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ResourceHelper();
                return _instance;
            }
        }
        private static ResourceHelper _instance;
        #endregion

        private Dictionary<string, ResourceInfo> resourceMap = new Dictionary<string, ResourceInfo>();
        private Dictionary<string, DependenceInfo> depenMap = new Dictionary<string, DependenceInfo>();
        AssetBundleManifest assetBundleManifest = null;

        public void Setup()
        {
            // load resource map
            resourceMap.Clear();
            depenMap.Clear();

            string path = PathUtil.GetAssetBundlePath(PathUtil.resourceMapFileName);
            string json = "";
            if (File.Exists(path))
            {
                json = File.ReadAllText(path);
            }
            else
            {
                TextAsset ta = Resources.Load(Path.GetFileNameWithoutExtension(PathUtil.resourceMapFileName)) as TextAsset;
                json = ta.text;
            }

            ResourceInfoList allInfo = JsonUtility.FromJson<ResourceInfoList>(json);
            allInfo.infoList.ForEach((x) => { resourceMap.Add(x.resourcePath, x); });
            allInfo.depenInfoList.ForEach((x) => { depenMap.Add(x.assetbundleName, x); });

            string manifestPath = PathUtil.GetAssetBundlePath("AssetBundle");
            if (!File.Exists(manifestPath))
                manifestPath = PathUtil.StreamingAssetsPath("AssetBundles/AssetBundle");

            AssetBundle ab = AssetBundle.LoadFromFile(manifestPath);
            assetBundleManifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            ab.Unload(false);
        }

        #region Sync Loading

        public GameObject LoadGameObject(string path)
        {
            if (resourceMap.ContainsKey(path))
            {
                var ab = LoadAssetBundle(resourceMap[path].assetBundleName);

                var ret = ab.LoadAsset(resourceMap[path].resourceName, typeof(GameObject)) as GameObject;
                ret = GameObject.Instantiate(ret);

                ResourceNode rn = ret.GetComponent<ResourceNode>();
                if (rn == null)
                {
                    rn = ret.AddComponent<ResourceNode>();
                }
                rn.RecordReferencedBundle(ab);

                return ret;
            }
            else
            {
                UnityEngine.Debug.LogErrorFormat("Cannot Find Resource: {0}", path);
            }

            return null;
        }

        public T LoadObject<T>(string path) where T : UnityEngine.Object
        {
            if (resourceMap.ContainsKey(path))
            {
                var ab = LoadAssetBundle(resourceMap[path].assetBundleName);

                var ret = ab.LoadAsset<T>(resourceMap[path].resourceName) as T;
                return ret;
            }
            else
            {
                UnityEngine.Debug.LogErrorFormat("Cannot Find Resource: {0}", path);
            }

            return default;
        }

        private AssetBundle LoadAssetBundle(string assetBundleName)
        {
            AssetBundle ret = null;
            AssetBundle cache = ResourceCache.Instance.GetBundleCache(assetBundleName);

            if (cache != null)
            {
                ret = cache;
            }
            else
            {
                string path = PathUtil.GetAssetBundlePath(assetBundleName);
                if (!File.Exists(path))
                    path = PathUtil.StreamingAssetsPath("AssetBundles/" + assetBundleName);

                AssetBundle ab = AssetBundle.LoadFromFile(path);

                // add first ref when first loaded, which means bundle itself
                var bc = ResourceCache.Instance.AddBundleRef(ab);
                var depens = LoadDependencies(assetBundleName);
                foreach (var depen in depens)
                {
                    bc.depends.Add(ResourceCache.Instance.AddBundleRef(depen));
                }

                ret = ab;
            }

            return ret;
        }

        private List<AssetBundle> LoadDependencies(string assetBundleName)
        {
            List<AssetBundle> depenAbs = new List<AssetBundle>();
            // load dependencies iterally
            var depens = GetDependenciesNames(assetBundleName);
            for (int i = 0; i < depens.Length; ++i)
            {
                depenAbs.Add(LoadAssetBundle(depens[i]));
            }

            return depenAbs;
        }
        #endregion

        string[] GetDependenciesNames(string assetBundleName)
        {
            DependenceInfo d = null;
            if (!depenMap.TryGetValue(assetBundleName, out d))
            {
                d = new DependenceInfo();
                d.assetbundleName = assetBundleName;
                d.depenAssetsList = assetBundleManifest.GetAllDependencies(assetBundleName);
                depenMap.Add(assetBundleName, d);
            }
            return d.depenAssetsList;
        }

    }
}
