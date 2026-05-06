using EFT.UI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace tarkin.cruelty.bep
{
    internal class UILoader : IDisposable
    {
        readonly string _pathBundleDir;

        private AssetBundle _bundle;

        List<IDisposable> _disposables = new List<IDisposable>();

        internal UILoader(string pathBundleDir)
        {
            _pathBundleDir = pathBundleDir;
        }

        public void Load(CommonUI commonUI)
        {
            string bundlePath = Path.Combine(_pathBundleDir, "cruelty-ui");
            _bundle = AssetBundle.LoadFromFile(bundlePath);

            _disposables.Add(new CrueltyAdapterHealth(commonUI, _bundle));
        }

        public void Update()
        {
            foreach (var item in _disposables)
            {
                if (item is IUnityUpdateReceiver receiver)
                {
                    receiver.Update();
                }
            }
        }

        public void UnloadBundle()
        {
            if (_bundle != null)
                _bundle.Unload(false);
        }

        public void Dispose()
        {
            foreach (var item in _disposables)
            {
                item.Dispose();
            }

            UnloadBundle();
        }
    }
}
