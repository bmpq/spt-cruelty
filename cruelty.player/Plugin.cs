using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SPT.Reflection.Patching;
using UnityEngine;

namespace tarkin.cruelty.player
{
    [BepInPlugin("com.tarkin.cruelty.player", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal partial class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; }

        private PatchManager _patchManager;

        void Start()
        {
            Logger = base.Logger;

            _patchManager = new PatchManager(this, autoPatch: true);
            _patchManager.EnablePatches();

            if (Singleton<GameWorld>.Instantiated)
            {
                Singleton<GameWorld>.Instance.MainPlayer.GetOrAddComponent<PlayerGrapplingController>();
            }
        }

        void OnDestroy()
        {
            FindObjectsByTypeAndDestroy<PlayerGrapplingController>();

            _patchManager.DisablePatches();
            _patchManager = null;
        }

        static void FindObjectsByTypeAndDestroy<T>() where T : Object
        {
            foreach (var item in FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Component.Destroy(item);
            }
        }
    }
}
