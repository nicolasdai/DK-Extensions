using System.Collections;
using System.Collections.Generic;

using UnityEngine;

// Reference count operation class
public class ResourceNode : MonoBehaviour
{

    private List<AssetBundle> refBundle = new List<AssetBundle>();

    public void RecordReferencedBundle(AssetBundle ab)
    {
        if (!refBundle.Contains(ab))
            refBundle.Add(ab);
    }

    private void Start()
    {
        // add ref count when game object is loaded
        refBundle.ForEach((x) =>
        {
            ResourceCache.Instance.AddBundleRef(x);
        });
    }

    private void OnDestroy()
    {
        // subtract ref count when game object is loaded
        refBundle.ForEach((x) =>
        {
            ResourceCache.Instance.SubBundleRef(x);
        });
    }
}
