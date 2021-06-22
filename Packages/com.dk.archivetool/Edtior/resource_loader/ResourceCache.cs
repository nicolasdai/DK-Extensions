using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class ResourceCache
{

    #region  Sub Classes
    public class BundleCache
    {
        public int _ref;
        public List<BundleCache> depends = new List<BundleCache>();

        public AssetBundle assetBundle;

        public void AddRef()
        {
            _ref++;
        }

        public void SubRef()
        {
            _ref--;
        }
    }
    #endregion

    #region Singleton
    public static ResourceCache Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ResourceCache();
            return _instance;
        }
    }
    private static ResourceCache _instance;
    #endregion

    private Dictionary<string, BundleCache> cache = new Dictionary<string, BundleCache>();

    public AssetBundle GetBundleCache(string assetBundleName)
    {
        // if cache exists, return cached bundle
        if (cache.ContainsKey(assetBundleName))
        {
            return cache[assetBundleName].assetBundle;
        }

        // else return null
        return null;
    }

    public BundleCache AddBundleRef(AssetBundle ab)
    {
        if (cache.ContainsKey(ab.name))
        {
            cache[ab.name].AddRef();
        }
        else
        {
            var bc = new BundleCache();
            bc._ref = 1;
            bc.assetBundle = ab;

            cache.Add(ab.name, bc);
        }

        return cache[ab.name];
    }

    public void SubBundleRef(AssetBundle ab)
    {
        if (cache.ContainsKey(ab.name))
        {
            cache[ab.name].SubRef();

            if (cache[ab.name]._ref <= 1)
            {
                // sub bundle ref for dependencies iterally
                foreach (var bc in cache[ab.name].depends)
                {
                    SubBundleRef(bc.assetBundle);
                }

                // remove ab from cache
                cache.Remove(ab.name);

                // unload cached ab
                ab.Unload(true);
            }
        }
        else
        {
            Debug.LogWarningFormat("Trying to sub ref for an unexisted ab, some resource node may lost record. AB name: {0}", ab.name);
        }
    }

    public int GetRefCount(string assetBundleName)
    {
        if (cache.ContainsKey(assetBundleName))
        {
            return cache[assetBundleName]._ref;
        }

        return 0;
    }
}
