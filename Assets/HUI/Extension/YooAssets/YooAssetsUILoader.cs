#if HUI_YOOASSET
using System;
using System.Collections.Generic;
using HUI;
using UnityEngine;
using YooAsset;

namespace HUI.Extension
{
    public class YooAssetsUILoader : IUILoader
    {
        private const string DEFAULT_PACKAGE_NAME = "DefaultPackage";
        public event Action Initialized;

        private ResourcePackage package;

        private Dictionary<string, AssetHandle> map;
        public YooAssetsUILoader(string packageName = DEFAULT_PACKAGE_NAME)
        {
            map = new Dictionary<string, AssetHandle>();

            try
            {
                package = YooAssets.GetPackage(packageName);
            }
            catch
            {
                Initialize();
            }
        }
        public void Load(string key, UICallback<GameObject> onLoadComplete)
        {
            var handle = package.LoadAssetAsync<GameObject>(key);
            handle.Completed += (h) => {
                var asset = (GameObject)handle.AssetObject;

                if (asset != null)
                {
                    map[key] = h;
                }
                onLoadComplete?.Invoke(asset);
            };
        }

        public void Release(string key)
        {
            if (map.Remove(key, out var handle))
            {
                handle.Release();
                package.TryUnloadUnusedAsset(key);
            }
        }


        public void Initialize()
        {
            YooAssets.Initialize();
            package = YooAssets.CreatePackage(DEFAULT_PACKAGE_NAME);
            YooAssets.SetDefaultPackage(package);

            InitPackage();
        }

        private void InitPackage()
        {
#if UNITY_EDITOR
            var buildResult = EditorSimulateModeHelper.SimulateBuild(DEFAULT_PACKAGE_NAME);
            var packageRoot = buildResult.PackageRootDirectory;
            var fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);

            var createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters = fileSystemParams;
#else
        var fileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
        var createParameters = new OfflinePlayModeParameters();
        createParameters.BuildinFileSystemParameters = fileSystemParams;
#endif

            var initOperation = package.InitializeAsync(createParameters);
            initOperation.Completed += handle => {
                if (handle.Status == EOperationStatus.Succeed)
                {
                    Debug.Log("YooAssets initialize succeed.");
                    RequestPackageVersion();
                }
                else
                {
                    Debug.LogError($"YooAssets initialize failed. {handle.Error}");
                }
            };
        }

        private void RequestPackageVersion()
        {
            var operation = package.RequestPackageVersionAsync();
            operation.Completed += handle => {
                if (handle.Status == EOperationStatus.Succeed)
                {
                    string packageVersion = operation.PackageVersion;
                    Debug.Log($"Request package Version : {packageVersion}");
                    UpdatePackageManifest(packageVersion);
                }
                else
                {
                    Debug.LogError(handle.Error);
                }
            };
        }
        private void UpdatePackageManifest(string packageVersion)
        {
            var package = YooAssets.GetPackage(DEFAULT_PACKAGE_NAME);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            operation.Completed += handle => {
                if (handle.Status == EOperationStatus.Succeed)
                {
                    Initialized?.Invoke();
                }
                else
                {
                    Debug.LogError(handle.Error);
                }
            };
        }
    }

}
#endif