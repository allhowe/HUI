#if UIMODULE_ADDRESSBLES
using System;
using System.Collections.Generic;
using UIModule;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressblesUILoader : IUILoader
{
    private readonly Dictionary<string, GameObject> map;

    public AddressblesUILoader()
    {
        Addressables.InitializeAsync();
        map = new Dictionary<string, GameObject>();
    }

    public void Load(string key, UICallback<GameObject> onLoadComplete)
    {
        if (map.TryGetValue(key, out var value))
        {
            onLoadComplete?.Invoke(value);
            return;
        }

        Addressables.LoadAssetAsync<GameObject>(key).Completed += handle =>
        {
            if (handle.IsDone && handle.Status == AsyncOperationStatus.Succeeded)
            {
                var asset = handle.Result;
                if (asset != null)
                {
                    map[key] = asset;
                }
                onLoadComplete?.Invoke(asset);
            }
            else
            {
                throw new Exception($"ui load fail.path = {key}");
            }
        };
    }

    public void Release(string key)
    {
        if (map.TryGetValue(key, out var value))
        {
            map.Remove(key);
            Addressables.Release(value);
        }
    }
}

#endif
