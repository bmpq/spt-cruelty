using BepInEx;
using BepInEx.Logging;
using EFT.UI;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace tarkin.cruelty.bep
{
    [BepInPlugin("com.tarkin.cruelty", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal partial class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; }

        private PatchManager _patchManager;

        private AssetBundle _bundle;
        List<IDisposable> _disposables = new List<IDisposable>();

        void Start()
        {
            Logger = base.Logger;

            _patchManager = new PatchManager(this, autoPatch: true);
            _patchManager.EnablePatches();

            Patch_CommonUI_Awake.OnAwake += Load;
            if (MonoBehaviourSingleton<CommonUI>.Instantiated)
            {
                Load(MonoBehaviourSingleton<CommonUI>.Instance);
            }
        }

        void Load(CommonUI commonUI)
        {
            string bundlePath = Path.Combine(BepInEx.Paths.PluginPath, "tarkin-cruelty", "cruelty-ui");
            _bundle = AssetBundle.LoadFromFile(bundlePath);

            _disposables.Add(new CrueltyAdapterHealth(commonUI, _bundle));
        }

        void Update()
        {
            foreach (var item in _disposables)
            {
                if (item is IUnityUpdateReceiver receiver)
                {
                    receiver.Update();
                }
            }
        }

        void OnDestroy()
        {
            Patch_CommonUI_Awake.OnAwake -= Load;

            foreach (var item in _disposables)
            {
                item.Dispose();
            }

            if (_bundle != null)
                _bundle.Unload(false);

            _patchManager.DisablePatches();
            _patchManager = null;
        }
    }
}
