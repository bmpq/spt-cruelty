using EFT.UI;
using System;
using System.IO;
using UnityEngine;

namespace tarkin.cruelty.bep
{
    internal class UILoader : IDisposable
    {
        readonly string _pathBundleDir;

        private AssetBundle _bundle;

        internal UILoader(string pathBundleDir)
        {
            _pathBundleDir = pathBundleDir;
        }

        public void Load(CommonUI commonUI)
        {
            string bundlePath = Path.Combine(_pathBundleDir, "cruelty-ui");
            if (!File.Exists(bundlePath))
            {
                Plugin.Logger.LogWarning($"bundle not found at: {bundlePath}");
                return;
            }

            _bundle = AssetBundle.LoadFromFile(bundlePath);

            if ( _bundle != null )
                Plugin.Logger.LogInfo($"Successfully loaded bundle {_bundle.name}");
        }

        public void Unload()
        {
            if (_bundle != null)
                _bundle.Unload(false);
        }

        public void Dispose()
        {
            Unload();
        }
    }
}
