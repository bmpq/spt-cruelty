using BepInEx;
using BepInEx.Logging;
using EFT.UI;
using SPT.Reflection.Patching;

namespace tarkin.cruelty.bep
{
    [BepInPlugin("com.tarkin.cruelty", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    internal partial class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; }

        private PatchManager _patchManager;

        private UILoader _loader;

        void Start()
        {
            Logger = base.Logger;

            _patchManager = new PatchManager(this, autoPatch: true);
            _patchManager.EnablePatches();

            _loader = new UILoader(System.IO.Path.Combine(BepInEx.Paths.PluginPath, "tarkin-cruelty"));

            Patch_CommonUI_Awake.OnAwake += _loader.Load;
            if (MonoBehaviourSingleton<CommonUI>.Instantiated)
            {
                _loader.Load(MonoBehaviourSingleton<CommonUI>.Instance);
            }
        }

        void Update()
        {
            _loader.Update();
        }

        void OnDestroy()
        {
            Patch_CommonUI_Awake.OnAwake -= _loader.Load;

            _loader.Dispose();

            _patchManager.DisablePatches();
            _patchManager = null;
        }
    }
}
